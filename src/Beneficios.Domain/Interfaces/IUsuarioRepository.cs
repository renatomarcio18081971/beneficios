using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Guid> CreateAsync(UsuarioCreateParams usuario);
    Task<bool> UpdateAsync(UsuarioUpdateParams usuario);
    Task<bool> DeleteAsync(Guid id);
    Task<UsuarioQueryResult?> GetByIdAsync(Guid id);
    Task<UsuarioQueryResult[]> GetAllAsync();
    Task<UsuarioAuthResult?> GetByEmailAsync(string email);
    Task<bool> UpdateTokenAsync(Guid id, string token);
}
