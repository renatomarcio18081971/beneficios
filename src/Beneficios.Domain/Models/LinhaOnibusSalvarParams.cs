namespace Beneficios.Domain.Models;

public sealed class LinhaOnibusSalvarParams
{
    public Guid Id { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public decimal ValorTarifa { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
