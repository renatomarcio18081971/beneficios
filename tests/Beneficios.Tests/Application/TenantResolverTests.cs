using Beneficios.Application.Services;
using Beneficios.Domain;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;
using Xunit;

namespace Beneficios.Tests.Application;

public class TenantResolverTests
{
    private const string CatalogConnection =
        "Host=db.local;Port=5433;Database=beneficios_catalog;Username=postgres;Password=senha123;Search Path=beneficios";

    private readonly Mock<ITenantCatalogRepository> _catalogRepositoryMock = new();

    private TenantResolver CreateResolver()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = CatalogConnection,
            })
            .Build();

        return new TenantResolver(_catalogRepositoryMock.Object, configuration);
    }

    [Theory]
    [InlineData(null, "admin")]
    [InlineData("", "admin")]
    [InlineData("  ", "admin")]
    [InlineData("Empresa1", "empresa1")]
    public void NormalizeTenant_DeveNormalizarSubdominio(string? input, string expected)
    {
        Assert.Equal(expected, TenantResolver.NormalizeTenant(input));
    }

    [Theory]
    [InlineData("Empresa Exemplo LTDA", "tenant_empresa_exemplo_ltda")]
    [InlineData("Empresa-1", "tenant_empresa_1")]
    [InlineData("  Exemplo  ", "tenant_exemplo")]
    public void FromRazaoSocial_DeveGerarNomeDeSchema(string razaoSocial, string expectedSchema)
    {
        Assert.Equal(expectedSchema, TenantSchemaNames.FromRazaoSocial(razaoSocial));
    }

    [Fact]
    public async Task ResolveAsync_Admin_DeveRetornarConexaoCatalogo()
    {
        var resolver = CreateResolver();

        var result = await resolver.ResolveAsync("admin");

        Assert.NotNull(result);
        Assert.Equal(CatalogConnection, result!.ConnectionString);
        Assert.Equal(TenantSchemaNames.CatalogSchema, result.Schema);
        Assert.Equal("admin", result.Subdomain);
        _catalogRepositoryMock.Verify(
            repo => repo.ObterRazaoSocialPorDominioAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResolveAsync_TenantInexistente_DeveRetornarNull()
    {
        _catalogRepositoryMock
            .Setup(repo => repo.ObterRazaoSocialPorDominioAsync("empresa1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((string?)null);

        var resolver = CreateResolver();

        var result = await resolver.ResolveAsync("empresa1");

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveAsync_TenantValido_DeveMontarConnectionStringComSearchPathDaRazaoSocial()
    {
        _catalogRepositoryMock
            .Setup(repo => repo.ObterRazaoSocialPorDominioAsync("empresa1", It.IsAny<CancellationToken>()))
            .ReturnsAsync("Empresa Exemplo LTDA");

        var resolver = CreateResolver();

        var result = await resolver.ResolveAsync("empresa1");

        Assert.NotNull(result);
        var builder = new NpgsqlConnectionStringBuilder(result!.ConnectionString);
        Assert.Equal("db.local", builder.Host);
        Assert.Equal(5433, builder.Port);
        Assert.Equal("beneficios_catalog", builder.Database);
        Assert.Equal("postgres", builder.Username);
        Assert.Equal("senha123", builder.Password);
        Assert.Equal("tenant_empresa_exemplo_ltda,beneficios", builder.SearchPath);
        Assert.Equal("tenant_empresa_exemplo_ltda", result.Schema);
        Assert.Equal("empresa1", result.Subdomain);
    }

    [Fact]
    public void BuildTenantConnectionString_DeveDefinirSearchPathDaRazaoSocial()
    {
        var resolver = CreateResolver();

        var result = resolver.BuildTenantConnectionString("Empresa Test");
        var builder = new NpgsqlConnectionStringBuilder(result);

        Assert.Equal("tenant_empresa_test,beneficios", builder.SearchPath);
        Assert.Equal("beneficios_catalog", builder.Database);
    }
}
