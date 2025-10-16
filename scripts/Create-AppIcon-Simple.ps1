# Script simple pour créer l'icône PNG de l'application
Add-Type -AssemblyName System.Drawing

Write-Host "Création de l'icône de l'application..." -ForegroundColor Cyan

# Créer un bitmap 256x256
$bitmap = New-Object System.Drawing.Bitmap(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Fond blanc
$whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$graphics.FillRectangle($whiteBrush, 0, 0, 256, 256)

# Dessiner 3 barres verticales RGB
$barWidth = 60
$barHeight = 180
$startX = 35
$startY = 38

# Rouge
$redBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(220, 50, 50))
$graphics.FillRectangle($redBrush, $startX, $startY, $barWidth, $barHeight)

# Vert
$greenBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(50, 200, 50))
$graphics.FillRectangle($greenBrush, ($startX + $barWidth + 8), $startY, $barWidth, $barHeight)

# Bleu
$blueBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(50, 120, 220))
$graphics.FillRectangle($blueBrush, ($startX + ($barWidth + 8) * 2), $startY, $barWidth, $barHeight)

# Contours noirs
$blackPen = New-Object System.Drawing.Pen([System.Drawing.Color]::Black, 3)
$graphics.DrawRectangle($blackPen, $startX, $startY, $barWidth, $barHeight)
$graphics.DrawRectangle($blackPen, ($startX + $barWidth + 8), $startY, $barWidth, $barHeight)
$graphics.DrawRectangle($blackPen, ($startX + ($barWidth + 8) * 2), $startY, $barWidth, $barHeight)

$graphics.Dispose()

# Créer le dossier Resources
$resourcesDir = "Converter.Gui\Resources"
if (-not (Test-Path $resourcesDir)) {
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
}

# Sauvegarder en PNG
$pngPath = "$resourcesDir\app-icon.png"
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "✓ PNG créé: $pngPath" -ForegroundColor Green

# Nettoyer
$bitmap.Dispose()
$whiteBrush.Dispose()
$redBrush.Dispose()
$greenBrush.Dispose()
$blueBrush.Dispose()
$blackPen.Dispose()

Write-Host ""
Write-Host 'Icone creee avec succes!' -ForegroundColor Green
Write-Host "Fichier: $pngPath" -ForegroundColor White
