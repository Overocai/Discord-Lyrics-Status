using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Services;

namespace DiscordLyrics.ViewModels;

public sealed partial class AboutViewModel : ObservableObject, IActivatable
{
    private readonly IUpdateService _updates;
    private readonly INotificationService _notify;

    [ObservableProperty] private string _version = "1.0.0";
    [ObservableProperty] private string _updateStatus = string.Empty;
    [ObservableProperty] private bool _checking;

    public string Author => "Overocai";
    public string RepoUrl => "https://github.com/Overocai/Discord-Lyrics-Status";
    public string DiscordUrl => "https://discord.com/users/1288832011452153910";

    public AboutViewModel(IUpdateService updates, INotificationService notify)
    {
        _updates = updates;
        _notify = notify;
        Version = _updates.CurrentVersion;
    }

    public void OnActivated() => Version = _updates.CurrentVersion;

    [RelayCommand]
    private async Task CheckUpdates()
    {
        Checking = true;
        UpdateStatus = "Checking…";
        var info = await _updates.CheckAsync();
        Checking = false;

        if (info is null)
        {
            UpdateStatus = "You're on the latest version.";
            _notify.Success("You're up to date.");
        }
        else
        {
            UpdateStatus = $"Update available: v{info.Version}";
            _notify.Info($"Version {info.Version} is available.");
        }
    }

    [RelayCommand]
    private void OpenUrl(string url)
    {
        try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
        catch { /* ignore */ }
    }
}
