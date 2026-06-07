"""Main application loop tying media, lyrics, Discord and the UI together."""

from __future__ import annotations

import asyncio
import logging
import re
from typing import Optional

from rich.console import Console
from rich.live import Live

from .config import Config
from .discord_client import DiscordStatus
from .lyrics import LyricsTrack, fetch_lrc
from .media import get_now_playing
from .ui import render

log = logging.getLogger(__name__)
_WHITESPACE = re.compile(r"\s+")
_CAMEL = re.compile(r"([a-z])([A-Z])")


def clean_line(text: Optional[str]) -> Optional[str]:
    """Tidy a lyric line for display (collapse spaces, split joined words)."""
    if not text:
        return None
    text = _CAMEL.sub(r"\1 \2", text)
    text = _WHITESPACE.sub(" ", text).strip()
    return text or None


def _desired_status(cfg: Config, line: Optional[str], np) -> Optional[str]:
    if line:
        return f"{cfg.status_prefix}{line}"
    if cfg.show_song_when_no_lyrics and np.title:
        artist = f" — {np.artist}" if np.artist else ""
        return f"{cfg.status_prefix}{np.title}{artist}"
    return None


async def run(cfg: Config) -> None:
    cfg.validate()

    discord = DiscordStatus(cfg.token, cfg.max_status_length)
    console = Console()

    with console.status("[magenta]Validating Discord token…"):
        account = await asyncio.to_thread(discord.validate)
    handle = account.get("username", "unknown")
    if account.get("discriminator", "0") not in ("0", None):
        handle = f"{handle}#{account['discriminator']}"

    discord.clear()

    current_query: Optional[str] = None
    lyrics = LyricsTrack([], [])
    last_status: Optional[str] = ""  # "" forces the first real update through

    try:
        with Live(console=console, refresh_per_second=8, screen=False) as live:
            while True:
                np = await get_now_playing()

                if np is None or not np.playing:
                    if cfg.clear_on_pause:
                        await asyncio.to_thread(discord.clear)
                        last_status = None
                    live.update(
                        render(
                            account=handle,
                            title=np.title if np else "",
                            artist=np.artist if np else "",
                            position=np.position if np else 0.0,
                            playing=False,
                            prev_line=None,
                            current_line=None,
                            next_line=None,
                            has_lyrics=bool(lyrics),
                            status_text=None,
                        )
                    )
                    await asyncio.sleep(1.0)
                    continue

                if np.query != current_query:
                    current_query = np.query
                    lrc = await asyncio.to_thread(
                        fetch_lrc,
                        np.title,
                        np.artist,
                        use_cache=cfg.cache_lyrics,
                        synced_only=cfg.synced_only,
                        providers=cfg.providers,
                    )
                    lyrics = LyricsTrack.from_lrc(lrc)

                prev, current, nxt = lyrics.window(np.position, cfg.line_lead)
                current = clean_line(current)

                desired = _desired_status(cfg, current, np)
                if desired != last_status:
                    last_status = desired
                    asyncio.create_task(
                        asyncio.to_thread(discord.set_status, desired, cfg.emoji_name)
                    )

                live.update(
                    render(
                        account=handle,
                        title=np.title,
                        artist=np.artist,
                        position=np.position,
                        playing=True,
                        prev_line=clean_line(prev),
                        current_line=current,
                        next_line=clean_line(nxt),
                        has_lyrics=bool(lyrics),
                        status_text=desired,
                    )
                )

                await asyncio.sleep(cfg.poll_interval)
    finally:
        await asyncio.to_thread(discord.clear)
        discord.close()
