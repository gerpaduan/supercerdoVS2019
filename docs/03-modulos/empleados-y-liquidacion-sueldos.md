# Empleados y Liquidación de Sueldos

## Objetivo

Dar de alta empleados (vinculando una Persona para la cuenta corriente y un Usuario para que el empleado marque su propia jornada), registrar su trabajo (por hora, media jornada o jornada, con o sin distinción de turno mañana/tarde) y liquidar su sueldo periódicamente, acreditando el importe en la cuenta corriente de esa Persona con el mismo mecanismo que ya usan Ventas/Compras/Pagos (`MovCtaCte`). Módulo 100% nuevo (2026-09-29), solo Postgres — ver `docs/DECISIONS.md` para el detalle de cada decisión.

## Pantallas o componentes

- **`Empleados/Index`** — listado con filtro por legajo/nombre.
- **`Empleados/Editar`** — alta/edición unificada: crea o vincula una Persona y un Usuario en un solo paso (vincular es por modal de búsqueda — `Empleados/BuscarPersonas`/`BuscarUsuarios`, excluye preventivamente a quien ya es empleado de otro, ver `docs/DECISIONS.md` 2026-09-30), configura la forma de liquidación (Hora/MediaJornada/Jornada/Semana/Quincena/Mes) y carga tarifas (append-only, con turno y día de la semana opcionales).
- **`Empleados/Vacaciones`** — carga de períodos de vacaciones (fecha desde/hasta) por empleado.
- **`Empleados/Historial`** — historial completo de tarifas (nunca se pisan, cada cambio queda como fila nueva) y liquidaciones de un empleado, filtrable por rango de fechas.
- **`Jornadas/MiJornada`** — self-service del empleado logueado: marcar entrada/salida (Hora) o cargar jornada/media jornada con turno (Jornada/MediaJornada), ver la "jornada activa" (estimado desde la última liquidación confirmada hasta hoy) y el aviso de vacaciones.
- **`Jornadas/Fichaje` + `FichajeConfirmar`** — kiosco en un dispositivo habilitado (ver "Fichaje" más abajo): cualquier empleado por hora toca su nombre y confirma su entrada/salida sin contraseña.
- **`Jornadas/Index`** — vista admin: marcaciones de un empleado en un rango + "Correcciones pendientes de revisar".
- **`Liquidaciones/Index`, `Nueva`, `Preview`** — listado por empleado, selección de rango fecha+hora, preview editable (líneas automáticas + manuales) y confirmación.
- **`DispositivosSeguros/Index`** — botón "Habilitar fichaje" por dispositivo (reutiliza el módulo ya existente de dispositivos confiables).

## Flujo principal

1. **Alta de empleado** (`Negocio.Empleado.GuardarUnificado`): crea (`Tipo="Empleado"`) o vincula una Persona existente, crea o vincula un Usuario, guarda la ficha de `Empleado` con su `Legajo` y `FormaLiquidacion`.
2. **Marcación de jornada**: para `Hora`/`Jornada`/`MediaJornada` (`Empleado.RequiereRegistroJornada`), el empleado marca vía "Mi Jornada" o el kiosco de fichaje (`Negocio.RegistroJornada.Fichar`/`CargarJornada`). `Semana`/`Quincena`/`Mes` no registran nada (cobran el fijo del período, MVP sin prorrateo por ausencias).
3. **Liquidación** (`Negocio.LiquidacionSueldo.CalcularPreview` → `Confirmar`): dado un empleado y un rango fecha+hora, valida que no se solape con otra liquidación `Confirmada`, calcula automáticamente el detalle (horas/jornadas × tarifa vigente por turno+día, el fijo del período, o días de vacaciones), permite agregar líneas manuales (Premio/Bono/Aguinaldo) y confirma: persiste `LiquidacionSueldo`+`LiquidacionSueldoDetalle` y acredita el total en la cta cte de la Persona (`Negocio.CuentaCorriente.crearMovCtaCte`, `tabla=Liquidaciones`, `tipoMov=Credito`) en una única transacción (`Contratos.IUnitOfWork`).
4. **Eliminar una liquidación** genera el asiento opuesto en la cta cte (mismo mecanismo de reversa que usan Compras/Ventas) y libera el período.
5. **El cobro real** del saldo acumulado en la cta cte se hace después con el módulo **Finanzas > Pagos** ya existente — no hay una pantalla de "pago de sueldo" separada.

### Resolución de la tarifa vigente

`Negocio.Empleado.ResolverTarifaVigente(historial, turno, fecha)`: busca primero una `EmpleadoTarifa` con `Turno` exacto y `DiaSemana` igual al día de esa fecha; si no hay, cae a la fila con ese `Turno` y `DiaSemana=null` (general). Nunca inventa un valor: si no hay ninguna fila configurada, la línea de detalle queda con `SinTarifaConfigurada=true` y `ValorUnitario=0`, resaltada para que el usuario la complete a mano antes de confirmar.

### Fichaje en dispositivos habilitados

Reutiliza el módulo ya existente de **Dispositivos Seguros** (`Entidades/DispositivoSeguro.cs`, identificado por un token de navegador — cookie `cs_dev`, hash `Negocio.DispositivoSeguro.SerieDeToken`), con un flag nuevo `HabilitadoFichaje` independiente de `Bloqueado`. En `Jornadas/Fichaje`, si el dispositivo está habilitado se muestra la lista de empleados por hora activos; tocar dos veces un nombre resuelve automáticamente Ingreso/Salida (según si hay un `RegistroJornada` de hoy abierto) y permite "Modificar hora" con motivo obligatorio (combo de motivos típicos + "Otro" con texto libre).

**Nota operativa**: el pipeline global de WebCore exige sesión autenticada en toda acción (`Program.cs`, `RequireAuthenticatedUser`), así que `Jornadas/Fichaje` no es anónimo. En la práctica: alguien inicia sesión una vez en el dispositivo/tablet compartido y deja el navegador abierto en esa pantalla; el "sin contraseña" es por-empleado (tocar el nombre), no por-dispositivo.

## Validaciones

- Legajo único por empresa; una Persona y un Usuario solo pueden estar vinculados a un Empleado.
- No se puede confirmar una liquidación cuyo rango se superponga con otra `Confirmada` del mismo empleado.
- Un `RegistroJornada` solo se puede corregir mientras su fecha no esté cubierta por una liquidación `Confirmada`.
- Toda corrección (fichaje con "Modificar hora", o edición admin) exige motivo y queda visible en "Correcciones pendientes de revisar".
- El total de una liquidación se recalcula siempre server-side a partir del detalle recibido (nunca se confía en el total que mande el formulario).

## Dependencias

- **Persona/Usuario** (`Entidades/Persona.cs`, `Entidades/Usuario.cs`, `Negocio/Persona.cs`, `Negocio/Usuario.cs`) — sin cambios de esquema; solo se agrega `"Empleado"` como valor de `Persona.Tipo`.
- **CuentaCorriente** (`Negocio/CuentaCorriente.cs`) — `crearMovCtaCte` con el nuevo valor `MovCtaCte.tablas.Liquidaciones`.
- **DispositivoSeguro** (`Entidades/DispositivoSeguro.cs` y sus repos) — extendido con `HabilitadoFichaje`, dual SQL Server/Postgres (a diferencia del resto del módulo, porque `DispositivoSeguro` ya era compartido antes de este cambio).
- **Permisos** (`Entidades/Permisos.cs`) — `Permisos.Empleado.{VerEmpleados,NuevoEmpleado,VerLiquidaciones,NuevaLiquidacion}`, mismo esquema Ver/Editar de `PermisosUsuarios` que el resto del sistema.

## Observaciones

- **Sin prorrateo por ausencias** en Semana/Quincena/Mes (decisión explícita, MVP).
- **Sin fórmula legal de aguinaldo ni de vacaciones (LCT)**: Aguinaldo se carga como línea manual; Vacaciones se paga a la tarifa de Jornada del empleado por día (aproximación simplificada, editable antes de confirmar). Riesgo aceptado y documentado — no se inventó legislación laboral (CLAUDE.md §4).
- **`TarifaGeneral`** (plantilla de tarifa estándar del comercio) tiene el backend completo (`Negocio.Empleado.ListarTarifaGeneral/AgregarTarifaGeneral/ObtenerTarifaGeneralVigente`, `DatosPostgres/TarifaGeneralPg.cs`, migración `20260929c`) pero **sin pantalla propia en WebCore todavía** — PENDIENTE, junto con el botón "Copiar tarifa estándar" en `Empleados/Editar.cshtml` que la consumiría.
- **Migraciones aplicadas y verificadas** (2026-09-29) contra la base local `carnisys` (`~/hosts/postgres-local.env`): las 7 tablas nuevas quedaron con RLS habilitado, `dispositivosseguros.habilitadofichaje` se agregó, y los `formulario` 9001/9002 se sembraron sin chocar (`MAX(idform)` real era 34). Sigue pendiente correrlas en cualquier otro ambiente Postgres (VM/VPS) antes de desplegar ahí.
- **No probado en el navegador todavía**: el código compila limpio y el esquema ya existe en la base local, pero no se ejecutó el flujo completo (alta de empleado, fichaje, liquidación) contra la UI real.
- **Sin tests automatizados** para `Negocio.LiquidacionSueldo.CalcularPreview` (deuda explícita — lógica de negocio crítica, CLAUDE.md §2.3).
