using Beneficios.Application.DTOs;

namespace Beneficios.Application.Interfaces;

public interface IEmpresaService
{
    Task<Guid> SalvarAsync(EmpresaSalvarDto dto);
    Task<bool> AtualizarAsync(Guid id, EmpresaAtualizarDto dto, Guid? usuarioAlteracaoId);
    Task<bool> DeleteAsync(Guid id);
    Task<EmpresaDto?> ObterUmAsync(Guid id);
    Task<EmpresaDto[]> ObterTodosAsync();
    Task<EmpresaDto[]> FiltrarAsync(EmpresaFiltroDto filtro);
}
