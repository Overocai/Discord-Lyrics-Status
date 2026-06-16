# Auto-Update Strategy

## Today (v1.0) — check & notify

`Services/UpdateService` queries the GitHub Releases API:

```
GET https://api.github.com/repos/Overocai/Discord-Lyrics-Status/releases/latest
```

It compares `tag_name` (e.g. `v1.2.0`) against the running assembly version. If
newer, the About screen shows *"Update available: vX.Y.Z"* and a toast fires.
The user clicks through to the release page to download the new installer.

This is intentionally simple, safe and dependency-free.

## Phase 2 — in-app download + apply

Recommended path for silent updates without writing a custom updater:

1. **Adopt [Velopack](https://github.com/velopack/velopack)** (successor to Squirrel/Clowd).
   - `vpk pack` produces delta + full packages and a `RELEASES` feed.
   - Host the feed on **GitHub Releases** (same repo) — no server needed.
2. On launch (and every N hours) call `UpdateManager.CheckForUpdatesAsync()`.
3. Download in the background; apply on next restart with `ApplyUpdatesAndRestart()`.
4. Show progress via the existing toast/notification system.

Velopack handles versioned install folders, rollback and shortcuts, so it
coexists cleanly with the Inno Setup *first-install* experience (Inno installs
v1; Velopack takes over for subsequent updates).

## Channels

- **stable** — GitHub Releases marked *latest*.
- **beta** (optional) — pre-releases; gated behind a Settings toggle that points
  `UpdateService` at `releases` (including pre-releases) instead of `releases/latest`.

## Integrity

- Ship releases over HTTPS from GitHub's CDN.
- Phase 2: enable Velopack package signing and verify signatures before applying.
- Optionally Authenticode-sign `DiscordLyrics.exe` and the setup to remove
  SmartScreen warnings (requires a code-signing certificate).

## Versioning

Semantic versioning `MAJOR.MINOR.PATCH`, single source of truth in
`DiscordLyrics.csproj` (`<Version>`), surfaced at runtime via
`Assembly.GetExecutingAssembly().GetName().Version` and used by the installer
(`#define MyAppVersion`).
