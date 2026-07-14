namespace Beneficios.Domain.Models;

public record PerfilAtualizarParams
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public Guid? UsuarioAlteracaoId { get; init; }
    public IReadOnlyList<PerfilPermissaoParams> Permissoes { get; init; } = [];
}
