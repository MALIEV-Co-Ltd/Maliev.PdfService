using System.Globalization;

namespace Maliev.PdfService.Infrastructure.Services;

/// <summary>
/// Helper for formatting currency values in PDF documents.
/// </summary>
public static class CurrencyFormatHelper
{
    private static readonly Dictionary<string, string> CurrencySymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        { "THB", "฿" },
        { "USD", "$" },
        { "EUR", "€" },
        { "GBP", "£" },
        { "JPY", "¥" },
        { "CNY", "¥" },
        { "KRW", "₩" },
        { "SGD", "S$" },
        { "AUD", "A$" },
        { "CAD", "C$" },
    };

    /// <summary>
    /// Formats a decimal amount with the specified currency code.
    /// </summary>
    /// <param name="amount">The amount to format.</param>
    /// <param name="currencyCode">The ISO 4217 currency code.</param>
    /// <param name="decimals">Number of decimal places.</param>
    /// <returns>Formatted currency string (e.g., "฿1,234.56").</returns>
    public static string Format(decimal amount, string? currencyCode, int decimals = 2)
    {
        var symbol = GetSymbol(currencyCode);
        var format = decimals > 0 ? $"N{decimals}" : "N0";
        return $"{symbol}{amount.ToString(format)}";
    }

    /// <summary>
    /// Formats a double amount with the specified currency code.
    /// </summary>
    public static string Format(double amount, string? currencyCode, int decimals = 2)
    {
        return Format((decimal)amount, currencyCode, decimals);
    }

    /// <summary>
    /// Gets the currency symbol for the given currency code.
    /// </summary>
    public static string GetSymbol(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
            return "฿";

        return CurrencySymbols.TryGetValue(currencyCode, out var symbol) ? symbol : currencyCode;
    }
}
