using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IUsuarioService
{
    Task<Guid> SalvarAsync(UsuarioSalvarDto dto);
    Task<bool> AtualizarAsync(Guid id, UsuarioAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<bool> DeleteAsync(Guid id);
    Task<UsuarioDto?> ObterUmAsync(Guid id);
    Task<UsuarioDto[]> ObterTodosAsync();
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto, string tenantSubdomain);
}
