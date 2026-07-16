using Beneficios.Application.Services;
using Beneficios.Domain;
using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class TenantResolverIntegrationTests(PostgresFixture fixture)
{
    [SkippableFact]
    public async Task ResolveAsync_DeveRetornarConexaoAbertaParaTenantCadastrado()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        const string razaoSocial = "Tenant Integração LTDA";
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = Guid.NewGuid(),
            RazaoSocial = razaoSocial,
            Dominio = "empresatest",
        });

        await PostgresFixture.ProvisionTenantSchemaAsync(fixture.Connection!, razaoSocial);

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.CatalogConnectionString,
            })
            .Build();

        var tenantResolver = new TenantResolver(new TenantCatalogRepository(configuration), configuration);
        var resolution = await tenantResolver.ResolveAsync("empresatest");

        Assert.NotNull(resolution);
        Assert.Equal("tenant_tenant_integra__o_ltda", resolution!.Schema);

        var builder = new NpgsqlConnectionStringBuilder(resolution.ConnectionString);
        Assert.Equal("tenant_tenant_integra__o_ltda,beneficios", builder.SearchPath);

        await using var tenantConnection = new NpgsqlConnection(resolution.ConnectionString);
        await tenantConnection.OpenAsync();
        Assert.Equal(System.Data.ConnectionState.Open, tenantConnection.State);
    }
}
