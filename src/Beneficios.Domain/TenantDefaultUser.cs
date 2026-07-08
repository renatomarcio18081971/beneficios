namespace Beneficios.Domain;

public static class TenantDefaultUser
{
    public const string Nome = "Usuário cadastrar";
    public const string Email = "user@123.com";
    public const string Senha = "user@123";

    public static bool IsDefaultUser(string email) =>
        string.Equals(email.Trim(), Email, StringComparison.OrdinalIgnoreCase);
}
