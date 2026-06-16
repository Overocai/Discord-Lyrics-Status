using System.Security.Cryptography;
using System.Text;
using DiscordLyrics.Models;
using DiscordLyrics.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscordLyrics.Services;

/// <summary>Loads/saves <see cref="AppSettings"/> and the Discord token from SQLite.</summary>
public sealed class SettingsService : ISettingsService
{
    private const string TokenKey = "discord.token";

    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly ILogger<SettingsService> _log;
    private string? _tokenCache;

    public SettingsService(IDbContextFactory<AppDbContext> dbFactory, ILogger<SettingsService> log)
    {
        _dbFactory = dbFactory;
        _log = log;
    }

    public AppSettings Current { get; private set; } = new();
    public event Action? Changed;
    public bool HasToken => !string.IsNullOrWhiteSpace(GetToken());

    public async Task LoadAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var map = await db.Settings.AsNoTracking().ToDictionaryAsync(s => s.Key, s => s.Value);

        var s = new AppSettings();
        if (map.TryGetValue("statusPrefix", out var v)) s.StatusPrefix = v;
        if (map.TryGetValue("emojiName", out v)) s.EmojiName = v;
        if (map.TryGetValue("pollInterval", out v) && double.TryParse(v, out var d)) s.PollIntervalSeconds = d;
        if (map.TryGetValue("lineLead", out v) && double.TryParse(v, out d)) s.LineLeadSeconds = d;
        if (map.TryGetValue("maxLen", out v) && int.TryParse(v, out var i)) s.MaxStatusLength = i;
        if (map.TryGetValue("showSong", out v) && bool.TryParse(v, out var b)) s.ShowSongWhenNoLyrics = b;
        if (map.TryGetValue("clearOnPause", out v) && bool.TryParse(v, out b)) s.ClearOnPause = b;
        if (map.TryGetValue("syncedOnly", out v) && bool.TryParse(v, out b)) s.SyncedOnly = b;
        if (map.TryGetValue("autoStart", out v) && bool.TryParse(v, out b)) s.AutoStartEngine = b;
        if (map.TryGetValue("launchAtStartup", out v) && bool.TryParse(v, out b)) s.LaunchAtStartup = b;

        Current = s;
        Changed?.Invoke();
    }

    public async Task SaveAsync(AppSettings s)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        await UpsertAsync(db, "statusPrefix", s.StatusPrefix);
        await UpsertAsync(db, "emojiName", s.EmojiName);
        await UpsertAsync(db, "pollInterval", s.PollIntervalSeconds.ToString());
        await UpsertAsync(db, "lineLead", s.LineLeadSeconds.ToString());
        await UpsertAsync(db, "maxLen", s.MaxStatusLength.ToString());
        await UpsertAsync(db, "showSong", s.ShowSongWhenNoLyrics.ToString());
        await UpsertAsync(db, "clearOnPause", s.ClearOnPause.ToString());
        await UpsertAsync(db, "syncedOnly", s.SyncedOnly.ToString());
        await UpsertAsync(db, "autoStart", s.AutoStartEngine.ToString());
        await UpsertAsync(db, "launchAtStartup", s.LaunchAtStartup.ToString());
        await db.SaveChangesAsync();

        Current = s.Clone();
        Changed?.Invoke();
    }

    public string? GetToken()
    {
        if (_tokenCache is not null) return _tokenCache;
        try
        {
            using var db = _dbFactory.CreateDbContext();
            var row = db.Settings.AsNoTracking().FirstOrDefault(x => x.Key == TokenKey);
            if (row is null || string.IsNullOrEmpty(row.Value)) return null;
            _tokenCache = Unprotect(row.Value);
            return _tokenCache;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Could not read token");
            return null;
        }
    }

    public async Task SetTokenAsync(string token)
    {
        token = token.Trim();
        await using var db = await _dbFactory.CreateDbContextAsync();
        await UpsertAsync(db, TokenKey, Protect(token));
        await db.SaveChangesAsync();
        _tokenCache = token;
        Changed?.Invoke();
    }

    private static async Task UpsertAsync(AppDbContext db, string key, string value)
    {
        var row = await db.Settings.FindAsync(key);
        if (row is null) db.Settings.Add(new SettingEntry { Key = key, Value = value });
        else row.Value = value;
    }

    // ---- DPAPI (per-user) ----
    private static string Protect(string plain)
    {
        var bytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(plain), null, DataProtectionScope.CurrentUser);
        return Convert.ToBase64String(bytes);
    }

    private static string Unprotect(string encoded)
    {
        var bytes = ProtectedData.Unprotect(Convert.FromBase64String(encoded), null, DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(bytes);
    }
}
