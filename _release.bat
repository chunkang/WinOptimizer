REM ============================================================================
REM WinOptimizer — AGPL-3.0 + Commons Clause
REM Author:  Chun Kang <ck@ckii.com>
REM Modified: Claude (AI-assisted) (2026-03-24)
REM ============================================================================

@echo off
setlocal enabledelayedexpansion

:: Ensure we're in the repo root
if not exist src\WinOptimizer\WinOptimizer.csproj (
    echo ERROR: Run this script from the repository root.
    exit /b 1
)

:: Ensure gh CLI is on PATH
set "PATH=%ProgramFiles%\GitHub CLI;%LOCALAPPDATA%\GitHub CLI;%PATH%"
where gh >nul 2>&1
if errorlevel 1 (
    echo GitHub CLI not found. Installing via winget...
    where winget >nul 2>&1
    if errorlevel 1 (
        echo ERROR: winget is not available. Please install GitHub CLI manually from https://cli.github.com
        exit /b 1
    )
    winget install GitHub.cli --accept-source-agreements --accept-package-agreements
    if errorlevel 1 (
        echo ERROR: Failed to install GitHub CLI.
        exit /b 1
    )
    set "PATH=%ProgramFiles%\GitHub CLI;%LOCALAPPDATA%\GitHub CLI;%PATH%"
)

:: Check for uncommitted changes
git diff --quiet 2>nul
if errorlevel 1 (
    echo ERROR: You have uncommitted changes. Commit or stash them first.
    exit /b 1
)
git diff --cached --quiet 2>nul
if errorlevel 1 (
    echo ERROR: You have staged changes. Commit or stash them first.
    exit /b 1
)

:: Get version from git describe
for /f "tokens=*" %%i in ('git describe --tags --long --match "v[0-9]*" 2^>nul') do set GIT_DESCRIBE=%%i
if not defined GIT_DESCRIBE (
    echo ERROR: No version tags found. Create one first: git tag v0.1.0
    exit /b 1
)
echo Git describe: %GIT_DESCRIBE%

:: Parse version: v0.1.5-3-gabcdef → 0.1.8
for /f "tokens=1-5 delims=.-" %%a in ("%GIT_DESCRIBE:~1%") do (
    set MAJOR=%%a
    set MINOR=%%b
    set TAG_PATCH=%%c
    set COMMITS=%%d
)
set /a PATCH=!TAG_PATCH!+!COMMITS!
set VERSION=!MAJOR!.!MINOR!.!PATCH!
set TAG=v!VERSION!
echo Version: !VERSION! (tag: !TAG!)

:: Build first (while existing tag is still intact for correct version)
echo.
echo === Building release ===
call _build.bat
if errorlevel 1 (
    echo ERROR: Build failed.
    exit /b 1
)

set PUBLISH_DIR=src\WinOptimizer\bin\Release\net8.0-windows\win-x64\publish
set EXE=!PUBLISH_DIR!\WinOptimizer.exe

if not exist "!EXE!" (
    echo ERROR: Published executable not found at !EXE!
    exit /b 1
)

:: Create archives
echo.
echo === Creating archives ===
set SRC_ZIP=WinOptimizer-!VERSION!-source.zip
set BIN_ZIP=WinOptimizer-!VERSION!-win-x64.zip

if exist "!SRC_ZIP!" del "!SRC_ZIP!"
if exist "!BIN_ZIP!" del "!BIN_ZIP!"

git archive --format=zip --prefix=WinOptimizer-!VERSION!/ -o "!SRC_ZIP!" HEAD
if errorlevel 1 (
    echo ERROR: Failed to create source archive.
    exit /b 1
)
echo Created: !SRC_ZIP!

powershell -Command "Compress-Archive -Path '!EXE!' -DestinationPath '!BIN_ZIP!'"
if errorlevel 1 (
    echo ERROR: Failed to create binary archive.
    exit /b 1
)
echo Created: !BIN_ZIP!

:: Delete existing tag and release AFTER build (so build gets correct version)
git tag -l "!TAG!" | findstr /r "." >nul 2>&1
if not errorlevel 1 (
    echo.
    echo === Replacing existing release !TAG! ===
    gh release delete !TAG! --yes >nul 2>&1
    git tag -d !TAG! >nul 2>&1
    git push origin :refs/tags/!TAG! >nul 2>&1
)

:: Tag and push
echo.
echo === Creating tag !TAG! ===
git tag !TAG!
if errorlevel 1 (
    echo ERROR: Failed to create tag.
    exit /b 1
)
git push origin !TAG!
if errorlevel 1 (
    echo ERROR: Failed to push tag. Cleaning up local tag...
    git tag -d !TAG! >nul 2>&1
    exit /b 1
)
echo Tag !TAG! pushed.

:: Create GitHub release
echo.
echo === Creating GitHub release ===
gh release create !TAG! "!EXE!" "!SRC_ZIP!" "!BIN_ZIP!" --title "WinOptimizer !TAG!" --generate-notes
if errorlevel 1 (
    echo ERROR: Failed to create GitHub release.
    exit /b 1
)

:: Cleanup local archives
del "!SRC_ZIP!" 2>nul
del "!BIN_ZIP!" 2>nul

echo.
echo === Release !TAG! published successfully ===
