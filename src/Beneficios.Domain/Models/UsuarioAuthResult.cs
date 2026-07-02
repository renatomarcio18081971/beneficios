namespace Beneficios.Domain.Models;

public class UsuarioAuthResult
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public Guid EmpresaId { get; set; }
    public string Token { get; set; } = string.Empty;
}
