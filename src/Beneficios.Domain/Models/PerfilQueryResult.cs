namespace Beneficios.Domain.Models;

public class PerfilQueryResult
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool EhSistema { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public List<PerfilPermissaoParams> Permissoes { get; set; } = [];
}
