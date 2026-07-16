namespace Beneficios.Domain.Models;

public sealed class FuncionarioAfastamentoAtualizarParams
{
    public Guid Id { get; init; }
    public Guid FuncionarioId { get; init; }
    public string Tipo { get; init; } = "";
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public string? Observacao { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
