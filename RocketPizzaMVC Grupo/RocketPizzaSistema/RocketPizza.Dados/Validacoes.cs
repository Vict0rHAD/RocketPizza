using System.Text.RegularExpressions;

namespace RocketPizza.Dados;

public static class Validacoes
{
    public const string PadraoTelefone = @"^\(\d{2}\)\s9?\d{4}-\d{4}$";
    public const string PadraoApenasNumeros = @"^\d+$";
    public const string PadraoApenasLetras = @"^[A-Za-zÀ-ÿ][A-Za-zÀ-ÿ' -]*$";

    public static bool TelefoneValido(string? valor) => Regex.IsMatch(valor ?? "", PadraoTelefone);
    public static bool ApenasNumeros(string? valor) => Regex.IsMatch(valor ?? "", PadraoApenasNumeros);
    public static bool ApenasLetras(string? valor) => Regex.IsMatch(valor ?? "", PadraoApenasLetras);
}

