namespace Beneficios.Domain.Models;

public sealed class FuncionarioAfastamentoQueryResult
{
    public Guid Id { get; set; }
    public Guid FuncionarioId { get; set; }
    public string FuncionarioNome { get; set; } = "";
    public string Tipo { get; set; } = "";
    public DateOnly DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }
    public string? Observacao { get; set; }
}
