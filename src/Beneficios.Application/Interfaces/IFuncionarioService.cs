using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IFuncionarioService
{
    Task<Guid> SalvarAsync(FuncionarioSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, FuncionarioAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<FuncionarioDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioDto>> FiltrarAsync(FuncionarioFiltroDto filtro);
}
