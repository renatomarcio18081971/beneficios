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
            TenantSchemaSql.InsertDefaultUsuario(schemaName),
            new
            {
                Id = Guid.NewGuid(),
                Nome = TenantDefaultUser.Nome,
                Senha = Criptografia.Encrypt(TenantDefaultUser.Senha),
                Email = TenantDefaultUser.Email,
                EmpresaId = empresaId,
                DataInclusao = DateTime.UtcNow,
            },
            cancellationToken: cancellationToken));
    }
}
