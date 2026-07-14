using Beneficios.Domain.Enums;

namespace Beneficios.Domain.Models;

public class CalendarioDiaAtualizarParams
{
    public Guid Id { get; init; }
    public bool EhDiaUtil { get; init; }
    public TipoExcecaoCalendario? TipoExcecao { get; init; }
    public OrigemCalendarioDia Origem { get; init; }
    public string? Observacao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
