namespace Beneficios.Domain.Interfaces;

public interface ITenantSchemaAccessor
{
    string Schema { get; }
    bool EhCatalogo { get; }
}
