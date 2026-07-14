namespace Beneficios.Application.DTOs;

public class PerfilDto
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public bool EhSistema { get; set; }
    public DateTime DataInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public List<PermissaoMenuDto> Permissoes { get; set; } = [];
}
