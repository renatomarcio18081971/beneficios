namespace Beneficios.Domain.Models;

public record EmpresaTenantInfo
{
    public string NomeBanco { get; init; } = string.Empty;
    public string UsuarioBanco { get; init; } = string.Empty;
    public string SenhaBanco { get; init; } = string.Empty;
}
