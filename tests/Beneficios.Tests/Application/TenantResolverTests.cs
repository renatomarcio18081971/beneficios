using Beneficios.Application.Services;
using Beneficios.Domain.Interfaces;
using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Moq;
using Npgsql;

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

    [Fact]
    public async Task ResolveConnectionStringAsync_Admin_DeveRetornarConexaoCatalogo()
    {
        var resolver = CreateResolver();

        var result = await resolver.ResolveConnectionStringAsync("admin");

        Assert.Equal(CatalogConnection, result);
        _catalogRepositoryMock.Verify(
            repo => repo.ObterPorDominioAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResolveConnectionStringAsync_TenantInexistente_DeveRetornarNull()
    {
        _catalogRepositoryMock
            .Setup(repo => repo.ObterPorDominioAsync("empresa1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((EmpresaTenantInfo?)null);

        var resolver = CreateResolver();

        var result = await resolver.ResolveConnectionStringAsync("empresa1");

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveConnectionStringAsync_TenantValido_DeveMontarConnectionString()
    {
        _catalogRepositoryMock
            .Setup(repo => repo.ObterPorDominioAsync("empresa1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmpresaTenantInfo
            {
                NomeBanco = Criptografia.Encrypt("beneficios_tenant"),
                UsuarioBanco = Criptografia.Encrypt("tenant_user"),
                SenhaBanco = Criptografia.Encrypt("tenant_pass"),
            });

        var resolver = CreateResolver();

        var result = await resolver.ResolveConnectionStringAsync("empresa1");

        Assert.NotNull(result);
        var builder = new NpgsqlConnectionStringBuilder(result);
        Assert.Equal("db.local", builder.Host);
        Assert.Equal(5433, builder.Port);
        Assert.Equal("beneficios_tenant", builder.Database);
        Assert.Equal("tenant_user", builder.Username);
        Assert.Equal("tenant_pass", builder.Password);
        Assert.Equal("beneficios", builder.SearchPath);
    }

    [Fact]
    public void BuildTenantConnectionString_DeveDescriptografarCredenciaisDaEmpresa()
    {
        var resolver = CreateResolver();
        var empresa = new EmpresaTenantInfo
        {
            NomeBanco = Criptografia.Encrypt("db_tenant"),
            UsuarioBanco = Criptografia.Encrypt("user_tenant"),
            SenhaBanco = Criptografia.Encrypt("pass_tenant"),
        };

        var result = resolver.BuildTenantConnectionString(empresa);
        var builder = new NpgsqlConnectionStringBuilder(result);

        Assert.Equal("db_tenant", builder.Database);
        Assert.Equal("user_tenant", builder.Username);
        Assert.Equal("pass_tenant", builder.Password);
    }
}
