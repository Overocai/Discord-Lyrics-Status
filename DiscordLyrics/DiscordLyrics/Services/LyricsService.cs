using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using DiscordLyrics.Core;
using DiscordLyrics.Infrastructure.Text;
using DiscordLyrics.Models;
using Microsoft.Extensions.Logging;

namespace DiscordLyrics.Services;

/// <summary>Fetches time-synced lyrics from LRCLIB (https://lrclib.net), with disk + memory cache.</summary>
public sealed class LyricsService : ILyricsService
{
    private readonly IHttpClientFactory _http;
    private readonly ILogger<LyricsService> _log;
    private readonly ConcurrentDictionary<string, SyncedLyrics> _memory = new();

    public LyricsService(IHttpClientFactory http, ILogger<LyricsService> log)
    {
        _http = http;
        _log = log;
    }

    public async Task<SyncedLyrics> GetAsync(string title, string artist, TimeSpan duration, CancellationToken ct = default)
    {
        var cleanTitle = TrackCleaner.Title(title);
        var cleanArtist = TrackCleaner.Artist(artist);
        var key = Hash($"{cleanTitle}|{cleanArtist}");

        if (_memory.TryGetValue(key, out var cached)) return cached;

        var fromDisk = ReadCache(key);
        if (fromDisk is not null)
        {
            _memory[key] = fromDisk;
            return fromDisk;
        }

        var client = _http.CreateClient("lyrics");
        var lrc =
            await TryGetAsync(client, cleanTitle, cleanArtist, duration, ct)
            ?? await TrySearchAsync(client, cleanTitle, cleanArtist, ct)
            ?? await TrySearchAsync(client, title, artist, ct);

        var result = LrcParser.Parse(lrc);
        if (!result.IsEmpty)
        {
            _memory[key] = result;
            WriteCache(key, lrc!);
        }
        return result;
    }

    private async Task<string?> TryGetAsync(HttpClient c, string title, string artist, TimeSpan dur, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        try
        {
            var url = $"api/get?track_name={Uri.EscapeDataString(title)}" +
                      $"&artist_name={Uri.EscapeDataString(artist)}" +
                      (dur.TotalSeconds > 1 ? $"&duration={(int)dur.TotalSeconds}" : "");
            var resp = await c.GetAsync(url, ct);
            if (resp.StatusCode == HttpStatusCode.NotFound) return null;
            resp.EnsureSuccessStatusCode();
            var item = await resp.Content.ReadFromJsonAsync<LrcLibItem>(cancellationToken: ct);
            return NonEmpty(item?.SyncedLyrics);
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "lrclib get failed");
            return null;
        }
    }

    private async Task<string?> TrySearchAsync(HttpClient c, string title, string artist, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(title)) return null;
        try
        {
            var url = $"api/search?track_name={Uri.EscapeDataString(title)}" +
                      $"&artist_name={Uri.EscapeDataString(artist)}";
            var items = await c.GetFromJsonAsync<List<LrcLibItem>>(url, ct);
            var hit = items?.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.SyncedLyrics));
            return hit?.SyncedLyrics;
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "lrclib search failed");
            return null;
        }
    }

    private static string? NonEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private static string Hash(string s)
    {
        var bytes = SHA1.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes);
    }

    private static string CachePath(string key) => Path.Combine(AppPaths.CacheDirectory, key + ".lrc");

    private SyncedLyrics? ReadCache(string key)
    {
        try
        {
            var path = CachePath(key);
            return File.Exists(path) ? LrcParser.Parse(File.ReadAllText(path)) : null;
        }
        catch { return null; }
    }

    private void WriteCache(string key, string lrc)
    {
        try { File.WriteAllText(CachePath(key), lrc); }
        catch (Exception ex) { _log.LogDebug(ex, "lyrics cache write failed"); }
    }

    private sealed class LrcLibItem
    {
        [JsonPropertyName("syncedLyrics")] public string? SyncedLyrics { get; set; }
        [JsonPropertyName("plainLyrics")] public string? PlainLyrics { get; set; }
    }
}
