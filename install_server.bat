@echo off
setlocal enabledelayedexpansion

:: Navigate to the Servers directory
cd /D "%~dp0\Servers"

:: Check if package.json exists
if not exist "package.json" (
    echo package.json not found in Servers directory.
    pause
    exit /b 1
)

:: Install Node.js dependencies
echo Installing Node.js dependencies...
npm install

if "%ERRORLEVEL%" NEQ "0" (
    echo Failed to install Node.js dependencies.
    pause
    exit /b 1
)

:: Check if winston is installed
npm list winston
if "%ERRORLEVEL%" NEQ "0" (
    echo winston is not installed.
    pause
    exit /b 1
)

echo Node.js dependencies installed successfully.
pause
exit /b 0
