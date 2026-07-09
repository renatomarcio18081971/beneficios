using Beneficios.Domain.Models;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class EmpresaRepositoryTests(PostgresFixture fixture)
{
    [SkippableFact]
    public async Task SalvarAsync_DeveInserirEmpresaNoBanco()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var repository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        var result = await repository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Empresa Teste LTDA",
            Dominio = "teste",
        });

        Assert.Equal(empresaId, result);

        var empresa = await repository.ObterUmAsync(empresaId);
        Assert.NotNull(empresa);
        Assert.Equal("Empresa Teste LTDA", empresa.RazaoSocial);
        Assert.Equal("teste", empresa.Dominio);
    }

    [SkippableFact]
    public async Task ObterTodosAsync_DeveRetornarEmpresasOrdenadas()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var repository = new EmpresaRepository(fixture.Connection!);

        await repository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = Guid.NewGuid(),
            RazaoSocial = "Beta SA",
            Dominio = "beta",
        });
        await repository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = Guid.NewGuid(),
            RazaoSocial = "Alpha LTDA",
            Dominio = "alpha",
        });

        var empresas = await repository.ObterTodosAsync();

        Assert.Equal(2, empresas.Length);
        Assert.Equal("Alpha LTDA", empresas[0].RazaoSocial);
        Assert.Equal("Beta SA", empresas[1].RazaoSocial);
    }

    [SkippableFact]
    public async Task DeleteAsync_DeveRemoverEmpresaExistente()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var repository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        await repository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Empresa Remover",
            Dominio = "remover",
        });

        var deleted = await repository.DeleteAsync(empresaId);

        Assert.True(deleted);
        Assert.Null(await repository.ObterUmAsync(empresaId));
    }

    [SkippableFact]
    public async Task AtualizarAsync_DeveAtualizarEmpresaExistente()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var repository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        await repository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = "Empresa Original",
            Dominio = "original",
        });

        var updated = await repository.AtualizarAsync(new EmpresaAtualizarParams
        {
            Id = empresaId,
            RazaoSocial = "Empresa Atualizada",
            Dominio = "atualizada",
        });

        Assert.True(updated);

        var empresa = await repository.ObterUmAsync(empresaId);
        Assert.NotNull(empresa);
        Assert.Equal("Empresa Atualizada", empresa!.RazaoSocial);
        Assert.Equal("atualizada", empresa.Dominio);
    }
}
