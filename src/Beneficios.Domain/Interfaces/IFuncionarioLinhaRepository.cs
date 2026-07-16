using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IFuncionarioLinhaRepository
{
    Task<Guid> SalvarAsync(FuncionarioLinhaSalvarParams parametros);
    Task AtualizarAsync(FuncionarioLinhaAtualizarParams parametros);
    Task<FuncionarioLinhaQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioLinhaQueryResult>> FiltrarAsync(FuncionarioLinhaFiltroParams filtro);
    Task<bool> ExisteParAsync(Guid funcionarioId, Guid linhaOnibusId, Guid? excetoId = null);
    Task<bool> ExisteVinculoAbertoPorLinhaAsync(Guid linhaOnibusId);
    Task EncerrarAbertosPorFuncionarioAsync(Guid funcionarioId, DateOnly dataFim, DateTime dataAlteracao, Guid? usuarioAlteracaoId);
}
