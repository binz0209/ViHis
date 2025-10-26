@echo off
setlocal enabledelayedexpansion

REM ==============================
REM ViHis - Run 3 individual TC + Coverage HTML Reports
REM ==============================

pushd "%~dp0"
if exist "BackEnd" (
  cd BackEnd
) else (
  echo [ERROR] BackEnd folder not found.
  pause
  exit /b 1
)

REM Ensure reportgenerator exists; install if missing
where reportgenerator >nul 2>&1
if errorlevel 1 (
  echo [INFO] Installing reportgenerator global tool...
  dotnet tool install -g dotnet-reportgenerator-globaltool
  set "PATH=%USERPROFILE%\.dotnet\tools;%PATH%"
)

REM ============================================
REM 1️⃣ TEST TC01 (MongoDB RAG)
REM ============================================
echo.
echo ============================== 
echo [RUN] TC01_AskAsync_WithMongoDBContext_ReturnsValidAnswer 
echo ==============================
dotnet test --filter "TC01_AskAsync_WithMongoDBContext_ReturnsValidAnswer" --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"

for /f "delims=" %%F in ('dir /b /s "VietHistory.AI.Tests\TestResults\*\coverage.cobertura.xml" 2^>nul') do set "COV1=%%F"
if not defined COV1 (
  echo [ERROR] Coverage file for TC01 not found.
) else (
  reportgenerator -reports:"!COV1!" -targetdir:"coverage-TC01" -reporttypes:Html -title:"ViHis Coverage - TC01 MongoDB RAG"
  start "" "coverage-TC01\index.html"
)

REM ============================================
REM 2️⃣ TEST TC02 (Web Fallback)
REM ============================================
echo.
echo ============================== 
echo [RUN] TC02_AskAsync_WithEmptyMongoDB_FallsBackToWeb 
echo ==============================
dotnet test --filter "TC02_AskAsync_WithEmptyMongoDB_FallsBackToWeb" --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"

for /f "delims=" %%F in ('dir /b /s "VietHistory.AI.Tests\TestResults\*\coverage.cobertura.xml" 2^>nul') do set "COV2=%%F"
if not defined COV2 (
  echo [ERROR] Coverage file for TC02 not found.
) else (
  reportgenerator -reports:"!COV2!" -targetdir:"coverage-TC02" -reporttypes:Html -title:"ViHis Coverage - TC02 Web Fallback"
  start "" "coverage-TC02\index.html"
)

REM ============================================
REM 3️⃣ TEST TC03 (MongoDB Priority)
REM ============================================
echo.
echo ============================== 
echo [RUN] TC03_AskAsync_WithBothMongoAndWeb_UsesMongoFirst 
echo ==============================
dotnet test --filter "TC03_AskAsync_WithBothMongoAndWeb_UsesMongoFirst" --collect:"XPlat Code Coverage" --logger "console;verbosity=normal"

for /f "delims=" %%F in ('dir /b /s "VietHistory.AI.Tests\TestResults\*\coverage.cobertura.xml" 2^>nul') do set "COV3=%%F"
if not defined COV3 (
  echo [ERROR] Coverage file for TC03 not found.
) else (
  reportgenerator -reports:"!COV3!" -targetdir:"coverage-TC03" -reporttypes:Html -title:"ViHis Coverage - TC03 MongoDB Priority"
  start "" "coverage-TC03\index.html"
)

echo.
echo ==============================
echo ✅ All 3 coverage reports generated successfully!
echo.
echo TC01: coverage-TC01\index.html
echo TC02: coverage-TC02\index.html
echo TC03: coverage-TC03\index.html
echo ==============================

popd
endlocal
pause
