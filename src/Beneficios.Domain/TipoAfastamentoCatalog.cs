using Beneficios.Domain.Enums;

namespace Beneficios.Domain;

public static class TipoAfastamentoCatalog
{
    public static IReadOnlyList<(string codigo, string label)> Todos { get; } =
    [
        ("ferias", "Férias"),
        ("licenca_medica", "Licença médica"),
        ("maternidade_paternidade", "Maternidade/paternidade"),
        ("acidente", "Acidente"),
        ("suspensao", "Suspensão"),
        ("outros", "Outros"),
    ];

    public static string ParaBanco(TipoAfastamento tipo) => tipo switch
    {
        TipoAfastamento.Ferias => "ferias",
        TipoAfastamento.LicencaMedica => "licenca_medica",
        TipoAfastamento.MaternidadePaternidade => "maternidade_paternidade",
        TipoAfastamento.Acidente => "acidente",
        TipoAfastamento.Suspensao => "suspensao",
        TipoAfastamento.Outros => "outros",
        _ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, null),
    };

    public static TipoAfastamento DeBanco(string codigo) => codigo.ToLowerInvariant() switch
    {
        "ferias" => TipoAfastamento.Ferias,
        "licenca_medica" => TipoAfastamento.LicencaMedica,
        "maternidade_paternidade" => TipoAfastamento.MaternidadePaternidade,
        "acidente" => TipoAfastamento.Acidente,
        "suspensao" => TipoAfastamento.Suspensao,
        "outros" => TipoAfastamento.Outros,
        _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, null),
    };

    public static string Label(TipoAfastamento tipo)
    {
        var codigo = ParaBanco(tipo);
        return Todos.First(x => x.codigo == codigo).label;
    }
}