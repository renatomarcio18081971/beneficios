using Beneficios.Application.Interfaces;
using Beneficios.Application.Services;
using Xunit;

namespace Beneficios.Tests.Application;

public class TokenServiceTests
{
    private readonly ITokenService _tokenService;
    private readonly string _secretKey = "sua-chave-secreta-super-segura-com-pelo-menos-32-caracteres";
    private readonly string _issuer = "BeneficiosApi";
    private readonly string _audience = "BeneficiosClient";

    public TokenServiceTests()
    {
        _tokenService = new TokenService(_secretKey, _issuer, _audience);
    }

    [Fact]
    public void GenerateToken_DeveGerarTokenValido()
    {
        var usuarioId = Guid.NewGuid();
        var email = "teste@example.com";

        var token = _tokenService.GenerateToken(usuarioId, email);

        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }

    [Fact]
    public void ValidateToken_DeveValidarTokenCorreto()
    {
        var usuarioId = Guid.NewGuid();
        var email = "teste@example.com";

        var token = _tokenService.GenerateToken(usuarioId, email);
        var validatedUserId = _tokenService.ValidateToken(token);

        Assert.NotNull(validatedUserId);
        Assert.Equal(usuarioId, validatedUserId);
    }

    [Fact]
    public void ValidateToken_DeveRetornarNullParaTokenInvalido()
    {
        var tokenInvalido = "token-invalido-123";

        var result = _tokenService.ValidateToken(tokenInvalido);

        Assert.Null(result);
    }

    [Fact]
    public void ValidateToken_DeveRetornarNullParaTokenVazio()
    {
        var result = _tokenService.ValidateToken(string.Empty);

        Assert.Null(result);
    }
}
