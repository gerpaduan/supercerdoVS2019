-- Cierra el gap de RLS en usuarios/usuariopasswordresettokens (decision anterior documentada
-- en el comentario de clase de DatosPostgres/UsuarioPg.cs, revertida el 2026-08-21): sin RLS,
-- cualquier query que se olvidara del WHERE idempresa traia filas de otro tenant, passwordhash
-- incluido. El aislamiento a mano en cada metodo (defensa en profundidad, nunca la unica capa)
-- dejaba de proteger apenas alguien se olvidara de escribirlo -- exactamente el tipo de olvido
-- que RLS existe para cubrir. Decision del usuario, ver docs/DECISIONS.md.
--
-- Sin la excepcion "OR idempresa = 0" que usan otras tablas (auditoriacambiosucursalcaja,
-- catalogoglobalimportacionproductos, personas): los pocos metodos que necesitan cruzar todas
-- las empresas (login, "olvide mi contraseña", reseteo por token, desbloqueo de cuenta -- ver
-- comentario de clase de UsuarioPg.cs) usan SET LOCAL row_security=off en su propia transaccion
-- (DatosPostgres/UsuarioPg.cs, metodos AbrirSinRLS/NonQuerySinRLS/ReaderSinRLS/DataTableSinRLS),
-- no una excepcion de la politica. Esto incluye la cuenta global "admin" (id=0, idempresa=0):
-- bajo la politica estricta queda invisible para cualquier consulta de un tenant real, solo
-- alcanzable via los metodos que ya bypasean RLS a proposito.

ALTER TABLE usuarios ENABLE ROW LEVEL SECURITY;

CREATE POLICY usuarios_rls ON usuarios
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND idempresa = current_setting('app.id_empresa', true)::integer
    )
    WITH CHECK (
        idempresa = current_setting('app.id_empresa', true)::integer
    );

ALTER TABLE usuariopasswordresettokens ENABLE ROW LEVEL SECURITY;

CREATE POLICY usuariopasswordresettokens_rls ON usuariopasswordresettokens
    USING (
        current_setting('app.id_empresa', true) IS NOT NULL
        AND current_setting('app.id_empresa', true) <> ''
        AND idempresa = current_setting('app.id_empresa', true)::integer
    )
    WITH CHECK (
        idempresa = current_setting('app.id_empresa', true)::integer
    );

-- Unicidad global (todas las empresas) de nombre de usuario, case-insensitive -- requisito real
-- para que el login (que busca cruzando todas las empresas, sin saber el tenant todavia) no sea
-- ambiguo. Encontrado un caso real de colision antes de esta migracion: "admin_tercer" existia
-- en 2 empresas (id=8, idempresa=0, artefacto sin referencias en ningun lado -- borrado con
-- confirmacion del usuario; id=9, idempresa=3, el real). Verificado sin mas colisiones ni
-- usuarios NULL/vacios antes de crear el indice.
CREATE UNIQUE INDEX ux_usuarios_usuario_global ON usuarios (lower(usuario));
