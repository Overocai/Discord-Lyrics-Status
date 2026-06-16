using System.Globalization;
using System.Text.RegularExpressions;
using DiscordLyrics.Models;

namespace DiscordLyrics.Infrastructure.Text;

/// <summary>Parses an LRC document ("[mm:ss.xx] text", multiple tags per line) into sorted lines.</summary>
public static partial class LrcParser
{
    [GeneratedRegex(@"\[(\d{1,2}):(\d{2})(?:[.:](\d{1,3}))?\]")]
    private static partial Regex Tag();

    public static SyncedLyrics Parse(string? lrc)
    {
        var lines = new List<LyricLine>();
        if (string.IsNullOrWhiteSpace(lrc)) return new SyncedLyrics(lines);

        foreach (var raw in lrc.Replace("\r\n", "\n").Split('\n'))
        {
            var matches = Tag().Matches(raw);
            if (matches.Count == 0) continue;

            var text = Tag().Replace(raw, string.Empty).Trim();
            foreach (Match m in matches)
            {
                int min = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                int sec = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                int frac = 0;
                if (m.Groups[3].Success)
                {
                    var f = m.Groups[3].Value.PadRight(3, '0')[..3];
                    frac = int.Parse(f, CultureInfo.InvariantCulture);
                }
                var time = new TimeSpan(0, 0, min, sec, frac);
                lines.Add(new LyricLine(time, text));
            }
        }

        lines.Sort((a, b) => a.Time.CompareTo(b.Time));
        return new SyncedLyrics(lines);
    }
}
