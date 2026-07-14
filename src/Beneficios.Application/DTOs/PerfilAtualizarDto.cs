namespace Beneficios.Application.DTOs;

public record PerfilAtualizarDto(
    string Nome,
    IReadOnlyList<PermissaoMenuDto> Permissoes);
