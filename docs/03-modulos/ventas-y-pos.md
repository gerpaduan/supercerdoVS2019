# Ventas y POS

## Objetivo

Documentar el flujo de ventas, caja, articulos, pesaje y uso del POS.

## Secciones

- Pantallas involucradas
- Flujo de venta
- Validaciones
- Impresion y comprobantes
- Dependencias con balanza o perifericos
- Riesgos

## Ventas en curso (borrador en servidor) y advertencias del POS

Ver decisión completa, verificación y lista de archivos en `docs/DECISIONS.md` ("Ventas en curso:
borrador en servidor y advertencias del POS", 2026-09-21). Resumen operativo:

- **Motores.** Postgres: completa y prendida por defecto. **SQL Server (`SuperCerdo`: Servidor SM,
  San Lorenzo): desde 2026-09-23 hay una versión reducida, opt-in** (`PosBorrador:Habilitado=true` +
  `Datos/DB-Procedures/20260923-Create_Borradores.sql`; ver `docs/DECISIONS.md` "Borradores en SQL
  Server"): ventas en curso, recuperación, campana propia del cajero y "Ventas sin guardar" **sí**;
  campana/notificaciones del admin y advertencia "producto pesado sin agregar" **no** (etapa 2, apagadas
  con `PosBorradorSettings.SoportaNotificaciones`). Apagada (default en SQL Server), el POS sigue con
  el `POSDraft` local de siempre.
- **Qué guarda**: mientras el cajero arma la venta, el carrito se resguarda en la tabla
  `ventaborrador` (identificado por un `clientid` GUID del navegador), con autoguardado en cada
  cambio y un latido periódico. **Nunca toca `ventas`/`lineaventa`** — esas tablas se escriben recién
  al "Finalizar venta", como siempre.
- **Recuperación**: panel de Ayuda (F1) → "Ver ventas sin cerrar" lista las de toda la sucursal con
  usuario e ítems; el cajero carga/descarta solo las propias, una ajena pide autorización de un
  supervisor (usuario+clave).
- **Al cerrar la caja** con una venta en curso del dueño de esa caja, se avisa y hace falta confirmar;
  si se confirma, queda registrado (evento + notificación al admin) y la venta sigue recuperable.
- **Advertencia "producto pesado sin agregar"**: si un producto queda en pantalla con peso/cantidad
  estable más de `PosBorrador:SegundosProductoSinAgregar` segundos (config, default 7) y sale sin
  agregarse al carrito, se registra en `ventaproductosinagregar` como advertencia silenciosa (el
  cajero no se entera). Se agrupa por operador y día en una sola notificación.
- **Admin**: campana en el topbar (solo admin, polling ~60 s) + `/Actividades` muestran las ventas
  interrumpidas/descartadas y las advertencias, con detalle y revisión (Justificada/Sospechosa).
- **Configuración**: sección `PosBorrador:*` en `WebCore.dll.config`/`App.config` (umbrales de
  latido, minutos sin señal para considerar "interrumpida", segundos de producto en pantalla, etc.).
  Ninguno está hardcodeado — ver `WebCore/Helpers/PosBorradorSettings.cs` para los defaults.

**Deuda conocida**: `FinalizarVenta` sigue confiando en el precio que manda el cliente (una venta
recuperada conserva los precios de cuando se armó); la detección de "producto sin agregar" es del
lado del navegador y puede tener falsos positivos (cliente arrepentido, código equivocado); no hay
proceso en segundo plano, así que sin un admin activo no hay alerta push (el rastro en la base queda
igual). Detalle completo en `docs/DECISIONS.md`.
