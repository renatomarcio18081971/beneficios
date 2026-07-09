namespace Beneficios.Domain.Entities;

using Beneficios.Domain.Enums;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public UsuarioPerfil Perfil { get; set; } = UsuarioPerfil.Empresa;
    public string Token { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public Guid? UsuarioAlteracaoId { get; set; }

    public Empresa? Empresa { get; set; }
}
