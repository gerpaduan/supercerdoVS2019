# Decisiones de arquitectura

## 2026-08-20 (la mas reciente) - Negocio.Tests: primer test de logica de negocio ajena a IUnitOfWork (ComisionTarjeta)

Cerrada la ronda de contrato `IUnitOfWork` (entrada de mas abajo), este es el primer test que
cubre una regla de negocio real distinta: el calculo de `ComisionTarjeta` dentro de
`agregarVenta`/`modificarVenta` (switch sobre `FormaPago`, parametrizado por
`comisionDebito`/`comisionCredito` via `Entidades.ParamKeys`). No se extrajo el switch a un
metodo propio para facilitar el test (no se refactoriza de paso, CLAUDE.md §5) -- se prueba de
forma indirecta llamando a `agregarVenta`/`modificarVenta` y verificando el efecto observable:
`oVentaE.ComisionTarjeta` queda seteado en el mismo objeto pasado por referencia.

**Hallazgo real durante la primera corrida** (no un bug de produccion, un hueco en el fake):
con `FormaPago=Debito`/`Credito`, `egresoCajaPagoTarjeta` (que agregarVenta llama siempre,
distinto de Efectivo) intenta crear un `EgresoCaja` real via `oCierreN.addOrEditEgresoCaja` --
sin `cierreCajaN` inyectado, caia al constructor SQL-Server-only de siempre y explotaba
intentando abrir una conexion real (`NullReferenceException` dentro de `Utilidades.Conexion`).
Corregido inyectando un fake nuevo, `FakeCierreCajaRepository` (solo `addOrEditEgresoCaja` con
cuerpo real). Confirma, una vez mas, el valor de este enfoque: encontro un hueco de cobertura
en la infraestructura de test antes de que llegara a esconder un bug real.

**Fake nuevo**: `FakeParametrosContext` (`IParametrosContext` en memoria, valores fijados a
mano por clave via `.ConFloat(key, valor)`).

**5 tests nuevos, los 5 pasan tras el fix del fake**: `Efectivo_ComisionSiempreCero`,
`Debito_TomaElPorcentajeDelParametroComisionDebito`,
`Credito_TomaElPorcentajeDelParametroComisionCredito`, `OtraFormaDePago_ComisionCero`,
`ModificarVenta_TambienCalculaLaComision`. Suite completa: **19/19**. Solucion completa sigue
compilando limpio.

## 2026-08-20 (2) - Negocio.Tests: Negocio.Venta.modificarVenta, y cierre de la ronda de contrato IUnitOfWork

`modificarVenta` es un metodo separado de `agregarVenta` en `Negocio.Venta` (a diferencia de
Compra, donde `AddOrEditCompra` cubre alta y edicion en uno solo -- confirmado leyendo el
codigo: `modificarCompra` existe pero es un passthrough liviano sin `TransactionScope`/
`IUnitOfWork`, no participa de este contrato). Por eso `modificarVenta` necesitaba su propio
test, con su propio wrapper `TransactionScope`-vs-`IUnitOfWork`.

**`FakeVentaRepository` extendido**: `modificarVenta` ahora tiene cuerpo real (antes tiraba
`NotImplementedException`), con una excepcion configurable independiente de la de
`agregarVenta` (`excepcionAlModificar`), para poder probar los 2 metodos con casos de falla
propios sin que se pisen.

**2 tests nuevos, los 2 pasan al primer intento**:
`ModificarVenta_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien`,
`ModificarVenta_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla`. Suite completa: **14/14**.
Solucion completa sigue compilando limpio.

**Con esto se cierra la ronda de contrato `IUnitOfWork`**: cubierto en los 3 callers reales
(`agregarVenta`, `modificarVenta`, `AddOrEditCompra`, `addOrEditPago` -- 4 metodos, 3 clases)
mas la logica de anulacion de `crearMovCtaCte` (2 ramas). Lo que quedaria, si se retoma en el
futuro: tests de integracion real contra Postgres (fuera del alcance de "unitarios con repos
falsos", la estrategia elegida al arrancar esta suite), y logica de negocio no relacionada a
`IUnitOfWork` (ej. calculo de `ComisionTarjeta`, egresos de caja por tarjeta).

## 2026-08-20 (3) - Negocio.Tests: mismo contrato de IUnitOfWork sobre Negocio.CuentaCorriente.addOrEditPago, ultimo de los 3 callers reales

Cierra la cobertura del contrato `IUnitOfWork` en sus 3 callers reales de esta migracion
(`Negocio.Venta.agregarVenta`, `Negocio.Compra.AddOrEditCompra`, y ahora
`Negocio.CuentaCorriente.addOrEditPago`). Diferencia de diseño respecto a los otros 2: en
Venta/Compra el repo de `CuentaCorriente` se inyecta *aparte* (`ctaCteN:`), con su propio fake;
en Pagos, `addOrEditPago` es un metodo de `Negocio.CuentaCorriente` mismo, asi que el **mismo**
`ICuentaCorrienteRepository` sirve tanto para `IniciarUnitOfWork`/`addOrEditPago` (el pago en
si) como para `getMovCtaCteBy`/`addOrEditMovCtaCte` (via `crearMovCtaCtePago`, que se ejecuta
despues sobre el mismo pago ya guardado).

**`FakeCuentaCorrienteRepository` extendido** (no uno nuevo, reuso deliberado): constructor
ahora acepta `unitOfWorkAEntregar`/`excepcionAlAddOrEditPago` opcionales (default null, sin
romper los tests existentes que ya lo construian sin argumentos); `getChequesPorPago` fiel al
real (`idPago<=0` -> lista vacia, pago nuevo sin cheques que buscar); `resetearChequesAsignados`
no-op; `addOrEditPago` asigna un Id nuevo o tira la excepcion configurada.

**2 tests nuevos, los 2 pasan al primer intento**:
`AddOrEditPago_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien`,
`AddOrEditPago_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla` -- este ultimo verifica ademas que,
si `addOrEditPago` (paso 1) falla, `crearMovCtaCtePago` (paso 2) nunca se ejecuta: cero
`MovCtaCte` creados. Suite completa: **12/12**. Solucion completa sigue compilando limpio.

**Cobertura actual de `Negocio.Tests`**: logica de anulacion de `crearMovCtaCte` (2 ramas) +
contrato `IUnitOfWork` en los 3 callers reales (Venta/Compra/Pagos). Pendiente, si se quiere
seguir: las ramas de edicion (`modificarVenta`, edicion de Compra).

## 2026-08-20 (4) - Negocio.Tests: mismo contrato de IUnitOfWork sobre Negocio.Compra

Mismo patron que `VentaIUnitOfWorkTests` (entrada de mas abajo), aplicado a
`Negocio.Compra.AddOrEditCompra` -- el contrato de `IUnitOfWork` (`Completar()` solo si toda la
operacion sale bien) es identico en Venta y Compra, mismo diseño, misma etapa de la migracion.
Escenario minimo elegido para no tener que fakear `ICierreCajaRepository`: `esEgresoCaja=false`
(corta ese bloque entero), `TipoCompra=Cortes` (no `PesajeCortes`, evita `actualizarEstadoPesaje`),
listas de medias/cortes vacias. `crearMovCtaCteCompra` (que si se ejecuta siempre) reusa
`FakeCuentaCorrienteRepository` igual que en Venta. Fake nuevo: `FakeCompraRepository`, mismo
criterio que `FakeVentaRepository` (solo `IniciarUnitOfWork`/`addOrEditCompra` con cuerpo real,
resto `NotImplementedException`).

**2 tests nuevos, los 2 pasan al primer intento**:
`AddOrEditCompra_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien`,
`AddOrEditCompra_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla`. Suite completa: **10/10**.
Solucion completa sigue compilando limpio.

**Cobertura actual de `Negocio.Tests`**: la logica de anulacion de `crearMovCtaCte` (las 2
ramas: Pagos y "sacar de cta cte" de Venta/Compra) y el contrato `IUnitOfWork` en los 2
callers reales (`Negocio.Venta`, `Negocio.Compra`). Pendiente, si se quiere seguir: el mismo
contrato sobre `Negocio.CuentaCorriente.addOrEditPago` (Pagos tambien lo usa, no probado
todavia en aislamiento) y `Negocio.Venta.modificarVenta`/`Negocio.Compra` en su rama de edicion.

## 2026-08-20 (5) - Negocio.Tests: primer test directo sobre Negocio.Venta -- contrato de IUnitOfWork

Primer test que instancia `Negocio.Venta` directamente (los anteriores probaban
`Negocio.CuentaCorriente` en aislamiento). Cubre el contrato central de la arquitectura
`IUnitOfWork` construida esta sesion (ver "Venta resuelta de fondo", mas abajo): sobre
`agregarVenta` con el camino Postgres (`IniciarUnitOfWork()` devuelve un `IUnitOfWork` no nulo),
`Completar()` debe llamarse **solo** si toda la operacion sale bien; si algo falla, nunca se
llama (el `Dispose()` de `UnitOfWorkPg` hace el rollback implicito). Un regresion aca -- alguien
que "simplifica" `agregarVenta` mas adelante y rompe ese orden -- dejaria una transaccion real
de Postgres commiteada a medias o sin rollback, silenciosamente.

**Fakes nuevos**: `FakeUnitOfWork` (registra si `Completar()`/`Dispose()` se llamaron, sin logica
real), `FakeVentaRepository` (implementacion minima de las 47 `IVentaRepository` -- solo
`IniciarUnitOfWork`/`agregarVenta` tienen cuerpo real; el resto tira `NotImplementedException`,
sin necesidad para el escenario probado: venta en Efectivo, sin lineas, sin expendios, para que
`egresoCajaPagoTarjeta` corte temprano y no haga falta fakear `ICierreCajaRepository`).
`crearMovCtaCteVenta` (que si se ejecuta, siempre) reusa el `FakeCuentaCorrienteRepository` ya
existente via el constructor aditivo de `Negocio.Venta` (`ctaCteN:`).

**2 tests nuevos, los 2 pasan al primer intento**:
`AgregarVenta_ConIUnitOfWork_CompletaLaTransaccionSiTodoSaleBien` (feliz: `Completar()` y
`Dispose()` llamados, en ese orden logico) y
`AgregarVenta_ConIUnitOfWork_NoCompletaLaTransaccionSiFalla` (repo tira una excepcion real:
se verifica que se propaga envuelta con el mensaje "Error en registrar la venta", con la
excepcion original como `InnerException`, y que `Completar()` **nunca** se llamo). No cubre el
camino SQL Server/`TransactionScope` (dificil de observar desde un test unitario sin una base
real) -- sigue siendo el mecanismo de siempre, sin cambios de esta migracion.

Suite completa: **8/8**. Solucion completa sigue compilando limpio.

## 2026-08-20 (6) - Negocio.Tests: cubre tambien la rama "sacar de cta cte" (Venta/Compra)

Continua la entrada de mas abajo (arranque de `Negocio.Tests`). `CuentaCorrienteAnulacionTests`
cubria la rama de `crearMovCtaCte` que usa Pagos (cambio de tipo/importe -> anula y crea uno
nuevo). Faltaba la rama que usan Venta/Compra: sacar algo de cta cte (`crearMovCtaCte=false`
via `oVentaE.EnCtaCte`/`oCompraE.EnCtaCte`) -- deja el original intacto y crea un opuesto con
`QuitadoCtaCta=true`, **sin** insertar un registro nuevo despues (a diferencia de Pagos). Nuevo
archivo `CuentaCorrienteQuitarDeCtaCteTests.cs`, mismo `FakeCuentaCorrienteRepository` (la
logica de `crearMovCtaCte` no distingue por tabla, alcanza con probarla una vez con
`tabla=Ventas`). Replica el resultado verificado a mano para Compra (entrada "Compra: mismo fix
de IUnitOfWork...", mas abajo): registro original intacto + opuesto
(`Tipo=Debito,Importe=-160,QuitadoCtaCta=true,Detalle="Quitado de Cta.Cte."`).

2 tests nuevos, los 2 pasan al primer intento (el fake ya estaba corregido de la entrada
anterior): `SacarVentaDeCtaCte_DejaElOriginalIntactoYCreaUnOpuesto`,
`VolverAPonerEnCtaCte_TrasHaberlaSacado_CreaUnNuevoRegistroActivo`. Suite completa: **6/6**.
Solucion completa sigue compilando limpio.

**Sigue pendiente, no en el alcance de esta entrada**: extender a Venta/Compra sus propias
clases `Negocio.*` directamente (mas alla de la logica compartida de `CuentaCorriente` que ya
queda cubierta), y la decision de si en algun momento se suma integracion real contra Postgres.

## 2026-08-20 (7) - Arranca la suite de tests automatizados: proyecto Negocio.Tests, xUnit, unitarios con repos falsos

Hasta ahora, cero tests automatizados en el repo -- toda la verificacion de esta migracion fue
manual (HTTP + SQL directo). El usuario pidio arrancar la suite. Decisiones tomadas (con
confirmacion explicita via preguntas):

- **Framework: xUnit.** Alternativas descartadas: NUnit (equivalente, sin ventaja concreta
  para este proyecto), MSTest (mas verboso para casos parametrizados).
- **Estrategia: unitarios con repos falsos, sin tocar SQL Server ni Postgres.** Gracias al
  patron de constructor aditivo que ya tienen `Negocio.CuentaCorriente`/`Venta`/`Compra`
  (inyectan cualquier `Contratos.I*Repository`, real o falso), se puede testear la logica de
  negocio pura sin una base levantada. Integracion contra Postgres real queda para una etapa
  aparte, si se decide mas adelante.
- **`Negocio.Tests/Negocio.Tests.csproj` (nuevo, agregado a `CarniSys.sln`)**: SDK-style,
  **`net472`** (no `net10.0`, el default del template `dotnet new xunit`) -- `Negocio`/`Datos`/
  `Utilidades`/`Entidades` son .NET Framework 4.7.2; referenciar un TFM moderno arriesgaba
  incompatibilidades reales. `Utilidades.csproj` tiene referencias COM (`ResolveComReference`)
  que **`dotnet build` no puede resolver** (task no soportada en MSBuild de .NET Core) -- el
  proyecto de tests se compila y corre con el MSBuild de Visual Studio (el mismo binario que ya
  se usa para toda la solucion), no con `dotnet build`/`dotnet test`. Runner: `vstest.console.exe`
  (bundjob con VS) contra el `.dll` compilado.
- **Namespace del proyecto: `NegocioTests`, no `Negocio.Tests`.** `Negocio.Tests` como namespace
  quedaria anidado dentro de `Negocio` -- la resolucion de nombres de C# hace que tipos del
  namespace contenedor (`Negocio.Persona`, `Negocio.Sucursal`, `Negocio.Usuario`, las clases de
  logica de negocio) tapen a los `using Entidades;` (`Entidades.Persona`, etc.), rompiendo la
  compilacion con errores confusos. Se descubrio por error de compilacion real, no de antemano.

**Primer target elegido**: la logica de anulacion de `Negocio.CuentaCorriente.crearMovCtaCte`
(usada por Venta/Compra/Pagos), replicando con asserts los 3 escenarios de Pagos verificados a
mano en la entrada de mas abajo (modificar importe, Pago->Cobro, cambiar persona). El fake
(`Fakes/FakeCuentaCorrienteRepository.cs`) replica la semantica EXACTA de
`CuentaCorrientePg.getMovCtaCteBy`/`addOrEditMovCtaCte` (leida de su SQL real, no inventada):
ultimo registro por Tabla+IdTabla, insert si Id==0, update in-place si no.

**Bug real encontrado en el fake durante el primer corrida** (no en `Negocio.CuentaCorriente`):
`getMovCtaCteBy` devolvia la referencia viva al objeto guardado en la lista en memoria, no una
copia -- una lectura real de ADO.NET siempre materializa un objeto nuevo por fila. Como
`crearMovCtaCte` muta el objeto que recibe (`oMovCtaCte.Id = 0`, etc.) antes de "reinsertarlo",
la mutacion corrompia el registro ya persistido in-place. Corregido devolviendo una copia
(`Clonar`). Confirma el valor de este tipo de test: agarro un bug real, aunque estaba en el
fake y no en el codigo de produccion.

**4 tests, los 4 pasan** (`vstest.console.exe`, target net472):
`PrimerPago_CreaUnSoloMovimiento`, `ModificarImporte_AnulaElViejoYCreaUnoNuevo`,
`PagoAConvertidoEnCobro_AnulaElViejoYCreaUnoNuevo`, `CambiarSoloLaPersona_ActualizaElMovimientoExistenteSinAnular`.
Solucion completa (`CarniSys.sln`, con `Negocio.Tests` agregado) sigue compilando limpio con
el MSBuild de Visual Studio.

**Pendiente, no en el alcance de esta entrada**: extender la suite a Venta/Compra (mismo patron
de fake, otros repos), y decidir si en algun momento se suma integracion real contra Postgres.

## 2026-08-20 (8) - Bug real preexistente encontrado y corregido: StockController usaba el SP de WinForms en vez del de Web

Durante la auditoria de que falta para el modo dual, encontre que `Negocio.Corte` tiene 5
metodos que quedan 100% en SQL Server sin importar `DataEngine` (`obtenerEmbutidos`,
`reiniciarStockReal/Teorico`, `CierreStock`, `StockIngresoEgreso`, `TotalKgsCortePorCompra`,
via el campo `oCorteDSqlServer`). De los 5, solo `CierreStock` tenia un caller real en `Web/`:
`StockController.ObtenerProductosNoCargadosCierre` (parte del flujo de Cierre de Stock al
cargar una compra).

Mi primer instinto fue reemplazar el llamado por `CierreStockWeb` (ya migrada a Postgres, usada
en `ReportesController`) asumiendo que era el mismo dato con otro nombre de SP. **Verifique
antes de tocar codigo, corriendo `a_CierreStock` y `a_CierreStockWeb` contra la base real con
los mismos parametros**: la columna `DIF` (la que lee `StockController`) da `.00` en **todos**
los casos en `a_CierreStock`, contra valores reales no-cero en `a_CierreStockWeb` -- no son la
misma columna pese al nombre igual.

**El usuario confirmo la causa real**: `a_CierreStock` es el SP de WinForms; en Web corresponde
usar siempre `a_CierreStockWeb`. `StockController` (codigo Web) estaba llamando al SP
equivocado -- un **bug preexistente de la aplicacion original, no un gap de la migracion a
Postgres** (aunque lo encontre por estar auditando esa migracion). Corregido: el llamado ahora
usa `oCorteN.CierreStockWeb(...)`, que ya esta migrada a Postgres via `Contratos.ICorteRepository`
-- de paso, cierra tambien el ultimo gap real de Postgres en `Corte.cs` (los otros 4 metodos de
`oCorteDSqlServer` no tienen ningun caller en `Web/`).

**Verificado con HTTP real, login real, ambos motores**: `POST /Stock/ProductosNoCargadosCierre`
(idSucursal=1, fechaCompra=2026-08-20) devuelve la lista completa de productos con
`stockActual` real (ej. CARRE=229.319, Chorizo=-1.25, Costilla=36.76 -- antes del fix, todos
daban 0 por el bug de SP) -- **respuesta identica byte a byte en SQL Server y Postgres**.

**Correccion a la entrada anterior sobre RLS**: en el reporte de estado de esta misma sesion
dije que `usuarios`/`usuariopasswordresettokens` sin RLS era un gap pendiente de decision. Era
un error mio -- ya estaba decidido y confirmado por el usuario (ver las entradas de la Etapa
13a, mas abajo: "usuarios en Postgres NO lleva RLS... El usuario señalo la razon antes de que
se implementara"). No era una decision nueva, la saco de la lista de pendientes.

## 2026-08-20 (9) - Cierre del cableado a NegocioFactory: los 9 controllers que quedaban pendientes

Cierra el hallazgo documentado en la entrada de Compra (mas abajo): un barrido con
`grep -rln "= new Negocio\." Web/Controllers/*.cs` habia encontrado controllers con cableado
parcial (algunos campos a `NegocioFactory`, otros en el constructor plano SQL-Server-only) o
directamente sin cablear. Corregidos los 9 que quedaban:

- **Cableado parcial** (un campo suelto seguia en SQL Server aunque el resto del controller ya
  usaba `NegocioFactory`): `BaseController` (`ResolverUsuarioCreador`, un `Negocio.Usuario`
  local usado solo para resolver el usuario real detras de la sesion compartida de sala de
  produccion), `CajasController` (`oSucursalN`/`oUsuarioN`/`oVentaN`), `HomeController`
  (`oUsuarioN`/`oSucursalN`/`oVentaN`/`oCorteN`), `ReportesController`
  (`oSucursalN`/`oCorteN`/`oCompraN`/`oVentaN`).
- **Sin cablear ningun campo**: `AuditoriaLoginController`, `ElaboradosController`,
  `MovimientosController`, `PuntosExpendioController` (incluye un `Negocio.Usuario` local en
  `ExpendiosGenerados`), `UsuariosController` (incluye un `Negocio.Usuario` local en
  `GuardarUsuario` usado solo para re-consultar el Id tras un alta).

Mismo patron que el resto de la migracion: cada `new Negocio.X(...)` reemplazado 1:1 por
`Web.Infrastructure.NegocioFactory.CrearX(...)`, preservando los argumentos exactos que ya
tenia cada caller (algunos pasaban `param`, otros no -- se respeto tal cual). Sin cambios de
logica, solo de que motor decide `DataEngine`.

**Con esto, `grep -rln "= new Negocio\." Web/Controllers/*.cs` da solo 2 resultados, ambos
deliberados**: `MigracionPostgresController` (herramienta de comparacion, no de producto, arma
ambos motores a proposito) y `WhatsAppController` (feature sin migrar, ver memoria del usuario
2026-07-29). Todos los controllers de producto quedan cableados.

**Verificado**: `CarniSys.sln` compila limpio. Barrido HTTP de los 9 controllers tocados (mas
`/Home`), login real, en **ambos motores** -- 200 en los 8 con accion `Index`/default, y en
`PuntosExpendioController` (sin accion `Index`) contra `ExpendiosGenerados` -- sin errores en
ninguno de los dos.

**Lo que sigue sin resolver, fuera de alcance de esta entrada** (ver reporte de estado
2026-08-20): `usuarios`/`usuariopasswordresettokens` sin RLS en Postgres pese a tener
`idempresa` (requiere decision explicita, es dato de auth/PII); metodos de `Corte.cs`/
`Compra.cs` que siguen 100% en SQL Server (documentado, no bug); cero tests automatizados;
sin `docs/RUNBOOK.md`; sin sincronizacion de datos ni red hacia Postgres desde `ServidorSM`/
`San Lorenzo`.

## 2026-08-20 (10) - Pagos/Cobros: mismo fix de IUnitOfWork; hallazgo de negocio preexistente (no bug) sobre cuando se anula un MovCtaCte

Pedido del usuario: 3 escenarios sobre `CuentaCorrientePg.addOrEditPago` (Pagos/Cobros) --
modificar el importe de un pago, convertir un Pago en Cobro (toggle `AProveedor`), y corregir
la persona de un pago -- verificando en los 3 casos que se anule el `MovCtaCte` viejo y se cree
uno nuevo.

**Fix de plomeria, mismo patron que Venta/Compra**: `FinanzasController.OnActionExecuting`
tenia `oSucursalN`/`oUsuarioN`/`oPersonasN` sin cablear a `NegocioFactory` (mismo hallazgo de
cableado parcial que las etapas anteriores; `oCtaCteN`/`oCierreN` ya estaban bien). Corregido.
`Contratos.ICuentaCorrienteRepository.IniciarUnitOfWork()` agregado (`Datos.CuentaCorriente`:
null; `CuentaCorrientePg`: `UnitOfWorkPg` real); parametro opcional `IUnitOfWork` agregado a
`getChequesPorPago`, `resetearChequesAsignados`, `addOrEditPago`. `CuentaCorrientePg.addOrEditPago`
reestructurado igual que `agregarVenta`/`AddOrEditCompra` (`EjecutarAddOrEditPago` extraido,
rama unica por motor). `Negocio.CuentaCorriente.addOrEditPago` reescrito con el mismo wrapper
`TransactionScope`-vs-`IUnitOfWork`; `crearMovCtaCtePago` ahora threadea el `unitOfWork` hacia
`crearMovCtaCte` (que ya lo aceptaba desde el fix de Venta).

**Hallazgo real, sin relacion con la plomeria de transacciones**: el escenario "corregir la
persona de un pago -> debe anularse y crearse en la nueva persona" **no se cumple** -- y esto
es comportamiento preexistente de `crearMovCtaCte` (`Negocio/CuentaCorriente.cs`), sin cambios
en esta etapa, no un bug introducido por el fix. La logica de anulacion solo se dispara cuando
difieren **Tipo o Importe** del `MovCtaCte` encontrado (`getMovCtaCteBy` busca por
`Tabla+IdTabla`, sin filtrar por persona); si un pago cambia solo de persona (mismo tipo, mismo
importe), el registro existente se **actualiza in-place** (mismo `Id`, nuevo `IdPersona`) en vez
de anularse y recrearse. Verificado identico en ambos motores (ver abajo) -- confirma que es
logica de negocio heredada, no una regresion de la migracion.

**Decision del usuario (2026-08-20, mismo dia)**: retira el pedido de que el cambio de persona
tambien anule+recree. El comportamiento actual (mover el `MovCtaCte` existente a la persona
correcta, in-place, sin anulacion) queda confirmado como el deseado -- un pago sigue siendo
"el mismo movimiento", solo corregido de persona, no una operacion nueva. Alternativa descartada:
anular+recrear por cambio de persona (lo pedido originalmente) -- se descarta porque duplicaria
el historial de movimientos sin necesidad real, dado que Tipo/Importe no cambiaron. Sin cambios
de codigo: `crearMovCtaCte` (Venta/Compra/Pagos) queda como esta.

**Verificado con escrituras reales, ambos motores, pago Id=61 en cada uno**:
- Escenario A (importe 100 -> 150): registro original (`Debito -100`) intacto, `ANULACION`
  (`Credito +100`) + nuevo registro (`Debito -150`) creados. Identico en Postgres y SQL Server.
- Escenario B (Pago -> Cobro, `AProveedor` true -> false, mismo importe 150): registro anterior
  (`Debito -150`) intacto, `ANULACION` (`Credito +150`) + nuevo registro (`Credito +150`, sin
  "ANULACION" en el detalle) creados. Identico en ambos motores.
- Escenario C (persona 13 -> 19, mismo tipo/importe): **sin anulacion** -- el ultimo `MovCtaCte`
  (mismo `Id`) paso de `IdPersona=13` a `IdPersona=19` in-place. Comportamiento identico,
  registro por registro, en Postgres y SQL Server (confirma que no es una regresion).

**Estado final**: `DataEngine=SqlServer` (confirmado). `CarniSys.sln` compila limpio. Pago
`Id=61` (`NroRecibo=TEST-PG-A` en Postgres, `TEST-SQL-A` en SQL Server) queda como dato de
prueba, sin via real de la app para eliminar pagos (`eliminarPago` es `NotImplementedException`
preexistente, ver Etapa 5).

## 2026-08-20 (11) - Compra: mismo fix de IUnitOfWork + ComprasController nunca habia sido cableado a NegocioFactory

Pedido del usuario: repetir para `Compra` el mismo test que `Venta` (cargar en CtaCte, sacarla,
verificar la anulacion en cuenta corriente). Se encontraron y corrigieron **2 problemas reales**.

**1) `Negocio.Compra.AddOrEditCompra` tenia el mismo `TransactionScope` sin resolver** que
`Venta`/`modificarVenta` (arreglados en las 2 entradas anteriores) -- nunca se habia migrado a
`IUnitOfWork` porque el primer test de Compra (Ingreso Stock, via `StockController`) tenia
`EnCtaCte` hardcodeado a `false`, asi que nunca ejercito el camino de CuentaCorriente que
rompe con multiples conexiones Postgres dentro del mismo TransactionScope. Mismo fix aplicado:
`Contratos.ICompraRepository.IniciarUnitOfWork()` (`Datos.Compra`: null: `CompraPg`:
`UnitOfWorkPg` real), parametro opcional `IUnitOfWork` en `addOrEditCompra`,
`agregarCortePorCompra`, `agregarMediaRes`; `Negocio.Compra.AddOrEditCompra` reestructurado
igual que `agregarVenta` (`EjecutarAddOrEditCompra` extraido, rama unica por motor).
**Limite documentado**: `oCorteN.editPrecioCorte` y `actualizarEstadoPesaje` (dentro del mismo
metodo) siguen sin participar de la unidad de trabajo compartida -- caminos condicionales
(`ActualizarPrecioVenta`, tipo `PesajeCortes`) no exercitados por ningun test real hasta ahora.
Si alguna vez fallan en Postgres, aplicar el mismo patron ahi tambien.

**2) Hallazgo mayor, sin relacion con transacciones**: al probar el flujo real de "compra a
proveedor" (`ComprasController`, distinto de `StockController`), la escritura fue a **SQL
Server incluso con `DataEngine=Postgres`** -- `ComprasController.OnActionExecuting` nunca habia
sido cableado a `NegocioFactory` salvo el campo `oCierreN` (tocado de pasada en la etapa de
`CierreCaja`). Los otros 5 campos (`oCompraN`, `oSucursalN`, `oUsuarioN`, `oPersonaN`,
`oCorteN`) seguian en el constructor plano SQL-Server-only. Corregido, cableados los 5 a
`NegocioFactory`. **Confirmado por barrido completo (`grep -rn "= new Negocio\." Web/Controllers`)
que el mismo patron -- controllers con algun campo cableado de una etapa puntual, pero otros
campos del mismo controller todavia sin tocar -- se repite en `CajasController`,
`FinanzasController`, `HomeController`, `ReportesController`, `MovimientosController`,
`PuntosExpendioController`, `UsuariosController`, `AuditoriaLoginController`, `ElaboradosController`**.
No se corrigen en esta etapa (fuera del pedido puntual de Compra) -- queda como hallazgo
importante para una etapa dedicada aparte, con el mismo criterio de "un controller/clase por
vez" ya usado toda la migracion.

**Verificado con escrituras reales, ambos motores, ciclo completo carga+anulacion**:
- Postgres (`ComprasController`, ya cableado): compra `Cortes`, `EnCtaCte=true`, proveedor real
  -- `idcompra=9034`, linea en `corteporcompra`, `movctacte` con `Tipo=Credito, Importe=160,
  QuitadoCtaCta=false`. Modificada la misma compra a `EnCtaCte=false` -- confirmado por SQL
  directo: registro original **intacto**, mas un segundo registro real y opuesto
  (`Tipo=Debito, Importe=-160, QuitadoCtaCta=true, Detalle="Quitado de Cta.Cte."`). SQL Server
  en 0 para esa observacion.
- SQL Server, mismo ciclo completo (`idcompra=9035`): resultado **identico** (mismos montos,
  mismo texto de detalle) -- cero divergencia de comportamiento entre motores.
- Regresion final en `SqlServer` sobre `/Home`, `/Compras/Index`, `/Ventas/Index`,
  `/Stock/Index`, `/Productos/Index`, `/Finanzas/CtasCtes` -- limpia.

**Estado final**: `DataEngine=SqlServer` (confirmado). `CarniSys.sln` compila limpio. Datos de
prueba (compras 9034 en Postgres, 9033/9035 en SQL Server local de dev, observaciones
`TEST_COMPRA_CTACTE_ANULACION*`) quedan documentados, sin via real de la app para eliminar
compras.

## 2026-08-20 - modificarVenta: mismo fix de IUnitOfWork, verificado el ciclo completo de anulacion en Cuenta Corriente

Pedido del usuario: modificar una venta en Cuenta Corriente y confirmar que el movimiento se
anula correctamente. `Negocio.Venta.modificarVenta` tenia el mismo `TransactionScope` sin
resolver que `agregarVenta` (arreglado en la entrada anterior) -- mismo fix aplicado aca:
`VentaPg.modificarVenta` ahora acepta `Contratos.IUnitOfWork` (reusa la conexion/transaccion
compartida en vez de abrir la propia via `AbrirConTenant`), `Datos.Venta.modificarVenta` la
ignora (SQL Server sigue con `TransactionScope`), y `Negocio.Venta.modificarVenta` se reestructuro
igual que `agregarVenta` (cuerpo extraido a `EjecutarModificarVenta`, rama unica por motor).
De paso se corrigio un comentario desactualizado en `Contratos/IVentaRepository.cs` que decia
que el reverso de EgresosCaja no estaba implementado -- si lo esta, desde la Etapa 8.

**Test real pedido por el usuario, verificado de punta a punta en los dos motores**:
1. Se crea una venta real en CtaCte (`FormaPago=CtaCte`, persona real no-Consumidor-Final) --
   confirmado por SQL directo: `movctacte` con `Tipo=Debito, Importe=-150, QuitadoCtaCta=false`.
2. Se modifica la misma venta sacandola de CtaCte (`FormaPago=Efectivo`, `SoloFormaPago=true`)
   via un POST real a `/Ventas/ModificarVenta`.
3. Verificado por SQL directo: el registro original de `movctacte` **queda intacto** (historial),
   y se crea un **segundo registro real, opuesto**: `Tipo=Credito, Importe=+150,
   QuitadoCtaCta=true, Detalle="Quitado de Cta.Cte."` -- saldo neto de la cuenta corriente del
   cliente vuelve a 0. Mismo resultado exacto (mismos montos, mismo texto de detalle) en
   Postgres (venta 1739) y en SQL Server (venta 1734) -- cero divergencia de comportamiento
   entre motores tras el fix.

**Verificado**: `CarniSys.sln` compila limpio. Regresion final en `SqlServer` sobre
`/Home`, `/Ventas/Index`, `/Ventas/POS`, `/Ventas/MisVentas`, `/Finanzas/CtasCtes`, limpia.
`DataEngine=SqlServer` (confirmado). Datos de prueba (ventas 1739 en Postgres y 1734 en SQL
Server, observaciones `TEST_CTACTE_ANULACION`/`TEST_CTACTE_ANULACION_SS`) quedan documentados,
sin via real de la app para eliminar ventas.

## 2026-08-20 - Venta resuelta de fondo: IUnitOfWork explicito reemplaza TransactionScope en el camino Postgres

Cierre de la deuda de `Venta` dejada abierta en la entrada anterior. Implementada la "Opcion 2"
(conexion+transaccion explicita compartida, en vez de depender de `TransactionScope`+auto-
enlistment de Npgsql, que ya habia demostrado ser end poco confiable para el aislamiento RLS).

**Diseno implementado** (~10 archivos, todo aditivo):
- `Contratos.IUnitOfWork` (interfaz nueva, sin depender de Npgsql): `Completar()` + `IDisposable`.
- `DatosPostgres.UnitOfWorkPg` (implementacion real): abre una conexion+transaccion explicita
  una sola vez, fija `app.id_empresa` una vez, y la expone para reusar.
- `Contratos.IVentaRepository.IniciarUnitOfWork()`: `Datos.Venta` (SQL Server) devuelve `null`
  (sigue con `TransactionScope`, sin cambios); `VentaPg` devuelve una `UnitOfWorkPg` real.
- Parametro opcional `Contratos.IUnitOfWork unitOfWork = null` agregado a los 6 metodos que
  necesitaban compartir la transaccion: `IVentaRepository.agregarVenta/asignarVentaEnExpendio/
  agregarLineaVenta`, `ICierreCajaRepository.addOrEditEgresoCaja`,
  `ICuentaCorrienteRepository.getMovCtaCteBy/addOrEditMovCtaCte`. Las implementaciones SQL
  Server (`Datos.Venta/CierreCaja/CuentaCorriente`) ignoran el parametro -- cero cambio de
  comportamiento. `DbPg.cs` gano 3 overloads (`NonQuery`/`Scalar`/`Reader`) que aceptan
  `IUnitOfWork` directamente, centralizando la rama "usar la conexion compartida vs abrir la
  propia" en un solo lugar en vez de repetirla en cada metodo Postgres.
- `Negocio.Venta.agregarVenta` reescrito: pide `oVentaD.IniciarUnitOfWork()`; si es null usa
  `TransactionScope` exactamente como antes (extraido a un metodo privado `EjecutarAgregarVenta`
  para no duplicar el cuerpo); si no es null, envuelve la misma logica en `using (unitOfWork)`
  y la completa/descarta en vez de usar `scope.Complete()`. `crearMovCtaCteVenta`,
  `egresoCajaPagoTarjeta` y `agregarLineaVenta` (los 3 metodos internos de `Negocio.Venta`)
  propagan `unitOfWork` hacia sus repos.

**Un segundo bug real encontrado en el camino, sin relacion con transacciones**:
`VentaPg.agregarVenta` nunca incluia `idempresa` en la lista de columnas del INSERT a
`ventas` -- la columna cae al `DEFAULT 0`, y la politica RLS de escritura
(`WITH CHECK (idempresa = current_setting(...)::integer)`, sin la excepcion de `idempresa = 0`
que si tiene la politica de lectura) rechazaba el insert con `42501`. Este era el error real
detras del mensaje de RLS que parecia (pero no era) un problema del `IUnitOfWork` -- confirmado
recreando el error incluso con la transaccion compartida funcionando perfectamente. Revisado el
resto de `VentaPg.cs`/`CuentaCorrientePg.cs`/`CierreCajaPg.cs`/`CompraPg.cs`: ningun otro INSERT
tiene el mismo problema, caso aislado.

**Verificado con escrituras reales, ambos motores**:
- Postgres, venta en efectivo: POST real a `/Ventas/FinalizarVenta` -- 200, `ventaId=1737`.
  Confirmado por SQL directo: venta + linea en Postgres con `idempresa=1` correcto, **sin**
  movimiento de cuenta corriente (correcto -- `FormaPago=Efectivo` no es CtaCte), SQL Server
  en 0 para esa observacion.
- Postgres, venta con tarjeta de debito (ejercita `egresoCajaPagoTarjeta`/`CierreCajaPg`, no
  probado hasta ahora): POST real -- 200, `ventaId=1738`. Confirmado por SQL directo: venta +
  egreso de caja ("Venta Debito - ID:1738") ambos en Postgres, compartiendo la misma unidad de
  trabajo con la venta.
- SQL Server, venta en efectivo (para confirmar cero regresion en el camino TransactionScope
  tras la reestructuracion): POST real -- 200, `ventaId=1733`, confirmado en SQL Server directo.
- Regresion completa (10 rutas de Ventas/Stock/Productos/Cajas/Finanzas) en ambos motores,
  limpia, sin excepciones ni caidas de IIS Express.

**Datos de prueba dejados en Postgres, documentados** (sin via real de la app para
eliminar ventas -- mismo criterio que la compra de prueba de la entrada anterior):
`ventas.idventa` 1737 y 1738 (observaciones `TEST_DUAL_MODE_VENTA_3` /
`TEST_DUAL_MODE_VENTA_TARJETA`), mas su linea de venta y egreso de caja asociados.

**Estado final**: `DataEngine=SqlServer`. `CarniSys.sln` compila limpio. Con esto, `Compra` y
`Venta` quedan con escritura real verificada de punta a punta en ambos motores, con las
garantias de concurrencia ya confirmadas en la entrada anterior (el `IUnitOfWork` reusa
exactamente el mismo patron de conexion+transaccion explicita que esos tests validaron).

## 2026-08-20 - Verificacion de concurrencia real: aislamiento por tenant confirmado con evidencia (no solo lectura de docs)

Pregunta del usuario, antes de seguir con el fix de `Venta`: ¿los mecanismos de aislamiento (RLS + `set_config('app.id_empresa', ..., true)` + pool de conexiones Npgsql) soportan de verdad múltiples conexiones simultáneas -- mismo usuario desde 2 terminales, varios usuarios del mismo tenant, y varios tenants en paralelo -- sin mezclar datos? Toda la verificación de esta sesión hasta ahora fue **secuencial** (un curl a la vez) -- pregunta legítima, sin responder todavía con evidencia real.

**3 tests reales corridos, todos con resultado correcto:**

1. **Reset de `SET LOCAL` al terminar la transacción** (con el rol real de la app, `carnisys_user`, sujeto a RLS): dentro de una transacción con `app.id_empresa='1'` seteado, `corte` devuelve 57 filas (correcto). Fuera de esa transacción, en la **misma sesión/conexión física** -- sin volver a setear nada --, `current_setting('app.id_empresa', true)` da vacío, y cualquier query que dependa de convertirlo a entero **falla con un error duro** (`la sintaxis de entrada no es válida para tipo integer`), no devuelve datos de otro tenant ni de forma silenciosa. Confirma que una conexión reciclada por el pool nunca puede heredar el tenant de un uso anterior.

2. **2 conexiones Postgres genuinamente concurrentes, tenants distintos, superpuestas en el tiempo**: conexión A (tenant 1) abre transacción y duerme 3 segundos a mitad de camino; conexión B (tenant 2) arranca 1 segundo después, **mientras A todavía está "adentro"**. Resultado: A ve 57 filas (su propio `corte`), B ve 8 filas (el suyo) -- cero mezcla, verificado con los dos procesos corriendo en paralelo de verdad (`psql &`, no secuencial).

3. **8 escrituras HTTP reales, genuinamente simultáneas, mismo tenant/sesión** (simulando "el mismo usuario desde varias terminales" o varios usuarios del mismo tenant escribiendo a la vez): 8 POSTs concurrentes a `/Cajas/GuardarTipoEgresoCaja` bajo Postgres -- las 8 devolvieron éxito, las 8 quedaron en la base con IDs únicos sin colisión ni pérdida (304-311), limpiado por la vía real (delete real, no SQL manual).

**Conclusión, con alcance explícito**: el mecanismo de aislamiento (RLS + `SET LOCAL` transaccional) es seguro bajo concurrencia real para **todo el código que pasa por una transacción explícita** -- que es el 100% de lo verificado en las 10 etapas de cableado del modo dual más los 4 bugs recién cerrados de `Compra`. **No cubre** el camino roto de `Venta` (`TransactionScope` ambiente sin transacción explícita, ver entrada anterior) -- ese sigue sin verificar para concurrencia porque todavía ni siquiera funciona en el caso secuencial simple. La Opción 2 (conexión+transacción explícita compartida) hereda automáticamente estas garantías de concurrencia ya verificadas, porque vuelve a usar el mismo patrón de transacción explícita que estos 3 tests confirmaron seguro.

## 2026-08-20 - Testeo profundo de escritura real: Compra funciona en Postgres, Venta bloqueada por un problema de fondo con TransactionScope+RLS

Pedido del usuario: probar `Compra` y `Venta` con una escritura real vía HTTP en modo Postgres (deuda explícita dejada en las etapas de `StockController`/`VentasController`). Se encontraron y corrigieron **4 bugs de arquitectura reales**, y quedó **1 problema de fondo sin resolver**, documentado abajo.

### Bugs encontrados y corregidos

1. **`Negocio.Compra` (constructor Postgres) tenía 6 dependencias internas hardcodeadas a SQL Server** (`Corte`, `Sucursal`, `Usuario`, `CierreCaja`, `Persona`, `CuentaCorriente`) -- mismo patrón que el gap de `Negocio.Usuario` cerrado en la etapa de `LoginController`, pero mucho más extendido. Corregido con 6 parámetros opcionales nuevos en el constructor (default null = comportamiento de siempre), `NegocioFactory.CrearCompra` ahora los pasa ya cableados a Postgres reutilizando los `Crear*` que ya existían.

2. **`ConexionPg.AbrirConTenant` y `DbPg.cs` no eran compatibles con `TransactionScope` ambiente** (usado por `Compra`/`Venta`/`CuentaCorriente`, pensado para SQL Server): cada llamada abría su propia conexión Npgsql **y** su propia transacción explícita, chocando con el auto-enlistment de Npgsql en la transacción ambiente -- error real reproducido: `"A transaction is already in progress; nested/concurrent transactions aren't supported"`. Corregido: `AbrirConTenant` detecta `System.Transactions.Transaction.Current` y solo abre transacción explícita si NO hay una ambiente. `DbPg.cs` y los 8 archivos `DatosPostgres/*.cs` que llaman `tx.Commit()`/`tx.Rollback()` directo se corrigieron a `tx?.Commit()`/`tx?.Rollback()` (43 ocurrencias) -- sin esto, con `tx=null` tiraba `NullReferenceException`.

3. **`Negocio.CierreCaja.validarCajaAbiertaVendedor` creaba una instancia nueva de sí misma** (`new Negocio.CierreCaja(_empresa)`, siempre SQL Server) en vez de usar `this` -- mismo patrón de dependencia hardcodeada, encontrado al intentar probar `Venta` (la caja abierta se validaba contra el motor equivocado). Corregido usando `this.findByIdOrLast(...)` directo. Barrido del mismo anti-patrón (`new Negocio.<ClasePropia>(...)` dentro de la propia clase) en el resto de `Negocio/*.cs`: sin otras instancias.

4. **Bug introducido y corregido en el mismo commit**: al cablear las 3 dependencias internas de `Venta` (`CuentaCorriente`, `CierreCaja`, `Persona`, mismo patrón que Compra), agregar `new Negocio.CierreCaja(empresa)` como default eager en el constructor plano de `Venta` creó un ciclo real -- `Negocio.CierreCaja` ya construye su propio `Negocio.Venta` en el constructor (existente desde antes, sin relación con esta sesión), así que `Venta → CierreCaja → Venta → ...` es recursión infinita. Resultado: `StackOverflowException`, que termina el proceso de IIS Express sin poder capturarse (**tiró abajo IIS Express 2 veces** durante la verificación). Corregido: las 3 dependencias de `Venta` quedan `null` por default y cada uno de los 3 métodos que las usa construye la suya al vuelo si no fue inyectada -- mismo comportamiento exacto que tenía el código antes de tocarlo.

### Verificado con una escritura real

**`Compra` (Ingreso Stock) -- funciona end-to-end en Postgres**: POST real a `/Stock/Guardar` (formulario reconstruido desde el servido real), `TipoCompra=Ingreso Stock`, 1 línea de producto. Resultado: 302 (éxito), confirmado por SQL directo que la compra (`idcompra=9033`) y su línea (`corteporcompra`) quedaron en Postgres; SQL Server siguió en 0 para esa observación. `crearMovCtaCteCompra` correctamente NO generó movimiento de cuenta corriente (`AddOrEditCompra` fija `EnCtaCte=false` para movimientos de stock simples -- comportamiento esperado, no un gap). El dato de prueba queda en Postgres, sin vía real de la app para eliminarlo (`StockController` no tiene acción de eliminar/anular) -- decisión del usuario: dejarlo documentado, no borrar por SQL directo.

### Problema de fondo sin resolver: `Venta` sigue bloqueada

Con los 4 bugs de arriba corregidos, un POST real a `/Ventas/FinalizarVenta` (venta simple, forma de pago Efectivo, 1 línea) **ya no crashea ni tira el error de transacción anidada**, pero falla con un error distinto: `"42501: el nuevo registro viola la política de seguridad de registros para la tabla «ventas»"` -- RLS de Postgres rechazando el INSERT.

**Causa probable**: el fix de `ConexionPg.AbrirConTenant` (que evita abrir una transacción explícita propia cuando hay un `TransactionScope` ambiente, confiando en el auto-enlistment de Npgsql) no está preservando `app.id_empresa` de forma confiable entre los múltiples `AbrirConTenant` de una sola operación de venta -- cada uno abre una conexión nueva, y si Npgsql no la enlista de verdad en la transacción ambiente (o el auto-enlistment no cubre bien el patrón `SET LOCAL` vía `set_config`), el contexto de tenant se pierde antes del INSERT real. Sin dato huérfano: verificado por SQL directo que no quedó ninguna fila parcial en `ventas`.

**Diagnóstico, no una decisión tomada todavía**: la opción probada (auto-enlistment ambiente, "Opción 1" del análisis original) resolvió el choque de transacciones pero no garantiza que el aislamiento por tenant (RLS) se mantenga correcto a través de múltiples conexiones dentro de una misma operación -- necesita la alternativa más robusta ("Opción 2": una única conexión+transacción explícita compartida a través de toda la cadena de llamadas, sin depender del comportamiento de `TransactionScope`+Npgsql). Es un cambio más grande, que toca varios métodos de `VentaPg`/`CuentaCorrientePg`/`CierreCajaPg`. Queda pendiente, a definir con el usuario antes de continuar.

**Estado dejado**: `DataEngine=SqlServer` (confirmado). `CarniSys.sln` compila limpio. Regresión completa en `SqlServer` sobre `/Home`, `/Ventas/Index`, `/Ventas/POS`, `/Stock/Index`, `/Productos/Index`, `/Cajas/CajasAbiertas` -- todas 200, sin excepciones, confirmando que ninguno de los 4 fixes de arriba afectó el camino SQL Server (todos son aditivos/con fallback).

## 2026-08-20 - CuentaCorrientePg.obtenerPagos: alias corregidos, ultimo gap abierto cerrado

Pedido explícito del usuario: cerrar el único gap que seguía en la sección "Abiertos" de
`docs/GAPS.md`. Mismo bug de alias que los otros 5 métodos de esta clase (etapa
`CuentaCorriente`, `9dd132dc`) -- alias en minúsculas sin comillas (`razonsocial`,
`nrorecibo`, `aproveedor`, `operacion`, `formapago`, `creadopor`, `actualizadopor`) en vez de
los originales de SQL Server (`razonSocial`, `nroRecibo`, `aProveedor`, `Operacion`,
`formaPago`, `CreadoPor`, `ActualizadoPor`). Corregido citando los alias exactos entre comillas
dobles, mismo patrón que el resto de la clase.

**Nota sobre el caller real**: `obtenerPagos` no lo llama ningún controller de `Web/` --
su único caller es `Presentacion/Pagos/formPagos.cs` (WinForms), que nunca toca Postgres por
diseño. El fix es por consistencia/fidelidad con el resto de la clase ya cerrada, no porque
haya un riesgo activo hoy.

**Verificado**: `CarniSys.sln` compila limpio. Sin caller real en Web, se verificó con `psql`
directo (`SELECT` completo con `set_config('app.id_empresa', '1', false)` simulando el
contexto de tenant) -- los 14 alias devueltos coinciden exactamente con los de SQL Server.

## 2026-08-20 - GAPS.md: 3 entradas movidas a "fuera de alcance"

Decisión del usuario: no se migran datos a `ServidorSM`/`San Lorenzo` hasta que lo pida
explícitamente -- ninguno de los 3 gaps que dependían de esa decisión (confirmar
`buscarProveedor` contra producción real, o esperar un caller real en `StockController` para
`obtenerProveedores`/topología legacy de `SucursalPg`) tiene una acción disponible hoy. Se
movieron de "Abiertos" a "Fuera de alcance" en `docs/GAPS.md`, con la razón documentada en cada
entrada, en vez de implementarlos sin caller real (violaría §2.7, no inventar) o borrarlos sin
dejar rastro. Queda solo 1 entrada en "Abiertos": `CuentaCorrientePg.obtenerPagos` (alias sin
verificar, sin caller todavía).

## 2026-08-20 - Auditoria de production-readiness: 2 gaps de RLS cerrados, GAPS.md actualizado

Tras cerrar los 10 módulos del modo dual (`LoginController`, `328d1b55`), auditoría completa
pedida por el usuario para responder "¿está usable la migración a Postgres para largar a
producción?". Contexto clave aclarado por el usuario en esta etapa: **por ahora no se migran
datos de `ServidorSM`/`San Lorenzo` (SQL Server, producción real) -- eso queda para más
adelante**. El foco actual es que la base `carnisys` de Postgres local quede completamente
correcta y equivalente a SQL Server, porque **esa base (multi-tenant) es la que eventualmente
se lanza a producción**, no un simple espejo de una sola empresa.

**Hallazgo de la auditoría, 2 gaps de RLS sin documentar**: `auditoriacambiosucursalcaja`
(tabla de auditoría, solo-escritura desde la app) y `catalogoglobalimportacionproductos`
(tracking de qué producto global importó cada empresa) tienen columna `idempresa` pero
**no tenían política RLS** -- rompía el principio de "aislamiento a nivel de fila desde el
día 1" del stack estándar. No eran una fuga activa (el código ya filtra por `_idEmpresa` a
mano en ambas, y todo query pasa por `ConexionPg.AbrirConTenant`, que ya fija
`app.id_empresa` en la sesión), pero quedaban sin el respaldo a nivel de base que sí tiene el
resto de las tablas multi-tenant.

**Fix**: mismo patrón de política ya usado en el resto de la base (`<tabla>_rls`, `USING`
sobre `current_setting('app.id_empresa', true)` con fallback a `idempresa = 0`, `WITH CHECK`
exigiendo `idempresa = current_setting(...)`). Aplicado con el rol dueño de las tablas
(`carnisys_admin`), no con el rol de bypass RLS. Verificado que el rol real de la app
(`carnisys_user`, el que usa `ConexionPostgresPiloto`) **no es superusuario, no tiene
`BYPASSRLS` y no es dueño de ninguna tabla** -- confirma que el aislamiento por RLS en toda
la base es real, no cosmético.

**`docs/GAPS.md` corregido**: decía "sin gaps abiertos" desde la Etapa 8 (2026-08-18), lo cual
ya no reflejaba el código -- corregido con el inventario real de 4 `TODO(claude)` vigentes
(`PersonaPg.buscarProveedor`/`obtenerProveedores`, `SucursalPg` x2 de topología legacy,
`CuentaCorrientePg.obtenerPagos` con alias sin verificar) más 1 ítem documentado como
explícitamente fuera de alcance (`CuentaCorrientePg.eliminarPago`, SP inexistente en SQL
Server, solo alcanzable desde WinForms).

**Verificado**: regresión HTTP completa con `DataEngine=Postgres` sobre las 2 rutas que tocan
las tablas modificadas (`/Productos/Index`, `/Productos/VerGlobales`,
`/Productos/VerGlobalesTiposProducto`, que ejercitan lectura de
`catalogoglobalimportacionproductos`) -- 200 limpio, sin cambio de comportamiento (esperado,
ya filtraban bien a mano). Regresión final en `SqlServer`, limpia.

**Pendientes reales para un cutover de producción, listados sin resolver en esta etapa** (ver
respuesta completa al usuario, no repetida acá para no duplicar): falta mecanismo de
sincronización de datos SQL Server → Postgres, falta infraestructura de red entre
`ServidorSM`/`San Lorenzo` y un Postgres accesible, cero tests automatizados en toda la
solución, no existe `docs/RUNBOOK.md` con el procedimiento del toggle `DataEngine` ni de
rollback, `Compra`/`Venta` nunca se probaron con una escritura real vía HTTP en modo dual. El
`RUNBOOK.md` queda en pausa por decisión del usuario -- no es prioridad mientras no se planee
el cutover real de `ServidorSM`/`San Lorenzo`.

## 2026-08-20 - Modo dual: LoginController cableado (ultimo modulo pendiente) + fix real en Negocio/Usuario.cs (gap de Sucursal/Empresa hardcodeado a SQL Server)

Continuación y cierre de la serie de wiring iniciada en el piloto `352f7537` (hasta `VentasController` `c0022081`). `LoginController` -- el controller de mayor riesgo del plan original, toca cada login de la app -- tenía 5 bloques repetidos en distintas acciones (`OnActionExecuting`, `Index` POST tras validar contraseña, `ChangePassword`, `CambiarSucursal`) más `ForgotPassword`/`ValidarUbicacion` (usan `Parametros`/`DispositivoSeguro`), todos cambiados a `NegocioFactory.Crear*`.

**Gap real encontrado ANTES de cablear, no en el controller sino en `Negocio/Usuario.cs`**: 3 métodos (`validarUsuario`, `ValidarUsuarioWeb` -- el que valida la contraseña en el login real --, `ObtenerUsuarioPorIdentificador` -- el lookup pre-autenticación) hacían `new Datos.Sucursal(_empresa)` **hardcodeado a SQL Server**, sin importar si `oUsuarioD` (el repo inyectado) era Postgres. Ningún controller cableado hasta ahora llamaba estos 3 métodos de verdad -- en los 5 controllers previos `Usuario` estaba wireado pero sin caller real (`oUsuarioN` sin uso), así que el gap nunca se había manifestado. Con `LoginController` -- el único caller real de estos 3 métodos -- se hubiera manifestado como: login exitoso (credenciales validadas contra el motor correcto) pero `user.Sucursal`/`user.Empresa` **siempre resueltos contra SQL Server**, incluso en modo Postgres. Invisible hoy porque los datos de la empresa piloto están espejados en ambas bases con el mismo `idEmpresa`/`idSucursal`, pero rompe el aislamiento real que el resto del modo dual sí garantiza.

**Fix, confirmado con el usuario antes de tocarlo (`AskUserQuestion`)**: mismo patrón ya usado para `CortePuntoStockSucursal` en `Negocio/Corte.cs` (etapa `CortePuntoStockSucursal`, `ef0b37b3`) -- 4to parámetro opcional `Contratos.ISucursalRepository sucursalRepositorio = null` en el constructor Postgres-capaz de `Negocio.Usuario`, con un helper `ObtenerSucursalRepo()` que devuelve el repo inyectado o cae a `Datos.Sucursal(_empresa)` si es null (preserva el comportamiento de siempre para cualquier caller que no lo pase). `NegocioFactory.CrearUsuario` ahora pasa el mismo `SucursalPg` que ya construía para `UsuarioPg` también como este 4to parámetro. `MigracionPostgresController` (herramienta de comparación) no se tocó -- no llama a ninguno de los 3 métodos afectados, sin riesgo.

**Verificado con especial cuidado, por tratarse de autenticación**: `CarniSys.sln` completo compila limpio. Login real end-to-end en `SqlServer` y en `Postgres` -- ambos devuelven 302 a `/Home`, y el nombre de sucursal renderizado (`lblSucursalActual`) es **correcto en ambos motores** ("San Lorenzo"), confirmando que el fix resuelve `Sucursal`/`Empresa` contra el motor correcto y no cae silenciosamente a SQL Server. Contraseña incorrecta probada en `Postgres`: rechazada (200, se queda en la pantalla de login), sin excepción. **Escritura real**: `/Login/CambiarSucursal` (cambia la sucursal asignada al usuario logueado) probado en `Postgres` -- cambio de sucursal 2 ("San Lorenzo") a 1 ("San Martin"), confirmado por SQL directo que **solo Postgres cambió** (`idsucursaluser`: Postgres pasó a 1, SQL Server siguió en 2), restaurado por el mismo endpoint real. `/Login/ChangePassword` (GET) también probado en `Postgres`, sin errores. Regresión final en `SqlServer`: login real + rutas clave de todas las etapas anteriores (`Ventas`, `Stock`, `Productos`, `Cajas`, `Finanzas`), todas 200, sucursal correcta.

**Cierre de la serie**: con esta etapa, los 10 módulos identificados en el plan original de modo dual están cableados (`Empresa`, `DispositivoSeguro`, `Parametros`, `Sucursal`, `Persona`, `CuentaCorriente`, `CierreCaja`, `Corte`+`CortePuntoStockSucursal`+`Compra` vía `ProductosController`/`StockController`, `Venta` vía `VentasController`, `Usuario` vía `LoginController`). Quedan sin wirear a ningún controller real: `OtrasClases` (sin caller en ningún controller de `Web/`) y `CatalogoGlobalProducto` (solo el helper de `ProductosController`, ya cableado). `WhatsApp.cs` sigue excluido por decisión del usuario (feature no implementada).

## 2026-08-20 - Modo dual: VentasController cableado (6 clases, incluye Venta por primera vez)

Continuación de la serie (hasta `StockController` `ff4a600b`). `VentasController` (POS/ventas -- el controller de mayor tráfico de la app): 6 call sites en `OnActionExecuting` (`Venta`, `Sucursal`, `Usuario`, `Persona`, `Corte` cambiados a `NegocioFactory.Crear*`; `CierreCaja` ya estaba cableado desde la etapa de `CierreCaja`). Primera vez que `Venta` pasa por la factory con tráfico real -- `VentaPg.cs` no tenía ningún `TODO(claude)` pendiente, cerrado sin deuda desde su propia etapa de migración.

**Chequeo de riesgo antes de cablear**: revisados los métodos reales que `VentasController` llama sobre `oCorteN` (ninguno de los 6 SQL-Server-only) y sobre `oPersonaN` (`findById`, `getConsumidorFinal` -- ambos ya migrados, `getConsumidorFinal` es un método compuesto de `Negocio.Persona` que solo llama a `findById` internamente, sin SQL propio). `oUsuarioN` no tiene ningún caller real en este controller (mismo patrón "campo cableado sin uso" que en `ProductosController`/`StockController`).

**Verificación de escritura, misma nota honesta que la etapa anterior**: no se hizo una venta real de prueba -- `FinalizarVenta` tiene aún más validación de negocio que una compra (descuento de stock, vínculo con caja abierta, medios de pago), y fabricar un POST sintético fiel sería más riesgoso que informativo. En su lugar, verificación de lectura reforzada: diff completo (no solo status 200) de `/Ventas/Index`, `/Ventas/Facturas`, `/Ventas/Lineas`, `/Ventas/MisVentas` entre ambos motores -- **contenido de grilla idéntico byte a byte** en las 4 rutas, más `/Ventas/POS` (pantalla del punto de venta) sin errores en ningún motor. El path de escritura de `VentaPg` queda respaldado por la verificación de su propia etapa de migración, no por una prueba fresca en este commit -- misma deuda explícita que `CompraPg`.

**Verificado**: `CarniSys.sln` completo compila limpio. HTTP end-to-end con login real: regresión en `SqlServer` y `Postgres` sobre las 5 rutas -- 200 limpio en ambos motores, grillas idénticas. Regresión final en `SqlServer` (+ rutas de `Stock`/`Productos` de etapas anteriores), limpia.

## 2026-08-20 - Modo dual: StockController cableado (5 clases, incluye Compra por primera vez)

Continuación de la serie (hasta `ProductosController` `7794d967`). `StockController` ("Stock"/existencias, pesajes, ajustes): 5 call sites en `OnActionExecuting` (`Compra`, `Sucursal`, `Usuario`, `Corte`, `Persona`) cambiados a `NegocioFactory.Crear*`. Primera vez que `Compra` pasa por la factory con tráfico real (antes solo se había verificado con el harness `psql` de su propia etapa de migración).

**Riesgo aceptado, documentado antes de cablear (decisión del usuario vía `AskUserQuestion`)**: `StockController.ObtenerProveedoresExistencia()` llama a `oPersonaN.buscarProveedor("")`, uno de los 2 métodos de `PersonaPg` que seguían sin implementar. A diferencia de los gaps anteriores (SQL traducible), `buscarProveedor` en SQL Server es un **stored procedure** (`EXEC buscarProveedor`) que **no existe en la base local de dev** (confirmado con `sp_helptext`) -- sin definición real para traducir, así que no se inventó una. El único caller ya envuelve la llamada en `try/catch` y devuelve lista vacía ante cualquier error -- **mismo comportamiento degradado que SqlServer ya tiene hoy en esta base local**, así que cablear no introduce una regresión nueva, sólo hereda la misma limitación a ambos motores. Si en el futuro se confirma que el SP existe en `ServidorSM`/`San Lorenzo` (bases de producción reales), traducirlo desde ahí, no desde una suposición.

**Un cuarto caso del mismo bug mecánico** (`42P08`, ya visto 3 veces esta sesión en `VentaPg`, `CuentaCorrientePg`, `CortePg` x2): `CortePg.obtenerTiposProductoGrilla` (usada por `StockController` línea 1352, hermana de `obtenerTiposProductoGrillaEmpresa` ya corregida en la etapa anterior) tenía el mismo `@buscar IS NULL OR ILIKE @buscar` sin cast. Corregido igual (`@buscar::text`).

**Límite de cobertura conocido, ejercitado por primera vez con tráfico real**: `StockController` llama `oCorteN.CierreStock(...)` (en `ObtenerProductosNoCargadosCierre`, ruta `ProductosNoCargadosCierre`) -- uno de los 6 métodos de `Corte` sin equivalente Postgres (cobertura parcial documentada desde el diseño original del modo dual). Sigue golpeando SQL Server siempre, sin importar `DataEngine`. No probado explícitamente en esta etapa (requiere `idSucursal`/`fechaCompra` reales de un flujo de cierre en curso) -- comportamiento ya aceptado de antemano, no es un gap nuevo.

**Verificación de escritura, nota honesta**: a diferencia de las etapas anteriores, **no se hizo una escritura real de prueba** para `Compra` -- construir un POST válido a `/Stock/Guardar` requiere pasar las validaciones de negocio de una compra/pesaje real (proveedor, líneas de corte, sucursal, tipo), con riesgo de dejar un registro corrupto en una tabla financiera si el POST no es 100% fiel. En su lugar, se reforzó la verificación de lectura: diff completo (no solo status 200) de `/Stock/Index`, `/Stock/Lineas` y `/Stock/ExistenciaPorSucursales` entre ambos motores -- **contenido de grilla idéntico byte a byte** en las 3 rutas. El path de escritura de `CompraPg` queda respaldado por la verificación de su propia etapa de migración (harness `psql`, sin `TODO(claude)` pendiente), no por una prueba fresca en este commit. Queda como deuda explícita si se quiere una prueba de escritura real más adelante.

**Verificado**: `CarniSys.sln` completo compila limpio. HTTP end-to-end con login real: regresión en `SqlServer` y `Postgres` sobre `/Stock/Index`, `/Stock/Lineas`, `/Stock/ExistenciaPorSucursales` -- 200 limpio en ambos motores, contenido de grilla idéntico. Regresión final en `SqlServer` (+ rutas de `Productos` de la etapa anterior), limpia.

## 2026-08-20 - Modo dual: ProductosController cableado (5 clases) + cierre de gaps reales en PersonaPg y CortePg (extensión unaccent)

Continuación de la serie (piloto `352f7537`, hasta `CierreCaja` `dea821cc`). `ProductosController` ("Productos"/catálogo de la empresa) es el primer controller con **5 clases Negocio distintas** en su `OnActionExecuting`: `Sucursal`, `Corte`, `Usuario`, `Persona`, `CortePuntoStockSucursal` -- las 5 cambiadas a sus `NegocioFactory.Crear*` correspondientes, más un sexto call site (`ObtenerGestorCatalogoGlobal`, un helper privado que arma `CatalogoGlobalProducto` con `EmpresaContextNulo` -- catálogo global no está scopeado a una empresa) cambiado igual.

**Nota de riesgo**: `Usuario` en este controller es una instancia local sin ningún caller real (`oUsuarioN` se asigna pero nunca se usa en `ProductosController`) -- wirearlo no tiene efecto funcional, es solo consistencia con el resto de la clase. Cablear `Usuario` de verdad (con riesgo real, ej. `LoginController`) sigue pendiente para una etapa futura aparte.

**Dos gaps reales encontrados al probar, confirmados con el usuario antes de tocarlos (AskUserQuestion)**:

1. **`PersonaPg.obtenerProveedoresConCompras()`** -- bloqueaba `/Productos/Index`. Traía un `TODO(claude)` desde el piloto original (Etapa 2, 2026-08-18) diciendo "requiere la tabla Compras migrada a Postgres, fuera de alcance" -- bloqueo ya obsoleto (`CompraPg` está migrada hace varias etapas, mismo patrón ya visto con `PersonaPg.buscarPersona`/`personaTieneCompras_Ventas` en la etapa de `PersonasController`). Implementado: `SELECT DISTINCT` con `JOIN` a `compras`/`personas`, sin complejidad de collation.
2. **`PersonaPg.existenMarcasParecidas()`** -- bloqueaba el chequeo de marcas duplicadas en `/Productos/Marcas`. Más complejo que el resto de esta clase: el original de SQL Server usa `COLLATE Latin1_General_CI_AI` (case- **y accent**-insensitive), a diferencia de todo lo demás en esta migración que solo necesita case-insensitive (`ILIKE` solo, acorde a la collation default `Modern_Spanish_CI_AS` de la base). Se instaló la extensión `unaccent` en la base Postgres local (`CREATE EXTENSION unaccent`, requirió el rol superusuario -- el rol de bypass RLS de uso diario no tiene privilegio `CREATE` sobre extensiones) y se tradujo a `unaccent(p.razonsocial) ILIKE unaccent(@texto)`. Verificado por separado con `psql` que `unaccent('Jamón') = unaccent('Jamon')` antes de dar la traducción por buena.

Los otros 2 métodos de `PersonaPg` que seguían sin implementar (`buscarProveedor`, `obtenerProveedores`) siguen así a propósito -- sin caller en ningún controller ya cableado (los usa `StockController`, todavía no cableado).

**Un tercer gap encontrado y corregido sin necesidad de preguntar (mismo bug mecánico ya visto 2 veces antes esta sesión, en `VentaPg` y `CuentaCorrientePg`)**: `CortePg.obtenerTiposProductoGrillaEmpresa` y `CortePg.obtenerTiposProductoCatalogoGlobal` (bloqueaba `/Productos/Tipos`) tenían el mismo patrón `@buscar IS NULL OR ... ILIKE @buscar` con el parámetro atado a `DBNull.Value` sin tipo -- Npgsql no puede inferir el tipo y tira `42P08`. Corregido con cast explícito (`@buscar::text`) en ambos métodos, mismo patrón que las dos veces anteriores.

**Hallazgo aparte, no relacionado al modo dual, no corregido (fuera de alcance)**: `/Productos/VerGlobales` y `/Productos/Tipos` mostraron un timeout intermitente (`Tiempo de espera de la operación de espera agotado`, ~35s) **en el motor SqlServer**, tanto antes como después de este cambio -- reproducido de forma aislada, mismo código sin tocar (`Datos/CatalogoGlobalProducto.ObtenerCatalogoGlobalPagina`). Causa real: la consulta arma un `ROW_NUMBER() OVER` con un `LEFT JOIN` de auto-referencia sobre las ~101.943 filas de `CatalogoGlobalProducto` **completas** en cada request (sin filtro cuando `busqueda=''`), y el plan en frío tarda más que el timeout default de comando (~30s); en caliente (plan cacheado) baja a <1s. No es una regresión de esta etapa -- mismo SQL, mismo call site que ya existía. Queda como límite de performance conocido, no corregido acá (fuera del alcance del pedido, §5); candidato a revisar aparte si se vuelve un problema recurrente en uso real.

**Verificado**: `CarniSys.sln` completo compila limpio. HTTP end-to-end con login real: regresión en `SqlServer` sobre `/Home`, `/Productos/Index`, `/Productos/VerGlobales`, `/Productos/Tipos`, `/Productos/Marcas` (+ rutas de la etapa anterior) -- 200 en las 8, con el timeout en frío ya documentado arriba resuelto por reintento. Con `Postgres`, mismas 4 rutas de `Productos`, 200 limpio (sin rastro de `NotImplementedException` ni `42P08`) tras cerrar los 3 gaps. Verificación directa por `psql` del comportamiento de `unaccent()` antes de confiar en la traducción. Regresión final en `SqlServer`, limpia.

## 2026-08-20 - Modo dual: séptimo módulo cableado (CierreCaja, 5 controllers)

Continuación de la serie (piloto `352f7537`, y los 6 anteriores hasta `9dd132dc`). `CierreCaja` se usa en 5 controllers reales, cada uno con exactamente un call site en `OnActionExecuting`: `CajasController` (home natural del módulo -- cierres de caja, egresos de caja, tipos de egreso, cambio de sucursal de caja), `ComprasController`, `FinanzasController`, `ReportesController` y `VentasController` (el de mayor tráfico de los cinco). Los 5 call sites cambiados a `NegocioFactory.CrearCierreCaja(empresa)` / `NegocioFactory.CrearCierreCaja(empresa, param)` según la firma que ya tenía cada uno.

**Decisión de alcance**: se cablearon los 5 de una sola vez, no de a uno como en etapas con gaps reales (`Persona`, `CuentaCorriente`). Razón: a diferencia de esos dos casos, `CierreCajaPg.cs` **no traía ningún `TODO(claude)` ni advertencia pendiente** de etapas anteriores -- ya había sido cerrado y verificado sin deuda conocida. Con la clase 100% cubierta y sin cobertura parcial (a diferencia de `Compra`/`Corte`), cablear los 5 controllers que la usan es el mismo caso que `CuentaCorriente` (3 controllers, mismo criterio), solo que con más call sites.

**Verificado**: `CarniSys.sln` completo compila limpio (solo warnings preexistentes de `Presentacion`, sin relación). HTTP end-to-end con login real (credenciales de prueba en `~/hosts/carnisys-web-local.env`): regresión en `SqlServer` sobre `/Home`, `/Cajas/CajasAbiertas`, `/Cajas/TiposEgresoCaja`, `/Compras/Index`, `/Finanzas/CtasCtes`, `/Finanzas/Cheques`, `/Reportes/Index`, `/Ventas/Index` -- las 8 rutas devolvieron 200 sin contenido de error. Con `Postgres`, mismas 8 rutas, mismo resultado. **Escritura real**: alta de un tipo de egreso de caja de prueba (`/Cajas/GuardarTipoEgresoCaja`, POST reconstruido desde el form real servido) con `DataEngine=Postgres`, confirmado por SQL directo que el registro apareció **solo en Postgres** (`tiposegresocaja`, id 304) y que SQL Server siguió en 0 filas para ese valor; restaurado por la vía real (`/Cajas/EliminarTipoEgresoCaja`), confirmado por SQL directo que quedó en 0 filas en ambos motores. Regresión final en `SqlServer` sobre las mismas 8 rutas, sin rastro del dato de prueba.

**Nota operativa**: los archivos `.env` de `~/hosts/` no son shell-safe para `source` directo (algunos valores rompen el parseo de bash por caracteres especiales en `NOTES`) -- se extraen con `grep '^VAR=' archivo.env | cut -d'=' -f2- | tr -d '\r'` en vez de `source`, más robusto contra ese formato simple de "una variable por línea" que no es sintaxis de shell.

## 2026-08-19 - Modo dual: sexto módulo cableado (CuentaCorriente, 3 controllers) + cierre de gaps reales en CuentaCorrientePg

Continuación de la serie (piloto `352f7537`, y los 5 anteriores hasta `9b93a626`). `CuentaCorriente` se usa en 3 controllers reales (`HomeController` -- dashboard, `FinanzasController` -- CtasCtes/Cheques, `ReportesController`), 3 call sites cambiados a `NegocioFactory.CrearCuentaCorriente`.

**Hallazgo real al probar, más grande que los anteriores**: `CuentaCorrientePg.cs` traía su propio `TODO(claude)` desde el piloto original (Etapa 5) advirtiendo que 6 métodos con `DataTable` crudo tenían alias en minúsculas/snake_case **sin verificar** contra los nombres reales de `Datos/CuentaCorriente.cs` (que usa alias con espacios y mayúsculas, ej. `[Nombre Identif.]`, `[Razon Social]`, `[obs.]`) -- el propio comentario decía "si en el futuro se conecta una View real hay que re-verificar los alias antes de usarlos". Justo eso pasó al cablear estos 3 controllers. Confirmado con el usuario, se revisaron y corrigieron los 5 métodos que sí tienen caller en los controllers ya cableados (`obtenerCtasCtes`, `obtenerResumenDashboard`, `obtenerCheques`, `obtenerTotalesPagosBalance`, `obtenerUltimosPagosDashboard`) -- `obtenerChequesPendientesDashboard` no necesitó cambios (sin alias multi-palabra en el original) y `obtenerPagos` queda sin verificar (sin caller todavía).

**Dos bugs reales corregidos**:
1. **Alias de columna**: se citaron los alias exactos entre comillas dobles de Postgres (`AS "Nombre Identif."`, `AS "Razon Social"`, `AS "Recibido_De"`, `AS "Entregado_A"`, `AS "obs."`, etc.), byte a byte iguales al original -- necesario porque algunos callers (`FinanzasController.GetCheques`) leen la fila por indexer directo (`row["Recibido_De"]`), sin fallback, así que un alias que no matchea exactamente tira una excepción, no un valor vacío.
2. **Tipo de parámetro Npgsql sin inferir**: `obtenerCtasCtes` recibe `idPersona` como `int?`; al ser `null`, `AddWithValue(..., DBNull.Value)` no le da a Npgsql ningún tipo para inferir, y la consulta fallaba con `42P08: no se pudo determinar el tipo del parámetro $1`. Mismo patrón ya resuelto antes esta sesión (Etapa 12b, `VentaPg`) -- se cast explícito (`@idPersona::int`) en cada uso dentro del SQL.

**Nota, no un bug**: el campo `Propio` de `GetCheques` (JSON) se serializa como `"1"`/`"0"` en SQL Server (`bit`) vs `"True"`/`"False"` en Postgres (`boolean`) -- mismo tipo de diferencia de representación ya documentada varias veces esta sesión (`.ToString()` de tipos distintos). Sin uso real en las Views (`Propio` no se lee en ningún lado del lado cliente), no se corrige.

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` directo (rol real): `obtenerCtasCtes` con el cast explícito y parámetro nulo. HTTP end-to-end con login real: regresión en `SqlServer` (3 controllers); con `Postgres`, `/Finanzas/CtasCtes` **idéntico celda a celda** contra SQL Server, `/Home` (dashboard) idéntico, y `/Finanzas/GetCheques` idéntico salvo la representación de `Propio` ya explicada. Regresión final en `SqlServer`.

## 2026-08-19 - Modo dual: quinto controller cableado (PersonasController) + cierre de 2 gaps reales en PersonaPg

Continuación de la serie (piloto `352f7537`, `DispositivosSegurosController` `ed7685e3`, `ParametrosController` `710c390d`, `SucursalController` `bf694ef5`). `PersonasController` ("Personas"/proveedores/clientes): 3 call sites (`oPersonaN` + 2 `Negocio.Sucursal` embebidos), cambiados a `NegocioFactory.CrearPersona`/`CrearSucursal`.

**Hallazgo real al probar, no un bug de esta etapa**: `PersonaPg.buscarPersona` y `personaTieneCompras_Ventas` estaban sin implementar (`throw NotImplementedException("TODO(claude)...")`) desde una sesión anterior (piloto de una sola tabla, Etapa 2, 2026-08-18) -- en ese momento `Compras`/`Ventas` no existían en Postgres y quedó pendiente resolver `LIKE` case-insensitive. **Ambos bloqueos ya no aplican**: `CompraPg`/`VentaPg` están migradas desde hace varias etapas, y el patrón `LIKE`→`ILIKE` ya se usó y verificó esta sesión en `CatalogoGlobalProductoPg`. Confirmado con el usuario, se implementaron los 2 métodos (los únicos que `PersonasController` usa realmente) -- los otros 4 (`buscarProveedor`, `obtenerProveedores`, `obtenerProveedoresConCompras`, `existenMarcasParecidas`) quedan sin implementar a propósito: no tienen caller en ningún controller ya cableado (`StockController`/`ProductosController` los usan, pero mezclan `Corte`, que tiene cobertura parcial -- no cableados todavía).

**Verificado que la collation es compatible antes de traducir**: la base tiene collation default `Modern_Spanish_CI_AS` (case-insensitive, **accent-sensitive**) -- `ILIKE` de Postgres es exactamente eso, sin necesidad de `unaccent` ni normalización extra. (`existenMarcasParecidas`, fuera de este alcance, usa `COLLATE ... CI_AI` -- accent-*insensitive* explícito, un caso distinto que si se implementa en el futuro sí necesitaría resolver ese matiz aparte, ej. con la extensión `unaccent`.)

**Bug real encontrado y corregido durante la verificación HTTP**: mi primera traducción de `buscarPersona` seleccionaba `i.iva` (nombre completo, "Consumidor Final") en vez de `i.abrev` (abreviatura, "Cons.Final") -- el original de SQL Server selecciona `abrev`. Corregido antes de cerrar la etapa; confirmado con diff completo de la grilla real (`/Personas/Index`) contra SQL Server.

**Nota, no un bug de esta etapa**: un registro de prueba (`JUANCITO PEREZ`, id=25) tiene un carácter suelto distinto entre SQL Server y Postgres en `identificacion` -- dato ya migrado en una etapa anterior a esta sesión, con un byte ambiguo/corrupto en el original; no se investiga más a fondo (no es un valor de negocio real).

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` directo: búsqueda case-insensitive y `personaTieneCompras_Ventas` contra datos reales. HTTP end-to-end con login real: regresión en `SqlServer`; con `Postgres`, `/Personas/Index` **idéntico celda a celda** contra SQL Server (salvo el carácter suelto ya explicado), `/Personas/Editar/23` idéntico, y **escritura real** (`/Personas/Guardar` cambiando `Telefono`) confirmando aislamiento por SQL directo y restaurada por la misma vía. Regresión final en `SqlServer`.

**Nota operativa**: IIS Express volvió a caerse dos veces durante esta etapa (mismo patrón de inestabilidad ya documentado en etapas anteriores) -- resuelto reiniciando el proceso, sin relación con el código.

## 2026-08-19 - Modo dual: cuarto controller cableado (SucursalController)

Continuación de la serie (piloto `352f7537`, `DispositivosSegurosController` `ed7685e3`, `ParametrosController` `710c390d`). `SucursalController` ("Mis Sucursales"): un solo call site (`oSucursalN = new Negocio.Sucursal(empresa);`), cambiado a `NegocioFactory.CrearSucursal(empresa)`.

**Verificado con edición real de una sucursal**: con `DataEngine=Postgres`, se cargó `/Sucursal/Editar/2` (San Lorenzo), se posteó `/Sucursal/Guardar` cambiando `Direccion` a un valor de prueba, se confirmó por SQL directo que **solo Postgres cambió** (SQL Server siguió con la dirección real), y se restauró por el mismo camino real. Regresión con `DataEngine=SqlServer` antes y después (incluye el menú de sucursales que arma `BaseController` en cada request, ya ejercitado desde el piloto).

## 2026-08-19 - Modo dual: tercer controller cableado (ParametrosController)

Continuación de la serie (piloto `352f7537`, `DispositivosSegurosController` en `ed7685e3`). `ParametrosController` ("Parámetros" en Configuración): un solo call site (`oParametrosN = new Negocio.Parametros(empresa);`), cambiado a `NegocioFactory.CrearParametros(empresa)`.

**Verificado con escritura real de grilla completa**: a diferencia de `DispositivosSeguros` (alta/baja de una fila), `Parametros/Guardar` postea la grilla completa (15 parámetros visibles para el CUIT de prueba, mezcla de texto/decimal/booleano vía checkbox). Con `DataEngine=Postgres`, se reconstruyó el POST real completo (extrayendo todos los campos del HTML servido, preservando orden y tipos) cambiando un solo valor (`codProdGenerico`, `999999` → `777777`), confirmando por SQL directo que **solo Postgres cambió** (SQL Server siguió en `999999`), y se restauró el valor original por el mismo camino real. Regresión con `DataEngine=SqlServer` antes y después.

## 2026-08-19 - Modo dual: segundo controller cableado (DispositivosSegurosController)

Continuación de la etapa anterior (piloto `BaseController`+`EmpresaController`, commit `352f7537`). Siguiente controller de bajo riesgo, mismo criterio de a-uno-por-vez: `DispositivosSegurosController` ("Dispositivos seguros" en Configuración) -- un solo call site (`oDispositivoN = new Negocio.DispositivoSeguro(empresa);` en `OnActionExecuting`), cambiado a `NegocioFactory.CrearDispositivoSeguro(empresa)`.

**Verificado con datos reales de alta/baja, no solo lectura** (la tabla origen está vacía en ambos motores, así que la prueba significativa es el ciclo completo de escritura): con `DataEngine=Postgres`, se agregó un dispositivo de prueba vía `/DispositivosSeguros/Agregar` (POST real), se confirmó por SQL directo que **solo apareció en Postgres** (SQL Server siguió en 0 filas), se confirmó que la lista HTTP real lo mostraba, y se eliminó vía `/DispositivosSeguros/Eliminar` (POST real, no edición manual) -- Postgres quedó en 0 filas otra vez. Regresión con `DataEngine=SqlServer` antes y después, sin cambios de comportamiento.

**Nota operativa**: IIS Express se cayó solo entre la etapa anterior y esta (sin causa identificada, posiblemente por los múltiples recycles de `Web.config` al alternar el switch) -- se reinició sin drama, no relacionado con el código de esta etapa.

## 2026-08-19 - Modo dual SQL Server / PostgreSQL en trafico real (piloto: BaseController + EmpresaController)

Hasta acá, toda la migración a Postgres (14/15 clases `Negocio/*.cs`, `WhatsApp.cs` descartado) quedó construida y verificada en paralelo, pero **nunca conectada a tráfico real** -- todo controller de producto seguía usando el constructor SQL Server de siempre. El usuario pidió el siguiente paso: activar el modo dual en código real, con un parámetro que decida el motor, pensando en un cutover futuro por-deploy (hoy hay 2 bases de producción SQL Server single-tenant, `ServidorSM` y `San Lorenzo`, cada una un deploy físico separado; Postgres será la futura base multi-tenant).

**Decisiones de diseño, confirmadas con el usuario (3 preguntas, esta sesión)**:
1. **El switch es un `appSetting` en `Web.config`** (`DataEngine`, valores `SqlServer`/`Postgres`, default `SqlServer`), no una columna por-empresa. Cada deploy ya tiene su propio `Web.config` físicamente separado -- cambiar el motor de un servidor entero es editar una línea, sin tocar código ni recompilar.
2. **La lógica de armado de los objetos Postgres vive en `Web/Infrastructure/NegocioFactory.cs`** (archivo nuevo), no adentro de `Negocio/*.cs`. Con esto, `Negocio.dll` y WinForms (`Presentacion/`, `wsAFIPvs2008/`) quedan con **cero dependencia nueva de `DatosPostgres.dll`** -- por diseño, WinForms nunca puede terminar en modo Postgres, ni por accidente. Coherente con la regla ya vigente de este repo: "solo se trabaja en Web/, nunca WinForms".
3. **Alcance de esta etapa: un piloto de bajo riesgo**, no los ~20 controllers reales de una sola vez. Se cableó `BaseController` (que corre en *todo* request autenticado: arma `IParametrosContext` y el menú de sucursales) + `EmpresaController` ("Mi Empresa") -- 3 clases 100% migradas y sin ningún caso de cobertura parcial (`Parametros`, `Sucursal`, `Empresa`). El resto de los controllers reales quedan para etapas siguientes, un módulo por vez, mismo criterio del resto de esta migración.

**`NegocioFactory` tiene los 14 métodos completos** (`Crear<X>` por cada clase migrada), aunque el piloto solo cablea 3 -- para que la próxima etapa (cablear el siguiente controller real) no tenga que repetir el trabajo de reconstruir el grafo de dependencias (`PersonaPg` → `SucursalPg` → `CortePg` → `VentaPg`, etc., el mismo orden que ya usaba `MigracionPostgresController`).

**`Compra`/`Corte` en modo Postgres**: los métodos sin equivalente Postgres (`backup`/`restaurarBD` de `Compra`; `obtenerEmbutidos`, `reiniciarStockReal/Teorico`, `CierreStock`, `StockIngresoEgreso`, `TotalKgsCortePorCompra` de `Corte`) siguen golpeando SQL Server siempre, vía el mismo campo `oXDSqlServer` que ya existía -- limitación conocida y documentada, no un bug nuevo introducido por el toggle.

**Supuestos y límites explícitos (fuera de alcance de esta etapa)**:
- **Freshness de datos**: activar `DataEngine=Postgres` en un deploy asume que Postgres ya tiene los datos de esa empresa al día -- no hay ningún mecanismo de sincronización continua todavía. Los datos en Postgres son las fotos ya migradas durante esta sesión.
- **Solo funciona hoy en dev**: `ConexionPostgresPiloto` apunta a `localhost`. `ServidorSM`/`San Lorenzo` no tienen red hacia ningún Postgres -- no se aprovisionó infraestructura nueva en esta etapa.

**Convención para etapas futuras**: cada vez que se cablee un controller real nuevo, cambiar sus `new Negocio.X(...)` por `NegocioFactory.CrearX(...)` en el mismo commit (el método ya existe en la factory para las 14 clases migradas).

**Verificado -- primera vez que tráfico HTTP real (no `MigracionPostgresController`) sirve datos desde Postgres**:
- `CarniSys.sln` completo compila limpio.
- **Regresión con `DataEngine=SqlServer`** (default): login real + `/Empresa/Index` (datos de "Mi Empresa", horarios, menú de sucursales San Martín/San Lorenzo) -- idéntico a como era antes de este cambio.
- **Con `DataEngine=Postgres`**: mismo flujo completo contra Postgres real -- `/Empresa/Index` muestra los mismos datos (`SuperCerdo`, horarios, sucursales), y **`/Empresa/Guardar` (escritura real) se probó de punta a punta**: se cambió `Slogan2` a un valor de prueba vía POST real, se confirmó que **solo Postgres cambió** (`SELECT` directo a ambas bases: Postgres con el valor de prueba, SQL Server intacto) -- aislamiento entre motores funcionando como se diseñó -- y se restauró el valor original por el mismo camino (POST real, no edición manual).
- Vuelto a `DataEngine=SqlServer`: regresión final confirmada, nada roto.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: CatalogoGlobalProducto.cs completo

Quinto de los módulos chicos 0%-migrados (tras `Parametros.cs`, `CortePuntoStockSucursal.cs`, `Empresa.cs`, `DispositivoSeguro.cs`). 4/4 métodos públicos: `findCorteGlobalByCodigo`, `ObtenerCatalogoGlobalPagina`, `ObtenerTiposCatalogoGlobal`, `ObtenerCatalogoGlobalPorIds`. Catálogo global de productos (compartido entre todas las empresas, sin `idEmpresa`, sin RLS -- mismo criterio que `formularios`/`alicuotasiva`).

**El más grande en volumen de datos de toda esta ronda**: `dbo.CatalogoGlobalProducto` tiene **101.943 filas reales** (vs. decenas/cientos del resto). Confirmado con el usuario antes de arrancar, dado el cambio de escala respecto a los módulos anteriores. `idcorte` **no es identity** (valores fijos preasignados desde el catálogo origen) -- se replica igual en Postgres, sin autoincremental.

**Simplificación deliberada en `ObtenerCatalogoGlobalPorIds`**: el original batchea los ids de a 2000 (límite de parámetros de `SqlCommand` en SQL Server). En Postgres se usa `idcorte = ANY(@ids)` con un array nativo (mismo patrón ya usado en `CortePg`) -- sin el límite de SQL Server, no hace falta el batching. Mismo resultado, una sola consulta en vez de N.

**`LIKE` → `ILIKE`** en la búsqueda de texto (`ObtenerCatalogoGlobalPagina`): SQL Server usa `LIKE` case-insensitive por la collation de la base (sin `LOWER()`/`UPPER()` explícito en el código original). Postgres's `LIKE` es case-sensitive por defecto -- se tradujo a `ILIKE` para preservar el comportamiento real observado, no el literal del texto del operador. Verificado con búsqueda real (`yerba` en minúscula encuentra `YERBA LA MERCED...` en mayúsculas, mismo resultado en ambos motores).

**Hallazgo en la exportación de datos**: un nombre de producto real contiene una comilla doble literal (`"nestum® Listo Para Tomar...`), que rompe el parser CSV de Postgres (el formato CSV trata `"` como carácter de cita incluso con delimitador `|`). Se reemplazó por `'` en el export, mismo criterio ya usado para `|`/CR/LF en etapas anteriores -- pérdida de fidelidad mínima en un nombre de producto, no en ningún campo de negocio (`código`, `precio`, etc.).

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` (sin transacción, todo de solo lectura): los 4 métodos verificados directamente, incluida la búsqueda `ILIKE`. HTTP end-to-end con login real (`ger`/idEmpresa=1) contra la nueva acción `CompararCatalogoGlobal`: página 1 (20/20 filas, 160/160 celdas idénticas) y búsqueda por texto (`yerba`, 10/10 filas idénticas).

**Nota operativa, no un bug de esta etapa -- incidente real de infraestructura durante la verificación**: en medio de la prueba HTTP, la base `CarniSys` de SQL Server Express local quedó en estado `RECOVERY_PENDING` (motor no pudo completar la recuperación tras la contención de recursos ya documentada en etapas anteriores de esta sesión) -- ningún login podía abrirla, ni siquiera `sa`. Detenido todo intento de "arreglarlo" automáticamente (riesgo de pérdida de datos reales); se avisó al usuario y se esperó a que el servicio se recuperara (reinicio del servicio `MSSQL$SQLEXPRESS`, fuera del alcance de esta sesión por permisos) antes de continuar. Sin impacto en Postgres ni en el código migrado -- una vez la base volvió a responder, la verificación dio resultados idénticos sin cambios.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: DispositivoSeguro.cs completo

Cuarto de los módulos chicos 0%-migrados (tras `Parametros.cs`, `CortePuntoStockSucursal.cs`, `Empresa.cs`). 4/4 métodos: `Listar`, `Agregar`, `Eliminar`, `ExisteSerieSegura` -- dispositivos con número de serie que saltean `LoginRateLimiter` en el login. `dbo.DispositivosSeguros` está **vacía en SQL Server** (0 filas reales), sin datos que migrar.

**`dispositivosseguros` es la 3ra tabla de esta ronda con `idEmpresa` pero sin RLS en SQL Server** (mismo patrón ya encontrado en `empresaparametros` y `cortepuntostocksucursal`) -- se agrega RLS estándar en Postgres como mejora deliberada, mismo criterio ya confirmado con el usuario, sin volver a preguntar dado el precedente ya establecido dos veces en esta misma ronda de módulos.

**`ExisteSerieSegura` se usa en el login antes de autenticar** (para decidir si se saltea el rate limiter por IP) pero el `idEmpresa` ya se conoce en ese punto (resuelto del candidato por usuario/email) -- a diferencia de `usuarios` (Etapa 13a), acá no hay problema de "tenant todavía no conocido", así que RLS estándar no genera ningún conflicto.

**Verificación distinta al resto de la migración, por la tabla vacía**: sin datos reales para comparar, se armó una acción de self-test (`CompararDispositivoSeguro`) que ejercita `Agregar`→`ExisteSerieSegura`→`Listar`→`Eliminar` contra los 2 motores con un número de serie descartable generado en cada request, comparando que ambos den el mismo resultado (en vez de comparar contra datos preexistentes). Complementa el harness `psql` (rol real, transacción, `ROLLBACK`), que ya había verificado los 4 métodos por separado.

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql`: alta, `ExisteSerieSegura`, `Listar` (con `JOIN` a `usuarios`), baja -- sin residuo tras el rollback. HTTP end-to-end con login real (`ger`/idEmpresa=1): self-test `CompararDispositivoSeguro` da `False`/`False` antes de `Agregar` y `True`/`True` después, en ambos motores, sin residuo en ninguna de las 2 bases tras la request (confirmado con `COUNT(*)` directo post-request).

**Nota operativa, no un bug de esta etapa**: SQL Server Express local mostró un patrón de intermitencia más severo que lo habitual esta vez -- respondía, volvía a colgarse en segundos, varias veces seguidas. Se resolvió esperando confirmación de estabilidad sostenida (3 chequeos exitosos seguidos) antes de reintentar, en vez de reintentar contra una ventana de disponibilidad efímera.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Empresa.cs completo + fix de drift de schema en `empresas` (horario laboral)

Tercero de los módulos chicos 0%-migrados (tras `Parametros.cs`, `CortePuntoStockSucursal.cs`). 2/2 métodos: `findById`, `ActualizarDatosBasicos` (pantalla "Mi Empresa" -- edición de datos no-fiscales por el propio tenant, distinto del CRUD cross-tenant de `SystemAdministrationRepository`).

**Hallazgo real antes de migrar, confirmado con el usuario y resuelto en el mismo cambio**: la tabla `empresas` en Postgres ya existía desde la Etapa 3 (creada para `ISucursalRepository.findEmpresaById`), pero le faltaban las **4 columnas de horario laboral** (`HorarioDiurnoDesde/Hasta`, `HorarioTardeDesde/Hasta`) agregadas en SQL Server el 2026-08-14 (feature de restricciones de login) -- drift de schema entre las dos bases, sin detectar hasta ahora. Se agregaron con `ALTER TABLE` + se migraron los valores reales de las 7 filas existentes (incluida `idEmpresa=-1`, mismo patrón "template" ya visto en `EmpresaParametros`).

**Segundo hallazgo relacionado**: `SucursalPg.MapEmpresa` (ya en uso por el controller de comparación) tampoco mapeaba esas 4 columnas -- no podía, porque no existían. Corregido en el mismo cambio (los 2 `SELECT` de `findEmpresaById`/`findEmpresaByCuit` en `SucursalPg.cs` ahora incluyen las 4 columnas, y `MapEmpresa` las mapea con los mismos defaults que el original -- `00:00:00`/`23:59:59` si vinieran nulas).

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` (rol real, transacción explícita, `ROLLBACK`): `findById` y `ActualizarDatosBasicos` sobre una empresa real -- sin residuo. HTTP end-to-end con login real (`ger`/idEmpresa=1) contra la nueva acción `CompararEmpresa`: **14/14 celdas idénticas**, incluidos los horarios reales (`23:59:00`, valor distinto al default -- confirma que tanto la migración de datos como el fix de `SucursalPg.MapEmpresa` funcionan).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: CortePuntoStockSucursal.cs completo + cierre de gap en Corte.cs

Segundo de los módulos chicos 0%-migrados (tras `Parametros.cs`). 3/3 métodos: `CrearParaTodasLasSucursales`, `GuardarPuntosStockLote`, `FindPorSucursal` -- punto de stock por combinación Producto (Corte) x Sucursal.

**Hallazgo real antes de migrar, confirmado con el usuario y resuelto en el mismo cambio**: `Negocio/Corte.cs` (ya migrado a Postgres en la Etapa 11c) tenía `Datos.CortePuntoStockSucursal` **hardcodeado a SQL Server en sus 2 constructores**, incluido el que recibe `ICorteRepository` de Postgres -- los reportes de stock (`FindPorSucursal`, usado en `CierreStockWeb`/`ObtenerExistenciaPorSucursalesPlano`) siempre leían puntos de stock desde SQL Server, nunca desde Postgres, aunque el resto de `Corte.cs` corriera contra Postgres. No estaba documentado como deuda conocida -- se encontró al revisar los callers antes de migrar este módulo.

**Resuelto sin romper compatibilidad**: el 2do constructor de `Negocio.Corte` ahora acepta un 4to parámetro opcional `Contratos.ICortePuntoStockSucursalRepository puntoStockRepositorio = null` (default `null` → mismo comportamiento SQL Server de siempre). Los 4 call-sites existentes de ese constructor (en `MigracionPostgresController`) no pasan el nuevo parámetro y no cambian de comportamiento. Solo `CompararStockReportes` se actualizó para pasar `CortePuntoStockSucursalPg`, cerrando el gap específicamente donde se puede verificar (reportes de stock).

**`cortepuntostocksucursal` (128 filas) tiene `idEmpresa` pero en SQL Server no tiene RLS** -- mismo patrón ya encontrado y resuelto en `empresaparametros` (etapa anterior): no es un caso "usuarios" (el tenant siempre se conoce al llamar), simplemente nunca se le agregó RLS en el original. Se agrega RLS estándar en Postgres como mejora deliberada, mismo criterio ya confirmado con el usuario.

**`GuardarPuntosStockLote` usa `INSERT ... ON CONFLICT (idempresa, idcorte, idsucursal) DO UPDATE`** en vez del `MERGE` original -- mismo patrón ya usado en `AddOrEditPermisos`/`Parametros.SetValor`, dentro de una transacción explícita (mismo criterio del original: todas las sucursales de un producto se guardan atómicamente).

**Verificado**: `CarniSys.sln` completo compila limpio (incluido el cambio de firma en `Negocio/Corte.cs`, sin romper ningún caller existente). Harness `psql` (rol real, transacción explícita, `ROLLBACK`): upsert sin duplicar la constraint única, alta idempotente por sucursal (`CrearParaTodasLasSucursales` re-ejecutado no duplica) -- sin residuo. HTTP end-to-end con login real (`ger`/idEmpresa=1): `CompararPuntoStockSucursal` (55/55 productos, 110/110 celdas idénticas) y, reverificando el gap cerrado, `CompararStockReportes` (8/8 filas, idénticas salvo el mismo ruido de formato decimal ya documentado en la Etapa 11c -- `14,3000000000` vs `14,30`, mismo valor numérico).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Parametros.cs completo

Con `Corte.cs`/`Venta.cs`/`Usuario.cs` cerrados, se retoma el relevamiento de módulos 0%-migrados: quedaban 6 chicos (`CatalogoGlobalProducto`, `CortePuntoStockSucursal`, `DispositivoSeguro`, `Empresa`, `Parametros`, `WhatsApp`). El usuario eligió `Parametros.cs` por ser el más transversal (`Negocio.Parametros` implementa `IParametrosContext`, usado en toda la app para leer configuración por empresa). 5/5 métodos migrados de una sola vez (sin dos-campos intermedio, dado el tamaño): `ObtenerGrid`, `GuardarGrid`, `ObtenerDiccionario`, `ObtenerValor`, `SetValor` -- los últimos 2 sin caller real hoy (solo `Negocio.Parametros` usa los primeros 3), migrados igual por completitud de la interfaz.

**Decisión importante, confirmada con el usuario -- mejora deliberada, no réplica 1:1**: `empresaparametros` (147 filas) tiene `idEmpresa` pero en SQL Server **no tiene RLS** (verificado, 0 filas en `sys.security_policies`) -- a diferencia de las ~39 tablas RLS ya trianguladas en la Etapa 4. No es un caso como `usuarios` (no hay problema de tenant-todavía-no-conocido); simplemente nunca se le agregó RLS en el original, y el aislamiento hoy depende solo del filtro explícito de aplicación (`WHERE idEmpresa=@idEmpresa` en cada query). En Postgres se agregó RLS estándar como backstop adicional, documentado como mejora explícita.

**Hallazgo en los datos**: `EmpresaParametros` tiene 21 filas con `idEmpresa=-1` (mismo total que `Parametros`, sugiere un set "template" nunca usado) -- el código original hace *match exacto* de `idEmpresa` (nunca `-1` en producción real), así que esas filas son inalcanzables por cualquiera de los 5 métodos tanto en el original como en la traducción. Se migraron igual (fidelidad), y quedan igual de inalcanzables bajo RLS en Postgres (ningún tenant real tiene `idEmpresa=-1`).

**Nueva regla operativa aplicada, ya anticipada en la Etapa 13d**: exportación de datos con texto libre vía `System.Data.SqlClient`/PowerShell directo (Unicode de punta a punta) en vez de `sqlcmd -f 65001`, que había resultado no confiable (encoding inconsistente fila por fila). Funcionó sin problemas para las 2 tablas de esta etapa.

**Mismo trade-off ya documentado en la Etapa 12c** (`FacturaElectronica`): 4 de 21 filas de `parametros.descripcion` contienen un `|` literal (texto explicativo tipo "1 : Si | 0: No"), reemplazado por `/` en el export para no romper el delimitador del pipeline -- pérdida de fidelidad mínima, confinada a texto descriptivo/documentación, no a ningún valor de negocio (`nombre`, `valor`, `tipo` idénticos en ambos motores).

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` (rol real `carnisys_user`, transacción explícita, `ROLLBACK`): upsert sobre una fila ya existente y sobre una nueva -- sin duplicar la PK compuesta, sin residuo. HTTP end-to-end con login real (`ger`/idEmpresa=1) contra la nueva acción `CompararParametros`: 21/21 filas, 101/105 celdas idénticas exactas, las 4 restantes con la sustitución `|`→`/` ya explicada y aceptada.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Usuario.cs, bloque Auditoria de ubicacion (Etapa 13d) -- Usuario.cs queda 19/19 migrado

Ultima de las 4 sub-etapas de `Usuario.cs` (CRUD/login core, `23f38358`; Permisos, `97f8ba93`; Recuperación de contraseña, `e70e6283`; **Auditoría de ubicación**). 2 métodos de `Datos/Usuario.cs` (líneas 521-620): `RegistrarLoginUbicacion` (insert) y `obtenerLoginUbicacionLog` (lectura, `JOIN` con `Usuarios`/`Sucursal`, `TOP 500`, devuelve `DataTable` consumido por `AuditoriaLoginController` leyendo columnas por nombre exacto -- se replicaron los mismos alias `AS "IdUsuario"`, `"UsuarioNombre"`, etc. en Postgres).

**`loginubicacionlog` (tabla nueva, 11 columnas, identity nativa, 109 filas reales) no tiene `idEmpresa` propio** (confirmado en el schema real) -- el filtro por tenant es enteramente vía el `JOIN` a `usuarios` (que a su vez no tiene RLS, Etapa 13a). Motivo distinto al de `usuarios`/`usuariopasswordresettokens`: ahí *sí* hay `idEmpresa` pero se omite RLS a propósito; acá directamente no hay columna para filtrar.

**Hallazgo real durante la migración de datos, con dos capas superpuestas** -- vale la pena dejarlo documentado en detalle porque no es intuitivo:
1. **`sqlcmd -f 65001` (forzar codepage de salida a UTF-8) produce encoding *inconsistente* fila por fila**, no un problema uniforme: para algunas filas devolvió texto UTF-8 correcto, para otras lo devolvió *doble-codificado* (bytes UTF-8 reinterpretados como Latin-1 y vueltos a codificar a UTF-8 -- ej. "ó" terminaba como 4 bytes en vez de 2). El bug de PowerShell/mojibake ya documentado en la Etapa 13 (scoping) es distinto: ese corrompía un archivo ya correcto al limpiarlo; este es el propio `sqlcmd` generando salida mixta.
2. **Al investigar cuál de las dos versiones era la "correcta", se descubrió que ninguna sqlcmd-based lo era**: exportando la misma tabla directamente vía `System.Data.SqlClient` (sin pasar por la consola/codepage de `sqlcmd` en absoluto) se confirmó que **2 filas reales de `dbo.LoginUbicacionLog` ("Sucursal sin validación de ubicación.", ids 1-2) tienen el texto genuinamente corrupto en la base de origen** (mojibake real, no un artefacto de exportación) -- probablemente insertado por un proceso viejo con un bug de encoding propio, mientras que las filas más nuevas (insertadas por la app actual) están correctas. **Se migró tal cual, sin "arreglar" la corrupción preexistente** -- alterar datos de origen más allá de lo que pide una migración 1:1 no es parte del alcance, y sería una decisión de negocio (¿corregir el texto? ¿a qué valor?) fuera de lugar para una tarea mecánica. Verificado con un diff celda-a-celda completo (840 valores) entre SQL Server y Postgres tras la corrección: **idénticos, incluida la corrupción replicada en las mismas 2 filas**.
3. **Nueva regla operativa para el resto del proyecto** (si hiciera falta exportar texto con acentos de nuevo): preferir exportar via `System.Data.SqlClient`/PowerShell directo (Unicode de punta a punta, sin ninguna capa de codepage de consola) en vez de `sqlcmd -f 65001`, que resultó no ser confiable.

**`Negocio/Usuario.cs` colapsado a un solo campo** (`oUsuarioD`, `oUsuarioDSqlServer` eliminado) -- con los 19/19 métodos de `Datos.Usuario` ya en `IUsuarioRepository`, mismo criterio que `CierreCaja` (Etapa 10) y `Venta.cs` (Etapa 12c).

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` (rol real `carnisys_user`, transacción explícita, `ROLLBACK`): alta de un registro de ubicación + lectura vía el `JOIN` equivalente -- sin residuo. HTTP end-to-end con login real (`ger`/idEmpresa=1) contra la nueva acción `CompararLoginUbicacionLog`: **70/70 filas, 840/840 celdas idénticas** entre SQL Server y Postgres (grilla completa, no solo conteo). Un primer intento de comparación dio 70 vs 69 -- investigado y confirmado como un artefacto de timing (una fila nueva, generada por un re-login real durante la propia verificación, llegó a SQL Server después del snapshot migrado a Postgres), no un bug; se resolvió re-exportando.

**Cierre de módulo**: con esta etapa, `Usuario.cs` (19/19 métodos) queda completamente migrado a `Contratos.IUsuarioRepository`/`DatosPostgres.UsuarioPg`. Igual que el resto de la migración, el código queda listo y verificado pero **no en producción** -- el login real de la app sigue usando el constructor SQL Server de siempre; el cutover de tráfico real es una decisión aparte.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Usuario.cs, bloque Recuperacion de contrasena (Etapa 13c)

Tercera de las 4 sub-etapas de `Usuario.cs` (CRUD/login core, Etapa 13a, `23f38358`; Permisos, Etapa 13b, `97f8ba93`; **Recuperación de contraseña** -> Auditoría de ubicación). 4 métodos de `Datos/Usuario.cs` (líneas 398-519): `CrearTokenRecuperacion`, `ObtenerTokenRecuperacion`, `MarcarTokenRecuperacionComoUsado`, `InvalidarTokensPendientesUsuario`.

**`usuariopasswordresettokens` (tabla nueva, 11 columnas, identity nativa, 6 filas reales) queda SIN RLS, por el mismo motivo raíz que `usuarios` (Etapa 13a) -- confirmado con el usuario antes de implementar**: `ObtenerTokenRecuperacion` busca por `tokenHash` sin filtrar por `idEmpresa` -- el link de recuperación de contraseña llega por mail a un usuario anónimo (sin sesión, sin tenant conocido) que hace click y cae directo en `ResetPassword` con el token en la URL; con RLS, esa consulta no podría encontrar la fila. Verificado contra la base real: `dbo.UsuarioPasswordResetTokens` tampoco tiene RLS (`sys.security_policies`, 0 filas) pese a tener `idEmpresa`. El aislamiento real para esta tabla no es por tenant sino por el token en sí (hash aleatorio, un solo uso, con expiración).

**Ningún método de este bloque filtra por `idEmpresa`** -- réplica exacta del SQL original (`CrearTokenRecuperacion` inserta el valor que le pasan, el resto opera solo por `id`/`tokenHash`/`idUsuario`).

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` (rol real `carnisys_user`, transacción explícita, `ROLLBACK`): alta de token + lectura por `tokenHash`, marcar como usado, e invalidar un token pendiente distinto del mismo usuario -- sin residuo. HTTP end-to-end con login real (`ger`/idEmpresa=1) contra la nueva acción `CompararTokenRecuperacion`: token real #1 (migrado de SQL Server) idéntico campo a campo en ambos motores, incluido el `tokenHash` con caracteres especiales de base64 (`+`, `/`, `=`).

**`tokenHash` no es un secreto reversible** (hash de un solo sentido, igual que las contraseñas hasheadas) -- se muestra en la vista de comparación sin problema, a diferencia de `clave`/`passwordHash`/`passwordSalt` (Etapa 13a), que sí se ocultan.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Usuario.cs, bloque Permisos (Etapa 13b)

Segunda de las 4 sub-etapas de `Usuario.cs` (CRUD/login core, Etapa 13a, commit `23f38358` -> **Permisos** -> Recuperación de contraseña -> Auditoría de ubicación). La más chica del módulo: 2 métodos de `Datos/Usuario.cs` (líneas 205-299), `getPermisosUsuario` (LEFT JOIN `Formularios`/`PermisosUsuarios` con defaults `-1/-1/true` cuando no hay fila propia) y `AddOrEditPermisos` (upsert por fila). Sin tablas nuevas -- `formularios`/`permisosusuarios` ya existían desde la Etapa 13a.

**`AddOrEditPermisos` usa `INSERT ... ON CONFLICT (idusuario, idform) DO UPDATE` nativo de Postgres** en vez del `IF EXISTS ... UPDATE ELSE INSERT` del SP original -- mismo efecto (upsert), una sola sentencia por fila en vez de un `SELECT` + rama condicional. La PK compuesta `(idusuario, idform)` de `permisosusuarios` (definida en la Etapa 13a) es lo que habilita el `ON CONFLICT` directo.

**`PermisosUsuarios.idEmpresa` tiene el mismo patrón de `DEFAULT` atado a `SESSION_CONTEXT('IdEmpresa')`** que ya se había encontrado en `Sectores` (Etapa 12a) -- confirmado con `sys.default_constraints`. El INSERT original no lo pasa explícito; en Postgres se bindea `idempresa=@idEmpresa` explícito, mismo criterio ya usado ahí.

**3 call sites redirigidos** en `Negocio/Usuario.cs`: el wrapper directo de `getPermisosUsuario`, el de `AddOrEditPermisos`, y una tercera llamada embebida dentro de `convertDatatableToList` (enriquece `Usuario.Permisos` al listar usuarios) -- las 3 pasaron de `oUsuarioDSqlServer` a `oUsuarioD`.

**Verificado**: `CarniSys.sln` completo compila limpio. Harness `psql` (rol real `carnisys_user`, transacción explícita, `ROLLBACK`): alta y edición sobre la misma fila (`idusuario=2, idform=3`) vía `ON CONFLICT`, confirmando que no duplica la PK y que los valores se actualizan correctamente; verificación adicional del `LEFT JOIN`/`COALESCE` de `getPermisosUsuario` para un formulario con fila propia y uno sin ella -- sin residuo tras el rollback (una comprobación posterior sin `app.id_empresa` seteado dio 0 filas por RLS, no por un fallo real del rollback -- mismo tipo de falso positivo ya documentado en la Etapa 11b, ahora también en Postgres). HTTP end-to-end con login real (`ger`/idEmpresa=1) contra la nueva acción `CompararPermisosUsuario`: **30/30 permisos idénticos campo a campo** (IdForm, Formulario, DiasVer, DiasEditar, SoloPropios) entre SQL Server y Postgres para el usuario #2.

**Nota operativa, no un bug de esta etapa**: la verificación HTTP encontró SQL Server Express genuinamente no-responsivo por varios minutos (incluso un `SELECT 1` directo tardó ~58s), sin `MSBuild.exe`/`VBCSCompiler.exe` corriendo -- a diferencia de las flakiness anteriores de esta sesión (causadas por *worker nodes* de compilación), esta vez coincidió con carga general alta de la máquina (múltiples procesos VS Code/Claude). Se resolvió esperando y reintentando; no requirió ninguna acción sobre el código o la base.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Usuario.cs, bloque CRUD/login core (Etapa 13a)

Con `Corte.cs` y `Venta.cs` completos, se releva el resto de `Datos/` (7 módulos 0% migrados) y se elige `Usuario.cs` (19 métodos, el más grande e importante -- login/permisos/sucursal asignada), dividido en 4 sub-etapas por sub-dominio: **CRUD/login core** (esta etapa) → Permisos → Recuperación de contraseña → Auditoría de ubicación. 11 métodos cubiertos: `obtenerUsuarios`, `getUsuarioActivos`, `getUsuarioById`, `addOrEditUser`, `setSucursalUsuario`, `setPermitirLoginFueraSucursal`, `setEsUsuarioProduccion`, `ActualizarEstadoBloqueoLogin`, `BuscarUsuariosPorIdentificador`, `ActualizarPasswordSeguro`, `ActualizarPasswordWebSeguro`.

**Decisión importante, corrige el default de esta migración -- confirmada y corregida por el usuario**: `usuarios` en Postgres **NO lleva RLS**, a diferencia de todas las demás tablas multi-tenant migradas hasta ahora. El usuario señaló la razón antes de que se implementara: en el momento del login todavía no se sabe a qué empresa pertenece el usuario -- si `usuarios` tuviera RLS filtrando por `app.id_empresa`, la propia consulta de login no podría encontrar la fila porque ese contexto todavía no existe. **Verificado contra la base real** (`sys.security_policies`/`sys.security_predicates` sobre `dbo.Usuarios`, 0 filas): SQL Server tampoco tiene RLS ahí, confirmando que replicar exactamente ese criterio (y no el RLS-por-defecto del resto de la migración) era lo correcto. El aislamiento por tenant se resuelve a nivel de aplicación (`WHERE idempresa=@idEmpresa` explícito donde corresponde, igual que ya hacía `Datos/Usuario.cs`). Verificado en vivo: `CompararUsuario?idUsuario=5` (usuario de `idEmpresa=2`) consultado desde una sesión de `idEmpresa=1` devuelve la fila completa e idéntica en ambos motores -- comportamiento cruzado consistente, no una fuga accidental.

**Hallazgo real, cambió el alcance de esta sub-etapa**: el SP `addOrEditUser` (via `sp_helptext`), en su rama de alta, además de insertar en `Usuarios` inserta permisos por defecto en `dbo.PermisosUsuarios` (`LEFT JOIN` contra `dbo.Formularios` + una tabla de 12 valores por defecto hardcodeada por formulario). Se resolvió con el mismo patrón de "minimal slice" ya usado en la Etapa 10 para Expendios: se trajo el schema + datos completos de `Formularios` (30 filas, catálogo global sin `idEmpresa`, sin RLS) y `PermisosUsuarios` (269 filas, con `idEmpresa`, RLS estándar) en esta sub-etapa, sin exponer todavía `getPermisosUsuario`/`AddOrEditPermisos` en `IUsuarioRepository` (quedan para la Etapa 13b).

**`Usuarios.id` pasa a ser IDENTITY en Postgres, por instrucción explícita del usuario**: ni en SQL Server ni en el stub Postgres previo era autonumérico -- el SP original calculaba `SELECT @id = ISNULL(MAX(id), 0) + 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)`, con un comentario del propio autor reconociendo el riesgo ("OJO: MAX+1 puede colisionar con concurrencia; ideal sería IDENTITY o SEQUENCE"). El usuario pidió resolverlo de raíz en vez de replicar la limitación: `ALTER TABLE usuarios ALTER COLUMN id ADD GENERATED BY DEFAULT AS IDENTITY` (retrofit sobre una tabla ya poblada, primera vez en esta migración -- toda otra tabla nueva definía identity desde el `CREATE TABLE`), con `setval` posicionado después del máximo id real. La rama de alta de `addOrEditUser` en Postgres usa `INSERT ... RETURNING id`, sin `MAX+1` ni locking.

**3 tablas nuevas/alteradas**: `usuarios` (ALTER, no recreación -- ya existía como stub de 6 columnas para `GetUsuarioLiviano`; se agregaron 14 columnas + identity, 15 filas reales enriquecidas via `UPDATE ... FROM stg_usuarios`, no `INSERT`), `formularios` (30 filas, catálogo global sin RLS), `permisosusuarios` (269 filas, RLS estándar, `idempresa` derivado por `JOIN` contra `Usuarios.idEmpresa` en el export y verificado 0 discrepancias contra el valor real ya almacenado).

**`ExisteColumnaUsuarios(columnName)`** (helper de `Datos/Usuario.cs` que consulta `sys.columns` en tiempo de ejecución para tolerar bases en distintos estados de esquema, ej. SM/San Lorenzo en SQL Server 2008) **no se replica en Postgres**: el schema se define completo de una sola vez, todas las columnas existen siempre.

**Verificado**: `CarniSys.sln` completo (incluidos `Presentacion.csproj`/`wsAFIPvs2008.csproj`) compila limpio -- se agregó `: Contratos.IUsuarioRepository` a `Datos/Usuario.cs` (primera vez que se toca este archivo, sin cambios de comportamiento) y la entrada `Compile` faltante para `Models/ComparacionUsuarioVm.cs` en `Web.csproj` (formato legacy, no autoincluye archivos nuevos). Harness `psql` (rol real `carnisys_user`, transacción explícita, `ROLLBACK`): alta de usuario (id autogenerado sin colisión con los 16 ids ya existentes, 30 filas de `permisosusuarios` creadas -- una por formulario -- con los valores por defecto exactos de la tabla `VALUES` del SP original, incluido el fallback `-1,-1,true` para formularios no listados), edición, `ActualizarPasswordSeguro`, `ActualizarEstadoBloqueoLogin` -- sin residuo tras el rollback. HTTP end-to-end con login real (IIS Express, usuario `ger`/idEmpresa=1, ya `superadmin=true` en la base -- no hizo falta crear un usuario descartable nuevo) contra la nueva acción `CompararUsuario`: usuario #2 (mismo tenant) y usuario #5 (`idEmpresa=2`, lookup cross-tenant) idénticos campo a campo en ambos motores.

**No se muestran `clave`/`passwordHash`/`passwordSalt` en la vista de comparación** (`CompararUsuario.cshtml`): son credenciales, nunca van en un output visible (CLAUDE.md §4).

**Nota operativa, no un bug de esta etapa**: timeouts intermitentes de conexión a SQL Server durante la verificación HTTP (login y la primera consulta de `EsSuperAdmin`), sin `MSBuild.exe`/`VBCSCompiler.exe` corriendo -- mismo patrón de contención intermitente ya documentado en la Etapa 12a, resuelto reintentando la request.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Corte.cs, bloque Movimiento (Etapa 11b)

Segunda de las 3 sub-etapas del resto de `Corte.cs`. 11 métodos: transferencias de stock entre sucursales (`Movimiento`/`CortePorMovimiento`), más `MovimientoHistorial` (tabla nueva, hallazgo al leer los SPs reales — auditoría insert-only, **sin PK ni identity** en el original, replicada igual en Postgres).

**Fix real, commit separado, confirmado por el usuario (SQL Server, no forma parte de la migración mecánica)**: `Datos/Corte.cs`, `obtenerUltimosMovimientosDashboard` consultaba `dbo.Movimientos` (plural, no existe) con columnas `idOrigen`/`idDestino` (no existen) — la tabla real es `Movimiento` (singular) con `sucursalOrigen`/`sucursalDestino`. El widget del dashboard (`HomeController.cs`) está envuelto en `try/catch` y fallaba en silencio en producción. Corregido en `Datos/Corte.cs`; `CortePg.cs` implementa la versión correcta desde el inicio.

**`modificarMovimiento`/`quitarCortesPorMovimiento` migrados aunque sin caller vivo hoy** (sus wrappers en `Negocio/Corte.cs` ya estaban comentados antes de esta etapa) — a diferencia de `obtenerEmbutidos` (Etapa 11a), estos SPs no están rotos, solo sin uso actual, así que se migran igual (costo bajo, interfaz completa) en vez de excluirlos.

**No-ops documentados**: `agregarCortePorMovimiento` (solo el `INSERT` real; la cascada `StockCorteSucursal` del SP ya viene **comentada en el propio SP de origen**, deshabilitada desde antes de esta migración) y `quitarCortesPorMovimiento` (cascada `StockCorteSucursal` no-op + `DELETE FROM CortePorMovimiento` real).

**Hallazgo al verificar por HTTP, no es un bug — mismo patrón que la Etapa 7**: `cargarCortesPorMovimiento(idMovimiento=2, acumulado=false)` devuelve 1 línea en vez de 3 bajo el tenant 1 real, en **ambos motores por igual**: 2 de las 3 filas de `CortePorMovimiento` referencian un `idCorte` que pertenece a otro tenant (`idEmpresa=3`), un artefacto de datos cruzados real. El `INNER JOIN` a `Corte`, protegido por RLS, excluye esas líneas silenciosamente en SQL Server (RLS por `SESSION_CONTEXT`) igual que en Postgres (RLS por `app.id_empresa`) — comportamiento fiel, no un gap. (Nota metodológica: verificado inicialmente con una consulta `psql` que bypaseaba RLS sin querer — vía el rol dueño de las tablas y sin transacción explícita, ambos casos evitan el chequeo de RLS. Repetido con el rol real de la app (`carnisys_user`) y una transacción explícita, coincide exacto con SQL Server. El chequeo por HTTP de esta etapa, que sí usa el camino real, nunca estuvo mal.)

**Verificado de punta a punta**: build de la solución completa sin errores. Harness `psql`+`ROLLBACK` para `addOrEditMovimiento` (alta con `RETURNING`, y edición completa: snapshot en `MovimientoHistorial` + `UPDATE` + ajuste de `actualizacionCompleta` + limpieza de líneas), `agregarCortePorMovimiento` y `eliminarMovimiento`, sin dejar datos de prueba. Las consultas de solo lectura (`obtenerMovimientos`, `obtenerLineasMov`, `obtenerUltimosMovimientosDashboard` ya corregido, `ObtenerTotalesPorMovimiento`) corridas contra datos reales sin errores. HTTP con login real (2 usuarios descartables, creados y borrados): `/MigracionPostgres/CompararMovimiento?idMovimiento=2` coincide exacto en ambos motores (incluida la línea de `CortePorMovimiento` visible bajo RLS); logueado como tenant 2, el mismo `idMovimiento=2` (tenant 1) da "no encontrado" en ambos motores por igual.

Con esto, `Corte.cs` queda con una sola sub-etapa pendiente: Stock/Reportes (Etapa 11c). **Corrección de una suposición previa, verificada recién con los SPs reales**: se había asumido que estos SPs grandes (`a_CierreStock` ~2050 líneas, `StockCierre_2` ~1250, `StockIngresoEgreso` ~1050, `a_ExistenciaStockPorSucursales` ~750) tendrían cascadas extensas de `StockCorteSucursal` (tabla obsoleta) igual que el resto de esta migración — **falso, confirmado por grep**: ninguna de las 8 SPs de Stock/Reportes menciona `StockCorteSucursal`. Son reportes reales tipo kardex, que calculan el stock agregando en el momento las tablas transaccionales ya migradas (`CortePorCompra`, `CortePorEmbutido`, `CortePorMovimiento`, `LineaVenta`) en vez de leer una tabla de stock materializada. Etapa 11c es grande y real, no mayormente no-op — se scopea aparte con el usuario antes de planificar. También sigue pendiente el resto de `Venta.cs` (CRUD completo de `Expendios`, `Sectores`, `FacturaElectronica`).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Corte.cs, bloque Embutido (Etapa 11a)

Primera de 3 sub-etapas en las que se dividió el resto de `Corte.cs` (38 métodos, 29 SPs reales, varios de más de 1000 líneas — mucho más grande de lo esperado al scopearlo). **Decisión confirmada con el usuario**: dividir en Embutido → Movimiento → Stock/Reportes, en ese orden. Esta etapa cubre solo Embutido (14 métodos relevados, 13 migrados).

**Hallazgo: `obtenerEmbutidos` excluido, documentado, no un descuido.** Confirmado con `sp_helptext` contra la base viva (no el snapshot): el SP hace `INNER JOIN` contra `StockCorteSucursal` dos veces — como esa tabla tiene 0 filas reales (confirmado de nuevo), el `JOIN` nunca matchea y el SP **siempre devuelve 0 filas hoy en SQL Server**, para cualquier dato. Además filtra sucursales hardcodeadas (`idSucursal = 2` y `= 1`, un quirk de un setup viejo de 2 sucursales). Verificado por grep en todo el repo: el wrapper `Negocio.Corte.obtenerEmbutidos` no tiene ningún caller real (ni Web ni Presentacion) — código muerto y ya roto en origen. No se agregó a `ICorteRepository`; mismo criterio que `backup`/`restaurarBD` (Etapa 9).

**Tablas nuevas**: `embutidos` (49 filas) y `corteporembutido` (180 filas), ambas con RLS estándar (4 predicados), verificadas contra la base viva antes de escribir el schema. `obtenerCorteProveedor`/`obtenerCortesPorProveedor` (2 de los 13 métodos) ya no necesitaron tablas nuevas — leen `corteproveedor`/`compras`/`corteporcompra`, migradas en la Etapa 9.

**No-op documentado**: `agregarCortePorEmbutido` — se replica solo el `INSERT INTO CortePorEmbutido` real; las 8 `UPDATE StockCorteSucursal` (cascada de stock del corte usado en el embutido) son no-op, mismo criterio de siempre (Etapa 6).

**`CortePg.cs` resuelve `Sucursal` con un helper liviano nuevo (`GetSucursalLiviana`)**, mismo patrón ya usado ahí mismo para `Usuario` (`GetUsuarioLiviano`) — se evitó agregar una dependencia `ISucursalRepository` al constructor de `CortePg` (que hubiera sido una firma pública nueva, tocando todos los call-sites existentes) para resolver un solo campo de un solo método.

**Verificado de punta a punta**: build de la solución completa sin errores. Harness `psql`+`ROLLBACK` para `agregarEmbutido` (incluido el `RETURNING idembutido` equivalente al `SELECT TOP 1 ... ORDER BY idEmbutido DESC` del SP original), `agregarCortePorEmbutido` y `anularEmbutido`, sin dejar datos de prueba. Los otros métodos de solo lectura (`buscarEmbutido`, `obtenerLineasEmb`, `obtenerInfoCorte`, `obtenerCorteProveedor`, `obtenerCortesPorProveedor`) se ejecutaron directo contra datos reales sin errores. HTTP con login real (2 usuarios descartables, creados y borrados): `/MigracionPostgres/CompararEmbutido?idEmbutido=1` (tenant 1, con línea de `CortePorEmbutido`) devuelve los mismos campos en ambos motores; logueado como tenant 2, el mismo `idEmbutido=1` (tenant 1) da "no encontrado" en **ambos** motores por igual (RLS).

Sigue pendiente: Movimiento (Etapa 11b) y Stock/Reportes (Etapa 11c) de `Corte.cs`, y el resto de `Venta.cs` (Expendios/Sectores/FacturaElectronica).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: cambiarSucursalCaja / obtenerPreviewCambioSucursalCaja (Etapa 10)

Última pieza diferida de `CierreCaja.cs` (excluida explícitamente en la Etapa 8 porque dependía de tablas que todavía no estaban migradas). A diferencia de toda la migración anterior, esta operación **no usa stored procedures**: está escrita 100% en C# (`Datos/CierreCaja.cs`) con SQL parametrizado directo — el propio código fue la fuente de verdad, leído completo, sin riesgo de `sp_helptext`/snapshot stale.

**Hallazgo al scopear la etapa**: de las 9 tablas que toca la operación, 8 ya estaban migradas (`CierreCaja`, `Ventas`, `Compras`, `CortePorCompra`, `MediaRes`, `Pagos`, `EgresosCaja`, `MovCtaCte`, `TemporalLineaVenta`) — solo faltaba **`Expendios`** (94 filas reales, activamente usada por `PuntosExpendioController`/POS, no obsoleta). **Confirmado con el usuario**: se migró un slice mínimo de `Expendios` — schema y datos reales completos, pero solo las 2 consultas que esta operación necesita (`SELECT` de ids por filtro + `UPDATE idsucursal`). El resto de `Expendios` (CRUD completo, `Sectores`, `FacturaElectronica`) sigue pendiente, su propia etapa futura.

**Movimiento de tipos, firma pública afectada (avisado, no un cambio silencioso)**: `CambioSucursalCajaTabla`/`CambioSucursalCajaPreview`/`CambioSucursalCajaResultado` vivían como clases anidadas dentro de `Datos.CierreCaja` (SQL Server). Se movieron a `Contratos/CambioSucursalCajaTypes.cs` (POCOs puros, sin cambio de forma) porque la interfaz `ICierreCajaRepository` ahora declara `obtenerPreviewCambioSucursalCaja`/`cambiarSucursalCaja` y ambas implementaciones (`Datos.CierreCaja` y `DatosPostgres.CierreCajaPg`) necesitan devolver el mismo tipo. Verificado antes del cambio: el único consumidor real (`Web/Controllers/CajasController.cs`) solo accede por propiedades, no rompe nada.

**`Negocio/CierreCaja.cs` vuelve a un solo campo de datos**: como estos 2 métodos eran los únicos fuera de `ICierreCajaRepository` (verificado por grep, cero otros usos), el campo `oCierreDSqlServer` del patrón de dos campos (introducido en la Etapa 8) quedó muerto tras esta etapa y se eliminó. Es la primera clase de esta migración en volver a tener un solo campo tras cubrir el 100% de su interfaz.

**Divergencia de comportamiento, ya anticipada y decidida en la Etapa 8, ahora concretada**: en Postgres `cierrecaja.id` es autoincremental (decisión del usuario), no namespaced por sucursal como en SQL Server — no hay nada que recalcular al mover una caja. `DatosPostgres.CierreCajaPg.cambiarSucursalCaja` solo actualiza `idsucursal` en `cierrecaja`, nunca `id`; `IdCierreCajaNuevo` del preview, del lado Postgres, es siempre el mismo `IdCierreCaja`. Verificado por HTTP: para la misma caja, SQL Server devuelve un `IdCierreCajaNuevo` renumerado (esquema namespaced) mientras Postgres devuelve el mismo id — divergencia visible y esperada, el resto de los campos (mensaje, conteos por tabla, bloqueo por caja abierta en destino) coincide exacto entre motores.

**`AuditoriaCambioSucursalCaja` sin RLS**: en SQL Server el original la crea de forma perezosa (DDL idempotente la primera vez que se ejecuta un cambio real) y **sin política de RLS** — omisión real del sistema fuente, no un descuido de esta migración. Se preserva la misma fidelidad en Postgres (`auditoriacambiosucursalcaja`, tabla provisionada de entrada en el schema de esta etapa en vez de vía DDL en tiempo de ejecución — mismo estado final, sin necesidad de emitir DDL desde `CierreCajaPg`).

**No se agregó comparador HTTP para `cambiarSucursalCaja` en sí**: es una operación mutante (mueve datos reales de sucursal), no tiene sentido "compararla" por HTTP sin dejar residuo en el piloto. Se verificó exclusivamente con un harness `psql`+`ROLLBACK` contra una caja real con volumen real de datos asociados (`id=20000007`: 317 ventas, 43 compras, 37 pagos, 93 expendios movidos correctamente, fila de auditoría insertada, todo revertido limpio). Solo `obtenerPreviewCambioSucursalCaja` (de solo lectura) se expone en `/MigracionPostgres/CompararPreviewCambioSucursalCaja` — verificado por HTTP con login real: caja bloqueada por "ya tiene caja abierta en destino" (mismo mensaje en ambos motores), caja ejecutable (mismos conteos por tabla en ambos motores, salvo el `IdCierreCajaNuevo` ya explicado), y caja de otro tenant ("no encontrado" en ambos motores).

Con esto, `CierreCaja.cs` queda 100% migrado. Trabajo pendiente de la migración: resto de `Corte.cs` (Embutido/Movimiento/reportes, Etapa 6) y resto de `Venta.cs` (CRUD completo de `Expendios`, `Sectores`, `FacturaElectronica`, Etapa 7).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Compra.cs completo (Etapa 9)

Última entidad núcleo del negocio: 30 métodos de 32 en `Datos/Compra.cs` (`Compras`, `CortePorCompra`, `MediaRes`, y `CorteProveedor` — tabla nueva encontrada leyendo los SPs reales, ver abajo). Mismo patrón mecánico: `Contratos/ICompraRepository.cs` -> `Datos.Compra : ICompraRepository` (cero cambios) -> patrón de dos campos en `Negocio/Compra.cs` (`oCompraD` interfaz para el batch, `oCompraDSqlServer` concreto solo para `backup`/`restaurarBD`) -> `DatosPostgres/CompraPg.cs`.

**`backup`/`restaurarBD` fuera de `ICompraRepository`**: `BACKUP DATABASE [SuperCerdo]` / `RESTORE DATABASE [SuperCerdo]`, herramientas administrativas de SQL Server sin equivalente 1:1 en Postgres (que usa `pg_dump`/`pg_basebackup`, mecanismo de otro motor). No es un olvido — quedan siempre en SQL Server vía `oCompraDSqlServer`.

**Hallazgo nuevo, no anticipado en el plan**: `agregarCortePorCompra` (verificado con `sp_helptext`) no es solo el `INSERT` en `CortePorCompra` — cuando la compra es tipo `'Cortes'`, además hace un upsert condicional en `CorteProveedor` (último precio y fecha de última compra por proveedor+corte). Esa tabla no estaba en el batch de la Etapa 6 (se había dejado fuera junto con `obtenerCorteProveedor`/`obtenerCortesPorProveedor`, que siguen sin migrar). Se agregó al schema de esta etapa (24 filas, RLS activo, verificado contra la base viva) e implementado con una transacción explícita (`ConexionPg.AbrirConTenant`) en `CompraPg.agregarCortePorCompra`, igual que el SP real: INSERT en `CortePorCompra`, luego si la compra es `'Cortes'` y ya existe fila en `CorteProveedor` para ese proveedor+corte hace `UPDATE` (solo si la fecha nueva es más reciente), si no `INSERT`. Verificado con harness `psql` + `ROLLBACK` (ambas ramas, update e insert).

**No-ops confirmados con `sp_helptext` (mismo criterio que Etapa 6: `StockCorteSucursal` nunca se porta a Postgres)**:
- `anularCompra`: de 9 statements del SP real, 1 es real (`UPDATE Compras SET estado='Anulado'`), 8 son cascadas `StockCorteSucursal` con `SuperCerdo.dbo.` hardcodeado — no-op.
- `quitarStockMedia`: 3 `UPDATE StockCorteSucursal` — no-op total, `CompraPg.quitarStockMedia` es un método vacío documentado.
- `quitarStockTeoricoMedia`: se replica solo la parte real (`DELETE FROM MediaRes WHERE idMedia=@`), el resto (3 `UPDATE StockCorteSucursal.stockTeorico`) es no-op.
- `agregarMediaRes`: se replica solo el `INSERT INTO MediaRes` real; las 6 `UPDATE StockCorteSucursal` (stock y stockTeorico, cascada de 3 niveles cada una) son no-op.
- `quitarStockCorte` no tiene parte no-op: siempre fue puro `DELETE FROM CortePorCompra`, se migró completo.

**Bug real, no inventado — `modificarCortePorCompra`**: el SP `dbo.modificarCortePorCompra` **no existe** en la base SQL Server real (confirmado con `sp_helptext`: *"The object 'dbo.modificarCortePorCompra' does not exist in database 'CarniSys'"*). `Datos/Compra.cs` lo invoca igual (dead code preexistente), pero no tiene ningún caller real en `Web/` (verificado por grep en toda la solución). Hoy mismo, en SQL Server, llamar a este método tira excepción. Por regla de incertidumbre (no inventar un UPDATE plausible para un SP que no existe), `CompraPg.modificarCortePorCompra` lanza `NotSupportedException` documentando el hallazgo — misma clase de falla que produce el original, sin adivinar comportamiento.

**Simplificaciones deliberadas, sin cambio de comportamiento observable**:
- `obtenerCompras`/`getLineasCompras`: el SP real duplica la query completa en dos ramas (`IF @idSucursal > 0` / `ELSE`), idénticas salvo el filtro de sucursal. En Postgres se unificó en una sola query con `(@idSucursal = 0 OR idsucursal = @idSucursal)` — mismo resultado para ambos casos, ya usado en `CierreCajaPg` (Etapa 8).
- `obtenerPesajesVinculadosPorDestinos`/`getIdsAjustePorPesajes`: el original trocea en lotes de 900 ids por el límite de parámetros de SQL Server; Postgres soporta arrays nativos (`= ANY(@ids)`) sin ese límite, así que no hace falta trocear.
- `conexionSucursal` (ruteo a sucursales remotas San Martín/San Lorenzo vía otra conexión SQL Server): ignorado en `CompraPg`, siempre consulta la base local — mismo tratamiento que `SucursalPg` con esas sucursales (fuera de alcance de toda la migración desde la Etapa 1).

**Verificado de punta a punta**: build de la solución completa sin errores. Harness directo (`psql` + `ROLLBACK`): `agregarCortePorCompra` (insert en `CortePorCompra` + ambas ramas del upsert de `CorteProveedor`) y `addOrEditCompra` en edición (update + limpieza de `CortePorCompra`/`MediaRes`), sin dejar datos de prueba. Los 3 reportes (`porcentajeCortesPorCompra`, `getPromMedias`, `getPorcCortesEnMedias`) y la query completa de `obtenerCompras` (68 filas, una por compra real, sin duplicados) se ejecutaron directo contra la base con datos reales sin errores. HTTP con login real (2 usuarios descartables, idEmpresa 1 y 2, creados y borrados): `/MigracionPostgres/CompararCompra?idCompra=5` (tenant 2, tipo Cortes) y `?idCompra=1` (tenant 1, tipo Ingreso Stock) devuelven los mismos campos en SQL Server y Postgres; logueado como tenant 1, `idCompra=5` (tenant 2) da "no encontrado" en **ambos** motores por igual (RLS).

Con esto, `Compra.cs` completo (salvo `backup`/`restaurarBD`, fuera de alcance por diseño) queda migrado. Trabajo pendiente de la migración: `cambiarSucursalCaja`/`obtenerPreviewCambioSucursalCaja` (Etapa 8), resto de `Corte.cs` (Embutido/Movimiento/reportes, Etapa 6), resto de `Venta.cs` (Expendios/Sectores/FacturaElectronica, Etapa 7).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: CierreCaja.cs, bloque CierreCaja/EgresosCaja/TiposEgresoCaja (Etapa 8)

Bloque priorizado explícitamente para cerrar el gap de `docs/GAPS.md` dejado por la Etapa 7 (reverso de `EgresosCaja` en `modificarVenta`). 15 métodos de 13 en `Datos/CierreCaja.cs`: CierreCaja (`findCierreCaja`, `addOrEditCierreCaja`, `findCierreCajaMultiples`), TiposEgresoCaja (CRUD chico), EgresosCaja (9 métodos, el movimiento de caja real). Mismo patrón mecánico: `Contratos/ICierreCajaRepository.cs` -> `Datos.CierreCaja : ICierreCajaRepository` (cero cambios) -> patrón de dos campos en `Negocio/CierreCaja.cs` (`oCierreD` interfaz, `oCierreDSqlServer` para `cambiarSucursalCaja`/`obtenerPreviewCambioSucursalCaja`, sin migrar) -> `DatosPostgres/CierreCajaPg.cs`.

**Fix real, commit separado, confirmado por el usuario (SQL Server, no forma parte de la migración mecánica)**: `CalcularNuevoIdCierreCaja` (privado, usado solo por `cambiarSucursalCaja`) calculaba el id nuevo con base `100_000_000 * idSucursalNueva` — un cero de más respecto a la base real (`10_000_000 * idSucursal`) que usa `addOrEditCierreCaja` para generar cajas nuevas. Las dos bases convivían inconsistentes en el mismo sistema. Corregido a `10_000_000` en `Datos/CierreCaja.cs`. No cambia ningún id ya persistido, solo los ids que se calculen de ahora en más al transferir una caja de sucursal.

**`CierreCaja.id`: autoincremental en Postgres, decisión explícita del usuario, divergencia deliberada entre motores**. En SQL Server `id` NO es identity — el SP real genera un esquema propio namespaced por sucursal (`10_000_000 * idSucursal + N`), usado además por `cambiarSucursalCaja` para recalcular el id al mover una caja de sucursal (ver el fix de arriba). Se evaluó explícitamente el impacto de no replicar ese esquema en Postgres: no rompe nada del batch de esta etapa (ningún método decodifica sucursal desde el id), pero **cuando se migre `cambiarSucursalCaja`** (fuera de esta etapa) esa función va a necesitar un diseño distinto en Postgres, ya que no habrá encoding numérico que recalcular — probablemente alcance con actualizar `idsucursal` sin tocar `id`. `Datos/CierreCaja.cs` (SQL Server) no se tocó: sigue generando el id con el esquema namespaced real, sin cambios. `TiposEgresoCaja.id` sí replica el esquema real (`MAX(id)+1` manual, mismo riesgo de condición de carrera ya reconocido en el código original) — no se pidió cambiar ese.

**Cierre del gap de `docs/GAPS.md`**: `VentaPg.modificarVenta` ahora implementa el reverso completo en `EgresosCaja` (buscar el último egreso con `tabla='Ventas' AND idtabla=@idVenta`, copiar la fila con monto negado y descripción prefijada `"Anulado:"`), dentro de la misma transacción que ya usaba. Verificado con un harness directo (`psql`, `ROLLBACK`): reproduce exacto el comportamiento del SP real. `docs/GAPS.md` queda sin entradas abiertas.

**Fuera de alcance de esta etapa, documentado en el código**: `cambiarSucursalCaja`/`obtenerPreviewCambioSucursalCaja` (operación cross-cutting que toca `CierreCaja`, `Ventas`, `Compras`, `CortePorCompra`, `MediaRes`, `Pagos`, `EgresosCaja`, `MovCtaCte`, `Expendios`, `TemporalLineaVenta` y una tabla de auditoría propia en una sola transacción) — ni siquiera están en `ICierreCajaRepository` todavía, se agregan en una etapa futura dedicada.

**Verificado de punta a punta por HTTP con login real** (2 usuarios de prueba descartables, idEmpresa 1 y 2, creados y borrados): `/MigracionPostgres/CompararEgresoCaja?idEgresoCaja=350` y `?idEgresoCaja=349` (con `TipoEgresoCaja` y `CreadoPor` resueltos) devuelven los mismos campos en SQL Server y Postgres; logueado como tenant 2, el mismo `idEgresoCaja=350` (tenant 1) da "no encontrado" en **ambos** motores por igual (RLS). Harness directo confirmó el autoincremental de `cierrecaja.id` (continúa desde el máximo migrado sin colisión, verificado `id=220000010`) y el reverso de `EgresosCaja`, ambos con `ROLLBACK` sin dejar datos de prueba.

Con esto, `cambiarSucursalCaja`/`obtenerPreviewCambioSucursalCaja`, el resto de `Corte.cs` (Embutido/Movimiento/reportes), el resto de `Venta.cs` (Expendios/Sectores/FacturaElectronica) y `Compra.cs` completo quedan como trabajo pendiente de la migración.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Venta.cs, bloque Ventas/LineaVenta (Etapa 7)

Bloque núcleo transaccional del POS (24 métodos de 41 totales): `Ventas`, `LineaVenta`, `TemporalLineaVenta`. Mismo patrón mecánico: `Contratos/IVentaRepository.cs` -> `Datos.Venta : IVentaRepository` (cero cambios) -> **patrón de dos campos** en `Negocio/Venta.cs` (`oVentaD` interfaz para el batch, `oVentaDSqlServer` concreto para Expendios/Sectores/FacturaElectronica, todavía sin migrar) -> `DatosPostgres/VentaPg.cs`.

**Dos dependencias nuevas encontradas leyendo los SPs reales, no anticipadas en el plan**:
- `agregarVenta` calculaba `diaFestivo` contra la tabla `Feriados`. **Confirmado con el usuario: `Feriados` está obsoleta** (0 filas reales, sin ninguna referencia en el código C#, solo la toca el SP). Se excluye del todo — `VentaPg.agregarVenta` usa `diafestivo = NULL` directo, mismo resultado observable que produce SQL Server hoy (la tabla vacía siempre resuelve NULL ahí también).
- `modificarVenta` genera un asiento inverso en `EgresosCaja` cuando se editan (con `eliminarLineas=true`) las líneas de una venta cta-cte que tenía un egreso previo ligado. `EgresosCaja`/`TiposEgresoCaja` son dominio de `CierreCaja.cs`, sin migrar. **Confirmado con el usuario: este gap es real e importante, no opcional** — queda documentado en `docs/GAPS.md` (nuevo, primer uso de ese archivo en este proyecto) con instrucciones concretas de cómo resolverlo cuando se aborde `CierreCaja.cs`. El resto de `modificarVenta` (borrado de líneas + `UPDATE Ventas`) sí está completo.

**`agregarStockVenta`**: verificado con `sp_helptext` que el SP real solo actualiza `StockCorteSucursal` (cascada de stock del corte vendido, con un bloque "PUCHERO" ya comentado/muerto en el propio SP de origen). Como `StockCorteSucursal` nunca se porta (decisión de la Etapa 6), queda como no-op documentado — mismo criterio ya aplicado, no una improvisación nueva.

**Bug real propio encontrado y corregido antes de cerrar la etapa**: mi primer `VentaPg.obtenerLineasVenta` no replicaba el `INNER JOIN` a `Corte` que tiene el SP real (verificado con `sp_helptext`, no visto en la primera lectura superficial). Descubierto comparando datos reales por HTTP: Venta #23 (tenant 1) tenía 2 líneas (`idLineaVenta` 61 y 64) cuyo `idCorte=3` pertenece a `idEmpresa=3` (dato cruzado/viejo de otro tenant) — SQL Server las excluye silenciosamente porque el `INNER JOIN` a `Corte` queda vacío para ese `idCorte` bajo RLS del tenant 1; mi primera versión las incluía igual con `Corte=null`. Corregido agregando el mismo `INNER JOIN` (con el mismo efecto de exclusión automática vía RLS de Postgres). Verificado tras el fix: conteos y contenido idénticos en ambos motores para las 2 ventas de prueba.

**Verificado de punta a punta por HTTP con login real** (2 usuarios de prueba descartables, idEmpresa 1 y 2, creados y borrados): `/MigracionPostgres/CompararVenta?idVenta=23` (15 líneas tras excluir las 2 cruzadas) y `?idVenta=18` (15 líneas, incluye cantidades negativas de anulación) devuelven exactamente los mismos campos en SQL Server y Postgres; logueado como tenant 2, la misma `idVenta=23` (tenant 1) da "no encontrado" en **ambos** motores por igual (RLS). Harness directo (`psql`, transacción con `ROLLBACK`) confirmó el camino de escritura de `agregarVenta`+`agregarLineaVenta` (INSERT multi-statement + `agregarLineaVenta` con `RETURNING`) sin dejar datos de prueba.

Con esto, `Venta.cs` (Expendios/LineaExpendio, Sectores, FacturaElectronica), el resto de `Corte.cs`, `CierreCaja.cs` y `Compra.cs` completos quedan como trabajo pendiente. `docs/GAPS.md` queda como inventario vivo del reverso de `EgresosCaja`, a resolver obligatoriamente en la etapa de `CierreCaja.cs`.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Corte.cs, bloque CRUD/referencia (Etapa 6)

Primera clase "grande" abordada, solo el bloque CRUD/referencia (24 de los ~78 metodos de `Datos/Corte.cs`): Corte (find/add/edit/delete/buscar), `ActualizacionCorte` (historial, ver correccion mas abajo), `CatalogoGlobalImportacionProductos` (staging de importacion), `Formulas`/`CortePorFormula`, `AlicuotasIva`, `TiposProducto`. Mismo patron mecanico: `Contratos/ICorteRepository.cs` (solo el batch, no las ~78 metodos totales) -> `Datos.Corte : ICorteRepository` (cero cambios) -> constructor aditivo en `Negocio.Corte` -> `DatosPostgres/CortePg.cs`.

**Complicacion nueva de este patron**: a diferencia de las etapas anteriores (una clase migrada entera de una vez), acá la interfaz cubre solo una FRACCION de `Datos.Corte`. El campo `oCorteD` de `Negocio/Corte.cs` no se pudo simplemente retipar a `ICorteRepository` porque la clase usa ese mismo campo para ~37 metodos fuera del batch (Embutido, Movimiento, reportes). Solucion: dos campos -- `oCorteD` (`ICorteRepository`, el batch migrado, puede ser SQL Server o Postgres) y `oCorteDSqlServer` (`Datos.Corte` concreto, el resto de la clase, siempre SQL Server en las dos constructores). Los ~37 call-sites de metodos fuera de alcance se redirigieron a `oCorteDSqlServer` mecanicamente. **Regla para las proximas etapas de clases grandes** (Venta.cs, CierreCaja.cs, Compra.cs, y el resto de Corte.cs): si la migracion es parcial, el patron de "campo unico retipado a la interfaz" de las etapas 2-5 no alcanza -- hace falta el patron de dos campos desde el arranque.

**Bug real encontrado y corregido en el camino**: `docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql` tenia el cuerpo de `addOrEditCorte` **truncado** (cortado a mitad de un comentario, con logica real despues del corte). Se trajo el cuerpo real con `EXEC sp_helptext 'dbo.addOrEditCorte'` contra la base viva -- reveló que ese SP (el que usa `Datos/Corte.cs` de verdad en cada alta/edicion) **tambien inserta en `ActualizacionCorte`**, tabla que la entrada de triage del 2026-08-18 habia marcado como muerta (solo habia verificado el SP `modificarCorte`, sin callers reales, y no vio que `addOrEditCorte` la escribe tambien). Confirmado contra la base viva: **244 filas reales**, RLS activo, ultima escritura 2026-08-11. Se migra junto con este batch (no es opcional): `CortePg.addOrEditCorte` replica el insert de auditoria igual que SQL Server. `Parametros_old` y `Claves` siguen confirmadas sin uso real, esa parte del triage no cambia. **Leccion**: el snapshot de SPs puede estar truncado o desactualizado para SPs individuales -- para cualquier SP que determine el diseño de una migracion, traer el cuerpo real con `sp_helptext` contra la base viva antes de confiar en el snapshot.

**Calculo de "Nivel"**: `addOrEditCorte` y `obtenerNivelCorte` comparten (verbatim) una logica de calculo de profundidad jerarquica via `idCorteMaestro`, con un tope deliberado de 4 niveles (subqueries anidadas, no recursion real). Se replico exacto en Postgres (`CortePg.CalcularNivel`, reusada por ambos metodos) -- no se "mejoro" a una CTE recursiva infinita, que hubiera sido un cambio de comportamiento no pedido.

**`StockCorteSucursal` (decision confirmada con el usuario)**: se confirmo con datos reales que la tabla tiene 0 filas y ningun lector/escritor real en C# (solo una variable WinForms nunca persistida en `Presentacion/Caja/formPOS.cs`) -- esta obsoleta. Decision: **nunca se porta a Postgres, en ninguna etapa futura** (incluido el cascade de Embutido/Movimiento cuando se aborde). Las SPs de SQL Server que la tocan (`EliminarCorte` y las del cascade) quedan intactas -- el usuario decidio explicitamente no tocar SQL Server, su codigo muerto sobre esa tabla no tiene efecto real. `CortePg.eliminarCorte` implementa el borrado real del Corte sin el paso de `StockCorteSucursal` (confirmado no-op, no un gap).

**Fuera de alcance de esta etapa, documentado en el codigo**: Embutido, Movimiento, cascade de stock, reportes (`CierreStock*`, `acum_Ventas`, `Balance`, dashboards), y `obtenerCorteProveedor`/`obtenerCortesPorProveedor` (dependen de `CorteProveedor`/`Compras`/`CortePorCompra`, dominio de `Compra.cs`, no migrado). Ninguno esta en `ICorteRepository` todavia -- se agregan cuando se aborden esos bloques, no son un olvido.

**Verificado de punta a punta por HTTP con login real** (2 usuarios de prueba descartables, idEmpresa 1 y 2, creados y borrados): `/MigracionPostgres/CompararCorte?idCorte=2` (con Marca resuelta via `IPersonaRepository` inyectado) y `?idCorte=21` (con CorteMaestro resuelto recursivamente, Nivel=1) devuelven exactamente los mismos campos en SQL Server y Postgres; logueado como tenant 2, el mismo `idCorte=2` (tenant 1) da "no encontrado" en **ambos** motores por igual (RLS).

Con esto, el resto de `Corte.cs` (Embutido, Movimiento, cascade de stock, reportes), `Venta.cs`, `CierreCaja.cs` y `Compra.cs` completos quedan como el trabajo pendiente de la migracion.

## 2026-08-18 - Corrección: `ActualizacionCorte` NO está muerta, la entrada del triage del 2026-08-18 estaba incompleta

Al leer `dbo.addOrEditCorte` completo (con `sp_helptext` contra la base viva, no el snapshot -- ver más abajo por qué) para la Etapa 6 (`Corte.cs`), se encontró que ese SP -- el que de verdad usa `Datos/Corte.cs.addOrEditCorte`, llamado en cada alta/edición real de un Corte -- **también inserta en `ActualizacionCorte`** en su rama de edición. La entrada de triage del 2026-08-18 más abajo ("3 tablas excluidas... `ActualizacionCorte` (unico SP que la toca, `modificarCorte`, invocado por nadie)") solo verificó `modificarCorte` (ese sí, confirmado sin callers) y no vio que `addOrEditCorte` la escribe también -- un gap real en esa verificación, no un cambio de comportamiento del sistema.

**Confirmado contra la base viva**: `ActualizacionCorte` tiene **244 filas reales**, última escritura 2026-08-11, y **RLS activo** (aparece en `sys.security_predicates`). No es una tabla muerta: es un historial write-only (nadie la lee de vuelta en el código, pero se escribe en cada edición real de un Corte).

**Decisión (confirmada con el usuario)**: se migra junto con el batch CRUD de `Corte.cs` (Etapa 6) -- `CortePg.addOrEditCorte` replica el insert de auditoría igual que el SQL Server real, aunque nada la lea nunca del lado Postgres tampoco. `Parametros_old` y `Claves` siguen confirmadas sin uso real, esa parte del triage original no cambia.

**Lección para las próximas etapas**: `docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql` puede estar **truncado o desactualizado** para SPs individuales (se encontró `addOrEditCorte` cortado a mitad de un comentario en el snapshot, con contenido real después del corte). Para cualquier SP que vaya a determinar el diseño de una migración, traer el cuerpo real con `EXEC sp_helptext 'dbo.<nombre>'` contra la base viva antes de confiar en el snapshot -- el snapshot sirve para descubrir/grep nombres, no como fuente de verdad del cuerpo completo.

## 2026-08-18 (la mas reciente) - Migracion SQL Server -> PostgreSQL: CuentaCorriente completa (Etapa 5)

Migradas `MovCtaCte`, `Pagos`, `Cheques`, `Bancos` (con RLS) + `Usuarios` (tabla de apoyo, sin RLS, mismo criterio que `Iva`/`Empresas`), completando `Datos/CuentaCorriente.cs`. Mismo patron mecanico de las etapas anteriores: `Contratos/ICuentaCorrienteRepository.cs` -> `Datos.CuentaCorriente : ICuentaCorrienteRepository` (cero cambios de comportamiento) -> constructor aditivo en `Negocio.CuentaCorriente` -> `DatosPostgres/CuentaCorrientePg.cs`.

**Bug propio detectado y corregido antes de cerrar la etapa**: la primera version de `getCtaCteByIdPersona` en Postgres se escribio sin haber leido el SP real (`dbo.getCtaCteByIdPersona`) -- violacion de la regla de "no inventar" (CLAUDE.md 2.7). Corregido leyendo el SP real desde `docs/08-relevamiento/snapshot-2026-08-18/stored-procedures.sql` (linea 3743) y verificando su comportamiento en vivo contra SQL Server: la primera rama del UNION usa literales `'-'` para las columnas de texto y, por una rareza real de SQL Server (`CAST('-' AS INT)` evalua a `0` en vez de tirar error de conversion, confirmado empiricamente, no documentado por Microsoft), esos mismos literales se convierten a `0` en las columnas int. Postgres no tiene esa conversion implicita -- se reprodujeron los valores observados (`0`/`'-'`) directamente en el SQL nuevo en vez de portar la sintaxis. Verificado con paridad exacta contra la base real (idPersona=22, fechaDesde=2026-06-01): mismo conteo de filas (13), mismo importe de "Saldo Anterior" (300266.53...) y mismo total agregado (-2655150.4690895081) en ambos motores.

**Alcance deliberadamente mas angosto en un punto, documentado en el codigo**: `getChequePorIDorNro`/`getChequesPorPago` en SQL Server resuelven `CreadoPor`/`ActualizadoPor` via `Datos.Usuario.getUsuarioById`, que ademas carga `Sucursal`+`Empresa` anidada del usuario. `CuentaCorrientePg` usa en cambio el mismo patron "liviano" que ya usa el resto de la clase (`MapUsuarioLiviano`, sin esa carga anidada) para los 19 metodos. No afecta el harness de comparacion de esta etapa (no se ejercita ese campo en profundidad).

**`eliminarPago`**: `NotImplementedException` documentando el bug real preexistente ya confirmado en la etapa de diseño (el SP `eliminarPago` no existe en SQL Server, solo alcanzable desde `Presentacion/` WinForms, fuera de alcance).

**Deuda documentada, no bloqueante**: los alias de columna de los metodos que devuelven `DataTable` crudo (`obtenerCtasCtes`, `obtenerResumenDashboard`, `obtenerCheques`, `obtenerPagos`, `obtenerTotalesPagosBalance`, `obtenerUltimosPagosDashboard`, `obtenerChequesPendientesDashboard`) quedaron en minusculas/snake_case en Postgres, sin verificar contra el nombre exacto (con espacios/mayusculas, ej. `"Nombre Identif."`, `"obs."`) que devuelve SQL Server. Marcado con `TODO(claude)` en `CuentaCorrientePg.cs`: no importa mientras ninguna View real consuma esta clase, hay que revisarlo antes de conectar una.

**Hallazgo operativo de la sesion, no relacionado a Postgres**: al preparar la prueba HTTP se detecto que la base local real que usa la app es `CarniSys` (`Initial Catalog=carnisys` en `connectionStrings.config`), no `SuperCerdo` (esa es la base del servidor remoto San Martin) -- un chequeo inicial contra la base equivocada hizo sospechar un problema de datos que no existia. Ademas, `MovCtaCte`/`Pagos`/`Cheques`/`Bancos` tienen RLS real activo en `CarniSys` (fail-closed): una sesion de `sqlcmd` por Windows Auth sin bypass ve 0 filas en esas tablas aunque tengan datos reales -- hace falta `EXEC sys.sp_set_session_context @key=N'EsAdminCarniSys', @value=1` para diagnosticar por fuera de la app. Datos reales confirmados en `CarniSys`: Personas=24, Sucursal=9, Bancos=35, Pagos=60, MovCtaCte=229, Cheques=16 (coincide exacto con lo migrado a Postgres esta etapa).

**Verificado de punta a punta por HTTP con login real** (2 usuarios de prueba descartables, creados y borrados: `test_piloto_pg5` idEmpresa=1 superadmin, `test_piloto_pg5_t2` idEmpresa=2 superadmin): `/MigracionPostgres/CompararPago?idPago=13` y `?idPago=27` (tenant 1, con 3 cheques relacionados cada uno) devuelven exactamente los mismos campos en SQL Server y Postgres, incluidos los cheques y la persona relacionada (resuelta via `IPersonaRepository` inyectado, sin instanciar `PersonaPg` propia); logueado como tenant 2, el mismo `idPago=13` (tenant 1) da "no encontrado" en **ambos** motores por igual (RLS bloqueando el cruce de tenant de forma identica). Bug de sintaxis Razor encontrado y corregido en el camino (mismo patron ya documentado: `@if` dentro de un bloque `else { }` ya en contexto de codigo, en `CompararPago.cshtml`).

Con esto, las 3 clases grandes restantes (`Corte.cs`, `Venta.cs`, `CierreCaja.cs`) y `Compra.cs` (por su acople a `Corte`/`CorteProveedor`) quedan como el trabajo pendiente de la migracion, a abordar en una sesion dedicada.

## 2026-08-18 - Migracion SQL Server -> PostgreSQL: triage de las 38 tablas RLS y exclusion de 2 tablas muertas

A pedido del usuario ("repetir en las 32 tablas ahora mismo"), se hizo un relevamiento del resto de las tablas con RLS antes de asumir que se puede repetir el patron de Persona/Sucursal mecanicamente. Hallazgo: son **39 tablas RLS reales** (confirmado contra `sys.security_predicates` de la base viva, no contra un conteo manual previo que no cuadraba dos veces seguidas).

**Triage**:
- **3 candidatas reales de bajo riesgo, mismo patron ya probado 2 veces**: `Conexiones` (ya vive en `Datos/Sucursal.cs`, el archivo ya migrado, la interfaz ya la declaraba), `Licencias` + `VencimientosLicencia` (`Datos/OtrasClases.cs`, chico y acotado). Se migran a continuacion de esta entrada.
- **`Formularios` diferida**: vive en `Datos/Usuario.cs` (30KB, clase multi-tabla que mezcla Usuarios/Formularios/PermisosUsuarios/tokens/login-log) con un solo metodo de lectura -- extraer una interfaz para toda esa clase ahora seria repetir el error de meterse en una clase grande sin plan dedicado. Queda para cuando se aborde `Usuario.cs` junto con las clases grandes.
- **3 tablas excluidas de la migracion, confirmado sin uso real en todo el repo**: `Parametros_old`, `ActualizacionCorte` (unico SP que la toca, `modificarCorte`, invocado por nadie), y `Claves` (encontrada al leer `Datos/OtrasClases.cs`: el metodo `Login(string clave)` que la consulta no lo llama nadie -- ni `Negocio/OtrasClases.cs` lo expone, ni hay ningun otro caller en el repo). Verificado con grep sobre **todo** el repo (`Web/`, `Presentacion/`, `wsAFIPvs2008/`, `Datos/`, `Negocio/`, los 117 SPs relevados) buscando cada nombre de tabla/SP/metodo como string literal -- cero referencias reales en los 3 casos (el unico match aparente, `Presentacion/Cortes/formCortes.cs` metodo `modificarCorte()`, es un nombre de metodo de WinForms coincidente, sin relacion). **Decision**: las 3 quedan afuera de la migracion. Si en el futuro aparece un uso real de alguna, se migra en ese momento, no antes.
- **~32 tablas restantes** viven mezcladas dentro de 5 clases grandes (`Datos/Corte.cs` 90KB/~15 tablas, `Datos/Venta.cs` 81KB/~13, `Datos/CierreCaja.cs` 55KB/~11, `Datos/CuentaCorriente.cs` 47KB/4, `Datos/Compra.cs` 30KB/3) mas `Formularios` en `Datos/Usuario.cs` -- **no es el mismo patron mecanico**, cada clase grande es su propio proyecto de scoping (comparable en esfuerzo a varias entidades como Persona juntas). Quedan para otra sesion, empezando por las menos entreveradas (`CuentaCorriente.cs`, `Compra.cs`) antes de las 3 peores (`Corte.cs`, `Venta.cs`, `CierreCaja.cs`). `StockCorteSucursal` es el caso mas extremo: sin un solo punto de entrada por nombre, repartida en SPs de stock invocados desde los 4 archivos grandes a la vez -- se planifica junto con esos 4, no aparte.

## 2026-08-18 - Migracion SQL Server -> PostgreSQL: estrategia de pooling de conexiones Npgsql (cierre del ultimo pendiente)

Ultimo pendiente de la lista original de la Etapa 2 (ver `docs/06-datos-e-integraciones/rls-postgres.md`, seccion "Estrategia de pooling de conexiones", con el detalle completo). Resumen:

- Verificado contra la documentacion oficial de Npgsql (no de memoria): `Pooling=true` y `No Reset On Close=false` son los defaults -- Npgsql ya poolea conexiones y resetea su estado al devolverlas al pool. El diseno actual (`SET LOCAL` via `set_config(..., true)` dentro de una transaccion que siempre termina en COMMIT/ROLLBACK antes de soltar la conexion, en `DatosPostgres/ConexionPg.cs`) ya es seguro con el pooling nativo de Npgsql tal cual esta, sin agregar nada.
- **Riesgo real encontrado**: `Maximum Pool Size` default de Npgsql es 100, igual que `max_connections` default de Postgres -- el pool de la app sola podria agotar todas las conexiones del servidor. Aplicado: `Maximum Pool Size=30` explicito en el connection string (`Web/Config/connectionStrings.config` y su `.example`), a ajustar con trafico real medido.
- **No hace falta PgBouncer para el despliegue actual** (un solo servidor de aplicacion). Se revisita solo si se escala a multiples instancias del proceso o si `pg_stat_activity` muestra contencion real. Si se adopta, tiene que ser modo `transaction` (nunca `session` ni `statement` -- `ConexionPg.AbrirConTenant` corre 2 statements en la misma transaccion, `statement` lo rompe).

Con esto se cierran los 3 pendientes que quedaban de la lista original de la migracion (Etapa 2): prueba end-to-end por HTTP, segunda entidad, y estrategia de pooling.

## 2026-08-18 - Migracion SQL Server -> PostgreSQL: segunda entidad, Sucursal (Etapa 3)

A pedido explicito del usuario, se repitio el patron de la Etapa 2 con una segunda entidad para confirmar que escala mas alla de un solo caso. `Sucursal` fue la otra candidata ya evaluada (menos SPs que `Persona` -- de hecho cero, todo SQL inline -- pero mas puntos de instanciacion: 53 `new Negocio.Sucursal(` + 4 `new Datos.Sucursal(` directos que saltean `Negocio.Sucursal`, ninguno de los dos grupos se toco).

**Mismo patron mecanico que Persona**: `Contratos/ISucursalRepository.cs` (10 metodos, espeja `Datos.Sucursal` exacto) -> `Datos.Sucursal : ISucursalRepository` (cero cambios de comportamiento) -> constructor aditivo en `Negocio.Sucursal` (el viejo queda intacto, los 53+4 call-sites existentes no cambian) -> `DatosPostgres/SucursalPg.cs` (6 metodos reales + 4 `NotImplementedException`).

**Alcance de `SucursalPg`**: reales `obtenerSucursales`, `findAll`, `findById` (con el join a `Empresas` para la propiedad `.Empresa`, igual que `Persona.findById` con `Iva`), `findEmpresaById`, `findEmpresaByCuit`, `ActualizarDatosBasicos`. Marcados `NotImplementedException`: `obtenerSucursalSanMartin`/`obtenerSucursalSanLorenzo` (hardcodean `idSucursal=2`/`idSucursal=1` -- confirmado en `Entidades/Sucursal.cs` que esos IDs mapean a los puntos de venta AFIP de los servidores legacy San Martin/San Lorenzo) y `obtenerConexiones`/`getIdSucursalByConexion` (tabla `Conexiones`, misma topologia legacy) -- los 4 estan atados a los 3 servidores excluidos desde la Etapa 1, no se inventa nada.

**`Empresas` entra como tabla de apoyo** (igual que `Iva` con `Persona`), sin RLS (confirmado desde la Etapa 1, tabla maestra). Sin FK `sucursal.idempresa -> empresas.idempresa` en el schema Postgres: SQL Server tampoco la tiene (fidelidad al esquema real). Migracion de datos real: 7 filas de `Empresas` + 9 de `Sucursal` desde la base local, IDs preservados -- confirmado antes de migrar que ninguna fila de `Sucursal` tiene `idEmpresa=0` (no aplica la convencion de "fila global" en esta tabla, a diferencia de `Personas`).

**Verificado de punta a punta por HTTP con login real** (mismo procedimiento que Persona: usuario de prueba descartable creado y borrado, nunca la cuenta real): `idSucursal=1` (tenant correcto) devuelve exactamente los mismos campos en SQL Server y Postgres, **incluida la empresa relacionada cargada via join** (`Empresa.NombreFantasia` = "SuperCerdo" en ambos lados); `idSucursal=3` (otro tenant) queda bloqueada por RLS en los dos motores por igual.

**Aprendizaje de la Etapa 2 aplicado sin tropiezos**: se agregaron `Models\ComparacionSucursalVm.cs` al `<Compile Include>` explicito de `Web.csproj` en el mismo paso en que se creo el archivo (no se repitio el error del 404 por archivo faltante en la lista).

Con esto, el patron `Negocio -> IRepository -> (Datos | DatosPostgres)` queda probado con 2 entidades de perfil bien distinto (una con SPs y logica de negocio real, otra sin SPs pero con muchos mas puntos de instanciacion) -- ambas funcionando identico en SQL Server y Postgres, RLS incluido.

## 2026-08-18 - Prueba end-to-end via HTTP de /MigracionPostgres/Comparar: resuelta, causa raiz era otra

Continuacion inmediata de la entrada siguiente (piloto de Persona, Etapa 2). Se probo `/MigracionPostgres/Comparar` de punta a punta con IIS Express + login real por HTTP (usuario de prueba descartable `test_piloto_pg`, id=9999, creado y borrado en la base local -- nunca se toco la cuenta real `ger`).

**Primer intento, bloqueado con 404**: se sospecho un problema de empaquetado (`Web.csproj`, proyecto clasico con `packages.config`, no resuelve las dependencias transitivas de `PackageReference` de `DatosPostgres`/Npgsql, SDK-style). Se corrigieron dos gaps reales en el camino (quedan en `Web.config`, eran necesarios de todas formas):
- Referencia a `netstandard, Version=2.0.0.0` en `<compilation><assemblies>` -- sin esto Razor no compila ninguna vista que use tipos de `Entidades`/`Contratos`/`DatosPostgres` (netstandard2.0), tira `CS0012`.
- `bindingRedirect` para las ~13 dependencias transitivas de Npgsql en `<runtime><assemblyBinding>`.

**Causa raiz real, encontrada sin necesitar permisos de administrador**: no era un problema de binding de ensamblados. Se armo un ejecutable de diagnostico chico (`TestLoader.exe` + `TestLoader.exe.config` copiado de `Web.config`, para que el CLR real respete los mismos `bindingRedirect` sin depender del fusion log de Windows) que cargo `Web.dll` con `Assembly.LoadFrom` real: `GetTypes()` devolvio los 538 tipos **sin ninguna excepcion**, pero `Web.Controllers.MigracionPostgresController` no existia en absoluto. La razon: **`Web.csproj` es un proyecto clasico con lista explicita de archivos (`<Compile Include>`), no un SDK-style con glob automatico** -- `MigracionPostgresController.cs` y `ComparacionPersonaVm.cs` nunca se agregaron a esa lista, asi que MSBuild simplemente no los compilaba (sin error, sin warning). El 404 era un sintoma correcto y esperado: el controller genuinamente no existia en el ensamblado.

**Fix real**: agregar `<Compile Include="Controllers\MigracionPostgresController.cs" />` y `<Compile Include="Models\ComparacionPersonaVm.cs" />` a `Web.csproj`. Con eso, `TestLoader.exe` encontro el tipo (542 tipos, controller resuelto), y la prueba real por HTTP funciono de punta a punta.

**Resultado verificado con 3 casos reales, via HTTP con login real, motor por motor**:
1. `idPersona=13` (tenant 1, sesion tenant 1): SQL Server y Postgres devuelven exactamente los mismos campos (razonSocial, identificacion, iva, etc.).
2. `idPersona=1` (Consumidor Final, `idEmpresa=0`, global): ambos motores lo muestran igual.
3. `idPersona=14` (tenant 2, sesion tenant 1): **ambos** motores devuelven "no encontrada" -- RLS bloqueando el cruce de tenant de forma identica en SQL Server y en Postgres.

**Leccion para la sesion**: antes de asumir un problema de infraestructura complejo (binding de ensamblados), verificar primero lo mas simple (¿el archivo nuevo esta realmente en la lista de compilacion del proyecto clasico?). El diagnostico con `TestLoader.exe`/`.exe.config` (cargar el ensamblado real respetando el `Web.config` real, sin necesitar el fusion log ni permisos de administrador) es reusable para el mismo tipo de problema en el futuro.

**Limpieza**: usuario de prueba `test_piloto_pg` (id=9999) borrado de la base local. IIS Express detenido. `TestLoader.exe`/`.cs`/`.config` borrados de `Web/bin/` (nunca se versionaron, `bin/` esta gitignoreado). Accion diagnostica `Ping()` removida del controller.

**Con esto, el criterio de exito de la Etapa 2 queda completo**: la logica de RLS multitenant funciona igual en SQL Server y Postgres, verificado tanto por fuera de la capa web (harness directo, entrada anterior) como a traves de la aplicacion real via HTTP.

## 2026-08-18 - Migracion SQL Server -> PostgreSQL: piloto real de Persona (Etapa 2)

Continuacion de la Etapa 1 (ver entrada siguiente). Objetivo: llevar el diseno a codigo real con una sola entidad piloto (`Persona`, elegida por representar mejor el patron -- mezcla SP + SQL inline + logica de negocio real, ver razones completas en el plan de la sesion), validando de punta a punta que `Negocio -> IRepository -> (Datos | DatosPostgres)` funciona con RLS incluido, sin tocar ningun llamador existente.

**Correccion de diseno sobre la marcha**: la interfaz de repositorio (`IPersonaRepository`) y la capa Postgres nueva se armaron como proyectos SDK-style apuntando a `netstandard2.0` (no clasicos `.NET Framework 4.7.2`), a pedido del usuario -- hay un plan de migrar el proyecto Web de ASP.NET MVC5 a ASP.NET Core en un futuro no inmediato, y este codigo nuevo queda reusable tal cual ese dia, sin reescribirlo. Esto forzo multi-targetear `Entidades.csproj` (`net472;netstandard2.0`) porque las firmas de la interfaz necesitan `Entidades.Persona` -- confirmado que no tiene ninguna dependencia especifica de `.NET Framework`, cambio mecanico sin tocar ninguna POCO. Se creo un proyecto `Contratos` (netstandard2.0) para alojar las interfaces, en vez de meterlas en `Entidades` -- separa "modelos de datos" de "contratos de repositorio".

**Bug real encontrado en el camino (no relacionado a Postgres)**: `addOrEditPersona` insertaba `idEmpresa=0` (visible a todas las empresas) en toda alta nueva desde `/Personas`, porque el SP nunca recibia `@idEmpresa` desde `Datos/Persona.cs` y su default era `0` en vez de caer a `SESSION_CONTEXT`. Corregido en SQL Server (ver entrada separada mas abajo, mismo dia) antes de seguir con el piloto -- el diseno de `PersonaPg` en Postgres ya nace con el comportamiento correcto (`idEmpresa` siempre sale del tenant de la sesion, nunca de lo que traiga el objeto).

**Alcance deliberadamente acotado de `PersonaPg`**: de los 12 metodos de `Datos.Persona`, solo 6 se implementaron de verdad en Postgres (`findById`, `addOrEditPersona`, `addOrEditPersonaConId`, `eliminarPersona`, `existeCuit`, `getIva`) -- los que tocan solo `Personas`/`Iva`. Los otros 6 (`buscarPersona`, `buscarProveedor`, `personaTieneCompras_Ventas`, `obtenerProveedores`, `obtenerProveedoresConCompras`, `existenMarcasParecidas`) quedan con `NotImplementedException` explicito porque hacen JOIN a `Compras`/`Ventas` (no migradas) o dependen del problema de collation case-insensitive (`LIKE`) que sigue pendiente -- no se inventa un resultado, se marca.

**Migracion de datos real** (primera de este proyecto): las 4 filas de `Iva` y las 24 de `Personas` de la base local de desarrollo se copiaron a Postgres preservando los IDs exactos (`GENERATED BY DEFAULT AS IDENTITY`, no `ALWAYS`, justamente para permitir esto) -- confirmado con el usuario que las filas `idEmpresa=0` existentes (`CONSUMIDOR FINAL`=1, `INDEFINIDO`=3, y 3 mas de datos de prueba) son intencionales y referenciadas por ID en otros lugares. Se sacó la FK `idiva -> iva.id` y se hizo `creado` nullable en el esquema Postgres porque SQL Server tampoco las tiene/exige (fidelidad al esquema real, no una "mejora" de paso).

**Verificado con un harness descartable** (fuera del repo, no commiteado) contra la base `carnisys` real de Postgres: lectura con tenant correcto ve su fila, tenant equivocado no ve nada (RLS bloqueando), la fila global (`idEmpresa=0`) se ve desde cualquier tenant, y un alta nueva sin especificar `idEmpresa` en el objeto queda con el tenant real (nunca 0) -- los 4 casos dieron el resultado esperado.

**Pendiente explicito para la proxima etapa**: probar `/MigracionPostgres/Comparar` de punta a punta vía navegador real (no se pudo en esta sesion por no tener control de UI/login interactivo) -- la logica ya esta verificada por fuera de la capa web con el harness. Extraer mas entidades/interfaces siguiendo el mismo patron. Decidir estrategia real de pooling de conexiones Npgsql para produccion.

## 2026-08-18 - Migracion SQL Server -> PostgreSQL: instalacion local + diseno de RLS (Etapa 1)

Pedido del usuario: empezar a migrar de SQL Server Express a PostgreSQL manteniendo las dos capas de datos coexistiendo desde `Web/` (la actual contra SQL Server, intacta; una nueva contra Postgres), sin tocar `Presentacion/` ni reescribir `Negocio/`. Preocupacion explicita: no romper el multitenant (RLS). Alcance de esta etapa: solo instalacion local + diseno documentado, nada de produccion tocado, ninguna tabla migrada.

**Rama de trabajo**: se sigue trabajando en `codex_ia` (la rama activa real, 772 commits por delante del ancestro comun con `master`, que quedo vieja). Tag de resguardo `pre-postgres-migration-20260818` creado sobre el commit previo a esta etapa.

**Limpieza previa**: se eliminaron del disco `tests/CarniSys.NG.UnitTests/` y `tests/CarniSys.NG.IntegrationTests/` (huerfanos, el `.sln` ya no los referenciaba) -- confirmado con el usuario, quien aclaro que el abandono de CarniSys.NG fue una decision de foco ("no lo iba a utilizar, empezar de cero"), no un problema tecnico de fondo.

**RLS real relevado contra la base viva** (no contra el script desactualizado del repo, `Datos/DB-Procedures/20260521-Create_RLS_Personas_IdEmpresa.sql`, que quedo con el nombre viejo `RLS_Personas_IdEmpresa`): la politica real se llama `RLS_Empresa`, cubre 32 tablas, con FILTER+BLOCK via `fn_rls_empresa_o_global_v2`/`fn_rls_block_empresa_o_global_v2` -- fail-closed, con bypass de superadmin por login (`cs_admin`) y por flag de sesion (`EsAdminCarniSys`, usado de verdad en `SystemAdministrationRepository.cs`). Detalle completo y mapeo a Postgres en `docs/06-datos-e-integraciones/rls-postgres.md` (nuevo).

**Decisiones tomadas con el usuario**:
- Solo la base multi-tenant `carnisys` (RLS activo) migra. Los servidores legacy SQL Server 2008 sin RLS (San Martin, San Lorenzo) quedan fuera.
- Postgres instalado nativo en Windows (installer EDB 17.11, modo unattended), no Docker.
- El `WITH CHECK` de las policies de Postgres queda **endurecido** respecto al BLOCK actual de SQL Server: una sesion de tenant normal no puede insertar una fila `idEmpresa=0` (hoy en SQL Server el BLOCK lo permitiria, pero no hay ninguna fila asi en uso). Verificado con una prueba de 5 pasos en la base descartable `rls_poc` -- ver `docs/06-datos-e-integraciones/rls-postgres.md`.
- Las 11 tablas con `idEmpresa` pero sin RLS (`Usuarios`, `Empresas`, `PermisosUsuarios`, etc.) quedan sin RLS tambien en Postgres -- son tablas maestras/meta, confirmado sin revision caso por caso.
- **Correccion de diseno importante**: la primera propuesta (controller -> Postgres directo, saltando `Negocio`) fue rechazada por el usuario -- `Negocio` contiene logica de negocio fundamental y nunca se saltea. Diseno corregido: `Negocio` sigue llamando por interfaz (`IRepository` por entidad, patron "extraer interfaz"), y quien varia es la implementacion detras (`Datos` SQL Server o `DatosPostgres` nuevo) -- `Negocio` no sabe ni le importa que motor hay detras. Se hace entidad por entidad, recien cuando esa entidad entre en migracion (no en esta etapa).

**Pendiente para la proxima etapa**: extraccion real de las interfaces (`Contratos`), proyecto `DatosPostgres`, estrategia de pooling de conexiones, y el DDL completo + clasificacion de los 117 SPs (Fase 4 del plan). Ver plan completo en el historial de la sesion -- resumen y detalle tecnico quedan en `docs/06-datos-e-integraciones/rls-postgres.md`.

## 2026-08-15 - Facturar sin venta asociada: venta real minima en vez de una venta en memoria

Pedido del usuario: poder generar una Factura Electronica desde `/Ventas/Facturas` sin tener una venta de productos real detras (solo Cliente + Total + Alicuota).

**Por que no una venta 100% en memoria (propuesta inicial descartada)**: el calculo fiscal que se manda a AFIP (`AFIP/GenerarFacturaService.cs`, `CalcularFiscalAfipConPorcentaje`) siempre suma `CantKg*PrecioKg` de `Venta.LineasVenta`, agrupando por alicuota -- no hay forma de mandarle un Total+Alicuota sueltos sin pasar por una `LineaVenta` real perteneciente a una `Venta` real (con `IdVenta` valido). Reescribir esa logica para aceptar un modo alternativo tocaba codigo fiscal ya probado en produccion -- riesgo desproporcionado para el pedido.

**Decision final, acordada con el usuario en 3 rondas de `AskUserQuestion`**: se crea una **venta real minima** (`FormaPago=Efectivo`, `EnCtaCte=false`) con **una sola linea real**, cuyo `PrecioKg` es el Total que el usuario ingreso a mano (no una linea en 0, eso mandaria una factura de $0 a AFIP). La linea se borra recien **despues** de que la factura ya tiene CAE -- nunca antes, porque `GenerarFacturaService` depende de ella durante la emision. La venta (sin lineas) queda en la base como registro permanente de esa factura.

**Por que `FormaPago=Efectivo` + `EnCtaCte=false`, sin excepcion**: es lo que evita, por construccion, los 2 efectos colaterales peligrosos de las rutas existentes de venta -- `egresoCajaPagoTarjeta` (reversa un egreso de caja si la forma de pago fuera tarjeta) y `crearMovCtaCteVenta` (si `EnCtaCte=true`, un `modificarVenta` posterior con lineas en 0 fuerza el movimiento de cuenta corriente a $0, zanjando silenciosamente la deuda del cliente -- ver `Negocio/CuentaCorriente.cs`). Con Efectivo/sin CtaCte ambas ramas son no-op, sin tener que auditar ni modificar esos metodos.

**Por que no se reusa `modificarVenta(eliminarLineas:true)` para el borrado post-emision**: ese metodo (via el SP `modificarVenta`) reversa egresos de caja y resetea cuenta corriente como side-effect del borrado de lineas -- exactamente lo que se queria evitar. Se escribio un metodo nuevo y minimo, `EliminarLineasVenta`, que solo hace `DELETE FROM LineaVenta WHERE idVenta=@id`.

**Placeholder de producto para la linea temporal: primer producto de la empresa, no el "Corte generico"**. La opcion inicial (`Negocio/Corte.cs ObtenerProductoGenerico()`, ya usado en produccion para "precio libre") se descarto porque depende del parametro `codProdGenerico`, que no todas las empresas tienen configurado -- corregido explicitamente por el usuario durante la revision del plan. Se usa en cambio `ObtenerCortesPorEmpresa(idEmpresa, false).FirstOrDefault()` (metodo ya existente, sin query nueva). Como la alicuota de ese producto casi seguro no coincide con la que el usuario elige en el modal, hace falta un paso extra: ver el siguiente punto.

**Correccion de alicuota post-insert, obligatoria**: verificado contra el SP real de la base local (`sp_helptext agregarLineaVenta`, no el `.sql` historico del repo) que el SP si acepta `@idAlicuotaIva`/`@alicuotaIva` como parametros propios de la linea, pero el wrapper C# (`Datos/Venta.cs agregarLineaVenta`) los hardcodea siempre desde `Corte.IdAlicuotaIva`/`.AlicuotaIva` del producto usado, ignorando cualquier valor puesto en el objeto `LineaVenta`. Por eso, despues de insertar la linea con la alicuota (incorrecta) del producto placeholder, se llama a un UPDATE nuevo y acotado (`ActualizarAlicuotaLineaVenta`) que la corrige a la elegida por el usuario en el modal.

**Riesgo aceptado, no resuelto**: no hay lock especial contra 2 usuarios generando una factura manual al mismo tiempo -- mismo nivel de concurrencia que cualquier alta de venta hoy.

**No probado**: el envio real a AFIP homologacion de punta a punta (se evito para no generar un comprobante fiscal de prueba real); la mecanica de venta+linea+correccion de alicuota+borrado si se verifico directo contra la base local (`docs/09-cambios-y-pendientes/bitacora-de-cambios.md`, entrada del mismo dia).

## 2026-08-14 - Dispositivos seguros por empresa: bypass del bloqueo de IP en login

Pedido del usuario: una pantalla de Configuración para marcar PCs de oficina como "dispositivos seguros" por número de serie, para que loguearse desde ellas no dispare el bloqueo por IP del login.

**Identificador elegido: CPU ID (WMI, `Win32_Processor.ProcessorId`), no número de serie de disco.** El usuario mencionó "disco" como ejemplo, pero el mecanismo real y activo en el sistema (usado en 4 puntos de WinForms, `Utilidades/Util_Form.cs`, `GetCPUId()`) es el CPU ID. El método de disco (`GetHDSerial()`, `VolumeSerialNumber` del volumen `C:`) existe en el mismo archivo pero está muerto/comentado en todo el código. Se usa CPU ID por consistencia con el resto del sistema, confirmado con el usuario antes de implementar.

**Tabla nueva (`DispositivosSeguros`), no se reusa `Licencias`.** `Licencias` es una tabla legacy de WinForms (terminal + sector + posible licenciamiento) con semántica propia -- mezclarla con seguridad del login web arriesgaba tocar algo que WinForms ya lee/escribe (`Presentacion/FormPrincipal.cs`, `Datos/Venta.cs`). Tabla dedicada, por empresa, con `UNIQUE(IdEmpresa, NumeroSerie)`.

**Riesgo de seguridad aceptado explícitamente por el usuario**: el número de serie viaja como un campo de formulario común (`NumeroSerieDispositivo`) en el POST de `/Login` -- el server no puede verificar criptográficamente que vino del agente local real en ese momento. Cualquiera que conozca o adivine un número de serie ya registrado podría mandarlo desde cualquier IP y saltarse el bloqueo. Es una conveniencia para reducir fricción en máquinas de oficina conocidas, no una barrera dura -- decisión tomada con el usuario antes de implementar, no un descuido.

**Alcance del bypass, acotado a propósito**: solo el bloqueo por IP (`LoginRateLimiter`, en memoria). El bloqueo persistente por cuenta (`Usuario.Bloqueado`, 5 errores de contraseña, tarea anterior) sigue aplicando igual sin ninguna excepción -- así un dispositivo seguro comprometido no habilita fuerza bruta ilimitada contra una cuenta puntual.

**PrintAgent como vía de lectura del hardware, con un endpoint nuevo (`GET /device-id`)**: el navegador no puede leer el CPU ID directamente. Se reusa el agente local de impresión ya instalado en las máquinas que imprimen (`127.0.0.1:18777`, antes solo `/health`, `/printers`, `/config`, `/print/expendio`) en vez de construir un segundo agente separado. El query WMI se duplicó inline en `PrintAgent/LocalPrintServer.cs` (no se referenció el proyecto `Utilidades` completo) para mantener el agente mínimo, mismo criterio que ya sigue hoy (única referencia extra antes de esto era `System.Web.Extensions`).

**Carga manual permitida además de la automática**: si el agente no está instalado/corriendo en la máquina del admin, o se quiere registrar un dispositivo distinto al que se está usando, el campo de número de serie queda editable a mano -- no es estrictamente "solo auto-detectado".

## 2026-08-14 - Mis Sucursales: seleccionar ubicación pegando un link de Google Maps

El botón "Usar mi ubicación actual" (geolocalización del navegador, `Web/Views/Sucursal/Editar.cshtml`) depende de que el navegador conceda el permiso de ubicación -- en uso real no le funcionó al usuario. Se agregó una alternativa que no depende de ningún permiso: un campo para pegar un link de Google Maps (o coordenadas sueltas) y un parser 100% client-side que extrae Latitud/Longitud, en orden de prioridad: `!3d<lat>!4d<lng>` (coordenada exacta del pin en URLs de "place"), `@<lat>,<lng>,<zoom>z` (centro del mapa), `?q=<lat>,<lng>` (mismo formato que ya usa el link "Ver en Maps" de la auditoría de accesos), o texto plano `<lat>, <lng>`.

**Alternativas descartadas** (confirmado con el usuario vía `AskUserQuestion`): mapa interactivo embebido con OpenStreetMap/Leaflet (gratis pero más desarrollo, no es "Google Maps" literal) y mapa embebido con Google Maps real (requiere API key de Google Cloud con facturación habilitada -- opción de pago, se ofreció mostrando el trade-off pero no se eligió).

**Límite conocido, no resuelto**: los links **acortados** de Google Maps (`maps.app.goo.gl/...`, `goo.gl/maps/...`) no traen las coordenadas en el propio link -- son un redirect que solo se resuelve pidiéndole la URL real a un servidor, y un parser client-side no puede seguirlos. Se le indica al usuario en el texto de ayuda del campo que pegue el link completo de la barra de direcciones, no el link corto de "Compartir". Si en la práctica es un problema recurrente, la solución sería resolver el shortlink desde el servidor (una request HTTP aparte).

**Bug de implementación encontrado y corregido en el camino**: el primer intento rompió la vista con un `Error del analizador` de Razor -- el símbolo `@` dentro del `<script>` (en un comentario) y dentro de un regex JS (`/@(-?\d+\.\d+).../`) es interpretado por Razor como inicio de código, no como texto/JS literal; hubo que escaparlo como `@@`. Un tercer `@` dentro de un atributo `placeholder` en HTML plano (no dentro de `<script>`) se comportó distinto -- `@@` ahí generó un error de parser distinto en vez de renderizar el literal esperado; se resolvió evitando el símbolo `@` en ese placeholder en particular, en vez de seguir peleando con el escapado en ese contexto.

**Verificado en vivo con Chrome real (CDP)**: los 4 formatos de entrada extraen las coordenadas correctas, un texto no reconocible muestra el mensaje de error sin romper el formulario, y el guardado persiste los valores exactos en `Sucursal.Latitud/Longitud`. El botón "Usar mi ubicación actual" original queda intacto, sin tocar su lógica.

## 2026-08-14 - Bloqueo de cuenta tras 5 errores de contraseña + desbloqueo por email o admin

Pedido del usuario: recuperación de contraseña por email, bloqueo de cuenta tras 5 errores, desbloqueo por email o por un admin, solo para usuarios activos.

**Hallazgo clave antes de diseñar nada**: la recuperación de contraseña por email YA estaba 100% implementada (`LoginController.ForgotPassword`/`ResetPassword`, tokens hasheados en `UsuarioPasswordResetTokens`, `SmtpMailHelper.SendPasswordReset`) -- solo le faltaba el link visible en `Login/Index.cshtml`, agregado en este mismo pase. Lo único nuevo a construir era el bloqueo persistente por cuenta.

**No se reemplazó `LoginRateLimiter`**: ese mecanismo (`Web/Helpers/LoginRateLimiter.cs`) es un límite en memoria, por IP, que se auto-desbloquea a los 15 minutos -- sigue funcionando igual, sin tocar. El bloqueo nuevo (`Usuario.Bloqueado`/`IntentosFallidosLogin`/`FechaBloqueoUtc`, persistente en base) es un mecanismo complementario, por cuenta: protege contra ataques distribuidos por IP rotando contra una cuenta puntual, cosa que el rate limiter por IP no cubre. Los dos conviven a propósito (defensa en profundidad).

**Reuso de la tabla de tokens en vez de una tabla nueva**: `UsuarioPasswordResetTokens` (token hasheado + expiración + un solo uso + `IdUsuario`) ya era genérica -- se le agregó una columna `proposito` (`'reset'` | `'unlock'`) para que sirva también para el link de desbloqueo, en vez de duplicar toda la lógica de generación/hash/expiración/invalidación en una tabla y un flujo aparte.

**Trade-off de seguridad aceptado, no un descuido**: el mensaje de login para una cuenta bloqueada ("Tu cuenta está bloqueada...") es distinto del mensaje genérico de credenciales incorrectas, lo que revela que la cuenta existe -- a diferencia del resto del flujo de login, que nunca distingue "no existe" de "contraseña incorrecta". Es inherente al pedido: si no se le avisa al usuario que está bloqueado, no tiene forma de saber que debe revisar su email. Se acotó el alcance de la revelación lo más posible (solo pasa cuando la cuenta específicamente está bloqueada, no en cualquier intento fallido).

**Anti-spam de mails, pedido explícito**: el mail de desbloqueo se manda una única vez, en el momento exacto de la transición a `Bloqueado=true` (intento número `Security:AccountLockoutMaxAttempts`, default 5). Los intentos posteriores contra una cuenta ya bloqueada entran por un chequeo temprano en `LoginController.Index` (antes de `ValidarUsuarioWeb`) que corta sin volver a incrementar el contador ni reenviar el mail. Intentos contra un usuario/email que no existe en la base tampoco cuentan ni disparan nada (`ObtenerUsuarioPorIdentificador` devuelve `null`).

**Deuda documentada**: si un usuario bloqueado no tiene email cargado, no hay forma de mandarle el link de desbloqueo -- la única vía queda un admin (`UsuariosController.DesbloquearUsuario`). No se resolvió con un mensaje especial en el login (mismo mensaje genérico de "cuenta bloqueada" para todos los casos, para no revelar si tiene o no email cargado).

## 2026-08-14 - Restricciones de login (horario laboral + espacio de trabajo) + auditoría de accesos

Pedido del dueño del negocio: evitar que empleados se loguen fuera de horario y fuera del local físico, y poder ver quién se logueó y desde dónde.

**Hallazgo clave antes de diseñar nada**: la geo-validación de login ("espacio de trabajo") YA estaba completamente implementada en `LoginController.cs` (columnas `Sucursal.Latitud/Longitud/RadioLoginMetros/ValidarUbicacionLogin`, `Usuario.PermitirLoginFueraSucursal`, Haversine, pantalla `ValidarUbicacion.cshtml`), pero apagada (sin coordenadas cargadas, `ValidarUbicacionLogin=0`). Y la tabla `LoginUbicacionLog` ya existía y se escribía, pero solo cuando la geo-validación estaba activa, y no había ninguna pantalla para leerla. El único desarrollo 100% nuevo fue el horario laboral.

**Horario laboral**: a pedido explícito del usuario, es un dato de **empresa** (no por usuario), con **2 jornadas diarias** ("diurno"/"tarde", típico de comercio que cierra al mediodía) — columnas nuevas `Empresas.HorarioDiurnoDesde/Hasta`, `HorarioTardeDesde/Hasta` (`TIME(0)`, default `00:00:00`/`23:59:59` = sin restricción real hasta que el admin las acote). Validado en `LoginController` en el mismo punto donde ya se decide la geo-validación, ANTES de crear sesión (bloqueo duro, sin dejar sesión a medio crear como sí hace el flujo de geo-validación con su pantalla intermedia). Admin siempre exceptuado, mismo criterio que la geo-validación existente. No se toca el rate limiter al bloquear por horario (las credenciales eran correctas).

**Admin de empresa, sin permiso nuevo**: para las pantallas nuevas "Mi Empresa"/"Mis Sucursales" se reusó el patrón ya existente `usuario.Admin && usuario.IdEmpresa == empresa.IdEmpresa` (`ParametrosController.cs`, `WhatsAppController.cs`) en vez de crear un permiso nuevo en `Entidades.Permisos` — es consistente con el resto de la UI, donde `Admin` ya funciona de facto como "admin de la empresa actual" (el rol cross-tenant separado es `superadmin`, otro concepto, gatea `SystemAdministrationController`).

**Auditoría de accesos**: se generalizó `RegistrarLoginUbicacion` para que registre TODO login exitoso (antes solo ocurría dentro del flujo de geo-validación) -- pedido explícito ("todos los logins exitosos, siempre"). La pantalla nueva (`AuditoriaLoginController`) queda gateada estrictamente por `Entidades.Permisos.Usuario.NuevoUsuario` (el permiso real de "crear usuarios", pedido explícito) o Admin -- deliberadamente MÁS estricto que el flag `puedeAdministrarUsuarios` del layout, que también deja pasar con el permiso de solo-ver-usuarios.

**Campos excluidos por diseño** (AFIP/infraestructura del tenant, quedan reservados al super-admin de plataforma): en "Mi Empresa" se excluyen `RazonSocialAfip`, `Cuit`, `Iibb`, `CondicionIVA`, `InicioActividad`, `TenantSlug`, `BasePath`, `EsRRII`, `NombreCertificado_pfx`, `Entorno_HOMO_PROD`, `BaseDatosNombre`, `Activa`. En "Mis Sucursales" se excluye únicamente `CodPuntoVentaAfip` (único campo AFIP de `Entidades.Sucursal`).

**Verificado en vivo, extremo a extremo, con Chrome real (CDP)** contra la base local: horario bloqueando a un usuario no-admin fuera de rango con el mensaje correcto, Admin exceptuado igual, guardado de horario/datos de empresa persistiendo en `Empresas`, guardado de Latitud/Longitud/Radio/ValidarUbicacionLogin en `Sucursal` activando de punta a punta la geo-validación PRE-EXISTENTE (login bloqueado en `ValidarUbicacion` por falta de coordenadas del navegador, mismo comportamiento que ya tenía el sistema), y la pantalla de auditoría mostrando en vivo tanto logins permitidos (con motivo "sin geo-validación requerida") como bloqueados. **Límite no probado**: el botón "usar mi ubicación actual" en sí (la llamada a `navigator.geolocation.getCurrentPosition`) no se pudo simular con datos falsos vía CDP (mismo tipo de limitación ya documentada en esta sesión para diálogos nativos del navegador -- `Browser.grantPermissions`/`Emulation.setGeolocationOverride` no lograron destrabar el prompt nativo) -- el código es una copia literal del mismo patrón ya usado y probado en `Login/Index.cshtml`, pero la prueba en vivo específica de ESE botón queda pendiente de una verificación manual real.

## 2026-08-13 - Usuario "de producción" compartido: sin password en el selector, y sin permiso para Ajuste de Stock

**Contexto**: usuario compartido por empresa (`Usuario.EsUsuarioProduccion`) para la sala de producción, sin acceso a Ventas/Finanzas/Fórmulas, que al guardar en Movimientos/Stock/Elaborados abre un modal para elegir qué empleado real está actuando (ese usuario queda como `CreadoPor`, nunca el usuario de producción). Reusa el modal `_ModalSeleccionUsuario.cshtml`/`seleccion-usuario.js` ya creado para el step-up de Cierre de Caja, que ya soportaba `requierePassword:false`.

**Decisión 1 -- sin contraseña en el selector, riesgo aceptado explícitamente por el usuario**: a diferencia del step-up de Cierre de Caja (que valida la contraseña real del usuario elegido), este selector no pide nada -- doble clic o Enter alcanza. Esto significa que cualquiera con la sesión de producción abierta puede atribuirse cualquier nombre de la lista de usuarios activos de la empresa, sin ninguna verificación de que sea realmente esa persona. Alternativa descartada: pedir contraseña como en Cajas -- explícitamente rechazada por el usuario porque el objetivo es evitar justamente que los empleados tipeen credenciales en cada carga. Se documenta como riesgo aceptado, no como bug pendiente.

**Decisión 2 -- Ajuste de Stock queda fuera del alcance, sin resolver**: `StockController.cs` exige `user.Admin == true` para operar el tipo "Ajuste" (chequeo hardcodeado, no un permiso de formulario). Como un usuario de producción nunca puede ser Admin (se valida server-side, mutuamente excluyente), no puede hacer Ajustes de Stock aunque tenga el resto de los permisos de Stock. No estaba en el pedido explícito del usuario (que nombró Movimientos, Stock -- Ingreso/Egreso/Pesaje/Cierre -- y Elaborados); si en el futuro hace falta habilitarlo, hay que reemplazar ese chequeo de `Admin` por un permiso de formulario propio, no forma parte de este cambio.

**Bloqueo real vs. cosmético**: el límite real de acceso a Ventas/Finanzas/Fórmulas es server-side, en `UsuariosController.GuardarPermisos` -- descarta cualquier permiso de esas categorías al guardar si el usuario destino es de producción, sin importar lo que llegue en el POST. Ocultar los links del menú y la pestaña "Formulas" es solo UX (evita clics muertos); si algún día se agrega una acción nueva sin pasar por ese guardado, el límite real sigue vigente porque cada acción de Ventas/Finanzas ya valida su propio permiso individualmente (no hay gate de clase en esos controllers, confirmado al investigar).

## 2026-08-13 - Replicado el fix de `obtenerCompras` (idPesajeAjustado) en los 3 servidores remotos

Pedido explícito del usuario: aplicar en producción **solo el cambio de base de datos** de hoy (el `ALTER PROCEDURE` sobre `obtenerCompras`, ver entrada anterior), no el código de la app -- eso no fue pedido y queda sin desplegar.

**Investigación previa (solo lectura, contra los 3 servidores) antes de tocar nada**: confirmé que la migración de junio (`20260620-Alter_Compras_IdPesajeAjustado.sql` -- agrega la columna `idPesajeAjustado` + actualiza `addOrEditCompra`/`agregarCompra`/`modificarCompra`) **ya estaba aplicada en los 3** (columna presente, SPs de escritura ya actualizados; ServidorSM ya tenía 1778 filas reales usando el campo). Solo faltaba el `ALTER` de hoy sobre `obtenerCompras`. Verifiqué además, con `sp_helptext` + diff normalizado contra la versión local pre-cambio, que el cuerpo de `obtenerCompras` en los 3 servidores era byte-idéntico al que parcheé localmente -- sin eso no hubiera aplicado el script a ciegas.

**Servidores y catálogos reales** (documentado para referencia futura, no estaba escrito en ningún lado):
- **ServidorSM** (`192.168.0.151`, LAN): catálogo `supercerdo`, SQL Server 2008, alcanzable directo por red (`192.168.0.151\sqlexpress`).
- **San Lorenzo** (`200.107.108.44`, IP pública): catálogo `supercerdo`, SQL Server 2008, puerto SQL expuesto directo a internet (sin túnel) -- alcanzable igual que ServidorSM.
- **VM CarniSys** (`179.43.118.202:2222` SSH): catálogo `carnisys` (multi-tenant, RLS activo vía `SESSION_CONTEXT('IdEmpresa')`, mismo mecanismo que la base local). A diferencia de los otros dos, el puerto SQL **no** está expuesto a la red -- hubo que subir el script por SFTP y ejecutarlo con `sqlcmd` corriendo en la propia VM vía SSH.

**Aplicado y verificado en los 3** (occurrences de `idPesajeAjustado` en la definición = 30, más un `EXEC obtenerCompras` de humo devolviendo filas reales sin error en cada uno). Sin backup completo de base -- justificado porque es un solo Stored Procedure sin tocar datos/tablas, con el `sp_helptext` original de cada servidor guardado de antemano como rollback inmediato si hiciera falta.

**Deuda explícita, no resuelta**: el código C# de hoy (badges de vinculación en `/Stock`, fix del bug de desvinculación silenciosa) sigue sin desplegarse a ningún servidor remoto -- las bases ya tienen el dato disponible, pero ningún sitio real lo muestra todavía en la UI. Deploy de código pendiente, no pedido en este pase.

## 2026-08-13 - /Stock: identificar vinculaciones Ajuste↔Pesaje↔Compra↔Pesaje-padre + fix de bug real de desvinculación silenciosa

**Pedido**: poder identificar, mirando la tabla y el detalle de `/Stock`, cuándo un registro está vinculado a un pesaje -- Ajuste mostrando su Pesaje (y viceversa, que ya andaba), Pesaje vinculado a otro Pesaje (padre-hijo) distinguido de vinculado a una Compra real, y un Pesaje mostrando qué otros pesajes tiene vinculados a él.

### Causa raíz #1 (bloqueaba todo lo demás): el SP `obtenerCompras` no proyectaba `idPesajeAjustado`

El código para mostrar estos vínculos en la grilla YA EXISTÍA (`StockController.cs`, `_StockTabla.cshtml`) de un commit de junio, pero el Stored Procedure `[dbo].[obtenerCompras]` que alimenta la grilla principal **nunca seleccionaba la columna `idPesajeAjustado`** en ninguno de sus 14 `SELECT` (7 tipos de compra × 2 ramas `@idSucursal>0`/`ELSE`) -- verificado con `sqlcmd`/`sp_helptext` contra la base real, no supuesto. El helper `LeerIntNullable` tiene un guard `Columns.Contains(...)` que hacía fallar esto en silencio, sin excepción -- por eso el usuario veía el dato faltante en la tabla aunque el código para mostrarlo estuviera escrito. El detalle expandible AJAX y la pantalla de edición SÍ tenían el dato bien (usan `findById` = `SELECT * FROM Compras`, sin ese problema).

**Fix**: nuevo `Datos/DB-Procedures/20260813-Alter_obtenerCompras_IdPesajeAjustado.sql` -- `ALTER PROCEDURE` con el cuerpo exacto extraído por `sp_helptext` antes del cambio, agregando `dbo.Compras.idPesajeAjustado` a cada uno de los 14 `SELECT` y su `GROUP BY` correspondiente. Verificado mecánicamente que no se alteró nada más: se normalizó el original y el nuevo (quitando la columna agregada) y se diffearon -- 0 diferencias de contenido, solo el rewrapping de líneas que hace `sp_helptext`.

### Hallazgo no pedido: `SuperCerdo` (base separada, con datos reales de un cliente) nunca recibió la migración de junio

Durante la verificación encontré que el servidor SQL local tiene una base `SuperCerdo` (10.922 compras reales, la que veníamos usando para pruebas de UI en sesiones anteriores) donde la columna `idPesajeAjustado` **no existía en absoluto** -- el ALTER de junio (`20260620-Alter_Compras_IdPesajeAjustado.sql`, agrega la columna + actualiza `addOrEditCompra`/`agregarCompra`/`modificarCompra`) nunca se había aplicado ahí. Confirmado con el usuario, apliqué ambos scripts (el de junio + el de hoy) contra `SuperCerdo` con su autorización explícita. **Después, el usuario pidió no seguir tocando `SuperCerdo`** y aclaró que el ambiente real de trabajo es `CarniSys` -- las migraciones ya aplicadas en `SuperCerdo` quedan (son aditivas, no se revierten), pero no se hicieron más cambios ahí.

**Corrección de un error mío en el camino**: en un primer chequeo concluí que `CarniSys` estaba vacía (`SELECT COUNT(*) FROM Compras` = 0) -- error mío: `CarniSys` es la base multi-tenant real del producto (RLS activo, `RLS_Empresa`, sobre `Compras`/`Sucursal`/`Personas`, filtrando por `SESSION_CONTEXT('IdEmpresa')`), y una conexión sin ese contexto seteado ve 0 filas aunque haya datos reales. Con `EXEC sys.sp_set_session_context @key=N'IdEmpresa', @value=1` sí aparecieron 58 compras reales de esa empresa. Toda la verificación final de este pase se hizo contra estos datos reales de `CarniSys`, no contra datos sintéticos completos (solo se insertó 1 fila sintética puntual para el caso "pesaje padre + hijos vinculados", que no existía naturalmente en los datos, y se borró al terminar).

### Diseño: nuevos campos y su uso

- `CompraIndexDetalleVm`/`StockEditVm` -- nuevo `bool CompraVinculadaEsPesaje`: distingue si el target de `IdCompraVinculada`/`IdPesajeAjustado` (mismo campo físico `idPesajeAjustado`, reusado para 3 relaciones distintas) es otro Pesaje (padre) o una Compra real (Cortes/MediaRes) -- se resuelve comparando el `tipoCompra` de la fila/entidad relacionada con `EsPesaje(...)`, helper ya existente. Badge nuevo `badge-warning "Pesaje padre #X"` en la tabla y título condicional en el detalle/edición, sin tocar el caso ya-correcto `badge-primary "Compra #X"`.
- `CompraIndexDetalleVm.PesajesHijosVinculadosIds` (`List<int>`) -- pesajes cuyo `idPesajeAjustado` apunta a este registro. Nuevo método batch `Datos/Compra.cs` `obtenerPesajesVinculadosPorDestinos(IEnumerable<int>)` (mismo patrón de lotes de 900 que `getIdsAjustePorPesajes`), usado una sola vez por toda la grilla (`ConstruirDetallesIndex`); el detalle AJAX (una sola fila) usa el método singular ya existente (`obtenerPesajesVinculadosPorDestino`). Badge nuevo `badge-secondary "N pesajes vinculados"` (tooltip con los ids) + sección propia en el detalle expandido.

### Bug real encontrado en vivo por el usuario (no introducido hoy, mis badges lo hicieron visible): guardar un Pesaje destino desvinculaba en silencio TODOS sus hijos

El usuario probó vincular una compra a un pesaje y notó que el badge "pesaje padre" de OTRO pesaje desapareció -- quedó huérfano. Causa: `StockController.cs` `SincronizarPesajesVinculados` corre en **cada guardado** de un Pesaje (no solo al usar el botón "Vincular pesajes"), y desvincula (`idPesajeAjustado = NULL`) todo id que esté en `idsPrevios` (los hijos reales, leídos de la base) pero no en `idsActuales` (`model.PesajesVinculadosIds`). El problema: `PesajesVinculadosIds` **nunca se precargaba desde el servidor** al abrir la pantalla de editar -- arrancaba vacío salvo que el usuario usara el modal "Vincular pesajes" en esa misma sesión. Resultado: abrir un Pesaje que ya tenía hijos y guardar por cualquier motivo no relacionado desvinculaba todos sus hijos existentes, en silencio.

**Fix, con confirmación explícita del usuario dado el impacto** (3 capas, no alcanzaba con una sola):
1. `StockController.CrearViewModelEdicion`: precarga `model.PesajesVinculadosIds = oCompraN.obtenerPesajesVinculadosPorDestino(compra.IdCompra)` cuando es un Pesaje.
2. `Editar.cshtml`: pasa ese valor al JS como `pesajesVinculadosExistentes` en la config de `StockUI.init(...)` -- en LOS 2 bloques de init (AJAX-modal y layout completo; el primer intento solo tocó uno por una diferencia de indentación en el `old_string` del reemplazo, encontrado al verificar en vivo por navegación directa que usa el segundo bloque).
3. `stock.js` `getPesajesVinculados()`: ahora es la UNIÓN de `state.config.pesajesVinculadosExistentes` (lo ya vinculado, servidor) con lo derivado de `state.lineas[].idPesajeVinculado` (lo vinculado en esta sesión) -- así `SincronizarPesajesVinculados` nunca encuentra hijos reales ausentes de `idsActuales`, sin importar si el usuario tocó el modal o no.

**Por qué no alcanzaba con precargar solo el ViewModel**: el mecanismo de "vincular pesajes" existente NO vincula el registro en abstracto -- absorbe las líneas/cortes del pesaje origen dentro del destino (`vincularPesaje()` en `stock.js`), y el rastro de "qué línea vino de qué pesaje" (`IdPesajeVinculado` en `StockLineaVm`) es deliberadamente efímero: `StockController.cs` lo resetea a `null`/`""` en cada carga (no hay columna persistida para eso en `CortePorCompra`). Por eso `rebuildHiddenInputs()` (que corre en CADA render de líneas, incluida la carga inicial de página) recalculaba `PesajesVinculadosIds` desde cero en cada request y pisaba cualquier precarga que no pasara también por el JS.

### Deuda/nota

Regla de cache-busting (`docs/DECISIONS.md` 2026-08-10) aplicada: `stock.js` bumpeado a `?v=39` en `Editar.cshtml` (2 referencias) tras el cambio en `getPesajesVinculados()`. Faltó bumpearlo en el primer intento, encontrado y corregido al verificar en vivo (el navegador seguía sirviendo la versión vieja del script).

## 2026-08-13 - Fix: /Ventas mostraba encabezados de fecha vacíos al filtrar (Tipo comprobante, Forma de pago, Cliente, Vendedor)

- **Bug reportado por el usuario** (con captura): al filtrar en `/Ventas` (ej. Tipo comprobante = B), la lista mostraba TODOS los encabezados de fecha del rango buscado, aunque la mayoría no tuviera ningún registro que matcheara el filtro debajo.
- **Causa**: `aplicarFiltros()` en `_VentasFacturasFiltrosScripts.cshtml` decide si mostrar/ocultar cada `.fecha-grupo` buscando un `.venta-item` visible entre `grupo.parentNode.children` -- pero `_TablaVentas.cshtml` renderiza la lista COMPLETA como una única `<ul>` plana, con `.fecha-grupo` y `.venta-item` como hermanos directos (no hay un contenedor por fecha). `parentNode.children` devuelve TODOS los hijos de la lista entera, no solo los de esa fecha -- entonces alcanzaba con que UN item de CUALQUIER fecha siguiera visible para que TODOS los encabezados de fecha se mostraran, sin importar si su propio grupo tenía 0 resultados.
- **Fix**: en vez de mirar todos los hijos de la lista, recorrer los hermanos siguientes de cada `.fecha-grupo` (`nextElementSibling`) hasta toparse con el próximo `.fecha-grupo` (o el final de la lista) -- ese es el rango real de items de esa fecha en una lista plana.
- **Alcance**: esta función (`aplicarFiltros()`, filtrado 100% client-side por DOM) solo se usa en modo Ventas (`esFacturas=false`); el modo Facturas filtra en servidor con paginación (`buscarFacturasServidor()`, comentario explícito en el mismo archivo) y no tiene este bug -- no se tocó esa rama.
- **Verificado con Chrome real (CDP)**: reproducido el escenario exacto de la captura (rango 01/08 al 13/08, 26 ventas en 7 fechas) y tildado Tipo comprobante=B. Antes del fix: los 7 encabezados de fecha quedaban visibles. Después del fix: solo `06/08/2026` (la única fecha con un resultado que matchea) queda visible, las otras 6 se ocultan -- confirmado tanto por inspección del DOM (`style.display`) como por captura de pantalla.

## 2026-08-12 - POS: modal "Historial de precios" al 50% de ancho, sin tocar los demás usos del modal compartido

- **Pedido**: reducir el ancho del modal de historial de precios del cliente en Ventas/POS a la mitad.
- `#modalFinanzasPOS` es un modal genérico reusado por Compras, CtasCtes, Mis Ventas, Detalle de Venta, Egresos e Historial de Precios -- cada uso alterna una clase CSS propia (`modal-compra-pos-layout`, `modal-ctasctes-pos-compacto`, etc.) según la URL que se carga (`POSFinanzas.cargar()`, `POS.cshtml`). El uso de "Historial de precios" (`/Ventas/HistorialPreciosCliente`) era el único, junto con `CtaCtePersona` (cuenta corriente de UN cliente puntual, distinto de `CtasCtes` general), que no tenía ninguna clase asociada -- caía en el ancho por defecto del markup (`modal-xl`, 1140px en desktop grande).
- **Decidido**: agregar un flag `esHistorialPrecios` (mismo patrón que los otros 5 flags de `cargar()`, basado en `url.indexOf(...)`) y una clase nueva `modal-historial-precios-pos-compacto` que solo pisa el `max-width` del `.modal-dialog` -- no toca colores/bordes/header como las demás clases "compacto" (esas hacen un restyle visual completo pensado para otro tipo de contenido; acá el pedido era solo ancho).
- **Valores**: se espejó cada breakpoint propio de `modal-xl` (500px por defecto Bootstrap / 800px en `.modal-lg`+`.modal-xl` ≥992px / 1140px en `.modal-xl` ≥1200px) a la mitad exacta: 250px / 400px / 570px. Se descartó copiar el patrón "compacto chico" (ej. `modal-ctasctes-pos-compacto`, pensado para contenido angosto tipo formulario) porque el contenido acá es una tabla de 5 columnas que ya tiene su propio `table-responsive` -- no hacía falta.
- **Por qué `CtaCtePersona` no se tocó**: comparte el mismo "sin clase" hoy, pero el pedido fue puntualmente sobre historial de precios -- tocarlo también hubiera sido scope creep no pedido (CLAUDE.md §5).
- **Verificado con Chrome real (CDP)**: abierto el modal con `POSFinanzas.cargar('/Ventas/HistorialPreciosCliente?idPersona=1', ...)` (mismo código real que dispara el botón/F8, sin mockear nada). A 1400px de viewport: `modal-historial-precios-pos-compacto` presente, ancho medido `outerWidth()=570px` (exacto). A 1024px: 400px, tabla se ve completa sin overflow (algunos nombres de producto largos pasan a 2-3 líneas, sin romper el layout). `Web.csproj` no necesitó cambios (solo `.cshtml`, sin archivos nuevos).

## 2026-08-12 - Captura de respaldo: debounce de 5s + numeracion incremental por sesion, en vez de pisar el archivo

- **Bug encontrado por el usuario**: la primera version de `captura-respaldo.js` (misma tarde, ver entrada de más abajo) usaba el mismo `timestamp(new Date()) + ' - ' + etiqueta` (resolución de segundo) para el nombre de la SUBCARPETA y del ARCHIVO -- si dos capturas caían en el mismo segundo (uso normal, no un caso raro: toda la sesión se dedicó a que "agregar línea" sea instantáneo), `getFileHandle(..., {create:true})` reabría el mismo archivo y la escritura siguiente pisaba a la anterior. Solo sobrevivía la última captura de cada ráfaga.
- **Decidido** (2 pedidos del usuario, confirmados explícitos): (1) todas las capturas de una misma visita a la pantalla (una nueva compra, un movimiento, etc.) van a UNA sola subcarpeta, con archivos numerados incrementalmente (1, 2, 3...) adentro -- no una subcarpeta nueva por captura. (2) Si se detectan agregados/quitados en menos de 5 segundos entre sí (ej. una carga automática rápida de varios productos), NO generar una captura por cada uno -- esperar a que se aquiete (5s sin actividad nueva) y generar UNA sola para todo el grupo, para no "colapsar" el sistema con renders de página completa (html2canvas) disparados en cadena.
- **Mecanismo**: debounce clásico de 5000ms por pantalla (`etiqueta`) -- cada llamada a `capturar()` reinicia un `setTimeout` pendiente; recién se ejecuta la captura real cuando pasan 5s sin ninguna llamada nueva. El número de la captura (`sesion.contador`) se incrementa DENTRO de la función que efectivamente escribe el archivo, no en `capturar()` -- así refleja capturas reales que se guardaron, no eventos crudos de agregar/quitar que el debounce terminó colapsando en una sola.
- **Trade-off aceptado, no resuelto** (aviso explícito, no fue pedido resolverlo y agrega complejidad real): si el usuario guarda y navega fuera de la pantalla dentro de esos 5 segundos de espera, la captura pendiente de la última acción se pierde -- no hay forma confiable de forzar una escritura async de un archivo grande durante `beforeunload`. Inherente al debounce pedido.
- **Verificado en el proceso real** (no solo por lectura de código): en vez de depender del diálogo nativo de elegir carpeta (no automatizable por CDP), se inyectó un handle real de `navigator.storage.getDirectory()` (Origin Private File System, misma interfaz `FileSystemDirectoryHandle` que un directorio elegido por el usuario) directamente en el mismo IndexedDB que el módulo lee, simulando una activación real. Con eso: (1) **caso ráfaga** -- 5 llamadas a `capturar()` disparadas en milisegundos generaron exactamente **1** archivo, numerado `(1)`, escrito ~6s después de la última llamada; (2) **caso normal** -- 3 llamadas espaciadas por 6s cada una generaron **3** archivos, numerados `(1)`, `(2)`, `(3)`, todos dentro de la MISMA subcarpeta (creada en la primera llamada). Limpiados los artefactos de prueba (archivos OPFS y la entrada de IndexedDB inyectada) al terminar.

## 2026-08-12 - Atajos de teclado con modal abierto: regla nueva, `$(".modal.show").length` como guard obligatorio (6 archivos)

- **Bug reportado**: con el modal de Compras (`#modalFinanzasPOS`) abierto en Ventas/POS, tipear un código + Enter agregaba un producto al carrito de la pantalla de atrás. Causa: `pos-cart.js` (`addProduct()`) y `pos-product.js` (`handleEnter()`, el punto central que llama tanto el teclado físico como el teclado numérico en pantalla, `pos-keyboard.js:135-137`) no chequeaban si había un modal abierto -- a diferencia de TODOS los demás atajos de esa misma pantalla (`pos-balanza.js`, `pos-help.js`, `POS.cshtml`), que sí lo hacían. El backdrop de Bootstrap bloquea el mouse pero no el teclado -- si el foco queda (o vuelve a quedar) en un input de la pantalla de atrás, cualquier tecla le sigue llegando.
- **Regla nueva** (CLAUDE.md §5.1 -- se encontró el mismo error en 2+ lugares al revisar, se corrige el patrón, no cada caso): todo atajo de teclado global (bindeado a `document`, no a un elemento específico) que ejecuta una acción de la pantalla (agregar producto, disparar el botón primario, etc.) **debe** empezar con `if ($(".modal.show").length) return;`. No alcanza con chequear `$(e.target).closest('.modal')` (eso solo protege si el evento se originó DENTRO del modal, no si el foco quedó afuera con el modal igual abierto encima -- exactamente este bug).
- **Corregido en 6 archivos** el mismo día: `pos-product.js` (`handleEnter`), `pos-cart.js` (`addProduct`), `movimientos.js`, `elaborados-carga.js`, `elaborados-rapido.js`, `compras.js` (agregado el guard, antes no existía) y `stock.js` (reemplazado un guard incompleto -- `closest('.modal')` -- por el correcto).
- **Verificado en el proceso real** (no solo por lectura de código), reproduciendo el bug exacto reportado: con el modal de Compras abierto, forzando el foco a `#inputCodigo` y completando el flujo de 2 pasos (código + Enter, cantidad + Enter) vía eventos de teclado reales, el carrito se mantuvo en 0 items (antes del fix, esto agregaba 1). Repetido sin el modal abierto: el mismo flujo sí agregó el producto (sin regresión). Las otras 5 correcciones usan el mismo guard, verificadas por lectura directa de cada archivo (mismo idioma de una sola línea, sin lógica nueva que pueda fallar de forma distinta).

## 2026-08-12 - POS Ventas: historial de últimos precios por cliente + copiar/pegar precio; contador "Producto X de Y" en modo lote de Productos

- **Pedido**: al seleccionar un cliente en el POS, poder ver rápido el último precio que se le cobró por productos que ya compró (para recordarlo al armar una venta nueva), con posibilidad de copiar ese precio y pegarlo en un ítem del carrito. El usuario pidió mi recomendación sobre botón vs. atajo de teclado.
- **Acceso: botón + atajo F8** (confirmado con el usuario) — F8 es la única tecla F1-F10 libre en el POS (F9=buscar cliente, F10=buscar producto ya ocupadas; F11/F12 quedan afuera porque el navegador los reserva para pantalla completa/devtools).
- **Forma del historial: último precio por producto, deduplicado, sobre las últimas 10 ventas del cliente** (no un tope fijo de productos) — confirmado con el usuario tras mostrarle la alternativa (lista cronológica con productos repetidos).
- **Consulta nueva y liviana** (`Datos/Venta.cs:obtenerUltimosPreciosPorCliente`, `ROW_NUMBER() OVER (PARTITION BY idCorte ...)` sobre las últimas N `Ventas` del cliente) en vez de reusar `getAllVentas(cargarLineas:true)` — esa ruta existente hace un patrón N+1 (1 SP por venta + 1 lookup de `Corte` por línea), no conviene para esto.
- **Gate de sensibilidad replicado de `Finanzas/CtaCtePersona`** (pedido explícito del usuario al revisar el plan, citando "validaciones para clientes con ctacte y una empresa o cuit particular donde se oculta info sensible"): se oculta el botón/F8 y se bloquea la acción del lado del servidor cuando el cliente es Consumidor Final, o cuando tiene `Persona.CtaCte == true` y el usuario logueado no tiene el permiso `Permisos.Finanza.VerCtasCtes` (admin siempre pasa) — mismo criterio exacto que ya usa `FinanzasController.PuedeVerSaldosCuentaCorriente`/`OcultarSaldo`. El gate del botón es solo UX; el límite real lo vuelve a aplicar `VentasController.HistorialPreciosCliente` antes de devolver precios, así que forzar la apertura sin pasar por el botón (ej. editando el DOM) no expone datos que no debería.
- **`PersonasController.Obtener`** (usado por el POS para resolver datos de cliente) se extendió con `ctaCte` en el JSON — antes no lo devolvía, hacía falta para decidir el gate del lado del cliente.
- **Copiar/pegar precio: en memoria JS, no portapapeles del SO** — decisión técnica propia (no pedida por el usuario, pero necesaria): `navigator.clipboard.writeText` exige HTTPS, y no todos los despliegues de este proyecto corren con SSL siempre activo. Como copiar y pegar ocurren dentro de la misma página, alcanza con `window.POSPrecioCopiado` + un botón "Pegar" en el destino — cero dependencia de permisos de portapapeles.
- **2 bugs reales encontrados y corregidos recién al verificar en vivo, no por lectura de código**:
  1. La consulta SQL nueva asumía una columna `corteDesc` en `dbo.Corte` que no existe (el nombre real es `corte`) — la acción devolvía 500 siempre. Corregido y reverificado contra la base local.
  2. El handler `hidden.bs.modal` de `#modalFinanzasPOS` limpiaba `window.POSPrecioCopiado` al cerrar CUALQUIER modal de esa familia, incluido el propio historial de donde se copia el precio — como hace falta cerrar ese modal para volver al carrito y pegarlo, esto rompía la feature completa (el precio se perdía antes de poder usarlo). Se movió la limpieza a `actualizarAccesoHistorialPreciosCliente` (se limpia al cambiar de cliente, no al cerrar el modal).
- **Extra confirmado en el mismo pase**: contador "Producto X de Y" en el modo "Modificar en lotes" de `Web/Views/Productos/_StockPorSucursalesProductoModal.cshtml` (sugerencia de una tarea anterior, sin implementar hasta que el usuario la confirmó ahora). Resuelto 100% del lado del cliente en `Productos/Index.cshtml` (`verStockSucursales`/`getVisibleStockRows`, ya existentes) — ni el partial ni el controller necesitan saber la posición.
- **Verificado en vivo (Chrome real vía CDP), no solo por lectura de código**: historial con datos reales de un cliente con 13 ventas (`idPersona=22`, deduplicado correctamente a 5 productos distintos); botón oculto con Consumidor Final; con un cliente `CtaCte=true` (`idPersona=23`) y el usuario de prueba (admin), el botón y los datos se muestran correctamente (confirma que el bit `ctaCte` viaja bien por toda la cadena) -- **el caso bloqueado (usuario no-admin sin el permiso) no se pudo probar en vivo** por no haber un usuario de prueba sin ese permiso a mano, queda como deuda de verificación aunque el código replica exactamente el patrón ya probado de `CtaCtePersona`; copiar un precio, cerrar el modal y pegarlo en `#txtPrecioKg` de una línea (después del fix del bug 2); atajo F8 abre el modal; contador de lote aparece/desaparece correctamente según `modoLotePuntoStockActivo`. `Web.csproj`/`Datos.csproj`/`Negocio.csproj` compilan limpio.

## 2026-08-12 - Respaldo automático por captura de pantalla (Web), replicando WinForms: File System Access API + html2canvas, 100% local

- **Contexto**: WinForms tiene esto desde 2016 (`Utilidades/Util_Form.cs:485-527`, `Graphics.CopyFromScreen`) en Alta Movimiento, Stock, Elaborados y Compras (comparte formulario con Stock) -- al agregar/quitar una línea, guarda un PNG (mal nombrado `.jpg`) en `%USERPROFILE%\Desktop\Capturas\`. Sin comentario de negocio ni en el código ni en el historial de git que explique el motivo -- se infiere del propio mecanismo (recuperar el trabajo en curso si la PC se apaga o el sistema se cierra inesperado a mitad de una edición).
- **Decidido**: replicar en Web para las mismas 4 pantallas, con 2 correcciones deliberadas sobre el mecanismo de WinForms:
  1. **Dónde se guarda**: en la máquina del usuario (como WinForms), NO en el servidor. Primer planteo mío proponía subir a una carpeta del servidor -- el usuario lo corrigió explícitamente. Un navegador no tiene forma de escribir en el disco local sin permiso del usuario, así que se usa la **File System Access API** (`showDirectoryPicker()`): el usuario elige/crea una carpeta una sola vez (requiere un click real, restricción de seguridad del navegador, no se puede saltear) y de ahí en más cada captura se escribe sola, sin diálogo. El handle elegido se persiste en IndexedDB, compartido entre las 4 pantallas (se elige una vez en cualquiera, las otras 3 lo reconocen solas).
  2. **Cómo se captura la imagen**: WinForms usa `Graphics.CopyFromScreen` (pantalla física). Un navegador no tiene equivalente sin pedir permiso de captura de pantalla en cada uso (`getDisplayMedia`, inviable disparado en cada click) -- y aunque lo tuviera, seguiría sin resolver el pedido explícito del usuario ("la página completa, no solo lo visible, porque hay campos que quedan fuera de la altura de pantalla"), porque una captura de pantalla real solo trae lo que se ve en el momento. Se usa **html2canvas** (vendorizada en `Content/vendor/html2canvas/`, no CDN -- mismo criterio que el resto de librerías de terceros de este proyecto) renderizando `document.documentElement` con `windowWidth`/`windowHeight` = `scrollWidth`/`scrollHeight` del documento completo, no del viewport.
- **Alcance**: solo Compras, Movimientos, Elaborados, Stock (confirmado explícito, `AskUserQuestion`) -- Ventas/POS y Punto de Expendio, que en WinForms usan un disparador distinto (checkbox de lectura de peso, no agregar/quitar línea), quedan fuera de esta ronda.
- **Sin componente de servidor, sin retención/limpieza**: al guardarse en el disco del propio usuario (no en uno compartido), no hace falta pensar en límites de espacio ni en aislar por usuario -- mismo perfil de riesgo que WinForms (cada quien su disco, su problema). Esto fue una vuelta atrás explícita sobre mi primer planteo (que sí necesitaba retención, al proponer guardar en el servidor).
- **Limitación aceptada**: File System Access API solo existe en navegadores Chromium (Chrome/Edge) -- no Firefox/Safari. Degradación con gracia: en un navegador sin soporte, el botón "Activar respaldo automático" no aparece, la pantalla funciona igual sin el respaldo.
- **Alternativa descartada**: descarga automática simple (`<a download>`). Funciona en cualquier navegador sin pedir permiso, pero dos problemas reales: (1) deja el archivo en la carpeta "Descargas" genérica del navegador, no en una carpeta elegida tipo "Capturas"; (2) si el navegador tiene activado "preguntar dónde guardar cada archivo", aparecería un diálogo de guardado en CADA click de agregar/quitar línea -- inviable para un respaldo silencioso.
- **Verificado en el proceso real** (no solo por lectura de código): el diálogo nativo de `showDirectoryPicker()` no se puede automatizar por CDP (es un diálogo del sistema operativo, no un elemento de la página) -- se verificó todo lo demás en Chrome real: (1) `html2canvas` capturando el documento completo -- con el viewport forzado a 500px de alto y el documento con scrollHeight real de ~1025px, el canvas resultante dio ~1423px de alto (con devicePixelRatio 1.25 aplicado), muy por encima del viewport, confirmando que trae contenido fuera de la pantalla visible; (2) `capturar()` sin ningún handle activado no lanza ninguna excepción ni deja ninguna promesa rechazada sin manejar -- confirmado con listeners de `error`/`unhandledrejection` armados antes de la llamada; (3) la secuencia completa de escritura (`getDirectoryHandle` → `getFileHandle` → `createWritable` → `write` → `close`) se probó end-to-end contra `navigator.storage.getDirectory()` (Origin Private File System, implementa la MISMA interfaz `FileSystemDirectoryHandle`/`FileSystemFileHandle` que un directorio elegido por el usuario, pero sin pedir permiso) -- el archivo final coincidió en tamaño exacto con el blob generado y tiene la firma PNG válida (`89 50 4E 47`); (4) las 4 pantallas cargan `html2canvas`/`CapturaRespaldo` sin errores y muestran el botón "Activar" correctamente en su primera carga. **Pendiente de verificación manual real** (requiere interacción humana con el diálogo nativo del SO, no automatizable): elegir una carpeta real y confirmar que aparece el archivo esperado tras agregar/quitar una línea.

## 2026-08-12 - Compras/Index: detalle de lineas via AJAX perezoso, no eager (pedido explicito del usuario)

- **Decidido**: el detalle de lineas de cada compra en `Compras/Index` se carga por AJAX **solo cuando el usuario expande esa fila especifica** (`ComprasController.Detalle(idCompra)`), nunca al cargar la lista completa. Mismo patron que `Stock/Index`/`StockController.Detalle`.
- **Alternativa descartada**: completar `ConstruirDetallesIndex` (el metodo que ya arma el resto del detalle de cabecera) para que TAMBIEN trajera `Lineas` de todas las compras visibles, de una sola vez -- mas simple (menos codigo, un solo mecanismo en vez de dos), pero dispara una consulta de lineas por cada fila visible en la lista **aunque el usuario nunca las expanda**. Con un rango de fechas amplio (la pantalla no pagina), esto podria ser docenas/cientos de consultas extra en cada carga de pagina.
- **Por que gano la version lazy**: consultado explicitamente al usuario (`AskUserQuestion`, 2026-08-12), eligio el patron de Stock por sobre la alternativa eager mas simple, priorizando performance sobre menos codigo. Como consecuencia, `ConstruirDetallesIndex` y sus helpers `Leer*` (que solo poblaban el detalle de cabecera desde el DataTable de la lista) quedaron sin uso y se borraron -- el detalle completo (cabecera + lineas) ahora se arma en un solo lugar (`ComprasController.Detalle`), desde el entity (`findById_convertToCompra`) en vez del DataTable de la lista.
- **Deuda heredada, no nueva**: la tabla de lineas de `_ComprasDetalle.cshtml` (copiada de `_StockDetalle.cshtml`) solo muestra cortes, no medias reses -- mismo limite que ya tiene Stock. Cantidad/CantidadMedias/Total del encabezado si contemplan ambos tipos (se calculan aparte, sumando `obtenerMediasPorCompra` cuando `TipoCompra` es Media Res).

## 2026-08-12 - Fix real del bug de hora en Elaborados; columnas/detalle en EgresosCaja; auditoria en Movimientos; doble-click global

- **Elaborados (Index y Lineas) -- bug real, no un limite del sistema**: `NormalizarFechaDesde`/`NormalizarFechaHasta` en `ElaboradosController.cs` truncaban la hora **siempre e incondicionalmente**, incluso cuando el usuario tipeaba una hora real -- el usuario reporto "cambio la hora, aprieto Buscar, y se resetea a 00:00". Se confirmo contra `Presentacion/Embutidos/formEmbutidos.cs:72` que WinForms **si** filtra por hora correctamente en la pantalla equivalente (pasa `fechaDesde.Value`/`fechaHasta.Value` sin truncar, el ajuste a fin de dia solo se usa para el valor default al abrir el form). Esto contradice la entrada anterior (2026-08-11) que asumia el truncado de Elaborados como diseno deliberado "solo visual" -- era en realidad el mismo bug que ya se habia corregido en Cajas/EgresosCaja, Productos y PuntosExpendio, pero nunca se aplico a Elaborados mismo (la pantalla que origino la referencia). Corregido con la misma regla "hasta inteligente" ya establecida (ver entrada anterior). Al ser metodos estaticos compartidos, un solo cambio arregla `Index` y `Lineas` a la vez. No hizo falta tocar el SP `buscarEmbutido` ni las vistas.
- **Cajas/EgresosCaja -- columnas nuevas**: se agregaron "Detalle" (icono `badge-pill badge-info` condicional, mismo patron que la columna "Obs." de `Elaborados/_TablaElaborados.cshtml`, en vez de texto completo en la fila compacta) y "Sucursal" a la fila principal de `_EgresosCajaTabla.cshtml` -- ninguna de las 2 existia ahi antes (solo en el detalle expandido). Orden confirmado con el usuario via pregunta directa: `Monto -> Detalle -> Sucursal -> Acciones`.
- **Detalle expandido de EgresosCaja -- mismo lenguaje visual que Elaborados**: se reemplazo el `<div class="row">` generico por una tarjeta `.egreso-detalle-meta-card`/`.egreso-detalle-meta-row` (calco de `.elaborado-detalle-meta-card` en `_DetalleElaborado.cshtml`, incluida la variante dark-mode), con "Detalle" en su propia fila destacada arriba (`.egreso-detalle-valor-principal`, mas peso tipografico) en vez de ser un campo mas entre otros -- pedido explicito ("que el detalle tenga mas presencia").
- **Movimientos/Index -- Creado/Creado por/Actualizado/Actualizado por al expandir**: el dato ya estaba disponible sin tocar el SP -- `MovimientosController.Detalle(id)` ya llamaba a `cargarMovimiento(id, false)` (trae la entidad completa con esos 4 campos) pero solo se usaba `Observaciones`. Se agregaron los 4 campos a `MovimientoDetalleVm` y al partial `_MovimientoDetalle.cshtml`, con el mismo layout de 2 columnas (tabla de lineas + tarjeta meta) que usa Elaborados.
- **Doble-click para expandir/compactar -- global, en las 7 tablas que tenian el mecanismo de expand pero no el atajo**: precedente ya funcional en `Stock/ExistenciaPorSucursales.cshtml` (unico caso que ya lo tenia). Se replico el mismo criterio (guard `if (target dentro de a/button) return`, despues disparar `.click()`/`.trigger('click')` sobre el boton de detalle YA existente de la fila, sin duplicar logica de toggle) en: `Movimientos/Index`, `Elaborados/_TablaElaborados`, `Elaborados/Formulas`, `Stock/_StockTabla`, `Compras/_ComprasTabla`, `Cajas/_EgresosCajaTabla`, `Finanzas/Cheques`. Para las 4 vistas cuyas filas no tenian una clase CSS propia (Formulas, Stock, Compras, Cheques) no hizo falta tocar el Razor -- alcanzo con `e.target.closest('tr')` + `row.querySelector(selector-del-boton)` (vanilla) o `tr:has(selector-del-boton)` (jQuery) para delegar el listener sin depender de una clase nueva.
- **EgresosCaja -- atajo Alt+Enter para "Nuevo Egreso"**: mismo patron ya usado en `movimientos.js`/`compras.js` (guard `altKey && !ctrlKey && !metaKey && !shiftKey && !repeat && key === 'enter'`). Con un guard adicional (`if (!$('#btnNuevoEgresoCaja').length) return`) porque `egresos-caja.js` se carga tambien embebido en otras pantallas (POS) donde el boton no existe -- evita registrar un atajo global que interfiera fuera de contexto.
- **Cache-busting**: bump de `?v=` en los `.js` externos tocados -- `elaborados.js` (2->3), `movimientos.js` (27->28), `egresos-caja.js` (9->10) -- verificado con grep en todos los `.cshtml` que los referencian. Los cambios en Formulas/Stock/Compras/Cheques son JS inline (`@section Scripts`), sin archivo `.js` externo, no aplica bump.
- **Verificado**: `Web.csproj` compila limpio. La verificacion en vivo (Chrome/CDP) de las 4 partes quedo a cargo del usuario -- pidio testear el resultado el mismo antes de continuar con mas cambios automatizados.

## 2026-08-11 - Filtro de hora real en Index, solo donde WinForms ya filtra por hora

- **Pedido original**: agregar filtro de hora a los Index que ya tienen "Fecha Desde"/"Fecha Hasta" y no lo tienen, tomando `/Elaborados` como referencia. Al investigar, `/Elaborados` en realidad **descarta** la hora que el usuario tipea (`ElaboradosController.cs` `NormalizarFechaDesde`/`NormalizarFechaHasta` truncan a `.Date` y fuerzan 00:00:00-23:59:59) -- el input permite elegir hora pero es cosmetico, no filtra. Se le consulto al usuario si replicar ese comportamiento "solo visual" (bajo riesgo) o hacer que la hora filtre de verdad; primero pidio que filtrara siempre, pero al mostrarle que el SP legacy detras de Compras/Stock (`obtenerCompras`) y Movimientos (`obtenerMovimientos`) tiene un truco `@fechaHasta+1` que asume medianoche (compartido con WinForms, forkearlo hubiera sido necesario), el usuario dio el criterio definitivo: **guiarse por que filtra WinForms para cada modulo**.
- **Auditoria contra `Presentacion/` (solo lectura, sin tocar nada ahi)**: `formMovimientos.cs:112`, `formCompras.cs:92`, `formStock.cs:82` truncan a `.Date` antes de llamar a sus SPs -- WinForms **no** filtra por hora en esos 3 modulos, asi que **Movimientos/Index, Stock/Index y Compras/Index quedan sin tocar** (ni siquiera el input visual -- agregarlo sin que filtrara de verdad hubiera reintroducido el mismo problema "solo visual" de Elaborados). `formEgresosCaja.cs:90` en cambio pasa `fechaDesde.Value`/`fechaHasta.Value` sin truncar, con `DateTimePicker.CustomFormat = "dd/MM/yyyy HH:mm:ss"` (`formEgresosCaja.Designer.cs:251,278`) -- WinForms **si** filtra por hora ahi, confirmado ademas contra la base local (`OBJECT_DEFINITION('dbo.obtenerEgresosCaja')`: `WHERE fechaHora between @fechaDesde and @fechaHasta`, sin el truco `+1`). Se replico ese mismo modo en `Cajas/EgresosCaja` (web).
- **Productos/Index ("Actualizado en periodo") y PuntosExpendio/ExpendiosGenerados**: sin equivalente en `Presentacion/` (confirmado por grep, 0 resultados en ambos casos) -- son features 100% propias de Web (LINQ en memoria y SQL inline respectivamente, sin SP legacy compartido), asi que aplicar hora real ahi es seguro y se incluyeron en el alcance.
- **Regla "hasta inteligente"** (ya existia en `CajasController.EgresosCaja`, se generalizo a los otros 2): `Desde` se usa tal cual (`>=`). `Hasta`, si `TimeOfDay == TimeSpan.Zero` (el usuario no toco la hora), se extiende a `hasta.AddDays(1).AddSeconds(-1)` (23:59:59, "todo ese dia"); si el usuario si especifico una hora real, se usa tal cual (`<=`). Queda como convencion para cualquier filtro de fecha+hora nuevo en el proyecto.
- **Bug real encontrado al verificar `PuntosExpendio/ExpendiosGenerados`**: sacar el `CONVERT(date, e.fechaExpendio)` del `WHERE` en `Datos/Venta.cs:obtenerExpendiosEmpresa` no alcanzaba -- el mismo metodo **tambien truncaba los parametros** al armarlos (`p.AddWithValue("@fechaHasta", fechaHasta.Value.Date)`, linea 930), un segundo punto de truncado que se paso por alto en el plan inicial. Con Desde+Hasta juntos esto produjo un bug real y reproducible (0 resultados siempre que ambos parametros venian con hora), detectado recien al probar contra datos reales, no por lectura de codigo. Corregido sacando el `.Date` de esas 2 lineas tambien. `obtenerExpendiosPorUsuario` (mismo archivo, linea ~879-880, metodo distinto, fuera de alcance) se dejo con su truncado intacto -- no forma parte de este cambio.
- **Sin cambios de SP ni de `Datos/Compra.cs`/`Negocio/Compra.cs`/`Datos/Corte.cs`/`Negocio/Corte.cs`**: al excluir Movimientos/Stock/Compras del alcance, no hizo falta forkear ningun SP legacy -- cero riesgo sobre `Presentacion/`.
- **Verificado end-to-end contra datos reales de la base local** (no solo por API/fetch, tambien via la UI real): `Cajas/EgresosCaja` -- egreso de prueba creado via el modal real (`btnNuevoEgresoCaja`) a las 13:08:00, filtrar con Hasta=13:00 lo excluye, Hasta=13:30 lo incluye, Desde=13:30 lo excluye (test data borrado despues). `Productos/Index` -- producto real (`idCorte=33`, CARRE) reguardado via el formulario real de edicion (`btnHabilitarEdicionProducto` + `btnGuardarProducto`) a las 13:16:46, mismo patron de 3 filtros, los 3 correctos (sin necesidad de limpiar datos, fue un re-guardado legitimo). `PuntosExpendio/ExpendiosGenerados` -- contra un expendio real preexistente (`idExpendio=94`, 20:28:38 del 2026-08-06), mismo patron de 3 filtros vía la UI real (`btnBuscarExpendiosGenerados`), los 3 correctos tras el fix del bug de truncado. `Web.csproj`/`Datos.csproj` compilan limpio.

## 2026-08-11 (continuacion) - Resuelta la deuda de logging: listener global para Trace.TraceError

- **Decidido**: en vez de un helper de logging propio en `ErrorController`, se configuro **un solo listener global** (`System.Diagnostics.TextWriterTraceListener`) en `Global.asax.cs Application_Start()`, escribiendo a `~/App_Data/error-web.log` (ruta configurable via el nuevo AppSetting `ErrorLogPath`, mismo patron ya usado por `PerfInstrumentationLogPath`). Se prefirio esto a un helper nuevo porque `Trace.TraceError` es un sumidero global de proceso -- un solo listener arregla **los 4 call-sites existentes** (`ErrorController.General`, y los 3 preexistentes en `ProductosController.cs`: `GuardarPuntosStockSucursal`, `GenerarPdfEtiquetas` x2) de una sola vez, sin duplicar la logica de escritura a archivo que ya existe en `Utilidades.PerformanceInstrumentation.WriteLine` -- regla CLAUDE.md SS5.1 (arreglar el loop, no cada instancia).
- **Por que no reusar `PerformanceInstrumentation` directamente**: sus metodos publicos (`LogServerEvent`, etc.) estan gateados por `PerfInstrumentationEnabled` (AppSetting, pensado para diagnostico de performance opcional) -- un error de aplicacion debe loggearse siempre, independientemente de ese flag. Se opto por un mecanismo separado (`Trace.Listeners`) en vez de tocar esa clase para agregar un modo "siempre on", evitando mezclar 2 conceptos distintos (performance vs. errores) en el mismo archivo/clase.
- **Resolucion de ruta sin hardcodear** (SS1.1): igual que `PerformanceInstrumentation.ResolveLogPath()`, pero usando `HttpRuntime.AppDomainAppPath` en vez de `Server.MapPath` -- `Application_Start` corre antes de que exista un `HttpContext`, `Server.MapPath` no esta disponible ahi.
- **`Trace.AutoFlush = true`**: sin esto, un crash abrupto (recycle del AppDomain, falla de proceso) podria perder las ultimas lineas bufferizadas antes de que se escribieran a disco -- justamente el escenario que este logging existe para capturar. `Application_End` llama a `Trace.Flush()` como resguardo adicional en un shutdown ordenado.
- **Deuda nueva, documentada, no resuelta en este pase**: el archivo no rota ni tiene limite de tamano -- crece sin limite mientras la app este arriba. Aceptable para el volumen esperado (errores de aplicacion, no logging de alto volumen) pero si con el tiempo se vuelve un problema, hace falta rotacion (por tamano o por fecha) -- no se implemento por no haber sido pedido y por YAGNI (SS1), no por descuido.
- **Verificado en el entorno real** (no solo por lectura de codigo): se agrego temporalmente una linea `Trace.TraceError(...)` de diagnostico en `Application_BeginRequest`, se recompilo, se reinicio IIS Express, se disparo un request real, y se confirmo que `App_Data/error-web.log` se creo con el mensaje esperado en el formato estandar de `TextWriterTraceListener` -- confirmando que la resolucion de ruta, la creacion de directorio y el registro del listener funcionan en el proceso real (no solo en teoria). La linea de diagnostico se revirtio y el log de prueba se borro antes de terminar; el codigo final (sin la linea temporal) se recompilo limpio una vez mas. **No verificado por el mismo motivo que el redirect** (ver entrada anterior): el disparo real de un 500 en produccion no se pudo reproducir en `localhost` por `customErrors mode="RemoteOnly"` + `httpErrors errorMode="DetailedLocalOnly"` (preexistentes) -- se verifico el mecanismo de logging de forma aislada e inequivoca en su lugar.

## 2026-08-11 (continuacion) - Errores de navegacion 404/500: redirect directo al Home en vez de pagina de error con marca

- **Decidido**: `ErrorController.NotFound()`/`General()` ya no renderizan ninguna vista -- redirigen con `RedirectToAction("Index", "Home")`. El enfoque inicial (restylear `Views/Error/NotFound.cshtml`/`General.cshtml` con la marca de la app, precedente `Login/Index.cshtml`) se descarto durante la revision del plan: el usuario confirmo explicitamente que prefiere volver al Home sin mostrar ninguna pantalla de error, ni siquiera una con marca -- motivado en parte porque en PWA instalada no hay boton "atras" del navegador. `Views/Error/NotFound.cshtml` y `General.cshtml` se borraron (dejarlas hubiera sido codigo muerto, ninguna accion las devuelve).
- **Guard de loop**: si la excepcion/404 ocurre sobre el Home mismo (`EsRutaRaiz(rawUrl)` == true para `/`, `/Home`, `/Home/Index`), no se redirige de nuevo -- se devuelve un `Content(...)` de texto plano minimo. Sin este guard, un Home roto causaria un loop infinito de redirects.
- **Bug preexistente encontrado (no introducido por este cambio)**: `Web/Controllers/ErrorController.cs` nunca estuvo en `Web.csproj` (`<Compile Include>` faltante, proyecto de formato legacy) -- la clase jamas se compilo al `.dll`, asi que `customErrors`/`httpErrors` de `Web.config` (apuntando a `Error/NotFound`/`Error/General`) estuvieron muertos desde que se crearon, para cualquier usuario. Se agrego la linea faltante como parte de este cambio -- sin eso, esta tarea (ni la version anterior del controller) podia funcionar. Vale la pena una revision aparte de si hay otros archivos en `Web/` en la misma situacion (no auditado, fuera de alcance de esta tarea).
- **Logging antes de perder el error de vista**: `General()` sigue capturando `Server.GetLastError()`, ahora se loggea con `Trace.TraceError(...)` (mismo patron ya usado en `ProductosController.cs`) antes de redirigir. **Deuda resuelta el mismo dia** (ver entrada anterior, "Resuelta la deuda de logging: listener global para Trace.TraceError") -- se configuro un `TextWriterTraceListener` global en `Global.asax.cs`, el `Trace.TraceError` de esta entrada y los 3 preexistentes de `ProductosController.cs` ya persisten a `App_Data/error-web.log`.
- **Alternativa descartada**: mostrar detalle tecnico del error solo a administradores o en un log visible dentro de la app. No se evaluo a fondo porque el pedido fue puntual (ocultar la pagina rota al usuario), no agregar observabilidad -- si se necesita, es una tarea aparte.
- **Verificado**: `Web.csproj` compila limpio. `Error/NotFound` y `Error/General` invocados directamente devuelven 302 -> `Home/Index` -> (sin sesion) `Login`, cadena completa confirmada con `Invoke-WebRequest` real. El guard de loop se verifico por lectura de codigo, no por reproduccion real (no hay forma segura de romper Home a proposito en el ambiente de prueba sin modificar codigo aparte). Localmente (`localhost`), `customErrors mode="RemoteOnly"` + `httpErrors errorMode="DetailedLocalOnly"` (preexistentes, sin tocar) siguen mostrando el diagnostico detallado de ASP.NET en vez del redirect -- comportamiento deliberado y ya documentado del proyecto para desarrollo local; el redirect aplica normalmente para usuarios remotos/produccion, el caso real que motivo el pedido.

## 2026-08-11 (continuacion) - Stock/Editar: columna Producto+Codigo combinada con 2 links de orden, y buscador oculto tras interruptor

- **Decidido**: la columna "Producto" de `#tablaLineasStock` (muestra nombre + "Código: X" apilados en una sola celda) queda como **1 sola columna fisica** con 2 mini-links de orden independientes adentro (`data-sort-key="producto"` / `data-sort-key="codigo"`), en vez de partirla en 2 columnas separadas como ya esta en `Movimientos/Editar` (`#tablaLineasMovimiento` si tiene Codigo y Producto como `<th>` distintos).
- **Por que**: pedido explicito del usuario al elegir entre las 2 opciones ofrecidas -- prioriza no agrandar el ancho de la tabla ni cambiar el layout visual ya existente de esa columna, aunque tecnicamente hubiera sido mas simple replicar el patron de Movimientos 1:1 (`ensureSortableHeaders` por posicion).
- **Alternativa descartada**: partir en 2 columnas (`Producto` / `Código`) igual que Movimientos, reusando el mecanismo de orden existente sin adaptacion. Mas simple de implementar pero cambia el layout de la tabla, que el usuario no pidio tocar.

- **Decidido**: el input de busqueda en vivo (tanto en Stock como en Movimientos) arranca **oculto**, detras de un boton de lupa (`btn-sm`) que lo muestra/oculta compartiendo la misma fila -- no hay una fila de input siempre visible arriba de la tabla. Al cerrar el interruptor, se limpia el filtro (`state.searchText=''`) y se re-renderiza sin busqueda activa.
- **Por que**: pedido explicito del usuario tras ver la primera version del plan ("que el input... esté oculto y se habilite mediante un interruptor de búsqueda, que no ocupe mucha altura para no estirar la vista"). Limpiar el filtro al cerrar evita dejar un filtro activo invisible para el usuario (sin el input abierto no hay forma de ver que la tabla esta filtrada).
- **Extendido a Movimientos por pedido explicito del usuario** en el mismo mensaje ("agregar el mismo buscador en vivo en movimientos") -- Movimientos ya tenia orden por columna, se le sumo unicamente la busqueda con el mismo patron de interruptor oculto, sin tocar el mecanismo de orden existente.
- **Invariante verificada en ambas pantallas**: la busqueda es puramente un filtro de vista -- totales/resumen e inputs hidden que se envian al guardar siempre reflejan **todas** las lineas cargadas, nunca solo las visibles bajo un filtro activo. Ver bitacora 2026-08-11 para el detalle de verificacion.

## 2026-08-10 (continuacion) - Cartel de "guardado correctamente": 2 exclusiones deliberadas de alcance

- **Decidido**: `modal-cheque.js` (`Finanzas/Cheques`, accion `GuardarCheque`) queda **sin tocar**, aunque el resto de los modales in-place similares (`egresos-caja.js`) sí migraron al cartel nuevo con timer.
- **Por qué**: este JS se carga tambien embebido dentro de `Ventas/POS.cshtml` (via `Finanzas/AddOrEditPago`), pero a diferencia de `egresos-caja.js`/`compras.js`, **no tiene ningun guard `desdePos`** que distinga si el modal se abrio desde POS o desde la pantalla standalone de Cheques. Agregar el cartel ahi sin poder acotarlo hubiera violado la exclusion explicita de LayoutPOS del pedido del usuario. Inventar una deteccion de contexto nueva (ej. chequear si existe un ancestro con cierta clase/id de POS) no estaba pedido y agregaba riesgo real de romper algo en produccion para un caso menor -- se prefirio dejarlo con su Swal legacy (sin timer, funcional, solo menos prolijo) antes que arriesgar el flujo de POS.
- **Alternativa descartada**: agregar un `desdePos` nuevo a `modal-cheque.js` copiando el patron de `egresos-caja.js`. Requeria entender de donde sale ese flag en los otros archivos (revisar `compras.js`/`egresos-caja.js` en detalle) y replicarlo con confianza -- mas alcance del que el pedido ameritaba para un modulo menor, y el usuario explicito que el cambio debia ser aditivo y sin riesgo de romper nada.

- **Decidido**: `MovimientosController.Guardar` y `FinanzasController.AddOrEditPagoPost` quedan **sin el cartel nuevo**, a diferencia del resto de los ~24 flujos de guardado bajo LayoutBase.
- **Por qué**: ambos ya abren un modal propio de "post-guardado" con header verde y texto claro ("Movimiento guardado"/"Pago guardado") mas botones de accion (imprimir, etc.) -- el problema real que motivo el pedido del usuario (falta de feedback claro) no aplica ahi. Agregar el cartel de 2s antes de ese modal hubiera generado un parpadeo doble (cartel se autocierra -> se abre el modal de acciones) sin beneficio real. Confirmado con el usuario via `AskUserQuestion` antes de decidir.
- **Alternativa descartada**: agregar igual el cartel por uniformidad estricta con el resto de la app. El usuario eligio explicitamente no hacerlo.

## 2026-08-10 - Regla nueva: al editar un `.css`/`.js` con query string `?v=N`, subir N en el mismo cambio

- **Regla**: `custom.css`, `movimientos.js` y varios otros assets estáticos del proyecto se referencian con cache-busting manual (`?v=2`, `?v=25`, etc.) en los `.cshtml` que los cargan. Si se edita el archivo pero no se sube el número de versión en **todos** los lugares que lo referencian, el navegador sigue sirviendo la copia vieja cacheada indefinidamente -- el cambio queda invisible aunque el archivo en disco ya esté actualizado y el build haya compilado sin errores.
- **Por qué (2 casos reales, mismo día, mismo bug -- ver bitácora 2026-08-10)**: al arreglar el ancho del scanner de Movimientos/Stock, el primer intento no tuvo ningún efecto visible pese a que `custom.css` en disco (confirmado con `fetch()` cache-busted) sí tenía la regla nueva -- hubo que subir `custom.css?v=2` a `v=3` (y de nuevo a `v=4` tras un segundo fix) en `_LayoutBase.cshtml` **y** `_LayoutPOS.cshtml` (2 lugares). Después, el atajo de Enter en `movimientos.js` tuvo el mismo síntoma exacto -- hubo que subir `movimientos.js?v=25` a `v=26` en `Movimientos/Editar.cshtml` **y** `Movimientos/Index.cshtml` (2 lugares también).
- **Regla para código futuro**: al editar cualquier `.css`/`.js` referenciado con `?v=N`, `grep` el nombre del archivo en todo `Web/Views/` para encontrar **todos** los lugares que lo cargan (no asumir que hay uno solo) y subir el número en todos, en el mismo commit que el cambio. Si el archivo no tiene versión (`?v=`) todavía, no hace falta agregarla para este caso puntual -- pero si ya la tiene, no se puede editar el contenido sin tocar el número.
- **Cómo se verificó** (para no repetir el mismo susto): comparar el resultado de `getComputedStyle`/medición real en el navegador contra lo esperado: si no cambia nada pese a que el archivo en disco sí cambió (confirmable con un `fetch()` con cache-busting propio, o revisando Network tab), sospechar cache del asset antes que asumir que la regla CSS/JS está mal escrita.

## 2026-08-10 - Scanner compartido (Movimientos/Stock): `width:auto` en vez de `width:100%` para poder usar margin negativo (bleed)

- **Decidido**: para que `.scanner-wrapper` (el video del scanner) ocupe el ancho completo de su card en vez de quedar recuadrado por el padding de `.card-body`, la regla scoped (`.scanner-shared-card .scanner-wrapper`) pisa el `width:100%; max-width:100%` de la regla base con `width:auto; max-width:none;`, además del `margin: -.5rem -.5rem 0 -.5rem` (bleed).
- **Por qué**: con `width:100%` (heredado de la regla base, `custom.css` línea ~482) puesto a la vez que un `margin` explícito en ambos lados, el modelo de caja de CSS queda **sobre-restringido** -- la ecuación `margin-left + border + padding + width + padding + border + margin-right = ancho del contenedor` no puede tener 3 valores explícitos (`margin-left`, `width`, `margin-right`) todos fijos a la vez si no cierran matemáticamente. Por spec (CSS2.1 §10.3.3), el navegador **ignora el `margin-right` especificado** y lo recalcula para que la ecuación cierre -- en la práctica, el margen negativo no tenía ningún efecto visual, aunque `getComputedStyle` seguía reportando el valor "especificado" (`-8px`) engañosamente, como si se hubiera aplicado. Se detectó porque `getBoundingClientRect().width` del wrapper no cambió nada entre antes y después del primer intento del fix.
- **Fix**: dejar `width` en `auto` (no explícito) para que sea la variable que el navegador resuelve a partir de los márgenes, en vez de al revés. Con `width:auto`, la ecuación se resuelve tomando `margin-left`/`margin-right` como fijos y calculando `width` para que cierre -- ahí sí el margen negativo expande la caja como se espera. Verificado con `getComputedStyle` real: wrapper pasó de 116px/130px (sin efecto) a 146.8px, calzando con el ancho real de la card-body.
- **Lección para código futuro con el mismo patrón** (bleed de un elemento hijo por fuera del padding del padre, vía margin negativo): si el hijo tiene un `width` explícito (no `auto`) en cualquier regla que aplique (propia o heredada), hay que pisarlo a `auto` (o usar `calc(100% + Npx)` en vez de margin negativo) -- un margin negativo con `width` explícito puesto es una trampa silenciosa: no tira error, `getComputedStyle` no delata el problema, y el único síntoma real es que el elemento no se mueve/agranda como se esperaba.

## 2026-08-08 - Etiquetas de producto (PDF): segundo rediseño, con membrete, sobre `PdfPTable` reemplazado por posicionamiento absoluto

- **Decidido**: el diseño de etiqueta de la entrada anterior (mismo día, `PdfPTable`/`PdfPCell` apiladas verticalmente) se reemplazó por un layout con membrete CarniSys (logo real del repo, `Web/Content/img/CarniSys_Logo_sinSlogan.png`), calcado de una foto de referencia que pasó el usuario. El mecanismo de dibujo cambió de `PdfPTable` a posicionamiento absoluto (`ColumnText`/`PdfContentByte`) porque el layout de la referencia es asimétrico (nombre+logo en una fila, barcode+fecha en otra) -- no es una pila vertical simple.
- **Por qué no seguir con `PdfPTable`**: además de no poder expresar fácil un layout de 2 columnas por fila, ya había un bug real documentado (celdas con `FixedHeight` que no dibujan contenido sin avisar, ver la entrada anterior) -- posicionamiento absoluto lo evita de raíz, a costa de tener que calcular manualmente cada coordenada (más verboso, pero sin la trampa de FixedHeight).
- **Regla para futuro código con texto de longitud variable + iTextSharp posicionado absoluto**: `ColumnText.ShowTextAligned` (usado para precio, etiquetas cortas, fecha) **no tiene límite de ancho ni wrapea** -- solo usarlo para textos de longitud acotada/conocida. Para cualquier texto de longitud variable (nombres de producto, descripciones), usar `ColumnText` con `SetSimpleColumn(...)` + `Go()`, que sí respeta un rectángulo y wrapea de verdad. El primer intento de esta ronda usó `ShowTextAligned` para el nombre del producto y un nombre largo se desbordó visualmente sobre la etiqueta vecina en la grilla -- corregido antes de entregar, pero es el tipo de bug que se repite si no se recuerda la regla.
- **Regla adicional, encontrada por el usuario ya con el fix anterior aplicado**: acotar un texto a un `ColumnText`/`SetSimpleColumn` evita que se desborde HORIZONTALMENTE, pero **no garantiza que no choque visualmente con otro elemento dibujado aparte** (en este caso, la línea divisoria trazada con `PdfContentByte.MoveTo/LineTo` a una coordenada Y fija) -- el límite inferior de la columna (`lly`) define hasta dónde `ColumnText` *puede* escribir, no cuánto texto *elige* escribir; una segunda línea puede calcularse a pocos puntos de ese límite y terminar visualmente pegada o cruzada con algo dibujado justo ahí. Cuando dos elementos posicionados por separado deben coexistir sin superponerse, dejar un margen de seguridad real entre ambos (no que compartan la misma coordenada límite) -- no alcanza con que cada uno "no se pase" de un punto en teoría.
- **Alternativa descartada**: mantener el diseño anterior (sin logo, sin línea, fecha corta sin hora) y solo agregar el logo como un elemento más dentro de la `PdfPTable` existente. Se descartó porque la referencia pedida tiene nombre y logo **en la misma fila** (no apilados), algo que una `PdfPTable` de 1 columna no puede expresar sin volverse una tabla de 2 columnas con celdas de ancho variable -- más compleja que reescribir con posicionamiento absoluto, y el `FixedHeight` seguiría siendo una trampa latente.
- **Ver también**: `docs/09-cambios-y-pendientes/bitacora-de-cambios.md` (misma fecha, entrada "segundo rediseño") para el detalle completo de verificación.

## 2026-08-08 - Etiquetas de producto (PDF): rediseño con codigo de barras + 3 tamaños, en vez de portar el WinForms 1:1

- **Decidido**: al arreglar `GenerarEtiquetasPdf` (accion faltante, ver bitacora), no se porto el diseño del WinForms (`Presentacion/Cortes/formEtiquetas.cs`: nombre + precio + texto "COD: xxxx", tamaño fijo 60x35mm) tal cual -- se rediseño a pedido explicito del usuario ("diseñame el mejor tipo de etiqueta que puedas, para imprimir"). Agrega codigo de barras real (EAN-13/EAN-8 con Code128 de fallback) y ofrece 3 tamaños: 40x30mm, 60x35mm (el que ya usaba el WinForms), 100x50mm.
- **Por que agregar codigo de barras**: el usuario lo pidio explicitamente al confirmarlo por `AskUserQuestion`. El producto ya tiene un `Codigo` numerico que se imprimia solo como texto ("COD: 1234") -- un codigo de barras real permite escanear la etiqueta en caja igual que un producto de fabrica, sin trabajo adicional (iTextSharp 5.5.13.4, ya referenciado en `Web.csproj`, trae `BarcodeEAN`/`Barcode128` nativos).
- **Por que EAN con fallback a Code128, no un solo formato fijo**: no todos los `Codigo` de la base son EAN validos (dígito verificador incorrecto o largo distinto a 8/13) -- forzar EAN a un codigo que no lo es produce un barcode que no decodifica al valor real. Se prueba EAN-13 (con padding a 13 dígitos, porque `Codigo` se guarda como `long` y pierde el cero inicial de un EAN real), despues EAN-8, y si ninguno valida su dígito verificador se usa Code128 (acepta cualquier numero, sigue siendo escaneable, y el POS ya busca por el valor numerico de `Codigo`, no por el tipo de simbolo).
- **Por que 3 tamaños en vez de 1 fijo**: pedido explicito del usuario, confirmado por `AskUserQuestion` (opciones 40x30/60x35/100x50mm). Caso de uso distinto por tamaño: chica para gondola con espacio limitado (sin fecha, por espacio), mediana el tamaño ya conocido/impreso hoy, grande para mostrador donde conviene un codigo de barras mas facil de escanear de lejos.
- **Bug real de iTextSharp encontrado y evitado (no arreglado, es libreria de terceros)**: en `PdfPCell` con `FixedHeight` seteado, si el contenido (texto o imagen) necesita mas espacio vertical del que el `FixedHeight` permite, iTextSharp 5.5.13.4 **no dibuja nada y no tira excepcion** -- la celda queda vacia en silencio, sin ningun rastro en el PDF resultante ni en logs. Verificado con un harness de prueba aislado (PowerShell + reflection contra el DLL real, variando fuente/zona y extrayendo texto con `PdfTextExtractor` para confirmar presencia/ausencia). Esto afecto 2 partes del diseño nuevo: el precio (fuente 26pt/38pt en zonas ajustadas quedaba invisible; bajado a 19pt/28pt con margen) y el codigo de barras (el patron `new PdfPCell()` + `.AddElement(imagen)` no dibuja nada bajo `FixedHeight`; el fix fue usar el constructor directo `new PdfPCell(imagen, false)`). **Regla para futuro codigo con iTextSharp 5.x en este proyecto**: si se usa `FixedHeight` en una `PdfPCell`, verificar contenido real con `PdfTextExtractor` contra un PDF de prueba antes de asumir que "compila y da 200 OK" significa que el contenido se ve -- este bug no se detecta de otra forma.
- **Alternativa descartada**: portar el WinForms tal cual (mas rapido, cero riesgo de bug nuevo) -- se descarto porque el usuario pidio explicitamente "el mejor diseño posible", no una copia.
- **No resuelto, deuda conocida**: nombres de producto muy largos (>40 caracteres) pueden recortarse visualmente en la fuente mas chica de cada tamaño (mismo limite que ya tenia el WinForms original, nunca resuelto ahi tampoco). Ver bitacora para el detalle.

## 2026-08-08 - Reemplazo completo de la base de datos de la VM de produccion (`carnisys.com`) por la base local

- **Decidido**: la base `carnisys` de la VM de produccion se reemplazo integramente por la base local de desarrollo (no una migracion incremental, no un `ALTER`/sincronizacion de esquema como se hizo en SM/San Lorenzo -- un `RESTORE ... WITH REPLACE` completo). Cualquier dato que existiera solo en la base de la VM antes de este cambio se perdio.
- **Por que**: pedido explicito del usuario. Dado el riesgo (irreversible, produccion, puede haber datos reales de negocio), se detuvo el trabajo y se pregunto de forma directa antes de tocar nada (`AskUserQuestion`, 3 opciones: reemplazo completo / solo igualar esquema como en SM-SL / explicar primero) -- el usuario confirmo explicitamente "si, quiero el reemplazo completo previo backup por las dudas".
- **Salvaguarda aplicada**: backup completo de la base de la VM tomado ANTES de restaurar nada, guardado en el propio servidor (`carnisys_PRE-REEMPLAZO-LOCAL_20260808.bak`) -- si en algun momento se necesita recuperar algo de lo que habia antes del reemplazo, esta ahi. No se penso ni se pidio un plan de "recuperar datos especificos" mas alla de tener el backup disponible.
- **Alternativa descartada**: aplicar solo los scripts SQL pendientes (mismo criterio que SM/San Lorenzo, ver entrada del deploy del 2026-08-07) sin tocar los datos existentes de la VM -- el usuario la tenia disponible como opcion en la pregunta y eligio explicitamente no usarla.
- **Implicancia a futuro**: la VM de produccion ahora tiene datos de prueba/desarrollo, no datos reales de un cliente -- cualquier trabajo futuro contra esa base (deploys, verificaciones "con datos reales de produccion") debe tener esto en cuenta; ya no es representativa del uso real del sistema hasta que se vuelva a cargar con datos de negocio genuinos.

## 2026-08-08 - San Lorenzo (`TiposProducto.idEmpresa`): drift de esquema sin script versionado, no un bug de codigo

- **Decidido**: el fix de `/Productos` caido en San Lorenzo (`ALTER TABLE dbo.TiposProducto ADD idEmpresa`, aplicado por el usuario directamente en el servidor con backup previo) se acompaña con un script versionado nuevo en el repo, `Datos/DB-Procedures/20260808-Alter_TiposProducto_Add_IdEmpresa.sql` -- ver `docs/09-cambios-y-pendientes/bitacora-de-cambios.md` para el detalle completo del incidente.
- **Por que el gap existia**: el commit `8e25a6ff` (2026-06-19) agrego la columna `idEmpresa` a `TiposProducto` **solo en `Datos/Corte.cs`**, sin el `ALTER TABLE` acompanante en `Datos/DB-Procedures/` -- a diferencia de todos los demas cambios de esquema de este proyecto, que si versionan su script. Local llego a tener la columna por otro camino (no via un script de este repo tampoco -- probablemente aplicada a mano en su momento); San Lorenzo, unico servidor legacy (SQL Server 2008) que no se toco desde entonces, se quedo atras y el Web desplegado ahi (que ya corre codigo posterior a ese commit) empezo a fallar.
- **Backfill del script nuevo, con auto-deteccion en vez de un valor hardcodeado**: en vez de fijar `idEmpresa=1` a secas (correcto para San Lorenzo, confirmado contra su tabla `Empresas` real, pero un numero mágico si se reusa en otro servidor), el script chequea `COUNT(*) FROM Empresas` -- si es exactamente 1 (caso legacy single-empresa), usa ese `idEmpresa` real; si hay mas de una fila (multi-tenant, como local), agrega la columna con default 0 y no toca datos existentes, dejando esa migracion para una decision manual aparte.
- **Alternativa descartada**: replicar tambien la RLS nativa (`CREATE SECURITY POLICY`, ya presente en local sobre esta tabla) en San Lorenzo. Se descarto porque San Lorenzo corre SQL Server 2008 (RTM) -- RLS nativa es feature de SQL Server 2016+, no existe la sintaxis ahi. El filtro de aislamiento sigue siendo 100% a nivel de aplicacion (`WHERE (reservadoSistema=1 OR idEmpresa=@idEmpresa)` en el SQL de `Datos/Corte.cs`) en los servidores legacy, igual que ya documentado para otras tablas (ver entrada del deploy 2026-08-05, "Ambos corren SQL Server 2008 (RTM), sin soporte de RLS nativa").
- **Pendiente**: no se pudo confirmar si SM (`servidorsm.env`) tiene el mismo drift -- sin conectividad LAN durante esta sesion. Revisar cuando haya acceso.

## Objetivo

Registrar por que se eligio X y no Y, para proteger decisiones deliberadas de "correcciones" espontaneas de otra sesion/IA.

## 2026-08-08 - Regla nueva: todo `.cshtml`/`.cs`/`.js`/`.css` nuevo en `Web/` se agrega al `.csproj` en el mismo commit

- **Regla**: `Web.csproj` es un proyecto **old-style** (sin globbing de MSBuild) -- cada archivo nuevo bajo `Web/` necesita una entrada explicita (`<Compile Include>` para `.cs`, `<Content Include>` para `.cshtml`/`.js`/`.css`/etc.) o **queda invisible para el publish**, aunque compile y funcione perfecto en local. Al crear un archivo nuevo en `Web/`, agregarlo al `.csproj` en el mismo commit -- no un paso aparte, no "ya lo agrego despues".
- **Por que (2 casos reales, mismo bug, ver bitacora 2026-08-08)**: `_FacturasRows.cshtml` y `_ModalObservacionesExpendio.cshtml` se crearon, commitearon y funcionaron sin problema en local durante dias -- pero nunca se agregaron al `.csproj`. Recien se noto al desplegar a SM/San Lorenzo: el publish precompilado (`AspNetCompileMerge`) arma su lista de archivos a incluir/precompilar **desde el `.csproj`, no desde el filesystem** -- los 2 archivos quedaron afuera del build publicado, mientras que en local (`IIS Express`, sin ese paso de publish) el motor de vistas de ASP.NET los lee directo del disco y nunca noto la ausencia. Resultado: `/Ventas/POS` (y probablemente `/Ventas/Facturas`) rotos en produccion con "No se encuentra la vista parcial", nada roto en local ni en ningun `dotnet build`/`msbuild Build` de la sesion.
- **Por que la regla y no un parche puntual** (CLAUDE.md global, §5.1: error del mismo tipo 2+ veces = regla, no parche por archivo): paso 2 veces seguidas en el mismo lote de trabajo: sintoma identico (`.cshtml` real en disco + en git, ausente del `.csproj`), mismo tipo de archivo, mismo directorio (`Views\Ventas\`). Cualquier archivo nuevo agregado a mano (fuera de Visual Studio, que agrega la entrada solo al crear el archivo desde el propio IDE) tiene el mismo riesgo.
- **Verificacion mecanica sugerida antes de cada deploy a un servidor real**: `git log --diff-filter=A --name-only <ultimo-commit-deployado>..HEAD -- 'Web/*.cs' 'Web/*.cshtml' 'Web/*.js' 'Web/*.css'` (archivos nuevos desde el ultimo deploy) contra `grep` de cada nombre en `Web.csproj` -- si alguno no aparece, agregarlo antes de publicar. Asi se encontraron y confirmaron ambos casos el 2026-08-08.

## 2026-08-07 - Guardar una venta CtaCte con el carrito vacio: avisar y confirmar, no bloquear ni guardar en silencio

- **Decidido (version final, corregida por el usuario sobre un primer intento)**: si al editar una venta ya guardada se eliminan todos los productos y se presiona "Finalizar", **no se cancela la venta ni se le cambia la forma de pago**. Se guarda como una edicion normal: las lineas ya quedan invertidas (mecanismo preexistente, `CompletarAnulacionesVenta` -- ej. "Carre 5kg" anulado agrega "Carre -5kg"), el total da $0 solo, y se pasa por el modal de forma de pago de siempre (el usuario puede confirmar la misma forma de pago que ya tenia, CtaCte incluido).
- **Por que**: era parte de un bug real (ver bitacora, mismo dia -- guardar asi tiraba `NullReferenceException` por un bug de fondo en `CuentaCorriente.crearMovCtaCte`). Al arreglar el crash, un primer intento trato el caso como "cancelar la venta" (forzar `formaPago: "Efectivo"` con un aviso previo) -- el usuario corrigio esto explicitamente: la venta no se cancela, se guarda con su total en $0 via las lineas opuestas, manteniendo la forma de pago original.
- **Alternativa descartada (primer intento, revertido)**: tratar "guardar con carrito vacio" como sinonimo de "cancelar la venta" (mismo camino que el boton dedicado "Cancelar venta", forzando `formaPago: "Efectivo"` con confirmacion previa). Semanticamente incorrecto: vaciar el carrito de una venta ya guardada es una edicion valida con total $0, no una cancelacion -- forzar Efectivo reclasificaba incorrectamente una venta CtaCte.
- **Como quedo implementado**: en el handler de `btnFinalizar` (`Web/Views/Ventas/POS.cshtml`), el atajo `guardarVentaAnuladaDirectamente()` (forma de pago fija en Efectivo, sin pasar por el modal) queda reservado **solo para una venta nueva, nunca guardada** -- ahi si es un descarte real del carrito en curso, no una edicion. Para la edicion de una venta existente, el caso "carrito vacio" ya no es especial: cae en el mismo flujo que cualquier edicion con lineas activas.

## 2026-08-07 - `formCierresDeCaja`: "Puede ver" arrastra a "Puede editar" -- acoplamiento deliberado, no generalizable

- **Decidido**: para el permiso `formCierresDeCaja` (idForm=9) unicamente, "Puede ver" y "Puede editar" dejan de ser dos casillas independientes en `Usuarios/Permisos` -- otorgar "Puede ver" otorga automaticamente "Puede editar" (modificar un cierre de caja historico), con los mismos dias atras. Es una regla de negocio pedida explicitamente por el usuario para ESTE permiso puntual, no un cambio de comportamiento general del sistema de permisos.
- **Por que**: el usuario considera que quien puede ver el historial de cierres deberia poder corregirlo tambien -- no hay un caso de uso real de "puede ver pero no puede corregir un cierre historico" para este formulario en particular.
- **Por que quedo hardcodeado por `IdForm==9` y no como una propiedad generica del formulario** (ej. una columna `Formularios.VerImplicaEditar`): el modelo posteado por `Usuarios/Permisos` solo trae `IdForm` (int), no hay ninguna tabla de mapeo idForm<->clave estable en el proyecto hoy, y el pedido fue puntual para un unico formulario. Agregar una columna nueva y su UI de configuracion para un caso de uso de un solo formulario era mas alcance del pedido. **Si en el futuro aparece un segundo formulario con la misma regla**, ahi si conviene generalizar (columna en `Formularios` o una lista de idForm's en un solo lugar) en vez de seguir hardcodeando casos por `IdForm==N` en 2 archivos (`UsuariosController.GuardarPermisos` y `Permisos.cshtml`).
- **"Alcance" (Propios/Todos) queda fuera del acoplamiento**: a diferencia de "Puede editar"/"Dias editar" (que quedan derivados y grisados en la UI), el `<select>` de Alcance sigue siendo una eleccion real e independiente, editable, para esta fila -- no hay ninguna regla de negocio que diga que "Alcance" deba copiarse de ningun lado.
- **Leccion tecnica de paso**: un `<select>`/input con `disabled` en HTML no se incluye en el POST del formulario -- si se hubiera dejado "Alcance" deshabilitado (como Editar/Dias editar), el servidor habria recibido el default del binding (`false`/"Todos") en vez de lo elegido en pantalla. Por eso "Alcance" quedo intencionalmente habilitado en vez de espejado/grisado como el resto de la fila derivada.

## 2026-08-06 - Autorizacion temporal de Cierre de Caja: `MemoryCache` en vez de `Session` para la elevacion

- **Decidido**: la autorizacion temporal (step-up) que habilita a un usuario sin permiso a operar Cierre de Caja se guarda en `System.Runtime.Caching.MemoryCache.Default` (keyeada por `Session.SessionID`, 5 minutos de expiracion absoluta), NO en `Session[...]`.
- **Por que**: `CajasController` tiene `[SessionState(SessionStateBehavior.ReadOnly)]` -- eleccion deliberada preexistente, casi seguro para que las varias llamadas AJAX concurrentes de esta pantalla (historial, actividades, cajas abiertas, egresos) no se serialicen por el lock exclusivo de sesion que ASP.NET toma en modo `Required`/escritura. Escribir en `Session[...]` desde un controller `ReadOnly` no lanza excepcion pero tampoco persiste de forma confiable entre requests.
- **Alternativa descartada**: sacar el atributo `ReadOnly` de `CajasController` para poder usar `Session` normalmente. Se descarto porque cambiaria el comportamiento de TODAS las acciones de este controller (no solo las nuevas), con riesgo real de reintroducir contencion de lock en una pantalla que ya hace bastante AJAX concurrente -- cambio de alcance mucho mayor al pedido, y sin necesidad: `MemoryCache` resuelve lo mismo sin tocar el `SessionState` existente.
- **No hay convencion previa de `MemoryCache` en este proyecto** para reusar -- es la primera vez que se usa. Si aparece una necesidad similar (estado efimero por sesion, en un controller `ReadOnly`), este es el patron a seguir (`PermisosHelper.RegistrarElevacionCierre`/`ObtenerUsuarioAutorizadoCierre`/`RevocarElevacionCierre`).

## 2026-08-06 - Bug preexistente en `Negocio.Usuario.tienePermiso`: encontrado durante Cierre de Caja step-up, fix intentado y REVERTIDO

- **No decidido/no aplicado -- documentado a proposito para que quede a la vista**: mientras se probaba la autorizacion temporal de Cierre de Caja (ver bitacora ronda 9) con un usuario NO admin como autorizador, se encontro que `Negocio/Usuario.cs` (`tienePermiso`, ~linea 375) niega el permiso a cualquier usuario no-admin cuyo grant tenga `DiasPermitidosVer`/`DiasPermitidosEditar = -1` (pensado como "sin limite", mismo criterio que ya usa `ObtenerFechaMinimaPermitida` en `PermisosHelper.cs`): `DateTime.Today.AddDays(-(-1))` da MAÑANA, y "mañana <= fechaDesde(hoy)" es siempre falso. Verificado contra la base: **163 de ~210 `PermisosUsuarios` tienen ese valor**, y los 7 usuarios con `formCerrarCaja` otorgado (todos los no-admin incluidos) lo tienen. Los admins no lo notan porque bypassean el chequeo antes de llegar a esta cuenta.
- **Fix intentado**: tratar `DiasPermitidosVer`/`Editar < 0` como "sin restriccion de fecha" (saltear la cuenta), calcado del criterio de `ObtenerFechaMinimaPermitida`. Compilo y en un primer test (con un admin de por medio) parecio andar.
- **Por que se revirtio**: al probarlo con un usuario realmente sin el permiso, el fix le dio acceso de todos modos. Causa raiz: `Datos.Usuario.getPermisosUsuario` arma la lista de permisos del usuario con `Formularios f LEFT JOIN PermisosUsuarios p ON f.idForm=p.idForm AND p.idUsuario=@idUsuario`, y usa `COALESCE(p.diasPermitidosVer, -1)` -- es decir, **todo formulario que el usuario NUNCA tuvo otorgado tambien llega con `DiasPermitidosVer=-1`**, exactamente el mismo valor que un grant real "sin limite". Con el fix puesto, ambos casos (otorgado-sin-limite vs. nunca-otorgado) se volvian indistinguibles y ambos pasaban -- cualquier usuario terminaba con acceso a cualquier formulario. Se revirtio el cambio en el momento (confirmado con `git diff` que `Negocio/Usuario.cs` quedo identico al original) antes de seguir.
- **Estado actual**: el bug original (niega de mas para grants `-1` legitimos) sigue sin resolver, intencionalmente -- es preferible a la alternativa (dar de mas). El fix correcto necesita distinguir "hay fila en `PermisosUsuarios`" de "no hay fila", algo que la consulta actual no expone (ambos casos colapsan al mismo `-1`). Requiere tocar la capa de carga de permisos (`Datos/Usuario.cs`, posiblemente `Entidades.PermisosUsuarios`), no solo el chequeo de fecha -- alcance mayor al de esta ronda, se deja para una sesion aparte dedicada a esto.
- **Impacto practico mientras tanto**: cualquier feature que dependa de que un usuario NO ADMIN tenga un permiso "sin limite" (`-1`, el valor mas comun en la base) va a fallar aunque el permiso este otorgado. Concretamente, el step-up de Cierre de Caja de esta ronda **solo funciona hoy con un autorizador Admin** -- un supervisor no-admin con `formCerrarCaja` otorgado no puede autorizar a nadie hasta que esto se arregle.

- **ADDENDUM 2026-08-07 -- diagnostico corregido, esta entrada quedaba mal planteada**: `-1` **no significa "sin limite"** -- significa **"permiso no otorgado"**, a proposito. Confirmado por el usuario (dueño del negocio) y por el codigo de guardado ya existente, `UsuariosController.GuardarPermisos`: `DiasPermitidosVer = PuedeVer ? Math.Max(0, DiasVer) : -1` -- un permiso realmente otorgado SIEMPRE tiene `DiasPermitidosVer >= 0`; nunca es negativo por diseño. Las 163 filas con `-1` mencionadas arriba no eran "grants sin limite mal manejados" -- eran, en su enorme mayoria, permisos NUNCA tildados para ese usuario (el join de `Datos.Usuario.getPermisosUsuario` hace que "nunca otorgado" y "otorgado sin limite" fueran indistinguibles solo porque el segundo caso, en la practica, no existe: no hay ningun flujo de la UI que produzca un `-1` para un permiso que si fue tildado). `Negocio.Usuario.tienePermiso` **no tiene bug** -- niega correctamente cuando no hay grant real, que es el comportamiento esperado. Verificado end-to-end (ver bitacora 2026-08-07): otorgado `formCerrarCaja` de verdad a un usuario no-admin via `Usuarios/Permisos`, el step-up de Cierre de Caja funciono sin ningun cambio de codigo -- el "impacto practico" descripto arriba (autorizador no-admin no puede autorizar) **no existe**, era una conclusion basada en el diagnostico incorrecto. `tienePermiso` sigue sin tocarse -- no porque el fix sea riesgoso, sino porque nunca hizo falta ningun fix.

## 2026-08-06 - Observacion de expendio en POS Venta: columna nueva en `dbo.Expendios` + fix de un bug que la descartaba

- **Decidido**: agregar `observaciones nvarchar(MAX) NULL` a `dbo.Expendios` y un parametro `@observaciones` a `agregarExpendio` (SP), para que el comentario que ya se puede cargar en Punto de Expendio (boton "Comentario", agregado en una ronda anterior) se persista de verdad y pueda mostrarse al vendedor de POS Venta al cargar ese expendio.
- **Por que**: pedido de mostrar la observacion al cargar un expendio en Venta reveló que esa observacion nunca llegaba a la base -- `PuntosExpendioController.FinalizarPOS` la pisaba con `Observaciones = ""` antes de insertar (bug preexistente, no de esta sesion), y el SP ni siquiera tenia donde recibirla. Confirmado con `sqlcmd`/`sp_helptext` contra la base local antes de escribir el plan, no asumido.
- **Alcance confirmado con el usuario antes de tocar el esquema**: se le presento el hallazgo (la funcionalidad pedida no era solo visual, requeria cambio de base) y se pidio confirmacion explicita de seguir con el `ALTER TABLE`/`ALTER PROCEDURE` antes de aplicarlo.
- **Alternativa descartada**: dejar la observacion solo en memoria del lado del cliente (como esta hoy `PuntoExpendioEditVm.Observaciones`) y tratar de "pasarla" de alguna forma indirecta hacia Venta sin persistirla (ej. via `sessionStorage` compartido). Se descarto porque un expendio puede cargarse a una venta minutos u horas despues, en otra sesion de navegador incluso -- sin persistir en la fila del expendio, la observacion se pierde apenas se cierra o recarga la pestania de origen.
- **El mismo bug existe tambien en la accion `Guardar`/`Abrir.cshtml`** (flujo clasico no-POS de Punto de Expendio, linea 218 de `PuntosExpendioController.cs`) -- **no se toco**, queda fuera del alcance investigado esta ronda (el pedido era especificamente sobre el flujo POS). Si se usa ese flujo clasico para cargar expendios con comentario, el mismo problema va a reaparecer ahi.
- **Diseño del merge hacia el comentario de la venta**: sin marcadores de texto visibles en el comentario persistido (a diferencia del patron ya usado en `calculadora-billetes.js` para un caso similar) porque el comentario de la venta se imprime en el ticket -- un marcador tipo `[INICIO_DETALLE...]` ahi se veria mal. En cambio, el modulo (`ventas-expendios-pos.js`) recuerda en memoria el ultimo bloque de texto que el mismo inserto y lo busca/reemplaza por texto plano si hace falta actualizarlo -- evita duplicar sin ensuciar el ticket impreso.
- **Verificado**: `BACKUP DATABASE` antes de aplicar el `ALTER`. Flujo end-to-end probado con Chrome real + consultas a la base (con `SESSION_CONTEXT('IdEmpresa')` seteado -- la tabla tiene RLS, `RLS_Empresa`, sin ese contexto las queries de verificacion daban falsos "0 filas"). Ver la entrada correspondiente en `bitacora-de-cambios.md` (ronda 8) para el detalle completo de la verificacion.

## 2026-08-06 - `pos-multi-instance.js`: namespace por producto (`productKey`) en la clave de conflicto de "Duplicar POS"

- **Decidido**: agregar `productKey` a `window.POSMultiInstanceConfig` (leido por `Scripts/app/pos-multi-instance.js`, compartido entre `Ventas/POS` y `PuntosExpendio/POS`), incluido en la clave de `localStorage` que detecta "POS ya abierto". Expendio setea `productKey: 'expendio'`; Venta no setea nada (default `''`, clave de storage identica a la que ya tenia).
- **Por que**: al portar el boton "Duplicar POS" a Expendio (mismo modulo que usa Venta), la clave de conflicto original era solo `usuario+sucursal` -- sin distinguir PRODUCTO. Verificado en Chrome real: con Expendio abierto, entrar a `/Ventas/POS` (mismo cajero) disparaba un falso "POS ya abierto. Use Duplicar POS" -- el modulo los trataba como dos instancias del mismo POS, cuando son dos pantallas distintas que un cajero puede necesitar simultaneamente (ej. atender un punto de expendio y facturar una venta aparte).
- **Alternativa descartada**: no tocar `pos-multi-instance.js` y aceptar que Venta y Expendio compartan el limite de "una sola instancia abierta" entre ambos. Se descarto porque no hay ningun requerimiento de negocio que diga que un cajero no puede tener las dos pantallas abiertas a la vez, y el sintoma (bloqueo cruzado inesperado) se hubiera visto como un bug real la primera vez que alguien lo pisara en produccion.
- **Compatibilidad**: al dejar `productKey` vacio por default, la clave de Venta (`carnisys-pos-multi-v1-<user>-<sucursal>`) no cambio -- ninguna sesion de Venta ya abierta en produccion se ve afectada por este cambio.
- **Verificado**: con Chrome real, limpiando `localStorage` y abriendo Expendio y despues Venta en secuencia, ya no aparece el falso conflicto.

## 2026-08-05 - Facturas: filtros server-side + carga progresiva de 50 en 50, en vez de traer todo el rango y filtrar en el DOM

- **Pedido**: corregir la pantalla `/Ventas/Facturas` ("/Facturas") y sus filtros (sintoma reportado: el boton "Aplicar" de fecha nunca se habilitaba) y evaluar carga progresiva de 50 en 50 para mejorar la carga.
- **Hallazgo 1 (bug confirmado)**: `Web/Views/Ventas/Facturas.cshtml` tenia `disabled` hardcodeado en el boton "Aplicar", sin ningun script que lo sacara -- nunca se podia hacer click. Ademas: (a) un ternario roto en `_VentasFacturasFiltrosScripts.cshtml` (las 2 ramas devolvian el mismo string) hacia que el aviso "cambiaste las fechas" nunca apareciera en Facturas; (b) el `<select>` de Sucursal tenia un `onchange="this.form.submit()"` inline que competia con el listener JS real; (c) "Tipo de comprobante" tenia sus opciones (Etiqueta -> codigo AFIP) duplicadas en 2 archivos sin una fuente unica.
- **Hallazgo 2 (volumen real, cambia el analisis de performance)**: la base local tiene 98 facturas -- sin problema ahi. Pero **SM tiene 22.629 filas en `FacturaElectronica` (21.521 con CAE valido) y San Lorenzo 57.184 (55.706 con CAE)**, medido 2026-08-05. La pantalla traia **todo** el rango de fechas pedido en una sola respuesta (SQL sin `TOP`/`OFFSET`, sin indice sobre `fechaEmisionAfip`) y armaba el HTML completo server-side; los filtros de Vendedor/Cliente/Forma de pago/Tipo de comprobante eran 100% DOM (esconder/mostrar `<li>` ya renderizados, `Web/Controllers/VentasController.cs` no los recibia como parametro). Con ese volumen, es un problema real, no prematuro.
- **Decidido**: reescribir la pantalla para que pagine de verdad, reusando el unico patron de paginacion server-side ya probado en el proyecto (`ProductosController.BuscarGlobales` + `Datos.CatalogoGlobalProducto.ObtenerCatalogoGlobalPagina`, el modal "Catalogo Global" de Productos): CTE con `ROW_NUMBER() OVER(...)`, lotes de 50, `hayMas` calculado con "peek-ahead" (pedir 51 filas, si vuelven 51 hay pagina siguiente) en vez de un `COUNT` aparte, respuesta JSON con HTML pre-renderizado (`RenderPartialViewToString`). Adaptado a pagina completa (no modal): el trigger de scroll es sobre `window`, no un div `overflow:auto`.
- **Requisito derivado**: para que el scroll combine bien con los filtros, Vendedor/Cliente/Forma de pago/Tipo de comprobante **tuvieron que pasar a ser filtros SQL reales** (`WHERE ... LIKE`/`IN(...)`, parametrizados) -- si hubieran quedado como DOM, al scrollear solo habrian filtrado dentro de lo ya cargado, no contra el total real. Esto es un cambio de arquitectura, no solo un fix.
- **Nuevo dato encontrado al server-filtrar Forma de pago**: hay facturas reales con `formaPago = 'Contado'`, valor que no estaba en `Entidades.Venta.FormaPagoEnum` (Efectivo/Debito/Credito/CtaCte/Qr/Transferencia). Se agrego `Contado` a ese enum -- confirmado por grep que es **exclusivo de Web**: WinForms usa un enum homonimo distinto, `Entidades.Venta.formaPagoEnum` (minuscula, con el comentario "modificar los valores en formVentaCaja"), ningun archivo de `Presentacion/` referencia el de mayuscula. Cambio seguro, no toca WinForms.
- **`Cantidad`/`TotalFacturado` del header**: ya no pueden salir de sumar `Model.Facturas` (que ahora solo trae 50 filas) -- se agrego `Datos.Venta.ObtenerFacturasResumen` (mismos filtros que `BuscarFacturasPagina`, sin paginar, `COUNT`+`SUM`), pedido una sola vez por busqueda (pagina 1), no en cada scroll.
- **Bug de sintaxis SQL encontrado al migrar a CTE (no un bug de datos)**: la query original (`f.*, v.idVenta, v.observaciones, ...`) funcionaba como `SELECT` plano porque SQL Server tolera nombres de columna duplicados ahi (`FacturaElectronica` ya tiene sus propias columnas `idVenta` y `observaciones`; `SqlDataReader` ya resolvia por nombre contra la primera coincidencia, `f.*`, que venia primero). Envuelta en un CTE (necesario para el `ROW_NUMBER()`), SQL Server rechaza nombres duplicados de plano (`Column 'idVenta' was specified multiple times`). Fix: sacar `v.idVenta`/`v.observaciones` de la lista explicita (redundantes, `f.*` ya los trae) -- mismo dato leido, mismo comportamiento que antes, verificado con el error real de SQL Server al correr contra la base local.
- **Indice agregado**: `FacturaElectronica` no tenia indice sobre `fechaEmisionAfip` (la columna del rango de fechas y del `ORDER BY`) -- `IX_FacturaElectronica_FechaEmisionAfip` (`fechaEmisionAfip DESC` + `INCLUDE (CAE, idVenta)`), aplicado solo en local por ahora. Medido igual (sin el indice, contra datos reales de San Lorenzo, worst-case sin ningun filtro angosto): `BuscarFacturasPagina` sobre las 57.184 filas completas (2020-2026) -> 33ms; `ObtenerFacturasResumen` sobre las mismas -> 106ms. Ya rinde bien sin el indice a esta escala; el indice es una mejora de bajo riesgo para cuando el volumen siga creciendo, no una urgencia detectada.
- **Analizado y descartado, mismo pedido**: si convenia aplicar el mismo patron a `Ventas/Index`. Medido contra datos reales: `Ventas` tiene 190.634 filas en SM / 381.201 en San Lorenzo, pero **ya tiene indices** (`idx_venta_fechaVenta` + `IX_Ventas_FechaSucursalVendedorCliente`, cobertura amplia) -- la consulta `getAllVentas` tarda 3ms (ultimos 30 dias) a 61ms (25 anios de historial completo) en SM. A diferencia de Facturas, ahi el SQL nunca fue el cuello de botella. Se decidio no tocar `Ventas/Index` en este pase -- el unico riesgo remanente (renderizar un DOM grande con un rango de fechas muy amplio) es especulativo, sin evidencia de uso real, y el propio `Index()` ya arranca acotado a "hoy". Revisitar con evidencia concreta si en el futuro se reporta lentitud real ahi.
- **Verificado**: `Web.csproj` compila limpio. En vivo (Chrome real via CDP, login real): boton Aplicar habilitado, aviso de fechas pendientes visible, filtros de Cliente/Vendedor/Forma de pago (incluido "Contado")/Tipo de comprobante probados contra la base local via `/Ventas/BuscarFacturas` con resultados correctos (`cantidad`/`totalFacturado` consistentes). Paginacion real probada bajando temporalmente el tamano de pagina a 5 (los ~20 datos locales no alcanzan para probar 50): scroll dispara la carga de paginas siguientes, se detiene solo al agotarse (`hayMas=false`), sin duplicar encabezados de fecha en los limites de pagina (verificado programaticamente: 0 duplicados tras cruzar 3 limites de pagina). Vuelto a 50 antes de terminar. Contra produccion (San Lorenzo, solo lectura): ambas queries nuevas verificadas con datos y volumen reales, tiempos arriba.
- **Pendiente**: aplicar el `ALTER`/`CREATE INDEX` y el codigo a los servidores reales (VM produccion, SM, San Lorenzo) -- queda fuera de este pase, paso aparte con su propio backup y confirmacion, mismo procedimiento que trabajos anteriores.

## 2026-08-05 - Egreso Stock negativo y Stock Inicial ignorando la fecha pedida, en a_CierreStockWeb y a_ExistenciaStockPorSucursales

- **Hallazgo 1 (signo)**: `Web/Controllers/StockController.cs` (linea ~618) guarda los movimientos "Egreso Stock" con `cantKg` NEGATIVO (`cantidad = cantidad * -1` al guardar, aunque el usuario carga un numero positivo en la UI). Tanto `a_CierreStockWeb` como `a_ExistenciaStockPorSucursales` sumaban ese `cantKg` tal cual, sin corregir signo, a diferencia de `EgresoMovimiento`/`EgresoElaborado`/`Ventas` (siempre positivos) y del legacy `a_CierreStock` (que si forzaba el signo con `*-1`). Resultado: `Egr.Stock`/`EgresoStock` salia negativo en pantalla, `Tot.EGR`/`TotalEgresos` quedaba subestimado, y `Faltante`/`StockActual` (que restan `EgresoStock`) terminaban sumandolo -- un egreso de stock inflaba el stock calculado en vez de reducirlo.
- **Hallazgo 2 (fecha inicial en a_CierreStockWeb)**: `StockInicial` se calculaba con `MAX(fechaCompra)` incondicional (el cierre mas reciente sin condicion), ignorando `@fechaDesde`. Sin efecto en modo "Stock Actual"/"Stock Retroactivo" (ahi `@fechaDesde` ya es el cierre mas reciente), pero incorrecto en modo "Cierre Stock", donde `ReportesController.AplicarConfiguracionFechasSegunReporte` pasa a proposito el *ante*-ultimo cierre como `@fechaDesde` (para auditar el periodo entre 2 cierres consecutivos via `Faltante`). El SP seguia usando el ultimo cierre como punto de partida, comparandolo consigo mismo en vez del periodo real.
- **Hallazgo 3 (fecha en a_ExistenciaStockPorSucursales)**: mismo patron -- `FechaUltimoCierre` (usado como `StockInicial`) se calculaba con `MAX(c.fechaCompra)` sin acotar por `@fechaHasta`. Ya identificado y mitigado el 2026-08-05 (entrada siguiente, cronologicamente anterior en esta sesion) solo con un guard de UI que bloquea pedir una `FechaHasta` anterior al ultimo cierre -- el SQL de fondo seguia sin respetar `@fechaHasta`.
- **Decidido**: corregir los 3 en un solo pase, por ser la misma familia de problema (calculo de stock por movimientos). `Datos/DB-Procedures/20260805-Alter_a_CierreStockWeb_SignoEgresoStockYFechaInicial.sql` y `20260805-Alter_a_ExistenciaStockPorSucursales_SignoEgresoStockYFechaUltimoCierre.sql`. Fix 1: multiplicar por `-1` la fila `'Egreso Stock'` al acumularla en `#Operaciones`, para que quede como magnitud positiva. Fix 2: `StockInicial` en `a_CierreStockWeb` ahora usa `c.fechaCompra LIKE @fechaDesde` (mismo patron que `StockCierre` ya usaba con `@fechaHasta`), en vez del `MAX()` incondicional. Fix 3: `FechaUltimoCierre` en `a_ExistenciaStockPorSucursales` ahora acota el `MAX()` con `AND c.fechaCompra <= @fechaHasta` en la condicion del `LEFT JOIN` (no en el `WHERE`, para no perder sucursales sin cierres antes de esa fecha) -- todo lo que ya usa `s.FechaUltimoCierre` rio abajo queda corregido automaticamente sin tocar nada mas.
- **Sin cambios de C#**: confirmado (agente de exploracion dedicado) que `Datos/Corte.cs`, `Negocio/Corte.cs`, `ReportesController.cs`, `StockController.cs` y las vistas (`Reportes/Index.cshtml`, `Stock/_TablaExistenciaPorSucursales.cshtml`) hacen transporte fiel de los valores del SP hacia la UI (`Convert.ToSingle`/`.ToString("N3")` puro), sin ninguna transformacion de signo. El fix es 100% SQL.
- **Guard de UI de Existencia por Sucursales** (agregado en la entrada anterior de este mismo dia): se deja intacto, no se toca en este cambio -- queda como proteccion redundante-pero-inofensiva ahora que el SQL de fondo tambien respeta `@fechaHasta`. Sacarlo (para habilitar consultas historicas reales en esa pantalla) es una decision de producto aparte, no un requisito de este fix.
- **Alternativa descartada**: tocar `StockController.cs` para dejar de guardar `cantKg` negativo en "Egreso Stock". Se descarto porque el legacy `a_CierreStock` (compartido con WinForms, nunca se toca) depende de que ese valor venga negativo -- su propio `Egr.Stock` hace `(SUM(...))*-1` asumiendo el signo de origen negativo. Cambiar el signo en el guardado hubiera roto WinForms sin forma de probarlo. Corregir el signo en el punto de lectura (los 2 SPs de Web) es mas seguro y ademas es exactamente donde ya viven las otras 2 diferencias de comportamiento vs. el legacy (ver entrada del 2026-08-05 sobre `a_CierreStockWeb`).
- **Verificado**: al planificar se asumio "base local sin compras (0 filas)" en base a `SELECT COUNT(*) FROM dbo.Compras` -- resulto ser una lectura incorrecta: la RLS ocultaba 60 filas reales sin el `session_context` de admin (mismo patron ya visto con `Corte`/`Sucursal`). Corregido antes de verificar (`sp_set_session_context 'EsAdminCarniSys', 1`). Verificacion en dos niveles: (1) escenario sembrado y controlado (fechas en 2030, sin colision con datos reales), dentro de `BEGIN TRAN`/`ROLLBACK` (sin residuo: `Compras` volvio a 60 filas), contra ambos SPs antes y despues de aplicar los `ALTER` -- los 3 hallazgos se reprodujeron exactamente antes del fix (`Egr.Stock=-15`, `Faltante=30` en vez de `-20`, `FechaUltimoCierre` tomando un cierre posterior al `@fechaHasta` pedido) y se corrigieron exactamente despues (`Egr.Stock=15`, `Faltante=-20`, `FechaUltimoCierre` respetando `@fechaHasta`); (2) dato real preexistente en la base local (`idCompra=9007`, "Egreso Stock" de San Lorenzo, `cantKg=-1.0`, producto "CARRE") confirmado de punta a punta contra la app real corriendo (login real via `curl` + IIS Express, sin overrides de fecha -- las fechas las calculo el controller como lo haria un usuario real): `Egr. Stock` paso de mostrarse negativo a `1,000` en las dos pantallas (Reportes y Existencia por Sucursales), con `Tot.Egr`/`TotalIngresos`/`StockActual` matematicamente consistentes con la formula. `Web.csproj` compila limpio (sin cambios de C#, mismos warnings preexistentes no relacionados).
- **Pendiente**: aplicar ambos `ALTER` a la VM de produccion (`carnisys.com`) -- SM y San Lorenzo ya quedaron aplicados y verificados (ver entrada siguiente, "Deploy del fix de Egreso Stock/Stock Inicial a SM y San Lorenzo").

## 2026-08-05 - Deploy del fix de Egreso Stock/Stock Inicial a SM y San Lorenzo

- **Decidido**: aplicar los 2 `ALTER PROCEDURE` de la entrada anterior (`20260805-Alter_a_CierreStockWeb_SignoEgresoStockYFechaInicial.sql`, `20260805-Alter_a_ExistenciaStockPorSucursales_SignoEgresoStockYFechaUltimoCierre.sql`) a los 2 servidores reales que ya tenian `a_CierreStockWeb`/`a_ExistenciaStockPorSucursales` desplegados (SM, San Lorenzo), a pedido explicito del usuario. La VM de produccion `carnisys.com` queda pendiente, no se toco en este pase.
- **Conectividad**: en ambos servidores el SQL Server acepta conexion TCP directa (sin necesidad de SSH ni RDP para el trabajo de base de datos): SM en `192.168.0.151\sqlexpress` (LAN), San Lorenzo con los datos de `SQL_INSTANCE`/`SQL_DB`/`SQL_USER`/`SQL_PASSWORD` ya presentes en `~/hosts/sanlorenzo.env` (convencion CLAUDE.md SS4.1). Ambos corren **SQL Server 2008 (RTM)**, sin soporte de RLS nativa (feature de 2016+) -- a diferencia de la base local, no hace falta `sp_set_session_context` para ver los datos ahi.
- **Adaptacion necesaria**: los 2 scripts tienen `USE [CarniSys]` hardcodeado (nombre de la base local). SM y San Lorenzo llaman a su base `SuperCerdo`, no `CarniSys` (confirmado por la connection string real, ya presente comentada en `Web/Config/connectionStrings.config`, y por `DB_NAME()` contra cada servidor). Se generaron copias temporales (fuera del repo, en el scratchpad de la sesion) con `USE [SuperCerdo]` en vez de `USE [CarniSys]`, mismo patron ya usado en sesiones anteriores de este proyecto para estos 2 servidores -- **los archivos versionados en `Datos/DB-Procedures/` no se tocaron**, siguen apuntando a `CarniSys` porque son la fuente de verdad para local/VM.
- **Regla del repo aplicada** (`docs/09-cambios-y-pendientes/riesgos-conocidos.md`, REGLA 2026-08-03): antes de alterar, se trajo el texto real de ambos SPs de cada servidor (`OBJECT_DEFINITION`) y se comparo contra los scripts base `20260804-Create_a_CierreStockWeb.sql`/`20260804-Alter_a_ExistenciaStockPorSucursales_FiltroEmpresaEnMapaCorte.sql` -- **0 drift en los 2 servidores** (diff identico al de la base local: solo diferencias de boilerplate `USE`/`GO`, cuerpo del SP byte-a-byte igual). Seguro aplicar el `ALTER` de la entrada anterior tal cual, sin reconciliar nada primero.
- **Backup previo** (ambos servidores, antes de alterar): `BACKUP DATABASE SuperCerdo TO DISK = '...\SuperCerdo_pre-fix-egresostock-signo-20260805.bak'` en la carpeta de backup por defecto de cada instancia (`c:\Program Files (x86)\Microsoft SQL Server\MSSQL10.SQLEXPRESS\MSSQL\Backup\`). SM: ~38.500 paginas (~300 MB). San Lorenzo: ~43.400 paginas (~340 MB). Ambas bases en modelo de recuperacion FULL, `ONLINE`.
- **Verificado** (ambos servidores, mismo criterio que en local): (1) `OBJECT_DEFINITION` post-`ALTER` confirma que el texto de los 2 SPs en cada servidor ahora contiene el multiplicador de signo de `EgresoStock` y el nuevo filtro de fecha; (2) chequeo funcional contra un movimiento "Egreso Stock" real y reciente de cada servidor (no un dato sembrado): SM, `idCompra=11482` (2026-08-04, sucursal San Martin, corte "Osobucco", `cantKg=-8.3` en la tabla) -> `EXEC a_ExistenciaStockPorSucursales` devuelve `EgresoStock=8.300` (positivo), `TotalEgresos=11.700`, `StockActual=13.533`, consistente con la formula. San Lorenzo, `idCompra=4598` (2026-08-05, mismo dia, sucursal San Lorenzo, corte "Ensalada Grande", `cantKg=-1.0`) -> `EgresoStock=2.000` (suma dos egresos reales del mismo producto), `TotalEgresos=2.000`, `StockActual=4.000`, consistente. Sin necesidad de sembrar datos de prueba en ninguno de los dos -- ambos tenian movimientos reales de "Egreso Stock" recientes para verificar contra.
- **No se toco codigo ni se redeployo la app** en ninguno de los 2 servidores -- este cambio es exclusivamente de base de datos (2 `ALTER PROCEDURE`), el codigo C# ya desplegado no necesita cambios (ver entrada anterior, "Sin cambios de C#").
- **Pendiente**: VM de produccion (`carnisys.com`) sigue sin este fix -- aplicar cuando se confirme, con el mismo procedimiento (backup, verificar drift, `ALTER` con `USE [CarniSys]` sin adaptar ya que esa VM si usa ese nombre de base).

## 2026-08-05 - Existencia por Sucursales: bloquear FechaHasta anterior al ultimo cierre, en vez de arreglar el SQL para fechas arbitrarias

- **Hallazgo**: `a_ExistenciaStockPorSucursales` calcula el "ultimo cierre" por sucursal (`FechaUltimoCierre`, usado como punto de partida del calculo de stock) con un `MAX(fechaCompra)` sin ningun limite de fecha -- ignora `@fechaHasta` por completo. Mismo patron de bug ya encontrado en `a_CierreStockWeb` (ahi ignora `@fechaDesde`), confirmado por el usuario como el mismo error de fondo repetido en las dos SPs.
- **Por que no es un bug en el uso normal**: la pantalla casi siempre se consulta "a hoy" (`@fechaHasta` por defecto es `GETDATE()`), y un cierre de stock nunca puede ser del futuro -- ahi "el cierre mas reciente que existe" y "el mas reciente hasta la fecha pedida" son lo mismo. El problema aparece unicamente si se pide una `FechaHasta` pasada y hubo un cierre mas nuevo desde entonces.
- **Decidido**: en vez de reescribir el SQL para que `FechaUltimoCierre` respete `@fechaHasta` (mas invasivo, toca la CTE que arma `#Sucursales`), se decidio evitar que la combinacion invalida de inputs llegue a pasar: la pantalla ahora muestra siempre el/los ultimo/s cierre/s relevantes (`Negocio.Corte.ObtenerUltimosCierresPorSucursal`, reusando `Datos.Corte.fechaUltimoCierreStock_Sucursal` ya existente) y no deja pedir una `FechaHasta` anterior a ese limite -- ni desde el navegador (`min` del `datetime-local`, actualizado por AJAX al cambiar de sucursal) ni desde el servidor (`StockController.BuscarExistenciaPorSucursales` rechaza la consulta con un mensaje claro en vez de correrla).
- **Alternativa descartada**: acotar el `MAX()` de `FechaUltimoCierre` por `@fechaHasta` directamente en el SP. Se descarto por ahora porque resuelve el sintoma en esta pantalla puntual pero no en `a_CierreStockWeb` (que tiene el mismo problema con `@fechaDesde` y esta en un flujo distinto, Reportes en vez de Stock), y el usuario pidio el bloqueo especificamente para "existencia por sucursal" -- una correccion de SQL mas amplia queda pendiente como decision aparte si se pide.
- **No resuelto todavia**: `a_CierreStockWeb` (Reportes -> Cierre Stock) sigue teniendo el bug de origen sin corregir ni mitigar -- ese fue analizado y explicado, pero no se toco a pedido explicito del alcance ("para existencia por sucursal").
- **Verificado**: build limpio de `Web.csproj` y `Presentacion.csproj` (el cambio en `Negocio/Corte.cs` es aditivo, no afecta WinForms). Probado con Chrome/curl real logueado como usuario real: pantalla inicial muestra el limite correcto por sucursal ("San Lorenzo: 01/08/2026 14:50"); endpoint AJAX `ObtenerFechaMinimaExistencia` con "Todas" devuelve el desglose completo (San Lorenzo 01/08, San Martin 01/07) y el maximo entre ambas (mas restrictivo, correcto para "Todas"); pedir `FechaHasta=2026-07-01` (anterior al cierre) devuelve el mensaje de rechazo sin ejecutar el calculo invalido; pedir una fecha valida sigue funcionando normal (44 filas).

## 2026-08-05 - `a_CierreStockWeb`: SP nuevo en vez de alterar `a_CierreStock` (compartido con WinForms)

- **Decidido**: para el segundo tramo de la auditoria de performance de reportes de stock, en vez de modificar `dbo.a_CierreStock` (el SP que arma Stock Actual/Cierre Stock), se creo un SP nuevo y exclusivo de Web, `dbo.a_CierreStockWeb` (`Datos/DB-Procedures/20260804-Create_a_CierreStockWeb.sql`), con metodos C# propios (`Datos/Corte.cs:CierreStockWeb`, `Negocio/Corte.cs:CierreStockWeb`). `Web/Controllers/ReportesController.cs` (`CargarReporteStockDesdeCierres`, `CargarReporteProyeccionVentasVsStock`) pasa a usar el SP nuevo. El metodo `CierreStock` existente y el SP viejo **no se tocaron**.
- **Por que**: se confirmo por `grep` en `Presentacion/` que `a_CierreStock` esta genuinamente compartido con WinForms -- 4 llamadas reales (`Presentacion/Cortes/formReporteStock.cs` x2, `Presentacion/Stock/formStockActual.cs`, `Presentacion/Stock/formAddOrEditStock.cs`). Por regla del proyecto WinForms no se toca nunca, y no hay forma de probar un cambio ahi (sin entorno de testing de esa capa). Forkear el SP evita el riesgo por completo: WinForms sigue con el SP intacto para siempre, Web gana el SP optimizado.
- **De yapa, no solo performance**: al escribir el SP nuevo se encontraron 2 bugs de comportamiento reales en el viejo `a_CierreStock`: (1) arma `#AllCortes`/`Sucursal` con un `CROSS APPLY (SELECT TOP 1 ... FROM Sucursal WHERE idSucursal=@idSucursal)`, que sin filas correlacionadas se comporta como `INNER JOIN` -- con `@idSucursal=0` (el default que manda `ReportesController` cuando el usuario no filtra por sucursal) el reporte devuelve **0 filas sin error**; (2) el filtro `enCierreStock=1` se aplicaba en un punto distinto segun si habia `@texto` o no, haciendo que buscar por texto se salteara ese filtro. El SP nuevo corrige ambos: soporta `@idSucursal=0` = "todas las sucursales de la empresa" (mismo patron `CROSS JOIN #Sucursales` que ya usa `a_ExistenciaStockPorSucursales`) y aplica `enCierreStock=1` una sola vez, temprano, en `#AllCortes`, antes de cualquier otro filtro.
- **Alternativa descartada**: alterar `a_CierreStock` in-place con los mismos fixes de performance/bugs. Se descarto por el riesgo de romper WinForms sin posibilidad de probarlo -- unico consumidor no verificable de este repo.
- **Tambien parte de este cambio (Track A)**: `dbo.a_ExistenciaStockPorSucursales` (100% exclusivo de Web, confirmado por grep) se corrigio in-place agregando `idEmpresa` al ancla de la CTE recursiva `#MapaCorte` (`Datos/DB-Procedures/20260804-Alter_a_ExistenciaStockPorSucursales_FiltroEmpresaEnMapaCorte.sql`), sacando del calculo las filas de catalogo global/otras empresas. Se agrego ademas el indice faltante `IX_CortePuntoStockSucursal_Sucursal` (`idSucursal`) que `FindPorSucursal` necesitaba.
- **Verificado**: parity check fila por fila entre `a_CierreStockWeb` y `a_CierreStock` (mismos parametros, usando la fecha real de produccion `fechaUltimoCierreStock_Sucursal` -- una fecha de prueba arbitraria da falsos positivos por el bug de `enCierreStock`) -- **0 diferencias** salvo los 2 comportamientos ya aceptados arriba. Track A aplicado y verificado contra la base local (~328ms, corrida limpia). Ver detalle completo de riesgos en `docs/09-cambios-y-pendientes/riesgos-conocidos.md` (2026-08-04, entrada de los 2 bugs).
- **Pendiente**: aplicar ambos scripts SQL y el `CREATE INDEX` en los servidores reales (VM produccion, SM, San Lorenzo) -- solo corridos contra la base local hasta ahora.

## 2026-08-05 - Separar el catalogo global de productos en tabla propia (`dbo.CatalogoGlobalProducto`)

- **Decidido**: sacar el catalogo global de productos (compartido entre todas las empresas) de `dbo.Corte` a una tabla fisica propia, `dbo.CatalogoGlobalProducto`. `dbo.Corte` queda solo con filas `idEmpresa > 0`. El flujo de "leer global -> copiar a la empresa con su idEmpresa" (ya existente: `ClonarProductoGlobal` + `InsertarCorteEnEmpresa` + `CatalogoGlobalImportacionProductos`) se mantiene igual, solo cambia el origen de la lectura.
- **Por que**: en la base local, `dbo.Corte` tenia ~102.008 filas totales, de las cuales ~102.000 eran catalogo global (`idEmpresa=0`) y solo un puñado (56-65 segun el ambiente) eran productos reales de una empresa. Esto ya habia causado 3 incidentes de performance documentados (`docs/07-operacion-y-soporte/incidencias-frecuentes.md`, REGLA del 2026-08-01: `a_ExistenciaStockPorSucursales`, `a_CierreStock`, `Acum_Ventas`) por SPs que confiaban en la RLS "empresa o global" sin filtrar explicitamente. Los 2 indices agregados el 2026-08-03 (`IX_Corte_Codigo_IdEmpresa`, `IX_Corte_IdCorteMaestro_Solo`) mitigaban el sintoma puntual pero no el problema de fondo: cualquier query nueva sobre `Corte` seguia arrastrando ~102K filas ajenas por diseño.
- **Alternativa descartada**: seguir agregando indices/filtros parche sobre la tabla mezclada. Se descarto porque no resuelve el problema de raiz (el volumen sigue ahi, cualquier nuevo desarrollador puede repetir el mismo bug de "SP confia en la RLS y trae de mas"), y porque el volumen real (~102K vs ~60 filas utiles) hace que separar sea la solucion proporcional al problema, no over-engineering.
- **Riesgo evaluado y descartado**: `Datos/Corte.cs` y `Negocio/Corte.cs` son compartidos con WinForms (`Presentacion.csproj` referencia `Negocio.csproj`, que referencia `Datos.csproj`). El usuario confirmo explicitamente (2026-08-05) que WinForms no usa el catalogo global -- Compras y el resto de `Presentacion/` operan solo sobre productos ya cargados en la empresa (`buscarCodigoCorte`, `buscarCorteSinMaestro`). Por eso no hizo falta auditar el SQL real de esos 2 SPs legacy (sin script versionado en el repo) antes de borrar las filas `idEmpresa=0` de `Corte`.
- **No se toco**: la RLS `fn_rls_empresa_o_global_v2`/`fn_rls_block_empresa_o_global_v2` sobre `dbo.Corte` -- su rama "OR idEmpresa=0" queda inerte pero inofensiva una vez que Corte ya no tiene filas globales. Simplificarla es un cambio de seguridad aparte, fuera de este pedido. Tampoco se toco `dbo.TiposProducto`, que tiene el mismo patron (`idEmpresa=0` mezclado) a menor escala -- fuera de alcance, el pedido fue puntual sobre productos.
- **Verificado**: migracion corrida y verificada contra la base local (`.\sqlexpress`/`carnisys`): 101.943 filas migradas (coincide exacto con el numero ya documentado en incidentes previos), conteo `Corte WHERE idEmpresa=0` = conteo `CatalogoGlobalProducto` post-migracion. Flujo probado de punta a punta contra la app real (login + `/Productos/Index` + modal de catalogo global + importar un producto de prueba): el producto importado aparece en `dbo.Corte` con `idEmpresa=1` y los datos correctos. `Web.csproj` y `Presentacion.csproj` compilan limpios.
- **Borrado ejecutado** (2026-08-05, misma sesion, confirmacion explicita del usuario, con backup previo de la base local en `...\MSSQL\Backup\CarniSys_pre-delete-corte-idempresa0_20260805.bak`): `Datos/DB-Procedures/20260804-Delete_Corte_IdEmpresa0.sql` corrido contra la base local. `dbo.Corte` paso de 102.008 a 66 filas (0 con `idEmpresa=0`); `dbo.CatalogoGlobalProducto` conserva las 101.943 filas migradas. **Solo aplicado en la base local de desarrollo** -- produccion (VM `carnisys.com`) y los servidores SM/San Lorenzo quedan pendientes, a correr aparte con su propio backup y confirmacion cuando el usuario lo pida (SM/San Lorenzo, segun lo documentado, no tienen catalogo global mezclado para empezar -- verificar antes de asumir que hace falta ahi).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Corte.cs, bloque Stock/Reportes (Etapa 11c, ultima sub-etapa de Corte.cs)

Ultima de las 3 sub-etapas en las que se dividio el resto de `Corte.cs` (Embutido en 11a, Movimiento en 11b). Cubre los reportes de kardex de stock (inicial/ingresos/egresos/actual) que alimentan Reportes y Existencia por Sucursales en Web, y varias pantallas de WinForms.

- **Excluidos, decision explicita del usuario** (riesgo/complejidad muy por encima del resto de la migracion, no un default del proceso): `reiniciarStockReal`/`reiniciarStockTeorico` (no-ops confirmados en Etapa 6), `CierreStock` (dispatcher a `StockCierre_2`/`a_CierreStock` segun `nroCierre`), `TotalKgsCortePorCompra` (llama a `a_CierreStock` directo). Los SPs subyacentes `StockCierre_2`/`a_CierreStock` (+1000 lineas c/u, cascadas `UNION` multi-nivel de jerarquia `Corte` sin CTE recursivo limpio) siguen existiendo intactos para WinForms (4 llamadas reales, nunca se tocan).
- **Hallazgo nuevo durante el scoping, tambien excluido con confirmacion del usuario**: `StockIngresoEgreso` referencia `dbo.ActualizacionStock`/`dbo.ActualizacionStockPorCorte`, **tablas que no existen en la base** (confirmado con `sys.tables`) -- la rama que las usa es parte del `UNION` principal de la subconsulta "Ingreso", no condicional, asi que el SP entero tira "Invalid object name" **cada vez que se ejecuta**, sin excepcion. Unico caller es WinForms (`formReporteStock.cs`), sin caller Web -- confirmado permanentemente roto en produccion hoy, sin que nadie lo haya reportado (bajo uso). No se "arregla" ni se porta la version rota: queda fuera de alcance, documentado.
- **10 metodos migrados** (todos verificados con `sp_helptext`/lectura directa de `Datos/Corte.cs` contra la base y el codigo vivos, no contra el snapshot desactualizado de `docs/08-relevamiento/`): `reporteTeoricoReal`/`imprimirTeoricoReal` (mismo SP `StockTeoricoReal`, `imprimirTeoricoReal` sin caller vivo en ningun lado -- se migro igual por costo marginal ~0, mismo criterio que `modificarMovimiento`/`quitarCortesPorMovimiento` en 11b), `fechaUltimoCierreStock_Sucursal` (trivial, sobre `compras` ya migrada), `CierreStockWeb` (SP `a_CierreStockWeb`), `acum_Ventas` (SP `Acum_Ventas` -- **no estaba en el inventario original de 38 metodos**, aparecio al leer `Datos/Corte.cs` completo; auto-contenido, sin `StockCorteSucursal` ni tablas nuevas), `TotalPorCortesVendidos`, `TotalMovimientosPorCorte`, `ObtenerSerieVentasPorCorte` (Text), `Balance` (SP `BalanceConsFinal_FecDesde_Hasta`, con post-procesamiento en C# replicado literal), `ObtenerExistenciaPorSucursalesPlano` (SP `a_ExistenciaStockPorSucursales`).
- **Sin schema ni migracion de datos**: las 12 tablas fuente que estos 10 metodos leen (`corte`, `sucursal`, `compras`, `corteporcompra`, `mediares`, `corteproveedor`, `ventas`, `lineaventa`, `movimiento`, `cortepormovimiento`, `embutidos`, `corteporembutido`) ya estaban migradas en etapas anteriores -- primera sub-etapa de `Corte.cs` puramente de capa de queries.
- **Traduccion de los 2 SPs con temp tables + CTE recursivo** (`a_CierreStockWeb`, `a_ExistenciaStockPorSucursales`): `#TempTable` -> CTE, con `WITH RECURSIVE` reemplazando la jerarquia recursiva madre/hija de `Corte` (`OPTION (MAXRECURSION 20)` -> columna `nivel` + `WHERE nivel < 10` explicito en la rama recursiva, Postgres no tiene limite nativo equivalente). Sin funciones/procedimientos Postgres, mismo criterio arquitectonico que el resto del proyecto (SQL de texto parametrizado via `NpgsqlCommand`). `a_CierreStockWeb`/`a_ExistenciaStockPorSucursales` resultaron ser SPs re-escritos recientemente (headers fechados 2026-08, no legado) con fixes de calculo ya documentados en entradas anteriores de este archivo -- se tradujeron **sobre esa version corregida**, verificada fresca contra la base viva (no contra la version del snapshot de relevamiento, que estaba desactualizada).
- **`c.fechaCompra LIKE @fechaDesde/@fechaHasta`** (SQL Server, sin wildcards en el parametro, equivalente a `=` tras la conversion implicita datetime->string): traducido directo a `= @fechaDesde/@fechaHasta` en Postgres -- el propio SP documenta la intencion como "cargado EXACTO en @fecha", sin ambiguedad.
- **`CAST(codigo AS NCHAR)` sin longitud**: una suposicion incorrecta de una sesion anterior ("trunca a 1 char") quedo descartada al re-verificar -- el default real de `CAST`/`CONVERT` sin longitud en SQL Server es 30, y la comparacion ignora el padding de espacios, asi que nunca fue un bug. Traducido como `codigo::text = @texto` en Postgres (sin necesidad de padding, mismo resultado).
- **`TotalMovimientosPorCorte`: precedencia AND/OR sin parentesis preservada tal cual** -- el segundo branch del `UNION` original es `(A AND B AND C AND D) OR E`, no `A AND B AND C AND (D OR E)`: si el texto buscado matchea `idMovimiento` como numero, se saltea el filtro de fecha/sucursal/corte por completo. Bug real preexistente, pero sin caller Web (solo WinForms) y de bajo impacto (busqueda, no calculo de stock) -- se replico igual, no se "arreglo" de paso (regla de scope, SS5 CLAUDE.md).
- **Nueva regla de la sesion, confirmada dos veces**: SQL Server `CarniSys` tiene **su propia RLS** (`RLS_Empresa`/`fn_rls_empresa_o_global_v2`, analoga a la de Postgres). `sqlcmd -E` (autenticacion Windows) no setea `SESSION_CONTEXT` y **filtra silenciosamente a 0 filas** cualquier query de datos contra tablas con RLS -- sin error, indistinguible de "la base esta vacia". Causo una falsa alarma de "perdida de datos" en esta sesion hasta que se encontro `DB_RLS_BYPASS_USER=cs_admin` ya documentado en `~/hosts/carnisys-web-local.env`. Toda query de datos futura contra esta base debe usar ese login (o el login real de la app con `SESSION_CONTEXT` seteado), nunca `-E`. Guardado en memoria persistente del agente para no repetirlo.
- **Verificacion numerica cross-engine** (no solo "no tira error" -- diff de valores reales, ya que son reportes calculados sin PK): `CierreStockWeb` contra datos reales de San Lorenzo (idEmpresa=1, idSucursal=2, rango entre 2 cierres de stock reales) -> 12/13 filas identicas exactas, 1/13 con diferencia de ~0.002 sobre ~1192 en un valor intermedio (explicado: `Corte.porcentaje`/`porcentajeHueso` son `float`/`double precision` en **ambos** motores -- ruido de orden de operaciones en punto flotante entre los dos planificadores de consulta, no un error de traduccion). `ObtenerExistenciaPorSucursalesPlano` contra el mismo tenant/sucursal (43 productos) -> **43/43 filas identicas** en `StockActual`/`EstadoStock`. `TotalPorCortesVendidos` spot-check (22 filas) -> match exacto salvo ruido de punto flotante en el ultimo digito. Todas las comparaciones se corrieron con el filtro de `idEmpresa` explicito en ambos lados (no con el login de bypass de RLS sin acotar, que produce fugas cross-tenant falsas -- mismo tipo de error metodologico ya documentado en la Etapa 11b para Postgres, esta vez del lado SQL Server).
- **HTTP end-to-end**: nueva accion `MigracionPostgresController.CompararStockReportes` (grilla completa lado a lado, no "encontrado/no encontrado" -- son reportes calculados sin PK), probada con login real (IIS Express, usuario descartable `test_etapa11c`/idEmpresa=1) contra `CierreStockWeb`: 13 filas en ambos motores, sin error, consistente con la verificacion directa por SQL.
- **`Web.csproj` compila limpio** (solucion completa, incluidos `Presentacion.csproj`/`wsAFIPvs2008.csproj`, sin errores nuevos -- warnings preexistentes no relacionados).
- **`oCorteDSqlServer` no se elimina** de `Negocio/Corte.cs` (a diferencia de `CierreCaja` en Etapa 10): quedan 6 metodos permanentemente fuera de la interfaz (`obtenerEmbutidos` de 11a + los 5 excluidos de esta etapa).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Venta.cs, bloque Sectores/Licencias (Etapa 12a)

Con `Corte.cs` completo, se sigue con `Venta.cs`. **Correccion de premisa relevada con un agente Explore**: `Venta.cs` NO era territorio virgen -- `Contratos/IVentaRepository.cs`/`DatosPostgres/VentaPg.cs` ya tenian 24/47 metodos migrados (bloque Ventas/LineaVenta/TemporalLineaVenta, "Etapa 7"/"Etapa 8" de este mismo archivo, fechada 2026-08-19, aparentemente inmediatamente antes de esta sesion) -- codigo-completo y verificado via `MigracionPostgresController.CompararVenta`, pero **no en produccion** (ningun constructor real usa `VentaPg`, solo el controller de comparacion). Quedaban 23 metodos en 3 sub-bloques: Expendios/LineaExpendio (9), Sectores/Licencias (7), FacturaElectronica (8, el de mayor riesgo -- AFIP). El usuario confirmo el orden: Sectores/Licencias primero (mas chico y simple), Expendios/LineaExpendio despues, FacturaElectronica al final.

- **7 metodos migrados**: `obtenerSectores`, `existeSector`, `agregarSector`, `modificarSector`, `sectorEstaEnUso`, `eliminarSector`, `getUltimoSectorSelect`. Todos SQL de texto simple (`Datos/Venta.cs:1012-1150`), sin SPs, sin jerarquia recursiva -- la sub-etapa mas chica de toda la migracion hasta ahora.
- **`modificarSector`/`eliminarSector` son transaccionales y tocan 3/2 tablas** (`Sectores`+`Expendios`+`Licencias`; `Sectores`+`Licencias`) aunque el CRUD completo de Expendios/LineaExpendio es la *proxima* sub-etapa -- ya se necesitaba que `expendios`/`licencias` existan en Postgres. Resultaron ya existir (94 filas `expendios`, 4 filas `licencias`, RLS estandar) -- migradas como slice minimo en la Etapa 10 (`cambiarSucursalCaja`). Solo hizo falta crear `sectores` (2 columnas, `sector text`/`idempresa integer`, sin PK/identity en el original, se replica igual; 4 filas reales migradas).
- **`Sectores.idEmpresa` en SQL Server tiene un `DEFAULT` atado a `SESSION_CONTEXT('IdEmpresa')`** (confirmado con `sys.default_constraints`) -- `agregarSector` no lo pasa explicito del lado SQL Server, confia en ese default. En Postgres no hay default por sesion equivalente: se bindea `idempresa=@idEmpresa` explicito en el `INSERT`, mismo criterio que el resto de `VentaPg.cs`/`CortePg.cs`.
- **Patron de transaccion multi-statement reusado tal cual** de `VentaPg.modificarVenta` (ya existente, Etapa 7/8): `ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx)` + `NpgsqlCommand(sql, con, tx)` por statement + `Commit()`/`Rollback()`.
- **Verificado**: `Web.csproj`/solucion completa compilan limpio. Harness `psql` con el rol app real (`carnisys_user`, no el owner que bypassea RLS) + transaccion explicita + `ROLLBACK`: `agregarSector`, `modificarSector` (cascada verificada: 1 `sectores` + 81 `expendios` + 1 `licencias` afectados) y `eliminarSector` (cascada: 1 `sectores` + 1 `licencias`) -- sin residuo tras el rollback. HTTP end-to-end con login real (IIS Express, usuario descartable `test_etapa12a`/idEmpresa=1, borrado al final) contra la nueva accion `MigracionPostgresController.CompararSectores`: 3/3 sectores identicos en ambos motores, `existeSector`/`sectorEstaEnUso` coinciden (`True`/`True` para "Carniceria").
- **Nota operativa, no un bug de esta etapa**: durante la verificacion HTTP aparecieron timeouts intermitentes de conexion a SQL Server (`Sucursal.findAll`, `Usuario.getPermisosUsuario`, handshake de conexion) -- causados por procesos `MSBuild.exe` de compilaciones anteriores en esta sesion que quedaron corriendo como *worker nodes* persistentes (flag `-m`) compitiendo por RAM/CPU con SQL Server Express. `sqlcmd` directo respondia instantaneo (0.6s) durante los timeouts, confirmando que no era un problema del motor ni del codigo. Resuelto matando los `MSBuild.exe` colgados -- mismo tipo de limpieza ya conocida para `VBCSCompiler.exe` en sesiones anteriores, ahora tambien aplica a los worker nodes de `MSBuild -m`.

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Venta.cs, bloque Expendios/LineaExpendio (Etapa 12b)

Segunda de las 3 sub-etapas del resto de `Venta.cs` (Sectores/Licencias, Etapa 12a, commit `2e555fc5` -> **Expendios/LineaExpendio** -> FacturaElectronica). Cubre los 8 metodos de `Datos/Venta.cs` (`#region EXPENDIO`, líneas 821-1214): `agregarExpendio`, `agregarLineaExprendio`, `asignarVentaEnExpendio`, `obtenerUltimosExpendios`, `obtenerExpendiosPorUsuario`, `obtenerExpendiosEmpresa`, `getExpedioById`, `obtenerLineasExpendio`.

- **7 metodos entran a `IVentaRepository`** (los 6 con wrapper directo + `asignarVentaEnExpendio`). `obtenerLineasExpendio` queda fuera de la interfaz a proposito -- sin caller externo, se resuelve como helper privado `GetLineasExpendio` dentro de `VentaPg.cs` (mismo patron que `Datos.Venta` lo resuelve como metodo interno).
- **Hallazgo real durante el scoping**: `asignarVentaEnExpendio` no tenia wrapper propio en `Negocio.Venta.cs` -- se llamaba embebido dentro de `agregarVenta` (linea 111, `foreach` sobre `ListaExpendios`), hardcodeado a `oVentaDSqlServer` a proposito porque Expendios no existia todavia (aunque `agregarVenta` en si ya corre sobre `oVentaD` desde la Etapa 7). Se redirigio ese call embebido a `oVentaD` en esta etapa -- mismo criterio que los calls embebidos de `fechaUltimoCierreStock_Sucursal` dentro de `ObtenerCortesListado`/`findAllCortes` en la Etapa 11c.
- **`agregarExpendio` (SP real, via `sp_helptext`) tiene un efecto colateral no evidente desde la firma C#**: `UPDATE Licencias SET sector=@sector WHERE nroLicencia=@serialCPU` antes del `INSERT INTO Expendios`. El parametro `@idExpendio` que recibe el SP nunca se usa (dead param, el id real sale de `SELECT TOP 1 ... ORDER BY idExpendio DESC`, no de `SCOPE_IDENTITY()`) -- se replico igual, sin "arreglar" la carrera teorica bajo concurrencia.
- **Tabla nueva**: `lineaexpendio` (7 columnas, `pesoBalanza` es `tinyint` en SQL Server -- no `bit` -- se mapea a `boolean` igual, consistente con `Entidades.LineaVenta.PesoBalanza`; 148 filas reales migradas). `expendios`/`licencias` ya existian desde la Etapa 10.
- **Hallazgo de verificacion, descartado como bug**: al probar `agregarExpendio` con una licencia compartida entre 3 tenants (`nroLicencia='178BFBFF00A50F00'`, filas con `idEmpresa=0/1/2`), el `UPDATE Licencias` fallo por RLS en Postgres (`WITH CHECK` bloquea la fila de `idEmpresa=0`, visible por la excepcion "empresa o global" pero no escribible por otro tenant). Se confirmo con `sys.security_predicates` que SQL Server tiene el **mismo predicado BLOCK AFTER UPDATE** sobre `Licencias` (`RLS_Empresa`) -- comportamiento identico en ambos motores, no una divergencia de la traduccion. Se reverifico con una licencia exclusiva de un solo tenant y funciono correcto.
- **Patron de casteo para parametros de fecha genuinamente nulables** (`obtenerExpendiosPorUsuario`/`obtenerExpendiosEmpresa`, primera vez en esta migracion que un `DateTime?` real se bindea como `DBNull.Value`): Npgsql no puede inferir el tipo de un parametro `DBNull` sin contexto -- se resuelve con un cast explicito en el SQL (`@fechaDesde::date IS NULL OR ...`), idioma estandar de Npgsql para este caso. Verificado con `psql` que el patron compila y filtra correcto con fechas NULL y con fechas reales.
- **Verificado**: `Web.csproj`/solucion completa compilan limpio. Harness `psql` (rol real `carnisys_user`, transaccion explicita, `ROLLBACK`): `agregarExpendio` (con la cascada a `licencias`), `agregarLineaExprendio`, `asignarVentaEnExpendio` -- sin residuo. HTTP end-to-end con login real (usuario descartable `test_etapa12b`/idEmpresa=1, borrado al final) contra la nueva accion `CompararExpendio`: expendio #1 identico en ambos motores (Sector, Vendedor, Sucursal, y las 2 `LineasVenta` con sus mismos valores de kg/precio/balanza).

## 2026-08-19 - Migracion SQL Server -> PostgreSQL: Venta.cs, bloque FacturaElectronica (Etapa 12c) -- Venta.cs queda 100% migrado (47/47 metodos)

Ultima de las 3 sub-etapas del resto de `Venta.cs` (Sectores/Licencias, `2e555fc5`; Expendios/LineaExpendio, `71ce9d2e`; **FacturaElectronica**, el de mayor riesgo de negocio -- AFIP). Cubre los 8 metodos de `Datos/Venta.cs` (`#region FACTURA ELECTRONICA`, líneas 1218-1688): `esVentaSinFacturar`, `existeFacturaElect`, `existeNotaCreditoElect`, `addOrEditFactuElec`, `getFactuElecById`, `BuscarFacturasPagina`, `ObtenerFacturasResumen`, `getAlicuotaIvaFactura`.

- **7 metodos entran a `IVentaRepository`**; `getAlicuotaIvaFactura` queda fuera a proposito -- sin caller externo, resuelto como helper privado `GetAlicuotaIvaFactura` en `VentaPg.cs` (mismo criterio que `GetLineasExpendio` en la 12b).
- **Hallazgo 1, aclarado por el usuario -- NO es un bug**: el SP `addOrEditFacturaElectronica`, en la rama de edicion, tiene `fechaEmisionAfip = fechaEmisionAfip` (auto-asignacion, sin el `@` del parametro). Es intencional: una vez que AFIP emite el CAE, la fecha de emision queda **legalmente inmutable** -- editar otros campos despues no debe alterarla. Traducido a Postgres omitiendo `fechaemisionafip` del `SET` en la rama de edicion (mismo efecto, mas explicito). Verificado con `psql`: se edito una factura real (`observaciones`) y `fechaemisionafip` permanecio exactamente igual antes y despues.
- **Hallazgo 2, confirmado con el usuario -- mejora deliberada en Postgres**: `Datos.Venta.addOrEditFactuElec` (C#) nunca envolvia la cabecera + el loop de `AlicuotaIvaPorFactura` en una transaccion -- riesgo real de fila huerfana si el loop fallaba a mitad de camino. La version Postgres usa `ConexionPg.AbrirConTenant` (mismo patron que `modificarVenta`/`agregarExpendio`) para que cabecera + alicuotas sean atomicas. No cambia ninguna firma ni comportamiento visible cuando todo sale bien.
- **3 tablas nuevas**: `facturaelectronica` (28 columnas, 109 filas reales; `error` es `tinyint` en SQL Server, se mapea a `boolean`), `alicuotaivaporfactura` (6 columnas, 38 filas). `alicuotasiva` **ya existia** (migrada en la Etapa 6 junto con el bloque CRUD/referencia de `Corte.cs` -- comparte la misma tabla fisica que `obtenerAlicuotasIva`/`findAlicuotaIvaById`; mismas 6 filas ya presentes, sin RLS por ser catalogo global) -- el script de migracion se corrigio para documentar esto en vez de un `CREATE TABLE` redundante.
- **Hallazgo de migracion de datos**: un `mensajeError` real de una factura rechazada por AFIP contenia un caracter `|` literal en el texto del error ("...no corresponde a una cuit pais. | 10015: Para facturas B..."), rompiendo el delimitador `|` usado en el pipeline de export/import de esta migracion (mismo pipeline usado en todas las etapas anteriores, primera vez que un dato real choca con el delimitador). Se resolvio reemplazando `|` por `/` en los campos de texto libre (`mensajeError`, `observaciones`, etc.) durante el export -- perdida de fidelidad minima y documentada (un caracter en un mensaje de log diagnostico, no en ningun campo con valor semantico como `CAE`/`importeTotal`), no en los datos que importan para la logica de negocio.
- **`BuscarFacturasPagina`/`ObtenerFacturasResumen` reusan el patron existente**: 2 helpers privados (`ConstruirWhereFacturas`/`AgregarParametrosFacturas`) para el `WHERE` dinamico con placeholders numerados (`@fp0`, `@cc0`, ...), y el `MapFacturaCompleta` de Postgres **reusa `MapVenta`/`CargarRelacionesVenta` tal cual** (los alias de columna de la CTE de paginacion coinciden exactamente) -- igual que el original SQL Server reusa `MapVenta` dentro de su propio `MapFacturaCompleta`, no hizo falta reimplementar el mapeo de Vendedor/Sucursal/Persona.
- **`Negocio/Venta.cs` colapsado a un solo campo** (`oVentaD`, sin `oVentaDSqlServer`) -- mismo criterio que `CierreCaja` en la Etapa 10: con los 47/47 metodos de `Datos.Venta` ya en `IVentaRepository`, el segundo campo "siempre SQL Server" quedo sin ningun uso real.
- **Verificado**: `Web.csproj`/solucion completa compilan limpio. Harness `psql` (rol real `carnisys_user`, transaccion explicita, `ROLLBACK`): alta de una factura + su alicuota (atomico, mejora del Hallazgo 2) y edicion de una factura real confirmando que `fechaemisionafip` no cambia (Hallazgo 1) -- sin residuo. HTTP end-to-end con login real (usuario descartable `test_etapa12c`/idEmpresa=1, borrado al final) contra la nueva accion `CompararFactura`: factura #33 identica en ambos motores (CAE, RazonSocialAFIP, ImporteTotal, IdVenta, y su alicuota de IVA).
- **Cierre de modulo**: con esta etapa, `Venta.cs` (47/47 metodos) queda completamente migrado a `Contratos.IVentaRepository`/`DatosPostgres.VentaPg`. Igual que el resto de la migracion, el codigo queda listo y verificado pero **no en produccion** -- ningun constructor real fuera de `MigracionPostgresController` instancia `VentaPg` todavia; el cutover de trafico real es una decision aparte, no incluida en el alcance de esta migracion.
