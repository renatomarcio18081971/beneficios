using Beneficios.Domain.Entities;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class UsuarioTests
{
    [Fact]
    public void Usuario_DeveSerCriadoComPropriedadesCorretas()
    {
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = "João Silva",
            Email = "joao@example.com",
            Senha = "senha123",
            EmpresaId = Guid.NewGuid(),
            DataInclusao = DateTime.UtcNow
        };

        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("João Silva", usuario.Nome);
        Assert.Equal("joao@example.com", usuario.Email);
        Assert.NotEqual(Guid.Empty, usuario.EmpresaId);
    }

    [Fact]
    public void Usuario_EmailDeveSerValido()
    {
        var usuario = new Usuario
        {
            Email = "teste@example.com"
        };

        Assert.Contains("@", usuario.Email);
    }

    [Fact]
    public void Usuario_DataInclusaoDeveSerPreenchida()
    {
        var usuario = new Usuario
        {
            DataInclusao = DateTime.UtcNow
        };

        Assert.True(usuario.DataInclusao <= DateTime.UtcNow);
    }

    [Fact]
    public void Usuario_DevePermitirTokenEPropriedadesDeAuditoria()
    {
        var usuarioAlteracaoId = Guid.NewGuid();
        var dataAlteracao = DateTime.UtcNow;
        var empresa = new Empresa { Id = Guid.NewGuid(), RazaoSocial = "Empresa" };

        var usuario = new Usuario
        {
            Token = "jwt-token",
            DataAlteracao = dataAlteracao,
            UsuarioAlteracaoId = usuarioAlteracaoId,
            Empresa = empresa
        };

        Assert.Equal("jwt-token", usuario.Token);
        Assert.Equal(dataAlteracao, usuario.DataAlteracao);
        Assert.Equal(usuarioAlteracaoId, usuario.UsuarioAlteracaoId);
        Assert.Same(empresa, usuario.Empresa);
    }

    [Fact]
    public void Usuario_DeveTerPropriedadePerfil()
    {
        var usuario = new Usuario { Perfil = UsuarioPerfil.Admin };
        Assert.Equal(UsuarioPerfil.Admin, usuario.Perfil);
    }

    [Fact]
    public void Usuario_PropriedadesPadraoDevemSerInicializadas()
    {
        var usuario = new Usuario();

        Assert.Equal(string.Empty, usuario.Nome);
        Assert.Equal(string.Empty, usuario.Senha);
        Assert.Equal(string.Empty, usuario.Email);
        Assert.Equal(string.Empty, usuario.Token);
        Assert.Equal(UsuarioPerfil.Empresa, usuario.Perfil);
        Assert.Null(usuario.Empresa);
        Assert.Null(usuario.DataAlteracao);
        Assert.Null(usuario.UsuarioAlteracaoId);
    }
}
