namespace DiscordLyrics.Models;

/// <summary>User-facing configuration. Persisted as key/value rows in SQLite.</summary>
public sealed class AppSettings
{
    public string StatusPrefix { get; set; } = "♪ ";
    public string EmojiName { get; set; } = string.Empty;
    public double PollIntervalSeconds { get; set; } = 0.5;
    public double LineLeadSeconds { get; set; } = 0.4;
    public int MaxStatusLength { get; set; } = 128;
    public bool ShowSongWhenNoLyrics { get; set; } = true;
    public bool ClearOnPause { get; set; } = true;
    public bool SyncedOnly { get; set; } = true;
    public bool AutoStartEngine { get; set; } = true;
    public bool LaunchAtStartup { get; set; } = false;

    public AppSettings Clone() => (AppSettings)MemberwiseClone();
}
