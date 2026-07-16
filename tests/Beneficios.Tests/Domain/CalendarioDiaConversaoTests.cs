using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class CalendarioDiaConversaoTests
{
    [Theory]
    [InlineData(OrigemCalendarioDia.Geracao, "geracao")]
    [InlineData(OrigemCalendarioDia.Nacional, "nacional")]
    [InlineData(OrigemCalendarioDia.Manual, "manual")]
    public void Origem_RoundTrip(OrigemCalendarioDia origem, string banco)
    {
        Assert.Equal(banco, CalendarioDiaConversao.OrigemParaBanco(origem));
        Assert.Equal(origem, CalendarioDiaConversao.OrigemDeBanco(banco));
        Assert.Equal(origem, CalendarioDiaConversao.OrigemDeBanco(banco.ToUpperInvariant()));
    }

    [Fact]
    public void OrigemDeBanco_Invalido_DeveLancar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CalendarioDiaConversao.OrigemDeBanco("x"));
    }

    [Theory]
    [InlineData(TipoExcecaoCalendario.Nacional, "nacional")]
    [InlineData(TipoExcecaoCalendario.Estadual, "estadual")]
    [InlineData(TipoExcecaoCalendario.Municipal, "municipal")]
    [InlineData(TipoExcecaoCalendario.FeriasColetivas, "ferias_coletivas")]
    [InlineData(TipoExcecaoCalendario.PontoFacultativo, "ponto_facultativo")]
    public void TipoExcecao_RoundTrip(TipoExcecaoCalendario tipo, string banco)
    {
        Assert.Equal(banco, CalendarioDiaConversao.TipoExcecaoParaBanco(tipo));
        Assert.Equal(tipo, CalendarioDiaConversao.TipoExcecaoDeBanco(banco));
    }

    [Fact]
    public void TipoExcecao_Nulo_DeveRetornarNulo()
    {
        Assert.Null(CalendarioDiaConversao.TipoExcecaoParaBanco(null));
        Assert.Null(CalendarioDiaConversao.TipoExcecaoDeBanco(null));
        Assert.Null(CalendarioDiaConversao.TipoExcecaoDeBanco("  "));
    }

    [Fact]
    public void TipoExcecaoDeBanco_Invalido_DeveLancar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CalendarioDiaConversao.TipoExcecaoDeBanco("x"));
    }
}
