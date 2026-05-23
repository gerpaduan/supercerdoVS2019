Carnisys Balanza Agent

Objetivo
- Leer la balanza local desde la PC donde está conectada.
- Exponer una API HTTP solo en 127.0.0.1.
- Permitir que la web consuma el peso sin tocar puertos COM en el servidor ASP.NET.

Instalación local
1. Compilar el proyecto Carnisys.Balanza.Agent en Release.
2. Copiar los archivos generados a una carpeta local de la terminal.
3. Ejecutar Carnisys.Balanza.Agent.exe.
4. En el icono del área de notificación abrir "Configurar balanza".
5. Elegir Marca, Modelo y Puerto COM.
6. Presionar "Probar lectura".
7. Guardar.
8. El agente se registra para iniciar automáticamente con el usuario actual de Windows.

Endpoints locales
- GET  http://127.0.0.1:5100/status
- GET  http://127.0.0.1:5100/peso
- GET  http://127.0.0.1:5100/config
- POST http://127.0.0.1:5100/config
- GET  http://127.0.0.1:5100/puertos
- POST http://127.0.0.1:5100/probar

Notas
- El agente escucha únicamente en 127.0.0.1.
- La configuración se guarda en %LocalAppData%\CarniSys\BalanzaAgent\config.json.
- Si la balanza no responde o el puerto está ocupado, la API sigue viva y devuelve error claro.
