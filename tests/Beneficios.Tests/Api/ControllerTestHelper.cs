using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Beneficios.Tests.Api;

internal static class ControllerTestHelper
{
    public static void SetAuthenticatedUser(ControllerBase controller, Guid? userId = null)
    {
        var id = userId ?? Guid.NewGuid();
        var claims = new[] { new Claim(JwtRegisteredClaimNames.Sub, id.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    public static void SetRequestHost(ControllerBase controller, string host, string? tenantHeader = null)
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.HttpContext.Request.Host = new HostString(host);
        if (tenantHeader != null)
            controller.HttpContext.Request.Headers["X-Tenant"] = tenantHeader;
    }
}
