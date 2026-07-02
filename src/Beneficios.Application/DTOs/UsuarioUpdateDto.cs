namespace Beneficios.Application.DTOs;

public record UsuarioUpdateDto(
    string Nome,
    string Email,
    Guid EmpresaId
);
