"""Thread-safe snapshot of the player state shared between worker and GUI."""

from __future__ import annotations

import threading
from dataclasses import dataclass, field
from typing import Optional


@dataclass
class PlayerState:
    # connection
    account: str = ""
    connected: bool = False
    error: Optional[str] = None

    # track
    title: str = ""
    artist: str = ""
    position: float = 0.0
    duration: float = 0.0
    playing: bool = False

    # lyrics
    has_lyrics: bool = False
    prev_line: Optional[str] = None
    current_line: Optional[str] = None
    next_line: Optional[str] = None

    # discord status
    status_text: Optional[str] = None

    # cover art (raw image bytes); art_token changes when the art changes
    art: Optional[bytes] = None
    art_token: int = 0

    _lock: threading.Lock = field(default_factory=threading.Lock, repr=False)

    def update(self, **kwargs) -> None:
        with self._lock:
            for key, value in kwargs.items():
                setattr(self, key, value)

    def snapshot(self) -> dict:
        with self._lock:
            return {k: v for k, v in self.__dict__.items() if not k.startswith("_")}
