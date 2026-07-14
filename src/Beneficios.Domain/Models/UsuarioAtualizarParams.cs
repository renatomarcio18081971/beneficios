namespace Beneficios.Domain.Models;

public record UsuarioAtualizarParams
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid EmpresaId { get; init; }
    public Guid? PerfilId { get; init; }
    public Guid? UsuarioAlteracaoId { get; init; }
}
