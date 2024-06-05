@echo off
setlocal enabledelayedexpansion

:: Navigate to the Servers directory
cd /D "%~dp0\Servers"

:: Check if server.mjs exists
if not exist "server.mjs" (
    echo server.mjs not found in Servers directory.
    pause
    exit /b 1
)

:: Start the Node.js server
echo Starting Node.js server...
node server.mjs

if "%ERRORLEVEL%" NEQ "0" (
    echo Failed to start Node.js server.
    pause
    exit /b 1
)

echo Node.js server started successfully.
pause
exit /b 0
