"""Entry point.

    python main.py          # graphical interface (default)
    python main.py --cli    # terminal dashboard

Shows your currently playing song's synced lyrics as your Discord custom status.
"""

from __future__ import annotations

import asyncio
import logging
import sys

from discord_lyrics.config import Config, ensure_config_exists, save_token


def _force_utf8() -> None:
    """Avoid UnicodeEncodeError for emojis/box characters on Windows consoles."""
    for stream in (sys.stdout, sys.stderr):
        try:
            stream.reconfigure(encoding="utf-8")  # type: ignore[union-attr]
        except (AttributeError, ValueError):
            pass


def main() -> None:
    _force_utf8()
    logging.basicConfig(
        level=logging.WARNING,
        format="%(asctime)s %(levelname)s %(name)s: %(message)s",
        datefmt="%H:%M:%S",
    )

    ensure_config_exists()  # create config.json from the example on first run
    cfg = Config.load()

    if "--cli" in sys.argv:
        if not cfg.has_token():
            token = input("Paste your Discord token: ").strip()
            if not token:
                print("No token provided. Exiting.")
                sys.exit(1)
            save_token(token)
            cfg = Config.load()
        from discord_lyrics.app import run
        try:
            asyncio.run(run(cfg))
        except KeyboardInterrupt:
            print("\nStopped. Status cleared.")
            sys.exit(0)
    else:
        from discord_lyrics.gui import launch
        launch(cfg)


if __name__ == "__main__":
    main()
