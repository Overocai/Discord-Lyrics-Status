"""Thin Discord client for updating the current user's custom status.

Improvements over a naive ``requests.patch`` per frame:
  * Reuses a single keep-alive HTTP session.
  * De-duplicates updates (never sends the same status twice).
  * Honours 429 rate limits with a cooldown instead of hammering the API.
  * Validates the token on startup with a clear error message.
"""

from __future__ import annotations

import logging
import time
from typing import Optional

import requests

log = logging.getLogger(__name__)

# Sentinel so the very first status (including an explicit None) is always sent.
_UNSET = object()


class DiscordStatus:
    API = "https://discord.com/api/v9"

    def __init__(self, token: str, max_length: int = 128) -> None:
        self.max_length = max_length
        self.session = requests.Session()
        self.session.headers.update(
            {
                "Authorization": token,
                "Content-Type": "application/json",
                "User-Agent": "Mozilla/5.0",
            }
        )
        self._last_payload = _UNSET
        self._cooldown_until = 0.0

    def validate(self) -> dict:
        """Return the account info, or raise SystemExit on a bad token."""
        try:
            resp = self.session.get(f"{self.API}/users/@me", timeout=10)
        except requests.RequestException as exc:
            raise SystemExit(f"Could not reach Discord: {exc}")

        if resp.status_code == 401:
            raise SystemExit("Invalid Discord token (401 Unauthorized).")
        if resp.status_code != 200:
            raise SystemExit(
                f"Unexpected response validating token: HTTP {resp.status_code}"
            )
        return resp.json()

    def set_status(self, text: Optional[str], emoji_name: str = "") -> None:
        """Set (or clear, when ``text`` is falsy) the custom status."""
        if text:
            text = text[: self.max_length]
            custom = {"text": text}
            if emoji_name:
                custom["emoji_name"] = emoji_name
        else:
            custom = None

        if custom == self._last_payload:
            return
        if time.monotonic() < self._cooldown_until:
            return

        try:
            resp = self.session.patch(
                f"{self.API}/users/@me/settings",
                json={"custom_status": custom},
                timeout=10,
            )
        except requests.RequestException as exc:
            log.debug("status update failed: %s", exc)
            return

        if resp.status_code == 429:
            retry_after = 5.0
            try:
                retry_after = float(resp.json().get("retry_after", retry_after))
            except (ValueError, requests.JSONDecodeError):
                pass
            self._cooldown_until = time.monotonic() + retry_after
            log.warning("rate limited by Discord, backing off %.1fs", retry_after)
            return

        if resp.ok:
            self._last_payload = custom
        else:
            log.debug("status update returned HTTP %s", resp.status_code)

    def clear(self) -> None:
        self.set_status(None)

    def close(self) -> None:
        self.session.close()
