namespace Beneficios.Domain.Models;

public sealed class FuncionarioLinhaAtualizarParams
{
    public Guid Id { get; init; }
    public Guid LinhaOnibusId { get; init; }
    public int Quantidade { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public DateTime DataAlteracao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
