# Design System

A dark, high-contrast system matching **Discord's dark theme**: graphite grays
with a **blurple** accent. Defined as WPF resource dictionaries in `Themes/` and
consumed via `StaticResource` / `DynamicResource`.

> Note: the accent token keys keep the historical `Red` names (e.g. `Brush.Red`)
> so the whole UI re-themes from one file — but the *values* are Discord blurple.

## Color tokens (`Themes/Colors.xaml`)

| Token | Hex | Use |
|---|---|---|
| `Color.Black` | `#1E1F22` | Window / title bar |
| `Color.Sidebar` | `#2B2D31` | Navigation rail |
| `Color.Surface` | `#313338` | Content background |
| `Color.SurfaceAlt` | `#383A40` | Inputs, insets |
| `Color.Card` | `#2B2D31` | Cards / panels |
| `Color.CardHover` | `#35373C` | Hover state |
| `Color.Border` | `#3F4147` | 1px separators |
| `Color.Graphite` | `#4E5058` | Disabled / tracks |
| `Color.TextPrimary` | `#F2F3F5` | Headings, key text |
| `Color.TextSecondary` | `#B5BAC1` | Body |
| `Color.TextMuted` | `#80848E` | Captions, labels |
| `Color.Red` (accent) | `#5865F2` | **Primary accent — blurple** |
| `Color.RedHover` | `#4752C4` | Accent hover |
| `Color.RedPressed` | `#3C45A5` | Accent pressed |
| `Color.RedDark` | `#4752C4` | Badges / highlights |
| `Color.RedDeep` | `#3C45A5` | Gradient end |
| `Color.Success` | `#23A55A` | Online / success |
| `Color.Warning` | `#F0B232` | Warning |
| `Color.Danger` | `#DA373C` | Danger / close button |

Two signature brushes: `Brush.RedGradient` (blurple — the brand mark + accents)
and `Brush.SurfaceGradient` (album-art placeholder, installer sidebar).

## Typography (`Themes/Typography.xaml`)

Segoe UI Variable with graceful fallback. Scale:

| Style | Size | Weight | Use |
|---|---|---|---|
| `Text.Display` | 28 | Bold | Page titles |
| `Text.Title` | 20 | SemiBold | Card / song titles |
| `Text.Subtitle` | 15 | SemiBold | Sub-headers |
| `Text.Body` | 13 | Regular | Paragraphs |
| `Text.Caption` | 11 | SemiBold | Metadata |
| `Text.SectionLabel` | 11 | Bold | UPPERCASE group labels |

## Components (`Themes/Controls.xaml`)

- **`Card`** — rounded (14px) bordered surface; the layout primitive.
- **`Button.Primary`** — red, 10px radius, hover/pressed/disabled states.
- **`Button.Ghost`** — transparent bordered, subtle hover.
- **`Button.Caption` / `Button.Close`** — window chrome buttons (Segoe MDL2 glyphs).
- **`NavItem`** — sidebar `RadioButton` with glyph, left red indicator bar when selected.
- **`Input`** — `TextBox` with red focus ring.
- **`Toggle`** — iOS-style `CheckBox` switch (red when on).
- **`Progress`** — slim 6px red progress bar.
- Slim, chromeless scrollbars.

## Motion

- Toast notifications: 180ms opacity + 220ms cubic-ease slide-in, auto-dismiss 3.4s, 220ms fade-out.
- Nav/hover transitions via control-template triggers (instant, snappy).

## Iconography

Glyphs from **Segoe MDL2 Assets** (ships with Windows) — no image icons needed
for UI chrome. Brand mark is the red-gradient tile with a music note (see `Assets/`).

## Principles

1. **No emojis in the UI.** Status text may contain a user-chosen prefix only.
2. **One accent.** Blurple is the only chromatic color; everything else is neutral (red is reserved for the close button / danger).
3. **Depth via elevation, not shadow noise** — surfaces step lighter as they rise.
4. **Generous radius** (10–14px) for a modern, product feel.
