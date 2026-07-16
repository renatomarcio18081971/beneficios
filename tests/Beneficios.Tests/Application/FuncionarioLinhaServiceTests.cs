using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class FuncionarioLinhaServiceTests
{
    private readonly Mock<IFuncionarioLinhaRepository> _vinculoRepo = new();
    private readonly Mock<ILinhaOnibusRepository> _linhaRepo = new();
    private readonly Mock<IFuncionarioRepository> _funcionarioRepo = new();
    private readonly FuncionarioLinhaService _sut;

    private readonly Guid _funcionarioId = Guid.NewGuid();
    private readonly Guid _linhaId = Guid.NewGuid();

    public FuncionarioLinhaServiceTests()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<FuncionarioLinhaProfile>()).CreateMapper();
        _sut = new FuncionarioLinhaService(
            _vinculoRepo.Object,
            _linhaRepo.Object,
            _funcionarioRepo.Object,
            mapper);
    }

    [Fact]
    public async Task SalvarAsync_VtInativo_DeveFalhar()
    {
        SetupFuncionarioExiste();
        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
            .ReturnsAsync(
            [
                new FuncionarioBeneficioQueryResult
                {
                    CodigoBeneficio = FuncionarioLinhaService.CodigoValeTransporte,
                    Ativo = false,
                },
            ]);
        SetupLinhaVigente();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemVtInativo, ex.Message);
        _vinculoRepo.Verify(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()), Times.Never);
    }

    [Fact]
    public async Task SalvarAsync_SemBeneficioVt_DeveFalhar()
    {
        SetupFuncionarioExiste();
        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
            .ReturnsAsync(Array.Empty<FuncionarioBeneficioQueryResult>());
        SetupLinhaVigente();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemVtInativo, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_LinhaNaoVigente_DeveFalhar()
    {
        SetupFuncionarioComVt();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = _linhaId,
                Descricao = "Linha 100",
                DataInicio = new DateOnly(2025, 1, 1),
                DataFim = new DateOnly(2025, 12, 31),
                ValorTarifa = 4.50m,
            });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(dataInicio: new DateOnly(2026, 1, 15)), null));

        Assert.Equal(FuncionarioLinhaService.MensagemLinhaNaoVigente, ex.Message);
        _vinculoRepo.Verify(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()), Times.Never);
    }

    [Fact]
    public async Task SalvarAsync_ParDuplicado_DeveFalhar()
    {
        SetupFuncionarioComVt();
        SetupLinhaVigente();
        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, null)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemParDuplicado, ex.Message);
        _vinculoRepo.Verify(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()), Times.Never);
    }

    [Fact]
    public async Task SalvarAsync_QuantidadeZero_DeveFalhar()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(quantidade: 0), null));

        Assert.Equal(FuncionarioLinhaService.MensagemQuantidade, ex.Message);
        _funcionarioRepo.Verify(r => r.ObterPorIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task SalvarAsync_DataFimMenor_DeveFalhar()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(
                dataInicio: new DateOnly(2026, 1, 31),
                dataFim: new DateOnly(2026, 1, 1)), null));

        Assert.Equal(FuncionarioLinhaService.MensagemDataFim, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_FuncionarioNaoEncontrado_DeveFalhar()
    {
        _funcionarioRepo.Setup(r => r.ObterPorIdAsync(_funcionarioId))
            .ReturnsAsync((FuncionarioQueryResult?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemFuncionarioNaoEncontrado, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_LinhaNaoEncontrada_DeveFalhar()
    {
        SetupFuncionarioComVt();
        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId)).ReturnsAsync((LinhaOnibusQueryResult?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.SalvarAsync(DtoSalvar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemLinhaNaoEncontrada, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_Valido_DevePersistir()
    {
        SetupFuncionarioComVt();
        SetupLinhaVigente();
        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, null)).ReturnsAsync(false);
        _vinculoRepo.Setup(r => r.SalvarAsync(It.IsAny<FuncionarioLinhaSalvarParams>()))
            .ReturnsAsync((FuncionarioLinhaSalvarParams p) => p.Id);

        var id = await _sut.SalvarAsync(DtoSalvar(quantidade: 2), null);

        Assert.NotEqual(Guid.Empty, id);
        _vinculoRepo.Verify(r => r.SalvarAsync(It.Is<FuncionarioLinhaSalvarParams>(
            p => p.FuncionarioId == _funcionarioId
                 && p.LinhaOnibusId == _linhaId
                 && p.Quantidade == 2
                 && p.DataInicio == new DateOnly(2026, 1, 1))), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_VinculoNaoEncontrado_DeveFalhar()
    {
        var id = Guid.NewGuid();
        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioLinhaQueryResult?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.AtualizarAsync(id, DtoAtualizar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemVinculoNaoEncontrado, ex.Message);
    }

    [Fact]
    public async Task AtualizarAsync_Valido_DevePersistir()
    {
        var id = Guid.NewGuid();
        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new FuncionarioLinhaQueryResult
            {
                Id = id,
                FuncionarioId = _funcionarioId,
                LinhaOnibusId = _linhaId,
                Quantidade = 1,
                DataInicio = new DateOnly(2026, 1, 1),
            });
        SetupFuncionarioComVt();
        SetupLinhaVigente();
        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, id)).ReturnsAsync(false);

        await _sut.AtualizarAsync(id, DtoAtualizar(quantidade: 3), null);

        _vinculoRepo.Verify(r => r.AtualizarAsync(It.Is<FuncionarioLinhaAtualizarParams>(
            p => p.Id == id && p.Quantidade == 3 && p.LinhaOnibusId == _linhaId)), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_EncerrarSemVt_DevePermitir()
    {
        var id = Guid.NewGuid();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new FuncionarioLinhaQueryResult
            {
                Id = id,
                FuncionarioId = _funcionarioId,
                LinhaOnibusId = _linhaId,
                Quantidade = 1,
                DataInicio = hoje.AddDays(-30),
            });
        SetupFuncionarioExiste();
        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
            .ReturnsAsync(Array.Empty<FuncionarioBeneficioQueryResult>());
        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = _linhaId,
                Descricao = "Linha 100",
                DataInicio = hoje.AddDays(-60),
                DataFim = null,
                ValorTarifa = 4.50m,
            });
        _vinculoRepo.Setup(r => r.ExisteParAsync(_funcionarioId, _linhaId, id)).ReturnsAsync(false);

        await _sut.AtualizarAsync(id, new FuncionarioLinhaAtualizarDto(
            _linhaId, 1, hoje.AddDays(-30), hoje.AddDays(-1)), null);

        _vinculoRepo.Verify(r => r.AtualizarAsync(It.Is<FuncionarioLinhaAtualizarParams>(
            p => p.Id == id && p.DataFim == hoje.AddDays(-1))), Times.Once);
        _funcionarioRepo.Verify(r => r.ObterBeneficiosAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_ManterAbertoSemVt_DeveFalhar()
    {
        var id = Guid.NewGuid();
        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new FuncionarioLinhaQueryResult
            {
                Id = id,
                FuncionarioId = _funcionarioId,
                LinhaOnibusId = _linhaId,
                Quantidade = 1,
                DataInicio = new DateOnly(2026, 1, 1),
            });
        SetupFuncionarioExiste();
        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
            .ReturnsAsync(Array.Empty<FuncionarioBeneficioQueryResult>());
        SetupLinhaVigente();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sut.AtualizarAsync(id, DtoAtualizar(), null));

        Assert.Equal(FuncionarioLinhaService.MensagemVtInativo, ex.Message);
        _vinculoRepo.Verify(r => r.AtualizarAsync(It.IsAny<FuncionarioLinhaAtualizarParams>()), Times.Never);
    }

    [Fact]
    public async Task FiltrarAsync_SomenteVigentes_DeveFiltrar()
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        _vinculoRepo.Setup(r => r.FiltrarAsync(It.IsAny<FuncionarioLinhaFiltroParams>()))
            .ReturnsAsync(
            [
                new FuncionarioLinhaQueryResult
                {
                    Id = Guid.NewGuid(),
                    FuncionarioId = _funcionarioId,
                    FuncionarioNome = "Ana",
                    LinhaOnibusId = _linhaId,
                    LinhaDescricao = "Vigente",
                    Quantidade = 1,
                    DataInicio = hoje.AddDays(-10),
                    DataFim = null,
                },
                new FuncionarioLinhaQueryResult
                {
                    Id = Guid.NewGuid(),
                    FuncionarioId = _funcionarioId,
                    FuncionarioNome = "Ana",
                    LinhaOnibusId = Guid.NewGuid(),
                    LinhaDescricao = "Encerrada",
                    Quantidade = 1,
                    DataInicio = hoje.AddDays(-30),
                    DataFim = hoje.AddDays(-1),
                },
            ]);

        var lista = await _sut.FiltrarAsync(_funcionarioId, true);
        Assert.Single(lista);
        Assert.Equal("Vigente", lista[0].LinhaDescricao);
    }

    [Fact]
    public async Task ObterPorIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id)).ReturnsAsync((FuncionarioLinhaQueryResult?)null);
        Assert.Null(await _sut.ObterPorIdAsync(id));
    }

    [Fact]
    public async Task ObterPorIdAsync_Encontrado_DeveMapear()
    {
        var id = Guid.NewGuid();
        _vinculoRepo.Setup(r => r.ObterPorIdAsync(id))
            .ReturnsAsync(new FuncionarioLinhaQueryResult
            {
                Id = id,
                FuncionarioId = _funcionarioId,
                FuncionarioNome = "Ana",
                LinhaOnibusId = _linhaId,
                LinhaDescricao = "Linha 100",
                Quantidade = 2,
                DataInicio = new DateOnly(2026, 1, 1),
            });

        var dto = await _sut.ObterPorIdAsync(id);
        Assert.NotNull(dto);
        Assert.Equal("Ana", dto!.FuncionarioNome);
        Assert.Equal(2, dto.Quantidade);
    }

    [Fact]
    public async Task FiltrarAsync_SemSomenteVigentes_DeveRetornarTodos()
    {
        _vinculoRepo.Setup(r => r.FiltrarAsync(It.IsAny<FuncionarioLinhaFiltroParams>()))
            .ReturnsAsync(
            [
                new FuncionarioLinhaQueryResult
                {
                    Id = Guid.NewGuid(),
                    FuncionarioId = _funcionarioId,
                    FuncionarioNome = "Ana",
                    LinhaOnibusId = _linhaId,
                    LinhaDescricao = "A",
                    Quantidade = 1,
                    DataInicio = new DateOnly(2026, 1, 1),
                },
                new FuncionarioLinhaQueryResult
                {
                    Id = Guid.NewGuid(),
                    FuncionarioId = _funcionarioId,
                    FuncionarioNome = "Ana",
                    LinhaOnibusId = Guid.NewGuid(),
                    LinhaDescricao = "B",
                    Quantidade = 1,
                    DataInicio = new DateOnly(2025, 1, 1),
                    DataFim = new DateOnly(2025, 6, 1),
                },
            ]);

        var lista = await _sut.FiltrarAsync(_funcionarioId, false);
        Assert.Equal(2, lista.Count);
    }

    [Fact]
    public async Task EncerrarAbertosPorFuncionarioAsync_DeveDelegarRepositorio()
    {
        var usuarioId = Guid.NewGuid();
        await _sut.EncerrarAbertosPorFuncionarioAsync(_funcionarioId, usuarioId);

        _vinculoRepo.Verify(
            r => r.EncerrarAbertosPorFuncionarioAsync(
                _funcionarioId,
                It.IsAny<DateOnly>(),
                It.IsAny<DateTime>(),
                usuarioId),
            Times.Once);
    }

    private FuncionarioLinhaSalvarDto DtoSalvar(
        int quantidade = 1,
        DateOnly? dataInicio = null,
        DateOnly? dataFim = null) =>
        new(_funcionarioId, _linhaId, quantidade, dataInicio ?? new DateOnly(2026, 1, 1), dataFim);

    private FuncionarioLinhaAtualizarDto DtoAtualizar(int quantidade = 1) =>
        new(_linhaId, quantidade, new DateOnly(2026, 1, 1), null);

    private void SetupFuncionarioExiste()
    {
        _funcionarioRepo.Setup(r => r.ObterPorIdAsync(_funcionarioId))
            .ReturnsAsync(new FuncionarioQueryResult { Id = _funcionarioId, Nome = "Ana" });
    }

    private void SetupFuncionarioComVt()
    {
        SetupFuncionarioExiste();
        _funcionarioRepo.Setup(r => r.ObterBeneficiosAsync(_funcionarioId))
            .ReturnsAsync(
            [
                new FuncionarioBeneficioQueryResult
                {
                    CodigoBeneficio = FuncionarioLinhaService.CodigoValeTransporte,
                    Ativo = true,
                },
            ]);
    }

    private void SetupLinhaVigente()
    {
        _linhaRepo.Setup(r => r.ObterPorIdAsync(_linhaId))
            .ReturnsAsync(new LinhaOnibusQueryResult
            {
                Id = _linhaId,
                Descricao = "Linha 100",
                DataInicio = new DateOnly(2026, 1, 1),
                DataFim = null,
                ValorTarifa = 4.50m,
            });
    }
}
