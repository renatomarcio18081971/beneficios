namespace Beneficios.Domain.Models;

using Beneficios.Domain.Enums;

public class UsuarioQueryResult
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public Guid? PerfilId { get; set; }
    public string? PerfilAcessoNome { get; set; }
    public UsuarioPerfil Perfil { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
