@echo off
rem Doble clic: lo mas rapido para ver los cambios en localhost. Frena el WebCore anterior, compila incremental (sin clean) y lo levanta.
rem Si algo "no toma" el cambio, usar webcore-run.cmd (clean + build). Detalle en webcore-run.ps1.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0webcore-run.ps1" -Incremental %*
rem Solo llega aca si el servidor se detuvo o fallo la compilacion: se deja la ventana abierta para leer el motivo.
echo.
echo El script termino (codigo %ERRORLEVEL%).
pause
