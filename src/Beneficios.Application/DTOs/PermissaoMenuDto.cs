namespace Beneficios.Application.DTOs;

public record PermissaoMenuDto(
    string CodigoMenu,
    bool Visualizar,
    bool Criar,
    bool Editar,
    bool Excluir);
