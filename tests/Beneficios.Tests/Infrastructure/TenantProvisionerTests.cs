using Beneficios.Domain.Models;
using Beneficios.Domain;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Beneficios.Infrastructure.Tenancy;
using Dapper;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class TenantProvisionerTests(PostgresFixture fixture)
{
    [SkippableFact]
    public async Task ProvisionAsync_DeveCriarSchemaTabelaEUsuarioPadrao()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = Guid.NewGuid();
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Nova Empresa LTDA",
            Dominio = "novaempresa",
        });

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = fixture.CatalogConnectionString,
            })
            .Build();

        var provisioner = new TenantProvisioner(configuration);
        await provisioner.ProvisionAsync("Nova Empresa LTDA", empresaId);

        var schemaName = TenantSchemaNames.FromRazaoSocial("Nova Empresa LTDA");
        var quotedSchema = TenantSchemaSql.QuoteIdentifier(schemaName);

        var tableExists = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
                SELECT 1
                FROM information_schema.tables
                WHERE table_schema = @SchemaName
                  AND table_name = 'usuarios'
            )
            """,
            new { SchemaName = schemaName });

        Assert.True(tableExists);

        var usuario = await fixture.Connection.QueryFirstOrDefaultAsync<UsuarioProvisionadoResult>(
            $"""
            SELECT nome AS Nome, email AS Email, senha AS Senha, perfil AS Perfil, empresa_id AS EmpresaId
            FROM {quotedSchema}.usuarios
            WHERE email = @Email
            """,
            new { Email = TenantDefaultUser.Email });

        Assert.NotNull(usuario);
        Assert.Equal(TenantDefaultUser.Nome, usuario!.Nome);
        Assert.Equal(TenantDefaultUser.Email, usuario.Email);
        Assert.Equal("Empresa", usuario.Perfil);
        Assert.Equal(empresaId, usuario.EmpresaId);
        Assert.Equal(Criptografia.Encrypt(TenantDefaultUser.Senha), usuario.Senha);

        var perfisTable = await fixture.Connection!.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS(
              SELECT 1 FROM information_schema.tables
              WHERE table_schema = @SchemaName AND table_name = 'perfis')
            """, new { SchemaName = schemaName });
        Assert.True(perfisTable);

        var dono = await fixture.Connection.QueryFirstOrDefaultAsync<DonoProvisionadoResult>(
            $"""
            SELECT id AS Id, nome AS Nome, eh_sistema AS EhSistema
            FROM {quotedSchema}.perfis WHERE eh_sistema = TRUE LIMIT 1
            """);
        Assert.NotNull(dono);
        Assert.Equal("Dono", dono!.Nome);

        var permCount = await fixture.Connection.ExecuteScalarAsync<int>(
            $"""
            SELECT COUNT(*) FROM {quotedSchema}.perfil_permissoes
            WHERE perfil_id = @Id AND visualizar AND criar AND editar AND excluir
            """, new { dono.Id });
        Assert.Equal(ModulosSistemaCatalog.Todos.Count, permCount);

        var perfilIdUsuario = await fixture.Connection.ExecuteScalarAsync<Guid?>(
            $"""
            SELECT perfil_id FROM {quotedSchema}.usuarios WHERE email = @Email
            """, new { Email = TenantDefaultUser.Email });
        Assert.Equal(dono.Id, perfilIdUsuario);
    }

    private sealed class DonoProvisionadoResult
    {
        public Guid Id { get; init; }
        public string Nome { get; init; } = string.Empty;
        public bool EhSistema { get; init; }
    }

    private sealed class UsuarioProvisionadoResult
    {
        public string Nome { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string Senha { get; init; } = string.Empty;
        public string Perfil { get; init; } = string.Empty;
        public Guid EmpresaId { get; init; }
    }
}
