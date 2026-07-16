using Microsoft.AspNetCore.Mvc.Testing;

namespace Beneficios.Tests.Api;

public class CorsConfigurationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorsConfigurationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                "Host=localhost;Port=5432;Database=beneficios_test;Username=postgres;Password=postgres");
        });
    }

    [Fact]
    public async Task Options_DevePermitirOriginAdminLocalhost4200()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/usuarios/login");
        request.Headers.Add("Origin", "http://admin.localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").Single();
        Assert.Equal("http://admin.localhost:4200", allowedOrigin);
    }

    [Fact]
    public async Task Options_DevePermitirOriginTenantLocalhost4200()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var request = new HttpRequestMessage(HttpMethod.Options, "/api/usuarios/login");
        request.Headers.Add("Origin", "http://empresa1.localhost:4200");
        request.Headers.Add("Access-Control-Request-Method", "POST");

        var response = await client.SendAsync(request);

        Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
        var allowedOrigin = response.Headers.GetValues("Access-Control-Allow-Origin").Single();
        Assert.Equal("http://empresa1.localhost:4200", allowedOrigin);
    }
}
