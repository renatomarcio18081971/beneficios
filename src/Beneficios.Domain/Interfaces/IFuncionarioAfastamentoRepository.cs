using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IFuncionarioAfastamentoRepository
{
    Task<Guid> SalvarAsync(FuncionarioAfastamentoSalvarParams parametros);
    Task AtualizarAsync(FuncionarioAfastamentoAtualizarParams parametros);
    Task ExcluirAsync(Guid id);
    Task<FuncionarioAfastamentoQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioAfastamentoQueryResult>> FiltrarAsync(FuncionarioAfastamentoFiltroParams filtro);
    Task<IReadOnlyList<FuncionarioAfastamentoQueryResult>> ListarPorFuncionarioAsync(Guid funcionarioId);
    Task<bool> ExisteSobreposicaoAsync(Guid funcionarioId, DateOnly dataInicio, DateOnly? dataFim, Guid? excetoId = null);
    Task<FuncionarioAfastamentoQueryResult?> ObterAtivoEmAsync(Guid funcionarioId, DateOnly referencia);
}
