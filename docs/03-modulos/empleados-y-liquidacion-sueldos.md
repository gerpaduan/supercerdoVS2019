# Empleados y Liquidación de Sueldos

## Objetivo

Dar de alta empleados (vinculando una Persona para la cuenta corriente y un Usuario para que el empleado marque su propia jornada), registrar su trabajo (por hora, media jornada o jornada, con o sin distinción de turno mañana/tarde) y liquidar su sueldo periódicamente, acreditando el importe en la cuenta corriente de esa Persona con el mismo mecanismo que ya usan Ventas/Compras/Pagos (`MovCtaCte`). Módulo 100% nuevo (2026-09-29), solo Postgres — ver `docs/DECISIONS.md` para el detalle de cada decisión.

## Pantallas o componentes

- **`Empleados/Index`** — listado con filtro por nombre, identificación o CUIT (columna "Empleado" combinada: razón social, y entre paréntesis la identificación solo si difiere — ver `WebCore.Helpers.EmpleadoDisplay`).
- **`Empleados/Editar`** — alta/edición unificada: crea o vincula una Persona y un Usuario en un solo paso (vincular es por modal de búsqueda — `Empleados/BuscarPersonas`/`BuscarUsuarios`, excluye preventivamente a quien ya es empleado de otro, ver `docs/DECISIONS.md` 2026-09-30), configura la forma de liquidación (Hora/MediaJornada/Jornada/Semana/Quincena/Mes) y carga tarifas (append-only, con turno y día de la semana opcionales).
- **`Empleados/Vacaciones`** — carga de períodos de vacaciones (fecha desde/hasta) por empleado.
- **`Empleados/Historial`** — historial completo de tarifas (nunca se pisan, cada cambio queda como fila nueva) y liquidaciones de un empleado, filtrable por rango de fechas.
- **`Jornadas/MiJornada`** — self-service del empleado logueado: marcar entrada/salida (Hora) o cargar jornada/media jornada con turno (Jornada/MediaJornada), ver sus marcaciones **desde su última liquidación confirmada** (o los últimos 14 días si todavía no tiene ninguna — ver `docs/DECISIONS.md` 2026-09-30), sin ningún monto de sueldo, y el aviso de vacaciones. Suma un link "¿Te olvidaste de fichar un día anterior?" (autoreporte, ver más abajo).
- **`Jornadas/MiJornadaProduccion`** (nueva, 2026-09-30) — misma pantalla pero para el usuario de producción compartido (`Usuario.EsUsuarioProduccion`): un único botón que abre `_ModalSeleccionUsuario` (sin contraseña) para elegir qué empleado está fichando, y lo manda a `FichajeConfirmar`.
- **`Jornadas/Fichaje` + `FichajeConfirmar`** — kiosco en un dispositivo habilitado (ver "Fichaje" más abajo): cualquier empleado que registre jornada (Hora, Jornada o MediaJornada) toca su nombre y confirma su entrada/salida o su jornada del día, sin contraseña.
- **`Jornadas/Index`** — vista admin: marcaciones de un empleado en un rango (feriado resaltado en amarillo con badge junto a la fecha, en vez de columna aparte; corrección de turno/feriado desplegable por fila, detrás de un botón "Corregir ▾"), "Agregar una marcación olvidada" y "Correcciones pendientes de revisar" (historial completo de auditoría, ver más abajo). El rango por defecto arranca en la última liquidación confirmada del empleado (o 14 días si no tiene ninguna); "Este mes"/"Mes anterior" son atajos, y el form manual queda para cualquier otro período. Muestra un resumen de horas/jornadas totales del rango, con un estimado de sueldo si el empleado tiene tarifa configurada (ver "Cantidad" más abajo).
- **`Liquidaciones/Index`, `Nueva`, `Preview`** — listado por empleado, selección de rango fecha+hora, preview editable (líneas automáticas + manuales, con recálculo de total en vivo en el navegador) y confirmación (solo por botón o Alt+Enter, nunca por Enter — confirmar acredita plata real en la cuenta corriente).
- **`DispositivosSeguros/Index`** — botón "Habilitar fichaje" por dispositivo (reutiliza el módulo ya existente de dispositivos confiables).

## Flujo principal

1. **Alta de empleado** (`Negocio.Empleado.GuardarUnificado`): crea (`Tipo="Empleado"`) o vincula una Persona existente, crea o vincula un Usuario, guarda la ficha de `Empleado` con su `FormaLiquidacion`.
2. **Marcación de jornada**: para `Hora`/`Jornada`/`MediaJornada` (`Empleado.RequiereRegistroJornada`), el empleado marca vía "Mi Jornada" o el kiosco de fichaje (`Negocio.RegistroJornada.Fichar`/`CargarJornada`). `Semana`/`Quincena`/`Mes` no registran nada (cobran el fijo del período, MVP sin prorrateo por ausencias).
3. **Liquidación** (`Negocio.LiquidacionSueldo.CalcularPreview` → `Confirmar`): dado un empleado y un rango fecha+hora, valida que no se solape con otra liquidación `Confirmada`, calcula automáticamente el detalle (horas/jornadas × tarifa vigente por turno+día, el fijo del período, o días de vacaciones), permite agregar líneas manuales (Premio/Bono/Aguinaldo) y confirma: persiste `LiquidacionSueldo`+`LiquidacionSueldoDetalle` y acredita el total en la cta cte de la Persona (`Negocio.CuentaCorriente.crearMovCtaCte`, `tabla=Liquidaciones`, `tipoMov=Credito`) en una única transacción (`Contratos.IUnitOfWork`).
4. **Eliminar una liquidación** genera el asiento opuesto en la cta cte (mismo mecanismo de reversa que usan Compras/Ventas) y libera el período.
5. **El cobro real** del saldo acumulado en la cta cte se hace después con el módulo **Finanzas > Pagos** ya existente — no hay una pantalla de "pago de sueldo" separada.

### Resolución de la tarifa vigente

`Negocio.Empleado.ResolverTarifaVigente(historial, turno, fecha, esFeriado)`: si `esFeriado`, intenta primero una `EmpleadoTarifa` vigente con `DiaSemana=Feriado`; si no hay (o no es feriado), busca una con `Turno` exacto y `DiaSemana` igual al día real de esa fecha; si tampoco, cae a la fila con ese `Turno` y `DiaSemana=null` (general). Nunca inventa un valor: si no hay ninguna fila configurada, la línea de detalle queda con `SinTarifaConfigurada=true` y `ValorUnitario=0`, resaltada para que el usuario la complete a mano antes de confirmar.

### Qué significa "Cantidad" en un `RegistroJornada`

Depende de `FormaLiquidacion` del empleado, y es fuente frecuente de confusión (2026-09-30):

- **Hora**: `Cantidad` **no se usa para nada** — `Negocio.LiquidacionSueldo.CalcularHora` deriva las horas trabajadas restando `HoraSalida - HoraEntrada`, ignora el campo por completo. Por eso `Jornadas/Index` no lo muestra como editable para estos empleados: en su lugar calcula y muestra las horas trabajadas de esa fila (solo informativo, no se guarda), y el mini-form de "Corregir" ni siquiera lo incluye — corregir la hora ya alcanza.
- **Jornada/MediaJornada**: `Cantidad` **es el multiplicador real** que usa `Negocio.LiquidacionSueldo.CalcularJornadaOMediaJornada` (`Cantidad × ValorUnitario` de la `EmpleadoTarifa` de ese turno). Como ahora solo se permite una asistencia por día (ver "una sola asistencia por día" más abajo), normalmente vale `1` — pero sigue siendo editable en "Corregir" por si hiciera falta un ajuste excepcional (ej. pagar una fracción).

### Feriados y días no laborables (persistente por marcación)

`RegistroJornada.EsFeriado` (bool, 2026-09-30) es un dato del propio registro, no algo que se tipee al liquidar. Se marca **solo** desde `Jornadas/Index` (`Corregir` o "Agregar una marcación olvidada", ambos admin) — nunca desde el fichaje ni desde el autoreporte del propio empleado (ver "Auditoría" más abajo): declarar un feriado es una decisión administrativa. Al liquidar, `Negocio.LiquidacionSueldo` agrupa las líneas separando las marcadas como feriado (usan la tarifa de `DiaSemana=Feriado` si existe) de las que no.

### Fichaje universal y "Mi Jornada" para producción

`Jornadas/Fichaje` + `FichajeConfirmar` ya no son exclusivos de empleados por Hora: cualquier `FormaLiquidacion` que registre jornada (`Empleado.RequiereRegistroJornada` — Hora, Jornada, MediaJornada) puede fichar ahí. Para Jornada/MediaJornada, el turno se estima solo por la hora del reloj (antes de las 12:00 → Mañana, después → Tarde), editable a mano en la confirmación.

El usuario de producción compartido (`Usuario.EsUsuarioProduccion`) no tiene un `Empleado` propio, así que `Jornadas/MiJornada` lo detecta y renderiza `MiJornadaProduccion.cshtml`: un botón abre `_ModalSeleccionUsuario` (mismo componente que Compras/Movimientos/Stock, sin contraseña) para elegir qué empleado está fichando, y lo redirige a `FichajeConfirmar?idUsuario=<id>`. El gate de acceso a `FichajeConfirmar`/`FichajeRegistrar` es `EsFichajeHabilitado(dispositivo) || EsUsuarioProduccion` — la propia sesión autenticada del usuario de producción alcanza, sin necesitar además un dispositivo marcado.

### Observación opcional al fichar

En `FichajeConfirmar`, un campo de texto libre opcional ("Observación") viaja junto con el fichaje sin implicar una corrección de hora — se guarda en `RegistroJornada.Observaciones`. Para Hora, como entrada y salida comparten la misma fila, una observación de la entrada y otra de la salida se concatenan con etiqueta (`"[Entrada] ... | [Salida] ..."`) en vez de pisarse.

### Auditoría de correcciones y altas manuales

Toda corrección (`Negocio.RegistroJornada.Corregir`) o alta manual (`AgregarManual`) agrega una fila a `registrojornadahistorial` (RLS estándar, migración `20260930b`) con una **foto completa del estado anterior** (fecha/turno/horas/cantidad/feriado antes de ESE cambio puntual), el motivo, quién y cuándo — si la misma marcación se corrige varias veces, quedan varias filas, ninguna se pisa. `TipoEvento` distingue `Correccion` / `AltaManualAdmin` / `AltaManualPropia` (esta última cuando el propio empleado carga un día olvidado desde "Mi Jornada" — ver abajo). `Jornadas/Index` lista este historial completo en "Correcciones pendientes de revisar", resolviendo el nombre de quien modificó vía `Negocio.Usuario.getUsuarioById`. Los campos `RegistroJornada.Actualizado`/`ActualizadoPor`/`MotivoCorreccion` siguen existiendo como resumen del último cambio (sin cambios), coexistiendo con la tabla de auditoría completa.

**Autoreporte de un día anterior** (`Jornadas/MiJornada` → `AgregarPropio`): el empleado puede cargar una marcación de un día que se olvidó, con motivo siempre obligatorio. A diferencia de un alta manual de admin, **nunca puede marcarse como feriado** (se fuerza `EsFeriado=false` sin importar lo que se mande) y queda etiquetada `AltaManualPropia` en el historial, para que quien revisa antes de liquidar sepa que esa fila tiene menos verificación que una marcación real o una corrección de admin. `AgregarPropio` resuelve el empleado siempre desde `_usuarioActual.Id` (nunca desde un `idEmpleado` del formulario), así nadie puede autoreportar en nombre de otro.

### Fichaje en dispositivos habilitados

Reutiliza el módulo ya existente de **Dispositivos Seguros** (`Entidades/DispositivoSeguro.cs`, identificado por un token de navegador — cookie `cs_dev`, hash `Negocio.DispositivoSeguro.SerieDeToken`), con un flag `HabilitadoFichaje` independiente de `Bloqueado`. En `Jornadas/Fichaje`, si el dispositivo está habilitado se muestra la lista de empleados activos que registran jornada; tocar un nombre lleva a `FichajeConfirmar`, que resuelve automáticamente Ingreso/Salida (Hora, según si hay un `RegistroJornada` de hoy abierto) o el turno estimado (Jornada/MediaJornada), y permite "Modificar hora" (solo Hora) con motivo obligatorio (combo de motivos típicos + "Otro" con texto libre).

**Nota operativa**: el pipeline global de WebCore exige sesión autenticada en toda acción (`Program.cs`, `RequireAuthenticatedUser`), así que `Jornadas/Fichaje` no es anónimo. En la práctica: alguien inicia sesión una vez en el dispositivo/tablet compartido y deja el navegador abierto en esa pantalla; el "sin contraseña" es por-empleado (tocar el nombre), no por-dispositivo.

### Ayuda en pantalla

Cada pantalla del módulo suma un ícono "i" (mismo patrón que `Usuarios/Editar.cshtml`/`DispositivosSeguros/Index.cshtml`: botón + modal Bootstrap) con el manual de uso de esa pantalla, con texto específico según la audiencia (admin vs. empleado) y mención breve de pantallas relacionadas cuando corresponde (ej. Liquidación → Jornadas).

## Validaciones

- Una Persona y un Usuario solo pueden estar vinculados a un Empleado.
- No se puede confirmar una liquidación cuyo rango se superponga con otra `Confirmada` del mismo empleado.
- Un `RegistroJornada` solo se puede corregir o completar (autoreporte/alta manual) mientras su fecha no esté cubierta por una liquidación `Confirmada`.
- Toda corrección (fichaje con "Modificar hora", edición admin, alta manual o autoreporte) exige motivo y queda en el historial de auditoría, visible en "Correcciones pendientes de revisar".
- `EsFeriado` nunca es asignable desde el fichaje ni desde el autoreporte propio — solo desde una corrección o alta manual de un admin.
- **Jornada/MediaJornada: una sola asistencia por día** (`Negocio.RegistroJornada.YaAsistioEseDia`, 2026-09-30) — `CargarJornada` (fichaje en tiempo real) y `AgregarManual` (alta manual de admin o autoreporte) rechazan una segunda marcación del mismo empleado para la misma fecha. `Hora` queda afuera de esta regla: puede tener varios pares entrada/salida en el mismo día (turno mañana + turno tarde, etc.).
- El total de una liquidación se recalcula siempre server-side a partir del detalle recibido (nunca se confía en el total que mande el formulario).

## Dependencias

- **Persona/Usuario** (`Entidades/Persona.cs`, `Entidades/Usuario.cs`, `Negocio/Persona.cs`, `Negocio/Usuario.cs`) — sin cambios de esquema; solo se agrega `"Empleado"` como valor de `Persona.Tipo`.
- **CuentaCorriente** (`Negocio/CuentaCorriente.cs`) — `crearMovCtaCte` con el nuevo valor `MovCtaCte.tablas.Liquidaciones`.
- **DispositivoSeguro** (`Entidades/DispositivoSeguro.cs` y sus repos) — extendido con `HabilitadoFichaje`, dual SQL Server/Postgres (a diferencia del resto del módulo, porque `DispositivoSeguro` ya era compartido antes de este cambio).
- **Permisos** (`Entidades/Permisos.cs`) — `Permisos.Empleado.{VerEmpleados,NuevoEmpleado,VerLiquidaciones,NuevaLiquidacion}`, mismo esquema Ver/Editar de `PermisosUsuarios` que el resto del sistema. Los 4 están en `UsuariosController.ClavesBloqueadasUsuarioProduccion` (defensa en profundidad: un usuario de producción nunca administra empleados ni ve liquidaciones).
- **Usuario de producción** (`Entidades/Usuario.EsUsuarioProduccion`, `_ModalSeleccionUsuario` + `seleccion-usuario.js`) — reutilizado tal cual para "Mi Jornada" de producción, sin cambios al componente.
- **`JornadasController` → `Negocio.LiquidacionSueldo`** (2026-09-30): `MiJornada` inyecta `Negocio.LiquidacionSueldo` (antes solo lo usaba `LiquidacionesController`) para resolver `ObtenerUltimaConfirmada` y armar la ventana de "Marcaciones recientes".

## Observaciones

- **Sin prorrateo por ausencias** en Semana/Quincena/Mes (decisión explícita, MVP).
- **Sin fórmula legal de aguinaldo ni de vacaciones (LCT)**: Aguinaldo se carga como línea manual; Vacaciones se paga a la tarifa de Jornada del empleado por día (aproximación simplificada, editable antes de confirmar). Riesgo aceptado y documentado — no se inventó legislación laboral (CLAUDE.md §4).
- **`TarifaGeneral`** (plantilla de tarifa estándar del comercio) tiene el backend completo (`Negocio.Empleado.ListarTarifaGeneral/AgregarTarifaGeneral/ObtenerTarifaGeneralVigente`, `DatosPostgres/TarifaGeneralPg.cs`, migración `20260929c`) pero **sin pantalla propia en WebCore todavía** — PENDIENTE, junto con el botón "Copiar tarifa estándar" en `Empleados/Editar.cshtml` que la consumiría.
- **Migraciones aplicadas y verificadas** contra la base local `carnisys` (`~/hosts/postgres-local.env`): las 7 tablas originales (2026-09-29) más `registrojornada.{esferiado,observaciones}`, la tabla `registrojornadahistorial` y el drop de `empleado.legajo` (2026-09-30) quedaron aplicadas y verificadas (`\d empleado`/`\d registrojornada`). Sigue pendiente correrlas en cualquier otro ambiente Postgres (VM/VPS) antes de desplegar ahí.
- **Se eliminó `Empleado.Legajo`** (2026-09-30): era un código interno redundante con `Persona.Identificacion`/`Persona.Cuit`. La búsqueda de empleados (`Empleados/Index` y los combos de Jornadas/Liquidaciones) pasa a filtrar/mostrar por razón social, identificación o CUIT — ver `docs/DECISIONS.md`.
- **Probado en el navegador** (2026-09-30): alta de empleado con tarifa de feriado, alta manual + 2 correcciones sucesivas sobre la misma marcación (confirmado que el historial guarda las 3 fotos sin pisarse), *cálculo* de liquidación (`CalcularPreview`) agrupando una línea feriada aparte a su propia tarifa, y el link Liquidación → Jornadas ida y vuelta. **Pendiente probar con un login de empleado real** (self-service "Mi Jornada"/autoreporte/fichaje de kiosco/producción): se verificó por código y por el camino admin, pero no se ejecutó con una sesión de empleado real por no tener su contraseña a mano en esta sesión.
- **Sin tests automatizados** para `Negocio.LiquidacionSueldo.CalcularPreview` ni para `Negocio.RegistroJornada` (deuda explícita — lógica de negocio crítica, CLAUDE.md §2.3).
- **Bug corregido (2026-09-30)**: los `<input type="number">` de Cantidad/Valor unitario en `Liquidaciones/Preview` y de Cantidad en `Jornadas/Index` (Corregir) renderizaban con coma decimal (cultura es-AR del proceso) — inválido para ese tipo de input, el navegador los trataba como vacíos. Afectaba tanto la vista previa como los valores realmente posteados al confirmar una liquidación. Corregido formateando con `CultureInfo.InvariantCulture`; no se repite en el resto de `WebCore/Views` (barrido completo, ver `docs/DECISIONS.md`).
- **Dos bugs críticos corregidos (2026-09-30) — hasta ahora nunca se había logrado confirmar una liquidación de verdad**: (1) el binding nativo de `decimal[]` (`Confirmar`) y `decimal?[]` (`Guardar` de tarifas) bajo la cultura es-AR del proceso inflaba cualquier valor con decimales ~100x (confirmado: "1.00"→100, "20000.00"→2.000.000), causando el error reportado por el usuario ("desbordamiento de campo numeric") o, peor, un total silenciosamente incorrecto cuando no llegaba a desbordar. Corregido parseando esos campos como `string` con `CultureInfo.InvariantCulture`. (2) `Negocio.LiquidacionSueldo.Confirmar`/`Eliminar` pasaban `Sucursal=null` a `crearMovCtaCte`, que siempre la exige — **ninguna liquidación se había podido confirmar nunca**, en ningún ambiente. Corregido usando `usuarioAccion.Sucursal`. Verificado en vivo de punta a punta por primera vez: calcular → confirmar → total y cuenta corriente correctos. Ver `docs/DECISIONS.md`.
