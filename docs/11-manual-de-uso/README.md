# Manual de uso y ayuda por pantalla

> Este archivo lo **regenera** `scripts/check-ayuda.ps1`: no editarlo a mano.

Fuente de verdad de la ayuda que muestra la app (boton **Ayuda** de la barra superior, panel lateral encima de la pantalla actual, y los manuales por rol en `/Ayuda`). Un `.md` por pantalla; el manual de cada rol se arma solo a partir de ellos. Se **embeben en el .dll** al compilar (`WebCore/WebCore.csproj`). Decision y reglas: `docs/DECISIONS.md` ("Manuales por rol y ayuda en cada pantalla").

## Estructura
- `pantallas/<Controller>.<Action>.md` -- ayuda de UNA pantalla (clave = controller/action de la ruta; `alias:` para otras acciones que muestran la misma vista).
- `conceptos/<slug>.md` -- explicaciones transversales (roles, dispositivo seguro, PIN...).
- `referencia/<slug>.md` -- tablas de consulta (mapa de configuraciones).

## Imagenes y videos
Los archivos viven en `WebCore/wwwroot/ayuda/media/<Controller.Action>/` (NO se embeben en el .dll; viajan con el publish como cualquier estatico). En el `.md`: `![texto alternativo](media:Usuarios.Index/listado.png)`; los `.webm`/`.mp4` salen como `<video controls>` (extension MediaLinks de Markdig). Un clic en una imagen la abre completa en pestana nueva.

Se generan con `scripts/AyudaMedia` (Playwright contra un WebCore local, usuario admin de la base de DESARROLLO por variables de entorno `AYUDA_MEDIA_USER` / `AYUDA_MEDIA_PASSWORD`; ver cabecera de `scripts/AyudaMedia/Program.cs`): `dotnet run --project scripts/AyudaMedia`. Capturas con recuadros numerados, clip `.webm` sin audio. Hoy solo cubre `Usuarios.Index` (piloto). Regla: si cambia la pantalla, se regeneran las capturas en el mismo commit que el `.md`. Cuidado: las capturas muestran datos de la base usada; no usar una base con datos de clientes reales.

Encabezado de cada archivo: `pantalla` (solo pantallas), `titulo`, `rol` (`usuario` | `admin` | `superadmin`), `modulo`, `orden`, `permiso` (informativo), `revisada` (yyyy-MM-dd), `alias` (opcional). Enlaces internos: `[texto](ayuda:pantalla/Usuarios.Index)`, `(ayuda:concepto/slug)`, `(ayuda:referencia/slug)`.

**Regla**: toda vista o comportamiento nuevo/modificado actualiza su `.md` y, si hay una configuracion nueva, `referencia/configuraciones.md`, en el mismo commit. Lo no verificado contra el codigo se marca `PENDIENTE`; si la doc contradice al codigo, manda el codigo. Control: `powershell -File scripts\check-ayuda.ps1`.

## Cobertura (46 de 95 pantallas)

## Manual del usuario

| Tipo | Titulo | Modulo | Archivo | Revisada |
|---|---|---|---|---|
| conceptos | Bloqueo de usuarios por intentos fallidos | Acceso y seguridad | `conceptos/bloqueo-de-usuarios.md` | 2026-10-03 |
| conceptos | Bloqueo por inactividad (sesión y POS) | Acceso y seguridad | `conceptos/bloqueo-por-inactividad.md` | 2026-10-20 |
| conceptos | Clave rápida (PIN) | Acceso y seguridad | `conceptos/clave-rapida-pin.md` | 2026-10-03 |
| conceptos | Ingresar con la dirección de tu empresa (CUIT) | Acceso y seguridad | `conceptos/login-por-cuit.md` | 2026-10-04 |
| conceptos | Intentos de clave al autorizar (POS, Cierre de Caja, Ver fórmula) | Acceso y seguridad | `conceptos/intentos-de-clave-al-autorizar.md` | 2026-10-20 |
| conceptos | Buscar cliente, vendedor o proveedor en los filtros (F9 / F10) | Conceptos generales | `conceptos/selector-cliente-vendedor-proveedor.md` | 2026-10-13 |
| conceptos | Menú de usuario (arriba a la derecha) | Conceptos generales | `conceptos/menu-de-usuario.md` | 2026-10-14 |
| conceptos | Roles y permisos (quién puede hacer qué) | Conceptos generales | `conceptos/roles-y-permisos.md` | 2026-10-03 |
| conceptos | Fórmula secreta | Elaborados | `conceptos/formula-secreta.md` | 2026-10-20 |
| pantallas | Cambiar contraseña | Acceso y seguridad | `pantallas/Login.ChangePassword.md` | 2026-10-03 |
| pantallas | Clave rápida (PIN) | Acceso y seguridad | `pantallas/Login.ChangePin.md` | 2026-10-03 |
| pantallas | Desbloquear usuario (enlace del mail) | Acceso y seguridad | `pantallas/Login.UnlockAccount.md` | 2026-10-03 |
| pantallas | Este dispositivo no está autorizado | Acceso y seguridad | `pantallas/Login.DispositivoNoAutorizado.md` | 2026-10-03 |
| pantallas | Ingreso al sistema | Acceso y seguridad | `pantallas/Login.Index.md` | 2026-10-13 |
| pantallas | Recuperar contraseña | Acceso y seguridad | `pantallas/Login.ForgotPassword.md` | 2026-10-03 |
| pantallas | Restablecer contraseña | Acceso y seguridad | `pantallas/Login.ResetPassword.md` | 2026-10-03 |
| pantallas | Egresos de caja | Caja | `pantallas/Cajas.EgresosCaja.md` | 2026-10-09 |
| pantallas | Compras | Compras | `pantallas/Compras.Index.md` | 2026-10-13 |
| pantallas | Líneas de compra | Compras | `pantallas/Compras.Lineas.md` | 2026-10-14 |
| pantallas | Carga / ingreso de elaborado | Elaborados | `pantallas/Elaborados.Carga.md` | 2026-10-09 |
| pantallas | Editar fórmula | Elaborados | `pantallas/Elaborados.EditarFormula.md` | 2026-10-09 |
| pantallas | Elaborados | Elaborados | `pantallas/Elaborados.Index.md` | 2026-10-14 |
| pantallas | Fórmulas | Elaborados | `pantallas/Elaborados.Formulas.md` | 2026-10-04 |
| pantallas | Ingreso rápido y desarme de elaborado | Elaborados | `pantallas/Elaborados.EditarIngresoRapido.md` | 2026-10-05 |
| pantallas | Líneas de elaborado | Elaborados | `pantallas/Elaborados.Lineas.md` | 2026-10-14 |
| pantallas | Pago / Cobro | Finanzas | `pantallas/Finanzas.AddOrEditPago.md` | 2026-10-13 |
| pantallas | Productos | Productos | `pantallas/Productos.Index.md` | 2026-10-16 |
| pantallas | Cargar o modificar un movimiento de stock | Stock | `pantallas/Stock.Editar.md` | 2026-10-16 |
| pantallas | Facturas | Ventas | `pantallas/Ventas.Facturas.md` | 2026-10-13 |
| pantallas | Líneas de venta | Ventas | `pantallas/Ventas.Lineas.md` | 2026-10-14 |
| pantallas | Punto de venta (POS) — saldo del cliente y ticket | Ventas | `pantallas/Ventas.POS.md` | 2026-10-19 |
| pantallas | Ventas | Ventas | `pantallas/Ventas.Index.md` | 2026-10-13 |

## Manual del administrador

| Tipo | Titulo | Modulo | Archivo | Revisada |
|---|---|---|---|---|
| conceptos | Dispositivo seguro (qué es y cómo se configura) | Acceso y seguridad | `conceptos/dispositivo-seguro.md` | 2026-10-15 |
| pantallas | Auditoría de accesos | Acceso y seguridad | `pantallas/AuditoriaLogin.Index.md` | 2026-10-03 |
| pantallas | Dispositivos seguros | Acceso y seguridad | `pantallas/DispositivosSeguros.Index.md` | 2026-10-15 |
| pantallas | Cajas abiertas y cierre de caja | Caja | `pantallas/Cajas.CajasAbiertas.md` | 2026-10-20 |
| pantallas | Certificado ARCA | Facturación | `pantallas/CertificadoArca.Index.md` | 2026-10-12 |
| pantallas | Titulares de facturación | Facturación | `pantallas/TitularesFiscales.Index.md` | 2026-10-12 |
| pantallas | Mi Empresa | Mi empresa | `pantallas/Empresa.Index.md` | 2026-10-03 |
| pantallas | Reportes | Reportes | `pantallas/Reportes.Index.md` | 2026-10-13 |
| pantallas | Existencia por sucursales | Stock | `pantallas/Stock.ExistenciaPorSucursales.md` | 2026-10-19 |
| pantallas | Modificar pedido | Stock | `pantallas/Pedidos.Editar.md` | 2026-10-20 |
| pantallas | Pedido (detalle) | Stock | `pantallas/Pedidos.Detalle.md` | 2026-10-19 |
| pantallas | Pedidos | Stock | `pantallas/Pedidos.Index.md` | 2026-10-19 |
| pantallas | Qué pedir | Stock | `pantallas/Reposicion.Index.md` | 2026-10-13 |
| pantallas | Sugerir puntos de stock | Stock | `pantallas/Reposicion.Puntos.md` | 2026-10-13 |
| pantallas | Nuevo / Modificar usuario | Usuarios | `pantallas/Usuarios.Editar.md` | 2026-10-15 |
| pantallas | Permisos del usuario | Usuarios | `pantallas/Usuarios.Permisos.md` | 2026-10-03 |
| pantallas | Usuarios | Usuarios | `pantallas/Usuarios.Index.md` | 2026-10-03 |
| referencia | Mapa de configuraciones de la empresa | Referencia | `referencia/configuraciones.md` | 2026-10-20 |

## Manual del super administrador

| Tipo | Titulo | Modulo | Archivo | Revisada |
|---|---|---|---|---|
| conceptos | Titular compartido entre empresas (mismo CUIT en varias empresas) | Administración de la plataforma | `conceptos/titular-compartido-entre-empresas.md` | 2026-10-12 |
| pantallas | Alta rápida de empresa | Administración de la plataforma | `pantallas/SystemAdministration.AltaRapidaEmpresa.md` | 2026-10-03 |
| pantallas | Empresas (administración del sistema) | Administración de la plataforma | `pantallas/SystemAdministration.Empresas.md` | 2026-10-12 |
| pantallas | Nueva / Modificar empresa | Administración de la plataforma | `pantallas/SystemAdministration.EditarEmpresa.md` | 2026-10-12 |
| pantallas | Nuevo / Modificar usuario (administración del sistema) | Administración de la plataforma | `pantallas/SystemAdministration.EditarUsuario.md` | 2026-10-03 |
| pantallas | Usuarios (administración del sistema) | Administración de la plataforma | `pantallas/SystemAdministration.Usuarios.md` | 2026-10-03 |
| referencia | Mapa de configuraciones de la plataforma | Referencia | `referencia/configuraciones-plataforma.md` | 2026-10-12 |

## Pantallas sin ayuda todavia

- **Actividades**: Index
- **CodigosBarra**: Editar, Index
- **Compras**: AutorizarModulo, Editar
- **Elaborados**: IngresoRapido
- **Empleados**: Editar, Historial, Index, Vacaciones
- **Finanzas**: Cheques, CtaCtePersona, CtasCtes, Pagos
- **Home**: Index, Utilidades
- **Jornadas**: Fichaje, FichajeConfirmar, Index, MiJornada, MiJornadaProduccion
- **Liquidaciones**: Index, Nueva, Preview
- **Movimientos**: Editar, Index, Lineas
- **PadronPlataforma**: Index
- **Parametros**: Index
- **Personas**: Editar, Index
- **Productos**: AddOrEdit, EmbedGuardado, Marcas, Tipos
- **PuntosExpendio**: ExpendiosGenerados, POS, Sectores
- **SeleccionUsuario**: Index
- **Stock**: Index, Lineas
- **Sucursal**: Editar, Index
- **SucursalesSpike**: Index
- **SystemAdministration**: EditarSucursal, Sucursales
- **Ventas**: AutorizarModulo, DetalleFactura, DetalleVenta
