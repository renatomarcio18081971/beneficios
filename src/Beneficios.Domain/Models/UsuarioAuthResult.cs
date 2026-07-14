namespace Beneficios.Domain.Models;

using Beneficios.Domain.Enums;

public class UsuarioAuthResult
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public UsuarioPerfil Perfil { get; set; }
    public Guid? PerfilId { get; set; }
    public string EmpresaDominio { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string? PerfilAcessoNome { get; set; }
}
