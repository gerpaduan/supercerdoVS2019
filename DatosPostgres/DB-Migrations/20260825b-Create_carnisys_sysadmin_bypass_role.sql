-- Rol dedicado, NOLOGIN + BYPASSRLS, para DatosPostgres/SystemAdministrationPg.cs -- el backend
-- Postgres del ultimo modulo 100% SQL Server (administracion de sistema: alta/edicion de
-- Empresas, Sucursales, Usuarios cruzando TODOS los tenants, gate de superadmin, ver
-- docs/DECISIONS.md 2026-08-25). Mismo patron que carnisys_usuarios_bypass (20260821b): un rol
-- separado en vez de reusar ese, porque el motivo es distinto (aca no es "tenant todavia
-- desconocido" como en login, es "el super-admin de plataforma opera sobre todas las empresas
-- por diseno") y porque necesita permisos sobre tablas que carnisys_usuarios_bypass no toca
-- (empresas, sucursal, corte, empresaparametros, cortepuntostocksucursal).
--
-- Equivale a Db.OpenAdmin() del lado SQL Server (Utilidades/Conexion.cs), que setea
-- session_context EsAdminCarniSys=1 -- SystemAdministrationRepository.cs (SQL Server) usa esa
-- conexion "admin" para el 100% de sus metodos, sin excepcion; este rol replica ese mismo
-- criterio de "una sola conexion privilegiada para toda la clase" del lado Postgres.
--
-- NOLOGIN: nadie se conecta con este rol directamente -- solo se asume con SET LOCAL ROLE
-- desde una sesion ya autenticada como carnisys_user (ver SystemAdministrationPg.AbrirAdmin).
-- El efecto vive y muere con la transaccion, igual que carnisys_usuarios_bypass.

CREATE ROLE carnisys_sysadmin_bypass NOLOGIN BYPASSRLS;

GRANT SELECT, INSERT, UPDATE, DELETE ON
    empresas, sucursal, usuarios, corte, empresaparametros, cortepuntostocksucursal
    TO carnisys_sysadmin_bypass;

-- Catalogo de referencia, solo lectura para este modulo (combo de IVA del producto "Codigo
-- Generico" en el alta de empresa).
GRANT SELECT ON alicuotasiva TO carnisys_sysadmin_bypass;

-- Las columnas identity que este modulo inserta sin especificar el id (RETURNING id) necesitan
-- USAGE+SELECT sobre la secuencia subyacente para el rol que ejecuta el INSERT (una vez que se
-- hace SET ROLE, los permisos se evaluan como ese rol, no como carnisys_user). Confirmado en
-- vivo con \ddp que ninguna de estas secuencias tiene grant por defecto -- hace falta explicito.
GRANT USAGE, SELECT ON SEQUENCE
    sucursal_idsucursal_seq, corte_idcorte_seq,
    cortepuntostocksucursal_idcortepuntostocksucursal_seq, usuarios_id_seq
    TO carnisys_sysadmin_bypass;

GRANT carnisys_sysadmin_bypass TO carnisys_user;
