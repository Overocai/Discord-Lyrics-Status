# Generates the Discord Lyrics brand assets with GDI+.
# Palette: deep black, graphite, premium red.
Add-Type -AssemblyName System.Drawing

$assets    = Join-Path $PSScriptRoot '..\DiscordLyrics\Assets'
$installer = Join-Path $PSScriptRoot '..\DiscordLyrics\Installer'
New-Item -ItemType Directory -Force -Path $assets, $installer | Out-Null

# Discord palette: graphite + blurple
$BLACK    = [System.Drawing.Color]::FromArgb(255, 30, 31, 34)    # #1E1F22
$GRAPHITE = [System.Drawing.Color]::FromArgb(255, 43, 45, 49)    # #2B2D31
$RED      = [System.Drawing.Color]::FromArgb(255, 88, 101, 242)  # blurple #5865F2
$REDDARK  = [System.Drawing.Color]::FromArgb(255, 71, 82, 196)   # #4752C4
$WHITE    = [System.Drawing.Color]::White
$TEXTSUB  = [System.Drawing.Color]::FromArgb(255, 181, 186, 193) # #B5BAC1

function New-Note {
    param([System.Drawing.Graphics]$g, [single]$cx, [single]$cy, [single]$scale, [System.Drawing.Color]$color)
    $g.SmoothingMode = 'AntiAlias'
    $brush = New-Object System.Drawing.SolidBrush $color
    $pen   = New-Object System.Drawing.Pen $color, ([single](6 * $scale))
    # note head
    $g.FillEllipse($brush, $cx - 34*$scale, $cy + 6*$scale, 34*$scale, 26*$scale)
    # stem
    $g.FillRectangle($brush, $cx - 2*$scale, $cy - 46*$scale, 5*$scale, 56*$scale)
    # flag
    $pts = @(
        (New-Object System.Drawing.PointF(($cx + 3*$scale), ($cy - 46*$scale))),
        (New-Object System.Drawing.PointF(($cx + 30*$scale), ($cy - 30*$scale))),
        (New-Object System.Drawing.PointF(($cx + 3*$scale), ($cy - 16*$scale)))
    )
    $g.FillPolygon($brush, $pts)
    $brush.Dispose(); $pen.Dispose()
}

function New-Tile {
    param([int]$size, [bool]$rounded = $true)
    $bmp = New-Object System.Drawing.Bitmap $size, $size
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = 'AntiAlias'
    $rect = New-Object System.Drawing.Rectangle 0, 0, $size, $size
    $grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $RED, $REDDARK, 45
    if ($rounded) {
        $r = [int]($size * 0.22)
        $path = New-Object System.Drawing.Drawing2D.GraphicsPath
        $path.AddArc(0, 0, $r, $r, 180, 90)
        $path.AddArc($size - $r, 0, $r, $r, 270, 90)
        $path.AddArc($size - $r, $size - $r, $r, $r, 0, 90)
        $path.AddArc(0, $size - $r, $r, $r, 90, 90)
        $path.CloseFigure()
        $g.FillPath($grad, $path)
    } else {
        $g.FillRectangle($grad, $rect)
    }
    New-Note -g $g -cx ([single]($size/2)) -cy ([single]($size/2)) -scale ([single]($size/120.0)) -color $WHITE
    $g.Dispose(); $grad.Dispose()
    return $bmp
}

# ---- icon.ico (256px PNG-embedded ICO) ----
$icoBmp = New-Tile -size 256 -rounded $true
$ms = New-Object System.IO.MemoryStream
$icoBmp.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
$png = $ms.ToArray(); $ms.Dispose()

$icoPath = Join-Path $assets 'icon.ico'
$fs = [System.IO.File]::Create($icoPath)
$bw = New-Object System.IO.BinaryWriter $fs
$bw.Write([uint16]0); $bw.Write([uint16]1); $bw.Write([uint16]1)        # ICONDIR
$bw.Write([byte]0); $bw.Write([byte]0); $bw.Write([byte]0); $bw.Write([byte]0)
$bw.Write([uint16]1); $bw.Write([uint16]32)
$bw.Write([uint32]$png.Length); $bw.Write([uint32]22)                   # size + offset
$bw.Write($png)
$bw.Flush(); $bw.Dispose(); $fs.Dispose()
$icoBmp.Dispose()
Write-Host "icon.ico written"

# ---- logo.png (512 transparent tile) ----
$logo = New-Tile -size 512 -rounded $true
$logo.Save((Join-Path $assets 'logo.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$logo.Dispose()

# ---- banner.png (1200x420 hero) ----
$bw2 = 1200; $bh = 420
$banner = New-Object System.Drawing.Bitmap $bw2, $bh
$g = [System.Drawing.Graphics]::FromImage($banner)
$g.SmoothingMode = 'AntiAlias'; $g.TextRenderingHint = 'ClearTypeGridFit'
$g.FillRectangle((New-Object System.Drawing.SolidBrush $BLACK), 0, 0, $bw2, $bh)
$tile = New-Tile -size 200 -rounded $true
$g.DrawImage($tile, 90, 110, 200, 200); $tile.Dispose()
$fTitle = New-Object System.Drawing.Font 'Segoe UI', 56, ([System.Drawing.FontStyle]::Bold)
$fSub   = New-Object System.Drawing.Font 'Segoe UI', 22, ([System.Drawing.FontStyle]::Regular)
$g.DrawString('Discord Lyrics', $fTitle, (New-Object System.Drawing.SolidBrush $WHITE), 330, 150)
$g.DrawString('Synced lyrics on your Discord status', $fSub, (New-Object System.Drawing.SolidBrush $TEXTSUB), 336, 250)
$g.FillRectangle((New-Object System.Drawing.SolidBrush $RED), 336, 232, 220, 4)
$g.Dispose()
$banner.Save((Join-Path $assets 'banner.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$banner.Save((Join-Path $installer 'installer-banner.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$banner.Dispose()

# ---- Inno Setup wizard images (BMP) ----
# Large sidebar: 164x314
$side = New-Object System.Drawing.Bitmap 164, 314
$g = [System.Drawing.Graphics]::FromImage($side)
$g.SmoothingMode = 'AntiAlias'
$rect = New-Object System.Drawing.Rectangle 0, 0, 164, 314
$grad = New-Object System.Drawing.Drawing2D.LinearGradientBrush $rect, $BLACK, $REDDARK, 110
$g.FillRectangle($grad, $rect)
$tile = New-Tile -size 96 -rounded $true
$g.DrawImage($tile, 34, 90, 96, 96); $tile.Dispose()
$g.Dispose()
$side.Save((Join-Path $installer 'installer-sidebar.bmp'), [System.Drawing.Imaging.ImageFormat]::Bmp)
$side.Save((Join-Path $installer 'installer-sidebar.png'), [System.Drawing.Imaging.ImageFormat]::Png)
$side.Dispose()

# Small header: 55x58
$small = New-Tile -size 58 -rounded $false
$crop = New-Object System.Drawing.Bitmap 55, 58
$g = [System.Drawing.Graphics]::FromImage($crop)
$g.DrawImage($small, 0, 0, 55, 58)
$g.Dispose()
$crop.Save((Join-Path $installer 'installer-small.bmp'), [System.Drawing.Imaging.ImageFormat]::Bmp)
$crop.Dispose(); $small.Dispose()

Write-Host "All assets generated in:"
Write-Host "  $assets"
Write-Host "  $installer"
