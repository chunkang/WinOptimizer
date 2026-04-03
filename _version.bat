@echo off
REM ============================================================================
REM WinOptimizer Version Configuration
REM The build system (MSBuild) reads VERSION_MAJOR/MINOR/BUILD from this file.
REM Run this script directly to update the version interactively.
REM ============================================================================

SET VERSION_MAJOR=0
SET VERSION_MINOR=2
SET VERSION_BUILD=5

REM --- If called with /q flag, skip interactive mode (for CI/build) ---
if "%~1"=="/q" goto :eof

REM --- Interactive mode ---
echo.
echo  ======================================
echo   WinOptimizer Version Manager
echo  ======================================
echo.
echo   Current version: %VERSION_MAJOR%.%VERSION_MINOR%.%VERSION_BUILD%
echo.
echo  --------------------------------------
echo   Enter new version (leave blank to keep current value)
echo  --------------------------------------
echo.

set /p "NEW_MAJOR=  Major [%VERSION_MAJOR%]: "
if "%NEW_MAJOR%"=="" set "NEW_MAJOR=%VERSION_MAJOR%"

set /p "NEW_MINOR=  Minor [%VERSION_MINOR%]: "
if "%NEW_MINOR%"=="" set "NEW_MINOR=%VERSION_MINOR%"

set /p "NEW_BUILD=  Build [%VERSION_BUILD%]: "
if "%NEW_BUILD%"=="" set "NEW_BUILD=%VERSION_BUILD%"

echo.
echo  --------------------------------------
echo   Version change: %VERSION_MAJOR%.%VERSION_MINOR%.%VERSION_BUILD% -^> %NEW_MAJOR%.%NEW_MINOR%.%NEW_BUILD%
echo  --------------------------------------
echo.

if "%NEW_MAJOR%.%NEW_MINOR%.%NEW_BUILD%"=="%VERSION_MAJOR%.%VERSION_MINOR%.%VERSION_BUILD%" (
    echo   No changes. Exiting.
    echo.
    goto :eof
)

set /p "CONFIRM=  Apply this version? (Y/N): "
if /i not "%CONFIRM%"=="Y" (
    echo.
    echo   Cancelled. Version unchanged.
    echo.
    goto :eof
)

REM --- Update version values in this file using PowerShell ---
powershell -NoProfile -Command ^
    "$f = '%~f0';" ^
    "$c = Get-Content $f -Raw;" ^
    "$c = $c -replace 'SET VERSION_MAJOR=\d+', 'SET VERSION_MAJOR=%NEW_MAJOR%';" ^
    "$c = $c -replace 'SET VERSION_MINOR=\d+', 'SET VERSION_MINOR=%NEW_MINOR%';" ^
    "$c = $c -replace 'SET VERSION_BUILD=\d+', 'SET VERSION_BUILD=%NEW_BUILD%';" ^
    "[IO.File]::WriteAllText($f, $c)"

echo.
echo   Version updated to %NEW_MAJOR%.%NEW_MINOR%.%NEW_BUILD%
echo.
