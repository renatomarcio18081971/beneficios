namespace Beneficios.Domain.Models;

public class EmpresaQueryResult
{
    public Guid Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Dominio { get; set; } = string.Empty;
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
}
