using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IEmpresaRepository
{
    Task<Guid> SalvarAsync(EmpresaSalvarParams empresa);
    Task<bool> AtualizarAsync(EmpresaAtualizarParams empresa);
    Task<bool> DeleteAsync(Guid id);
    Task<EmpresaQueryResult?> ObterUmAsync(Guid id);
    Task<EmpresaQueryResult[]> ObterTodosAsync();
}
