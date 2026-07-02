namespace Beneficios.Domain.Entities;

public class Usuario
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public Guid? UsuarioAlteracaoId { get; set; }

    public Empresa? Empresa { get; set; }
}
