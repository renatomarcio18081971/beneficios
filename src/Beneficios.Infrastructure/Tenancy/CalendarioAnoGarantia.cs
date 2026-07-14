using Beneficios.Domain.Interfaces;
using Beneficios.Infrastructure.Configurations;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Beneficios.Infrastructure.Tenancy;

public class CalendarioAnoGarantia : ICalendarioAnoGarantia
{
    private readonly string _catalogConnectionString;

    public CalendarioAnoGarantia(IConfiguration configuration)
    {
        _catalogConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    public async Task GarantirAnoCorrenteEmTenantsExistentesAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = DatabaseConfiguration.CreateConnection(_catalogConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();

        var schemas = (await connection.QueryAsync<string>(new CommandDefinition(
            """
            SELECT schema_name
            FROM information_schema.schemata
            WHERE schema_name LIKE 'tenant_%'
            ORDER BY schema_name
            """,
            cancellationToken: cancellationToken))).ToArray();

        var ano = DateTime.UtcNow.Year;
        foreach (var schemaName in schemas)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                TenantSchemaSql.CriarTabelaCalendarioDias(schemaName),
                cancellationToken: cancellationToken));

            await CalendarioAnoGerador.GerarSeAusenteAsync(connection, schemaName, ano, cancellationToken);
        }
    }
}
