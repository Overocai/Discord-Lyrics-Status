"""Entry point: python main.py

Shows your currently playing song's synced lyrics as your Discord custom status.
"""

from __future__ import annotations

import asyncio
import logging
import sys

from discord_lyrics.app import run
from discord_lyrics.config import Config


def main() -> None:
    logging.basicConfig(
        level=logging.WARNING,
        format="%(asctime)s %(levelname)s %(name)s: %(message)s",
        datefmt="%H:%M:%S",
    )

    cfg = Config.load()
    try:
        asyncio.run(run(cfg))
    except KeyboardInterrupt:
        print("\nStopped. Status cleared.")
        sys.exit(0)


if __name__ == "__main__":
    main()
