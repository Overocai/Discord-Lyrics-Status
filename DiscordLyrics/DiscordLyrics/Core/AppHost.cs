using System.IO;
using System.Net.Http;
using DiscordLyrics.Infrastructure.Navigation;
using DiscordLyrics.Services;
using DiscordLyrics.Storage;
using DiscordLyrics.ViewModels;
using DiscordLyrics.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace DiscordLyrics.Core;

/// <summary>Composition root: every dependency the app uses is registered here.</summary>
public static class AppHost
{
    public static IHost Build()
    {
        AppPaths.EnsureCreated();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppPaths.LogsDirectory, "log-.txt"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        return Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) =>
            {
                // Persistence — factory pattern so singleton services can create
                // a short-lived context per unit of work (no captive dependency).
                services.AddDbContextFactory<AppDbContext>(o =>
                    o.UseSqlite($"Data Source={AppPaths.DatabaseFile}"));

                // Networking
                services.AddHttpClient("lyrics", c =>
                {
                    c.BaseAddress = new Uri("https://lrclib.net/");
                    c.DefaultRequestHeaders.UserAgent.ParseAdd("DiscordLyrics/1.0 (+https://github.com/Overocai)");
                    c.Timeout = TimeSpan.FromSeconds(12);
                });
                services.AddHttpClient("discord", c =>
                {
                    c.BaseAddress = new Uri("https://discord.com/");
                    c.Timeout = TimeSpan.FromSeconds(12);
                });
                services.AddHttpClient("updates");

                // Core services (singletons: they hold long-lived state / background loops)
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IMediaService, MediaService>();
                services.AddSingleton<ILyricsService, LyricsService>();
                services.AddSingleton<IDiscordService, DiscordService>();
                services.AddSingleton<INotificationService, NotificationService>();
                services.AddSingleton<IHistoryService, HistoryService>();
                services.AddSingleton<IUpdateService, UpdateService>();
                services.AddSingleton<IStatusEngine, StatusEngine>();

                // Navigation
                services.AddSingleton<INavigationService, NavigationService>();

                // ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<LyricsViewModel>();
                services.AddSingleton<DiscordViewModel>();
                services.AddSingleton<ProfilesViewModel>();
                services.AddSingleton<HistoryViewModel>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<LogsViewModel>();
                services.AddSingleton<AboutViewModel>();

                // Views
                services.AddSingleton<MainWindow>();
            })
            .Build();
    }
}
