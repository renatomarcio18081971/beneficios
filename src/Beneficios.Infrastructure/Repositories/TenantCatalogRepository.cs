using Beneficios.Domain.Interfaces;
using Beneficios.Infrastructure.Configurations;
using Dapper;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Beneficios.Infrastructure.Repositories;

public class TenantCatalogRepository : ITenantCatalogRepository
{
    private readonly string _catalogConnectionString;

    public TenantCatalogRepository(IConfiguration configuration)
    {
        _catalogConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    public async Task<string?> ObterRazaoSocialPorDominioAsync(
        string dominio,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT razao_social
            FROM empresas
            WHERE LOWER(dominio) = LOWER(@Dominio)
            """;

        using IDbConnection connection = DatabaseConfiguration.CreateConnection(_catalogConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();

        return await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { Dominio = dominio }, cancellationToken: cancellationToken));
    }
}
