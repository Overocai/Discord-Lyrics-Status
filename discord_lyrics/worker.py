"""Headless background loop: media -> lyrics -> Discord, writing to PlayerState.

This contains no UI. Both the GUI and (optionally) other front-ends can run it
in a thread and read the shared :class:`PlayerState`.
"""

from __future__ import annotations

import asyncio
import logging
import re
from typing import Optional

from .config import Config
from .discord_client import DiscordStatus
from .lyrics import LyricsTrack, fetch_lrc
from .media import NowPlaying, get_now_playing, get_thumbnail
from .state import PlayerState

log = logging.getLogger(__name__)
_WHITESPACE = re.compile(r"\s+")
_CAMEL = re.compile(r"([a-z])([A-Z])")


def clean_line(text: Optional[str]) -> Optional[str]:
    if not text:
        return None
    text = _CAMEL.sub(r"\1 \2", text)
    text = _WHITESPACE.sub(" ", text).strip()
    return text or None


def desired_status(cfg: Config, line: Optional[str], np: NowPlaying) -> Optional[str]:
    if line:
        return f"{cfg.status_prefix}{line}"
    if cfg.show_song_when_no_lyrics and np.title:
        artist = f" — {np.artist}" if np.artist else ""
        return f"{cfg.status_prefix}{np.title}{artist}"
    return None


async def _sleep(stop: asyncio.Event, seconds: float) -> None:
    """Sleep up to ``seconds``, waking early if ``stop`` is set."""
    try:
        await asyncio.wait_for(stop.wait(), timeout=seconds)
    except asyncio.TimeoutError:
        pass


async def run(cfg: Config, state: PlayerState, stop: asyncio.Event) -> None:
    discord = DiscordStatus(cfg.token, cfg.max_status_length)

    try:
        account = await asyncio.to_thread(discord.validate)
    except SystemExit as exc:
        state.update(error=str(exc), connected=False)
        return

    handle = account.get("username", "unknown")
    if account.get("discriminator", "0") not in ("0", None):
        handle = f"{handle}#{account['discriminator']}"
    state.update(account=handle, connected=True)

    await asyncio.to_thread(discord.clear)

    current_query: Optional[str] = None
    lyrics = LyricsTrack([], [])
    last_status: Optional[str] = ""
    art_token = 0

    try:
        while not stop.is_set():
            np = await get_now_playing()

            if np is None or not np.playing:
                if cfg.clear_on_pause:
                    await asyncio.to_thread(discord.clear)
                    last_status = None
                state.update(
                    playing=False,
                    title=np.title if np else "",
                    artist=np.artist if np else "",
                    position=np.position if np else 0.0,
                    duration=np.duration if np else 0.0,
                    prev_line=None,
                    current_line=None,
                    next_line=None,
                    status_text=None,
                )
                await _sleep(stop, 1.0)
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
                art = await get_thumbnail()
                art_token += 1
                state.update(art=art, art_token=art_token)

            prev, current, nxt = lyrics.window(np.position, cfg.line_lead)
            current = clean_line(current)

            desired = desired_status(cfg, current, np)
            if desired != last_status:
                last_status = desired
                asyncio.create_task(
                    asyncio.to_thread(discord.set_status, desired, cfg.emoji_name)
                )

            state.update(
                playing=True,
                title=np.title,
                artist=np.artist,
                position=np.position,
                duration=np.duration,
                has_lyrics=bool(lyrics),
                prev_line=clean_line(prev),
                current_line=current,
                next_line=clean_line(nxt),
                status_text=desired,
            )
            await _sleep(stop, cfg.poll_interval)
    finally:
        await asyncio.to_thread(discord.clear)
        discord.close()
