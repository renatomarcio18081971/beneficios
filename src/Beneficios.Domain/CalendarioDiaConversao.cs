using Beneficios.Domain.Enums;

namespace Beneficios.Domain;

public static class CalendarioDiaConversao
{
    public static string OrigemParaBanco(OrigemCalendarioDia origem) => origem switch
    {
        OrigemCalendarioDia.Geracao => "geracao",
        OrigemCalendarioDia.Nacional => "nacional",
        OrigemCalendarioDia.Manual => "manual",
        _ => throw new ArgumentOutOfRangeException(nameof(origem), origem, null),
    };

    public static OrigemCalendarioDia OrigemDeBanco(string valor) => valor.ToLowerInvariant() switch
    {
        "geracao" => OrigemCalendarioDia.Geracao,
        "nacional" => OrigemCalendarioDia.Nacional,
        "manual" => OrigemCalendarioDia.Manual,
        _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null),
    };

    public static string? TipoExcecaoParaBanco(TipoExcecaoCalendario? tipo) => tipo switch
    {
        null => null,
        TipoExcecaoCalendario.Nacional => "nacional",
        TipoExcecaoCalendario.Estadual => "estadual",
        TipoExcecaoCalendario.Municipal => "municipal",
        TipoExcecaoCalendario.FeriasColetivas => "ferias_coletivas",
        TipoExcecaoCalendario.PontoFacultativo => "ponto_facultativo",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };

    public static TipoExcecaoCalendario? TipoExcecaoDeBanco(string? valor) =>
        string.IsNullOrWhiteSpace(valor)
            ? null
            : valor.ToLowerInvariant() switch
            {
                "nacional" => TipoExcecaoCalendario.Nacional,
                "estadual" => TipoExcecaoCalendario.Estadual,
                "municipal" => TipoExcecaoCalendario.Municipal,
                "ferias_coletivas" => TipoExcecaoCalendario.FeriasColetivas,
                "ponto_facultativo" => TipoExcecaoCalendario.PontoFacultativo,
                _ => throw new ArgumentOutOfRangeException(nameof(valor), valor, null),
            };
}
