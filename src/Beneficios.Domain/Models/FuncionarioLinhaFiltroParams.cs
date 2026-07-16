namespace Beneficios.Domain.Models;

public sealed class FuncionarioLinhaFiltroParams
{
    public Guid? FuncionarioId { get; init; }
    public bool? SomenteVigentes { get; init; }
    public DateOnly? Referencia { get; init; }
}
