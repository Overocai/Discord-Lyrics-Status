using System.Diagnostics;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DiscordLyrics.Core;
using DiscordLyrics.Infrastructure.Navigation;

namespace DiscordLyrics.ViewModels;

public sealed partial class LogsViewModel : ObservableObject, IActivatable
{
    [ObservableProperty] private string _logText = "No logs yet.";
    [ObservableProperty] private string _fileName = string.Empty;

    public void OnActivated() => Load();

    [RelayCommand]
    private void Refresh() => Load();

    [RelayCommand]
    private void OpenFolder()
    {
        try { Process.Start(new ProcessStartInfo(AppPaths.LogsDirectory) { UseShellExecute = true }); }
        catch { /* ignore */ }
    }

    private void Load()
    {
        try
        {
            var latest = new DirectoryInfo(AppPaths.LogsDirectory)
                .GetFiles("log-*.txt")
                .OrderByDescending(f => f.LastWriteTimeUtc)
                .FirstOrDefault();

            if (latest is null) { LogText = "No logs yet."; FileName = string.Empty; return; }

            FileName = latest.Name;
            // Read the tail (last ~400 lines) without locking the writer.
            using var fs = new FileStream(latest.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(fs);
            var lines = reader.ReadToEnd().Split('\n');
            LogText = string.Join('\n', lines.TakeLast(400)).Trim();
        }
        catch (Exception ex)
        {
            LogText = "Could not read log file: " + ex.Message;
        }
    }
}
