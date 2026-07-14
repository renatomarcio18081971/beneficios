using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IPerfilService
{
    Task<Guid> SalvarAsync(PerfilSalvarDto dto);
    Task AtualizarAsync(Guid id, PerfilAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task DeleteAsync(Guid id);
    Task<PerfilDto?> ObterUmAsync(Guid id);
    Task<PerfilDto[]> ObterTodosAsync();
    Task<PerfilDto[]> FiltrarAsync(PerfilFiltroDto filtro);
}
