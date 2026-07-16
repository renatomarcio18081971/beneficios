using Beneficios.Domain.Enums;

namespace Beneficios.Domain;

public static class FuncionarioConversao
{
    public static string JornadaParaBanco(JornadaTrabalho jornada) => jornada switch
    {
        JornadaTrabalho.QuarentaQuatroHorasClt => "44h_clt",
        JornadaTrabalho.QuarentaHorasSegSex => "40h_seg_sex",
        JornadaTrabalho.TrintaSeisHoras => "36h",
        JornadaTrabalho.DozePorTrintaSeis => "12x36",
        JornadaTrabalho.SeisPorUm => "6x1",
        JornadaTrabalho.CincoPorUm => "5x1",
        JornadaTrabalho.CincoPorDois => "5x2",
        JornadaTrabalho.TempoParcial => "tempo_parcial",
        JornadaTrabalho.EspecialCategoria => "especial_categoria",
        _ => throw new ArgumentOutOfRangeException(nameof(jornada), jornada, null),
    };

    public static JornadaTrabalho JornadaDeBanco(string valor) => valor.ToLowerInvariant() switch
    {
        "44h_clt" => JornadaTrabalho.QuarentaQuatroHorasClt,
        "40h_seg_sex" => JornadaTrabalho.QuarentaHorasSegSex,
        "36h" => JornadaTrabalho.TrintaSeisHoras,
        "12x36" => JornadaTrabalho.DozePorTrintaSeis,
        "6x1" => JornadaTrabalho.SeisPorUm,
        "5x1" => JornadaTrabalho.CincoPorUm,
        "5x2" => JornadaTrabalho.CincoPorDois,
        "tempo_parcial" => JornadaTrabalho.TempoParcial,
        "especial_categoria" => JornadaTrabalho.EspecialCategoria,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null),
    };

    public static string TipoContratoParaBanco(TipoContrato tipo) => tipo switch
    {
        TipoContrato.Clt => "clt",
        TipoContrato.Estagio => "estagio",
        TipoContrato.Terceirizado => "terceirizado",
        TipoContrato.Pj => "pj",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };

    public static TipoContrato TipoContratoDeBanco(string valor) => valor.ToLowerInvariant() switch
    {
        "clt" => TipoContrato.Clt,
        "estagio" => TipoContrato.Estagio,
        "terceirizado" => TipoContrato.Terceirizado,
        "pj" => TipoContrato.Pj,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null),
    };

    public static string SituacaoParaBanco(SituacaoFuncionario situacao) => situacao switch
    {
        SituacaoFuncionario.Ativo => "ativo",
        SituacaoFuncionario.Afastado => "afastado",
        SituacaoFuncionario.Desligado => "desligado",
        _ => throw new ArgumentOutOfRangeException(nameof(situacao), situacao, null),
    };

    public static SituacaoFuncionario SituacaoDeBanco(string valor) => valor.ToLowerInvariant() switch
    {
        "ativo" => SituacaoFuncionario.Ativo,
        "afastado" => SituacaoFuncionario.Afastado,
        "desligado" => SituacaoFuncionario.Desligado,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null),
    };

    public static string? SomenteDigitosOuNulo(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;
        var digitos = new string(valor.Where(char.IsDigit).ToArray());
        return digitos.Length == 0 ? null : digitos;
    }
}
