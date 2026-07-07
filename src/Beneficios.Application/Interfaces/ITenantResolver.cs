namespace Beneficios.Application.Interfaces;

public interface ITenantResolver
{
    Task<string?> ResolveConnectionStringAsync(string tenantSubdomain, CancellationToken cancellationToken = default);
}
