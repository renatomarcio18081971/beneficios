using Beneficios.Application.DTOs;
using Beneficios.Domain.Enums;

namespace Beneficios.Application.Interfaces;

public interface IFuncionarioAfastamentoService
{
    Task<Guid> SalvarAsync(AfastamentoSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, AfastamentoAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task ExcluirAsync(Guid id);
    Task<AfastamentoDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<AfastamentoDto>> FiltrarAsync(
        Guid? funcionarioId,
        TipoAfastamento? tipo,
        DateOnly? dataInicio,
        DateOnly? dataFim);
}
