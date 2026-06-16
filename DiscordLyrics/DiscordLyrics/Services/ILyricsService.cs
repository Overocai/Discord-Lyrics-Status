using DiscordLyrics.Models;

namespace DiscordLyrics.Services;

public interface ILyricsService
{
    /// <summary>Fetch synced lyrics for a track (cleaned + cached). Returns empty if none found.</summary>
    Task<SyncedLyrics> GetAsync(string title, string artist, TimeSpan duration, CancellationToken ct = default);
}
