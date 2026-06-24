@echo off
title Discord Lyrics - Instalar dependencias / Install requirements
cd /d "%~dp0"

echo ================================================
echo   Discord Lyrics - installing dependencies
echo   (pip install -r requirements.txt)
echo ================================================
echo.

py --version >nul 2>nul
if %errorlevel%==0 (
    py -m pip install --upgrade pip
    py -m pip install -r requirements.txt
) else (
    python -m pip install --upgrade pip
    python -m pip install -r requirements.txt
)

set RESULT=%errorlevel%
echo.
if "%RESULT%"=="0" (
    echo Done!  Now run the app with:   python main.py
) else (
    echo Something went wrong. Install Python 3.9+ from https://python.org
    echo and make sure "Add Python to PATH" is checked during setup.
)
echo.
pause
