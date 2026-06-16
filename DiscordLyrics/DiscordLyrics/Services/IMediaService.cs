using DiscordLyrics.Models;

namespace DiscordLyrics.Services;

/// <summary>Reads the system "now playing" session via Windows media transport controls.</summary>
public interface IMediaService
{
    Task<NowPlaying> GetNowPlayingAsync(bool includeThumbnail = true, CancellationToken ct = default);
}
