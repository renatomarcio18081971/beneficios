using Beneficios.Domain.Models;

namespace Beneficios.Application.Interfaces;

public interface ITenantResolver
{
    Task<TenantResolution?> ResolveAsync(
        string tenantSubdomain,
        CancellationToken cancellationToken = default);
}
