using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Models;
using DiscordLyrics.Services;

namespace DiscordLyrics.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject, IActivatable
{
    private readonly ISettingsService _settings;
    private readonly INotificationService _notify;

    [ObservableProperty] private string _statusPrefix = "♪ ";
    [ObservableProperty] private string _emojiName = string.Empty;
    [ObservableProperty] private double _pollIntervalSeconds = 0.5;
    [ObservableProperty] private double _lineLeadSeconds = 0.4;
    [ObservableProperty] private int _maxStatusLength = 128;
    [ObservableProperty] private bool _showSongWhenNoLyrics = true;
    [ObservableProperty] private bool _clearOnPause = true;
    [ObservableProperty] private bool _syncedOnly = true;
    [ObservableProperty] private bool _autoStartEngine = true;
    [ObservableProperty] private bool _launchAtStartup;

    public SettingsViewModel(ISettingsService settings, INotificationService notify)
    {
        _settings = settings;
        _notify = notify;
    }

    public void OnActivated()
    {
        var s = _settings.Current;
        StatusPrefix = s.StatusPrefix;
        EmojiName = s.EmojiName;
        PollIntervalSeconds = s.PollIntervalSeconds;
        LineLeadSeconds = s.LineLeadSeconds;
        MaxStatusLength = s.MaxStatusLength;
        ShowSongWhenNoLyrics = s.ShowSongWhenNoLyrics;
        ClearOnPause = s.ClearOnPause;
        SyncedOnly = s.SyncedOnly;
        AutoStartEngine = s.AutoStartEngine;
        LaunchAtStartup = StartupManager.IsEnabled();
    }

    [RelayCommand]
    private async Task Save()
    {
        var s = new AppSettings
        {
            StatusPrefix = StatusPrefix,
            EmojiName = EmojiName,
            PollIntervalSeconds = Math.Clamp(PollIntervalSeconds, 0.25, 5),
            LineLeadSeconds = Math.Clamp(LineLeadSeconds, 0, 3),
            MaxStatusLength = Math.Clamp(MaxStatusLength, 16, 128),
            ShowSongWhenNoLyrics = ShowSongWhenNoLyrics,
            ClearOnPause = ClearOnPause,
            SyncedOnly = SyncedOnly,
            AutoStartEngine = AutoStartEngine,
            LaunchAtStartup = LaunchAtStartup,
        };
        await _settings.SaveAsync(s);
        StartupManager.Set(LaunchAtStartup);
        _notify.Success("Settings saved.");
    }
}
