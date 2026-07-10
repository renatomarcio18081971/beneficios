using Beneficios.Application.Interfaces;
using Beneficios.Application.Services;
using Beneficios.Domain;

namespace Beneficios.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantResolver tenantResolver)
    {
        if (ShouldBypass(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var tenant = context.Request.Headers["X-Tenant"].FirstOrDefault()
            ?? ExtractSubdomain(context.Request.Host.Host);

        var resolution = await tenantResolver.ResolveAsync(tenant);
        if (resolution is null)
        {
            var normalizedTenant = TenantResolver.NormalizeTenant(tenant);
            _logger.LogWarning("Tenant não encontrado: {Tenant}", normalizedTenant);
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { message = "Tenant não encontrado" });
            return;
        }

        context.Items[TenantContextKeys.ConnectionString] = resolution.ConnectionString;
        context.Items[TenantContextKeys.Subdomain] = resolution.Subdomain;
        context.Items[TenantContextKeys.Schema] = resolution.Schema;

        _logger.LogDebug(
            "Tenant resolvido: subdomain={Subdomain}, schema={Schema}, path={Path}",
            resolution.Subdomain,
            resolution.Schema,
            context.Request.Path);

        await _next(context);
    }

    private static bool ShouldBypass(PathString path)
    {
        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "admin";

        var parts = host.Split('.');
        return parts.Length < 2 ? "admin" : parts[0];
    }
}
