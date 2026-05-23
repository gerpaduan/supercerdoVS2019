@echo off
setlocal
set "TARGET=%LocalAppData%\CarniSys\BalanzaAgent"
if not exist "%TARGET%" mkdir "%TARGET%"
xcopy "%~dp0*" "%TARGET%\" /E /Y /I >nul
start "" "%TARGET%\Carnisys.Balanza.Agent.exe" --configure
echo Agente copiado en:
echo %TARGET%
echo.
echo Se abrió el configurador inicial.
endlocal
