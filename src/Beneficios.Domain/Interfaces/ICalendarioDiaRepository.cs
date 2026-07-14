using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface ICalendarioDiaRepository
{
    Task<bool> AnoExisteAsync(int ano);
    Task InserirLoteAsync(IReadOnlyList<CalendarioDiaSalvarParams> dias);
    Task<CalendarioDiaQueryResult[]> ObterPorMesAsync(int ano, int mes);
    Task<CalendarioDiaQueryResult?> ObterPorIdAsync(Guid id);
    Task<bool> AtualizarAsync(CalendarioDiaAtualizarParams parametros);
}
