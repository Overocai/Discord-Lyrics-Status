namespace DiscordLyrics.Services;

public sealed record DiscordUser(string Id, string Username, string? GlobalName)
{
    public string Display => string.IsNullOrWhiteSpace(GlobalName) ? Username : GlobalName!;
}

public interface IDiscordService
{
    bool Connected { get; }
    DiscordUser? Account { get; }
    event Action? StateChanged;

    /// <summary>Validate a token (or the stored one). Returns the account on success, null otherwise.</summary>
    Task<DiscordUser?> ValidateAsync(string? token = null, CancellationToken ct = default);

    /// <summary>Set the custom status. Null text clears it. De-duplicates and honours 429 cooldown.</summary>
    Task SetStatusAsync(string? text, string? emojiName, CancellationToken ct = default);

    Task ClearStatusAsync(CancellationToken ct = default);
}
