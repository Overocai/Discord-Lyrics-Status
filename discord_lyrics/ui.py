"""Terminal dashboard rendered with `rich`."""

from __future__ import annotations

from typing import Optional

from rich.align import Align
from rich.console import Group
from rich.panel import Panel
from rich.table import Table
from rich.text import Text


def _format_time(seconds: float) -> str:
    seconds = max(0, int(seconds))
    return f"{seconds // 60:02d}:{seconds % 60:02d}"


def _progress_bar(position: float, width: int = 30) -> str:
    # We don't always know the song duration, so this is a lightweight
    # "spinner of dots" that advances each second rather than a true bar.
    filled = int(position) % width
    return "─" * filled + "●" + "·" * (width - filled - 1)


def render(
    *,
    account: str,
    title: str,
    artist: str,
    position: float,
    playing: bool,
    prev_line: Optional[str],
    current_line: Optional[str],
    next_line: Optional[str],
    has_lyrics: bool,
    status_text: Optional[str],
) -> Panel:
    info = Table.grid(padding=(0, 2))
    info.add_column(justify="right", style="bright_black")
    info.add_column(style="bold")
    info.add_row("Song", title or "—")
    info.add_row("Artist", artist or "—")
    info.add_row("Time", f"{_format_time(position)}  {_progress_bar(position)}")

    lyric_lines = Text()
    if not playing:
        lyric_lines.append("⏸  Paused / nothing playing\n", style="yellow")
    elif not has_lyrics:
        lyric_lines.append("No synced lyrics found for this track\n", style="yellow")
    else:
        lyric_lines.append((prev_line or " ") + "\n", style="bright_black")
        lyric_lines.append("➤ " + (current_line or "…") + "\n", style="bold cyan")
        lyric_lines.append((next_line or " ") + "\n", style="bright_black")

    status_repr = status_text if status_text else "(cleared)"
    footer = Text()
    footer.append("Status  ", style="bright_black")
    footer.append(status_repr, style="green")

    body = Group(info, Text(), Align.left(lyric_lines), footer)

    return Panel(
        body,
        title="[bold magenta]Discord Lyrics Status[/]",
        subtitle=f"[bright_black]{account} · Ctrl+C to quit[/]",
        border_style="magenta",
        padding=(1, 2),
    )
