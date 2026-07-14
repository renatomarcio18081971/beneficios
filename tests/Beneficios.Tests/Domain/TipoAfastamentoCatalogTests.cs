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
}
