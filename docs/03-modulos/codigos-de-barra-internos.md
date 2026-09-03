# Motor de códigos de barra internos (balanza, prefijo EAN 20-29)

## Objetivo

Interpretar códigos de barra EAN-13 generados por balanzas comerciales (prefijo 20-29), donde el código codifica internamente el PLU del producto y un valor (precio o cantidad/peso) en posiciones configurables. Configurable por empresa, sin nada hardcodeado en C#. No reemplaza la búsqueda exacta por EAN completo (`dbo.Corte.codigo`) — la complementa: si el prefijo no es 20-29, o es 20-29 pero la empresa no tiene formato configurado, el flujo cae al camino de siempre.

## Entidades

- **`dbo.FormatosCodigoBarras`** (`Entidades.FormatoCodigoBarras`, `Datos.FormatoCodigoBarras`/`DatosPostgres.FormatoCodigoBarrasPg`, `Negocio.FormatoCodigoBarras`): un formato por `(IdEmpresa, Prefijo)` — `UNIQUE` real en SQL Server y en Postgres (a diferencia del gap conocido en `DispositivosSeguros`, donde Postgres no replicó el `UNIQUE` de SQL Server). Campos: `Nombre`, `Prefijo` (20-29), `LongitudTotal` (siempre 13), `PosicionCodigo`/`LongitudCodigo` (PLU), `PosicionValor`/`LongitudValor`, `TipoValor` (`Precio`/`Cantidad`), `CantidadDecimales`, `Activo`, `Prioridad` (se guarda, sin efecto funcional — solo orden en la UI). Scripts: `Datos/DB-Procedures/20260901-Create_FormatosCodigoBarras.sql`, `DatosPostgres/DB-Migrations/20260901-Create_formatoscodigobarras.sql` (con RLS en Postgres).
- **El PLU extraído se busca contra el campo `Codigo` ya existente de `Corte`** — decisión explícita del usuario (2026-09-03): no se agregó ningún campo nuevo a `Corte`. La empresa que use este motor debe cargar sus productos con ese número corto como `Codigo`.

## Motor centralizado

`Negocio/BarcodeInterpreter.cs`, con el mismo patrón de dos constructores que `Negocio.Corte`/`Negocio.DispositivoSeguro` (instanciado vía `Web/Infrastructure/NegocioFactory.CrearBarcodeInterpreter`). Dos métodos:

- **`Interpretar(codigoDeBarras, idEmpresa)`**: valida checksum EAN-13 (`Utilidades.ValidacionEan.EsEan13Valido`, y su copia en `Utilidades.Core` para el leg net10.0 de `Negocio.csproj`) → si no es EAN-13 válido o el prefijo no es 20-29, devuelve `Caso=CodigoInvalido` (`EsCodigoInterno=false`) → busca `FormatoCodigoBarras` activo para `(idEmpresa, prefijo)`, si no hay devuelve `Caso=PrefijoSinFormato` (`EsCodigoInterno=false`) → si hay formato pero el código no coincide con sus posiciones/longitud, `Caso=EstructuraInvalida` → si el PLU extraído no tiene `Corte` en la empresa, `Caso=ProductoNoEncontrado` → si todo resuelve, `Caso=Interpretado` con `Producto`, `TipoValor` y `Valor` (raw / 10^CantidadDecimales). Nunca tira excepción — un `try/catch` de última instancia devuelve `EstructuraInvalida` genérico ante cualquier fallo no previsto.
- **`InterpretarCodigoGenerico(codigoNormalizado, ingresoCantidadX, codigoBaseGenerico)`**: migración 1:1 (mismo regex `^[^G]*G(\d+)[^G]*$`, mismo umbral de 8 dígitos) del mecanismo "código genérico" que antes vivía duplicado en `VentasController.BuscarProducto` y `PuntosExpendioController.BuscarProductoPOS`.

## Wiring en el POS

`Web/Controllers/VentasController.cs` (`BuscarProducto`) y `Web/Controllers/PuntosExpendioController.cs` (`BuscarProductoPOS`) llaman a los **mismos 2 métodos de la misma instancia** de `BarcodeInterpreter` — la interpretación es idéntica entre las dos pantallas; cada controller sigue armando su propio shape de JSON (ya distintos antes de esta feature, consumidos por JS específico de cada vista). Campo nuevo y aditivo en ambas respuestas: `cantidadSugerida` (nullable) — se usa cuando `TipoValor=Cantidad`, para que `Web/Scripts/app/pos-product.js` cargue el peso real extraído en vez de forzar cantidad=1 (gap real: sin este campo, cualquier pesada por código interno hubiera quedado cargada como 1 unidad).

`IdEmpresa` se resuelve igual que siempre en ambos controllers (`Session["Usuario"].IdEmpresa`, fallback a `empresa.IdEmpresa` de `EmpresaContextWeb`).

## Pantalla de configuración

`Web/Controllers/CodigosBarraController.cs` + `Web/Views/CodigosBarra/{Index,Editar}.cshtml`, bajo el menú "Configuración" (mismo patrón de permisos que `DispositivosSegurosController`: cualquier usuario de la empresa ve, solo Admin administra). El formulario de alta/edición trae 3 presets (JS inline, sin roundtrip) con el layout "5+5" típico de balanza (posiciones 3-7 PLU, 8-12 valor) y un ejemplo de lectura dígito-por-dígito recalculado en vivo. El `Prefijo` no se edita una vez creado el formato — cambiar de prefijo implica dar de baja y crear uno nuevo.

## Dependencias

- `Utilidades/ValidacionEan.cs` (y su copia idéntica en `Utilidades.Core/ValidacionEan.cs`, mismo criterio que el resto de archivos duplicados en ese proyecto): 4ta implementación del checksum EAN en el repo. Las 3 existentes (`Web/Controllers/ProductosController.cs` x2, `Utilidades/GenerarCodigoBarra.cs`) no se tocaron — fuera de alcance de esta feature.
- `Contratos/ICorteBusquedaSimpleRepository.cs`: interfaz angosta (2 de los 40+ métodos de `ICorteRepository`) para que `BarcodeInterpreter` no dependa del resto de `Corte` (Movimientos, Formulas, Embutidos, reportes). `Datos.Corte`/`DatosPostgres.CortePg` implementan ambas interfaces.
- Tests: `Negocio.Tests/BarcodeInterpreterTests.cs`, `BarcodeInterpreterCodigoGenericoTests.cs` (15 tests, xUnit, ver `docs/DECISIONS.md` sobre el runner `vstest.console.exe`/MSBuild de Visual Studio).

## Riesgos conocidos / deuda

- No hay infraestructura de test para `pos-product.js` (JS) ni para los controllers Web (no existe `Web.Tests`, no hay Jest/Karma) — el wiring del campo `cantidadSugerida` en el cliente solo se verificó manualmente (curl a los endpoints `BuscarProducto`/`BuscarProductoPOS`, no un test automatizado de UI).
- El script SQL Server (`Datos/DB-Procedures/20260901-Create_FormatosCodigoBarras.sql`) no se corrió contra ninguna instancia de SQL Server en esta sesión (el entorno local usa `DataEngine=Postgres`) — verificado solo por lectura y por compilación de `Datos/FormatoCodigoBarras.cs`. Correrlo y verificarlo la primera vez que se despliegue con `DataEngine=SqlServer` (San Lorenzo o Servidor SM).
