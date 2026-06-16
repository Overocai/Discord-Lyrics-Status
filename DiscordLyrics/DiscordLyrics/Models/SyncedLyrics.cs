namespace DiscordLyrics.Models;

/// <summary>A single time-synced lyric line.</summary>
public sealed record LyricLine(TimeSpan Time, string Text);

/// <summary>A parsed LRC document with helpers to find the line at a given position.</summary>
public sealed class SyncedLyrics
{
    public IReadOnlyList<LyricLine> Lines { get; }

    public SyncedLyrics(IReadOnlyList<LyricLine> lines) => Lines = lines;

    public bool IsEmpty => Lines.Count == 0;

    /// <summary>Index of the line that should be displayed at <paramref name="position"/> (or -1).</summary>
    public int IndexAt(TimeSpan position)
    {
        if (Lines.Count == 0) return -1;

        // Binary search for the last line whose timestamp is <= position.
        int lo = 0, hi = Lines.Count - 1, result = -1;
        while (lo <= hi)
        {
            int mid = (lo + hi) / 2;
            if (Lines[mid].Time <= position)
            {
                result = mid;
                lo = mid + 1;
            }
            else
            {
                hi = mid - 1;
            }
        }
        return result;
    }

    /// <summary>(previous, current, next) lines around <paramref name="position"/>.</summary>
    public (string Prev, string Current, string Next) Window(TimeSpan position)
    {
        int i = IndexAt(position);
        string prev = i > 0 ? Lines[i - 1].Text : string.Empty;
        string cur = i >= 0 ? Lines[i].Text : string.Empty;
        string next = i >= 0 && i + 1 < Lines.Count ? Lines[i + 1].Text : string.Empty;
        return (prev, cur, next);
    }
}
