@echo off
cd /d "%~dp0"
chcp 65001 >nul
setlocal enabledelayedexpansion

echo.
echo ======================================
echo      PhotoBOX Build Script
echo ======================================
echo.

set "PROJECT=PhotoBOX.App"
set "PUBLISH_DIR=publish"
set "INSTALLER_DIR=installer"
set "INNO_SCRIPT=installer.iss"

echo [1/3] Clean...
if exist "%PUBLISH_DIR%" rmdir /s /q "%PUBLISH_DIR%"
if exist "%INSTALLER_DIR%\*.exe" del /q "%INSTALLER_DIR%\*.exe"

echo [2/3] Build + Publish...
dotnet publish "%PROJECT%\%PROJECT%.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "%PUBLISH_DIR%"

if errorlevel 1 (
    echo.
    echo Build FAILED.
    pause
    exit /b 1
)

echo [3/3] Installer...

set "ISCC="
if exist "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" (
    set "ISCC=C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
) else if exist "C:\Program Files\Inno Setup 6\ISCC.exe" (
    set "ISCC=C:\Program Files\Inno Setup 6\ISCC.exe"
)

if "!ISCC!"=="" (
    echo.
    echo Inno Setup 6 not found.
    echo Install from: https://jrsoftware.org/isdown.php
    pause
    exit /b 1
)

"!ISCC!" "%INNO_SCRIPT%"

if errorlevel 1 (
    echo.
    echo Installer generation FAILED.
    pause
    exit /b 1
)

echo.
echo ======================================
echo   Build complete!
echo   installer/ に Setup.exe が生成されました。
echo ======================================
echo.
pause
