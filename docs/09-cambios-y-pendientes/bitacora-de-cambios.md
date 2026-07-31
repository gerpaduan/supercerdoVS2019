# Bitacora de cambios

## Objetivo

Registrar cambios funcionales o tecnicos realizados sobre el sistema.

## Secciones

- Fecha
- Cambio
- Archivos o modulos afectados
- Motivo
- Resultado

## 2026-07-29

- Cambio: primer deploy de produccion hecho a traves de Claude, contra la VM Windows (`carnisys.com`), commit `79f7d53f` de `codex_ia`. Incluye el fix de atajos de teclado de `CtaCtePersona` (ver `docs/07-operacion-y-soporte/incidencias-frecuentes.md`) y la limpieza de `AGENTS.md`/`PROMPT_CARNISYS.md`/`docs/10-carnisys-ng/`.
- Archivos o modulos afectados: sistema web legacy en `C:\inetpub\wwwroot\CarniSysWeb` de la VM.
- Motivo: pedido explicito de deploy a produccion.
- Resultado: publish Release precompilado, subido por SFTP y aplicado con el App Pool `CarniSys` detenido brevemente. Se preservaron intactos `Config\connectionStrings.config`, `Config\appSettings.secrets.config`, `AFIP\` y `App_Data\` de la VM (no se tocan en un deploy: son estado vivo, no codigo). Backup completo del `CarniSysWeb` previo en `C:\inetpub\wwwroot\web\backups\CarniSysWeb_20260729_210501` (rollback: restaurar esa carpeta con robocopy /MIR y reiniciar el App Pool). Verificado con `200 OK` en `/Login/Index` sin excepciones tras el deploy. Ver riesgo documentado en `riesgos-conocidos.md` sobre `requireSSL`.

## 2026-07-29 (segundo deploy, mismo dia) - Fix de POS/Movimientos/pagos rotos en produccion

- Cambio: se agregaron a `Web.csproj` 6 archivos `<Content Include>` de `Scripts\app` que existian en disco y se usaban desde vistas reales pero nunca se habian agregado al proyecto: `modal-request-loading.js`, `modal-postmovimiento.js`, `movimientos.js`, `pago-cheques.js`, `pago-ux.js`, `swal-single-confirm.js`. Se agrego ademas un header explicito de antiforgery en `Web/Scripts/app/forma-pago.js` (refuerzo, no la causa raiz).
- Archivos o modulos afectados: `Web.csproj`, `Web/Scripts/app/forma-pago.js`.
- Motivo: el usuario reporto que el POS no permitia finalizar ninguna venta al elegir forma de pago. Investigando se encontro que `modal-request-loading.js` (el script que auto-inyecta el token CSRF en los POST JSON) daba `404` en produccion, aunque funcionaba perfecto en local. Causa: el publish de produccion solo empaqueta lo que `Web.csproj` lista como `Content`; en local, IIS Express sirve cualquier archivo del disco sin mirar el `.csproj`, por eso el problema era invisible en desarrollo. Se verifico con Chrome real (via CDP) contra `carnisys.com` con el codigo original, confirmando `400` en `/Ventas/FinalizarVenta` siempre. Se audito el resto del proyecto (130 vistas, 7 css) y no hay mas archivos huerfanos.
- Resultado: republish + mismo procedimiento de deploy documentado en `despliegue-y-publicacion.md`. Backup previo en `C:\inetpub\wwwroot\web\backups\CarniSysWeb_20260729_220100`. Verificado con Chrome real contra produccion: `window.ModalRequestLoading` ya existe, y `FinalizarVenta` responde `200` (antes `400` en el 100% de los intentos desde el 2026-07-28 21:23). De paso quedaron arreglados Movimientos, pagos/cheques y las confirmaciones de SweetAlert en toda la app, que dependian de los mismos archivos huerfanos.

## 2026-07-30 - Lectura de codigo de barra por camara, cross-browser

- Cambio: `Permissions-Policy` ya no bloquea la camara (`camera=()` -> `camera=(self)`), y se agrego ZXing (`@zxing/library@0.23.0`, vendorizado) como motor de decodificacion de respaldo para navegadores sin `BarcodeDetector` (Firefox, Safari/iOS).
- Archivos o modulos afectados: `Web/Helpers/SecurityRuntime.cs`, `Web/Scripts/app/scanner.js`, `Web/Content/vendor/zxing/zxing.min.js` (nuevo), `Web/Web.csproj`, `Web/Views/Shared/_LayoutBase.cshtml`, `Web/Views/Shared/_LayoutPOS.cshtml`.
- Motivo: el usuario reporto que la lectura de codigo de barra por camara dejo de funcionar hace varios dias, en las 5 pantallas donde se usa (POS de Ventas, POS de PuntosExpendio, y 3 escaners en Productos). Ver detalle de la causa e investigacion en `docs/07-operacion-y-soporte/incidencias-frecuentes.md`.
- Resultado: compila y precompila sin errores (Release). Verificado con Chrome real: round-trip de ZXing con una imagen EAN-13 real decodifica correctamente, e integracion completa de `scanner.js` con el motor nativo deshabilitado a proposito (simulando Firefox/Safari) procesa frames y detecta codigos sin excepciones. Pendiente: probar en un dispositivo real con Firefox y con Safari/iOS, y con un lector fisico de codigo de barra apuntando a productos reales — no se pudo automatizar esa parte.

## 2026-07-30 - POS: mejoras de UI/UX en modo responsive y desktop (dos rondas)

- Cambio: primera ronda (7 puntos, solo responsive) — el panel inferior arranca scrolleado hacia el boton Finalizar al iniciar/tras una venta; total e importe con tipografia mas chica; input cantidad mas chico (alto y fuente, igualado al de codigo); descripcion del producto mas oscura en modo claro; bloque "Conexion balanza" oculto en responsive; los 3 botones del pie (ayuda/comentario/cancelar) a la misma altura; inputs del modal de forma de pago ensanchados a 12 caracteres. Segunda ronda (8 puntos, mezcla de responsive y general): se oculta la flechita "volver arriba" en responsive (no tiene sentido en el flujo del POS); el badge "1" de comentario se apila debajo del boton en vez de salirse del borde; la calculadora de billetes ya no abre el modal de impresion si el resultado es $0 (ver `incidencias-frecuentes.md`); estilo nuevo del encabezado del modal "Mis actividades" (antes generico/plano); el aviso de salida del POS (`beforeunload`) ahora tambien dispara si hay algun modal abierto, no solo con venta en curso; fix de texto invisible en hover del boton "Duplicar POS" en modo claro (ver `incidencias-frecuentes.md` y el riesgo relacionado en `riesgos-conocidos.md`); el modal de factura electronica ya no se puede cerrar por click afuera / Esc, y la X del header pasa por la misma logica que "Cancelar"; estilo nuevo de la fila de filtros del modal "Mis ventas" (buscar cliente + forma de pago + volver).
- Archivos o modulos afectados: `Web/Views/Ventas/POS.cshtml`, `Web/Views/Shared/_LayoutPOS.cshtml`, `Web/Views/Ventas/_FormaPagoModal.cshtml`, `Web/Views/Ventas/_ModalComentarioVenta.cshtml` (solo lectura, sin cambios), `Web/Scripts/app/calculadora-billetes.js`, `Web/Scripts/app/pos-cart.js`, `Web/Scripts/app/factura-electronica.js`, `Web/Views/Ventas/_FacturaElectronica.cshtml`, `Web/Views/Ventas/_MisVentas.cshtml`, `Web/Views/Cajas/_MisEgresosCaja.cshtml` (solo lectura), mas el bump de cache-busting (`?v=`) de los 3 `.js` tocados en `_LayoutBase.cshtml`, `_LayoutPOS.cshtml`, `DetalleFactura.cshtml`, `DetalleVenta.cshtml`, `Ventas/POS.cshtml` y `PuntosExpendio/POS.cshtml`.
- Motivo: pedido explicito del usuario, en dos tandas, de pulir la experiencia del POS (principalmente mobile/responsive) sin tocar la vista de escritorio ni romper funcionalidad existente.
- Resultado: probado en Chrome real (headless via CDP) logueado con el usuario de pruebas de `~/hosts/carnisys-web-local.env`, con cache de navegador deshabilitada (se detecto que el navegador tenia cacheado el JS viejo por el `?v=` sin bump — ver leccion en `incidencias-frecuentes.md`). Confirmado en vivo: flechita oculta en `<=991.98px`, badge de comentario apilado, calculadora cierra sin post-modal con $0 y sigue abriendo el post-modal con monto real (sin regresion), header de "Mis actividades" con el color nuevo, condicion de aviso de salida detecta modal abierto, `modalFacturaElectronica` con `data-backdrop=static data-keyboard=false`, filtros de "Mis ventas" en una sola fila en desktop y apilados a 100% de ancho en mobile. No se pudo probar en vivo, por alcance/tiempo: el hover del boton "Duplicar POS" (se verifico por calculo de especificidad CSS, no por simulacion real de `:hover`) y el flujo completo de facturacion electronica de punta a punta (requiere completar una venta real). Ninguno de los dos cambios de este lote se commiteo ni se subio a `codex_ia` todavia — pendiente de confirmacion del usuario para el siguiente `git push`.

## 2026-07-10

- Cambio: se unifico la base NG de `Stock` en una sola pantalla `Edit` por `tipoCompra`, alineada a la estructura del MVC.
- Archivos o modulos afectados: `src/CarniSys.NG.Web`.
- Motivo: el modulo legacy usa una sola vista compartida para `Ingreso`, `Egreso`, `Cierre`, `Ajuste` y `Pesaje`, con bloques condicionales por accion; mantener pantallas separadas en NG iba contra ese flujo probado.
- Resultado: NG ahora entra a una unica vista base con combo de accion y secciones visuales que cambian segun `tipoCompra`; todavia faltan la grilla operativa real, modales y persistencia.

- Cambio: se agrego el `index` operativo NG de `Stock` con accesos separados a `Ingreso`, `Egreso`, `Cierre` y `Ajuste`.
- Archivos o modulos afectados: `src/CarniSys.NG.Web`.
- Motivo: alinear la navegacion del modulo con el legacy antes de implementar escrituras reales.
- Resultado: NG ya muestra las entradas operativas del modulo protegidas por `formAddOrEditStock`; cada una abre una pantalla placeholder controlada, sin guardar datos ni alterar stock real.

- Cambio: se agrego base NG de `punto stock por sucursal` en la edicion de productos, manteniendo `puntoStock` general como respaldo compatible.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar el vertical de `Stock` con el menor impacto posible sobre el comportamiento legacy ya estable, antes de abrir edicion masiva o flujos operativos mas sensibles.
- Resultado: NG ya puede persistir puntos de stock por sucursal para productos de empresa; la matriz de stock recalcula `BAJO`, `SIN STOCK`, `NEGATIVO` y diferencias usando esos valores cuando existen, y si todavia no hay configuracion especifica mantiene el valor general actual como fallback seguro.

- Cambio: se agrego base NG de `punto stock por sucursal` en la edicion de productos, manteniendo `puntoStock` general como respaldo compatible.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar el vertical de `Stock` con el menor impacto posible sobre el comportamiento legacy ya estable, antes de abrir edicion masiva o flujos operativos mas sensibles.
- Resultado: NG ya puede persistir puntos de stock por sucursal para productos de empresa; la matriz de stock recalcula `BAJO`, `SIN STOCK`, `NEGATIVO` y diferencias usando esos valores cuando existen, y si todavia no hay configuracion especifica mantiene el valor general actual como fallback seguro.

- Cambio: se agrego `Stock` NG en modo consulta con matriz de existencia por sucursales y detalle por producto.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar con el proximo vertical funcional usando el stored procedure legacy ya existente, sin abrir todavia ingresos, egresos, cierres ni ajustes.
- Resultado: NG ya permite consultar stock actual por producto y sucursal con filtro por texto, tipo, estado y fecha; mantiene fuera de alcance cualquier escritura operativa del modulo.

## 2026-07-10

- Cambio: se agrego la base de `Etapa 1` para `CarniSys NG` con modelos y contratos minimos de autenticacion, empresa, sucursal y permisos.
- Archivos o modulos afectados: `src/CarniSys.NG.Domain`, `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar despues del scaffold inicial con un avance chico, compilable y alineado al plan tecnico.
- Resultado: `src/CarniSys.NG.Web` compila con servicios placeholder y reglas base reutilizables, sin tocar el comportamiento del sistema legacy.

## 2026-07-10

- Cambio: se agrego autenticacion por cookie, pantalla de login NG, claims de empresa/sucursal/permisos y proteccion de la home autenticada.
- Archivos o modulos afectados: `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: avanzar la `Etapa 1` sin introducir todavia dependencias estructurales al sistema legacy.
- Resultado: `src/CarniSys.NG.Web` compila con flujo de ingreso y sesion autenticada listos para reemplazar el backend placeholder por autenticacion real.

## 2026-07-10

- Cambio: se reemplazo el login placeholder por un adaptador SQL propio en NG contra `Usuarios`, `Empresas`, `Sucursal` y `PermisosUsuarios`, sin referenciar proyectos legacy.
- Archivos o modulos afectados: `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: avanzar a autenticacion real manteniendo la arquitectura NG desacoplada del legacy.
- Resultado: el proyecto compila con autenticacion real configurable y manejo controlado de errores de conexion; la validacion contra la base legacy quedo limitada por accesibilidad del SQL configurado en `Web.config`.

## 2026-07-10

- Cambio: se agrego autorizacion reutilizable por permiso legacy y pantallas NG de contexto para validar lectura y edicion sin tocar modulos productivos.
- Archivos o modulos afectados: `src/CarniSys.NG.Web`.
- Motivo: dejar una base de permisos reutilizable antes de bajar a un modulo real.
- Resultado: `Home` ahora muestra empresa, sucursal y resumen de permisos; `PermissionsController` valida permisos reales con filtros NG y el proyecto compila sin warnings.

## 2026-07-10

- Cambio: se agrego el primer modulo vertical NG en solo lectura para `Productos`, consumiendo `Corte` con filtro por empresa y permiso real de consulta.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: validar de punta a punta autenticacion, empresa y permisos sobre una pantalla real sin riesgo de escritura.
- Resultado: `ProductsController` y su vista ya funcionan como base segura para avanzar luego a detalle o edicion, con compilacion correcta.

## 2026-07-10

- Cambio: se agrego detalle de producto NG en solo lectura, enlazado desde el listado y protegido por el mismo permiso legacy de consulta.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: consolidar el primer modulo vertical antes de habilitar cualquier escritura.
- Resultado: `Productos` ya cubre listado y ficha de detalle sin tocar el flujo de edicion legacy, con compilacion correcta.

## 2026-07-10

- Cambio: se habilito una edicion minima y controlada de producto NG sobre campos directos del registro `Corte` de la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: abrir la primera escritura en NG con alcance chico, reversible y sin invocar la logica completa del stored procedure legacy.
- Resultado: NG ya puede editar `precioKg`, `pesable`, `promedio`, `puntoStock`, `enCierreStock`, `habilitado` e `ingresoRapidoEmbutido`, manteniendo fuera de alcance codigo, tipo, marca, corte maestro, IVA y catalogo global.

## 2026-07-10

- Cambio: se agrego `Catalogo Global` NG en solo lectura, con listado y detalle de productos `idEmpresa = 0`.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el primer vertical de productos sin habilitar todavia importacion ni escritura sobre registros globales.
- Resultado: NG ya expone productos propios y catalogo global como referencias separadas, preservando la regla de que los globales no se editan ni operan directamente.

## 2026-07-10

- Cambio: se agrego `Personas` NG en solo lectura, con listado y ficha de detalle para registros propios y globales.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: abrir un segundo vertical funcional sin tocar escrituras, AFIP ni reglas sensibles del legacy.
- Resultado: NG ya consulta `Personas` filtrando por empresa activa y registros globales; como el legacy no expone un permiso dedicado para este modulo, la pantalla quedo protegida por autenticacion sin inventar un mapeo nuevo.

## 2026-07-10

- Cambio: se agrego `Usuarios` NG en solo lectura, con listado y ficha dentro de la empresa activa usando el permiso legacy real de consulta.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: seguir ampliando cobertura funcional con un modulo administrativo de bajo riesgo antes de habilitar altas o ediciones nuevas.
- Resultado: NG ya permite consultar usuarios de la empresa con filtro por activos y detalle basico, reutilizando `FormUsuarios` para autorizacion y manteniendo fuera de alcance la edicion de usuarios y permisos.

## 2026-07-10

- Cambio: se agrego consulta NG de `Permisos de usuario` enlazada desde `Usuarios`, reutilizando la misma grilla conceptual del MVC.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: separar correctamente el alcance funcional indicado por negocio: `Permisos de usuario` los administra la empresa, mientras `Sucursales` queda reservada al flujo de super usuario.
- Resultado: NG ya puede mostrar permisos por formulario para un usuario de la empresa activa; el acceso a esta pantalla exige `FormNuevoUsuario` en modo edicion, alineado al control del MVC, y todavia no habilita guardado desde NG.

## 2026-07-10

- Cambio: se habilito el guardado NG de `Permisos de usuario`, manteniendo la misma normalizacion de dias y alcance usada por el MVC.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo empresarial de administracion de permisos sin abrir todavia modulos reservados a super usuario como `Sucursales`.
- Resultado: NG ya permite editar y persistir la grilla de permisos por formulario; si el usuario editado es el mismo autenticado, la cookie se refresca para reconstruir sus claims y aplicar los cambios de sesion inmediatamente.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Usuarios` con sucursales existentes de la empresa activa.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo empresarial de usuarios sin mezclarlo con la administracion global de `Sucursales`, que sigue reservada al super usuario.
- Resultado: NG ya puede crear y modificar usuarios con nombre, login, email, sucursal, clave, admin, activo y login fuera de sucursal; mantiene validaciones equivalentes al MVC, protege la escritura con `FormNuevoUsuario` y refresca la sesion si el usuario modifica sus propios datos.

## 2026-07-10

- Cambio: se agregaron listados NG de `Tipos de producto` para empresa activa y catalogo global, ambos en solo lectura.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: seguir ampliando cobertura administrativa con un modulo de productos de bajo riesgo antes de tocar importacion o edicion de tipos.
- Resultado: NG ya muestra la grilla de tipos propios y reservados con `formTiposProducto`, y el catalogo global de tipos con `formAddOrEditTipoProducto`, replicando el alcance actual del MVC sin habilitar todavia importacion, alta ni modificacion de tipos.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Tipos de producto` para la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo administrativo basico del modulo sin abrir todavia importacion global ni eliminaciones.
- Resultado: NG ya permite crear y modificar tipos propios con `formAddOrEditTipoProducto`, mantiene bloqueada la edicion de tipos reservados del sistema y actualiza tambien el campo `tipo` de `Corte` al renombrar un tipo existente, igual que el MVC actual.

## 2026-07-10

- Cambio: se habilito importacion NG de `Tipos globales` hacia la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo aditivo de tipos de producto antes de evaluar borrados o cambios mas sensibles.
- Resultado: NG ya permite seleccionar tipos del catalogo global y agregarlos a la empresa activa, omitiendo los que ya existen y manteniendo transaccion completa ante fallos, en linea con el comportamiento del MVC.

## 2026-07-10

- Cambio: se habilito eliminacion NG de `Tipos de producto` propios de la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: cerrar el ciclo basico del modulo manteniendo las mismas restricciones del MVC para tipos reservados y tipos usados por productos.
- Resultado: NG ya permite eliminar tipos no reservados; si existen `Corte` asociados, la operacion se bloquea con mensaje equivalente al legacy y no se permite borrar tipos reservados del sistema.

## 2026-07-10

- Cambio: se agrego `Marcas` NG en solo lectura, con listado y ficha de detalle dentro del area de productos.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: seguir ampliando cobertura funcional con un modulo chico y seguro antes de habilitar edicion de marcas, que tiene mas reglas de negocio y advertencias.
- Resultado: NG ya consulta marcas y propietario asociado usando el permiso de lectura de productos (`formCortes`), respetando el filtro por empresa activa o registros globales y sin habilitar todavia altas, cambios ni confirmaciones por similitud.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Marcas` dentro del area de productos.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: completar el flujo operativo de marcas manteniendo las reglas ya existentes del MVC para nombre, propietario y advertencias de similitud.
- Resultado: NG ya permite crear y modificar marcas con propietario opcional; en edicion, los usuarios no administradores no pueden cambiar el nombre, y si se detectan marcas parecidas se exige confirmacion explicita antes de guardar.

## 2026-07-10

- Cambio: se habilito eliminacion NG de `Marcas` propias de la empresa actual.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: cerrar el ciclo basico del modulo de marcas antes de pasar a otro vertical.
- Resultado: NG ya permite eliminar marcas no globales; la operacion se bloquea si la marca tiene compras/ventas asociadas o si esta vinculada a productos/cortes, y no se habilita borrado sobre marcas globales.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Personas`, incluyendo consulta de padron AFIP/ARCA por CUIT y autocompletado de datos.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: continuar el vertical de personas preservando las reglas del MVC actual antes de pasar a otro modulo administrativo.
- Resultado: NG ya permite crear y modificar personas propias; mantiene bloqueada la edicion de personas globales, respeta la restriccion sobre Razon Social/CUIT/Identificacion cuando la persona tiene compras o ventas y el usuario no es administrador, reutiliza el permiso de `formCtasCtes` solo para la bandera de cuenta corriente, y expone una consulta AFIP/ARCA encapsulada que autocompleta datos si la empresa tiene configurado su certificado y template.

## 2026-07-10

- Cambio: se agrego `Movimientos` NG en modo consulta, con listado filtrable y ficha de detalle con lineas.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: avanzar con el siguiente vertical usando un modulo operativo de alcance acotado y bajo riesgo antes de abrir la edicion de movimientos.
- Resultado: NG ya consulta movimientos por sucursal origen/destino y rango de fechas reutilizando `formMovimientos`, muestra cabecera y lineas de cada movimiento, y agrega el acceso al modulo en el menu principal sin tocar todavia guardado, impresion ni carga de lineas.

## 2026-07-10

- Cambio: se habilito alta y edicion NG de `Movimientos`, con ingreso rapido por codigo y cache inicial de productos frecuentes.
- Archivos o modulos afectados: `src/CarniSys.NG.Application`, `src/CarniSys.NG.Infrastructure`, `src/CarniSys.NG.Web`.
- Motivo: cerrar el vertical de movimientos replicando el flujo operativo principal antes de pasar a otro modulo.
- Resultado: NG ya permite crear y modificar movimientos con `formNuevoMovimiento`, guarda cabecera y lineas usando los mismos stored procedures legacy, precarga hasta 1000 productos de codigo `0-999` al abrir el formulario, resuelve primero desde ese cache local y solo consulta al servidor por codigo exacto cuando hace falta; con `Enter` busca de inmediato, si encuentra salta al campo de kilos y si no encuentra deja el foco en el codigo mostrando el mensaje correspondiente.
