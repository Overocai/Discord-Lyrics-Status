# Wireframes

Custom-chrome window, 1120×720 default. Fixed 240px sidebar + fluid content.

## Shell

```
┌────────────┬─────────────────────────────────────────────────┐
│ ◆ DISCORD  │  section label                      _  □  ✕      │  ← title bar (drag)
│   LYRICS   ├─────────────────────────────────────────────────┤
│  STATUS..  │                                                   │
│            │                                                   │
│  MENU      │                                                   │
│  ▎ Dashboard│              ACTIVE PAGE CONTENT                 │
│    Lyrics  │           (ContentControl → DataTemplate)         │
│    Discord │                                                   │
│    Profiles│                                                   │
│    History │                                            ┌────┐ │
│  SYSTEM    │                                            │toast│ │
│    Settings│                                            └────┘ │
│    Logs    │                                                   │
│    About   │                                                   │
│ ● Live·user│                                                   │  ← footer status
└────────────┴─────────────────────────────────────────────────┘
```

## Dashboard

```
Dashboard                                              [ Start ]
Your live now-playing overview
┌───────────────────────────────────┐  ┌───────────────────┐
│ ┌─────┐  Song Title                │  │ DISCORD           │
│ │ art │  Artist                    │  │ ● username        │
│ │     │  ▓▓▓▓▓▓░░░░░░░░░░░          │  └───────────────────┘
│ └─────┘  1:23              3:40    │  ┌───────────────────┐
└───────────────────────────────────┘  │ CURRENT STATUS    │
┌──────────────────────────────────────│ ♪ current line    │
│ LYRICS                               ││ ...               │
│            previous line  (faded)    │└───────────────────┘
│        ♪  CURRENT LINE  (red, big)   │
│              next line   (faded)     │
└──────────────────────────────────────┘
🕘 128 tracks logged
```

## Lyrics (karaoke)

```
Song Title
Artist
┌────────────────────────────────────────────────────────────┐
│                                                              │
│                    previous line (muted)                     │
│                                                              │
│              CURRENT LINE — large, red, centered             │
│                                                              │
│                      next line (muted)                       │
│                                                              │
└────────────────────────────────────────────────────────────┘
```

## Discord

```
Discord
Connect your account to drive the custom status
┌──────────────────────────────────────────────────────────┐
│ ●  username                                  [ Disconnect ]│
│    000000000000000000                                      │
└──────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────┐
│ DISCORD TOKEN                                              │
│ [ •••••••••••••••••••••••••••••••••• ]                     │
│ [    Connect    ]                                          │
│ 🛈 Token encrypted with DPAPI, stored only on this PC.     │
└──────────────────────────────────────────────────────────┘
```

## Profiles

```
Profiles                                          [ New profile ]
Reusable status presets
┌──────────────────────────────────────────────────────────┐
│ Default  [ACTIVE]                  [ Activate ] [ Delete ] │
│ Prefix: ♪                                                  │
└──────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────┐
│ Gaming                             [ Activate ] [ Delete ] │
│ Prefix: 🎮                                                 │
└──────────────────────────────────────────────────────────┘
```

## History

```
History                                       [ Refresh ] [ Clear ]
128 tracks detected
┌──────────────────────────────────────────────────────────┐
│ Song Title                          [LYRICS]   16/06 18:24 │
│ Artist                                                     │
│ ────────────────────────────────────────────────────────  │
│ Another Song                                   16/06 18:19 │
│ Artist                                                     │
└──────────────────────────────────────────────────────────┘
```

## Settings

```
Settings
┌──────────────────────────────────────────────────────────┐
│ STATUS                                                     │
│ Status prefix   [ ♪          ]                             │
│ Emoji name      [            ]                             │
│ Poll(s)[0.5]  Lead(s)[0.4]  Max[128]                       │
└──────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────┐
│ BEHAVIOUR                                                  │
│ Show song when no lyrics                          (●— )    │
│ Clear status when paused                          (●— )    │
│ Only time-synced lyrics                           (●— )    │
│ Start engine on launch                            (●— )    │
│ Launch at Windows startup                         ( —○)    │
└──────────────────────────────────────────────────────────┘
[ Save changes ]
```

## Logs

```
Logs                                      [ Refresh ] [ Open folder ]
log-20260616.txt
┌──────────────────────────────────────────────────────────┐
│ 18:26:46 [INF] Engine started                              │
│ 18:26:47 [DBG] now playing: ...                            │
│ ...                                                        │
└──────────────────────────────────────────────────────────┘
```

## About

```
About
┌──────────────────────────────────────────────────────────┐
│ ◆  Discord Lyrics                                          │
│    Version 1.0.0                                           │
│  A premium desktop app that turns your music into ...      │
│ ───────────────────────────────────────────────────────   │
│ AUTHOR                              [ GitHub ] [ Discord ] │
│ Overocai                                                   │
└──────────────────────────────────────────────────────────┘
┌──────────────────────────────────────────────────────────┐
│ UPDATES                            [ Check for updates ]   │
│ You're on the latest version.                              │
└──────────────────────────────────────────────────────────┘
```
