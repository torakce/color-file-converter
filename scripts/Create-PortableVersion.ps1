# Script pour créer une version portable de Color File Converter
# Nécessite Ghostscript installé sur le système de build

param(
    [string]$OutputDir = "$PSScriptRoot\..\Release\Portable",
    [string]$GhostscriptSourcePath = "",
    [switch]$DownloadGhostscript = $false,
    [switch]$CleanBuild = $false
)

$ErrorActionPreference = "Stop"

Write-Host "=== Création de la version portable de Color File Converter ===" -ForegroundColor Green

# Nettoyer le dossier de sortie si demandé
if ($CleanBuild -and (Test-Path $OutputDir)) {
    Write-Host "Nettoyage du dossier de sortie..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}

# Créer le dossier de sortie
if (!(Test-Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
}

$ProjectRoot = Resolve-Path "$PSScriptRoot\.."
$GuiProject = Join-Path $ProjectRoot "Converter.Gui\Converter.Gui.csproj"

Write-Host "Compilation du projet en mode Release portable..." -ForegroundColor Yellow
Set-Location $ProjectRoot

# Publication avec toutes les dépendances incluses
dotnet publish $GuiProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output "$OutputDir\App" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:PublishTrimmed=false

if ($LASTEXITCODE -ne 0) {
    throw "Erreur lors de la compilation"
}

Write-Host "Application compilée avec succès!" -ForegroundColor Green

# Gestion de Ghostscript
$GhostscriptDir = Join-Path $OutputDir "App\Ghostscript"

if ($DownloadGhostscript) {
    Write-Host "Téléchargement automatique de Ghostscript non implémenté." -ForegroundColor Yellow
    Write-Host "Veuillez spécifier -GhostscriptSourcePath ou installer Ghostscript manuellement." -ForegroundColor Yellow
} elseif ($GhostscriptSourcePath -and (Test-Path $GhostscriptSourcePath)) {
    Write-Host "Copie de Ghostscript depuis: $GhostscriptSourcePath" -ForegroundColor Yellow
    
    if (!(Test-Path $GhostscriptDir)) {
        New-Item -Path $GhostscriptDir -ItemType Directory -Force | Out-Null
    }
    
    # Copier les fichiers essentiels de Ghostscript
    $SourceBin = Join-Path $GhostscriptSourcePath "bin"
    $SourceLib = Join-Path $GhostscriptSourcePath "lib" 
    $SourceResource = Join-Path $GhostscriptSourcePath "Resource"
    
    if (Test-Path $SourceBin) {
        Write-Host "  Copie du dossier bin..." -ForegroundColor Cyan
        Copy-Item -Path "$SourceBin\*" -Destination $GhostscriptDir -Recurse -Force
    }
    
    if (Test-Path $SourceLib) {
        Write-Host "  Copie du dossier lib..." -ForegroundColor Cyan
        $LibDir = Join-Path $GhostscriptDir "lib"
        if (!(Test-Path $LibDir)) { New-Item -Path $LibDir -ItemType Directory -Force | Out-Null }
        Copy-Item -Path "$SourceLib\*" -Destination $LibDir -Recurse -Force
    }
    
    if (Test-Path $SourceResource) {
        Write-Host "  Copie du dossier Resource..." -ForegroundColor Cyan
        $ResourceDir = Join-Path $GhostscriptDir "Resource"
        if (!(Test-Path $ResourceDir)) { New-Item -Path $ResourceDir -ItemType Directory -Force | Out-Null }
        Copy-Item -Path "$SourceResource\*" -Destination $ResourceDir -Recurse -Force
    }
    
    Write-Host "Ghostscript intégré avec succès!" -ForegroundColor Green
} else {
    Write-Host "=== ATTENTION: Ghostscript non inclus ===" -ForegroundColor Red
    Write-Host "Pour une version entièrement portable, vous devez:" -ForegroundColor Yellow
    Write-Host "1. Télécharger Ghostscript depuis https://ghostscript.com/releases/" -ForegroundColor Yellow
    Write-Host "2. Copier les dossiers bin, lib, Resource dans: $GhostscriptDir" -ForegroundColor Yellow
    Write-Host "3. Ou relancer ce script avec -GhostscriptSourcePath" -ForegroundColor Yellow
}

# Créer un fichier README pour les utilisateurs
$ReadmeContent = @"
Color File Converter - Version Portable
======================================

Cette version est entièrement autonome et ne nécessite pas d'installation.

UTILISATION:
1. Copiez ce dossier sur n'importe quel ordinateur Windows 64-bit
2. Lancez Converter.Gui.exe
3. L'application fonctionne immédiatement!

CONTENU:
- Converter.Gui.exe : Application principale
- Ghostscript\ : Moteur de conversion PDF vers TIFF (si inclus)

RÉSEAU:
Cette version peut être placée sur un lecteur réseau et utilisée 
directement par plusieurs utilisateurs sans installation.

CONFIGURATION:
- Les profils et paramètres sont sauvegardés dans le dossier utilisateur
- Chaque utilisateur a ses propres préférences

Version générée le: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

$ReadmeContent | Out-File -FilePath (Join-Path $OutputDir "README.txt") -Encoding UTF8

Write-Host ""
Write-Host "=== VERSION PORTABLE CRÉÉE AVEC SUCCÈS! ===" -ForegroundColor Green
Write-Host "Emplacement: $OutputDir" -ForegroundColor Cyan
Write-Host "Taille de l'application:" -ForegroundColor Cyan

$AppPath = Join-Path $OutputDir "App"
if (Test-Path $AppPath) {
    $Size = (Get-ChildItem -Path $AppPath -Recurse | Measure-Object -Property Length -Sum).Sum
    $SizeMB = [Math]::Round($Size / 1MB, 2)
    Write-Host "  $SizeMB MB" -ForegroundColor White
}

Write-Host ""
Write-Host "Pour déployer sur un réseau:" -ForegroundColor Yellow
Write-Host "1. Copiez le contenu de '$OutputDir' sur votre serveur réseau" -ForegroundColor White
Write-Host "2. Partagez le dossier en lecture pour vos utilisateurs" -ForegroundColor White  
Write-Host "3. Les utilisateurs peuvent lancer directement l'exe depuis le réseau" -ForegroundColor White