namespace Beneficios.Application.DTOs;

public record UsuarioSalvarDto(
    string Nome,
    string Senha,
    string Email,
    Guid EmpresaId
);
