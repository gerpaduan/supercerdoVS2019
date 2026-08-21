-- Rol dedicado, NOLOGIN + BYPASSRLS, para los pocos metodos de DatosPostgres/UsuarioPg.cs que
-- necesitan buscar cruzando todas las empresas (login, "olvide mi contraseña", reseteo por
-- token, desbloqueo de cuenta -- ver comentario de clase de UsuarioPg.cs). Reemplaza el intento
-- anterior con "SET LOCAL row_security = off": Postgres RECHAZA esa sentencia (error 42501) si
-- el rol que la ejecuta no tiene ya BYPASSRLS -- no existe forma de desactivar RLS "por esta
-- vez" sin el privilegio real. Verificado a mano con psql antes de esta migracion.
--
-- NOLOGIN: no es una credencial nueva, nadie se conecta con este rol directamente -- solo se
-- puede asumir con SET LOCAL ROLE desde una sesion ya autenticada como carnisys_user (ver
-- UsuarioPg.AbrirSinRLS). Verificado que la membresia por si sola NO habilita el bypass
-- automaticamente (BYPASSRLS es un atributo de rol, no un privilegio heredable via INHERIT) --
-- hace falta el SET LOCAL ROLE explicito en cada consulta puntual, asi que el resto de la app
-- sigue protegido por RLS sin cambios.

CREATE ROLE carnisys_usuarios_bypass NOLOGIN BYPASSRLS;

GRANT SELECT, INSERT, UPDATE, DELETE ON usuarios, usuariopasswordresettokens TO carnisys_usuarios_bypass;

GRANT carnisys_usuarios_bypass TO carnisys_user;
