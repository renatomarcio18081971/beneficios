namespace Beneficios.Application.DTOs;

public record EmpresaCreateDto(
    string RazaoSocial,
    string Dominio,
    string NomeBanco,
    string UsuarioBanco,
    string SenhaBanco
);
