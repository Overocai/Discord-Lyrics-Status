"""Centered, full-screen "now playing" dashboard rendered with `rich`."""

from __future__ import annotations

from typing import Optional

from rich.align import Align
from rich.console import Group, RenderableType
from rich.panel import Panel
from rich.rule import Rule
from rich.text import Text

ACCENT = "#c678dd"      # soft purple
ACCENT_HI = "#e5b3ff"
DIM = "#5c6370"
CURRENT = "#56d4ff"     # bright cyan for the active line


def _fmt(seconds: float) -> str:
    seconds = max(0, int(seconds))
    return f"{seconds // 60:02d}:{seconds % 60:02d}"


def _progress(position: float, duration: float, width: int = 32) -> Text:
    if duration and duration > 0:
        frac = min(1.0, max(0.0, position / duration))
    else:
        # Unknown length: a dot that slowly drifts across the bar.
        frac = (position % width) / width

    filled = int(frac * (width - 1))
    bar = Text(justify="center")
    bar.append(_fmt(position) + "  ", style=DIM)
    bar.append("━" * filled, style=ACCENT)
    bar.append("●", style=ACCENT_HI)
    bar.append("─" * (width - filled - 1), style=DIM)
    bar.append("  " + (_fmt(duration) if duration else "--:--"), style=DIM)
    return bar


def render(
    *,
    account: str,
    title: str,
    artist: str,
    position: float,
    duration: float,
    playing: bool,
    prev_line: Optional[str],
    current_line: Optional[str],
    next_line: Optional[str],
    has_lyrics: bool,
    status_text: Optional[str],
    max_width: int = 80,
) -> RenderableType:
    header = Text(justify="center")
    header.append("♪  ", style=f"bold {ACCENT_HI}")
    header.append(title or "Nothing playing", style="bold white")
    header.append("\n")
    header.append(artist or "—", style=DIM)

    lyrics = Text(justify="center")
    if not playing:
        lyrics.append("\n⏸  Paused\n", style="yellow")
    elif not has_lyrics:
        lyrics.append("\n♫  No synced lyrics for this track\n", style="yellow")
    else:
        lyrics.append((prev_line or "·") + "\n\n", style=DIM)
        lyrics.append(current_line or "♪ ♪ ♪", style=f"bold {CURRENT}")
        lyrics.append("\n\n")
        lyrics.append(next_line or "·", style=DIM)

    status = Text(justify="center")
    status.append("status  ", style=DIM)
    status.append(status_text or "(cleared)", style="bold #98c379")

    body = Group(
        Align.center(header),
        Text(),
        Align.center(_progress(position, duration)),
        Text(),
        Text(),
        Align.center(lyrics),
        Text(),
        Text(),
        Rule(style=DIM),
        Align.center(status),
    )

    panel = Panel(
        body,
        title=f"[bold {ACCENT}]🎵 Discord Lyrics Status[/]",
        subtitle=f"[{DIM}]{account} · Ctrl+C to quit[/]",
        border_style=ACCENT,
        padding=(2, 5),
        width=min(74, max(40, max_width - 4)),
    )

    return Align.center(panel, vertical="middle")
