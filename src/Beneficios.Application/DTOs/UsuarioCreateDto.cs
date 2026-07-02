namespace Beneficios.Application.DTOs;

public record UsuarioCreateDto(
    string Nome,
    string Senha,
    string Email,
    Guid EmpresaId
);
