namespace Beneficios.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid usuarioId, string email);
    Guid? ValidateToken(string token);
}
