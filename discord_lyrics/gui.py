"""Desktop GUI (customtkinter): a Spotify-style "now playing + lyrics" window."""

from __future__ import annotations

import asyncio
import io
import threading
from typing import Optional

import customtkinter as ctk
from PIL import Image, ImageDraw

from . import worker
from .config import Config
from .state import PlayerState

# Palette
BG = "#0d0f14"
CARD = "#1b1f2a"
ACCENT = "#7c4dff"     # purple (progress / note)
ACCENT2 = "#56d4ff"    # cyan (current lyric)
TEXT = "#f0f2f5"
SUB = "#8b93a7"
GREEN = "#3ba55d"
ERR = "#e06c75"


def _fmt(seconds: float) -> str:
    seconds = max(0, int(seconds))
    return f"{seconds // 60:02d}:{seconds % 60:02d}"


def _rounded(img: Image.Image, size: int, radius: int = 26) -> Image.Image:
    img = img.convert("RGBA").resize((size, size), Image.Resampling.LANCZOS)
    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).rounded_rectangle((0, 0, size, size), radius=radius, fill=255)
    img.putalpha(mask)
    return img


def _placeholder(size: int) -> Image.Image:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((0, 0, size - 1, size - 1), radius=26, fill=(27, 31, 42, 255))
    cx, cy = size // 2, size // 2
    color = (124, 77, 255, 255)
    d.ellipse((cx - 36, cy + 6, cx + 4, cy + 36), fill=color)        # note head
    d.rectangle((cx, cy - 46, cx + 4, cy + 22), fill=color)          # stem
    d.polygon([(cx + 4, cy - 46), (cx + 30, cy - 34), (cx + 4, cy - 20)], fill=color)  # flag
    return img


class WorkerThread(threading.Thread):
    """Runs the async worker loop in its own event loop/thread."""

    def __init__(self, cfg: Config, state: PlayerState) -> None:
        super().__init__(daemon=True)
        self.cfg = cfg
        self.state = state
        self._loop: Optional[asyncio.AbstractEventLoop] = None
        self._stop: Optional[asyncio.Event] = None

    def run(self) -> None:
        self._loop = asyncio.new_event_loop()
        asyncio.set_event_loop(self._loop)
        self._stop = asyncio.Event()
        try:
            self._loop.run_until_complete(worker.run(self.cfg, self.state, self._stop))
        except Exception as exc:  # surface any crash in the UI footer
            self.state.update(error=str(exc), connected=False)
        finally:
            try:
                self._loop.close()
            except Exception:
                pass

    def stop(self) -> None:
        if self._loop and self._stop and self._loop.is_running():
            self._loop.call_soon_threadsafe(self._stop.set)


class LyricsApp(ctk.CTk):
    ART = 240

    def __init__(self, cfg: Config, state: PlayerState, worker_thread: WorkerThread) -> None:
        super().__init__()
        self.cfg = cfg
        self.player = state  # NOTE: don't use self.state — Tk reserves .state()
        self.worker = worker_thread
        self._art_token = -1

        self.title("Discord Lyrics Status")
        self.geometry("470x680")
        self.minsize(430, 620)
        self.configure(fg_color=BG)

        self._ph_image = ctk.CTkImage(_placeholder(self.ART), size=(self.ART, self.ART))
        self._build()
        self.protocol("WM_DELETE_WINDOW", self._on_close)
        self.after(120, self._tick)

    def _build(self) -> None:
        self.grid_columnconfigure(0, weight=1)

        self.art_label = ctk.CTkLabel(self, text="", image=self._ph_image, fg_color="transparent")
        self.art_label.grid(row=0, column=0, pady=(30, 18))

        self.title_label = ctk.CTkLabel(
            self, text="—", font=ctk.CTkFont(size=22, weight="bold"),
            text_color=TEXT, wraplength=410,
        )
        self.title_label.grid(row=1, column=0, padx=24)

        self.artist_label = ctk.CTkLabel(
            self, text="", font=ctk.CTkFont(size=14), text_color=SUB,
        )
        self.artist_label.grid(row=2, column=0, pady=(2, 18))

        prog = ctk.CTkFrame(self, fg_color="transparent")
        prog.grid(row=3, column=0, padx=44, sticky="ew")
        prog.grid_columnconfigure(1, weight=1)
        self.t_cur = ctk.CTkLabel(prog, text="00:00", font=ctk.CTkFont(size=11), text_color=SUB, width=42)
        self.t_cur.grid(row=0, column=0)
        self.bar = ctk.CTkProgressBar(prog, height=6, corner_radius=3, progress_color=ACCENT, fg_color=CARD)
        self.bar.grid(row=0, column=1, padx=10, sticky="ew")
        self.bar.set(0)
        self.t_dur = ctk.CTkLabel(prog, text="--:--", font=ctk.CTkFont(size=11), text_color=SUB, width=42)
        self.t_dur.grid(row=0, column=2)

        lyr = ctk.CTkFrame(self, fg_color="transparent")
        lyr.grid(row=4, column=0, padx=24, pady=(24, 8), sticky="nsew")
        lyr.grid_columnconfigure(0, weight=1)
        self.grid_rowconfigure(4, weight=1)
        self.prev_label = ctk.CTkLabel(lyr, text="", font=ctk.CTkFont(size=14), text_color=SUB, wraplength=410)
        self.prev_label.grid(row=0, column=0, pady=7)
        self.cur_label = ctk.CTkLabel(
            lyr, text="", font=ctk.CTkFont(size=21, weight="bold"), text_color=ACCENT2, wraplength=420,
        )
        self.cur_label.grid(row=1, column=0, pady=7)
        self.next_label = ctk.CTkLabel(lyr, text="", font=ctk.CTkFont(size=14), text_color=SUB, wraplength=410)
        self.next_label.grid(row=2, column=0, pady=7)

        self.status_label = ctk.CTkLabel(self, text="", font=ctk.CTkFont(size=12), text_color=GREEN, wraplength=430)
        self.status_label.grid(row=5, column=0, pady=(8, 2))
        self.footer = ctk.CTkLabel(self, text="conectando…", font=ctk.CTkFont(size=11), text_color=SUB)
        self.footer.grid(row=6, column=0, pady=(0, 16))

    def _tick(self) -> None:
        s = self.player.snapshot()

        if s.get("error"):
            self.title_label.configure(text="Erro de conexão")
            self.artist_label.configure(text=str(s["error"]).split("\n")[0], text_color=ERR)
            self.footer.configure(text="desconectado", text_color=ERR)
            self.after(400, self._tick)
            return

        # cover art (only rebuild when it actually changes)
        if s.get("art_token") != self._art_token:
            self._art_token = s.get("art_token")
            data = s.get("art")
            try:
                img = _rounded(Image.open(io.BytesIO(data)), self.ART) if data else None
            except Exception:
                img = None
            if img is not None:
                self.art_label.configure(image=ctk.CTkImage(img, size=(self.ART, self.ART)))
            else:
                self.art_label.configure(image=self._ph_image)

        playing = bool(s.get("playing"))
        self.title_label.configure(text=s.get("title") or "Nada tocando")
        self.artist_label.configure(text=s.get("artist") or "", text_color=SUB)

        pos = s.get("position") or 0.0
        dur = s.get("duration") or 0.0
        self.t_cur.configure(text=_fmt(pos))
        self.t_dur.configure(text=_fmt(dur) if dur else "--:--")
        self.bar.set(min(1.0, pos / dur) if dur else 0.0)

        if not playing:
            self.prev_label.configure(text="")
            self.cur_label.configure(text="⏸  Pausado", text_color=SUB)
            self.next_label.configure(text="")
        elif not s.get("has_lyrics"):
            self.prev_label.configure(text="")
            self.cur_label.configure(text="♫  Sem letra sincronizada", text_color=SUB)
            self.next_label.configure(text="")
        else:
            self.prev_label.configure(text=s.get("prev_line") or "")
            self.cur_label.configure(text=s.get("current_line") or "♪ ♪ ♪", text_color=ACCENT2)
            self.next_label.configure(text=s.get("next_line") or "")

        st = s.get("status_text")
        self.status_label.configure(text=f"🎵  no status: {st}" if st else "")
        if s.get("connected"):
            self.footer.configure(text=f"conectado como {s.get('account', '')}", text_color=SUB)

        self.after(120, self._tick)

    def _on_close(self) -> None:
        self.footer.configure(text="encerrando…")
        self.worker.stop()
        self.after(200, self.destroy)


def launch(cfg: Config) -> None:
    cfg.validate()
    ctk.set_appearance_mode("dark")
    ctk.set_widget_scaling(1.0)

    state = PlayerState()
    worker_thread = WorkerThread(cfg, state)
    worker_thread.start()

    app = LyricsApp(cfg, state, worker_thread)
    app.mainloop()
