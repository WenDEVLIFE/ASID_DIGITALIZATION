@echo off
echo ============================================
echo   ASID Edge - Build ^& Package
echo ============================================
echo.

REM Check if dotnet is available
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo [ERROR] dotnet CLI not found!
    echo Install .NET SDK from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Run the build script
powershell -ExecutionPolicy Bypass -File "%~dp0build.ps1" %*

echo.
echo Press any key to exit...
pause >nul
