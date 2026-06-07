"""Configuration loading.

The Discord token is read from (in order of priority):
  1. The ``DISCORD_TOKEN`` environment variable
  2. ``config.json`` next to the project root

Keeping the token out of the source code means you can safely push the
repository to GitHub without leaking your account.
"""

from __future__ import annotations

import json
import os
from dataclasses import dataclass, field, fields
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
CONFIG_PATH = ROOT / "config.json"
EXAMPLE_PATH = ROOT / "config.example.json"

PLACEHOLDER_TOKEN = "PUT_YOUR_DISCORD_TOKEN_HERE"


def ensure_config_exists() -> None:
    """Create config.json from the example on first run, so every user has one."""
    if CONFIG_PATH.exists():
        return
    if EXAMPLE_PATH.exists():
        CONFIG_PATH.write_text(EXAMPLE_PATH.read_text(encoding="utf-8"), encoding="utf-8")


def save_token(token: str) -> None:
    """Write the token into config.json (creating it from the example if needed)."""
    data: dict = {}
    for path in (CONFIG_PATH, EXAMPLE_PATH):
        if path.exists():
            try:
                data = json.loads(path.read_text(encoding="utf-8"))
                break
            except json.JSONDecodeError:
                data = {}
    data["token"] = token.strip()
    CONFIG_PATH.write_text(
        json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8"
    )


@dataclass
class Config:
    # Discord user token. Prefer setting it via the DISCORD_TOKEN env var.
    token: str = ""
    # Text placed before every lyric line in the status.
    status_prefix: str = "🎵 "
    # Optional emoji shown next to the status (unicode char, e.g. "🎧").
    emoji_name: str = ""
    # Seconds between media polls. Lower = snappier, more CPU.
    poll_interval: float = 0.3
    # Show the upcoming line slightly early to compensate for API latency.
    line_lead: float = 0.4
    # Discord caps custom status text at 128 characters.
    max_status_length: int = 128
    # When no synced lyrics are found, show "Title — Artist" instead.
    show_song_when_no_lyrics: bool = True
    # Clear the status when playback is paused/stopped.
    clear_on_pause: bool = True
    # Cache downloaded lyrics to disk to avoid repeated lookups.
    cache_lyrics: bool = True
    # Only accept time-synced (.lrc) lyrics; plain lyrics are ignored.
    synced_only: bool = True
    # Restrict syncedlyrics to specific providers (empty = all).
    providers: list[str] = field(default_factory=list)

    @classmethod
    def load(cls) -> "Config":
        data: dict = {}
        if CONFIG_PATH.exists():
            try:
                data = json.loads(CONFIG_PATH.read_text(encoding="utf-8"))
            except json.JSONDecodeError as exc:
                raise SystemExit(f"config.json is not valid JSON: {exc}")

        known = {f.name for f in fields(cls)}
        cfg = cls(**{k: v for k, v in data.items() if k in known})

        env_token = os.environ.get("DISCORD_TOKEN")
        if env_token:
            cfg.token = env_token.strip()

        cfg.token = (cfg.token or "").strip()
        return cfg

    def has_token(self) -> bool:
        return self.token not in ("", PLACEHOLDER_TOKEN)

    def validate(self) -> None:
        if not self.has_token():
            raise SystemExit(
                "No Discord token configured.\n"
                "  -> Copy config.example.json to config.json and paste your token, or\n"
                "  -> set the DISCORD_TOKEN environment variable."
            )
