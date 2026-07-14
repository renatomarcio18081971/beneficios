using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Entities;

public class CalendarioDia
{
    public Guid Id { get; set; }
    public DateOnly Data { get; set; }
    public bool EhDiaUtil { get; set; }
    public TipoExcecaoCalendario? TipoExcecao { get; set; }
    public OrigemCalendarioDia Origem { get; set; }
    public string? Observacao { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public Guid? UsuarioAlteracaoId { get; set; }
}
