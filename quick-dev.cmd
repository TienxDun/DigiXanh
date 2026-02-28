@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: DigiXanh - Quick Dev Launcher v2.0
:: Usage: Double-click or run: quick-dev.cmd

set "ROOT=%~dp0"
set "BE_DIR=%ROOT%DigiXanh.API"
set "FE_DIR=%ROOT%digixanh-fe"
set "BE_HEALTH_URL=https://localhost:5001/api/health"
set "MAX_WAIT_SECONDS=90"

echo ================================================
echo    DigiXanh - Quick Dev Launcher
echo ================================================
echo.

:: Check directories exist
if not exist "%BE_DIR%" (
  echo [ERROR] Khong tim thay: %BE_DIR%
  pause & exit /b 1
)
if not exist "%FE_DIR%" (
  echo [ERROR] Khong tim thay: %FE_DIR%
  pause & exit /b 1
)

:: Step 1: Kill existing processes
echo [1/4] Dang dung cac tien trinh cu...
taskkill /F /IM "DigiXanh.API.exe" >nul 2>&1
taskkill /F /IM "dotnet.exe" >nul 2>&1  
taskkill /F /IM "node.exe" >nul 2>&1

:: Wait for ports to be released
timeout /t 3 /nobreak >nul
echo     OK - Da don dep
echo.

:: Step 2: Start Backend
echo [2/4] Khoi dong Backend...
start "DigiXanh BE" /d "%BE_DIR%" cmd /k "dotnet run --launch-profile https"

:: Step 3: Wait for BE to be healthy
echo [3/4] Doi BE san sang (toi da %MAX_WAIT_SECONDS%s)...
echo          (Nhan Ctrl+C de bo qua va khoi dong FE ngay)
echo.

set /a "elapsed=0"
set "be_ready=0"

:CHECK_LOOP
if !elapsed! geq %MAX_WAIT_SECONDS% goto BE_TIMEOUT

:: Check if BE is ready using curl (with cert ignore)
curl.exe -k -s -o nul -w "%%{http_code}" "%BE_HEALTH_URL%" > "%TEMP%\be_status.txt" 2>nul
set /p STATUS=<"%TEMP%\be_status.txt"

if "!STATUS!"=="200" (
  set "be_ready=1"
  goto BE_READY
)

:: Not ready yet, show progress and wait
set /a "elapsed+=2"
<nul set /p="."
timeout /t 2 /nobreak >nul
goto CHECK_LOOP

:BE_TIMEOUT
echo.
echo.
echo [WARN] BE chua san sang sau %MAX_WAIT_SECONDS%s
echo         Cua so BE van dang mo de ban kiem tra.
echo.
echo [?] Ban muon:
echo     [C] - Tiep tuc khoi dong FE (khuyen nghi)
echo     [D] - Dung lai va xem loi BE
echo.
choice /C CD /M "Lua chon"
if %ERRORLEVEL%==2 exit /b 1

:BE_READY
echo.
echo     OK - BE san sang tai https://localhost:5001
echo.

:: Step 4: Start Frontend
echo [4/4] Khoi dong Frontend...
start "DigiXanh FE" /d "%FE_DIR%" cmd /k "npm start"
timeout /t 2 /nobreak >nul

echo.
echo ================================================
echo    🚀 MO TRUONG DEV THANH CONG!
echo ================================================
echo.
echo    BE: https://localhost:5001
echo    FE: http://localhost:4200
echo.
echo    De dung: dong cua so cmd "DigiXanh BE" va "DigiXanh FE"
echo.
pause
exit /b 0
