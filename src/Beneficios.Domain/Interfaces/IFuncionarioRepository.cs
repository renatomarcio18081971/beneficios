using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IFuncionarioRepository
{
    Task<Guid> SalvarAsync(FuncionarioSalvarParams parametros, IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios);
    Task AtualizarAsync(FuncionarioAtualizarParams parametros, IReadOnlyList<FuncionarioBeneficioSalvarParams> beneficios);
    Task<FuncionarioQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<FuncionarioBeneficioQueryResult>> ObterBeneficiosAsync(Guid funcionarioId);
    Task<IReadOnlyList<FuncionarioQueryResult>> FiltrarAsync(FuncionarioFiltroParams filtro);
    Task<bool> CpfExisteAsync(string cpf, Guid? excetoId = null);
    Task<bool> MatriculaExisteAsync(string matricula, Guid? excetoId = null);
}
