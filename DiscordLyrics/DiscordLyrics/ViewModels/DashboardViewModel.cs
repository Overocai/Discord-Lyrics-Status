using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Services;

namespace DiscordLyrics.ViewModels;

public sealed partial class DashboardViewModel : ObservableObject, IActivatable
{
    private readonly IStatusEngine _engine;
    private readonly IDiscordService _discord;
    private readonly IHistoryService _history;
    private readonly INotificationService _notify;
    private readonly ISettingsService _settings;
    private string _artKey = string.Empty;

    [ObservableProperty] private string _trackTitle = "Nothing playing";
    [ObservableProperty] private string _trackArtist = string.Empty;
    [ObservableProperty] private string _positionText = "0:00";
    [ObservableProperty] private string _durationText = "0:00";
    [ObservableProperty] private double _progress;
    [ObservableProperty] private string _prevLine = string.Empty;
    [ObservableProperty] private string _currentLine = string.Empty;
    [ObservableProperty] private string _nextLine = string.Empty;
    [ObservableProperty] private bool _hasLyrics;
    [ObservableProperty] private string _lyricsHint = "Start the engine to see lyrics here.";
    [ObservableProperty] private bool _isPlaying;
    [ObservableProperty] private string _statusText = string.Empty;
    [ObservableProperty] private bool _connected;
    [ObservableProperty] private string _accountName = "Not connected";
    [ObservableProperty] private ImageSource? _art;
    [ObservableProperty] private bool _engineRunning;
    [ObservableProperty] private int _tracksLogged;

    public string ToggleLabel => EngineRunning ? "Stop" : "Start";

    public DashboardViewModel(
        IStatusEngine engine, IDiscordService discord, IHistoryService history,
        INotificationService notify, ISettingsService settings)
    {
        _engine = engine;
        _discord = discord;
        _history = history;
        _notify = notify;
        _settings = settings;

        _engine.Updated += OnEngineUpdated;
        _discord.StateChanged += OnDiscordChanged;
    }

    public async void OnActivated()
    {
        OnDiscordChanged();
        EngineRunning = _engine.Running;
        OnPropertyChanged(nameof(ToggleLabel));
        TracksLogged = await _history.CountAsync();
        if (!_discord.Connected) await _discord.ValidateAsync();
    }

    [RelayCommand]
    private async Task ToggleEngine()
    {
        if (_engine.Running)
        {
            await _engine.StopAsync();
            _notify.Info("Engine stopped.");
        }
        else
        {
            if (!_settings.HasToken)
            {
                _notify.Warning("Add your Discord token in the Discord tab first.");
                return;
            }
            await _engine.StartAsync();
            _notify.Success("Engine started — your status will follow the music.");
        }
        EngineRunning = _engine.Running;
        OnPropertyChanged(nameof(ToggleLabel));
    }

    private void OnEngineUpdated()
    {
        var d = Application.Current?.Dispatcher;
        if (d is null) return;
        d.Invoke(() =>
        {
            var np = _engine.Current;
            TrackTitle = np.HasTrack ? np.Title : "Nothing playing";
            TrackArtist = np.Artist;
            IsPlaying = np.IsPlaying;
            PositionText = Fmt(np.Position);
            DurationText = Fmt(np.Duration);
            Progress = np.Duration.TotalSeconds > 0
                ? Math.Clamp(np.Position.TotalSeconds / np.Duration.TotalSeconds, 0, 1) : 0;

            HasLyrics = _engine.HasLyrics;
            PrevLine = _engine.PrevLine;
            CurrentLine = _engine.CurrentLine;
            NextLine = _engine.NextLine;
            LyricsHint = _engine.Running
                ? (np.HasTrack ? "No synced lyrics found for this track." : "Play something to begin.")
                : "Start the engine to see lyrics here.";
            StatusText = _engine.StatusText ?? string.Empty;
            EngineRunning = _engine.Running;
            OnPropertyChanged(nameof(ToggleLabel));

            if (np.Key != _artKey)
            {
                _artKey = np.Key;
                Art = ImageHelper.FromBytes(np.Thumbnail);
            }
        });
    }

    private void OnDiscordChanged()
    {
        var d = Application.Current?.Dispatcher;
        if (d is null) return;
        d.Invoke(() =>
        {
            Connected = _discord.Connected;
            AccountName = _discord.Account?.Display ?? "Not connected";
        });
    }

    private static string Fmt(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:00}";
}
