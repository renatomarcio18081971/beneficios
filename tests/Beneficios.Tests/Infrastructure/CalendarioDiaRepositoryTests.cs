using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class CalendarioDiaRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Calendario Teste";

    [SkippableFact]
    public async Task InserirLote_E_ObterPorMes_DevePersistir()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new CalendarioDiaRepository(tenantConnection);

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        await repository.InserirLoteAsync(
        [
            new CalendarioDiaSalvarParams
            {
                Id = id1,
                Data = new DateOnly(2026, 3, 2),
                EhDiaUtil = true,
                Origem = OrigemCalendarioDia.Geracao,
            },
            new CalendarioDiaSalvarParams
            {
                Id = id2,
                Data = new DateOnly(2026, 3, 1),
                EhDiaUtil = false,
                TipoExcecao = TipoExcecaoCalendario.Nacional,
                Origem = OrigemCalendarioDia.Nacional,
                Observacao = "Teste",
            },
        ]);

        Assert.True(await repository.AnoExisteAsync(2026));
        Assert.False(await repository.AnoExisteAsync(2025));

        var mes = await repository.ObterPorMesAsync(2026, 3);
        Assert.Equal(2, mes.Length);
        Assert.Equal(id2, mes[0].Id);
        Assert.False(mes[0].EhDiaUtil);
        Assert.Equal(TipoExcecaoCalendario.Nacional, mes[0].TipoExcecao);

        var atualizado = await repository.AtualizarAsync(new CalendarioDiaAtualizarParams
        {
            Id = id2,
            EhDiaUtil = true,
            TipoExcecao = TipoExcecaoCalendario.Municipal,
            Origem = OrigemCalendarioDia.Manual,
            Observacao = "Ajuste",
        });
        Assert.True(atualizado);

        var dia = await repository.ObterPorIdAsync(id2);
        Assert.NotNull(dia);
        Assert.True(dia!.EhDiaUtil);
        Assert.Equal(OrigemCalendarioDia.Manual, dia.Origem);
        Assert.Equal("Ajuste", dia.Observacao);
    }
}
