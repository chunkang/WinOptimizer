@echo off
setlocal

set PROJECT=src\WinOptimizer\WinOptimizer.csproj
set CONFIG=Release
set RUNTIME=win-x64
set PUBLISH_DIR=src\WinOptimizer\bin\%CONFIG%\net8.0-windows\%RUNTIME%\publish

:: Check for .NET 8 SDK
echo === Checking prerequisites ===
dotnet --list-sdks 2>nul | findstr /b "8\." >nul 2>&1
if not errorlevel 1 goto :dotnet_ok

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

:dotnet_ok

:: Check for Git (needed for version tagging)
where git >nul 2>&1
if not errorlevel 1 goto :git_ok

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

:git_ok

echo Prerequisites OK.
echo.

:: Clean previous build output
echo === Cleaning previous build output ===
if exist src\WinOptimizer\bin rd /s /q src\WinOptimizer\bin
if exist src\WinOptimizer\obj rd /s /q src\WinOptimizer\obj

:: Increment build number
set BUILD_NUM=0
if exist .buildnumber set /p BUILD_NUM=<.buildnumber
set /a BUILD_NUM+=1
echo %BUILD_NUM%>.buildnumber
echo Build number: %BUILD_NUM%

echo === Restoring dependencies ===
dotnet restore %PROJECT% -r %RUNTIME%
if errorlevel 1 goto :error

echo === Publishing single-file executable ===
dotnet publish %PROJECT% -c %CONFIG% -r %RUNTIME% --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:BuildNumber=%BUILD_NUM% --no-restore
if errorlevel 1 goto :error

echo === Done ===
echo Output: %PUBLISH_DIR%\WinOptimizer.exe
goto :eof

:error
echo === Build failed ===
exit /b 1
