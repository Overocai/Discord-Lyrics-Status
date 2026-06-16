using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Models;
using DiscordLyrics.Services;

namespace DiscordLyrics.ViewModels;

public sealed partial class HistoryViewModel : ObservableObject, IActivatable
{
    private readonly IHistoryService _history;
    private readonly INotificationService _notify;

    [ObservableProperty] private int _total;
    [ObservableProperty] private bool _isEmpty = true;

    public ObservableCollection<PlayHistoryEntry> Entries { get; } = new();

    public HistoryViewModel(IHistoryService history, INotificationService notify)
    {
        _history = history;
        _notify = notify;
    }

    public async void OnActivated() => await LoadAsync();

    private async Task LoadAsync()
    {
        var items = await _history.RecentAsync(200);
        Entries.Clear();
        foreach (var e in items) Entries.Add(e);
        Total = await _history.CountAsync();
        IsEmpty = Entries.Count == 0;
    }

    [RelayCommand]
    private async Task Refresh() => await LoadAsync();

    [RelayCommand]
    private async Task Clear()
    {
        await _history.ClearAsync();
        await LoadAsync();
        _notify.Info("History cleared.");
    }
}
