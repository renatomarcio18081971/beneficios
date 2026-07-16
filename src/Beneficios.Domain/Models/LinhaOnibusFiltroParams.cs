namespace Beneficios.Domain.Models;

public sealed class LinhaOnibusFiltroParams
{
    public string? Descricao { get; init; }
    public bool? SomenteVigentes { get; init; }
    public DateOnly? Referencia { get; init; } // default hoje no service se SomenteVigentes
}
