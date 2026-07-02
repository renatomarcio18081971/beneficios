namespace Beneficios.Domain.Entities;

public class Empresa
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Dominio { get; set; } = string.Empty;
    public string NomeBanco { get; set; } = string.Empty;
    public string UsuarioBanco { get; set; } = string.Empty;
    public string SenhaBanco { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public Guid? UsuarioAlteracaoId { get; set; }
}
