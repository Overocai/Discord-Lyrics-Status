@echo off
title Discord Lyrics - Setup
cd /d "%~dp0"

echo ================================================
echo   Discord Lyrics - setup
echo ================================================
echo.

REM ---- 1) Find an existing Python -------------------------------------------
call :find_python
if defined PYCMD goto :have_python

echo Python was not found on this PC.
echo Trying to install it automatically...
echo.

where winget >nul 2>nul
if errorlevel 1 goto :install_manual

echo Installing Python 3.12 via winget (this can take a minute)...
winget install -e --id Python.Python.3.12 --silent --accept-package-agreements --accept-source-agreements
goto :after_install

:install_manual
echo winget is not available - downloading the official Python installer...
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; $u='https://www.python.org/ftp/python/3.12.7/python-3.12.7-amd64.exe'; $o=Join-Path $env:TEMP 'python-setup.exe'; Write-Host 'Downloading Python...'; Invoke-WebRequest -Uri $u -OutFile $o; Write-Host 'Running the installer (silent)...'; Start-Process -FilePath $o -ArgumentList '/quiet','InstallAllUsers=0','PrependPath=1','Include_test=0' -Wait"

:after_install
echo.
echo Python installation finished. Re-checking...
call :find_python
if defined PYCMD goto :have_python

REM PATH in THIS window is stale - try the default per-user install location
for /f "delims=" %%P in ('dir /b /s "%LocalAppData%\Programs\Python\python.exe" 2^>nul') do set "PYCMD=%%P"
if defined PYCMD goto :have_python

echo.
echo Python was installed, but this window can't see it yet because Windows
echo only updates PATH for NEW windows.
echo   ^>^>  Please CLOSE this window and double-click install.bat again.
echo.
pause
exit /b 1

REM ---- 2) Install the Python dependencies -----------------------------------
:have_python
echo Using Python: %PYCMD%
echo.
"%PYCMD%" -m pip install --upgrade pip
"%PYCMD%" -m pip install -r requirements.txt
set RESULT=%errorlevel%

echo.
if "%RESULT%"=="0" (
    echo Done!  Now run the app with:   "%PYCMD%" main.py
) else (
    echo Something went wrong installing the dependencies. See the messages above.
)
echo.
pause
exit /b %RESULT%

REM ---- helper: sets PYCMD to "py" or "python" if found ----------------------
:find_python
set "PYCMD="
py --version >nul 2>nul && set "PYCMD=py"
if not defined PYCMD ( python --version >nul 2>nul && set "PYCMD=python" )
goto :eof
