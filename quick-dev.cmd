@echo off
setlocal EnableExtensions EnableDelayedExpansion

set "ROOT=%~dp0"
set "BE_DIR=%ROOT%DigiXanh.API"
set "FE_DIR=%ROOT%digixanh-fe"

if not exist "%BE_DIR%" (
  echo [ERROR] Khong tim thay thu muc BE: "%BE_DIR%"
  call :WAIT
  exit /b 1
)

if not exist "%FE_DIR%" (
  echo [ERROR] Khong tim thay thu muc FE: "%FE_DIR%"
  call :WAIT
  exit /b 1
)

:MENU
cls
echo ================================================
echo DigiXanh - Quick CMD (BE ^& FE)
echo Root: %ROOT%
echo ================================================
echo === Cau hinh Port (da thong nhat) ===
echo BE: http://localhost:5000  ^|  https://localhost:5001
echo FE: http://localhost:4200
echo ================================================
echo [1] Kiem tra version (dotnet/node/npm)
echo [2] BE - dotnet restore
echo [3] BE - dotnet ef database update
echo [4] BE - Them migration moi
echo [5] BE - dotnet run (HTTP - port 5000)
echo [6] BE - dotnet run --launch-profile https (KHUYEN DUNG)
echo [7] FE - npm install
echo [8] FE - npm start
echo [9] Chay ca BE ^& FE (2 cua so moi)
echo [T] Chay unit test backend (dotnet test DigiXanh.sln)
echo [A] Mo Swagger (https://localhost:5001/swagger)
echo [B] Dung cac dotnet run cua API
echo [Q] Thoat
echo ================================================
choice /c 123456789TABQ /n /m "Nhap lua chon: "

if errorlevel 13 goto END
if errorlevel 12 goto STOP_BE
if errorlevel 11 goto OPEN_SWAGGER
if errorlevel 10 goto TEST_BE
if errorlevel 9 goto RUN_BOTH
if errorlevel 8 goto RUN_FE
if errorlevel 7 goto NPM_INSTALL
if errorlevel 6 goto RUN_BE_HTTPS
if errorlevel 5 goto RUN_BE
if errorlevel 4 goto ADD_MIGRATION
if errorlevel 3 goto EF_UPDATE
if errorlevel 2 goto RESTORE_BE
if errorlevel 1 goto CHECK_VERSION
goto MENU

:CHECK_VERSION
echo.
echo ---- dotnet --info (rut gon) ----
dotnet --info | findstr /R /C:"Version:" /C:"OS Name" /C:"RID"
echo.
echo ---- node -v ----
node -v
echo ---- npm -v ----
npm -v
echo.
call :WAIT
goto MENU

:RESTORE_BE
echo.
echo [BE] dotnet restore
pushd "%BE_DIR%"
dotnet restore
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:EF_UPDATE
echo.
echo [BE] dotnet ef database update
pushd "%BE_DIR%"
dotnet ef database update
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:ADD_MIGRATION
echo.
set /p MIGRATION_NAME=Nhap ten migration (vd: AddAuthTables): 
if "%MIGRATION_NAME%"=="" (
  echo Ten migration khong duoc de trong.
  call :WAIT
  goto MENU
)
echo [BE] dotnet ef migrations add %MIGRATION_NAME%
pushd "%BE_DIR%"
dotnet ef migrations add %MIGRATION_NAME%
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:RUN_BE
echo.
echo [BE] dotnet run (HTTP - port 5000)
echo Nhan Ctrl+C de dung server.
pushd "%BE_DIR%"
dotnet run
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:RUN_BE_HTTPS
echo.
echo [BE] dotnet run --launch-profile https (KHUYEN DUNG)
echo URL: https://localhost:5001
echo Nhan Ctrl+C de dung server.
pushd "%BE_DIR%"
dotnet run --launch-profile https
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:NPM_INSTALL
echo.
echo [FE] npm install
pushd "%FE_DIR%"
npm install
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:RUN_FE
echo.
echo [FE] npm start
echo URL: http://localhost:4200
echo Proxy: https://localhost:5001
echo Nhan Ctrl+C de dung dev server.
pushd "%FE_DIR%"
npm start
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:RUN_BOTH
echo.
echo [BE+FE] Mo 2 cua so cmd moi...
echo BE chay o: https://localhost:5001
echo FE chay o: http://localhost:4200
echo.
start "DigiXanh BE" /d "%BE_DIR%" cmd /k "dotnet run --launch-profile https"
timeout /t 3 /nobreak >nul
start "DigiXanh FE" /d "%FE_DIR%" cmd /k "npm start"
echo Da mo 2 cua so.
call :WAIT
goto MENU

:TEST_BE
echo.
echo [TEST] dotnet test DigiXanh.sln
pushd "%ROOT%"
dotnet test DigiXanh.sln
set "CODE=%ERRORLEVEL%"
popd
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:OPEN_SWAGGER
echo.
echo Thu mo Swagger tai https://localhost:5001/swagger
start "" "https://localhost:5001/swagger"
call :WAIT
goto MENU

:STOP_BE
echo.
echo [BE] Dung cac tien trinh DigiXanh.API dang chay
powershell -NoProfile -Command "$procs = Get-CimInstance Win32_Process | Where-Object { $_.Name -ieq 'DigiXanh.API.exe' -or ($_.Name -ieq 'dotnet.exe' -and $_.CommandLine -like '*DigiXanh.API*') -or ($_.Name -ieq 'dotnet-watch.exe' -and $_.CommandLine -like '*DigiXanh.API*') }; if (-not $procs) { Write-Host 'Khong tim thay tien trinh API dang chay.'; exit 0 }; $count = 0; foreach ($p in $procs) { Stop-Process -Id $p.ProcessId -Force -ErrorAction SilentlyContinue; if (-not (Get-Process -Id $p.ProcessId -ErrorAction SilentlyContinue)) { $count++ } }; Write-Host ('Da dung ' + $count + ' tien trinh API.')"
set "CODE=%ERRORLEVEL%"
echo.
echo Exit code: %CODE%
call :WAIT
goto MENU

:END
echo Thoat.
call :WAIT
endlocal
exit /b 0

:WAIT
echo.
pause
exit /b 0
