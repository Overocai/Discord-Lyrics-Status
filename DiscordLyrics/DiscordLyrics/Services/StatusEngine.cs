using DiscordLyrics.Models;
using Microsoft.Extensions.Logging;

namespace DiscordLyrics.Services;

public sealed class StatusEngine : IStatusEngine
{
    private readonly IMediaService _media;
    private readonly ILyricsService _lyrics;
    private readonly IDiscordService _discord;
    private readonly ISettingsService _settings;
    private readonly IHistoryService _history;
    private readonly ILogger<StatusEngine> _log;

    private CancellationTokenSource? _cts;
    private Task? _loop;
    private string _lastKey = string.Empty;
    private SyncedLyrics _activeLyrics = new(Array.Empty<LyricLine>());

    public StatusEngine(
        IMediaService media, ILyricsService lyrics, IDiscordService discord,
        ISettingsService settings, IHistoryService history, ILogger<StatusEngine> log)
    {
        _media = media;
        _lyrics = lyrics;
        _discord = discord;
        _settings = settings;
        _history = history;
        _log = log;
    }

    public bool Running => _loop is { IsCompleted: false };
    public NowPlaying Current { get; private set; } = NowPlaying.Empty;
    public bool HasLyrics { get; private set; }
    public string PrevLine { get; private set; } = string.Empty;
    public string CurrentLine { get; private set; } = string.Empty;
    public string NextLine { get; private set; } = string.Empty;
    public string? StatusText { get; private set; }

    public event Action? Updated;

    public Task StartAsync()
    {
        if (Running) return Task.CompletedTask;
        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => RunLoopAsync(_cts.Token));
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        if (_cts is null) return;
        _cts.Cancel();
        try { if (_loop is not null) await _loop; }
        catch (OperationCanceledException) { }
        finally
        {
            _cts.Dispose();
            _cts = null;
            _loop = null;
        }

        try { await _discord.ClearStatusAsync(); } catch { /* best effort */ }
        Current = NowPlaying.Empty;
        StatusText = null;
        PrevLine = CurrentLine = NextLine = string.Empty;
        HasLyrics = false;
        Updated?.Invoke();
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var settings = _settings.Current;
            try
            {
                await TickAsync(settings, ct);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "Engine tick failed");
            }

            var delay = TimeSpan.FromSeconds(Math.Clamp(settings.PollIntervalSeconds, 0.25, 5));
            try { await Task.Delay(delay, ct); }
            catch (OperationCanceledException) { break; }
        }
    }

    private async Task TickAsync(AppSettings settings, CancellationToken ct)
    {
        // Lightweight read; only fetch album art when the track actually changes.
        var np = await _media.GetNowPlayingAsync(includeThumbnail: false, ct);

        if (np.Key != _lastKey)
        {
            _lastKey = np.Key;
            if (np.HasTrack)
            {
                np = await _media.GetNowPlayingAsync(includeThumbnail: true, ct);
                _activeLyrics = await _lyrics.GetAsync(np.Title, np.Artist, np.Duration, ct);
                HasLyrics = !_activeLyrics.IsEmpty;
                _ = _history.AddAsync(np.Title, np.Artist, HasLyrics);
            }
            else
            {
                _activeLyrics = new SyncedLyrics(Array.Empty<LyricLine>());
                HasLyrics = false;
            }
        }
        else if (Current.Thumbnail is not null)
        {
            np = np with { Thumbnail = Current.Thumbnail };
        }

        Current = np;

        if (HasLyrics)
        {
            var lead = TimeSpan.FromSeconds(settings.LineLeadSeconds);
            (PrevLine, CurrentLine, NextLine) = _activeLyrics.Window(np.Position + lead);
        }
        else
        {
            PrevLine = CurrentLine = NextLine = string.Empty;
        }

        StatusText = ComposeStatus(np, settings);
        await _discord.SetStatusAsync(StatusText, settings.EmojiName, ct);

        Updated?.Invoke();
    }

    private string? ComposeStatus(NowPlaying np, AppSettings s)
    {
        if (!np.HasTrack) return s.ClearOnPause ? string.Empty : StatusText;
        if (!np.IsPlaying && s.ClearOnPause) return string.Empty;

        string? line = null;
        if (HasLyrics && !string.IsNullOrWhiteSpace(CurrentLine))
            line = s.StatusPrefix + CurrentLine;
        else if (s.ShowSongWhenNoLyrics)
            line = $"{s.StatusPrefix}{np.Title} — {np.Artist}".TrimEnd(' ', '—');

        if (string.IsNullOrWhiteSpace(line)) return string.Empty;
        return line.Length > s.MaxStatusLength ? line[..s.MaxStatusLength] : line;
    }
}
