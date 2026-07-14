using Beneficios.Domain.Models;
using Beneficios.Domain.ValueObjects;
using Beneficios.Infrastructure.Repositories;
using Xunit;

namespace Beneficios.Tests.Infrastructure;

[Collection("Postgres")]
public class UsuarioRepositoryTests(PostgresFixture fixture)
{
    private const string TenantRazaoSocial = "Empresa Teste";

    [SkippableFact]
    public async Task SalvarAsync_DeveInserirUsuarioNoBanco()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();

        var result = await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Joao Silva",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });

        Assert.Equal(usuarioId, result);

        var usuario = await repository.ObterUmAsync(usuarioId);
        Assert.NotNull(usuario);
        Assert.Equal("Joao Silva", usuario.Nome);
        Assert.Equal("joao@example.com", usuario.Email);
    }

    [SkippableFact]
    public async Task GetByEmailAsync_DeveRetornarUsuarioParaLogin()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();
        var senhaCriptografada = Criptografia.Encrypt("admin123");

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Admin",
            Senha = senhaCriptografada,
            Email = "admin@example.com",
            EmpresaId = empresaId
        });

        var usuario = await repository.GetByEmailAsync("admin@example.com");

        Assert.NotNull(usuario);
        Assert.Equal(usuarioId, usuario.Id);
        Assert.Equal(senhaCriptografada, usuario.Senha);
    }

    [SkippableFact]
    public async Task UpdateTokenAsync_DevePersistirTokenGeradoNoLogin()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Admin",
            Senha = Criptografia.Encrypt("admin123"),
            Email = "admin@example.com",
            EmpresaId = empresaId
        });

        var token = "jwt-token-exemplo";
        var updated = await repository.UpdateTokenAsync(usuarioId, token);

        Assert.True(updated);

        var usuario = await repository.GetByEmailAsync("admin@example.com");
        Assert.NotNull(usuario);
        Assert.Equal(token, usuario.Token);
    }

    [SkippableFact]
    public async Task AtualizarAsync_DeveAtualizarUsuarioExistente()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Joao",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });

        var updated = await repository.AtualizarAsync(new UsuarioAtualizarParams
        {
            Id = usuarioId,
            Nome = "Joao Silva",
            Email = "joao.silva@example.com",
            EmpresaId = empresaId
        });

        Assert.True(updated);

        var usuario = await repository.ObterUmAsync(usuarioId);
        Assert.NotNull(usuario);
        Assert.Equal("Joao Silva", usuario!.Nome);
        Assert.Equal("joao.silva@example.com", usuario.Email);
    }

    [SkippableFact]
    public async Task DeleteAsync_DeveRemoverUsuarioExistente()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Remover",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "remover@example.com",
            EmpresaId = empresaId
        });

        var deleted = await repository.DeleteAsync(usuarioId);

        Assert.True(deleted);
        Assert.Null(await repository.ObterUmAsync(usuarioId));
    }

    [SkippableFact]
    public async Task ObterTodosAsync_DeveRetornarUsuariosOrdenados()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Zeca",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "zeca@example.com",
            EmpresaId = empresaId
        });
        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Ana",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "ana@example.com",
            EmpresaId = empresaId
        });

        var usuarios = await repository.ObterTodosAsync();

        Assert.Equal(2, usuarios.Length);
        Assert.Equal("Ana", usuarios[0].Nome);
        Assert.Equal("Zeca", usuarios[1].Nome);
    }

    [SkippableFact]
    public async Task UpdateCodigoAlterarSenhaAsync_DevePersistirCodigo()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Joao",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });

        var updated = await repository.UpdateCodigoAlterarSenhaAsync(usuarioId, "123456");

        Assert.True(updated);

        var usuario = await repository.GetByCodigoAlterarSenhaAsync("123456");
        Assert.NotNull(usuario);
        Assert.Equal(usuarioId, usuario!.Id);
    }

    [SkippableFact]
    public async Task AtualizarSenhaAsync_DeveAtualizarSenhaELimparCodigo()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);
        var usuarioId = Guid.NewGuid();
        var novaSenhaCriptografada = Criptografia.Encrypt("novaSenha456");

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = usuarioId,
            Nome = "Joao",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });
        await repository.UpdateCodigoAlterarSenhaAsync(usuarioId, "654321");

        var updated = await repository.AtualizarSenhaAsync(usuarioId, novaSenhaCriptografada);

        Assert.True(updated);

        var usuario = await repository.GetByEmailAsync("joao@example.com");
        Assert.NotNull(usuario);
        Assert.Equal(novaSenhaCriptografada, usuario!.Senha);
        Assert.Null(await repository.GetByCodigoAlterarSenhaAsync("654321"));
    }

    [SkippableFact]
    public async Task FiltrarAsync_DeveFiltrarPorNome()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Joao Silva",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });
        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Maria Santos",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "maria@example.com",
            EmpresaId = empresaId
        });

        var usuarios = await repository.FiltrarAsync(new UsuarioFiltroParams { Nome = "Joao" });

        Assert.Single(usuarios);
        Assert.Equal("Joao Silva", usuarios[0].Nome);
    }

    [SkippableFact]
    public async Task FiltrarAsync_DeveFiltrarPorEmail()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Joao Silva",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });

        var usuarios = await repository.FiltrarAsync(new UsuarioFiltroParams { Email = "joao@" });

        Assert.Single(usuarios);
        Assert.Equal("joao@example.com", usuarios[0].Email);
    }

    [SkippableFact]
    public async Task FiltrarAsync_DeveRetornarVazioQuandoNenhumResultado()
    {
        await PostgresTestHelper.PrepareAsync(fixture);

        var empresaId = await SeedEmpresaAsync();
        await using var tenantConnection = await fixture.CreateTenantConnectionAsync(TenantRazaoSocial);
        var repository = new UsuarioRepository(tenantConnection);

        await repository.SalvarAsync(new UsuarioSalvarParams
        {
            Id = Guid.NewGuid(),
            Nome = "Joao Silva",
            Senha = Criptografia.Encrypt("senha123"),
            Email = "joao@example.com",
            EmpresaId = empresaId
        });

        var usuarios = await repository.FiltrarAsync(new UsuarioFiltroParams { Nome = "Inexistente" });

        Assert.Empty(usuarios);
    }

    private async Task<Guid> SeedEmpresaAsync()
    {
        var empresaRepository = new EmpresaRepository(fixture.Connection!);
        var empresaId = Guid.NewGuid();

        await empresaRepository.SalvarAsync(new EmpresaSalvarParams
        {
            Id = empresaId,
            RazaoSocial = TenantRazaoSocial,
            Dominio = "teste",
        });

        return empresaId;
    }
}
