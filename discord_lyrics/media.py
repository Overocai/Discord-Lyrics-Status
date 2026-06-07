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
from winrt.windows.storage.streams import DataReader


@dataclass
class NowPlaying:
    title: str
    artist: str
    position: float  # seconds into the track
    duration: float  # total track length in seconds (0 if unknown)
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

        duration = max(0.0, timeline.end_time.total_seconds() - timeline.start_time.total_seconds())
        if duration:
            position = min(position, duration)

        return NowPlaying(
            title=props.title or "",
            artist=props.artist or "",
            position=position,
            duration=duration,
            playing=playing,
        )
    except Exception:
        # No media session, app closing, transient WinRT error, etc.
        return None


async def get_thumbnail() -> Optional[bytes]:
    """Return the current track's cover art as raw image bytes, or ``None``."""
    try:
        sessions = await MediaManager.request_async()
        session = sessions.get_current_session()
        if session is None:
            return None

        props = await session.try_get_media_properties_async()
        ref = props.thumbnail
        if ref is None:
            return None

        stream = await ref.open_read_async()
        size = stream.size
        if not size:
            return None

        reader = DataReader(stream)
        await reader.load_async(size)
        buffer = bytearray(size)
        reader.read_bytes(buffer)
        return bytes(buffer)
    except Exception:
        return None
