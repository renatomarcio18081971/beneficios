using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface IPerfilRepository
{
    Task<Guid> SalvarAsync(PerfilSalvarParams perfil);
    Task<bool> AtualizarAsync(PerfilAtualizarParams perfil);
    Task<bool> DeleteAsync(Guid id);
    Task<PerfilQueryResult?> ObterUmAsync(Guid id);
    Task<PerfilQueryResult[]> ObterTodosAsync();
    Task<PerfilQueryResult[]> FiltrarAsync(PerfilFiltroParams filtro);
    Task<bool> EstaEmUsoAsync(Guid id);
    Task<PerfilPermissaoParams[]> ObterPermissoesPorUsuarioAsync(Guid usuarioId);
}
