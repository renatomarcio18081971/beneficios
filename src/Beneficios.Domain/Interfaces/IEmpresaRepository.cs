using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IEmpresaRepository
{
    Task<Guid> CreateAsync(EmpresaCreateParams empresa);
    Task<bool> UpdateAsync(EmpresaUpdateParams empresa);
    Task<bool> DeleteAsync(Guid id);
    Task<EmpresaQueryResult?> GetByIdAsync(Guid id);
    Task<EmpresaQueryResult[]> GetAllAsync();
}
