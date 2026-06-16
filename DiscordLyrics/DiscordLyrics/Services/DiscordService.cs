using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace DiscordLyrics.Services;

/// <summary>
/// Minimal Discord REST client for driving the account's custom status.
/// Uses the user token directly (selfbot behaviour — for educational use only).
/// </summary>
public sealed class DiscordService : IDiscordService
{
    private const string ApiBase = "api/v9/";

    private readonly IHttpClientFactory _http;
    private readonly ISettingsService _settings;
    private readonly ILogger<DiscordService> _log;

    private string? _lastText = "\0";       // sentinel so the first empty status still sends
    private DateTimeOffset _cooldownUntil = DateTimeOffset.MinValue;

    public DiscordService(IHttpClientFactory http, ISettingsService settings, ILogger<DiscordService> log)
    {
        _http = http;
        _settings = settings;
        _log = log;
    }

    public bool Connected { get; private set; }
    public DiscordUser? Account { get; private set; }
    public event Action? StateChanged;

    public async Task<DiscordUser?> ValidateAsync(string? token = null, CancellationToken ct = default)
    {
        token ??= _settings.GetToken();
        if (string.IsNullOrWhiteSpace(token)) { SetState(false, null); return null; }

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, ApiBase + "users/@me");
            req.Headers.TryAddWithoutValidation("Authorization", token);
            using var resp = await _http.CreateClient("discord").SendAsync(req, ct);

            if (!resp.IsSuccessStatusCode) { SetState(false, null); return null; }

            var me = await resp.Content.ReadFromJsonAsync<MeResponse>(cancellationToken: ct);
            var user = me is null ? null : new DiscordUser(me.Id ?? "", me.Username ?? "", me.GlobalName);
            SetState(user is not null, user);
            return user;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Discord validation failed");
            SetState(false, null);
            return null;
        }
    }

    public async Task SetStatusAsync(string? text, string? emojiName, CancellationToken ct = default)
    {
        var token = _settings.GetToken();
        if (string.IsNullOrWhiteSpace(token)) return;

        var normalized = text ?? string.Empty;
        if (normalized == _lastText) return;                 // de-dupe
        if (DateTimeOffset.UtcNow < _cooldownUntil) return;  // rate-limit backoff

        object? customStatus = string.IsNullOrEmpty(normalized)
            ? null
            : new { text = normalized, emoji_name = string.IsNullOrWhiteSpace(emojiName) ? null : emojiName };

        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Patch, ApiBase + "users/@me/settings")
            {
                Content = JsonContent.Create(new { custom_status = customStatus })
            };
            req.Headers.TryAddWithoutValidation("Authorization", token);

            using var resp = await _http.CreateClient("discord").SendAsync(req, ct);

            if (resp.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retry = resp.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(5);
                _cooldownUntil = DateTimeOffset.UtcNow + retry;
                _log.LogWarning("Discord rate-limited, backing off {Seconds}s", retry.TotalSeconds);
                return;
            }

            resp.EnsureSuccessStatusCode();
            _lastText = normalized;
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "Set status failed");
        }
    }

    public Task ClearStatusAsync(CancellationToken ct = default) => SetStatusAsync(string.Empty, null, ct);

    private void SetState(bool connected, DiscordUser? account)
    {
        Connected = connected;
        Account = account;
        StateChanged?.Invoke();
    }

    private sealed class MeResponse
    {
        [JsonPropertyName("id")] public string? Id { get; set; }
        [JsonPropertyName("username")] public string? Username { get; set; }
        [JsonPropertyName("global_name")] public string? GlobalName { get; set; }
    }
}
