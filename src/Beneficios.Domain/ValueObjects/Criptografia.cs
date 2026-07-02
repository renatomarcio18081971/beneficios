namespace Beneficios.Domain.ValueObjects;

public class Criptografia
{
    public static string Encrypt(string texto)
    {
        if (string.IsNullOrEmpty(texto))
            return texto;

        var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(texto);
        return Convert.ToBase64String(plainTextBytes);
    }

    public static string Decrypt(string textoCriptografado)
    {
        if (string.IsNullOrEmpty(textoCriptografado))
            return textoCriptografado;

        var base64EncodedBytes = Convert.FromBase64String(textoCriptografado);
        return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
    }
}
