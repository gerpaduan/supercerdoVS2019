@echo off
rem Doble clic: solo compila WebCore (Debug, sin levantar el servidor). Muestra errores y resumen.
dotnet build "%~dp0WebCore\WebCore.csproj" --nologo -v minimal -clp:"ErrorsOnly;Summary"
echo.
echo La compilacion termino (codigo %ERRORLEVEL%).
pause
