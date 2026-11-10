# Reportes y administracion

## Objetivo

Centralizar reportes, configuraciones y procesos administrativos.

## Secciones

- Reportes disponibles
  - `/Reportes` (`WebCore/Controllers/ReportesController.cs`): Stock Actual, Cierre Stock, Stock Retroactivo, Proyección Ventas vs Stock, Ventas por Producto y Balance Económico.
  - **Cierre Stock (2026-10-13)**: *Faltante* = stock teórico − *Stock Cierre* (con signo: positivo faltó, negativo se vendió de más). El *Estado* se calcula contra el Stock Cierre (`AgregarFilasStockSucursal`, parámetro `estadoSegunStockCierre`), no contra el Faltante, que es una diferencia y no un stock. Los totales separan lo que faltó de lo vendido de más, sumando los Faltantes positivos y los negativos por separado (`Totales.TotalFaltoMercaderia/TotalVendidoDeMas`) en vez de netearlos. Decisión: `docs/DECISIONS.md` (2026-10-13, Reportes / Cierre Stock).
  - **Sucursales (2026-10-13)**: se pueden tildar 1, varias o todas (`idsSucursales`; el `idSucursal` viejo sigue valiendo). Ventas por Producto, Proyección y Balance suman las tildadas (`IdsParaConsulta`: `[id]`, `[0]` = todas en una sola consulta, o la lista). Los 3 de stock, con 2+ sucursales, calculan cada una con sus propios cierres (`UsaCierresPorSucursal`). Detalle y decisiones: `docs/DECISIONS.md` (2026-10-13, Reportes) y `docs/11-manual-de-uso/pantallas/Reportes.Index.md`.
  - **Ventana independiente (2026-10-13)**: `/Reportes?popup=true` usa `Views/Shared/_LayoutEmbed.cshtml` (sin sidebar/topbar). El boton "Abrir en ventana" (`#btnAbrirVentanaReportes`, handler en `reportes.js`, helper compartido `ventana-independiente.js`, ventana de nombre fijo `reportes`) arma la URL con los mismos parametros que enviaria Buscar (`$form.serializeArray()`; `buscar=true` solo si `reportesConfig.consultaRealizada`) mas `popup=true`. El formulario lleva un campo oculto `popup=true` para conservar la ventana al buscar (Buscar es un GET que recarga la pagina). `Index` recibe el parametro `popup`; `FiltrosSecundarios` y los JSON no cambian. Decision: `docs/DECISIONS.md` (2026-10-13, Ventana independiente).
- Parametros y filtros
- Configuraciones sensibles
- Permisos
  - **Asistente de usuarios y permisos (2026-11-10)**: `Usuarios/Asistente` (`UsuariosController.Asistente/ValidarDatosAsistente/PermisosParaCopiar/GuardarAsistente`) crea o modifica un usuario en 4 pasos. El catálogo de qué controles tiene efecto en cada permiso, los perfiles y las claves bloqueadas para producción viven en `WebCore/Helpers/CatalogoPermisos.cs`; la traducción fila guardada ↔ estado en `WebCore/Helpers/AsistentePermisosMapper.cs`. La grilla anterior (`Usuarios/Permisos`) es la vista avanzada. Reglas y hallazgos: `docs/DECISIONS.md` (2026-11-10).
  - **Cuentas corrientes (2026-11-10)**: `formCtasCtes` (idform 17) = entrar al listado y a la cuenta de cualquier persona; `formCtaCteReservada` (idform 9003) = ver completas las cuentas reservadas (`CtaCteReservadaService`, `PersonasController.PuedeVerCtaCteReservada`).
