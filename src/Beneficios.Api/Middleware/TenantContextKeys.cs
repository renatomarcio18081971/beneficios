namespace Beneficios.Api.Middleware;

public static class TenantContextKeys
{
    public const string ConnectionString = "TenantConnectionString";
    public const string Subdomain = "TenantSubdomain";
    public const string Schema = "TenantSchema";
}
