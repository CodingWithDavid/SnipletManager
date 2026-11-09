$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore, WindowsBase

$sizes = 16, 24, 32, 48, 64, 128, 256
$frames = foreach ($size in $sizes) {
    $visual = [System.Windows.Media.DrawingVisual]::new()
    $drawing = $visual.RenderOpen()
    $drawing.PushTransform([System.Windows.Media.ScaleTransform]::new($size / 256.0, $size / 256.0))
    $blue = [System.Windows.Media.SolidColorBrush]::new([System.Windows.Media.ColorConverter]::ConvertFromString('#245A9C'))
    $lightBlue = [System.Windows.Media.SolidColorBrush]::new([System.Windows.Media.ColorConverter]::ConvertFromString('#AFC3E0'))
    $white = [System.Windows.Media.Brushes]::White
    $drawing.DrawRoundedRectangle($blue, $null, [System.Windows.Rect]::new(8, 8, 240, 240), 48, 48)
    $drawing.DrawRoundedRectangle($lightBlue, $null, [System.Windows.Rect]::new(52, 48, 124, 152), 14, 14)
    $drawing.DrawRoundedRectangle($white, $null, [System.Windows.Rect]::new(80, 68, 124, 152), 14, 14)
    $pen = [System.Windows.Media.Pen]::new($blue, 12)
    $pen.StartLineCap = [System.Windows.Media.PenLineCap]::Round
    $pen.EndLineCap = [System.Windows.Media.PenLineCap]::Round
    foreach ($y in 108, 140, 172) {
        $end = if ($y -eq 172) { 150 } else { 178 }
        $drawing.DrawLine($pen, [System.Windows.Point]::new(106, $y), [System.Windows.Point]::new($end, $y))
    }
    $drawing.Pop()
    $drawing.Close()
    $bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new($size, $size, 96, 96, [System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = [System.IO.MemoryStream]::new()
    $encoder.Save($stream)
    $bytes = $stream.ToArray()
    $stream.Dispose()
    [pscustomobject]@{ Size = $size; Bytes = $bytes }
}

$output = [System.IO.File]::Create((Join-Path $PSScriptRoot 'SnipletManager.ico'))
$writer = [System.IO.BinaryWriter]::new($output)
try {
    $writer.Write([uint16]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]$frames.Count)
    $offset = 6 + 16 * $frames.Count
    foreach ($frame in $frames) {
        $dimension = if ($frame.Size -eq 256) { 0 } else { $frame.Size }
        $writer.Write([byte]$dimension)
        $writer.Write([byte]$dimension)
        $writer.Write([byte]0)
        $writer.Write([byte]0)
        $writer.Write([uint16]1)
        $writer.Write([uint16]32)
        $writer.Write([uint32]$frame.Bytes.Length)
        $writer.Write([uint32]$offset)
        $offset += $frame.Bytes.Length
    }
    foreach ($frame in $frames) {
        $writer.Write([byte[]]$frame.Bytes)
    }
} finally {
    $writer.Dispose()
}
