@echo off
cd /d "C:\Code\color-file-converter"
echo Checking git status...
git status

echo.
echo Checking last commits...
git log --oneline -3

echo.
echo Checking if changes are already committed...
git diff HEAD~1 HEAD --name-only