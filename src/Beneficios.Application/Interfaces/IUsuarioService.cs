using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IUsuarioService
{
    Task<Guid> CreateAsync(UsuarioCreateDto dto);
    Task<bool> UpdateAsync(Guid id, UsuarioUpdateDto dto, Guid? usuarioAlteracaoId);
    Task<bool> DeleteAsync(Guid id);
    Task<UsuarioDto?> GetByIdAsync(Guid id);
    Task<UsuarioDto[]> GetAllAsync();
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
}
