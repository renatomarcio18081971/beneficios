using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Configurations;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Beneficios.Infrastructure.Tenancy;

public class TenantProvisioner : ITenantProvisioner
{
    private readonly string _catalogConnectionString;

    public TenantProvisioner(IConfiguration configuration)
    {
        _catalogConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    public async Task ProvisionAsync(
        string razaoSocial,
        Guid empresaId,
        CancellationToken cancellationToken = default)
    {
        var schemaName = TenantSchemaSql.BuildSchemaName(razaoSocial);
        var perfilId = Guid.NewGuid();
        var agora = DateTime.UtcNow;

        using IDbConnection connection = DatabaseConfiguration.CreateConnection(_catalogConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CreateSchema(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CreateUsuariosTable(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CreatePerfisTable(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.CreatePerfilPermissoesTable(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.AlterUsuariosAddPerfilId(schemaName),
            cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.InsertPerfilDono(schemaName),
            new
            {
                Id = perfilId,
                Nome = "Dono",
                DataInclusao = agora,
            },
            cancellationToken: cancellationToken));

        foreach (var modulo in ModulosSistemaCatalog.Todos)
        {
            // Dono: todas as flags true em cada módulo do catálogo
            await connection.ExecuteAsync(new CommandDefinition(
                TenantSchemaSql.InsertPerfilPermissao(schemaName),
                new
                {
                    Id = Guid.NewGuid(),
                    PerfilId = perfilId,
                    CodigoMenu = modulo.Codigo,
                    Visualizar = true,
                    Criar = true,
                    Editar = true,
                    Excluir = true,
                },
                cancellationToken: cancellationToken));
        }

        await connection.ExecuteAsync(new CommandDefinition(
            TenantSchemaSql.InsertDefaultUsuario(schemaName),
            new
            {
                Id = Guid.NewGuid(),
                Nome = TenantDefaultUser.Nome,
                Senha = Criptografia.Encrypt(TenantDefaultUser.Senha),
                Email = TenantDefaultUser.Email,
                EmpresaId = empresaId,
                PerfilId = perfilId,
                DataInclusao = agora,
            },
            cancellationToken: cancellationToken));
    }
}
