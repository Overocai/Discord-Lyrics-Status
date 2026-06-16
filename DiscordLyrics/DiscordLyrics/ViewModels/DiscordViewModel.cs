using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Services;

namespace DiscordLyrics.ViewModels;

public sealed partial class DiscordViewModel : ObservableObject, IActivatable
{
    private readonly ISettingsService _settings;
    private readonly IDiscordService _discord;
    private readonly INotificationService _notify;

    [ObservableProperty] private string _token = string.Empty;
    [ObservableProperty] private bool _connected;
    [ObservableProperty] private string _accountName = "Not connected";
    [ObservableProperty] private string _accountId = string.Empty;
    [ObservableProperty] private bool _hasStoredToken;
    [ObservableProperty] private bool _busy;

    public DiscordViewModel(ISettingsService settings, IDiscordService discord, INotificationService notify)
    {
        _settings = settings;
        _discord = discord;
        _notify = notify;
        _discord.StateChanged += Refresh;
    }

    public void OnActivated()
    {
        HasStoredToken = _settings.HasToken;
        Refresh();
    }

    [RelayCommand]
    private async Task Connect()
    {
        if (string.IsNullOrWhiteSpace(Token) || Token.Trim().Length < 20)
        {
            _notify.Warning("Paste your full Discord token.");
            return;
        }

        Busy = true;
        await _settings.SetTokenAsync(Token.Trim());
        var user = await _discord.ValidateAsync(Token.Trim());
        Busy = false;

        if (user is null)
        {
            _notify.Error("Token rejected by Discord. Check it and try again.");
            return;
        }

        Token = string.Empty;
        HasStoredToken = true;
        _notify.Success($"Connected as {user.Display}.");
    }

    [RelayCommand]
    private async Task Disconnect()
    {
        await _settings.SetTokenAsync(string.Empty);
        await _discord.ValidateAsync(string.Empty);
        HasStoredToken = false;
        _notify.Info("Token removed from this device.");
    }

    private void Refresh()
    {
        var d = Application.Current?.Dispatcher;
        if (d is null) return;
        d.Invoke(() =>
        {
            Connected = _discord.Connected;
            AccountName = _discord.Account?.Display ?? "Not connected";
            AccountId = _discord.Account?.Id ?? string.Empty;
        });
    }
}
