using DiscordLyrics.Models;

namespace DiscordLyrics.Services;

public interface ISettingsService
{
    AppSettings Current { get; }
    event Action? Changed;

    Task LoadAsync();
    Task SaveAsync(AppSettings settings);

    /// <summary>Discord token, stored encrypted at rest with Windows DPAPI.</summary>
    string? GetToken();
    Task SetTokenAsync(string token);
    bool HasToken { get; }
}
