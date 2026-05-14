@echo off
REM ============================================================
REM  KOAFiloServis - Setup EXE Uretici (cift tikla calistir)
REM  - Admin'e yukseltir
REM  - .NET SDK ve ISCC kontrolu yapar
REM  - setup\build.ps1 -Version <ARG> komutunu calistirir
REM ============================================================
setlocal enabledelayedexpansion

REM Bu .cmd dosyasinin bulundugu dizin
set "SCRIPT_DIR=%~dp0"
if "%SCRIPT_DIR:~-1%"=="\" set "SCRIPT_DIR=%SCRIPT_DIR:~0,-1%"

REM Versiyon parametresi
set "VERSION=%~1"
if "%VERSION%"=="" set "VERSION=1.0.0"

REM ---- Admin kontrolu ----
net session >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [INFO] Admin yetkisi gerekiyor, yukseltiliyor...
    powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -ArgumentList '%VERSION%' -Verb RunAs"
    exit /b 0
)

echo ============================================================
echo  KOAFiloServis Setup Builder
echo  Surum  : %VERSION%
echo  Klasor : %SCRIPT_DIR%
echo ============================================================
echo.

REM ---- dotnet kontrolu ----
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [HATA] .NET SDK bulunamadi. Lutfen .NET 10 SDK kurun:
    echo        https://dotnet.microsoft.com/download/dotnet/10.0
    pause
    exit /b 1
)

REM ---- ISCC kontrolu ----
set "ISCC="
for %%P in (
    "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
    "C:\Program Files\Inno Setup 6\ISCC.exe"
    "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
) do (
    if exist %%P set "ISCC=%%P"
)

if "%ISCC%"=="" (
    echo [HATA] Inno Setup 6 ^(ISCC.exe^) bulunamadi.
    echo        Kurmak icin: winget install JRSoftware.InnoSetup
    pause
    exit /b 1
)

echo [OK] dotnet ve ISCC bulundu.
echo.

REM ---- Build calistir ----
powershell -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT_DIR%\build.ps1" -Version %VERSION%
set "EXITCODE=%ERRORLEVEL%"

echo.
if %EXITCODE% EQU 0 (
    echo ============================================================
    echo  BASARILI! Cikti klasoru:
    echo    %SCRIPT_DIR%\output\v%VERSION%
    echo ============================================================
    REM Output klasorunu Explorer'da ac
    if exist "%SCRIPT_DIR%\output\v%VERSION%" start "" "%SCRIPT_DIR%\output\v%VERSION%"
) else (
    echo ============================================================
    echo  HATA! Cikis kodu: %EXITCODE%
    echo ============================================================
)

pause
exit /b %EXITCODE%
