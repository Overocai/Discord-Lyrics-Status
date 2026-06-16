using DiscordLyrics.Models;
using DiscordLyrics.Storage;
using Microsoft.EntityFrameworkCore;

namespace DiscordLyrics.Services;

public sealed class HistoryService : IHistoryService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public HistoryService(IDbContextFactory<AppDbContext> dbFactory) => _dbFactory = dbFactory;

    public async Task AddAsync(string title, string artist, bool hadLyrics)
    {
        if (string.IsNullOrWhiteSpace(title)) return;
        await using var db = await _dbFactory.CreateDbContextAsync();

        // Skip if the most recent entry is the same track (avoids duplicates per poll).
        // Order by Id (insertion order): SQLite cannot ORDER BY a DateTimeOffset column.
        var last = await db.History.OrderByDescending(h => h.Id).FirstOrDefaultAsync();
        if (last is not null &&
            string.Equals(last.Title, title, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(last.Artist, artist, StringComparison.OrdinalIgnoreCase))
            return;

        db.History.Add(new PlayHistoryEntry { Title = title, Artist = artist, HadLyrics = hadLyrics });
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PlayHistoryEntry>> RecentAsync(int take = 100)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.History.AsNoTracking()
            .OrderByDescending(h => h.Id).Take(take).ToListAsync();
    }

    public async Task<int> CountAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.History.CountAsync();
    }

    public async Task ClearAsync()
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        await db.History.ExecuteDeleteAsync();
    }
}
