using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface ILinhaOnibusService
{
    Task<Guid> SalvarAsync(LinhaOnibusSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, LinhaOnibusAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<LinhaOnibusDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<LinhaOnibusDto>> FiltrarAsync(string? descricao, bool? somenteVigentes, DateOnly? referencia = null);
}