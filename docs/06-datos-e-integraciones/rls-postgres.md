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

Las 9 tablas maestras/meta restantes (`Empresas`, `PermisosUsuarios`, `EmpresaParametros`, `CortePuntoStockSucursal`, `CatalogoGlobalImportacionProductos`, `ConfiguracionWhatsApp`, `DispositivosSeguros`, `WhatsAppEnvios`) quedan **sin RLS también en Postgres** — decisión original sin revisión caso por caso, no reevaluada desde entonces.

**`Usuarios`/`UsuarioPasswordResetTokens` reclasificadas (2026-08-21, ver `docs/DECISIONS.md`)**: la decisión de arriba se revirtió para estas dos -- sin RLS, cualquier consulta que se olvidara del `WHERE idempresa` filtraba usuarios (y `passwordhash`) de otro tenant, dependiendo 100% de que cada caller se acordara de chequear a mano. Ahora tienen RLS habilitado (sin la excepción `OR idempresa=0`, mas estricto que el resto de las tablas). Los pocos metodos que legitimamente necesitan cruzar todas las empresas (login, "olvide mi contraseña", reseteo por token, desbloqueo de cuenta -- tenant desconocido hasta resolver quien es el usuario) usan `SET LOCAL ROLE carnisys_usuarios_bypass` (rol `NOLOGIN BYPASSRLS`, otorgado como membresia a `carnisys_user`, ver `DatosPostgres/UsuarioPg.cs` y migracion `20260821b`) -- **no** `SET LOCAL row_security=off`: Postgres rechaza esa sentencia (`42501`) si el rol no tiene ya `BYPASSRLS`, no hay forma de desactivar RLS "por esta vez" sin el privilegio real. Verificado a mano que la membresia por si sola no habilita el bypass fuera de un `SET ROLE` explicito -- el resto de las consultas de `carnisys_user` sigue protegido por RLS sin cambios.

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

## Pendiente

- Collation/case-insensitivity (ver arriba).
- Aplicar este mismo diseño a las 32 tablas reales de `carnisys` (por ahora migradas: `personas`+`iva`, `sucursal`+`empresas` — ver `docs/DECISIONS.md`, Etapas 2 y 3).

## Estrategia de pooling de conexiones (decidido 18/08/2026)

**Contexto**: `DatosPostgres/ConexionPg.cs` abre una `NpgsqlConnection` nueva por operación (mismo patrón que `Utilidades/Conexion.cs` con SQL Server), ejecuta `SELECT set_config('app.id_empresa', @idEmpresa, true)` como primer statement de una transacción explícita, corre la operación real, y hace `COMMIT`/`ROLLBACK` — ver `DatosPostgres/DbPg.cs`. La pregunta pendiente era si esto es seguro con pooling de conexiones sin tener que agregar nada más.

**Verificado contra la documentación oficial de Npgsql** (`Pooling`, `No Reset On Close`, `Maximum Pool Size` en `connection-string-parameters.html`), no de memoria:

- `Pooling=true` es el default — Npgsql ya poolea conexiones físicas por debajo de cada `NpgsqlConnection`, sin que haga falta agregar nada (no se necesita PgBouncer solo para tener pooling).
- `No Reset On Close=false` es el default — Npgsql **resetea el estado de la conexión al devolverla al pool**. Es una segunda capa de seguridad, redundante con la primera: `SET LOCAL` (vía `set_config(..., true)`) ya se deshace solo al `COMMIT`/`ROLLBACK` de la transacción, **antes** de que la conexión vuelva al pool — así que el diseño actual es seguro incluso si alguna vez se pusiera `No Reset On Close=true` por performance.
- `Maximum Pool Size=100` es el default (era 20 antes de la versión 3.1). **Riesgo real a controlar en producción**: el `max_connections` default de PostgreSQL también es 100 — si el pool de la app sola puede llegar a 100 conexiones físicas, se come todo el presupuesto de conexiones del servidor, sin dejar margen para `psql`/pgAdmin/monitoreo/otros roles. **Decisión**: en el connection string de producción, fijar `Maximum Pool Size` explícito y conservador (ej. 20-30, a ajustar con tráfico real medido) en vez de confiar en el default de 100.

**Decisión: no hace falta PgBouncer para el despliegue actual** (un solo servidor de aplicación/VM). Se revisita **solo si**:
- Se escala a múltiples instancias del proceso de la app (cada una con su propio pool de Npgsql, sumando conexiones físicas), y el total se acerca a `max_connections` de Postgres.
- Métricas reales (`pg_stat_activity`) muestran contención de conexiones.

**Si se adopta PgBouncer más adelante, tiene que ser en modo `transaction`, nunca `session` ni `statement`**: `ConexionPg.AbrirConTenant` ejecuta 2 statements en la misma transacción (el `set_config` + la query real) — modo `statement` rompería esto enrutando cada statement a una conexión física distinta. Modo `transaction` preserva los límites de transacción por checkout de cliente, que es exactamente lo que necesita el patrón `SET LOCAL`.

**Principio no negociable, vale para cualquier estrategia futura**: nunca `SET` de sesión desnudo fuera de una transacción explícita — solo `SET LOCAL`/`set_config(..., true)` dentro de una transacción que siempre termina en `COMMIT` o `ROLLBACK` antes de soltar la conexión.
