using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DiscordLyrics.Services;

/// <summary>
/// Polls the GitHub Releases API and compares the latest tag with the running
/// assembly version. Download/apply is handled by the installer (see Updater/ docs).
/// </summary>
public sealed class UpdateService : IUpdateService
{
    private const string LatestReleaseUrl =
        "https://api.github.com/repos/Overocai/Discord-Lyrics-Status/releases/latest";

    private readonly IHttpClientFactory _http;
    private readonly ILogger<UpdateService> _log;

    public UpdateService(IHttpClientFactory http, ILogger<UpdateService> log)
    {
        _http = http;
        _log = log;
    }

    public string CurrentVersion =>
        Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

    public async Task<UpdateInfo?> CheckAsync(CancellationToken ct = default)
    {
        try
        {
            var client = _http.CreateClient("updates");
            using var req = new HttpRequestMessage(HttpMethod.Get, LatestReleaseUrl);
            req.Headers.UserAgent.ParseAdd("DiscordLyrics-Updater");
            req.Headers.Accept.ParseAdd("application/vnd.github+json");

            using var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return null;

            var rel = await resp.Content.ReadFromJsonAsync<Release>(cancellationToken: ct);
            if (rel?.TagName is null) return null;

            var latest = ParseVersion(rel.TagName);
            var current = Version.Parse(CurrentVersion);
            if (latest > current)
                return new UpdateInfo(latest.ToString(), rel.HtmlUrl ?? "", rel.Body ?? "");

            return null;
        }
        catch (Exception ex)
        {
            _log.LogDebug(ex, "Update check failed");
            return null;
        }
    }

    private static Version ParseVersion(string tag)
    {
        var cleaned = tag.TrimStart('v', 'V').Trim();
        return Version.TryParse(cleaned, out var v) ? v : new Version(0, 0, 0);
    }

    private sealed class Release
    {
        [JsonPropertyName("tag_name")] public string? TagName { get; set; }
        [JsonPropertyName("html_url")] public string? HtmlUrl { get; set; }
        [JsonPropertyName("body")] public string? Body { get; set; }
    }
}
