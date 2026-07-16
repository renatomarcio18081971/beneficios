using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class LinhaOnibusServiceTests
{
    private readonly Mock<ILinhaOnibusRepository> _linhaRepo = new();
    private readonly Mock<IFuncionarioLinhaRepository> _vinculoRepo = new();
    private readonly LinhaOnibusService _sut;

    public LinhaOnibusServiceTests()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<LinhaOnibusProfile>()).CreateMapper();
        _sut = new LinhaOnibusService(_linhaRepo.Object, _vinculoRepo.Object, mapper);
    }

    [Fact]
    public async Task SalvarAsync_DataFimMenor_DeveFalhar()
    {
        var dto = new LinhaOnibusSalvarDto(
            "Linha 100",
            new DateOnly(2026, 1, 31),
            new DateOnly(2026, 1, 1),
            4.50m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(LinhaOnibusService.MensagemDataFim, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_TarifaNegativa_DeveFalhar()
    {
        var dto = new LinhaOnibusSalvarDto(
            "Linha 100",
            new DateOnly(2026, 1, 1),
            null,
            -1m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(LinhaOnibusService.MensagemTarifa, ex.Message);
    }

    [Fact]
    public async Task AtualizarAsync_EncerrarComVinculoAberto_DeveFalhar()
    {
        var id = Guid.NewGuid();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = id,
                Descricao = "Linha 100",
                DataInicio = new DateOnly(2026, 1, 1),
                ValorTarifa = 4.50m,
            });
        _vinculoRepo.Setup(r => r.ExisteVinculoAbertoPorLinhaAsync(id)).ReturnsAsync(true);

        var dto = new LinhaOnibusAtualizarDto(
            "Linha 100",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            4.50m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AtualizarAsync(id, dto, null));
        Assert.Equal(LinhaOnibusService.MensagemVinculosAbertos, ex.Message);
        _linhaRepo.Verify(r => r.AtualizarAsync(It.IsAny<LinhaOnibusAtualizarParams>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_NaoEncontrada_DeveFalhar()
    {
        var id = Guid.NewGuid();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((LinhaOnibusQueryResult?)null);

        var dto = new LinhaOnibusAtualizarDto(
            "Linha 100",
            new DateOnly(2026, 1, 1),
            null,
            4.50m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.AtualizarAsync(id, dto, null));
        Assert.Equal(LinhaOnibusService.MensagemNaoEncontrada, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_Valido_DevePersistir()
    {
        _linhaRepo.Setup(r => r.SalvarAsync(It.IsAny<LinhaOnibusSalvarParams>()))
            .ReturnsAsync((LinhaOnibusSalvarParams p) => p.Id);

        var id = await _sut.SalvarAsync(new LinhaOnibusSalvarDto(
            "Linha 100",
            new DateOnly(2026, 1, 1),
            null,
            4.50m), null);

        Assert.NotEqual(Guid.Empty, id);
        _linhaRepo.Verify(r => r.SalvarAsync(It.Is<LinhaOnibusSalvarParams>(
            p => p.Descricao == "Linha 100" && p.ValorTarifa == 4.50m)), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_EncerrarSemVinculo_DevePersistir()
    {
        var id = Guid.NewGuid();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = id,
                Descricao = "Linha 100",
                DataInicio = new DateOnly(2026, 1, 1),
                ValorTarifa = 4.50m,
            });
        _vinculoRepo.Setup(r => r.ExisteVinculoAbertoPorLinhaAsync(id)).ReturnsAsync(false);

        await _sut.AtualizarAsync(id, new LinhaOnibusAtualizarDto(
            "Linha 100",
            new DateOnly(2026, 1, 1),
            new DateOnly(2026, 6, 30),
            5.00m), null);

        _linhaRepo.Verify(r => r.AtualizarAsync(It.Is<LinhaOnibusAtualizarParams>(
            p => p.Id == id && p.DataFim == new DateOnly(2026, 6, 30) && p.ValorTarifa == 5.00m)), Times.Once);
    }

    [Fact]
    public async Task FiltrarAsync_SomenteVigentes_DeveFiltrar()
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        _linhaRepo.Setup(r => r.FiltrarAsync(It.IsAny<LinhaOnibusFiltroParams>()))
            .ReturnsAsync(
            [
                new LinhaOnibusQueryResult
                {
                    Id = Guid.NewGuid(),
                    Descricao = "Vigente",
                    DataInicio = hoje.AddDays(-10),
                    DataFim = null,
                    ValorTarifa = 1m,
                },
                new LinhaOnibusQueryResult
                {
                    Id = Guid.NewGuid(),
                    Descricao = "Encerrada",
                    DataInicio = hoje.AddDays(-30),
                    DataFim = hoje.AddDays(-1),
                    ValorTarifa = 2m,
                },
            ]);

        var lista = await _sut.FiltrarAsync(null, true);
        Assert.Single(lista);
        Assert.Equal("Vigente", lista[0].Descricao);
    }

    [Fact]
    public async Task FiltrarAsync_SomenteVigentesComReferencia_DeveUsarDataInformada()
    {
        var referencia = new DateOnly(2025, 6, 15);
        _linhaRepo.Setup(r => r.FiltrarAsync(It.Is<LinhaOnibusFiltroParams>(
                f => f.SomenteVigentes == true && f.Referencia == referencia)))
            .ReturnsAsync(
            [
                new LinhaOnibusQueryResult
                {
                    Id = Guid.NewGuid(),
                    Descricao = "Na referencia",
                    DataInicio = new DateOnly(2025, 1, 1),
                    DataFim = new DateOnly(2025, 12, 31),
                    ValorTarifa = 1m,
                },
                new LinhaOnibusQueryResult
                {
                    Id = Guid.NewGuid(),
                    Descricao = "Fora",
                    DataInicio = new DateOnly(2026, 1, 1),
                    DataFim = null,
                    ValorTarifa = 2m,
                },
            ]);

        var lista = await _sut.FiltrarAsync(null, true, referencia);
        Assert.Single(lista);
        Assert.Equal("Na referencia", lista[0].Descricao);
    }

    [Fact]
    public async Task ObterPorIdAsync_NaoEncontrada_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((LinhaOnibusQueryResult?)null);
        Assert.Null(await _sut.ObterPorIdAsync(id));
    }

    [Fact]
    public async Task ObterPorIdAsync_Encontrada_DeveMapear()
    {
        var id = Guid.NewGuid();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = id,
                Descricao = "Linha 100",
                DataInicio = new DateOnly(2026, 1, 1),
                ValorTarifa = 4.50m,
            });

        var dto = await _sut.ObterPorIdAsync(id);
        Assert.NotNull(dto);
        Assert.Equal("Linha 100", dto!.Descricao);
        Assert.Equal(4.50m, dto.ValorTarifa);
    }

    [Fact]
    public async Task FiltrarAsync_SemSomenteVigentes_DeveRetornarTodas()
    {
        _linhaRepo.Setup(r => r.FiltrarAsync(It.IsAny<LinhaOnibusFiltroParams>()))
            .ReturnsAsync(
            [
                new LinhaOnibusQueryResult
                {
                    Id = Guid.NewGuid(),
                    Descricao = "A",
                    DataInicio = new DateOnly(2026, 1, 1),
                    ValorTarifa = 1m,
                },
                new LinhaOnibusQueryResult
                {
                    Id = Guid.NewGuid(),
                    Descricao = "B",
                    DataInicio = new DateOnly(2025, 1, 1),
                    DataFim = new DateOnly(2025, 12, 31),
                    ValorTarifa = 2m,
                },
            ]);

        var lista = await _sut.FiltrarAsync("x", false);
        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public async Task AtualizarAsync_SemDataFim_NaoConsultaVinculos()
    {
        var id = Guid.NewGuid();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = id,
                Descricao = "Linha 100",
                DataInicio = new DateOnly(2026, 1, 1),
                ValorTarifa = 4.50m,
            });

        await _sut.AtualizarAsync(id, new LinhaOnibusAtualizarDto(
            "Linha 100",
            new DateOnly(2026, 1, 1),
            null,
            5.00m), null);

        _vinculoRepo.Verify(r => r.ExisteVinculoAbertoPorLinhaAsync(It.IsAny<Guid>()), Times.Never);
        _linhaRepo.Verify(r => r.AtualizarAsync(It.IsAny<LinhaOnibusAtualizarParams>()), Times.Once);
    }
}