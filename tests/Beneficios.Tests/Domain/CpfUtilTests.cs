using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class CpfUtilTests
{
    [Fact]
    public void EhValido_CpfValido_DeveRetornarTrue()
        => Assert.True(CpfUtil.EhValido("52998224725"));

    [Fact]
    public void EhValido_CpfInvalido_DeveRetornarFalse()
        => Assert.False(CpfUtil.EhValido("11111111111"));

    [Fact]
    public void Normalizar_RemoveMascara()
        => Assert.Equal("52998224725", CpfUtil.Normalizar("529.982.247-25"));
}

public class JornadaTrabalhoCatalogTests
{
    [Fact]
    public void ExigeDetalhe_EspecialCategoria_DeveSerTrue()
    {
        Assert.True(JornadaTrabalhoCatalog.ExigeDetalhe("especial_categoria"));
        Assert.True(JornadaTrabalhoCatalog.ExigeDetalhe(JornadaTrabalho.EspecialCategoria));
    }

    [Fact]
    public void ExigeDetalhe_Outras_DeveSerFalse()
        => Assert.False(JornadaTrabalhoCatalog.ExigeDetalhe("44h_clt"));

    [Fact]
    public void Todas_DeveTerNoveItensComLabels()
    {
        Assert.Equal(9, JornadaTrabalhoCatalog.Todas.Count);
        Assert.All(JornadaTrabalhoCatalog.Todas, x => Assert.False(string.IsNullOrWhiteSpace(x.Label)));
    }

    [Fact]
    public void Conversao_RoundTrip_Jornadas()
    {
        foreach (JornadaTrabalho j in Enum.GetValues<JornadaTrabalho>())
        {
            var banco = FuncionarioConversao.JornadaParaBanco(j);
            Assert.Equal(j, FuncionarioConversao.JornadaDeBanco(banco));
        }
    }
}
