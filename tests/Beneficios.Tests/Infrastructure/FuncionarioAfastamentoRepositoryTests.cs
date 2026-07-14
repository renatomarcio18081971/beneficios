using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class FuncionarioAfastamentoRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Afastamento Repo";

    [SkippableFact]
    public async Task Salvar_Filtrar_Sobreposicao_Excluir_DeveFuncionar()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var conn = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var funcRepo = new FuncionarioRepository(conn);
        var afastRepo = new FuncionarioAfastamentoRepository(conn);

        var funcionarioId = Guid.NewGuid();
        await funcRepo.SalvarAsync(new FuncionarioSalvarParams
        {
            Id = funcionarioId,
            Nome = "Joao Afastado",
            Cpf = "52998224725",
            DataAdmissao = new DateOnly(2024, 1, 1),
            Cargo = "Analista",
            SalarioBase = 2000m,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Ativo,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataInclusao = DateTime.UtcNow,
        }, []);

        var afastId = Guid.NewGuid();
        await afastRepo.SalvarAsync(new FuncionarioAfastamentoSalvarParams
        {
            Id = afastId,
            FuncionarioId = funcionarioId,
            Tipo = "ferias",
            DataInicio = new DateOnly(2026, 1, 1),
            DataFim = new DateOnly(2026, 1, 20),
            Observacao = "Recesso",
            DataInclusao = DateTime.UtcNow,
        });

        Assert.True(await afastRepo.ExisteSobreposicaoAsync(
            funcionarioId, new DateOnly(2026, 1, 15), new DateOnly(2026, 1, 25)));
        Assert.False(await afastRepo.ExisteSobreposicaoAsync(
            funcionarioId, new DateOnly(2026, 2, 1), new DateOnly(2026, 2, 10)));

        var lista = await afastRepo.FiltrarAsync(new FuncionarioAfastamentoFiltroParams
        {
            FuncionarioId = funcionarioId,
            DataInicio = new DateOnly(2026, 1, 1),
            DataFim = new DateOnly(2026, 1, 31),
        });
        Assert.Single(lista);
        Assert.Equal("Joao Afastado", lista[0].FuncionarioNome);

        var ativo = await afastRepo.ObterAtivoEmAsync(funcionarioId, new DateOnly(2026, 1, 10));
        Assert.NotNull(ativo);
        Assert.Equal(afastId, ativo!.Id);

        await afastRepo.AtualizarAsync(new FuncionarioAfastamentoAtualizarParams
        {
            Id = afastId,
            FuncionarioId = funcionarioId,
            Tipo = "licenca_medica",
            DataInicio = new DateOnly(2026, 1, 1),
            DataFim = new DateOnly(2026, 1, 25),
            Observacao = "Atualizado",
            DataAlteracao = DateTime.UtcNow,
        });

        var detalhe = await afastRepo.ObterPorIdAsync(afastId);
        Assert.Equal("licenca_medica", detalhe!.Tipo);

        var porFunc = await afastRepo.ListarPorFuncionarioAsync(funcionarioId);
        Assert.Single(porFunc);

        await funcRepo.AtualizarSituacaoAsync(funcionarioId, "afastado");
        var ids = await funcRepo.ListarIdSituacaoAsync();
        Assert.Contains(ids, x => x.Id == funcionarioId && x.Situacao == "afastado");

        var filtrado = await funcRepo.FiltrarAsync(new FuncionarioFiltroParams { Situacao = SituacaoFuncionario.Afastado });
        Assert.Contains(filtrado, f => f.Id == funcionarioId);

        await afastRepo.ExcluirAsync(afastId);
        Assert.Null(await afastRepo.ObterPorIdAsync(afastId));
    }
}