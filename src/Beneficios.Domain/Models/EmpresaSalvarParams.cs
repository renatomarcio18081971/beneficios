namespace Beneficios.Domain.Models;

public record EmpresaSalvarParams
{
    public Guid Id { get; init; }
    public string RazaoSocial { get; init; } = string.Empty;
    public string Dominio { get; init; } = string.Empty;
    public string NomeBanco { get; init; } = string.Empty;
    public string UsuarioBanco { get; init; } = string.Empty;
    public string SenhaBanco { get; init; } = string.Empty;
}
