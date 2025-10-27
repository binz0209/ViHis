@echo off
echo ============================================
echo   VietHistory - Starting Development Mode
echo ============================================
echo.

REM Check if Node.js is installed
where node >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Node.js is not installed or not in PATH
    echo Please install Node.js from https://nodejs.org/
    pause
    exit /b 1
)

REM Check if .NET is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET SDK is not installed or not in PATH
    echo Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo Starting Backend API...
start "VietHistory Backend" cmd /k "cd BackEnd && dotnet run --project VietHistory.Api"

echo.
echo Waiting for backend to start...
timeout /t 5 >nul

echo.
echo Starting Frontend...
start "VietHistory Frontend" cmd /k "cd FrontEnd && npm install && npm run dev"

echo.
echo ============================================
echo   Frontend: http://localhost:3000
echo   Backend API: http://localhost:5000
echo ============================================
echo.
echo Press any key to exit...
pause >nul


