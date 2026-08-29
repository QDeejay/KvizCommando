@echo off
setlocal

dotnet publish "%~dp0DatabaseMigrationTool.csproj" -c Release
if errorlevel 1 (
    echo.
    echo DatabaseMigrationTool publish FAILED.
    exit /b 1
)

echo.
echo Published: %~dp0..\..\DatabaseMigrationTool.exe
