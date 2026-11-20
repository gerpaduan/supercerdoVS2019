# Balanzas e impresoras

## Objetivo

Identificar integraciones con balanzas, impresoras y otros perifericos.

## Secciones

- Dispositivo
- Pantallas relacionadas
- Configuracion
- Eventos o comandos
- Riesgos de soporte

## Balanza: agente local (Carnisys.Balanza.Agent)

- **Dispositivo**: balanzas Systel y Kretz por puerto COM (drivers `SystelDriver` / `KretzDriver`). El navegador no toca el COM: lo lee el agente, una app de bandeja (.NET Framework 4.7.2) instalada en cada PC con balanza en `%LocalAppData%\CarniSys\BalanzaAgent` (se registra al inicio de Windows).
- **Config**: `%LocalAppData%\CarniSys\BalanzaAgent\config.json` (marca, modelo, puerto, baudios, `IntervaloLecturaMs`, por defecto 150; mantenerlo ≤ ~800 ms, ver DECISIONS 2026-11-20). Log diario: `...\BalanzaAgent\logs\agente-AAAAMMDD.log` (14 días).
- **Lectura**: un hilo (`BalanzaReaderService.ReadLoop`) pide el peso, espera `min(intervalo, 250)` ms, lee y guarda la última lectura con hora monótona y `secuencia`; duerme `max(100, intervalo)` ms (~300 ms por vuelta con 150).
- **HTTP** solo en `127.0.0.1:5100` (`LocalBalanzaServer`, cada conexión en el ThreadPool, timeouts de 2 s, `Cache-Control: no-store`, CORS `*`): `GET /status`, `GET /peso`, `GET|POST /config`, `GET /puertos`, `POST /probar`.
- **Contrato `/peso`**: `ok`, `conectada`, `peso`, `pesoTexto`, `pesoDisplay` (con ` i` si inestable), `unidad`, `estable`, `inestable`, `negativo`, `marca`, `modelo`, `puerto`, `raw`, `fechaHora` (local, `yyyy-MM-ddTHH:mm:ss`), **`edadMs`** (antigüedad de la lectura al servirla) y **`secuencia`** (desde 2026-11-20), `error`. Watchdog: lectura sin renovar por más de `max(3000, 2 vueltas + 1000)` ms → `ok=false, conectada=false, error="Lectura congelada…"`; a los 10 s cierra el puerto para destrabar al lector. `/status` repite `conectada`, `error`, `fechaHora`, `edadMs`, `secuencia` de la última lectura.
- **Pantallas relacionadas**: cliente compartido `WebCore/wwwroot/Scripts/carnisys.balanza.js` (poll de `/peso` cada 250 ms mientras está activo, `/status` cada 3 s; `lecturaVigente()` y `validarParaAgregar()` son la única fuente de peso) usado por `elaborados-carga.js`, `elaborados-rapido.js`, `movimientos.js`, `stock.js`, `pos-balanza.js` (Ventas/POS y PuntosExpendio/POS) y `home-dashboard.js`. Regla: `docs/DECISIONS.md` (2026-11-20, «Balanza: lectura vigente»). Tests: `node --test "WebCore.Tests/js/*.test.js"`.
- **Distribución**: `WebCore/wwwroot/Content/downloads/Carnisys.Balanza.Agent.zip` (exe, exe.config, `Instalar-Local.cmd`, `README-Instalacion.txt`), descargable desde Utilidades. Armado: compilar `Carnisys.Balanza.Agent.csproj` en Release con el MSBuild de Visual Studio y comprimir esos 4 archivos. Actualizar en una PC: `despliegue-y-publicacion.md` (sección «Agente de balanza»).
- **Riesgos de soporte**: adaptadores USB-serie que cuelgan llamadas al COM (lo cubre el watchdog; queda en el log); dos programas no pueden abrir el mismo COM (el WinForms viejo o «Probar lectura» con el agente corriendo); PCs con el agente viejo hasta reinstalar (la web igual detecta la lectura congelada por `fechaHora`, a ~2,5 s).
- **Impresoras**: PENDIENTE (agente `PrintAgent`, no relevado en esta revisión).
