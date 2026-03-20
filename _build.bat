@echo off
setlocal

set PROJECT=src\WinOptimizer\WinOptimizer.csproj
set CONFIG=Release
set RUNTIME=win-x64
set PUBLISH_DIR=src\WinOptimizer\bin\%CONFIG%\net8.0-windows\%RUNTIME%\publish

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
