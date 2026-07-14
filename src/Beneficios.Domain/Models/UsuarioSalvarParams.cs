namespace Beneficios.Domain.Models;

using Beneficios.Domain.Enums;

public record UsuarioSalvarParams
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid EmpresaId { get; init; }
    public UsuarioPerfil Perfil { get; init; }
    public Guid? PerfilId { get; init; }
}
