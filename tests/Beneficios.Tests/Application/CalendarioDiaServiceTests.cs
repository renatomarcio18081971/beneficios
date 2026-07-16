using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class CalendarioDiaServiceTests
{
    private readonly Mock<ICalendarioDiaRepository> _repositoryMock = new();
    private readonly CalendarioDiaService _service;

    public CalendarioDiaServiceTests()
    {
        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<CalendarioDiaProfile>()).CreateMapper();
        _service = new CalendarioDiaService(_repositoryMock.Object, mapper);
    }

    [Fact]
    public async Task GerarAnoAsync_QuandoAnoNovo_DeveInserirTodosOsDias()
    {
        _repositoryMock.Setup(x => x.AnoExisteAsync(2026)).ReturnsAsync(false);
        IReadOnlyList<CalendarioDiaSalvarParams>? capturado = null;
        _repositoryMock
            .Setup(x => x.InserirLoteAsync(It.IsAny<IReadOnlyList<CalendarioDiaSalvarParams>>()))
            .Callback<IReadOnlyList<CalendarioDiaSalvarParams>>(d => capturado = d)
            .Returns(Task.CompletedTask);

        await _service.GerarAnoAsync(2026);

        Assert.NotNull(capturado);
        Assert.Equal(365, capturado!.Count);
        Assert.Contains(capturado, d => d.Origem == OrigemCalendarioDia.Nacional && d.Data == new DateOnly(2026, 1, 1));
        Assert.Contains(capturado, d => d.Data == new DateOnly(2026, 1, 2) && d.EhDiaUtil); // sexta
        Assert.Contains(capturado, d => d.Data == new DateOnly(2026, 1, 3) && !d.EhDiaUtil); // sábado
    }

    [Fact]
    public async Task GerarAnoAsync_QuandoAnoExiste_DeveLancar()
    {
        _repositoryMock.Setup(x => x.AnoExisteAsync(2026)).ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GerarAnoAsync(2026));
        Assert.Equal(CalendarioDiaService.MensagemAnoJaCadastrado, ex.Message);
        _repositoryMock.Verify(x => x.InserirLoteAsync(It.IsAny<IReadOnlyList<CalendarioDiaSalvarParams>>()), Times.Never);
    }

    [Fact]
    public async Task AtualizarAsync_DeveMarcarOrigemManual()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new CalendarioDiaQueryResult
        {
            Id = id,
            Data = new DateOnly(2026, 5, 1),
            EhDiaUtil = false,
            Origem = OrigemCalendarioDia.Nacional,
        });
        CalendarioDiaAtualizarParams? capturado = null;
        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<CalendarioDiaAtualizarParams>()))
            .Callback<CalendarioDiaAtualizarParams>(p => capturado = p)
            .ReturnsAsync(true);

        await _service.AtualizarAsync(id, new CalendarioDiaAtualizarDto(true, TipoExcecaoCalendario.Municipal, "BH"), null);

        Assert.NotNull(capturado);
        Assert.Equal(OrigemCalendarioDia.Manual, capturado!.Origem);
        Assert.True(capturado.EhDiaUtil);
    }

    [Fact]
    public async Task ObterPorIdAsync_NaoEncontrado_DeveRetornarNull()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((CalendarioDiaQueryResult?)null);
        Assert.Null(await _service.ObterPorIdAsync(id));
    }

    [Fact]
    public async Task ObterPorMesAsync_DeveMapear()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorMesAsync(2026, 7)).ReturnsAsync(
        [
            new CalendarioDiaQueryResult
            {
                Id = id,
                Data = new DateOnly(2026, 7, 1),
                EhDiaUtil = true,
                Origem = OrigemCalendarioDia.Geracao,
            },
        ]);

        var dias = await _service.ObterPorMesAsync(2026, 7);
        Assert.Single(dias);
        Assert.Equal(id, dias[0].Id);
        Assert.True(dias[0].EhDiaUtil);
    }

    [Fact]
    public async Task AtualizarAsync_NaoEncontrado_DeveLancar()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync((CalendarioDiaQueryResult?)null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AtualizarAsync(id, new CalendarioDiaAtualizarDto(true, null, null), null));
        Assert.Equal(CalendarioDiaService.MensagemDiaNaoEncontrado, ex.Message);
    }

    [Fact]
    public async Task AtualizarAsync_QuandoRepoRetornaFalse_DeveLancar()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(x => x.ObterPorIdAsync(id)).ReturnsAsync(new CalendarioDiaQueryResult
        {
            Id = id,
            Data = new DateOnly(2026, 5, 1),
            EhDiaUtil = true,
            Origem = OrigemCalendarioDia.Geracao,
        });
        _repositoryMock.Setup(x => x.AtualizarAsync(It.IsAny<CalendarioDiaAtualizarParams>())).ReturnsAsync(false);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.AtualizarAsync(id, new CalendarioDiaAtualizarDto(false, null, null), null));
        Assert.Equal(CalendarioDiaService.MensagemDiaNaoEncontrado, ex.Message);
    }

    [Theory]
    [InlineData(2024, 366)]
    [InlineData(2025, 365)]
    public void MontarDiasDoAno_DeveRespeitarAnoBissexto(int ano, int esperado)
    {
        Assert.Equal(esperado, CalendarioAnoFabrica.MontarDiasDoAno(ano).Count);
    }
}
