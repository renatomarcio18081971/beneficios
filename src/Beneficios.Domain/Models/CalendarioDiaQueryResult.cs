using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public class CalendarioDiaQueryResult
{
    public Guid Id { get; set; }
    public DateOnly Data { get; set; }
    public bool EhDiaUtil { get; set; }
    public TipoExcecaoCalendario? TipoExcecao { get; set; }
    public OrigemCalendarioDia Origem { get; set; }
    public string? Observacao { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
