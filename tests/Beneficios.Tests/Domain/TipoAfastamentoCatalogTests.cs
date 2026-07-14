using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class TipoAfastamentoCatalogTests
{
    [Fact]
    public void Catalog_Ferias_CodigoELabel()
    {
        Assert.Equal("ferias", TipoAfastamentoCatalog.ParaBanco(TipoAfastamento.Ferias));
        Assert.Equal("Férias", TipoAfastamentoCatalog.Label(TipoAfastamento.Ferias));
    }

    [Theory]
    [InlineData(TipoAfastamento.LicencaMedica, "licenca_medica", "Licença médica")]
    [InlineData(TipoAfastamento.MaternidadePaternidade, "maternidade_paternidade", "Maternidade/paternidade")]
    [InlineData(TipoAfastamento.Acidente, "acidente", "Acidente")]
    [InlineData(TipoAfastamento.Suspensao, "suspensao", "Suspensão")]
    [InlineData(TipoAfastamento.Outros, "outros", "Outros")]
    public void RoundTrip_DeBancoEParaBanco(TipoAfastamento tipo, string codigo, string label)
    {
        Assert.Equal(codigo, TipoAfastamentoCatalog.ParaBanco(tipo));
        Assert.Equal(tipo, TipoAfastamentoCatalog.DeBanco(codigo));
        Assert.Equal(label, TipoAfastamentoCatalog.Label(tipo));
    }

    [Fact]
    public void Todos_DeveTerSeisTipos()
    {
        Assert.Equal(6, TipoAfastamentoCatalog.Todos.Count);
    }
}