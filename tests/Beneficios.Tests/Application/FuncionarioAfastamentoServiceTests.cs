using AutoMapper;
using Beneficios.Application.DTOs;
using Beneficios.Application.Mappings;
using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Moq;
using Xunit;

namespace Beneficios.Tests.Application;

public class FuncionarioAfastamentoServiceTests
{
    private readonly Mock<IFuncionarioAfastamentoRepository> _afastRepo = new();
    private readonly Mock<IFuncionarioRepository> _funcRepo = new();
    private readonly FuncionarioAfastamentoService _sut;

    public FuncionarioAfastamentoServiceTests()
    {
        var mapper = new MapperConfiguration(c => c.AddProfile<AfastamentoProfile>()).CreateMapper();
        _sut = new FuncionarioAfastamentoService(_afastRepo.Object, _funcRepo.Object, mapper);
    }

    [Fact]
    public async Task SalvarAsync_Sobreposicao_DeveFalhar()
    {
        var funcionarioId = Guid.NewGuid();
        _funcRepo.Setup(r => r.ObterPorIdAsync(funcionarioId))
            .ReturnsAsync(CriarFuncionario(funcionarioId, SituacaoFuncionario.Ativo));
        _afastRepo.Setup(r => r.ExisteSobreposicaoAsync(funcionarioId, It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(), null))
            .ReturnsAsync(true);

        var dto = new AfastamentoSalvarDto(
            funcionarioId, TipoAfastamento.Ferias,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 1, 31), null);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SalvarAsync(dto, null));
        Assert.Equal(FuncionarioAfastamentoService.MensagemSobreposicao, ex.Message);
    }

    [Fact]
    public async Task SalvarAsync_Valido_DevePersistirERecalcAfastado()
    {
        var funcionarioId = Guid.NewGuid();
        _funcRepo.Setup(r => r.ObterPorIdAsync(funcionarioId))
            .ReturnsAsync(CriarFuncionario(funcionarioId, SituacaoFuncionario.Ativo));
        _afastRepo.Setup(r => r.ExisteSobreposicaoAsync(funcionarioId, It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(), null))
            .ReturnsAsync(false);
        _afastRepo.Setup(r => r.SalvarAsync(It.IsAny<FuncionarioAfastamentoSalvarParams>()))
            .ReturnsAsync((FuncionarioAfastamentoSalvarParams p) => p.Id);
        _afastRepo.Setup(r => r.ObterAtivoEmAsync(funcionarioId, It.IsAny<DateOnly>()))
            .ReturnsAsync(new FuncionarioAfastamentoQueryResult
            {
                Id = Guid.NewGuid(),
                FuncionarioId = funcionarioId,
                Tipo = "ferias",
                DataInicio = new DateOnly(2026, 1, 1),
            });

        var id = await _sut.SalvarAsync(new AfastamentoSalvarDto(
            funcionarioId, TipoAfastamento.Ferias,
            new DateOnly(2026, 1, 1), null, "obs"), null);

        Assert.NotEqual(Guid.Empty, id);
        _funcRepo.Verify(r => r.AtualizarSituacaoAsync(funcionarioId, "afastado"), Times.Once);
    }

    [Fact]
    public async Task ExcluirAsync_SemAtivo_DeveRecalcAtivo()
    {
        var afastamentoId = Guid.NewGuid();
        var funcionarioId = Guid.NewGuid();
        _afastRepo.Setup(r => r.ObterPorIdAsync(afastamentoId))
            .ReturnsAsync(new FuncionarioAfastamentoQueryResult
            {
                Id = afastamentoId,
                FuncionarioId = funcionarioId,
                Tipo = "ferias",
                DataInicio = new DateOnly(2026, 1, 1),
            });
        _funcRepo.Setup(r => r.ObterPorIdAsync(funcionarioId))
            .ReturnsAsync(CriarFuncionario(funcionarioId, SituacaoFuncionario.Afastado));
        _afastRepo.Setup(r => r.ObterAtivoEmAsync(funcionarioId, It.IsAny<DateOnly>()))
            .ReturnsAsync((FuncionarioAfastamentoQueryResult?)null);

        await _sut.ExcluirAsync(afastamentoId);

        _afastRepo.Verify(r => r.ExcluirAsync(afastamentoId), Times.Once);
        _funcRepo.Verify(r => r.AtualizarSituacaoAsync(funcionarioId, "ativo"), Times.Once);
    }

    [Fact]
    public async Task Recalc_Desligado_NaoAltera()
    {
        var funcionarioId = Guid.NewGuid();
        _funcRepo.Setup(r => r.ObterPorIdAsync(funcionarioId))
            .ReturnsAsync(CriarFuncionario(funcionarioId, SituacaoFuncionario.Desligado));
        _afastRepo.Setup(r => r.ExisteSobreposicaoAsync(funcionarioId, It.IsAny<DateOnly>(), It.IsAny<DateOnly?>(), null))
            .ReturnsAsync(false);
        _afastRepo.Setup(r => r.SalvarAsync(It.IsAny<FuncionarioAfastamentoSalvarParams>()))
            .ReturnsAsync((FuncionarioAfastamentoSalvarParams p) => p.Id);

        await _sut.SalvarAsync(new AfastamentoSalvarDto(
            funcionarioId, TipoAfastamento.LicencaMedica,
            new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 10), null), null);

        _funcRepo.Verify(r => r.AtualizarSituacaoAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    private static FuncionarioQueryResult CriarFuncionario(Guid id, SituacaoFuncionario situacao) => new()
    {
        Id = id,
        Nome = "Ana",
        Cpf = "52998224725",
        Cargo = "Analista",
        SalarioBase = 1,
        TipoContrato = TipoContrato.Clt,
        Situacao = situacao,
        Jornada = JornadaTrabalho.QuarentaHorasSegSex,
        DataAdmissao = new DateOnly(2024, 1, 1),
        DataInclusao = DateTime.UtcNow,
    };
}