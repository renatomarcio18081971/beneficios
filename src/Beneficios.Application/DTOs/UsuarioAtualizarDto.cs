namespace Beneficios.Application.DTOs;

public record UsuarioAtualizarDto(
    string Nome,
    string Email,
    Guid EmpresaId,
    Guid PerfilId
);
