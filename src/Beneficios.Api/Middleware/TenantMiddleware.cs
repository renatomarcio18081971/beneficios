using Beneficios.Application.Services;
using Beneficios.Application.Interfaces;
using Beneficios.Api.Middleware;

namespace Beneficios.Api.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
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

        var connectionString = await tenantResolver.ResolveConnectionStringAsync(tenant);
        if (connectionString is null)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new { message = "Tenant não encontrado" });
            return;
        }

        context.Items[TenantContextKeys.ConnectionString] = connectionString;
        context.Items[TenantContextKeys.Subdomain] = TenantResolver.NormalizeTenant(tenant);

        await _next(context);
    }

    private static bool ShouldBypass(PathString path)
    {
        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractSubdomain(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
            return "admin";

        var parts = host.Split('.');
        return parts.Length < 2 ? "admin" : parts[0];
    }
}
