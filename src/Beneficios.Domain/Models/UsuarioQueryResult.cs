namespace Beneficios.Domain.Models;

public class UsuarioQueryResult
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public string EmpresaNome { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
