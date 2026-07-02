namespace Beneficios.Application.DTOs;

public record EmpresaUpdateDto(
    string RazaoSocial,
    string Dominio,
    string NomeBanco,
    string UsuarioBanco,
    string SenhaBanco
);
