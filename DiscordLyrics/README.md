<div align="center">

<img src="DiscordLyrics/Assets/banner.png" alt="Discord Lyrics" width="640" />

# Discord Lyrics

**Your song's synced lyrics, live on your Discord custom status.**

<br/>

[![Install Now](https://img.shields.io/badge/⬇%20%20INSTALL%20NOW-E5383B?style=for-the-badge&logo=windows&logoColor=white&labelColor=9E1B1F)](https://github.com/Overocai/Discord-Lyrics-Status/releases/latest/download/DiscordLyrics-Setup.exe)
&nbsp;&nbsp;
[![Português](https://img.shields.io/badge/🇧🇷%20%20Ler%20em%20Português-1E2027?style=for-the-badge)](README.pt-BR.md)

<sub>Windows 10/11 · clicking <b>Install Now</b> downloads the installer automatically</sub>

</div>

<br/>

<details>
<summary><b>Developer &amp; build info</b> (click to expand)</summary>

<br/>

A full C# / .NET 9 / WPF rewrite — MVVM, dependency injection, SQLite/EF Core,
Serilog, a custom dark/red design system and a professional Inno Setup installer.

> ⚠️ Educational use only. This drives a Discord *user* account (selfbot behaviour),
> which may violate Discord's Terms of Service. Your token is stored encrypted
> (Windows DPAPI) on your machine and is never uploaded anywhere.

### Run from source
```bash
dotnet run --project DiscordLyrics/DiscordLyrics.csproj
```

### Publish a self-contained build
```bash
dotnet publish DiscordLyrics/DiscordLyrics.csproj -c Release -r win-x64 ^
    --self-contained true -p:PublishSingleFile=true -o publish
```

### Build the installer
1. Install [Inno Setup 6](https://jrsoftware.org/isdl.php)
2. Publish the app (command above) so `publish/` exists
3. `iscc DiscordLyrics/Installer/DiscordLyrics.iss` → `dist/DiscordLyrics-Setup.exe`
4. Upload `DiscordLyrics-Setup.exe` as an asset on a GitHub Release — the **Install Now** button then works.

### Documentation
- [Architecture](docs/ARCHITECTURE.md) · [Design System](docs/DESIGN_SYSTEM.md) · [Wireframes](docs/WIREFRAMES.md) · [Update strategy](docs/UPDATE_STRATEGY.md) · [Roadmap](docs/ROADMAP.md)

### Author
**Overocai** · [GitHub](https://github.com/Overocai) · [Discord](https://discord.com/users/1288832011452153910)

</details>
