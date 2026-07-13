namespace Beneficios.Application.DTOs;

public record AlterarSenhaDto(
    string Codigo,
    string NovaSenha,
    string ConfirmarNovaSenha
);
