using Beneficios.Domain.Enums;
using Beneficios.Domain.Models;

namespace Beneficios.Domain;

public static class CalendarioAnoFabrica
{
    public static IReadOnlyList<CalendarioDiaSalvarParams> MontarDiasDoAno(int ano)
    {
        var inicio = new DateOnly(ano, 1, 1);
        var fim = new DateOnly(ano, 12, 31);
        var mapa = new Dictionary<DateOnly, CalendarioDiaSalvarParams>();

        for (var data = inicio; data <= fim; data = data.AddDays(1))
        {
            var ehUtil = data.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday;
            mapa[data] = new CalendarioDiaSalvarParams
            {
                Id = Guid.NewGuid(),
                Data = data,
                EhDiaUtil = ehUtil,
                TipoExcecao = null,
                Origem = OrigemCalendarioDia.Geracao,
                Observacao = null,
            };
        }

        foreach (var feriado in FeriadosNacionaisCatalog.ObterParaAno(ano))
        {
            if (!mapa.TryGetValue(feriado.Data, out var dia))
                continue;

            mapa[feriado.Data] = new CalendarioDiaSalvarParams
            {
                Id = dia.Id,
                Data = dia.Data,
                EhDiaUtil = false,
                TipoExcecao = TipoExcecaoCalendario.Nacional,
                Origem = OrigemCalendarioDia.Nacional,
                Observacao = feriado.Nome,
            };
        }

        return mapa.Values.OrderBy(d => d.Data).ToList();
    }
}
