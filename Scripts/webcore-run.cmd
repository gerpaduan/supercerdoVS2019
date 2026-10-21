@echo off
rem Doble clic: recompila WebCore en limpio y lo levanta en localhost. La logica esta en webcore-run.ps1.
rem Los parametros se pasan tal cual, ej.: webcore-run.cmd -Incremental -NoBrowser
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0webcore-run.ps1" %*
echo.
echo El script termino (codigo %ERRORLEVEL%).
pause
