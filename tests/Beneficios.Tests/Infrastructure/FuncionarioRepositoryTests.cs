using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Dapper;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class FuncionarioRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Func Repo Motivo";

    [SkippableFact]
    public async Task ObterEFiltrar_ComAfastamentoAtivo_DevePreencherMotivo()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var conn = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var funcRepo = new FuncionarioRepository(conn);
        var afastRepo = new FuncionarioAfastamentoRepository(conn);

        var funcionarioId = Guid.NewGuid();
        await funcRepo.SalvarAsync(new FuncionarioSalvarParams
        {
            Id = funcionarioId,
            Nome = "Maria Motivo",
            Cpf = "39053344705",
            DataAdmissao = new DateOnly(2024, 1, 1),
            Cargo = "Analista",
            SalarioBase = 2000m,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Ativo,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataInclusao = DateTime.UtcNow,
        }, []);

        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        await afastRepo.SalvarAsync(new FuncionarioAfastamentoSalvarParams
        {
            Id = Guid.NewGuid(),
            FuncionarioId = funcionarioId,
            Tipo = "ferias",
            DataInicio = hoje.AddDays(-2),
            DataFim = null,
            DataInclusao = DateTime.UtcNow,
        });

        var porId = await funcRepo.ObterPorIdAsync(funcionarioId);
        Assert.Equal("ferias", porId!.MotivoAfastamentoAtivo);

        var lista = await funcRepo.FiltrarAsync(new FuncionarioFiltroParams());
        Assert.Contains(lista, f => f.Id == funcionarioId && f.MotivoAfastamentoAtivo == "ferias");
    }

    [SkippableFact]
    public async Task Filtrar_SemTabelaAfastamentos_DeveRetornarSemMotivo()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var conn = await fixture.CreateTenantConnectionAsync($"{TenantRazaoSocial} SoftFail");
        var funcRepo = new FuncionarioRepository(conn);

        var funcionarioId = Guid.NewGuid();
        await funcRepo.SalvarAsync(new FuncionarioSalvarParams
        {
            Id = funcionarioId,
            Nome = "Pedro Soft",
            Cpf = "15337551640",
            DataAdmissao = new DateOnly(2024, 1, 1),
            Cargo = "Analista",
            SalarioBase = 2000m,
            TipoContrato = TipoContrato.Clt,
            Situacao = SituacaoFuncionario.Ativo,
            Jornada = JornadaTrabalho.QuarentaHorasSegSex,
            DataInclusao = DateTime.UtcNow,
        }, []);

        await conn.ExecuteAsync("DROP TABLE IF EXISTS funcionario_afastamentos CASCADE");

        var porId = await funcRepo.ObterPorIdAsync(funcionarioId);
        Assert.NotNull(porId);
        Assert.Null(porId!.MotivoAfastamentoAtivo);

        var lista = await funcRepo.FiltrarAsync(new FuncionarioFiltroParams());
        Assert.Contains(lista, f => f.Id == funcionarioId && f.MotivoAfastamentoAtivo is null);
    }
}
