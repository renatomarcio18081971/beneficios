namespace Beneficios.Application.DTOs;

public record PerfilSalvarDto(
    string Nome,
    IReadOnlyList<PermissaoMenuDto> Permissoes);
