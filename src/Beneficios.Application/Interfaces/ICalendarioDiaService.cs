using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface ICalendarioDiaService
{
    Task GerarAnoAsync(int ano);
    Task<CalendarioDiaDto[]> ObterPorMesAsync(int ano, int mes);
    Task<CalendarioDiaDto?> ObterPorIdAsync(Guid id);
    Task AtualizarAsync(Guid id, CalendarioDiaAtualizarDto dto, Guid? usuarioAlteracaoId);
}
