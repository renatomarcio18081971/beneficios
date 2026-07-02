using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IEmpresaService
{
    Task<Guid> CreateAsync(EmpresaCreateDto dto);
    Task<bool> UpdateAsync(Guid id, EmpresaUpdateDto dto, Guid? usuarioAlteracaoId);
    Task<bool> DeleteAsync(Guid id);
    Task<EmpresaDto?> GetByIdAsync(Guid id);
    Task<EmpresaDto[]> GetAllAsync();
}
