using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class PerfilRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Perfil Teste";

    [SkippableFact]
    public async Task SalvarAsync_DeveInserirPerfilComPermissoes()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new PerfilRepository(tenantConnection);
        var perfilId = Guid.NewGuid();

        var result = await repository.SalvarAsync(new PerfilSalvarParams
        {
            Id = perfilId,
            Nome = "Operador",
            EhSistema = false,
            Permissoes =
            [
                new PerfilPermissaoParams
                {
                    Id = Guid.NewGuid(),
                    PerfilId = perfilId,
                    CodigoMenu = "dashboard",
                    Visualizar = true,
                },
                new PerfilPermissaoParams
                {
                    Id = Guid.NewGuid(),
                    PerfilId = perfilId,
                    CodigoMenu = "usuarios",
                    Visualizar = true,
                    Criar = true,
                    Editar = true,
                },
                new PerfilPermissaoParams
                {
                    Id = Guid.NewGuid(),
                    PerfilId = perfilId,
                    CodigoMenu = "perfis",
                    Visualizar = true,
                },
            ],
        });

        Assert.Equal(perfilId, result);

        var perfil = await repository.ObterUmAsync(perfilId);
        Assert.NotNull(perfil);
        Assert.Equal("Operador", perfil!.Nome);
        Assert.False(perfil.EhSistema);
        Assert.Equal(3, perfil.Permissoes.Count);
        Assert.Contains(perfil.Permissoes, p => p.CodigoMenu == "usuarios" && p.Criar);
    }

    [SkippableFact]
    public async Task FiltrarAsync_DeveFiltrarPorNome()
    {
        await PostgresTestHelper.PrepareAsync(fixture);
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new PerfilRepository(tenantConnection);

        await repository.SalvarAsync(new PerfilSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Consulta",
            Permissoes = [],
        });
        await repository.SalvarAsync(new PerfilSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Administração Local",
            Permissoes = [],
        });

        var filtrados = await repository.FiltrarAsync(new PerfilFiltroParams { Nome = "Consul" });

        Assert.Single(filtrados);
        Assert.Equal("Consulta", filtrados[0].Nome);
    }
}
