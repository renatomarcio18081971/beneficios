using Beneficios.Application.Interfaces;
using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Beneficios.Application.Services;

public class TenantResolver : ITenantResolver
{
    private readonly ITenantCatalogRepository _tenantCatalogRepository;
    private readonly string _catalogConnectionString;

    public TenantResolver(ITenantCatalogRepository tenantCatalogRepository, IConfiguration configuration)
    {
        _tenantCatalogRepository = tenantCatalogRepository;
        _catalogConnectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");
    }

    public async Task<TenantResolution?> ResolveAsync(
        string tenantSubdomain,
        CancellationToken cancellationToken = default)
    {
        var tenant = NormalizeTenant(tenantSubdomain);

        if (string.Equals(tenant, "admin", StringComparison.OrdinalIgnoreCase))
        {
            var catalogBuilder = new NpgsqlConnectionStringBuilder(_catalogConnectionString)
            {
                SearchPath = TenantSchemaNames.ObterSearchPathCatalogo(),
            };

            return new TenantResolution(
                catalogBuilder.ConnectionString,
                TenantSchemaNames.CatalogSchema,
                tenant);
        }

        var razaoSocial = await _tenantCatalogRepository.ObterRazaoSocialPorDominioAsync(tenant, cancellationToken);
        if (razaoSocial is null)
            return null;

        var schema = TenantSchemaNames.FromRazaoSocial(razaoSocial);
        var connectionString = BuildTenantConnectionString(razaoSocial);

        return new TenantResolution(connectionString, schema, tenant);
    }

    public static string NormalizeTenant(string? tenantSubdomain)
    {
        if (string.IsNullOrWhiteSpace(tenantSubdomain))
            return "admin";

        return tenantSubdomain.Trim().ToLowerInvariant();
    }

    public string BuildTenantConnectionString(string razaoSocial)
    {
        var builder = new NpgsqlConnectionStringBuilder(_catalogConnectionString)
        {
            SearchPath = TenantSchemaNames.ObterSearchPathTenant(razaoSocial),
        };

        return builder.ConnectionString;
    }
}
