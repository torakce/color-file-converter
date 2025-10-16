# Script pour créer l'icône de l'application
# Crée une icône 256x256 avec des barres RGB verticales

Add-Type -AssemblyName System.Drawing

# Créer un bitmap 256x256
$bitmap = New-Object System.Drawing.Bitmap(256, 256)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)

# Activer l'antialiasing pour un rendu de qualité
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Fond blanc
$whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$graphics.FillRectangle($whiteBrush, 0, 0, 256, 256)

# Dessiner 3 barres verticales RGB (inspiré du preset Couleurs)
$barWidth = 60
$barHeight = 180
$startX = 35
$startY = 38

# Barre Rouge
$redBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(220, 50, 50))
$graphics.FillRectangle($redBrush, $startX, $startY, $barWidth, $barHeight)

# Barre Verte
$greenBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(50, 200, 50))
$graphics.FillRectangle($greenBrush, $startX + $barWidth + 8, $startY, $barWidth, $barHeight)

# Barre Bleue
$blueBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::FromArgb(50, 120, 220))
$graphics.FillRectangle($blueBrush, $startX + ($barWidth + 8) * 2, $startY, $barWidth, $barHeight)

# Ajouter un contour noir autour de chaque barre
$blackPen = New-Object System.Drawing.Pen([System.Drawing.Color]::Black, 3)
$graphics.DrawRectangle($blackPen, $startX, $startY, $barWidth, $barHeight)
$graphics.DrawRectangle($blackPen, $startX + $barWidth + 8, $startY, $barWidth, $barHeight)
$graphics.DrawRectangle($blackPen, $startX + ($barWidth + 8) * 2, $startY, $barWidth, $barHeight)

# Nettoyer
$graphics.Dispose()

# Créer le dossier Resources s'il n'existe pas
$resourcesDir = "Converter.Gui\Resources"
if (-not (Test-Path $resourcesDir)) {
    New-Item -ItemType Directory -Path $resourcesDir -Force | Out-Null
}

# Sauvegarder en PNG d'abord
$pngPath = Join-Path $resourcesDir "app-icon.png"
$bitmap.Save($pngPath, [System.Drawing.Imaging.ImageFormat]::Png)
Write-Host "✓ PNG créé: $pngPath" -ForegroundColor Green

# Fonction pour convertir PNG en ICO
function ConvertTo-Icon {
    param(
        [string]$PngPath,
        [string]$IcoPath
    )
    
    # Charger le PNG
    $png = [System.Drawing.Image]::FromFile($PngPath)
    
    # Créer des versions à différentes tailles (256, 128, 64, 48, 32, 16)
    $sizes = @(256, 128, 64, 48, 32, 16)
    $icons = @()
    
    foreach ($size in $sizes) {
        $resized = New-Object System.Drawing.Bitmap($size, $size)
        $g = [System.Drawing.Graphics]::FromImage($resized)
        $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $g.DrawImage($png, 0, 0, $size, $size)
        $g.Dispose()
        $icons += $resized
    }
    
    # Créer le fichier ICO
    $stream = [System.IO.FileStream]::new($IcoPath, [System.IO.FileMode]::Create)
    $writer = [System.IO.BinaryWriter]::new($stream)
    
    # En-tête ICO
    $writer.Write([UInt16]0)  # Réservé
    $writer.Write([UInt16]1)  # Type: 1 = Icon
    $writer.Write([UInt16]$icons.Count)  # Nombre d'images
    
    # Calculer les offsets
    $offset = 6 + ($icons.Count * 16)  # En-tête + entrées
    
    # Écrire les entrées pour chaque taille
    foreach ($icon in $icons) {
        $ms = New-Object System.IO.MemoryStream
        $icon.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $imageData = $ms.ToArray()
        $ms.Dispose()
        
        $writer.Write([Byte]($icon.Width % 256))  # Largeur (0 = 256)
        $writer.Write([Byte]($icon.Height % 256)) # Hauteur (0 = 256)
        $writer.Write([Byte]0)   # Palette
        $writer.Write([Byte]0)   # Réservé
        $writer.Write([UInt16]1) # Plans de couleur
        $writer.Write([UInt16]32) # Bits par pixel
        $writer.Write([UInt32]$imageData.Length) # Taille
        $writer.Write([UInt32]$offset) # Offset
        
        $offset += $imageData.Length
    }
    
    # Écrire les données de chaque image
    foreach ($icon in $icons) {
        $ms = New-Object System.IO.MemoryStream
        $icon.Save($ms, [System.Drawing.Imaging.ImageFormat]::Png)
        $imageData = $ms.ToArray()
        $writer.Write($imageData)
        $ms.Dispose()
        $icon.Dispose()
    }
    
    $writer.Close()
    $stream.Close()
    $png.Dispose()
}

# Convertir en ICO
$icoPath = Join-Path $resourcesDir "app-icon.ico"
ConvertTo-Icon -PngPath $pngPath -IcoPath $icoPath
Write-Host "✓ ICO créé: $icoPath" -ForegroundColor Green
Write-Host ""
Write-Host "Icône créée avec succès!" -ForegroundColor Cyan
Write-Host "  - PNG: $pngPath" -ForegroundColor White
Write-Host "  - ICO: $icoPath" -ForegroundColor White

$bitmap.Dispose()
$whiteBrush.Dispose()
$redBrush.Dispose()
$greenBrush.Dispose()
$blueBrush.Dispose()
$blackPen.Dispose()
