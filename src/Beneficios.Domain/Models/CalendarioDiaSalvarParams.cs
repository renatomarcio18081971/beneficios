using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public class CalendarioDiaSalvarParams
{
    public Guid Id { get; init; }
    public DateOnly Data { get; init; }
    public bool EhDiaUtil { get; init; }
    public TipoExcecaoCalendario? TipoExcecao { get; init; }
    public OrigemCalendarioDia Origem { get; init; }
    public string? Observacao { get; init; }
}
