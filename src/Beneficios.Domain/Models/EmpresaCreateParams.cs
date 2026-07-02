namespace Beneficios.Domain.Models;

public record EmpresaCreateParams(
    Guid Id,
    string RazaoSocial,
    string Dominio,
    string NomeBanco,
    string UsuarioBanco,
    string SenhaBanco
);
