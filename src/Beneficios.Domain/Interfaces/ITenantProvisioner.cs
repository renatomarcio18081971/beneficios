namespace Beneficios.Domain.Interfaces;

public interface ITenantProvisioner
{
    Task ProvisionAsync(
        string razaoSocial,
        Guid empresaId,
        CancellationToken cancellationToken = default);
}
