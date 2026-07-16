namespace Beneficios.Domain.Models;

public sealed class FuncionarioAfastamentoFiltroParams
{
    public Guid? FuncionarioId { get; init; }
    public string? Tipo { get; init; }
    public DateOnly? DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
}
