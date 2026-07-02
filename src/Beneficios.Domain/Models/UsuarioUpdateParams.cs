namespace Beneficios.Domain.Models;

public record UsuarioUpdateParams(
    Guid Id,
    string Nome,
    string Email,
    Guid EmpresaId,
    Guid? UsuarioAlteracaoId
);
