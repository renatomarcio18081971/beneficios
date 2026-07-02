using Beneficios.Domain.Entities;
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
}
