using System.Globalization;

namespace GMMS.App.Services;

public static class CurrencyFormatter
{
    public static string FormatMMK(decimal amount)
    {
        var formattedNumber = amount.ToString("#,##0", CultureInfo.InvariantCulture);
        return $"Ks {formattedNumber}";
    }
}
