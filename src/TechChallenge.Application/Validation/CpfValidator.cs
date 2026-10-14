namespace TechChallenge.Application.Validation;

public static class CpfValidator
{
    public static bool CpfValido(string? cpf)
    {
        var digits = ApenasDigitos(cpf);
        if (digits.Length != 11)
            return false;

        if (digits.All(digit => digit == digits[0]))
            return false;

        var primeiroDigito = CalcularDigito(digits[..9], 10);
        var segundoDigito = CalcularDigito(digits[..10], 11);

        return digits[9] == primeiroDigito && digits[10] == segundoDigito;
    }

    public static string Formatar(string cpf)
    {
        var digits = ApenasDigitos(cpf);
        return $"{digits[..3]}.{digits[3..6]}.{digits[6..9]}-{digits[9..]}";
    }

    private static string ApenasDigitos(string? value)
    {
        return new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private static char CalcularDigito(string digits, int pesoInicial)
    {
        var soma = digits
            .Select((digit, index) => (digit - '0') * (pesoInicial - index))
            .Sum();

        var resto = soma % 11;
        var digito = resto < 2 ? 0 : 11 - resto;

        return (char)('0' + digito);
    }
}
