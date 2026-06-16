using System.IO;

namespace DiscordLyrics.Core;

/// <summary>Centralised, per-user writable locations (never write next to the .exe).</summary>
public static class AppPaths
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DiscordLyrics");

    public static string DatabaseFile => Path.Combine(Root, "discordlyrics.db");
    public static string LogsDirectory => Path.Combine(Root, "logs");
    public static string CacheDirectory => Path.Combine(Root, "cache");

    public static void EnsureCreated()
    {
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(LogsDirectory);
        Directory.CreateDirectory(CacheDirectory);
    }
}
