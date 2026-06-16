# Development Roadmap

Status legend: ✅ done in this foundation · 🔄 partial · ⬜ planned

## Phase 0 — Foundation (this delivery)

- ✅ .NET 9 WPF solution, clean folder architecture, DI host
- ✅ Design system (colors, typography, components) + custom-chrome shell
- ✅ Sidebar navigation across 8 sections (VM-first, DataTemplate routing)
- ✅ Services: media (WinRT), lyrics (LRCLIB), Discord (REST), status engine
- ✅ SQLite/EF persistence (history, profiles, settings) + Serilog logging
- ✅ DPAPI-encrypted token storage, in-app toasts, launch-at-startup
- ✅ Brand assets + Inno Setup installer script
- ✅ Update check against GitHub Releases

## Phase 1 — Polish the core experience

- ⬜ Wire **active Profile** into the engine (prefix/emoji/behaviour per profile)
- ⬜ Full scrolling lyrics view with line-by-line highlight + click-to-seek (where supported)
- ⬜ Settings live-apply (restart engine on poll-interval change)
- ⬜ System tray icon: minimise to tray, quick start/stop, now-playing tooltip
- ⬜ First-run onboarding wizard (token + permissions explainer)
- ⬜ Light theme + accent-color picker (design tokens already centralised)

## Phase 2 — Robustness & distribution

- ⬜ EF Core **migrations** (replace `EnsureCreated`) for safe schema evolution
- ⬜ Velopack in-app auto-update (see UPDATE_STRATEGY.md)
- ⬜ Authenticode signing for exe + installer (kill SmartScreen warnings)
- ⬜ Unit tests for `LrcParser`, `TrackCleaner`, `SyncedLyrics.Window`, engine compose logic
- ⬜ Telemetry-free crash log export from the Logs screen

## Phase 3 — Features

- ⬜ Multiple lyrics providers with fallback + per-track manual override/search
- ⬜ Rich Presence mode (activity card) in addition to custom status
- ⬜ Lyrics translation / romaji line
- ⬜ Per-app source selection when several media sessions are active
- ⬜ Global hotkeys (toggle engine, skip)
- ⬜ Localisation (en, pt-BR) — strings already isolated in views

## Phase 4 — Quality bar

- ⬜ Accessibility pass (AutomationProperties, keyboard nav, contrast checks)
- ⬜ Performance budget: < 60MB idle RAM, < 1% CPU at 0.5s poll
- ⬜ CI: GitHub Actions build + publish + draft release with installer artifact

## Known follow-ups from the foundation

- History ordering uses `Id` (SQLite cannot ORDER BY `DateTimeOffset`) — revisit with a value converter if date-range filtering is added.
- `IUpdateService` only checks; applying updates is Phase 2.
- Profiles are CRUD-only today; they don't yet feed the engine (Phase 1).
