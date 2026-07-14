namespace Beneficios.Domain;

public sealed record FeriadoNacional(DateOnly Data, string Nome);

public static class FeriadosNacionaisCatalog
{
    public static IReadOnlyList<FeriadoNacional> ObterParaAno(int ano)
    {
        var pascoa = CalcularPascoa(ano);
        var lista = new List<FeriadoNacional>
        {
            new(new DateOnly(ano, 1, 1), "Confraternização Universal"),
            new(pascoa.AddDays(-47), "Carnaval"),
            new(pascoa.AddDays(-2), "Sexta-feira Santa"),
            new(new DateOnly(ano, 4, 21), "Tiradentes"),
            new(new DateOnly(ano, 5, 1), "Dia do Trabalho"),
            new(pascoa.AddDays(60), "Corpus Christi"),
            new(new DateOnly(ano, 9, 7), "Independência do Brasil"),
            new(new DateOnly(ano, 10, 12), "Nossa Senhora Aparecida"),
            new(new DateOnly(ano, 11, 2), "Finados"),
            new(new DateOnly(ano, 11, 15), "Proclamação da República"),
            new(new DateOnly(ano, 11, 20), "Dia da Consciência Negra"),
            new(new DateOnly(ano, 12, 25), "Natal"),
        };

        return lista.OrderBy(f => f.Data).ToArray();
    }

    /// <summary>
    /// Algoritmo de Meeus/Jones/Butcher para a data da Páscoa no calendário gregoriano.
    /// </summary>
    public static DateOnly CalcularPascoa(int ano)
    {
        var a = ano % 19;
        var b = ano / 100;
        var c = ano % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var mes = (h + l - 7 * m + 114) / 31;
        var dia = ((h + l - 7 * m + 114) % 31) + 1;
        return new DateOnly(ano, mes, dia);
    }
}
