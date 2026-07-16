using Beneficios.Domain.Models;

namespace Beneficios.Domain.Interfaces;

public interface ILinhaOnibusRepository
{
    Task<Guid> SalvarAsync(LinhaOnibusSalvarParams parametros);
    Task AtualizarAsync(LinhaOnibusAtualizarParams parametros);
    Task<LinhaOnibusQueryResult?> ObterPorIdAsync(Guid id);
    Task<IReadOnlyList<LinhaOnibusQueryResult>> FiltrarAsync(LinhaOnibusFiltroParams filtro);
}
