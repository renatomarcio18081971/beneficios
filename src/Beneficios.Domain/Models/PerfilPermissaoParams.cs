namespace Beneficios.Domain.Models;

public record PerfilPermissaoParams
{
    public Guid Id { get; init; }
    public Guid PerfilId { get; init; }
    public string CodigoMenu { get; init; } = string.Empty;
    public bool Visualizar { get; init; }
    public bool Criar { get; init; }
    public bool Editar { get; init; }
    public bool Excluir { get; init; }
}
