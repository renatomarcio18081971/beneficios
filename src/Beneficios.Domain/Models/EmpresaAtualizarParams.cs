namespace Beneficios.Domain.Models;

public record EmpresaAtualizarParams
{
    public Guid Id { get; init; }
    public string RazaoSocial { get; init; } = string.Empty;
    public string Dominio { get; init; } = string.Empty;
    public Guid? UsuarioAlteracaoId { get; init; }
}
