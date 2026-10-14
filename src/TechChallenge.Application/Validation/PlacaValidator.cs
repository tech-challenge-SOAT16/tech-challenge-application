using System.Text.RegularExpressions;

namespace TechChallenge.Application.Validation;

public static partial class PlacaValidator
{
    public static bool PlacaValida(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            return false;

        var placaNormalizada = Normalizar(placa);
        return PlacaAntigaRegex().IsMatch(placaNormalizada) || PlacaMercosulRegex().IsMatch(placaNormalizada);
    }

    public static string Formatar(string placa)
    {
        var placaNormalizada = Normalizar(placa);

        if (PlacaAntigaRegex().IsMatch(placaNormalizada))
            return $"{placaNormalizada[..3]}-{placaNormalizada[3..]}";

        return placaNormalizada;
    }

    public static string Normalizar(string placa)
    {
        return placa.Replace("-", "").Replace(" ", "").ToUpperInvariant();
    }

    [GeneratedRegex("^[A-Z]{3}[0-9]{4}$")]
    private static partial Regex PlacaAntigaRegex();

    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z][0-9]{2}$")]
    private static partial Regex PlacaMercosulRegex();
}
