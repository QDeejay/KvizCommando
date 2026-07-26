using System.Globalization;
using System.Text.RegularExpressions;

namespace BWin2.Wasm;

internal static partial class Qb
{
    [GeneratedRegex(@"^[\s]*[+-]?(?:\d+(?:\.\d*)?|\.\d+)", RegexOptions.CultureInvariant)]
    private static partial Regex NumberPrefix();

    public static int Int(double value) => (int)Math.Floor(value);

    public static double Val(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        Match match = NumberPrefix().Match(value);
        return match.Success
            ? double.Parse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture)
            : 0;
    }

    public static string Str(double value)
    {
        string formatted = Math.Abs(value % 1) < 0.0000001
            ? ((long)value).ToString(CultureInfo.InvariantCulture)
            : value.ToString("0.#######", CultureInfo.InvariantCulture);

        if (value > -1 && value < 1 && value != 0)
            formatted = formatted.Replace("0.", ".", StringComparison.Ordinal);

        return value >= 0 ? " " + formatted : formatted;
    }

    public static string Left(string value, int length) =>
        value[..Math.Clamp(length, 0, value.Length)];

    public static string Right(string value, int length)
    {
        length = Math.Clamp(length, 0, value.Length);
        return value[^length..];
    }

    public static string Mid(string value, int start, int length)
    {
        int index = Math.Clamp(start - 1, 0, value.Length);
        int count = Math.Clamp(length, 0, value.Length - index);
        return value.Substring(index, count);
    }

}
