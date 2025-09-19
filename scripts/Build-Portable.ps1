# Script pour creer une version portable de Color File Converter
param(
    [string]$OutputDir = "$PSScriptRoot\..\Release\Portable",
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

Write-Host "=== Creation version portable ===" -ForegroundColor Green

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

Write-Host "Compilation..." -ForegroundColor Yellow
Set-Location $ProjectRoot

dotnet publish $GuiProject `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output "$OutputDir\App" `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    throw "Erreur compilation"
}

Write-Host "Application compilee!" -ForegroundColor Green

# Integration Ghostscript
$GhostscriptSource = Find-GhostscriptInstallation

if ($GhostscriptSource) {
    Write-Host "Integration Ghostscript..." -ForegroundColor Yellow
    
    $GhostscriptDir = Join-Path $OutputDir "App\Ghostscript"
    New-Item -Path $GhostscriptDir -ItemType Directory -Force | Out-Null
    
    # Copier bin
    $sourceBin = Join-Path $GhostscriptSource "bin"
    if (Test-Path $sourceBin) {
        Write-Host "Copie bin..." -ForegroundColor Cyan
        Copy-Item "$sourceBin\*" -Destination $GhostscriptDir -Recurse -Force
    }
    
    # Copier lib  
    $sourceLib = Join-Path $GhostscriptSource "lib"
    if (Test-Path $sourceLib) {
        Write-Host "Copie lib..." -ForegroundColor Cyan
        $libDir = Join-Path $GhostscriptDir "lib"
        New-Item -Path $libDir -ItemType Directory -Force | Out-Null
        Copy-Item "$sourceLib\*" -Destination $libDir -Recurse -Force
    }
    
    # Copier Resource
    $sourceResource = Join-Path $GhostscriptSource "Resource"
    if (Test-Path $sourceResource) {
        Write-Host "Copie Resource..." -ForegroundColor Cyan
        $resourceDir = Join-Path $GhostscriptDir "Resource"
        New-Item -Path $resourceDir -ItemType Directory -Force | Out-Null
        Copy-Item "$sourceResource\*" -Destination $resourceDir -Recurse -Force
    }
    
    Write-Host "Ghostscript integre!" -ForegroundColor Green
} else {
    Write-Host "Ghostscript non trouve" -ForegroundColor Yellow
}

# Documentation
$ReadmeContent = @"
Color File Converter - Version Portable
=======================================

UTILISATION:
1. Copiez ce dossier sur n'importe quel PC Windows 64-bit
2. Lancez Converter.Gui.exe
3. Ca marche!

RESEAU:
- Copiez sur un lecteur reseau partage
- Les utilisateurs lancent l'exe directement
- Aucune installation requise

CONTENU:
- Converter.Gui.exe : Application
- Ghostscript\ : Moteur de conversion (si present)

Version generee: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

$ReadmeContent | Out-File -FilePath (Join-Path $OutputDir "README.txt") -Encoding UTF8

# Statistiques
Write-Host ""
Write-Host "=== SUCCES! ===" -ForegroundColor Green
Write-Host "Emplacement: $OutputDir" -ForegroundColor White

$AppPath = Join-Path $OutputDir "App"
if (Test-Path $AppPath) {
    $totalSize = (Get-ChildItem -Path $AppPath -Recurse -File | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [Math]::Round($totalSize / 1MB, 2)
    Write-Host "Taille: $sizeMB MB" -ForegroundColor White
}

$gsDir = Join-Path $AppPath "Ghostscript"
$hasGs = Test-Path $gsDir
Write-Host "Ghostscript: $(if($hasGs) { 'Inclus' } else { 'Non inclus' })" -ForegroundColor White

Write-Host ""
Write-Host "Prochaines etapes:" -ForegroundColor Yellow
Write-Host "1. Testez localement" -ForegroundColor White  
Write-Host "2. Copiez App\ sur votre serveur" -ForegroundColor White
Write-Host "3. Partagez en lecture" -ForegroundColor White
Write-Host "4. Les users lancent l'exe!" -ForegroundColor White