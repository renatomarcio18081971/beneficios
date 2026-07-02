using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

public class UsuarioRepositoryTests(PostgresFixture fixture) : IClassFixture<PostgresFixture>
{
    [SkippableFact]
    public async Task CreateAsync_DeveInserirUsuarioNoBanco()
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");

        var empresaId = await SeedEmpresaAsync();
        var repository = new UsuarioRepository(fixture.Connection!);
        var usuarioId = Guid.NewGuid();

        var result = await repository.CreateAsync(new UsuarioCreateParams(
            usuarioId, "João Silva", Criptografia.Encrypt("senha123"), "joao@example.com", empresaId));

        Assert.Equal(usuarioId, result);

        var usuario = await repository.GetByIdAsync(usuarioId);
        Assert.NotNull(usuario);
        Assert.Equal("João Silva", usuario.Nome);
        Assert.Equal("joao@example.com", usuario.Email);
    }

    [SkippableFact]
    public async Task GetByEmailAsync_DeveRetornarUsuarioParaLogin()
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");

        var empresaId = await SeedEmpresaAsync();
        var repository = new UsuarioRepository(fixture.Connection!);
        var usuarioId = Guid.NewGuid();
        var senhaCriptografada = Criptografia.Encrypt("admin123");

        await repository.CreateAsync(new UsuarioCreateParams(
            usuarioId, "Admin", senhaCriptografada, "admin@example.com", empresaId));

        var usuario = await repository.GetByEmailAsync("admin@example.com");

        Assert.NotNull(usuario);
        Assert.Equal(usuarioId, usuario.Id);
        Assert.Equal(senhaCriptografada, usuario.Senha);
    }

    [SkippableFact]
    public async Task UpdateTokenAsync_DevePersistirTokenGeradoNoLogin()
    {
        Skip.If(!fixture.Disponivel, "PostgreSQL indisponivel para testes de repositorio.");

        var empresaId = await SeedEmpresaAsync();
        var repository = new UsuarioRepository(fixture.Connection!);
        var usuarioId = Guid.NewGuid();

        await repository.CreateAsync(new UsuarioCreateParams(
            usuarioId, "Admin", Criptografia.Encrypt("admin123"), "admin@example.com", empresaId));

        var token = "jwt-token-exemplo";
        var updated = await repository.UpdateTokenAsync(usuarioId, token);

        Assert.True(updated);

        var usuario = await repository.GetByEmailAsync("admin@example.com");
        Assert.NotNull(usuario);
        Assert.Equal(token, usuario.Token);
    }

    private async Task<Guid> SeedEmpresaAsync()
    {
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        await empresaRepository.CreateAsync(new EmpresaCreateParams(
            empresaId, "Empresa Teste", "teste", "db", "user", "pass"));

        return empresaId;
    }
}
