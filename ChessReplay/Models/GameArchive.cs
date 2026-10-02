using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ChessReplay.Models;

public sealed record GameArchive(int Year, int Month)
{
    // Chess.com archive URLs look like ".../games/2026/08".
    public static bool TryParseFromUrl(string archiveUrl, [NotNullWhen(true)] out GameArchive? archive)
    {
        var segments = archiveUrl.TrimEnd('/').Split('/');
        if (segments.Length < 2)
        {
            archive = null;
            return false;
        }

        return TryParse($"{segments[^2]}-{segments[^1]}", out archive);
    }

    // CLI/user input, expected as "YYYY-MM".
    public static bool TryParse(string value, [NotNullWhen(true)] out GameArchive? archive)
    {
        archive = null;

        var parts = value.Split('-');
        if (parts.Length != 2 || parts[0].Length != 4)
            return false;

        if (!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out var year))
            return false;

        if (!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var month))
            return false;

        if (year is < 1900 or > 9999 || month is < 1 or > 12)
            return false;

        archive = new GameArchive(year, month);
        return true;
    }

    public override string ToString() => $"{Year:D4}-{Month:D2}";
}
