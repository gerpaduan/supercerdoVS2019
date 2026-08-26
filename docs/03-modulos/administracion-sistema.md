# Administracion de sistema (super-admin de plataforma)

## Objetivo

Alta y edicion de Empresas, Sucursales y Usuarios **cruzando todos los tenants** -- distinto de "Mi Empresa"/"Mi Sucursal" (tenant-scoped, un idEmpresa fijo). Acceso restringido a usuarios con `superadmin=true` (columna `usuarios.superadmin`). Ver `docs/DECISIONS.md` 2026-08-25 para el porque de las decisiones de diseno.

## Acceso

`Web/Helpers/SystemAdministrationAccessHelper.PuedeAdministrarSistema(session)` -- lee `session["Usuario"]`, resuelve el repo via `Web.Infrastructure.NegocioFactory.CrearSystemAdministrationRepository()` y llama `EsSuperAdmin(usuario.Id)`. Sin usuario superadmin, la request cae en `~/Views/Shared/AccesoDenegado.cshtml`.

## Arquitectura (SQL Server y Postgres)

Unico modulo del repo cuyo contrato de repositorio (`ISystemAdministrationRepository`) vive en `Web/Helpers/`, no en `Contratos/`: su firma usa `Web.Models.*`/`System.Web.Mvc.SelectListItem`, tipos que no existen en el `netstandard2.0` puro de `Contratos`/`DatosPostgres`.

- `Web/Helpers/ISystemAdministrationRepository.cs` -- contrato.
- `Web/Helpers/SystemAdministrationRepository.cs` -- implementacion SQL Server (`SqlConnection` directo via `Utilidades.Db.OpenAdmin`, llama al SP `dbo.AA_AltaEmpresa`).
- `DatosPostgres/SystemAdministrationPg.cs` -- backend Postgres (Npgsql), habla solo `Entidades.*`/primitivos.
- `Web/Helpers/SystemAdministrationRepositoryPg.cs` -- adaptador que traduce VM<->Entidades y resuelve los defaults de negocio (pais->"Argentina", tenantSlug/basePath autogenerados por slug, etc.), delega el SQL a `SystemAdministrationPg`.
- `Web/Infrastructure/NegocioFactory.CrearSystemAdministrationRepository()` -- rutea segun `DataEngine` (`Web.config`), sin `IEmpresaContext` (el modulo es cross-tenant por diseno).

### Bypass de RLS en Postgres

Rol `carnisys_sysadmin_bypass` (`NOLOGIN BYPASSRLS`, migracion `DatosPostgres/DB-Migrations/20260825b-Create_carnisys_sysadmin_bypass_role.sql`), asumido con `SET LOCAL ROLE` dentro de cada transaccion (`SystemAdministrationPg.AbrirAdmin`) -- equivalente Postgres de `Db.OpenAdmin` (SQL Server, `session_context EsAdminCarniSys=1`). Grants: `empresas, sucursal, usuarios, corte, empresaparametros, cortepuntostocksucursal` (SELECT/INSERT/UPDATE/DELETE) + `alicuotasiva` y `iva` (solo SELECT, esta ultima agregada en `20260825c-Grant_iva_a_carnisys_sysadmin_bypass.sql`) + `USAGE/SELECT` sobre las secuencias identity de esas tablas.

## Alta de empresa (ambos motores)

Logica identica en los dos motores (fuente de verdad: cuerpo real del SP `dbo.AA_AltaEmpresa`, extraido via `sp_helptext` en vivo, 2026-08-25):

1. Validar CUIT unico (si viene).
2. Asignar `idEmpresa`: primer hueco `>= 1` libre, o `MAX+1` si no hay huecos.
3. Insertar la fila de `Empresas`.
4. Copiar `EmpresaParametros` desde la plantilla `idEmpresa = -1` (solo los que no existan ya para la empresa nueva).
5. Crear la Sucursal default: nombre `"Suc." + razonSocialAfip` (recortado a 50 caracteres), resto de campos vacios.
6. Crear 2 productos fijos en `Corte`:
   - Codigo `-1` "Ajuste de Formula": fijo, no editable, `habilitado=false`. Usado exclusivamente por `Negocio.Corte.ObtenerProductoAjusteFormula` (ver `docs/DECISIONS.md` 2026-08-22) -- **no** tiene relacion con el parametro `codProdGenerico`.
   - Codigo configurable (default `999999`) "Codigo Generico": editable en el formulario de alta (nombre/codigo/alicuota IVA), `habilitado=true`.

Concurrencia: SQL Server usa `SERIALIZABLE` + `UPDLOCK/HOLDLOCK` (locks de fila); Postgres usa `LOCK TABLE empresas IN SHARE ROW EXCLUSIVE MODE` (lock de tabla completa) -- decision deliberada, ver `docs/DECISIONS.md` 2026-08-25.

`CrearAltaRapida` (Empresa+Sucursal+Usuario en un flujo): fase 1 crea la empresa (commit propio), fase 2 (transaccion nueva) busca la sucursal default recien creada, la actualiza con los datos reales del formulario, y crea el usuario admin inicial.

### Sugerencias de "Condicion IVA" en Alta rapida de empresa y Editar empresa

`Empresa.CondicionIVA`/`SystemAdministrationEmpresaEditVm.CondicionIVA` es texto libre (se autocompleta desde AFIP/ARCA via `#btnBuscarAfipAltaRapida` en el alta, ver script inline de `AltaRapidaEmpresa.cshtml`), no un FK -- por eso el input sigue siendo un textbox, no un combo, en las 2 vistas. `ISystemAdministrationRepository.ObtenerCondicionesIva()` precarga un `<datalist>` (mismo `id="condicionesIvaList"` en ambas vistas, cada una con su propia instancia del elemento) con el mismo catalogo (tabla `Iva`/`iva`) que usa el combo de `/Personas` (`Datos/Persona.cs::getIva()` / `DatosPostgres/PersonaPg.cs::getIva()`), poblado en `SystemAdministrationController.OnActionExecuting` junto con `AlicuotasIva` (aplica a las 2 vistas sin repetir el fetch por accion).

Tabla `Iva`/`iva` sin columna `idEmpresa` en ningun motor (verificado en vivo contra Postgres local, 4 filas: Consumidor Final/Responsable Inscripto/Monotributista/Exento) -- catalogo global real, sin scoping por tenant. Ambos motores devuelven `SELECT id, iva FROM Iva/iva ORDER BY iva` sin `WHERE`, igual que `Datos/Persona.cs::getIva()`/`DatosPostgres/PersonaPg.cs::getIva()`.

## Generacion de IDs

- `Empresas.idEmpresa`: gap-fill manual (no es IDENTITY en ningun motor), igual en ambos.
- `Usuarios.id`: SQL Server usa `MAX(id)+1` con `UPDLOCK/HOLDLOCK` (patron marcado como riesgoso por el propio comentario original del SP); Postgres usa la IDENTITY nativa de la columna (`INSERT ... RETURNING id`) -- decision ya tomada para el resto del modulo Usuario (ver `DatosPostgres/UsuarioPg.cs`), no una limitacion nueva.

## Password

`usuarios.clave` (columna legacy) guarda el valor en texto plano en ambos motores -- fidelidad al original, no un descuido. El hash real (`passwordhash`/`passwordsalt`/`passwordhashiterations`) se calcula con `Utilidades.PasswordSecurity.HashPassword` en la capa `Web/` (no alcanzable desde `DatosPostgres`, que es `netstandard2.0`) y se persiste en una UPDATE separada, tanto en alta como en edicion (solo si se ingreso una clave nueva).

## Deuda conocida

- `permisosusuarios` no se siembra para usuarios creados desde este modulo (asimetria real del original, replicada tal cual -- a diferencia de `UsuarioPg.addOrEditUser`, que si siembra 12 permisos por defecto).
- El SP `sp_EmpresaParametros_SetDefaults` (SQL Server) es codigo muerto sin caller -- la copia de plantilla esta inline en `AA_AltaEmpresa` mismo (paso 3). No se porto.
- Backfill de los 2 productos default aplicado una sola vez (2026-08-25) a las empresas Postgres 1-6 ya existentes (excepto la 1, que ya los tenia) -- no es un mecanismo automatico, altas futuras de empresas SQL Server que nunca pasen por este flujo necesitarian el mismo backfill manual.
