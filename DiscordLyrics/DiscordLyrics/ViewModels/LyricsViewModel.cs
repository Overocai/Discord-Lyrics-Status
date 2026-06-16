using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Services;

namespace DiscordLyrics.ViewModels;

/// <summary>Full-screen karaoke view of the synced lyrics.</summary>
public sealed partial class LyricsViewModel : ObservableObject, IActivatable
{
    private readonly IStatusEngine _engine;

    [ObservableProperty] private string _trackTitle = "Nothing playing";
    [ObservableProperty] private string _trackArtist = string.Empty;
    [ObservableProperty] private string _prevLine = string.Empty;
    [ObservableProperty] private string _currentLine = string.Empty;
    [ObservableProperty] private string _nextLine = string.Empty;
    [ObservableProperty] private bool _hasLyrics;
    [ObservableProperty] private string _emptyHint = "Start the engine to see lyrics here.";

    public LyricsViewModel(IStatusEngine engine)
    {
        _engine = engine;
        _engine.Updated += OnUpdated;
    }

    public void OnActivated() => OnUpdated();

    private void OnUpdated()
    {
        var d = Application.Current?.Dispatcher;
        if (d is null) return;
        d.Invoke(() =>
        {
            var np = _engine.Current;
            TrackTitle = np.HasTrack ? np.Title : "Nothing playing";
            TrackArtist = np.Artist;
            HasLyrics = _engine.HasLyrics;
            PrevLine = _engine.PrevLine;
            CurrentLine = _engine.CurrentLine;
            NextLine = _engine.NextLine;
            EmptyHint = _engine.Running
                ? (np.HasTrack ? "No synced lyrics found for this track." : "Play something to begin.")
                : "Start the engine to see lyrics here.";
        });
    }
}
