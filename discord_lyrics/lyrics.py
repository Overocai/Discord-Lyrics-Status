"""Fetch, cache, parse and look up time-synced lyrics.

* Lyrics are fetched via ``syncedlyrics`` and cached to ``.cache/lyrics`` so the
  same song never triggers a second network lookup.
* ``[mm:ss.xx]`` timestamps (including several per line) are parsed into a sorted
  table and the current line is found with a binary search.
"""

from __future__ import annotations

import hashlib
import logging
import re
from bisect import bisect_right
from dataclasses import dataclass
from pathlib import Path
from typing import List, Optional, Sequence, Tuple

import syncedlyrics

from .config import ROOT

log = logging.getLogger(__name__)

CACHE_DIR = ROOT / ".cache" / "lyrics"
_TIMESTAMP = re.compile(r"\[(\d+):(\d+(?:\.\d+)?)\]")

# Release qualifiers that pollute lyric searches (e.g. "Song - Single Version").
_NOISE = (
    r"remaster(?:ed)?|single version|album version|radio edit|mono|stereo|"
    r"deluxe|bonus|anniversary|expanded|re-?recorded|taylor'?s version|"
    r"\d{4}\s*remaster|live|acoustic|version|edit|remix"
)
# YouTube-style tags (incl. Portuguese) that should always be removed.
_MEDIA_TAGS = (
    r"official(?:\s+(?:music\s+)?video|\s+audio|\s+lyric\s+video)?|"
    r"music\s+video|lyrics?|lyric\s+video|visuali[sz]er|mv|hd|hq|4k|8k|"
    r"explicit|clean|color\s*coded|legendado|legendada|tradu[çc][ãa]o|"
    r"oficial|v[ií]deo\s*oficial|[aá]udio|ao\s+vivo|clipe(?:\s+oficial)?"
)
_PAREN_NOISE = re.compile(
    r"\s*[\(\[][^)\]]*\b(?:" + _NOISE + r"|" + _MEDIA_TAGS + r")\b[^)\]]*[)\]]", re.I
)
_DASH_NOISE = re.compile(r"\s*-\s*[^-]*\b(?:" + _NOISE + r")\b.*$", re.I)
_FEAT = re.compile(r"\s*[\(\[]?\s*\b(?:feat\.?|ft\.?|featuring)\b[^)\]]*[)\]]?", re.I)
_TRAIL_TAGS = re.compile(r"\s*[\(\[][^)\]]*[)\]]\s*$")  # leftover empty-ish trailers


def clean_title(title: str) -> str:
    """Strip release/YouTube qualifiers and features so searches match better."""
    text = _PAREN_NOISE.sub("", title or "")
    text = _DASH_NOISE.sub("", text)
    text = _FEAT.sub("", text)
    return re.sub(r"\s+", " ", text).strip(" -").strip()


def clean_artist(artist: str) -> str:
    """Drop YouTube channel suffixes like ' - Topic' and trailing 'VEVO'."""
    text = re.sub(r"\s*-\s*topic\s*$", "", artist or "", flags=re.I)
    text = re.sub(r"\s*vevo\s*$", "", text, flags=re.I)
    return re.sub(r"\s+", " ", text).strip()


def _cache_path(query: str) -> Path:
    digest = hashlib.sha1(query.lower().encode("utf-8")).hexdigest()
    return CACHE_DIR / f"{digest}.lrc"


def fetch_lrc(
    title: str,
    artist: str,
    *,
    use_cache: bool = True,
    synced_only: bool = True,
    providers: Optional[Sequence[str]] = None,
) -> Optional[str]:
    """Return raw LRC text for a track (cached), or ``None`` if not found.

    Tries the cleaned title first, then the raw title, then the title alone.
    """
    # syncedlyrics iterates over `providers`, so it must be a list, never None.
    provider_list = list(providers) if providers else []
    artist = clean_artist(artist)
    clean = clean_title(title)

    queries: List[str] = []
    for candidate in (f"{clean} {artist}", f"{title} {artist}", clean):
        candidate = candidate.strip()
        if candidate and candidate not in queries:
            queries.append(candidate)

    if not queries:
        return None

    path = _cache_path(queries[0])
    if use_cache and path.exists():
        cached = path.read_text(encoding="utf-8")
        if cached:
            return cached  # only hits are cached, so a non-empty file is valid

    lrc: Optional[str] = None
    for query in queries:
        try:
            lrc = syncedlyrics.search(
                query, synced_only=synced_only, providers=provider_list
            )
        except Exception as exc:  # network/provider errors must not crash the loop
            log.debug("lyrics lookup failed for %r: %s", query, exc)
            lrc = None
        if lrc:
            break

    # Only cache hits; misses are cheap to retry (they happen on song change).
    if use_cache and lrc:
        CACHE_DIR.mkdir(parents=True, exist_ok=True)
        path.write_text(lrc, encoding="utf-8")

    return lrc


def parse_lrc(lrc: Optional[str]) -> List[Tuple[float, str]]:
    """Parse LRC text into a time-sorted ``[(seconds, text), ...]`` list."""
    entries: List[Tuple[float, str]] = []
    if not lrc:
        return entries

    for line in lrc.splitlines():
        stamps = list(_TIMESTAMP.finditer(line))
        if not stamps:
            continue
        text = line[stamps[-1].end():].strip()
        for stamp in stamps:
            seconds = int(stamp.group(1)) * 60 + float(stamp.group(2))
            entries.append((seconds, text))

    entries.sort(key=lambda item: item[0])
    return entries


@dataclass
class LyricsTrack:
    """A parsed, queryable synced-lyrics track."""

    times: List[float]
    texts: List[str]

    @classmethod
    def from_lrc(cls, lrc: Optional[str]) -> "LyricsTrack":
        entries = parse_lrc(lrc)
        return cls([t for t, _ in entries], [txt for _, txt in entries])

    def __bool__(self) -> bool:
        return any(self.texts)

    def window(self, position: float, lead: float = 0.0):
        """Return ``(prev, current, next)`` lines for the given position.

        Any of the three may be ``None`` (before the song starts, instrumental
        gaps, or after the last line).
        """
        if not self.times:
            return None, None, None

        idx = bisect_right(self.times, position + lead) - 1
        if idx < 0:
            return None, None, self.texts[0] or None

        prev = self.texts[idx - 1] if idx - 1 >= 0 else None
        current = self.texts[idx] or None
        nxt = self.texts[idx + 1] if idx + 1 < len(self.texts) else None
        return prev or None, current, nxt or None
