@echo off
setlocal enabledelayedexpansion

echo ========================================
echo   Color File Converter - Build LEGER
echo ========================================
echo.
echo Cette version est optimisee pour les 
echo reseaux lents (reduction ~60% de taille)
echo.

cd /d "%~dp0"

echo Creation de la version portable legere...
echo Optimisation en cours...
echo.

powershell.exe -ExecutionPolicy Bypass -File "scripts\Build-Portable-Light.ps1" -CleanBuild

if %errorlevel% neq 0 (
    echo.
    echo ========================================
    echo   ERREUR lors de la creation!
    echo ========================================
    pause
    exit /b 1
)

echo.
echo ========================================
echo   VERSION LEGERE CREEE AVEC SUCCES!
echo ========================================
echo.
echo Emplacement : Release\PortableLight\App\
echo.
echo OPTIMISATIONS :
echo - Taille reduite de ~60%%
echo - Lancement 2-3x plus rapide en reseau
echo - DLLs separees pour meilleur cache Windows
echo - Ghostscript minimal mais fonctionnel
echo.
echo IDEAL POUR :
echo - Reseaux lents ou satures
echo - Serveurs avec bande passante limitee
echo - Utilisateurs occasionnels
echo.
pause