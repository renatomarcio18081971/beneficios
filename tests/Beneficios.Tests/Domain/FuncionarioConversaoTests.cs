using Beneficios.Domain;
using Beneficios.Domain.Enums;
using Xunit;

namespace Beneficios.Tests.Domain;

public class FuncionarioConversaoTests
{
    [Theory]
    [InlineData(SituacaoFuncionario.Ativo, "ativo")]
    [InlineData(SituacaoFuncionario.Afastado, "afastado")]
    [InlineData(SituacaoFuncionario.Desligado, "desligado")]
    public void Situacao_RoundTrip(SituacaoFuncionario situacao, string banco)
    {
        Assert.Equal(banco, FuncionarioConversao.SituacaoParaBanco(situacao));
        Assert.Equal(situacao, FuncionarioConversao.SituacaoDeBanco(banco));
        Assert.Equal(situacao, FuncionarioConversao.SituacaoDeBanco(banco.ToUpperInvariant()));
    }

    [Fact]
    public void SituacaoDeBanco_Invalido_DeveLancar()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FuncionarioConversao.SituacaoDeBanco("invalido"));
    }

    [Theory]
    [InlineData(TipoContrato.Clt, "clt")]
    [InlineData(TipoContrato.Estagio, "estagio")]
    [InlineData(TipoContrato.Terceirizado, "terceirizado")]
    [InlineData(TipoContrato.Pj, "pj")]
    public void TipoContrato_RoundTrip(TipoContrato tipo, string banco)
    {
        Assert.Equal(banco, FuncionarioConversao.TipoContratoParaBanco(tipo));
        Assert.Equal(tipo, FuncionarioConversao.TipoContratoDeBanco(banco));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("abc", null)]
    [InlineData("13.456-789", "13456789")]
    [InlineData("CEP 12345-678", "12345678")]
    public void SomenteDigitosOuNulo(string? entrada, string? esperado)
    {
        Assert.Equal(esperado, FuncionarioConversao.SomenteDigitosOuNulo(entrada));
    }
}
