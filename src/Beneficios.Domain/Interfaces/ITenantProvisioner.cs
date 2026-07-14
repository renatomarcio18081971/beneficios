namespace Beneficios.Domain.Interfaces;

public interface ITenantProvisioner
{
    Task ProvisionAsync(
        string razaoSocial,
        Guid empresaId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Garante tabelas de perfil, perfil Dono e vínculo do usuário padrão em todos os schemas tenant_*.
    /// Idempotente — seguro para reexecução.
    /// </summary>
    Task EnsurePerfisEmTenantsExistentesAsync(CancellationToken cancellationToken = default);
}
