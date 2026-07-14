using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Beneficios.Infrastructure.Tenancy;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class TenantIsolationTests(PostgresFixture fixture)
{
    [SkippableFact]
    public async Task UsuariosDeUmTenant_NaoDevemAparecerNoOutroTenant()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        var empresaAId = Guid.NewGuid();
        var empresaBId = Guid.NewGuid();

        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaAId,
            RazaoSocial = "Empresa A",
            Dominio = "tenanta",
        });
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaBId,
            RazaoSocial = "Empresa B",
            Dominio = "tenantb",
        });

        await using var connectionA = await fixture.CreateTenantConnectionAsync("Empresa A");
        await using var connectionB = await fixture.CreateTenantConnectionAsync("Empresa B");

        var repositoryA = new UsuarioRepository(connectionA, TenantSchemaAccessor.ParaTenant("tenant_test"));
        var repositoryB = new UsuarioRepository(connectionB, TenantSchemaAccessor.ParaTenant("tenant_test"));

        await repositoryA.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Usuario A",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "usuarioa@example.com",
            EmpresaId = empresaAId,
        });

        var usuariosA = await repositoryA.ObterTodosAsync();
        var usuariosB = await repositoryB.ObterTodosAsync();

        Assert.Single(usuariosA);
        Assert.Equal("Usuario A", usuariosA[0].Nome);
        Assert.Empty(usuariosB);
    }
}
