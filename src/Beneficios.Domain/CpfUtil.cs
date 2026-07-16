using Beneficios.Domain.Enums;
using System.Text.RegularExpressions;

namespace Beneficios.Domain;

public static class CpfUtil
{
    public static string? Normalizar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            return null;

        var digitos = Regex.Replace(valor, @"\D", "");
        return digitos.Length == 0 ? null : digitos;
    }

    public static bool EhValido(string cpfSomenteDigitos)
    {
        if (string.IsNullOrWhiteSpace(cpfSomenteDigitos) || cpfSomenteDigitos.Length != 11)
            return false;

        if (cpfSomenteDigitos.Distinct().Count() == 1)
            return false;

        var nums = cpfSomenteDigitos.Select(c => c - '0').ToArray();
        if (nums.Any(n => n is < 0 or > 9))
            return false;

        var soma = 0;
        for (var i = 0; i < 9; i++)
            soma += nums[i] * (10 - i);
        var resto = soma % 11;
        var d1 = resto < 2 ? 0 : 11 - resto;
        if (nums[9] != d1)
            return false;

        soma = 0;
        for (var i = 0; i < 10; i++)
            soma += nums[i] * (11 - i);
        resto = soma % 11;
        var d2 = resto < 2 ? 0 : 11 - resto;
        return nums[10] == d2;
    }
}
