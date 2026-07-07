using Beneficios.Domain.Entities;
using Xunit;

namespace Beneficios.Tests.Domain;

public class EmpresaTests
{
    [Fact]
    public void Empresa_DeveSerCriadaComPropriedadesCorretas()
    {
        var empresa = new Empresa
        {
            Id = Guid.NewGuid(),
            RazaoSocial = "Empresa Teste LTDA",
            Dominio = "empresateste",
            DataInclusao = DateTime.UtcNow
        };

        Assert.NotEqual(Guid.Empty, empresa.Id);
        Assert.Equal("Empresa Teste LTDA", empresa.RazaoSocial);
        Assert.Equal("empresateste", empresa.Dominio);
    }

    [Fact]
    public void Empresa_DominioDeveSerPreenchido()
    {
        var empresa = new Empresa
        {
            Dominio = "meudominio"
        };

        Assert.NotNull(empresa.Dominio);
        Assert.NotEmpty(empresa.Dominio);
    }

    [Fact]
    public void Empresa_DataInclusaoDeveSerPreenchida()
    {
        var empresa = new Empresa
        {
            DataInclusao = DateTime.UtcNow
        };

        Assert.True(empresa.DataInclusao <= DateTime.UtcNow);
    }

    [Fact]
    public void Empresa_DevePermitirCredenciaisDeBanco()
    {
        var empresa = new Empresa
        {
            NomeBanco = "beneficios_db",
            UsuarioBanco = "postgres",
            SenhaBanco = "secret"
        };

        Assert.Equal("beneficios_db", empresa.NomeBanco);
        Assert.Equal("postgres", empresa.UsuarioBanco);
        Assert.Equal("secret", empresa.SenhaBanco);
    }

    [Fact]
    public void Empresa_DevePermitirPropriedadesDeAuditoria()
    {
        var usuarioAlteracaoId = Guid.NewGuid();
        var dataAlteracao = DateTime.UtcNow;

        var empresa = new Empresa
        {
            DataAlteracao = dataAlteracao,
            UsuarioAlteracaoId = usuarioAlteracaoId
        };

        Assert.Equal(dataAlteracao, empresa.DataAlteracao);
        Assert.Equal(usuarioAlteracaoId, empresa.UsuarioAlteracaoId);
    }

    [Fact]
    public void Empresa_PropriedadesPadraoDevemSerInicializadas()
    {
        var empresa = new Empresa();

        Assert.Equal(string.Empty, empresa.RazaoSocial);
        Assert.Equal(string.Empty, empresa.Dominio);
        Assert.Equal(string.Empty, empresa.NomeBanco);
        Assert.Equal(string.Empty, empresa.UsuarioBanco);
        Assert.Equal(string.Empty, empresa.SenhaBanco);
        Assert.Null(empresa.DataAlteracao);
        Assert.Null(empresa.UsuarioAlteracaoId);
    }
}
