# Script intelligent pour créer une version portable avec auto-détection de Ghostscript

param(
    [string]$OutputDir = "$PSScriptRoot\..\Release\Portable",
    [switch]$CleanBuild = $false,
    [switch]$NoGhostscript = $false
)

$ErrorActionPreference = "Stop"

function Find-GhostscriptInstallation {
    Write-Host "Recherche de l'installation Ghostscript..." -ForegroundColor Yellow
    
    # Méthode 1: Via Get-Command (PATH)
    try {
        $gsCmd = Get-Command gswin64c -ErrorAction SilentlyContinue
        if ($gsCmd) {
            $gsPath = Split-Path $gsCmd.Source -Parent
            $gsRoot = Split-Path $gsPath -Parent
            Write-Host "  Trouvé via PATH: $gsRoot" -ForegroundColor Green
            return $gsRoot
        }
    } catch { }
    
    # Méthode 2: Registre Windows (installation standard)
    try {
        $regPaths = @(
            "HKLM:\SOFTWARE\GPL Ghostscript",
            "HKLM:\SOFTWARE\WOW6432Node\GPL Ghostscript",
            "HKLM:\SOFTWARE\AFPL Ghostscript",
            "HKLM:\SOFTWARE\WOW6432Node\AFPL Ghostscript"
        )
        
        foreach ($regPath in $regPaths) {
            if (Test-Path $regPath) {
                $versions = Get-ChildItem $regPath -ErrorAction SilentlyContinue
                if ($versions) {
                    $latestVersion = $versions | Sort-Object Name -Descending | Select-Object -First 1
                    $gsPath = (Get-ItemProperty $latestVersion.PSPath -Name GS_DLL -ErrorAction SilentlyContinue).GS_DLL
                    if ($gsPath -and (Test-Path $gsPath)) {
                        $gsRoot = Split-Path (Split-Path $gsPath -Parent) -Parent
                        Write-Host "  Trouvé via registre: $gsRoot" -ForegroundColor Green
                        return $gsRoot
                    }
                }
            }
        }
    } catch { }
    
    # Méthode 3: Emplacements standards
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
                Write-Host "  Trouvé dans: $gsRoot" -ForegroundColor Green
                return $gsRoot
            }
        }
    }
    
    Write-Host "  Ghostscript non trouvé automatiquement" -ForegroundColor Red
    return $null
}

function Test-GhostscriptFiles {
    param([string]$GsPath)
    
    $requiredFiles = @(
        "bin\gswin64c.exe",
        "bin\gsdll64.dll"
    )
    
    foreach ($file in $requiredFiles) {
        $fullPath = Join-Path $GsPath $file
        if (!(Test-Path $fullPath)) {
            Write-Host "  Fichier manquant: $file" -ForegroundColor Red
            return $false
        }
    }
    return $true
}

Write-Host "=== Création de la version portable de Color File Converter ===" -ForegroundColor Green
Write-Host "Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Gray
Write-Host ""

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

Write-Host "✓ Application compilée avec succès!" -ForegroundColor Green

# Gestion de Ghostscript
if (!$NoGhostscript) {
    $GhostscriptSource = Find-GhostscriptInstallation
    
    if ($GhostscriptSource -and (Test-GhostscriptFiles $GhostscriptSource)) {
        Write-Host ""
        Write-Host "Intégration de Ghostscript portable..." -ForegroundColor Yellow
        
        $GhostscriptDir = Join-Path $OutputDir "App\Ghostscript"
        if (!(Test-Path $GhostscriptDir)) {
            New-Item -Path $GhostscriptDir -ItemType Directory -Force | Out-Null
        }
        
        # Copier les fichiers essentiels
        $filesToCopy = @{
            "bin" = @("*.exe", "*.dll", "*.cmd")
            "lib" = @("*")
            "Resource" = @("*")
        }
        
        foreach ($folder in $filesToCopy.Keys) {
            $sourcePath = Join-Path $GhostscriptSource $folder
            if (Test-Path $sourcePath) {
                Write-Host "  Copie du dossier $folder..." -ForegroundColor Cyan
                
                $targetPath = if ($folder -eq "bin") { $GhostscriptDir } else { Join-Path $GhostscriptDir $folder }
                if (!(Test-Path $targetPath)) {
                    New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
                }
                
                foreach ($pattern in $filesToCopy[$folder]) {
                    $sourcePattern = Join-Path $sourcePath $pattern
                    Get-ChildItem $sourcePattern -ErrorAction SilentlyContinue | ForEach-Object {
                        Copy-Item $_.FullName -Destination $targetPath -Recurse -Force
                    }
                }
            }
        }
        
        Write-Host "✓ Ghostscript intégré avec succès!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "⚠️  Ghostscript non trouvé ou incomplet" -ForegroundColor Yellow
        Write-Host "La version portable fonctionnera mais nécessitera Ghostscript installé sur les postes cibles." -ForegroundColor Yellow
    }
} else {
    Write-Host "Ghostscript ignoré (paramètre -NoGhostscript)" -ForegroundColor Yellow
}

# Créer un fichier de documentation
$ReadmeContent = @"
Color File Converter - Version Portable
======================================

Version générée le: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

INSTALLATION:
Cette version est entièrement autonome et ne nécessite AUCUNE installation!

UTILISATION:
1. Copiez ce dossier complet sur n'importe quel ordinateur Windows 64-bit
2. Lancez Converter.Gui.exe directement
3. L'application fonctionne immédiatement!

DÉPLOIEMENT RÉSEAU:
✓ Parfait pour les réseaux d'entreprise
✓ Copiez sur un lecteur réseau partagé
✓ Les utilisateurs lancent l'exe directement depuis le réseau
✓ Aucune installation requise sur les postes clients
✓ Chaque utilisateur garde ses propres préférences

CONTENU DU PACKAGE:
- Converter.Gui.exe     : Application principale (.NET inclus)
- Ghostscript\          : Moteur de conversion PDF vers TIFF (si inclus)
- README.txt            : Ce fichier

CONFIGURATION:
- Les profils de conversion sont sauvegardés dans le dossier utilisateur
- Chaque utilisateur a ses propres paramètres et préférences
- Aucune configuration système nécessaire

DÉPANNAGE:
Si la conversion ne fonctionne pas:
1. Vérifiez que le dossier Ghostscript\ est présent
2. Ou installez Ghostscript sur le poste (https://ghostscript.com)
3. Ou définissez la variable GHOSTSCRIPT_EXE

SUPPORT:
Version .NET 8 incluse - Compatible Windows 10/11 (64-bit)
"@

$ReadmeContent | Out-File -FilePath (Join-Path $OutputDir "README.txt") -Encoding UTF8

# Statistiques finales
Write-Host ""
Write-Host "=== VERSION PORTABLE CRÉÉE AVEC SUCCÈS! ===" -ForegroundColor Green
Write-Host "Emplacement: " -ForegroundColor Cyan -NoNewline
Write-Host $OutputDir -ForegroundColor White

$AppPath = Join-Path $OutputDir "App"
if (Test-Path $AppPath) {
    $files = Get-ChildItem -Path $AppPath -Recurse -File
    $totalSize = ($files | Measure-Object -Property Length -Sum).Sum
    $sizeMB = [Math]::Round($totalSize / 1MB, 2)
    $fileCount = $files.Count
    
    Write-Host "Taille totale: " -ForegroundColor Cyan -NoNewline
    Write-Host "$sizeMB MB ($fileCount fichiers)" -ForegroundColor White
}

$gsDir = Join-Path $AppPath "Ghostscript"
$hasGhostscript = Test-Path $gsDir
Write-Host "Ghostscript inclus: " -ForegroundColor Cyan -NoNewline
if ($hasGhostscript) {
    Write-Host "Oui" -ForegroundColor Green
} else {
    Write-Host "Non" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "PROCHAINES ETAPES:" -ForegroundColor Yellow
Write-Host "1. Testez la version portable localement" -ForegroundColor White
Write-Host "2. Copiez le dossier App sur votre serveur reseau" -ForegroundColor White
Write-Host "3. Partagez-le en lecture pour vos utilisateurs" -ForegroundColor White
Write-Host "4. Les utilisateurs peuvent lancer Converter.Gui.exe directement!" -ForegroundColor White

Write-Host ""