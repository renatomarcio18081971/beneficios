namespace Beneficios.Domain.Models;

public record PerfilSalvarParams
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public bool EhSistema { get; init; }
    public IReadOnlyList<PerfilPermissaoParams> Permissoes { get; init; } = [];
}
