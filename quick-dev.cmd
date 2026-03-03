@echo off
setlocal EnableExtensions EnableDelayedExpansion

:: DigiXanh - Quick Dev Launcher v3.0 (Loop Menu)
:: Usage: Double-click or run: quick-dev.cmd

set "ROOT=%~dp0"
set "BE_DIR=%ROOT%DigiXanh.API"
set "FE_DIR=%ROOT%digixanh-fe"
set "BE_RUN_SCRIPT=%ROOT%scripts\run-be-with-env.ps1"
set "BE_HEALTH_URL=https://localhost:5001/api/health"
set "MAX_WAIT_SECONDS=90"

call :CHECK_DIRS
if errorlevel 1 (
  pause
  exit /b 1
)

:MENU
echo.
echo ================================================
echo    DigiXanh - Quick Dev Launcher (Loop)
echo ================================================
echo.
echo [1] Start/Restart BE + FE
echo [2] Stop BE + FE
echo [3] Mo URL nhanh
echo [4] Thoat
echo.
choice /C 1234 /N /M "Chon [1-4]: "

if errorlevel 4 goto END
if errorlevel 3 goto OPEN_URLS
if errorlevel 2 goto STOP_ALL
if errorlevel 1 goto START_ALL
goto MENU

:START_ALL
echo.
echo [1/4] Dang dung cac tien trinh cu...
call :KILL_PROCESSES
timeout /t 2 /nobreak >nul
echo     OK - Da don dep
echo.

echo [2/3] Khoi dong Backend...
start "DigiXanh BE" /d "%ROOT%" cmd /k "powershell -ExecutionPolicy Bypass -File ""%BE_RUN_SCRIPT%"""

echo [3/3] Khoi dong Frontend...
start "DigiXanh FE" /d "%FE_DIR%" cmd /k "npm start"
timeout /t 1 /nobreak >nul

echo.
echo ================================================
echo    DANG KHOI DONG HE THONG (SONG SONG)
echo ================================================
echo    BE: https://localhost:5001
echo    FE: http://localhost:4200
echo.
echo    Luu y: Ban co the can doi vai giay de BE/FE 
echo    san sang hoan toan.
echo.
goto MENU

:STOP_ALL
echo.
echo Dang dung BE + FE...
call :KILL_PROCESSES
timeout /t 1 /nobreak >nul
echo OK - Da dung tien trinh.
goto MENU

:OPEN_URLS
echo.
echo Mo nhanh URL...
start "" "https://localhost:5001/swagger"
start "" "http://localhost:4200"
goto MENU

:KILL_PROCESSES
taskkill /F /IM "DigiXanh.API.exe" >nul 2>&1
taskkill /F /IM "dotnet.exe" >nul 2>&1
taskkill /F /IM "node.exe" >nul 2>&1
exit /b 0

:CHECK_DIRS
if not exist "%BE_DIR%" (
  echo [ERROR] Khong tim thay: %BE_DIR%
  exit /b 1
)
if not exist "%FE_DIR%" (
  echo [ERROR] Khong tim thay: %FE_DIR%
  exit /b 1
)
if not exist "%BE_RUN_SCRIPT%" (
  echo [ERROR] Khong tim thay script: %BE_RUN_SCRIPT%
  exit /b 1
)
exit /b 0

:END
echo.
echo Tam biet!
exit /b 0
