# Script pour creer une version portable OPTIMISEE (plus legere)
param(
    [string]$OutputDir = "$PSScriptRoot\..\Release\ColorFileConverter",
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

Write-Host "Compilation OPTIMISEE (structure propre)..." -ForegroundColor Yellow
Set-Location $ProjectRoot

# Dossier temporaire pour la compilation
$TempDir = Join-Path $OutputDir "Temp"
if (Test-Path $TempDir) {
    Remove-Item $TempDir -Recurse -Force
}

# Publication optimisee vers dossier temporaire
dotnet publish $GuiProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $TempDir `
    /p:PublishSingleFile=false `
    /p:PublishTrimmed=false `
    /p:IncludeNativeLibrariesForSelfExtract=false

if ($LASTEXITCODE -ne 0) {
    throw "Erreur compilation"
}

Write-Host "Reorganisation de la structure..." -ForegroundColor Yellow

# Creer la structure finale
$RuntimeDir = Join-Path $OutputDir "Runtime"
if (Test-Path $RuntimeDir) {
    Remove-Item $RuntimeDir -Recurse -Force
}
New-Item -Path $RuntimeDir -ItemType Directory -Force | Out-Null

# Copier l'executable principal a la racine
$mainExe = Join-Path $TempDir "Converter.Gui.exe"
Copy-Item $mainExe -Destination $OutputDir -Force
Write-Host "  Exe principal: Converter.Gui.exe" -ForegroundColor Cyan

# Copier les fichiers de configuration a la racine
$configFiles = @("Converter.Gui.runtimeconfig.json", "Converter.Gui.deps.json")
foreach ($configFile in $configFiles) {
    $sourcePath = Join-Path $TempDir $configFile
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath -Destination $OutputDir -Force
        Write-Host "  Config: $configFile" -ForegroundColor Cyan
    }
}

# Modifier le runtimeconfig.json pour pointer vers Runtime
$runtimeConfigPath = Join-Path $OutputDir "Converter.Gui.runtimeconfig.json"
if (Test-Path $runtimeConfigPath) {
    $runtimeConfig = Get-Content $runtimeConfigPath -Raw | ConvertFrom-Json
    if (-not $runtimeConfig.runtimeOptions.additionalProbingPaths) {
        $runtimeConfig.runtimeOptions | Add-Member -MemberType NoteProperty -Name "additionalProbingPaths" -Value @("./Runtime")
    } else {
        $runtimeConfig.runtimeOptions.additionalProbingPaths = @("./Runtime")
    }
    $runtimeConfig | ConvertTo-Json -Depth 10 | Set-Content $runtimeConfigPath -Encoding UTF8
    Write-Host "  Config mise a jour pour Runtime" -ForegroundColor Cyan
}

# Copier toutes les dependances vers Runtime (sauf l'exe principal et configs)
$excludeFiles = @("Converter.Gui.exe", "Converter.Gui.runtimeconfig.json", "Converter.Gui.deps.json")
Get-ChildItem -Path $TempDir | Where-Object { $_.Name -notin $excludeFiles } | Copy-Item -Destination $RuntimeDir -Recurse -Force

# Supprimer les fichiers inutiles pour reduire la taille
$filesToRemove = @(
    "*.pdb",           # Fichiers de debug
    "*.xml"            # Documentation XML seulement
)

Write-Host "Nettoyage des fichiers inutiles..." -ForegroundColor Yellow
foreach ($pattern in $filesToRemove) {
    $files = Get-ChildItem -Path $RuntimeDir -Name $pattern -Recurse -ErrorAction SilentlyContinue
    foreach ($file in $files) {
        $fullPath = Join-Path $RuntimeDir $file
        try {
            Remove-Item $fullPath -Force -ErrorAction SilentlyContinue
            Write-Host "  Supprime: $file" -ForegroundColor DarkGray
        } catch { }
    }
}

# Supprimer le dossier temporaire
Remove-Item -Path $TempDir -Recurse -Force

# Integration Ghostscript MINIMALE (seulement les fichiers essentiels)
$GhostscriptSource = Find-GhostscriptInstallation

if ($GhostscriptSource) {
    Write-Host "Integration Ghostscript MINIMALE..." -ForegroundColor Yellow
    
    $GhostscriptDir = Join-Path $RuntimeDir "Ghostscript"
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
Color File Converter - Version Portable OPTIMISEE
================================================

🚀 STRUCTURE PROPRE - EXE FACILEMENT ACCESSIBLE 

STRUCTURE:
- Converter.Gui.exe     : Application principale (a la racine - facile a trouver!)
- Runtime/              : Toutes les dependances et DLLs
- README.txt           : Ce fichier

UTILISATION:
1. Double-cliquez directement sur Converter.Gui.exe
2. Pas besoin de chercher dans des sous-dossiers!
3. Structure propre et professionnelle

FONCTIONNALITES:
✅ Conversion PDF vers TIFF (mono-page et multi-page)  
✅ Apercu avec navigation par pages (◀ ▶)
✅ Interface intuitive avec zoom et pan
✅ Profils personnalisables pour differents besoins
✅ Mode portable - aucune installation requise

AVANTAGES:
- Executable principal visible immediatement
- Dependances organisees dans Runtime/
- Lancement rapide et fiable
- Ideal pour distribution

Version generee: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

$ReadmeContent | Out-File -FilePath (Join-Path $OutputDir "README.txt") -Encoding UTF8

# Statistiques
Write-Host ""
Write-Host "=== VERSION OPTIMISEE CREEE! ===" -ForegroundColor Green
Write-Host "Emplacement: $OutputDir" -ForegroundColor White

if (Test-Path $OutputDir) {
    $totalSize = (Get-ChildItem -Path $OutputDir -Recurse -File | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [Math]::Round($totalSize / 1MB, 2)
    $fileCount = (Get-ChildItem -Path $OutputDir -Recurse -File).Count
    Write-Host "Taille: $sizeMB MB ($fileCount fichiers)" -ForegroundColor White
}

$gsDir = Join-Path $RuntimeDir "Ghostscript"
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
Write-Host "STRUCTURE APPLIQUEE:" -ForegroundColor Yellow
Write-Host "- Converter.Gui.exe visible a la racine" -ForegroundColor White  
Write-Host "- Runtime/ contient toutes les dependances" -ForegroundColor White
Write-Host "- Fichiers debug supprimes" -ForegroundColor White
Write-Host "- Ghostscript integre dans Runtime/" -ForegroundColor White
Write-Host "- Structure professionnelle et propre" -ForegroundColor White