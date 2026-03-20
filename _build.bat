@echo off
setlocal

set PROJECT=src\WinOptimizer\WinOptimizer.csproj
set CONFIG=Release
set RUNTIME=win-x64
set PUBLISH_DIR=src\WinOptimizer\bin\%CONFIG%\net8.0-windows\%RUNTIME%\publish

:: Check for .NET 8 SDK
echo === Checking prerequisites ===
dotnet --list-sdks 2>nul | findstr /b "8\." >nul 2>&1
if errorlevel 1 (
    echo .NET 8 SDK not found. Installing via winget...
    where winget >nul 2>&1
    if errorlevel 1 (
        echo ERROR: winget is not available. Please install .NET 8 SDK manually from https://dotnet.microsoft.com/download/dotnet/8.0
        exit /b 1
    )
    winget install Microsoft.DotNet.SDK.8 --accept-source-agreements --accept-package-agreements
    if errorlevel 1 (
        echo ERROR: Failed to install .NET 8 SDK.
        exit /b 1
    )
    echo Refreshing PATH...
    set "PATH=%ProgramFiles%\dotnet;%PATH%"
)

:: Check for Git (needed for version tagging)
where git >nul 2>&1
if errorlevel 1 (
    echo Git not found. Installing via winget...
    where winget >nul 2>&1
    if errorlevel 1 (
        echo ERROR: winget is not available. Please install Git manually from https://git-scm.com
        exit /b 1
    )
    winget install Git.Git --accept-source-agreements --accept-package-agreements
    if errorlevel 1 (
        echo ERROR: Failed to install Git.
        exit /b 1
    )
    set "PATH=%ProgramFiles%\Git\cmd;%PATH%"
)

echo Prerequisites OK.
echo.

echo === Restoring dependencies ===
dotnet restore %PROJECT%
if errorlevel 1 goto :error

echo === Building ===
dotnet build %PROJECT% -c %CONFIG% --no-restore
if errorlevel 1 goto :error

echo === Publishing single-file executable ===
dotnet publish %PROJECT% -c %CONFIG% -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
if errorlevel 1 goto :error

echo === Done ===
echo Output: %PUBLISH_DIR%\WinOptimizer.exe
goto :eof

:error
echo === Build failed ===
exit /b 1
