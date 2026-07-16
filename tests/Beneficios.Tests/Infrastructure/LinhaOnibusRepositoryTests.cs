using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class LinhaOnibusRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Linha Onibus Repo";

    [SkippableFact]
    public async Task Salvar_Filtrar_Atualizar_Vigencia_DeveFuncionar()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var conn = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repo = new LinhaOnibusRepository(conn);

        var vigenteId = Guid.NewGuid();
        var encerradaId = Guid.NewGuid();
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);

        await repo.SalvarAsync(new LinhaOnibusSalvarParams
        {
            Id = vigenteId,
            Descricao = "Linha Azul",
            DataInicio = hoje.AddDays(-30),
            DataFim = null,
            ValorTarifa = 4.50m,
            DataInclusao = DateTime.UtcNow,
        });

        await repo.SalvarAsync(new LinhaOnibusSalvarParams
        {
            Id = encerradaId,
            Descricao = "Linha Vermelha",
            DataInicio = hoje.AddDays(-60),
            DataFim = hoje.AddDays(-1),
            ValorTarifa = 5.00m,
            DataInclusao = DateTime.UtcNow,
        });

        var todas = await repo.FiltrarAsync(new LinhaOnibusFiltroParams());
        Assert.Equal(2, todas.Count);

        var vigentes = await repo.FiltrarAsync(new LinhaOnibusFiltroParams
        {
            SomenteVigentes = true,
            Referencia = hoje,
        });
        Assert.Single(vigentes);
        Assert.Equal(vigenteId, vigentes[0].Id);
        Assert.Equal("Linha Azul", vigentes[0].Descricao);

        var porDescricao = await repo.FiltrarAsync(new LinhaOnibusFiltroParams { Descricao = "Azul" });
        Assert.Single(porDescricao);

        var detalhe = await repo.ObterPorIdAsync(vigenteId);
        Assert.NotNull(detalhe);
        Assert.Equal(4.50m, detalhe!.ValorTarifa);

        await repo.AtualizarAsync(new LinhaOnibusAtualizarParams
        {
            Id = vigenteId,
            Descricao = "Linha Azul Atualizada",
            DataInicio = hoje.AddDays(-30),
            DataFim = hoje.AddDays(30),
            ValorTarifa = 4.75m,
            DataAlteracao = DateTime.UtcNow,
        });

        var atualizada = await repo.ObterPorIdAsync(vigenteId);
        Assert.Equal("Linha Azul Atualizada", atualizada!.Descricao);
        Assert.Equal(4.75m, atualizada.ValorTarifa);
        Assert.Equal(hoje.AddDays(30), atualizada.DataFim);
    }
}
