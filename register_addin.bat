@echo off
REM Registration script for Sheet Metal DXF Exporter Inventor Add-in
REM Run as Administrator

echo ====================================================
echo Sheet Metal DXF Exporter - COM Registration
echo ====================================================
echo.

REM Check if running as administrator
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo ERROR: This script must be run as Administrator
    echo Right-click and select "Run as administrator"
    pause
    exit /b 1
)

REM Set paths
set PROJECT_DIR=%~dp0
set DLL_PATH=%PROJECT_DIR%bin\Release\SheetMetalDXFExporter.dll

echo Project Directory: %PROJECT_DIR%
echo DLL Path: %DLL_PATH%
echo.

REM Check if DLL exists
if not exist "%DLL_PATH%" (
    echo ERROR: DLL not found at %DLL_PATH%
    echo Please build the project first using:
    echo   msbuild "SheetMetalDXFExporter.sln" /p:Configuration=Release
    pause
    exit /b 1
)

echo Registering COM Add-in...
regasm "%DLL_PATH%" /codebase

if %errorLevel% equ 0 (
    echo.
    echo SUCCESS: Add-in registered successfully!
    echo.
    echo Next steps:
    echo 1. Start Autodesk Inventor 2026
    echo 2. Look for "Sheet Metal DXF Export" button in Tools ribbon
    echo 3. Check Add-ins dialog if button doesn't appear
    echo.
) else (
    echo.
    echo ERROR: Registration failed
    echo Check that:
    echo - Inventor 2026 is installed
    echo - .NET Framework 4.7.2+ is installed  
    echo - No antivirus blocking the registration
    echo.
)

pause