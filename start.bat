@echo off
REM Script de lancement rapide pour AI Content Remover
REM Auteur: GitHub Copilot
REM Date: 2025-01-17

echo.
echo ========================================
echo   AI CONTENT REMOVER - BACKEND API
echo ========================================
echo.

cd /d "%~dp0AIContentRemover"

echo [1/3] Verification de .NET 8.0...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERREUR: .NET 8.0 n'est pas installe!
    echo Telechargez-le sur: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo OK - .NET est installe

echo.
echo [2/3] Restauration des packages NuGet...
dotnet restore >nul 2>&1
if errorlevel 1 (
    echo ERREUR: La restauration des packages a echoue
    pause
    exit /b 1
)
echo OK - Packages restaures

echo.
echo [3/3] Lancement de l'API...
echo.
echo ========================================
echo   API disponible sur:
echo   - HTTPS: https://localhost:5001
echo   - HTTP:  http://localhost:5000
echo   - Swagger UI: https://localhost:5001
echo ========================================
echo.
echo Appuyez sur Ctrl+C pour arreter
echo.

dotnet run

pause

