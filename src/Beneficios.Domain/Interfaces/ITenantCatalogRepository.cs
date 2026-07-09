namespace Beneficios.Domain.Interfaces;

public interface ITenantCatalogRepository
{
    Task<string?> ObterRazaoSocialPorDominioAsync(
        string dominio,
        CancellationToken cancellationToken = default);
}
