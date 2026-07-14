namespace Beneficios.Domain.Interfaces;

public interface ICalendarioAnoGarantia
{
    Task GarantirAnoCorrenteEmTenantsExistentesAsync(CancellationToken cancellationToken = default);
}
