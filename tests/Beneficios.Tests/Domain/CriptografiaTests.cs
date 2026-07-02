using Beneficios.Domain.ValueObjects;
using Xunit;

namespace Beneficios.Tests.Domain;

public class CriptografiaTests
{
    [Fact]
    public void Encrypt_DeveRetornarTextoCriptografado()
    {
        var textoOriginal = "senha123";
        var textoCriptografado = Criptografia.Encrypt(textoOriginal);

        Assert.NotEqual(textoOriginal, textoCriptografado);
        Assert.NotEmpty(textoCriptografado);
    }

    [Fact]
    public void Decrypt_DeveRetornarTextoOriginal()
    {
        var textoOriginal = "senha123";
        var textoCriptografado = Criptografia.Encrypt(textoOriginal);
        var textoDescriptografado = Criptografia.Decrypt(textoCriptografado);

        Assert.Equal(textoOriginal, textoDescriptografado);
    }

    [Fact]
    public void EncryptDecrypt_DeveSerReversivel()
    {
        var textos = new[] { "teste", "123456", "senha@#$", "email@example.com" };

        foreach (var texto in textos)
        {
            var criptografado = Criptografia.Encrypt(texto);
            var descriptografado = Criptografia.Decrypt(criptografado);
            Assert.Equal(texto, descriptografado);
        }
    }
}
