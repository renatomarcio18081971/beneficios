namespace Beneficios.Domain.Models;

public record EmpresaUpdateParams(
    Guid Id,
    string RazaoSocial,
    string Dominio,
    string NomeBanco,
    string UsuarioBanco,
    string SenhaBanco,
    Guid? UsuarioAlteracaoId
);
