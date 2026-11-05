@echo off
rem Doble clic: recompila WebCore en limpio (clean + build) y lo lanza en http://localhost:5270. Frena antes el WebCore anterior de este checkout.
rem Ctrl+C o cerrar la ventana detiene el servidor. Logica en Scripts\webcore-run.ps1.
call "%~dp0Scripts\webcore-run.cmd" %*
