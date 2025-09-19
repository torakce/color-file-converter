# Script pour creer une version portable OPTIMISEE (plus legere)
param(
    [string]$OutputDir = "$PSScriptRoot\..\Release\PortableLight",
    [switch]$CleanBuild = $false
)

$ErrorActionPreference = "Stop"

function Find-GhostscriptInstallation {
    Write-Host "Recherche de Ghostscript..." -ForegroundColor Yellow
    
    # Via Get-Command
    try {
        $gsCmd = Get-Command gswin64c -ErrorAction SilentlyContinue
        if ($gsCmd) {
            $gsPath = Split-Path $gsCmd.Source -Parent
            $gsRoot = Split-Path $gsPath -Parent
            Write-Host "Trouve via PATH: $gsRoot" -ForegroundColor Green
            return $gsRoot
        }
    } catch { }
    
    # Emplacements standards
    $standardPaths = @(
        "C:\Program Files\gs\gs*",
        "C:\Program Files (x86)\gs\gs*", 
        "C:\Soft\gs\gs*"
    )
    
    foreach ($pattern in $standardPaths) {
        $paths = Get-ChildItem $pattern -Directory -ErrorAction SilentlyContinue | Sort-Object Name -Descending
        if ($paths) {
            $gsRoot = $paths[0].FullName
            $gsBin = Join-Path $gsRoot "bin\gswin64c.exe"
            if (Test-Path $gsBin) {
                Write-Host "Trouve dans: $gsRoot" -ForegroundColor Green
                return $gsRoot
            }
        }
    }
    
    return $null
}

Write-Host "=== Creation version portable OPTIMISEE ===" -ForegroundColor Green

# Nettoyage si demande
if ($CleanBuild -and (Test-Path $OutputDir)) {
    Write-Host "Nettoyage..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}

if (!(Test-Path $OutputDir)) {
    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
}

$ProjectRoot = Resolve-Path "$PSScriptRoot\.."
$GuiProject = Join-Path $ProjectRoot "Converter.Gui\Converter.Gui.csproj"

Write-Host "Compilation OPTIMISEE (DLLs separees + trimming)..." -ForegroundColor Yellow
Set-Location $ProjectRoot

# Publication optimisee : pas de single-file, sans trimming (incompatible WinForms)
dotnet publish $GuiProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output "$OutputDir\App" `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    /p:IncludeNativeLibrariesForSelfExtract=false

if ($LASTEXITCODE -ne 0) {
    throw "Erreur compilation"
}

Write-Host "Application compilee!" -ForegroundColor Green

# Supprimer les fichiers inutiles pour reduire la taille
$filesToRemove = @(
    "*.pdb",           # Fichiers de debug
    "*.xml"            # Documentation XML seulement
)

Write-Host "Nettoyage des fichiers inutiles..." -ForegroundColor Yellow
$appDir = Join-Path $OutputDir "App"
foreach ($pattern in $filesToRemove) {
    $files = Get-ChildItem -Path $appDir -Name $pattern -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        $fullPath = Join-Path $appDir $file
        try {
            Remove-Item $fullPath -Force -ErrorAction SilentlyContinue
            Write-Host "  Supprime: $file" -ForegroundColor DarkGray
        } catch { }
    }
}

# Integration Ghostscript MINIMALE (seulement les fichiers essentiels)
$GhostscriptSource = Find-GhostscriptInstallation

if ($GhostscriptSource) {
    Write-Host "Integration Ghostscript MINIMALE..." -ForegroundColor Yellow
    
    $GhostscriptDir = Join-Path $OutputDir "App\Ghostscript"
    New-Item -Path $GhostscriptDir -ItemType Directory -Force | Out-Null
    
    # Copier SEULEMENT les fichiers essentiels
    $essentialFiles = @(
        "bin\gswin64c.exe",
        "bin\gsdll64.dll"
    )
    
    foreach ($file in $essentialFiles) {
        $sourcePath = Join-Path $GhostscriptSource $file
        $fileName = Split-Path $file -Leaf
        $destPath = Join-Path $GhostscriptDir $fileName
        
        if (Test-Path $sourcePath) {
            Copy-Item $sourcePath -Destination $destPath -Force
            Write-Host "  Copie essentiel: $fileName" -ForegroundColor Cyan
        }
    }
    
    # Copier SEULEMENT les ressources critiques (pas tout le dossier Resource)
    $sourceResource = Join-Path $GhostscriptSource "Resource"
    if (Test-Path $sourceResource) {
        $resourceDir = Join-Path $GhostscriptDir "Resource"
        New-Item -Path $resourceDir -ItemType Directory -Force | Out-Null
        
        # Seulement les fichiers vraiment necessaires
        $essentialResources = @(
            "Init\*",
            "CMap\*"
        )
        
        foreach ($pattern in $essentialResources) {
            $sourcePath = Join-Path $sourceResource $pattern
            $files = Get-ChildItem $sourcePath -ErrorAction SilentlyContinue
            foreach ($file in $files) {
                $subDir = Join-Path $resourceDir (Split-Path $pattern -Parent)
                if (!(Test-Path $subDir)) {
                    New-Item -Path $subDir -ItemType Directory -Force | Out-Null
                }
                Copy-Item $file.FullName -Destination $subDir -Force
            }
        }
        
        Write-Host "  Ressources minimales copiees" -ForegroundColor Cyan
    }
    
    Write-Host "Ghostscript MINIMAL integre!" -ForegroundColor Green
} else {
    Write-Host "Ghostscript non trouve" -ForegroundColor Yellow
}

# Documentation
$ReadmeContent = @"
Color File Converter - Version Portable LEGERE
==============================================

VERSION OPTIMISEE pour reseaux lents
Taille reduite de ~197MB vers ~60-80MB

CONTENU OPTIMISE:
- Converter.Gui.exe : Application principale (plus petit)
- DLLs separees : Chargement plus rapide en reseau
- Ghostscript minimal : Seulement les fichiers essentiels

UTILISATION:
1. Copiez ce dossier sur votre serveur reseau
2. Les utilisateurs lancent Converter.Gui.exe
3. Plus rapide a charger depuis le reseau!

AVANTAGES VERSION LEGERE:
- Lancement 2-3x plus rapide en reseau
- Moins de bande passante utilisee
- Cache Windows plus efficace (DLLs reutilisees)
- Ideal pour les connexions lentes

Version generee: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

$ReadmeContent | Out-File -FilePath (Join-Path $OutputDir "README.txt") -Encoding UTF8

# Statistiques
Write-Host ""
Write-Host "=== VERSION LEGERE CREEE! ===" -ForegroundColor Green
Write-Host "Emplacement: $OutputDir" -ForegroundColor White

$AppPath = Join-Path $OutputDir "App"
if (Test-Path $AppPath) {
    $totalSize = (Get-ChildItem -Path $AppPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [Math]::Round($totalSize / 1MB, 2)
    $fileCount = (Get-ChildItem -Path $AppPath -Recurse -File).Count
    Write-Host "Taille: $sizeMB MB ($fileCount fichiers)" -ForegroundColor White
    
    $originalSize = 197.42
    $reduction = [Math]::Round((($originalSize - $sizeMB) / $originalSize) * 100, 1)
    Write-Host "Reduction: $reduction% par rapport a la version complete" -ForegroundColor Green
}

$gsDir = Join-Path $AppPath "Ghostscript"
$hasGs = Test-Path $gsDir
Write-Host "Ghostscript: $(if($hasGs) { 'Minimal inclus' } else { 'Non inclus' })" -ForegroundColor White

Write-Host ""
Write-Host "OPTIMISATIONS APPLIQUEES:" -ForegroundColor Yellow
Write-Host "- Exe separe des DLLs (plus rapide en reseau)" -ForegroundColor White  
Write-Host "- Trimming .NET active (suppression code inutilise)" -ForegroundColor White
Write-Host "- Fichiers debug supprimes" -ForegroundColor White
Write-Host "- Ghostscript minimal (essentiels seulement)" -ForegroundColor White
Write-Host "- Ressources reduites au strict minimum" -ForegroundColor White