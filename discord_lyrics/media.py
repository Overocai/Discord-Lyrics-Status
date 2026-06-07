"""Read the currently playing track from the Windows media session.

Uses the same Windows ``GlobalSystemMediaTransportControls`` API that the
volume overlay uses, so it works with Spotify, browsers, the Groove app, etc.
"""

from __future__ import annotations

from dataclasses import dataclass
from datetime import datetime, timezone
from typing import Optional

from winrt.windows.media.control import (
    GlobalSystemMediaTransportControlsSessionManager as MediaManager,
    GlobalSystemMediaTransportControlsSessionPlaybackStatus as PlaybackStatus,
)


@dataclass
class NowPlaying:
    title: str
    artist: str
    position: float  # seconds into the track
    playing: bool

    @property
    def query(self) -> str:
        return f"{self.title} {self.artist}".strip()


async def get_now_playing() -> Optional[NowPlaying]:
    """Return the active track, or ``None`` when nothing is available."""
    try:
        sessions = await MediaManager.request_async()
        session = sessions.get_current_session()
        if session is None:
            return None

        playback = session.get_playback_info()
        props = await session.try_get_media_properties_async()
        timeline = session.get_timeline_properties()

        playing = playback.playback_status == PlaybackStatus.PLAYING

        # timeline.position is a snapshot from last_updated_time; when the track
        # is playing we extrapolate to "now" so the lyric line stays in sync.
        position = timeline.position.total_seconds()
        if playing:
            elapsed = (datetime.now(timezone.utc) - timeline.last_updated_time).total_seconds()
            position += max(0.0, elapsed)

        return NowPlaying(
            title=props.title or "",
            artist=props.artist or "",
            position=position,
            playing=playing,
        )
    except Exception:
        # No media session, app closing, transient WinRT error, etc.
        return None
