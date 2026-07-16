using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IFuncionarioLinhaService
{
    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarDto dto, Guid? usuarioAlteracaoId);
    Task AtualizarAsync(Guid id, FuncionarioLinhaAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<FuncionarioLinhaDto?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioLinhaDto>> FiltrarAsync(Guid? funcionarioId, bool? somenteVigentes);
    Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, Guid? usuarioAlteracaoId);
}
