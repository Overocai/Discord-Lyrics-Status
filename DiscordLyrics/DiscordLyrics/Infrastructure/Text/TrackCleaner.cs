using System.Text.RegularExpressions;

namespace DiscordLyrics.Infrastructure.Text;

/// <summary>
/// Normalises noisy media titles/artists (especially from YouTube) so the
/// lyrics lookup has a fair chance of matching.
/// </summary>
public static partial class TrackCleaner
{
    [GeneratedRegex(@"\s*[\(\[](?:official\s*)?(?:music\s*)?(?:lyric[s]?|video|audio|visualizer|clipe?\s*oficial|m/?v|hd|hq|4k|remaster(?:ed)?|explicit)[^\)\]]*[\)\]]",
        RegexOptions.IgnoreCase)]
    private static partial Regex BracketNoise();

    [GeneratedRegex(@"\s*-\s*(?:single|radio|album)?\s*(?:version|edit|remaster(?:ed)?(?:\s*\d{2,4})?)\s*$",
        RegexOptions.IgnoreCase)]
    private static partial Regex TrailingVersion();

    [GeneratedRegex(@"\s*-\s*topic\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex TopicSuffix();

    [GeneratedRegex(@"\s*vevo\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex VevoSuffix();

    public static string Title(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var t = BracketNoise().Replace(raw, string.Empty);
        t = TrailingVersion().Replace(t, string.Empty);
        return t.Trim(' ', '-', '|', '·').Trim();
    }

    public static string Artist(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var a = TopicSuffix().Replace(raw, string.Empty);
        a = VevoSuffix().Replace(a, string.Empty);
        return a.Trim();
    }
}
