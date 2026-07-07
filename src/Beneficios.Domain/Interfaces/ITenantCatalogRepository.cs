using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface ITenantCatalogRepository
{
    Task<EmpresaTenantInfo?> ObterPorDominioAsync(string dominio, CancellationToken cancellationToken = default);
}
