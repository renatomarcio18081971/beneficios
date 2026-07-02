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
}
