using Beneficios.Domain;
using Beneficios.Domain.Interfaces;

namespace Beneficios.Infrastructure.Tenancy;

public sealed class TenantSchemaAccessor : ITenantSchemaAccessor
{
    public TenantSchemaAccessor(string schema)
    {
        Schema = string.IsNullOrWhiteSpace(schema)
            ? TenantSchemaNames.CatalogSchema
            : schema;

        EhCatalogo = string.Equals(Schema, TenantSchemaNames.CatalogSchema, StringComparison.OrdinalIgnoreCase);
    }

    public string Schema { get; }
    public bool EhCatalogo { get; }

    public static TenantSchemaAccessor ParaTenant(string schema) => new(schema);

    public static TenantSchemaAccessor ParaCatalogo() => new(TenantSchemaNames.CatalogSchema);
}
