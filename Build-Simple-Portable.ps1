param(
    [string]$OutputPath = "Release/SimpleWorking"
)

Write-Host "=== Build Color File Converter Portable ===" -ForegroundColor Green

# 1. Build de l'application
Write-Host "1. Build de l'application..." -ForegroundColor Yellow
dotnet publish Converter.Gui --configuration Release --runtime win-x64 --self-contained true --output $OutputPath /p:PublishSingleFile=false /p:PublishTrimmed=false

if ($LASTEXITCODE -ne 0) {
    Write-Error "Erreur lors du build"
    exit 1
}

# 2. Copie des binaires Ghostscript pre-integres
Write-Host "2. Ajout de Ghostscript portable..." -ForegroundColor Yellow

$gsSource = "Resources/Ghostscript"
$gsTarget = "$OutputPath/Ghostscript"

if (Test-Path $gsSource) {
    Copy-Item $gsSource "$gsTarget" -Recurse -Force
    Write-Host "   Ghostscript copie depuis le repository" -ForegroundColor Green
} else {
    New-Item -ItemType Directory -Path $gsTarget -Force | Out-Null
    
    Write-Host "   Dossier Ghostscript cree" -ForegroundColor Yellow
    Write-Host "   Pour inclure Ghostscript, ajoutez les binaires dans: $gsSource" -ForegroundColor Cyan
    Write-Host "      Fichiers necessaires:" -ForegroundColor Cyan
    Write-Host "      - gswin64c.exe" -ForegroundColor Gray
    Write-Host "      - gsdll64.dll" -ForegroundColor Gray
    
    "# Remplacez ce fichier par le vrai gswin64c.exe" | Out-File "$gsTarget/gswin64c.exe.placeholder" -Encoding UTF8
    "# Remplacez ce fichier par le vrai gsdll64.dll" | Out-File "$gsTarget/gsdll64.dll.placeholder" -Encoding UTF8
}

# 3. Resume
Write-Host ""
Write-Host "=== Resume ===" -ForegroundColor Green
$files = Get-ChildItem $OutputPath -Recurse -File
$totalSize = ($files | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Release creee: $OutputPath" -ForegroundColor White
Write-Host "Fichiers: $($files.Count)" -ForegroundColor White
Write-Host "Taille: $([math]::Round($totalSize, 1)) MB" -ForegroundColor White

if ((Test-Path "$gsTarget/gswin64c.exe") -and ((Get-Item "$gsTarget/gswin64c.exe" -ErrorAction SilentlyContinue).Length -gt 100)) {
    Write-Host "SUCCESS: Ghostscript inclus et pret !" -ForegroundColor Green
} else {
    Write-Host "WARNING: Ghostscript manquant - voir Resources/Ghostscript/README.md" -ForegroundColor Yellow
}