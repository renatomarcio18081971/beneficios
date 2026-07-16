namespace Beneficios.Domain.Models;

public sealed class FuncionarioLinhaSalvarParams
{
    public Guid Id { get; init; }
    public Guid FuncionarioId { get; init; }
    public Guid LinhaOnibusId { get; init; }
    public int Quantidade { get; init; }
    public DateOnly DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public DateTime DataInclusao { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
