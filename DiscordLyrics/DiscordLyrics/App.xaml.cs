using System.Windows;
using System.Windows.Threading;
using DiscordLyrics.Core;
using DiscordLyrics.Storage;
using DiscordLyrics.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DiscordLyrics;

/// <summary>
/// Application entry point. Builds the generic host (DI + logging + EF),
/// applies the database migrations and shows the shell window.
/// </summary>
public partial class App : Application
{
    private IHost? _host;

    /// <summary>Global service provider, available to code-behind that cannot use constructor injection.</summary>
    public static IServiceProvider Services =>
        ((App)Current)._host?.Services
        ?? throw new InvalidOperationException("Host has not been initialised yet.");

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;

        _host = AppHost.Build();
        await _host.StartAsync();

        // Ensure the local SQLite database exists and is up to date.
        var dbFactory = _host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using (var db = await dbFactory.CreateDbContextAsync())
        {
            await db.Database.EnsureCreatedAsync();
        }

        var shell = _host.Services.GetRequiredService<MainWindow>();
        shell.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(2));
            _host.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Unhandled UI exception");
        MessageBox.Show(e.Exception.Message, "Discord Lyrics", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
