using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

public class EmpresaRepositoryTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [SkippableFact]
    public async Task CreateAsync_DeveInserirEmpresaNoBanco()
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");

        var repository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        var result = await repository.CreateAsync(new EmpresaCreateParams(
            empresaId,
            "Empresa Teste LTDA",
            "teste",
            Criptografia.Encrypt("dbname"),
            Criptografia.Encrypt("dbuser"),
            Criptografia.Encrypt("dbpass")));

        Assert.Equal(empresaId, result);

        var empresa = await repository.GetByIdAsync(empresaId);
        Assert.NotNull(empresa);
        Assert.Equal("Empresa Teste LTDA", empresa.RazaoSocial);
        Assert.Equal("teste", empresa.Dominio);
    }

    [SkippableFact]
    public async Task GetAllAsync_DeveRetornarEmpresasOrdenadas()
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");

        var repository = new EmpresaRepository(fixture.Connection!);

        await repository.CreateAsync(new EmpresaCreateParams(
            Guid.NewGuid(), "Beta SA", "beta", "db1", "user1", "pass1"));
        await repository.CreateAsync(new EmpresaCreateParams(
            Guid.NewGuid(), "Alpha LTDA", "alpha", "db2", "user2", "pass2"));

        var empresas = await repository.GetAllAsync();

        Assert.Equal(2, empresas.Length);
        Assert.Equal("Alpha LTDA", empresas[0].RazaoSocial);
        Assert.Equal("Beta SA", empresas[1].RazaoSocial);
    }

    [SkippableFact]
    public async Task DeleteAsync_DeveRemoverEmpresaExistente()
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");

        var repository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        await repository.CreateAsync(new EmpresaCreateParams(
            empresaId, "Empresa Remover", "remover", "db", "user", "pass"));

        var deleted = await repository.DeleteAsync(empresaId);

        Assert.True(deleted);
        Assert.Null(await repository.GetByIdAsync(empresaId));
    }
}
