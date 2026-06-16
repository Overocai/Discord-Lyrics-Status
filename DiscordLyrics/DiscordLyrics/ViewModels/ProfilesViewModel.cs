using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Models;
using DiscordLyrics.Services;
using DiscordLyrics.Storage;
using Microsoft.EntityFrameworkCore;

namespace DiscordLyrics.ViewModels;

public sealed partial class ProfilesViewModel : ObservableObject, IActivatable
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly INotificationService _notify;

    [ObservableProperty] private DiscordProfile? _selected;

    public ObservableCollection<DiscordProfile> Profiles { get; } = new();

    public ProfilesViewModel(IDbContextFactory<AppDbContext> dbFactory, INotificationService notify)
    {
        _dbFactory = dbFactory;
        _notify = notify;
    }

    public async void OnActivated() => await LoadAsync();

    private async Task LoadAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var items = await db.Profiles.AsNoTracking().OrderBy(p => p.Id).ToListAsync();
        Profiles.Clear();
        foreach (var p in items) Profiles.Add(p);
        Selected = Profiles.FirstOrDefault(p => p.IsActive) ?? Profiles.FirstOrDefault();
    }

    [RelayCommand]
    private async Task Add()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var p = new DiscordProfile { Name = $"Profile {Profiles.Count + 1}" };
        db.Profiles.Add(p);
        await db.SaveChangesAsync();
        await LoadAsync();
        _notify.Success("Profile created.");
    }

    [RelayCommand]
    private async Task Activate(DiscordProfile? profile)
    {
        if (profile is null) return;
        await using var db = await _dbFactory.CreateDbContextAsync();
        foreach (var row in db.Profiles) row.IsActive = row.Id == profile.Id;
        await db.SaveChangesAsync();
        await LoadAsync();
        _notify.Info($"'{profile.Name}' is now active.");
    }

    [RelayCommand]
    private async Task Delete(DiscordProfile? profile)
    {
        if (profile is null || Profiles.Count <= 1) return;
        await using var db = await _dbFactory.CreateDbContextAsync();
        var row = await db.Profiles.FindAsync(profile.Id);
        if (row is not null) { db.Profiles.Remove(row); await db.SaveChangesAsync(); }
        await LoadAsync();
    }
}
