using Beneficios.Application.Services;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class TenantResolverIntegrationTests(PostgresFixture fixture)
{
    [SkippableFact]
    public async Task ResolveConnectionStringAsync_DeveRetornarConexaoAbertaParaTenantCadastrado()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var catalogConnectionString = fixture.CatalogConnectionString!;
        var catalogBuilder = new NpgsqlConnectionStringBuilder(catalogConnectionString);

        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = Guid.NewGuid(),
            RazaoSocial = "Tenant Integração LTDA",
            Dominio = "empresatest",
            NomeBanco = Criptografia.Encrypt(catalogBuilder.Database ?? "postgres"),
            UsuarioBanco = Criptografia.Encrypt(catalogBuilder.Username ?? "postgres"),
            SenhaBanco = Criptografia.Encrypt(catalogBuilder.Password ?? string.Empty),
        });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = catalogConnectionString,
            })
            .Build();

        var tenantResolver = new TenantResolver(new TenantCatalogRepository(configuration), configuration);
        var tenantConnectionString = await tenantResolver.ResolveConnectionStringAsync("empresatest");

        Assert.NotNull(tenantConnectionString);

        await using var tenantConnection = new NpgsqlConnection(tenantConnectionString);
        await tenantConnection.OpenAsync();
        Assert.Equal(System.Data.ConnectionState.Open, tenantConnection.State);
    }
}
