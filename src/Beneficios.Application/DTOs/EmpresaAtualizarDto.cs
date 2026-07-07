namespace Beneficios.Application.DTOs;

public record EmpresaAtualizarDto(
    string RazaoSocial,
    string Dominio,
    string NomeBanco,
    string UsuarioBanco,
    string SenhaBanco
);
