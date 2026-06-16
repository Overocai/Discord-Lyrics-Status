using DiscordLyrics.Models;

namespace DiscordLyrics.Services;

/// <summary>The background orchestrator: media -> lyrics -> Discord status.</summary>
public interface IStatusEngine
{
    bool Running { get; }
    NowPlaying Current { get; }
    bool HasLyrics { get; }
    string PrevLine { get; }
    string CurrentLine { get; }
    string NextLine { get; }
    string? StatusText { get; }

    /// <summary>Raised after every poll. Subscribers must marshal to the UI thread.</summary>
    event Action? Updated;

    Task StartAsync();
    Task StopAsync();
}
