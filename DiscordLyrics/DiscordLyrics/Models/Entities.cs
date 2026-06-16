using System.ComponentModel.DataAnnotations;

namespace DiscordLyrics.Models;

/// <summary>One row per track that was detected, for the History section.</summary>
public sealed class PlayHistoryEntry
{
    public int Id { get; set; }
    [MaxLength(300)] public string Title { get; set; } = string.Empty;
    [MaxLength(300)] public string Artist { get; set; } = string.Empty;
    public bool HadLyrics { get; set; }
    public DateTimeOffset PlayedAt { get; set; } = DateTimeOffset.Now;
}

/// <summary>A reusable status preset (Profiles section): prefix + emoji + behaviour.</summary>
public sealed class DiscordProfile
{
    public int Id { get; set; }
    [MaxLength(80)] public string Name { get; set; } = "Default";
    [MaxLength(40)] public string StatusPrefix { get; set; } = "♪ ";
    [MaxLength(40)] public string EmojiName { get; set; } = string.Empty;
    public bool ClearOnPause { get; set; } = true;
    public bool ShowSongWhenNoLyrics { get; set; } = true;
    public bool IsActive { get; set; }
}

/// <summary>Simple key/value store backing <c>ISettingsService</c>.</summary>
public sealed class SettingEntry
{
    [Key, MaxLength(80)] public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
