@echo off
setlocal
set "TARGET=%LocalAppData%\CarniSys\BalanzaAgent"
if not exist "%TARGET%" mkdir "%TARGET%"
rem Si el agente esta corriendo su .exe queda bloqueado y xcopy no lo puede reemplazar: se cierra antes de copiar (actualizacion).
taskkill /IM Carnisys.Balanza.Agent.exe /F >nul 2>&1
xcopy "%~dp0*" "%TARGET%\" /E /Y /I >nul
start "" "%TARGET%\Carnisys.Balanza.Agent.exe" --configure
echo Agente copiado en:
echo %TARGET%
echo.
echo Se abrió el configurador inicial.
endlocal
