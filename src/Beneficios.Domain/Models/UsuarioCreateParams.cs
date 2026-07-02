namespace Beneficios.Domain.Models;

public record UsuarioCreateParams(
    Guid Id,
    string Nome,
    string Senha,
    string Email,
    Guid EmpresaId
);
