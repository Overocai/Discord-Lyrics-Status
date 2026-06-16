namespace DiscordLyrics.Services;

public sealed record UpdateInfo(string Version, string Url, string Notes);

/// <summary>Checks GitHub Releases for a newer published version.</summary>
public interface IUpdateService
{
    string CurrentVersion { get; }
    Task<UpdateInfo?> CheckAsync(CancellationToken ct = default);
}
