# Design System

A dark, high-contrast system built around **deep black, graphite, and premium
red**. Defined as WPF resource dictionaries in `Themes/` and consumed via
`StaticResource` / `DynamicResource`.

## Color tokens (`Themes/Colors.xaml`)

| Token | Hex | Use |
|---|---|---|
| `Color.Black` | `#07080A` | Window background |
| `Color.Sidebar` | `#0D0E12` | Navigation rail |
| `Color.Surface` | `#131419` | Content background |
| `Color.SurfaceAlt` | `#191B21` | Inputs, insets |
| `Color.Card` | `#1E2027` | Cards / panels |
| `Color.CardHover` | `#24262F` | Hover state |
| `Color.Border` | `#2A2C35` | 1px separators |
| `Color.Graphite` | `#34363F` | Disabled / tracks |
| `Color.TextPrimary` | `#F3F4F6` | Headings, key text |
| `Color.TextSecondary` | `#A0A3AD` | Body |
| `Color.TextMuted` | `#6A6D78` | Captions, labels |
| `Color.Red` | `#E5383B` | **Primary accent** |
| `Color.RedHover` | `#FF4A4E` | Accent hover |
| `Color.RedPressed` | `#C42B2E` | Accent pressed |
| `Color.RedDark` | `#9E1B1F` | Badges / highlights |
| `Color.RedDeep` | `#6E1215` | Gradient end |
| `Color.Success` / `Warning` / `Danger` | green / amber / red | Semantic |

Two signature brushes: `Brush.RedGradient` (the brand mark + accents) and
`Brush.SurfaceGradient` (album-art placeholder, installer sidebar).

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
2. **One accent.** Red is the only chromatic color; everything else is neutral.
3. **Depth via elevation, not shadow noise** — surfaces step lighter as they rise.
4. **Generous radius** (10–14px) for a modern, product feel.
