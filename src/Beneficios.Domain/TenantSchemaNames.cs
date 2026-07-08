using System.Text;

namespace Beneficios.Domain;

public static class TenantSchemaNames
{
    public const string CatalogSchema = "beneficios";
    private const string TenantPrefix = "tenant_";

    public static string FromRazaoSocial(string razaoSocial)
    {
        var normalized = razaoSocial.Trim().ToLowerInvariant();
        var sanitized = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            sanitized.Append(char.IsAsciiLetterOrDigit(character) ? character : '_');
        }

        return TenantPrefix + sanitized;
    }

    public static string GetCatalogSearchPath() => CatalogSchema;

    public static string GetTenantSearchPath(string razaoSocial) =>
        $"{FromRazaoSocial(razaoSocial)},{CatalogSchema}";
}
