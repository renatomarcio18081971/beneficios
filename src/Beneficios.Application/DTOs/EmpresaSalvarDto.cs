namespace Beneficios.Application.DTOs;

public record EmpresaSalvarDto(
    string RazaoSocial,
    string Dominio,
    string NomeBanco,
    string UsuarioBanco,
    string SenhaBanco
);
