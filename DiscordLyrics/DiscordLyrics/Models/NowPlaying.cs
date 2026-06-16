namespace DiscordLyrics.Models;

/// <summary>An immutable snapshot of whatever is playing on the system right now.</summary>
public sealed record NowPlaying(
    string Title,
    string Artist,
    TimeSpan Position,
    TimeSpan Duration,
    bool IsPlaying,
    byte[]? Thumbnail)
{
    public static readonly NowPlaying Empty =
        new(string.Empty, string.Empty, TimeSpan.Zero, TimeSpan.Zero, false, null);

    public bool HasTrack => !string.IsNullOrWhiteSpace(Title);

    /// <summary>Stable identity used to detect when the track actually changed.</summary>
    public string Key => $"{Title}{Artist}".ToLowerInvariant();
}
