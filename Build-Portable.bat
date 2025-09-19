@echo off
setlocal enabledelayedexpansion

echo ========================================
echo   Color File Converter - Build Portable
echo ========================================
echo.
echo Cette operation va creer une version 
echo entierement autonome du logiciel.
echo.

cd /d "%~dp0"

echo Creation de la version portable...
echo Cela peut prendre quelques minutes...
echo.

powershell.exe -ExecutionPolicy Bypass -File "scripts\Build-Portable.ps1" -CleanBuild

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo   ERREUR lors de la creation!
    echo ========================================
    echo.
    echo Verifiez que :
    echo - .NET 8 SDK est installe
    echo - PowerShell est disponible
    echo - Vous avez les droits d'ecriture
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo   VERSION PORTABLE CREEE AVEC SUCCES!
echo ========================================
echo.
echo Emplacement : Release\Portable\App\
echo.
echo PROCHAINES ETAPES :
echo 1. Testez l'application localement
echo 2. Copiez le dossier App sur votre serveur
echo 3. Partagez en lecture pour vos utilisateurs
echo 4. Les users lancent Converter.Gui.exe !
echo.
echo La version inclut Ghostscript (pas besoin d'install)
echo et fonctionne sur n'importe quel PC Windows 64-bit.
echo.
pause