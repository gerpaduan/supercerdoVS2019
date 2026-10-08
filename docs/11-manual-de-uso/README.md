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

## Cobertura (27 de 87 pantallas)

## Manual del usuario

| Tipo | Titulo | Modulo | Archivo | Revisada |
|---|---|---|---|---|
| conceptos | Bloqueo de usuarios por intentos fallidos | Acceso y seguridad | `conceptos/bloqueo-de-usuarios.md` | 2026-10-03 |
| conceptos | Clave rápida (PIN) | Acceso y seguridad | `conceptos/clave-rapida-pin.md` | 2026-10-03 |
| conceptos | Ingresar con la dirección de tu empresa (CUIT) | Acceso y seguridad | `conceptos/login-por-cuit.md` | 2026-10-04 |
| conceptos | Menú de usuario (arriba a la derecha) | Conceptos generales | `conceptos/menu-de-usuario.md` | 2026-10-03 |
| conceptos | Roles y permisos (quién puede hacer qué) | Conceptos generales | `conceptos/roles-y-permisos.md` | 2026-10-03 |
| conceptos | Fórmula secreta | Elaborados | `conceptos/formula-secreta.md` | 2026-10-04 |
| pantallas | Cambiar contraseña | Acceso y seguridad | `pantallas/Login.ChangePassword.md` | 2026-10-03 |
| pantallas | Clave rápida (PIN) | Acceso y seguridad | `pantallas/Login.ChangePin.md` | 2026-10-03 |
| pantallas | Desbloquear usuario (enlace del mail) | Acceso y seguridad | `pantallas/Login.UnlockAccount.md` | 2026-10-03 |
| pantallas | Este dispositivo no está autorizado | Acceso y seguridad | `pantallas/Login.DispositivoNoAutorizado.md` | 2026-10-03 |
| pantallas | Ingreso al sistema | Acceso y seguridad | `pantallas/Login.Index.md` | 2026-10-04 |
| pantallas | Recuperar contraseña | Acceso y seguridad | `pantallas/Login.ForgotPassword.md` | 2026-10-03 |
| pantallas | Restablecer contraseña | Acceso y seguridad | `pantallas/Login.ResetPassword.md` | 2026-10-03 |
| pantallas | Egresos de caja | Caja | `pantallas/Cajas.EgresosCaja.md` | 2026-10-07 |
| pantallas | Carga / ingreso de elaborado | Elaborados | `pantallas/Elaborados.Carga.md` | 2026-10-05 |
| pantallas | Editar fórmula | Elaborados | `pantallas/Elaborados.EditarFormula.md` | 2026-10-05 |
| pantallas | Elaborados | Elaborados | `pantallas/Elaborados.Index.md` | 2026-10-05 |
| pantallas | Fórmulas | Elaborados | `pantallas/Elaborados.Formulas.md` | 2026-10-04 |
| pantallas | Ingreso rápido y desarme de elaborado | Elaborados | `pantallas/Elaborados.EditarIngresoRapido.md` | 2026-10-05 |
| pantallas | Líneas de elaborado | Elaborados | `pantallas/Elaborados.Lineas.md` | 2026-10-05 |
| pantallas | Pago / Cobro | Finanzas | `pantallas/Finanzas.AddOrEditPago.md` | 2026-10-07 |

## Manual del administrador

| Tipo | Titulo | Modulo | Archivo | Revisada |
|---|---|---|---|---|
| conceptos | Dispositivo seguro (qué es y cómo se configura) | Acceso y seguridad | `conceptos/dispositivo-seguro.md` | 2026-10-03 |
| pantallas | Auditoría de accesos | Acceso y seguridad | `pantallas/AuditoriaLogin.Index.md` | 2026-10-03 |
| pantallas | Dispositivos seguros | Acceso y seguridad | `pantallas/DispositivosSeguros.Index.md` | 2026-10-03 |
| pantallas | Cajas abiertas y cierre de caja | Caja | `pantallas/Cajas.CajasAbiertas.md` | 2026-10-07 |
| pantallas | Mi Empresa | Mi empresa | `pantallas/Empresa.Index.md` | 2026-10-03 |
| pantallas | Nuevo / Modificar usuario | Usuarios | `pantallas/Usuarios.Editar.md` | 2026-10-03 |
| pantallas | Permisos del usuario | Usuarios | `pantallas/Usuarios.Permisos.md` | 2026-10-03 |
| pantallas | Usuarios | Usuarios | `pantallas/Usuarios.Index.md` | 2026-10-03 |
| referencia | Mapa de configuraciones de la empresa | Referencia | `referencia/configuraciones.md` | 2026-10-04 |

## Manual del super administrador

| Tipo | Titulo | Modulo | Archivo | Revisada |
|---|---|---|---|---|
| pantallas | Alta rápida de empresa | Administración de la plataforma | `pantallas/SystemAdministration.AltaRapidaEmpresa.md` | 2026-10-03 |
| pantallas | Empresas (administración del sistema) | Administración de la plataforma | `pantallas/SystemAdministration.Empresas.md` | 2026-10-03 |
| pantallas | Nueva / Modificar empresa | Administración de la plataforma | `pantallas/SystemAdministration.EditarEmpresa.md` | 2026-10-03 |
| pantallas | Nuevo / Modificar usuario (administración del sistema) | Administración de la plataforma | `pantallas/SystemAdministration.EditarUsuario.md` | 2026-10-03 |
| pantallas | Usuarios (administración del sistema) | Administración de la plataforma | `pantallas/SystemAdministration.Usuarios.md` | 2026-10-03 |
| referencia | Mapa de configuraciones de la plataforma | Referencia | `referencia/configuraciones-plataforma.md` | 2026-10-03 |

## Pantallas sin ayuda todavia

- **Actividades**: Index
- **CertificadoArca**: Index
- **CodigosBarra**: Editar, Index
- **Compras**: AutorizarModulo, Editar, Index, Lineas
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
- **Productos**: AddOrEdit, EmbedGuardado, Index, Marcas, Tipos
- **PuntosExpendio**: ExpendiosGenerados, POS, Sectores
- **Reportes**: Index
- **SeleccionUsuario**: Index
- **Stock**: Editar, ExistenciaPorSucursales, Index, Lineas
- **Sucursal**: Editar, Index
- **SucursalesSpike**: Index
- **SystemAdministration**: EditarSucursal, Sucursales
- **Ventas**: AutorizarModulo, DetalleFactura, DetalleVenta, Facturas, Index, Lineas, POS
