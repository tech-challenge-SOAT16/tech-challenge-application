namespace TechChallenge.Application.Validation;

public static class CnpjValidator
{
    public static bool CnpjValido(string? cnpj)
    {
        var digits = ApenasDigitos(cnpj);
        if (digits.Length != 14)
            return false;

        if (digits.All(digit => digit == digits[0]))
            return false;

        var primeiroDigito = CalcularDigito(digits[..12], [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);
        var segundoDigito = CalcularDigito(digits[..13], [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2]);

        return digits[12] == primeiroDigito && digits[13] == segundoDigito;
    }

    public static string Formatar(string cnpj)
    {
        var digits = ApenasDigitos(cnpj);
        return $"{digits[..2]}.{digits[2..5]}.{digits[5..8]}/{digits[8..12]}-{digits[12..]}";
    }

    private static string ApenasDigitos(string? value)
    {
        return new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private static char CalcularDigito(string digits, int[] pesos)
    {
        var soma = digits
            .Select((digit, index) => (digit - '0') * pesos[index])
            .Sum();

        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;

        return (char)('0' + digito);
    }
}
