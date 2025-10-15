# Script pour créer le package de release v2.2.0
# Color File Converter - Enhanced GUI Edition

$version = "2.2.0"
$outputDir = "Release\v$version"
$publishDir = "Converter.Gui\bin\Release\net8.0-windows\win-x64\publish"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Color File Converter v$version" -ForegroundColor Cyan
Write-Host "Package Builder" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Créer le dossier de sortie
Write-Host "[1/5] Creating output directory..." -ForegroundColor Yellow
if (Test-Path $outputDir) {
    Remove-Item -Path $outputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
Write-Host "  ✓ Created: $outputDir" -ForegroundColor Green
Write-Host ""

# Copier les fichiers publiés
Write-Host "[2/5] Copying published files..." -ForegroundColor Yellow
Copy-Item -Path "$publishDir\*" -Destination $outputDir -Recurse -Force
Write-Host "  ✓ Files copied from: $publishDir" -ForegroundColor Green
Write-Host ""

# Copier les fichiers de documentation
Write-Host "[3/5] Adding documentation..." -ForegroundColor Yellow
Copy-Item -Path "RELEASE_NOTES_v$version.md" -Destination "$outputDir\RELEASE_NOTES.md" -Force
Copy-Item -Path "README.md" -Destination "$outputDir\" -Force -ErrorAction SilentlyContinue
Write-Host "  ✓ Documentation added" -ForegroundColor Green
Write-Host ""

# Créer l'archive ZIP
Write-Host "[4/5] Creating ZIP archive..." -ForegroundColor Yellow
$zipFile = "ColorFileConverter-v$version-win-x64.zip"
if (Test-Path $zipFile) {
    Remove-Item -Path $zipFile -Force
}
Compress-Archive -Path "$outputDir\*" -DestinationPath $zipFile -CompressionLevel Optimal
Write-Host "  ✓ Created: $zipFile" -ForegroundColor Green
$zipSize = (Get-Item $zipFile).Length / 1MB
Write-Host "  ✓ Size: $($zipSize.ToString('F2')) MB" -ForegroundColor Green
Write-Host ""

# Afficher le résumé
Write-Host "[5/5] Summary" -ForegroundColor Yellow
Write-Host "  Package: $zipFile" -ForegroundColor Cyan
Write-Host "  Location: $(Get-Location)\$zipFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "✓ Package ready for GitHub Release!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor White
Write-Host "1. Go to: https://github.com/torakce/color-file-converter/releases/new" -ForegroundColor White
Write-Host "2. Select tag: v$version" -ForegroundColor White
Write-Host "3. Upload: $zipFile" -ForegroundColor White
Write-Host "4. Copy release notes from: RELEASE_NOTES_v$version.md" -ForegroundColor White
Write-Host "5. Publish release!" -ForegroundColor White
Write-Host ""
