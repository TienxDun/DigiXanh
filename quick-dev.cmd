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
echo [1] Kiem tra version (dotnet/node/npm)
echo [2] BE - dotnet restore
echo [3] BE - dotnet ef database update
echo [4] BE - Them migration moi
echo [5] BE - dotnet run
echo [6] FE - npm install
echo [7] FE - npm start
echo [8] Chay ca BE ^& FE (2 cua so moi)
echo [9] Chay unit test backend (dotnet test DigiXanh.sln)
echo [A] Mo Swagger (http://localhost:5000/swagger)
echo [Q] Thoat
echo ================================================
choice /c 123456789AQ /n /m "Nhap lua chon: "

if errorlevel 11 goto END
if errorlevel 10 goto OPEN_SWAGGER
if errorlevel 9 goto TEST_BE
if errorlevel 8 goto RUN_BOTH
if errorlevel 7 goto RUN_FE
if errorlevel 6 goto NPM_INSTALL
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
echo [BE] dotnet run
echo Nhan Ctrl+C de dung server.
pushd "%BE_DIR%"
dotnet run
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
start "DigiXanh BE" cmd /k "cd /d ""%BE_DIR%"" && dotnet run"
start "DigiXanh FE" cmd /k "cd /d ""%FE_DIR%"" && npm start"
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
echo Thu mo Swagger tai http://localhost:5000/swagger
start "" "http://localhost:5000/swagger"
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
