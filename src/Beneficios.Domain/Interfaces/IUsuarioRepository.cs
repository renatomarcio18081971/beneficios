using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Guid> SalvarAsync(UsuarioSalvarParams usuario);
    Task<bool> AtualizarAsync(UsuarioAtualizarParams usuario);
    Task<bool> DeleteAsync(Guid id);
    Task<UsuarioQueryResult?> ObterUmAsync(Guid id);
    Task<UsuarioQueryResult[]> ObterTodosAsync();
    Task<UsuarioAuthResult?> GetByEmailAsync(string email);
    Task<bool> UpdateTokenAsync(Guid id, string token);
}
