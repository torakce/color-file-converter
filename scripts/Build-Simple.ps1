# Script pour creer une version portable SIMPLE qui fonctionne
param(
    [string]$OutputDir = "$PSScriptRoot\..\Release\ColorFileConverterSimple",
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

Write-Host "=== Creation version portable SIMPLE ===" -ForegroundColor Green

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

Write-Host "Compilation SIMPLE et FONCTIONNELLE..." -ForegroundColor Yellow
Set-Location $ProjectRoot

# Publication simple qui fonctionne à tous les coups
dotnet publish $GuiProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $OutputDir `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    /p:IncludeNativeLibrariesForSelfExtract=false

if ($LASTEXITCODE -ne 0) {
    throw "Erreur compilation"
}

Write-Host "Application compilee avec succes!" -ForegroundColor Green

# Supprimer les fichiers inutiles pour reduire la taille
$filesToRemove = @(
    "*.pdb"            # Fichiers de debug seulement
)

Write-Host "Nettoyage des fichiers inutiles..." -ForegroundColor Yellow
foreach ($pattern in $filesToRemove) {
    $files = Get-ChildItem -Path $OutputDir -Name $pattern -Recurse -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        $fullPath = Join-Path $OutputDir $file
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
    
    $GhostscriptDir = Join-Path $OutputDir "Ghostscript"
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
        } else {
            Write-Host "  ERREUR - Fichier manquant: $fileName" -ForegroundColor Red
            Write-Host "  Chemin recherché: $sourcePath" -ForegroundColor Red
            Write-Host ""
            Write-Host "Installation Ghostscript incomplète détectée!" -ForegroundColor Red
            Write-Host "Réinstallez Ghostscript complètement depuis https://ghostscript.com/releases/index.html" -ForegroundColor Yellow
            exit 1
        }
    }
    
    # Copier les ressources Ghostscript ESSENTIELLES SEULEMENT  
    $libSource = Join-Path $GhostscriptSource "lib"
    if (Test-Path $libSource) {
        $libDest = Join-Path $GhostscriptDir "lib"
        New-Item -Path $libDest -ItemType Directory -Force | Out-Null
        
        # Copier SEULEMENT les fichiers PDF essentiels
        $essentialLibFiles = @(
            "ps2pdf.ps",
            "pdf_main.ps", 
            "pdf_sec.ps",
            "pdfwrite.ps",
            "gs_pdfwr.ps",
            "gs_pdf_e.ps",
            "gs_init.ps"
        )
        
        foreach ($libFile in $essentialLibFiles) {
            $srcFile = Join-Path $libSource $libFile
            if (Test-Path $srcFile) {
                Copy-Item $srcFile -Destination $libDest -Force
                Write-Host "  Lib essentiel: $libFile" -ForegroundColor DarkCyan
            }
        }
        
        # Resource dir minimal
        $resourceSource = Join-Path $GhostscriptSource "Resource"  
        $resourceDest = Join-Path $GhostscriptDir "Resource"
        if (Test-Path $resourceSource) {
            # Copier SEULEMENT Init (configuration de base)
            $initSource = Join-Path $resourceSource "Init" 
            $initDest = Join-Path $resourceDest "Init"
            if (Test-Path $initSource) {
                Copy-Item $initSource -Destination $initDest -Recurse -Force
                Write-Host "  Resources Init copiees" -ForegroundColor DarkCyan
            }
        }
    }
    
    Write-Host "Ghostscript MINIMAL integre!" -ForegroundColor Green
} else {
    Write-Host "ERREUR: Ghostscript est REQUIS pour créer une release complète!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Solutions:" -ForegroundColor Yellow  
    Write-Host "1. Installez Ghostscript depuis https://ghostscript.com/releases/index.html" -ForegroundColor White
    Write-Host "2. Vérifiez que gswin64c.exe est accessible dans le PATH" -ForegroundColor White
    Write-Host "3. Ou installez-le dans C:\Program Files\gs\" -ForegroundColor White
    Write-Host ""
    Write-Host "Une release sans Ghostscript intégré obligerait les utilisateurs" -ForegroundColor Red
    Write-Host "à installer Ghostscript eux-mêmes, ce qui va à l'encontre de" -ForegroundColor Red  
    Write-Host "l'objectif d'une version portable autonome." -ForegroundColor Red
    Write-Host ""
    Write-Host "Build interrompu. Installez Ghostscript et relancez." -ForegroundColor Red
    exit 1
}

# Documentation
$ReadmeContent = @"
Color File Converter - Version Portable SIMPLE ET FONCTIONNELLE
==============================================================

🚀 STRUCTURE SIMPLE - TOUT DANS UN DOSSIER

CONTENU:
- Converter.Gui.exe     : Application principale (lancez ce fichier!)
- Toutes les DLLs       : Dependencies .NET dans le meme dossier  
- Ghostscript/          : Conversion PDF vers TIFF
- README.txt           : Ce fichier

UTILISATION:
1. Double-cliquez directement sur Converter.Gui.exe
2. L'application se lance immediatement
3. Structure simple et fiable

FONCTIONNALITES:
✅ Conversion PDF vers TIFF (mono-page et multi-page)  
✅ Apercu avec navigation par pages (◀ ▶)
✅ Interface intuitive avec zoom et pan
✅ Profils personnalisables pour differents besoins
✅ Mode portable - aucune installation requise

AVANTAGES:
- Executable principal facilement identifiable
- Toutes les dependencies au meme endroit
- Lancement garanti sans probleme de chemin
- Structure eprouvee et fiable

Version generee: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

$ReadmeContent | Out-File -FilePath (Join-Path $OutputDir "README.txt") -Encoding UTF8

# Statistiques
Write-Host ""
Write-Host "=== VERSION SIMPLE CREEE! ===" -ForegroundColor Green
Write-Host "Emplacement: $OutputDir" -ForegroundColor White

if (Test-Path $OutputDir) {
    $totalSize = (Get-ChildItem -Path $OutputDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [Math]::Round($totalSize / 1MB, 2)
    $fileCount = (Get-ChildItem -Path $OutputDir -Recurse -File).Count
    Write-Host "Taille: $sizeMB MB ($fileCount fichiers)" -ForegroundColor White
}

$gsDir = Join-Path $OutputDir "Ghostscript"
$hasGs = Test-Path $gsDir
$gsExe = Join-Path $gsDir "gswin64c.exe"
$hasGsExe = Test-Path $gsExe

if ($hasGs -and $hasGsExe) {
    Write-Host "Ghostscript: ✅ Correctement intégré" -ForegroundColor Green
} else {
    Write-Host "Ghostscript: ❌ MANQUANT - Build échoué!" -ForegroundColor Red
    Write-Host "La release n'est pas autonome sans Ghostscript intégré." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "STRUCTURE SIMPLE:" -ForegroundColor Yellow
Write-Host "- Converter.Gui.exe a la racine (tres visible)" -ForegroundColor White  
Write-Host "- Toutes les DLLs dans le meme dossier" -ForegroundColor White
Write-Host "- Ghostscript/ pour les conversions" -ForegroundColor White
Write-Host "- Structure simple et eprouvee" -ForegroundColor White

Write-Host ""
Write-Host "TEST IMMEDIAT:" -ForegroundColor Green
Write-Host "cd `"$OutputDir`"" -ForegroundColor Cyan
Write-Host ".\Converter.Gui.exe" -ForegroundColor Cyan