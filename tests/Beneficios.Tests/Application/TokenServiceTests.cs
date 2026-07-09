using Beneficios.Application.Services;
using Beneficios.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Beneficios.Tests.Application;

public class TokenServiceTests
{
    private const string Secret = "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres";
    private readonly TokenService _service = new(Secret, "BeneficiosApi", "BeneficiosClient");

    [Fact]
    public void GenerateToken_DeveIncluirClaimsPerfilEEmpresaId()
    {
        var token = _service.GenerateToken(Guid.NewGuid(), "admin@test.com", UsuarioPerfil.Admin, null);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("Admin", jwt.Claims.First(c => c.Type == "perfil").Value);
        Assert.DoesNotContain(jwt.Claims, c => c.Type == "empresa_id");
    }

    [Fact]
    public void GenerateToken_ComEmpresa_DeveIncluirEmpresaId()
    {
        var empresaId = Guid.NewGuid();
        var token = _service.GenerateToken(Guid.NewGuid(), "u@test.com", UsuarioPerfil.Empresa, empresaId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal(empresaId.ToString(), jwt.Claims.First(c => c.Type == "empresa_id").Value);
    }

    [Fact]
    public void ValidateToken_DeveValidarTokenCorreto()
    {
        var usuarioId = Guid.NewGuid();
        var token = _service.GenerateToken(usuarioId, "teste@example.com", UsuarioPerfil.Empresa, Guid.NewGuid());
        var validatedUserId = _service.ValidateToken(token);

        Assert.NotNull(validatedUserId);
        Assert.Equal(usuarioId, validatedUserId);
    }

    [Fact]
    public void ValidateToken_DeveRetornarNullParaTokenInvalido()
    {
        var result = _service.ValidateToken("token-invalido-123");

        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_DeveRetornarNullParaTokenVazio()
    {
        var result = _service.ValidateToken(string.Empty);

        Assert.Null(result);
    }
}
