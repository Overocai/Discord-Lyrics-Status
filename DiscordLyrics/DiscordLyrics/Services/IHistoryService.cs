using DiscordLyrics.Models;

namespace DiscordLyrics.Services;

public interface IHistoryService
{
    Task AddAsync(string title, string artist, bool hadLyrics);
    Task<IReadOnlyList<PlayHistoryEntry>> RecentAsync(int take = 100);
    Task<int> CountAsync();
    Task ClearAsync();
}
