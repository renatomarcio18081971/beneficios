using System.Net;
using Beneficios.Application.Interfaces;
using Beneficios.Domain.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Beneficios.Tests.Api;

public class TenantMiddlewareTests
{
    [Fact]
    public async Task RequisicaoComTenantInexistente_DeveRetornar404()
    {
        var tenantResolverMock = new Mock<ITenantResolver>();
        tenantResolverMock
            .Setup(resolver => resolver.ResolveAsync("desconhecido", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantResolution?)null);

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    "Host=localhost;Port=5432;Database=beneficios_test;Username=postgres;Password=postgres");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        service => service.ServiceType == typeof(ITenantResolver));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddSingleton(tenantResolverMock.Object);
                });
            });

        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant", "desconhecido");

        var response = await client.GetAsync("/api/empresas");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_DeveIgnorarResolucaoDeTenant()
    {
        var tenantResolverMock = new Mock<ITenantResolver>();
        tenantResolverMock
            .Setup(resolver => resolver.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantResolution?)null);

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    "Host=localhost;Port=5432;Database=beneficios_test;Username=postgres;Password=postgres");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        service => service.ServiceType == typeof(ITenantResolver));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddSingleton(tenantResolverMock.Object);
                });
            });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        tenantResolverMock.Verify(
            resolver => resolver.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Swagger_DeveIgnorarResolucaoDeTenant()
    {
        var tenantResolverMock = new Mock<ITenantResolver>();
        tenantResolverMock
            .Setup(resolver => resolver.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TenantResolution?)null);

        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseSetting(
                    "ConnectionStrings:DefaultConnection",
                    "Host=localhost;Port=5432;Database=beneficios_test;Username=postgres;Password=postgres");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        service => service.ServiceType == typeof(ITenantResolver));
                    if (descriptor is not null)
                        services.Remove(descriptor);

                    services.AddSingleton(tenantResolverMock.Object);
                });
            });

        var client = factory.CreateClient();
        var response = await client.GetAsync("/swagger/index.html");

        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
        tenantResolverMock.Verify(
            resolver => resolver.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
