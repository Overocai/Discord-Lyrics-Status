# 🎵 Discord Lyrics Status

Turn your Discord **custom status** into a live, line-by-line lyrics display of
whatever you're listening to. The app reads the song from the Windows media
session (Spotify, browser, Groove…), fetches time-synced lyrics, and updates
your status in real time as the song plays.

```
╭───────────────────────── Discord Lyrics Status ─────────────────────────╮
│                                                                          │
│    Song    Bohemian Rhapsody                                             │
│  Artist    Queen                                                         │
│    Time    01:42  ───────────●··············                            │
│                                                                          │
│  Is this the real life?                                                  │
│  ➤ Is this just fantasy?                                                 │
│  Caught in a landslide                                                   │
│                                                                          │
│  Status  🎵 Is this just fantasy?                                        │
╰──────────────────────── yourname · Ctrl+C to quit ──────────────────────╯
```

---

## ✨ What's improved over similar scripts

- **Token kept out of the source** — loaded from `config.json` (git-ignored) or
  the `DISCORD_TOKEN` env var, so you can publish the repo safely.
- **Token validated on startup** with a clear error instead of silent failure.
- **Rate-limit aware** — respects Discord `429` responses with a cooldown and
  reuses a single keep-alive HTTP session.
- **Lyrics cached to disk** (`.cache/`) — each song is only looked up once.
- **Accurate line sync** — handles multiple timestamps per LRC line and uses a
  binary search; shows previous / current / next lines.
- **Graceful fallback** — when no synced lyrics exist, shows `Title — Artist`.
- **Clean modular code** with a `rich` terminal dashboard.

---

## ⚠️ Disclaimer

This tool automates actions on a **user account** (a "selfbot"), which is
**against Discord's Terms of Service**. It only changes *your own* custom status
and doesn't touch servers or other users, but using it could in theory get your
account actioned. **Use it at your own risk on an account you're willing to
lose.** This project is for educational purposes.

---

## 📦 Requirements

- **Windows 10 / 11** (relies on the Windows media-control API)
- **Python 3.9+**
- A Discord **user token**

## 🚀 Setup

```bash
git clone https://github.com/<your-username>/Discord-Lyrics-Status.git
cd Discord-Lyrics-Status

pip install -r requirements.txt

# create your private config from the template
copy config.example.json config.json   # Windows
# cp config.example.json config.json    # macOS/Linux
```

Open `config.json` and paste your token into `"token"`, **or** set an
environment variable instead (recommended):

```powershell
$env:DISCORD_TOKEN = "your_token_here"
```

Then run it:

```bash
python main.py
```

Play a song and your status starts updating automatically. Press **Ctrl+C** to
stop — your status is cleared on exit.

## ⚙️ Configuration (`config.json`)

| Key | Default | Description |
|-----|---------|-------------|
| `token` | `""` | Discord user token (env var `DISCORD_TOKEN` overrides this). |
| `status_prefix` | `"🎵 "` | Text placed before every lyric line. |
| `emoji_name` | `""` | Optional emoji shown next to the status (e.g. `"🎧"`). |
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
Windows Media API ──> media.py     (what song, what position)
syncedlyrics      ──> lyrics.py    (download + cache + parse LRC, find line)
                       app.py      (loop: pick current line each tick)
Discord REST API  <── discord_client.py  (set custom status, rate-limited)
rich              <── ui.py        (terminal dashboard)
```

## 🔑 Getting your token

You'll find guides online for extracting your Discord user token from the
client/DevTools. **Never share it** — anyone with your token has full access to
your account. This repo's `.gitignore` already excludes `config.json` to help
prevent accidental leaks.

## 🛠️ Troubleshooting

- **`Invalid Discord token (401)`** — token is wrong/expired; grab a fresh one.
- **No lyrics shown** — the song may not have synced lyrics; it'll fall back to
  the title. Delete `.cache/` to force a re-lookup.
- **Status not updating** — make sure the music is actually *playing* (not
  paused) and that Windows shows it in the media overlay (Win key + volume).

## 👤 Author

Made by **overocai** — [Discord](https://discord.com/users/1288832011452153910) (`1288832011452153910`).

## 📄 License

MIT © overocai — see [LICENSE](LICENSE).

*Inspired by community "lyrics-to-status" scripts, rewritten for safer config,
caching, rate-limit handling and a cleaner codebase.*
