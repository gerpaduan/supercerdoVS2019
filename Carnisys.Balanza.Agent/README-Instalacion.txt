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

Actualizar el agente en una PC que ya lo tiene
1. Descargar Carnisys.Balanza.Agent.zip desde Utilidades y descomprimirlo.
2. Ejecutar Instalar-Local.cmd: cierra el agente que esta corriendo, copia la version nueva y abre el configurador.
3. La configuracion (puerto, marca) se conserva. Cerrar el configurador sin cambios.
4. Verificar en el navegador: http://127.0.0.1:5100/peso debe mostrar "edadMs" (version 2026-11-20 o posterior).

Diagnostico: peso que no se actualiza
- Abrir http://127.0.0.1:5100/peso con la balanza conectada y mirar "edadMs": tiene que ser chico (menos de 1000).
- Si el error dice "Lectura congelada", el hilo que lee el puerto COM se trabo; el agente intenta destrabarlo solo a los 10 s.
- Log diario en %LocalAppData%\CarniSys\BalanzaAgent\logs\agente-AAAAMMDD.log (errores de lectura, reaperturas, congelamientos).

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
