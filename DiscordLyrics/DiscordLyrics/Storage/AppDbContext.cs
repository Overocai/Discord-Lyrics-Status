using DiscordLyrics.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscordLyrics.Storage;

/// <summary>EF Core / SQLite context for local persistence.</summary>
public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlayHistoryEntry> History => Set<PlayHistoryEntry>();
    public DbSet<DiscordProfile> Profiles => Set<DiscordProfile>();
    public DbSet<SettingEntry> Settings => Set<SettingEntry>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<PlayHistoryEntry>().HasIndex(x => x.PlayedAt);
        b.Entity<DiscordProfile>().HasData(new DiscordProfile
        {
            Id = 1, Name = "Default", StatusPrefix = "♪ ", IsActive = true
        });
    }
}
