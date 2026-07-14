using Beneficios.Domain.Enums;

namespace Beneficios.Application.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UsuarioId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UsuarioPerfil Perfil { get; set; }
    public Guid? EmpresaId { get; set; }
    public string EmpresaDominio { get; set; } = string.Empty;
    public Guid? PerfilAcessoId { get; set; }
    public string PerfilAcessoNome { get; set; } = string.Empty;
    public List<PermissaoMenuDto> Permissoes { get; set; } = [];
}
