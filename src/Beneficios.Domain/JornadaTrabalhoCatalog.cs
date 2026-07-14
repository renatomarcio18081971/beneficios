using Beneficios.Domain.Enums;

namespace Beneficios.Domain;

public static class JornadaTrabalhoCatalog
{
    public const string CodigoEspecialCategoria = "especial_categoria";

    public static IReadOnlyList<(string Codigo, string Label)> Todas { get; } =
    [
        ("44h_clt", "44h semanais (CLT padrão)"),
        ("40h_seg_sex", "40h semanais (segunda a sexta)"),
        ("36h", "36h semanais (turnos reduzidos)"),
        ("12x36", "12x36 (12h trabalho / 36h descanso)"),
        ("6x1", "6x1 (trabalha 6 dias, folga 1)"),
        ("5x1", "5x1 (trabalha 5 dias, folga 1)"),
        ("5x2", "5x2 (trabalha 5 dias, folga 2)"),
        ("tempo_parcial", "Tempo parcial (até 30h semanais)"),
        (CodigoEspecialCategoria, "Jornada especial por categoria"),
    ];

    public static bool ExigeDetalhe(string codigo) =>
        string.Equals(codigo, CodigoEspecialCategoria, StringComparison.OrdinalIgnoreCase);

    public static bool ExigeDetalhe(JornadaTrabalho jornada) =>
        jornada == JornadaTrabalho.EspecialCategoria;

    public static bool CodigoValido(string codigo) =>
        Todas.Any(x => x.Codigo.Equals(codigo, StringComparison.OrdinalIgnoreCase));
}
