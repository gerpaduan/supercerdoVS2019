# RLS: mapeo SQL Server → PostgreSQL

## Objetivo

Documentar cómo se traduce el aislamiento multi-tenant (Row-Level Security) de SQL Server a PostgreSQL, para la migración en curso (ver `docs/DECISIONS.md`, entrada 2026-08-18). Esta es la pieza más sensible de la migración: un error acá filtra datos entre empresas.

## Mecanismo real en SQL Server (verificado contra la base viva `carnisys`)

Política `RLS_Empresa` sobre 32 tablas, con predicado FILTER (SELECT/UPDATE/DELETE) + BLOCK (AFTER INSERT/UPDATE, BEFORE DELETE), ambos usando las mismas dos funciones:

```sql
CREATE FUNCTION dbo.fn_rls_empresa_o_global_v2(@idEmpresa int)
RETURNS TABLE WITH SCHEMABINDING
AS RETURN
    SELECT 1 AS fn_result
    WHERE
        (ORIGINAL_LOGIN() = 'cs_admin')
        OR TRY_CONVERT(bit, SESSION_CONTEXT(N'EsAdminCarniSys')) = 1
        OR (
            TRY_CONVERT(int, SESSION_CONTEXT(N'IdEmpresa')) IS NOT NULL
            AND (@idEmpresa = TRY_CONVERT(int, SESSION_CONTEXT(N'IdEmpresa')) OR @idEmpresa = 0)
        );
```

Fail-closed: sin `SESSION_CONTEXT('IdEmpresa')` seteado y sin ser admin, no se ve nada — ni las filas propias ni las "globales" (`idEmpresa=0`).

## Mapeo a PostgreSQL

| Concepto SQL Server | Equivalente PostgreSQL | Notas |
|---|---|---|
| `SESSION_CONTEXT('IdEmpresa')`, seteado por conexión física | `current_setting('app.id_empresa', true)`, seteado con `SET LOCAL` **dentro de una transacción explícita** | `SET LOCAL` se deshace solo al COMMIT/ROLLBACK — necesario por pooling de conexiones (una conexión física reciclada no debe arrastrar el tenant de la request anterior). Nunca `SET` a nivel de sesión desnudo. |
| Predicado FILTER (SELECT/UPDATE/DELETE) | Cláusula `USING` de la policy | |
| Predicado BLOCK (AFTER INSERT/UPDATE, BEFORE DELETE) | Cláusula `WITH CHECK` de la policy (para DELETE no aplica, el `USING` ya cumple ese rol) | |
| Login `cs_admin` (bypass fijo) | Rol con atributo `BYPASSRLS` | Ignora RLS a nivel de motor, sin lógica en la policy. |
| `SESSION_CONTEXT('EsAdminCarniSys')=1` (bypass conmutable, se usa de verdad en `SystemAdministrationRepository.cs`) | Condición extra en el `USING`: `current_setting('app.es_admin_carnisys', true) = 'true' OR (...)` | No se puede resolver con `BYPASSRLS` porque es conmutable en runtime por el mismo rol de app, no un rol distinto. |
| Fail-closed (había que codificarlo a mano en la función) | Propiedad del motor: tabla con RLS y sin policy que matchee = cerrada, salvo dueño/superusuario/`BYPASSRLS` | **Trampa**: el dueño de la tabla bypasea RLS por default aunque haya policies. Las tablas las crea un rol *distinto* al que usa la app (`carnisys_admin` dueño/DDL, `carnisys_user` de aplicación sin ownership), o se fuerza `ALTER TABLE ... FORCE ROW LEVEL SECURITY`. |
| Convención "fila global" (`idEmpresa=0` visible a todos) | `OR id_empresa = 0` dentro del `USING` | Hoy (18/08/2026) no hay ninguna fila con `idEmpresa=0` activa en ninguna tabla con RLS de `carnisys` — el patrón está vivo en el diseño pero sin uso real actual. |

### Policy de referencia

```sql
CREATE POLICY <tabla>_rls ON <tabla>
    FOR ALL
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND (id_empresa = current_setting('app.id_empresa', true)::int OR id_empresa = 0)
    )
    WITH CHECK (
        id_empresa = current_setting('app.id_empresa', true)::int
    );
```

**Decisión deliberada (endurecimiento respecto a SQL Server)**: el `WITH CHECK` de arriba **no** permite que una sesión de tenant normal inserte/actualice una fila a `idEmpresa=0`. En SQL Server el predicado BLOCK actual sí lo permitiría (usa la misma función que el FILTER), pero no hay ninguna fila así en uso hoy — confirmado con el usuario, se adopta el diseño más estricto.

### Tablas con `idEmpresa` sin RLS

Las 11 tablas maestras/meta (`Usuarios`, `Empresas`, `PermisosUsuarios`, `EmpresaParametros`, `CortePuntoStockSucursal`, `CatalogoGlobalImportacionProductos`, `ConfiguracionWhatsApp`, `DispositivosSeguros`, `UsuarioPasswordResetTokens`, `WhatsAppEnvios`, `AuditoriaCambioSucursalCaja`) quedan **sin RLS también en Postgres** — confirmado con el usuario, sin revisión caso por caso.

### Collation (pendiente para la Fase 4)

SQL Server usa `Modern_Spanish_CI_AS` (case-insensitive) en `carnisys`. Postgres no lo replica a nivel de collation por default (`=`/`LIKE` case-sensitive salvo `citext` o collation ICU no-determinística). Instancia local instalada con collation `Spanish_Argentina.1252` (toma el locale del sistema, `es-AR`) — resuelve solo el **orden** de texto (`ORDER BY`), no la comparación por igualdad. Decisión de cómo replicar (o no) el comportamiento case-insensitive queda para la Fase 4 (schema mapping).

## Prueba de escritorio verificada (18/08/2026)

Base descartable `rls_poc` en la instancia local (PostgreSQL 17.11), tabla `productos_poc` (id, nombre, id_empresa) con 3 filas: Tenant 1, Tenant 2, Global (`idEmpresa=0`). Policy exactamente como la de referencia arriba. Roles: `poc_tenant` (normal) y `poc_bypass` (`BYPASSRLS`).

| # | Escenario | Esperado | Resultado real |
|---|---|---|---|
| 1 | `poc_tenant`, sin `app.id_empresa` seteado, `SELECT *` | 0 filas | **0 filas** ✅ |
| 2 | `poc_tenant`, `SET LOCAL app.id_empresa='1'`, `SELECT *` | Tenant 1 + Global | **Tenant 1 + Global** ✅ |
| 3 | `poc_tenant`, `SET LOCAL app.id_empresa='2'`, `SELECT *` | Tenant 2 + Global | **Tenant 2 + Global** ✅ |
| 4 | `poc_tenant`, con `app.id_empresa='1'`, `INSERT ... id_empresa=0` | Rechazado | **Rechazado**: `ERROR: el nuevo registro viola la política de seguridad de registros para la tabla «productos_poc»` ✅ |
| 5 | `poc_bypass` (`BYPASSRLS`), sin nada seteado, `SELECT *` | Las 3 filas | **Las 3 filas** ✅ |

Los 5 resultados coinciden exactamente con el diseño. `rls_poc` es descartable — no se usa para nada más, no se toca `carnisys` real de Postgres (que todavía no existe).

## Pendiente para la próxima etapa (no resuelto acá)

- Estrategia real de pooling de conexiones desde .NET (Npgsql pooling nativo vs. pgbouncer en modo transacción) — el principio no negociable es que cualquier estrategia respete `SET LOCAL` dentro de transacción explícita, nunca `SET` de sesión desnudo.
- Collation/case-insensitivity (ver arriba).
- Aplicar este mismo diseño a las 32 tablas reales de `carnisys` (esto es solo el POC en `rls_poc`).
