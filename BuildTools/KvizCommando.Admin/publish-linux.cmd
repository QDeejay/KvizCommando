@echo off
setlocal

set "OUTPUT=%~dp0..\..\publish-linux\admin"

dotnet publish "%~dp0KvizCommando.Admin.csproj" -c Release -r linux-x64 --self-contained false -o "%OUTPUT%"
if errorlevel 1 (
    echo.
    echo KvizCommando.Admin Linux publish FAILED.
    exit /b 1
)

echo.
echo KvizCommando.Admin Linux publish OK.
echo Output: %OUTPUT%
