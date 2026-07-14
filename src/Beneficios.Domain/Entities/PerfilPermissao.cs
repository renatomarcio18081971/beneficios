namespace Beneficios.Domain.Entities;

public class PerfilPermissao
{
    public Guid Id { get; set; }
    public Guid PerfilId { get; set; }
    public string CodigoMenu { get; set; } = string.Empty;
    public bool Visualizar { get; set; }
    public bool Criar { get; set; }
    public bool Editar { get; set; }
    public bool Excluir { get; set; }
}
