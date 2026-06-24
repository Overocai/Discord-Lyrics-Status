# 🎵 Discord Lyrics Status

> **English** · [Português 🇧🇷](README.pt-BR.md)

Turn your Discord **custom status** into a live, line-by-line lyrics display of
whatever you're listening to — with a clean desktop app that shows the **album
art**, the song, a **progress bar**, and the lyrics scrolling in real time.

It reads the song straight from Windows' "now playing" (the same overlay you get
with the volume keys), so it works with **Spotify, YouTube, browsers and more —
no Spotify API or developer keys required.**

> [!WARNING]
> **Came here for the installer / desktop app? Please read this.**
> This project used to ship a separate **C# / .NET 9 / WPF desktop app** bundled
> with a one-click `.exe` installer (the old **`v1.0.0`** release). **It has been
> removed**, and so have that release and its git tag.
>
> **Why it's gone:** in real use it was simply **too heavy and too slow**. It had
> a multi-second cold start, dragged in the whole **.NET runtime + installer**
> baggage just to set a custom status, used a lot of memory, and — worst of all —
> had an **absurd delay**: a clearly noticeable lag between the song actually
> advancing and the status/lyrics catching up, so the lyrics were constantly out
> of sync. Not worth it for what the tool does.
>
> **What you get instead:** the project is now **only the lightweight Python
> version** in this repo. It starts instantly, polls the music several times a
> second, and keeps the lyrics tightly in sync with almost no lag — no installer,
> no runtime to install, no background bloat. Just follow the **Setup** steps
> below.

---

## 🖥️ The interface

The desktop window (built with `customtkinter`) is a Spotify-style "now playing"
card:

- 🖼️ **Album art** — pulled directly from Windows, rounded corners.
- 🎵 **Song title & artist** of the current track.
- 📊 **Progress bar** with elapsed time and total duration.
- 🎤 **Synced lyrics** in the center — the **current line is highlighted in
  cyan**, with the previous and next lines dimmed above and below.
- 🟢 **Footer** showing the exact text currently set as your Discord status and
  which account you're connected as.

Everything updates automatically as the song plays, and your Discord status
follows along line by line.

> Prefer the terminal? Run `python main.py --cli` for a `rich` text dashboard.

---

## ✨ Features

- **Live lyrics → Discord status**, line by line.
- **Modern GUI** with album art, progress bar and highlighted current line.
- **Works with any player** that shows in Windows media controls (Spotify,
  YouTube in a browser, Groove, etc.) — **no Spotify API needed**.
- **Smart YouTube handling** — strips junk like `(Official Video)`,
  `(Clipe Oficial)`, `(Lyrics)`, `[HD]`, `- Topic`, `VEVO` before searching.
- **Accurate sync** — handles multiple timestamps per LRC line via binary search.
- **Lyrics cached to disk** so each song is only looked up once.
- **Rate-limit aware** — respects Discord's `429` responses.
- **Token kept out of the source** — loaded from `config.json` (git-ignored) or
  the `DISCORD_TOKEN` env var, so the repo is safe to publish.

---

## ⚠️ Disclaimer

This tool automates a **user account** (a "selfbot"), which is **against
Discord's Terms of Service**. It only changes *your own* custom status and never
touches servers or other users, but using it could in theory get your account
actioned. **Use at your own risk on an account you're willing to lose.** This
project is for educational purposes.

---

## 📦 Requirements

- **Windows 10 / 11** (uses the Windows media-control API)
- **Python 3.9+**
- A Discord **user token**

## 🚀 Setup

```bash
git clone https://github.com/Overocai/Discord-Lyrics-Status.git
cd Discord-Lyrics-Status
```

**Install the dependencies.** The easiest way on Windows is to just
**double-click `install.bat`** — it runs `pip install -r requirements.txt` for
you and pauses at the end so you can read the result. Prefer the terminal? Do it
by hand instead:

```bash
pip install -r requirements.txt
```

Then create your private config from the template:

```bash
copy config.example.json config.json   # Windows
```

Open `config.json` and paste your token into `"token"`, **or** set an
environment variable instead (recommended):

```powershell
$env:DISCORD_TOKEN = "your_token_here"
```

Then run it:

```bash
python main.py            # graphical interface (default)
python main.py --cli      # terminal dashboard
```

Play a song and the window comes alive — your Discord status updates
automatically. Close the window (or press Ctrl+C in CLI mode) to clear it.

## ⚙️ Configuration (`config.json`)

| Key | Default | Description |
|-----|---------|-------------|
| `token` | `""` | Discord user token (env var `DISCORD_TOKEN` overrides this). |
| `status_prefix` | `"🎵 "` | Text placed before every lyric line. |
| `emoji_name` | `""` | Optional emoji shown next to the status. |
| `poll_interval` | `0.3` | Seconds between media checks. |
| `line_lead` | `0.4` | Show each line slightly early to offset API latency. |
| `max_status_length` | `128` | Discord's hard limit for status text. |
| `show_song_when_no_lyrics` | `true` | Fall back to `Title — Artist`. |
| `clear_on_pause` | `true` | Clear the status when playback pauses/stops. |
| `cache_lyrics` | `true` | Cache lyrics to `.cache/`. |
| `synced_only` | `true` | Only accept time-synced `.lrc` lyrics. |
| `providers` | `[]` | Restrict `syncedlyrics` to specific providers. |

## 🧠 How it works

```
Windows Media API ──> media.py     (song, position, duration, album art)
syncedlyrics      ──> lyrics.py    (download + cache + parse LRC, find line)
                       worker.py    (background loop, updates shared state)
Discord REST API  <── discord_client.py  (set custom status, rate-limited)
customtkinter     <── gui.py        (desktop window)  |  rich -> app.py (CLI)
```

## 🔑 Getting your token

The quickest way is to let Discord hand you your **own** token straight from the
browser **Console**:

1. Open **<https://discord.com/app>** in your browser (Chrome, Edge or Firefox)
   and log in to the account you want to use.
2. Press **F12** (or `Ctrl`+`Shift`+`I`) to open DevTools and click the
   **Console** tab.
3. Chrome/Edge block pasting into the console the first time — if you see a
   "Self-XSS" / "Don't paste code here" warning, type **`allow pasting`** and
   press Enter.
4. Paste this **single line** and press **Enter**:

   ```js
   window.webpackChunkdiscord_app.push([[Symbol()],{},o=>{for(let e of Object.values(o.c))try{if(!e.exports||e.exports===window)continue;e.exports?.getToken&&(token=e.exports.getToken());for(let o in e.exports)e.exports?.[o]?.getToken&&"IntlMessagesProxy"!==e.exports[o][Symbol.toStringTag]&&(token=e.exports[o].getToken())}catch{}}]),window.webpackChunkdiscord_app.pop(),token;
   ```

5. The console prints your token as a string in quotes, e.g.
   `'MTI4ODgz...XYZ'`. Copy the part **inside the quotes** — that's your token.
6. Paste it into `config.json` under `"token"`, or set the `DISCORD_TOKEN`
   environment variable.

> 💡 The snippet only reads the token from **your own** logged-in session and
> prints it locally — it doesn't send anything anywhere.

<details>
<summary>Alternative: the Network tab</summary>

Open Discord, press **F12 → Network**, do any action (e.g. send a message), click
a request and copy the **`authorization`** request-header value.
</details>

⚠️ **Never share your token** — anyone with it has full access to your account.
This repo's `.gitignore` already excludes `config.json` to prevent accidental
leaks.

## 🛠️ Troubleshooting

- **`Invalid Discord token (401)`** — the token is wrong/expired; grab a new one.
- **No lyrics** — the track may not have synced lyrics (it falls back to the
  title). Delete `.cache/` to force a re-lookup.
- **Wrong song detected** — if Spotify *and* a YouTube tab play at once, Windows
  reports the most recent session. Pause the one you don't want.
- **Lyrics slightly off** — tweak `line_lead` in `config.json`.

## 👤 Author

Made by **overocai** — [Discord](https://discord.com/users/1288832011452153910) (`1288832011452153910`).

## 📄 License

MIT © overocai — see [LICENSE](LICENSE).
