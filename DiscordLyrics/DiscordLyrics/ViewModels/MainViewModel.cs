using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure.Navigation;

namespace DiscordLyrics.ViewModels;

/// <summary>Shell view-model: owns navigation and the active page.</summary>
public sealed partial class MainViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    [ObservableProperty] private ObservableObject? _currentPage;
    [ObservableProperty] private string _activeSection = "dashboard";

    public MainViewModel(INavigationService navigation)
    {
        _navigation = navigation;
        _navigation.CurrentChanged += () => CurrentPage = _navigation.Current;
        Navigate("dashboard");
    }

    [RelayCommand]
    private void Navigate(string section)
    {
        ActiveSection = section;
        switch (section)
        {
            case "dashboard": _navigation.NavigateTo<DashboardViewModel>(); break;
            case "lyrics": _navigation.NavigateTo<LyricsViewModel>(); break;
            case "discord": _navigation.NavigateTo<DiscordViewModel>(); break;
            case "profiles": _navigation.NavigateTo<ProfilesViewModel>(); break;
            case "history": _navigation.NavigateTo<HistoryViewModel>(); break;
            case "settings": _navigation.NavigateTo<SettingsViewModel>(); break;
            case "logs": _navigation.NavigateTo<LogsViewModel>(); break;
            case "about": _navigation.NavigateTo<AboutViewModel>(); break;
        }
    }
}
