namespace Beneficios.Domain.Models;

public record TenantResolution(
    string ConnectionString,
    string Schema,
    string Subdomain);
