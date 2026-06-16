# Architecture

Discord Lyrics is a single WPF process organised in clean layers with a
view-model-first MVVM approach and constructor dependency injection.

## High-level layers

```
┌──────────────────────────────────────────────────────────────┐
│  Views (WPF / XAML)         MainWindow + 8 UserControls        │
│  ── bound via DataTemplates to ──                              │
│  ViewModels (CommunityToolkit.Mvvm)   one per section + shell  │
├──────────────────────────────────────────────────────────────┤
│  Services (interfaces + impl.)                                 │
│   IMediaService · ILyricsService · IDiscordService             │
│   IStatusEngine · ISettingsService · IHistoryService           │
│   IUpdateService · INotificationService · INavigationService   │
├──────────────────────────────────────────────────────────────┤
│  Storage (EF Core / SQLite)     AppDbContext + entities        │
│  Infrastructure   navigation · converters · text cleaners      │
│  Core             AppHost (DI) · AppPaths                       │
└──────────────────────────────────────────────────────────────┘
        Serilog logging cross-cuts every layer.
```

## Folder map

| Folder | Responsibility |
|---|---|
| `Core/` | Composition root (`AppHost`), writable paths (`AppPaths`) |
| `Models/` | DTOs (`NowPlaying`, `SyncedLyrics`) + EF entities + `AppSettings` |
| `Services/` | All behaviour behind interfaces (testable, swappable) |
| `Storage/` | `AppDbContext` (SQLite) |
| `Infrastructure/` | Navigation, value converters, image + text helpers, startup registry |
| `ViewModels/` | One observable VM per screen + `MainViewModel` shell |
| `Views/` | XAML for the shell window and each section |
| `Themes/` | Design-system resource dictionaries (Colors, Typography, Controls) |
| `Assets/` | App icon + brand images (copied to output) |
| `Installer/` | Inno Setup script + wizard images |
| `Logging/` | (Serilog configured in `Core/AppHost`) |
| `Updater/` | (Update logic lives in `Services/UpdateService`; see UPDATE_STRATEGY) |

## Dependency injection

`Core/AppHost.Build()` configures a generic `IHost`:

- **Singletons**: every service and view-model that holds long-lived state
  (the engine, settings cache, Discord/media clients, navigation).
- **`IDbContextFactory<AppDbContext>`**: singletons create a short-lived
  `AppDbContext` per unit of work — no captive DbContext.
- **`IHttpClientFactory`**: named clients (`lyrics`, `discord`, `updates`) with
  pre-configured base addresses, timeouts and user agents.

`App.OnStartup` builds the host, runs `EnsureCreatedAsync`, then resolves and
shows `MainWindow`.

## Navigation

`MainViewModel.Navigate(section)` calls `INavigationService.NavigateTo<TViewModel>()`,
which resolves the VM from DI and raises `CurrentChanged`. The shell's
`ContentControl` is bound to `CurrentPage`; `DataTemplate`s in `MainWindow.xaml`
map each VM type to its `UserControl`. VMs implementing `IActivatable` get an
`OnActivated()` callback to refresh data when shown.

## Data flow — the Status Engine

`StatusEngine` runs a cancellable background loop (`Task.Run`):

```
poll (lightweight, no art)
   │  track key changed?
   ├─ yes → re-read with album art
   │        fetch synced lyrics (LRCLIB, cached)
   │        log to history
   ▼
compute (prev, current, next) line at position + lead
compose status text  (prefix + line, truncated, paused/empty rules)
push to Discord  (de-duplicated, 429 backoff)
raise Updated  → VMs marshal to UI thread via Dispatcher
sleep (poll interval)
```

The engine never touches the UI directly; view-models subscribe to `Updated`
and copy state onto the dispatcher, keeping the interface responsive.

## Threading & performance

- Media art is only read from WinRT when the track actually changes.
- Lyrics are cached in memory and on disk (`%APPDATA%/DiscordLyrics/cache`).
- Bitmaps are decoded once and frozen for cross-thread use.
- Tiered PGO + concurrent GC enabled in the project file for fast startup and low RAM.
