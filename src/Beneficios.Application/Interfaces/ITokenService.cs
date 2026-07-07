using Beneficios.Domain.Enums;

namespace Beneficios.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(Guid usuarioId, string email, UsuarioPerfil perfil, Guid? empresaId);
    Guid? ValidateToken(string token);
}
