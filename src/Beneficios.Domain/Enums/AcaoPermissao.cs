namespace Beneficios.Domain.Enums;

[Flags]
public enum AcaoPermissao
{
    Nenhuma = 0,
    Visualizar = 1,
    Criar = 2,
    Editar = 4,
    Excluir = 8,
    Todas = Visualizar | Criar | Editar | Excluir
}
