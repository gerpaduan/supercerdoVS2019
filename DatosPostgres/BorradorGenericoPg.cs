using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IBorradorGenericoRepository: borradorgenerico (formulario en
    // curso de Compras/Stock/Movimientos/Embutidos), borradorgenericoevento (eventos append-only) y
    // notificaciones (campana del admin, misma tabla fisica que usa VentaBorradorPg). Ver migracion
    // 20260922a-Create_borradorgenerico.sql y docs/DECISIONS.md. Todas las tablas tienen RLS por
    // empresa: DbPg setea el tenant en cada operacion.
    // Relojes: ultimolatido/creado/actualizado/finalizado se calculan con now() de la BASE, nunca con el
    // reloj del navegador ni del servidor web (mismo criterio que VentaBorradorPg).
    public class BorradorGenericoPg : Contratos.IBorradorGenericoRepository
    {
        private const string ColumnasBorradorSinPayload =
            "b.id, b.idempresa, b.idsucursal, b.modulo, b.idregistro, b.idoperador, u.nombre AS nombreoperador, b.idusuariosesion, " +
            "b.clientid, b.resumen, b.cantlineas, NULL::text AS payload, b.estado, b.idresultado, " +
            "b.creado, b.ultimolatido, b.actualizado, b.finalizado, EXTRACT(EPOCH FROM (now() - b.ultimolatido))::int AS segundossinlatido";

        private const string ColumnasBorradorConPayload =
            "b.id, b.idempresa, b.idsucursal, b.modulo, b.idregistro, b.idoperador, u.nombre AS nombreoperador, b.idusuariosesion, " +
            "b.clientid, b.resumen, b.cantlineas, b.payload::text AS payload, b.estado, b.idresultado, " +
            "b.creado, b.ultimolatido, b.actualizado, b.finalizado, EXTRACT(EPOCH FROM (now() - b.ultimolatido))::int AS segundossinlatido";

        private const string ColumnasEvento =
            "e.id, e.idempresa, e.idborrador, e.fecha, e.tipo, e.idusuario, u.nombre AS nombreusuario, e.detalle, " +
            "b.modulo, ub.nombre AS nombreoperador, b.resumen, b.cantlineas";

        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public BorradorGenericoPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // ------------------------------------------------------------------------------------
        // Mapeos
        // ------------------------------------------------------------------------------------

        private static string Texto(System.Data.IDataRecord dr, string columna)
        {
            object valor = dr[columna];
            return valor == DBNull.Value ? null : Convert.ToString(valor);
        }

        private static int? EnteroNulo(System.Data.IDataRecord dr, string columna)
        {
            object valor = dr[columna];
            return valor == DBNull.Value ? (int?)null : Convert.ToInt32(valor);
        }

        private static DateTime? FechaNula(System.Data.IDataRecord dr, string columna)
        {
            object valor = dr[columna];
            return valor == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(valor);
        }

        private static Entidades.BorradorGenerico MapearBorrador(System.Data.IDataRecord dr)
        {
            return new Entidades.BorradorGenerico
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                Modulo = Convert.ToString(dr["modulo"]),
                IdRegistro = EnteroNulo(dr, "idregistro"),
                IdOperador = Convert.ToInt32(dr["idoperador"]),
                NombreOperador = Texto(dr, "nombreoperador"),
                IdUsuarioSesion = Convert.ToInt32(dr["idusuariosesion"]),
                ClientId = (Guid)dr["clientid"],
                Resumen = Texto(dr, "resumen"),
                CantLineas = Convert.ToInt32(dr["cantlineas"]),
                Payload = Texto(dr, "payload"),
                Estado = Convert.ToString(dr["estado"]),
                IdResultado = EnteroNulo(dr, "idresultado"),
                Creado = Convert.ToDateTime(dr["creado"]),
                UltimoLatido = Convert.ToDateTime(dr["ultimolatido"]),
                Actualizado = FechaNula(dr, "actualizado"),
                Finalizado = FechaNula(dr, "finalizado"),
                SegundosSinLatido = Convert.ToInt32(dr["segundossinlatido"])
            };
        }

        private static Entidades.BorradorGenericoEvento MapearEvento(System.Data.IDataRecord dr)
        {
            return new Entidades.BorradorGenericoEvento
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdBorrador = Convert.ToInt32(dr["idborrador"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Tipo = Convert.ToString(dr["tipo"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                NombreUsuario = Texto(dr, "nombreusuario"),
                Detalle = Texto(dr, "detalle"),
                Modulo = Texto(dr, "modulo"),
                NombreOperador = Texto(dr, "nombreoperador"),
                Resumen = Texto(dr, "resumen"),
                CantLineas = dr["cantlineas"] == DBNull.Value ? 0 : Convert.ToInt32(dr["cantlineas"])
            };
        }

        private static object ONulo(object valor)
        {
            return valor ?? DBNull.Value;
        }

        // ------------------------------------------------------------------------------------
        // Borradores
        // ------------------------------------------------------------------------------------

        public Entidades.ResultadoGuardarBorradorGenerico Guardar(Entidades.BorradorGenerico borrador)
        {
            if (borrador == null) throw new ArgumentNullException(nameof(borrador));

            // Upsert atomico: el DO UPDATE solo aplica si la fila es ACTIVA y del mismo operador,
            // sucursal y modulo; si no aplica no devuelve fila y abajo se averigua el motivo.
            object id = DbPg.Scalar(_connectionString, _idEmpresa, @"
                INSERT INTO borradorgenerico
                    (idempresa, idsucursal, modulo, idregistro, idoperador, idusuariosesion, clientid,
                     resumen, cantlineas, payload, estado, creado, ultimolatido)
                VALUES
                    (@idEmpresa, @idSucursal, @modulo, @idRegistro, @idOperador, @idUsuarioSesion, @clientId,
                     @resumen, @cantLineas, @payload, 'ACTIVA', now(), now())
                ON CONFLICT (idempresa, clientid) DO UPDATE SET
                    idregistro   = EXCLUDED.idregistro,
                    resumen      = EXCLUDED.resumen,
                    cantlineas   = EXCLUDED.cantlineas,
                    payload      = EXCLUDED.payload,
                    ultimolatido = now(),
                    actualizado  = now()
                WHERE borradorgenerico.estado = 'ACTIVA'
                  AND borradorgenerico.idoperador = EXCLUDED.idoperador
                  AND borradorgenerico.idsucursal = EXCLUDED.idsucursal
                  AND borradorgenerico.modulo = EXCLUDED.modulo
                RETURNING id;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", borrador.IdSucursal);
                    p.AddWithValue("modulo", borrador.Modulo);
                    p.AddWithValue("idRegistro", ONulo(borrador.IdRegistro));
                    p.AddWithValue("idOperador", borrador.IdOperador);
                    p.AddWithValue("idUsuarioSesion", borrador.IdUsuarioSesion);
                    p.AddWithValue("clientId", borrador.ClientId);
                    p.AddWithValue("resumen", ONulo(borrador.Resumen));
                    p.AddWithValue("cantLineas", borrador.CantLineas);
                    p.Add(new NpgsqlParameter("payload", NpgsqlDbType.Jsonb) { Value = ONulo(borrador.Payload) });
                });

            if (id != null && id != DBNull.Value)
            {
                borrador.Id = Convert.ToInt32(id);
                return Entidades.ResultadoGuardarBorradorGenerico.Guardado;
            }

            // No se guardo: distinguir "ya cerrado" de "ajeno".
            var existente = ObtenerPorClientId(borrador.ClientId, borrador.Modulo);
            if (existente != null && (existente.IdOperador != borrador.IdOperador || existente.IdSucursal != borrador.IdSucursal))
                return Entidades.ResultadoGuardarBorradorGenerico.Ajeno;

            return Entidades.ResultadoGuardarBorradorGenerico.YaCerrado;
        }

        public bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal, string modulo)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE borradorgenerico SET ultimolatido = now()
                WHERE idempresa = @idEmpresa AND clientid = @clientId AND idoperador = @idOperador
                  AND idsucursal = @idSucursal AND modulo = @modulo AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clientId", clientId);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("modulo", modulo);
                });
            return filas > 0;
        }

        public bool EnvejecerLatidoPorCierre(int idBorrador)
        {
            // 1 dia alcanza y sobra para superar cualquier umbral configurado (maximo 240 min = 4 h):
            // queda "interrumpido" de inmediato en vez de esperar el latido, sin tocar el estado.
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE borradorgenerico SET ultimolatido = now() - interval '1 day'
                WHERE idempresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", idBorrador);
                });
            return filas > 0;
        }

        public Entidades.BorradorGenerico ObtenerPorClientId(Guid clientId, string modulo)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM borradorgenerico b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.clientid = @clientId AND b.modulo = @modulo;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clientId", clientId);
                    p.AddWithValue("modulo", modulo);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.BorradorGenerico ObtenerPorId(int id)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM borradorgenerico b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.id = @id;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public List<Entidades.BorradorGenerico> ListarActivasPorSucursal(int idSucursal, string modulo)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM borradorgenerico b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.idsucursal = @idSucursal AND b.modulo = @modulo AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("modulo", modulo);
                });
        }

        public List<Entidades.BorradorGenerico> ListarActivasPorUsuarioSesion(int idUsuarioSesion)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM borradorgenerico b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.idusuariosesion = @idUsuarioSesion AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idUsuarioSesion", idUsuarioSesion);
                });
        }

        public bool TomarBorrador(int id, int idOperador, int idUsuarioSesion)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE borradorgenerico
                SET idoperador = @idOperador, idusuariosesion = @idUsuarioSesion, ultimolatido = now(), actualizado = now()
                WHERE idempresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("idUsuarioSesion", idUsuarioSesion);
                });
            return filas > 0;
        }

        public bool MarcarFinalizada(Guid clientId, string modulo, int idResultado)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE borradorgenerico
                SET estado = 'FINALIZADA', idresultado = @idResultado, payload = NULL, finalizado = now(), actualizado = now()
                WHERE idempresa = @idEmpresa AND clientid = @clientId AND modulo = @modulo AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clientId", clientId);
                    p.AddWithValue("modulo", modulo);
                    p.AddWithValue("idResultado", idResultado);
                });
            return filas > 0;
        }

        public bool MarcarDescartada(int id)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE borradorgenerico
                SET estado = 'DESCARTADA', payload = NULL, finalizado = now(), actualizado = now()
                WHERE idempresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                });
            return filas > 0;
        }

        public int PurgarFinalizadasAntiguas(string modulo, int dias)
        {
            return DbPg.NonQuery(_connectionString, _idEmpresa, @"
                DELETE FROM borradorgenerico
                WHERE idempresa = @idEmpresa AND modulo = @modulo AND estado = 'FINALIZADA'
                  AND finalizado < now() - (@dias * interval '1 day');",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("modulo", modulo);
                    p.AddWithValue("dias", dias);
                });
        }

        // ------------------------------------------------------------------------------------
        // Eventos
        // ------------------------------------------------------------------------------------

        public void AgregarEvento(Entidades.BorradorGenericoEvento evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO borradorgenericoevento (idempresa, idborrador, fecha, tipo, idusuario, detalle)
                VALUES (@idEmpresa, @idBorrador, now(), @tipo, @idUsuario, @detalle);",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idBorrador", evento.IdBorrador);
                    p.AddWithValue("tipo", evento.Tipo);
                    p.AddWithValue("idUsuario", evento.IdUsuario);
                    p.AddWithValue("detalle", ONulo(evento.Detalle));
                });
        }

        public List<Entidades.BorradorGenericoEvento> ListarEventosPorBorrador(int idBorrador)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasEvento + " FROM borradorgenericoevento e LEFT JOIN usuarios u ON u.id = e.idusuario " +
                "LEFT JOIN borradorgenerico b ON b.id = e.idborrador AND b.idempresa = e.idempresa LEFT JOIN usuarios ub ON ub.id = b.idoperador " +
                "WHERE e.idempresa = @idEmpresa AND e.idborrador = @idBorrador ORDER BY e.fecha, e.id;",
                MapearEvento,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idBorrador", idBorrador);
                });
        }

        // ------------------------------------------------------------------------------------
        // Notificaciones (misma tabla fisica que usa VentaBorradorPg)
        // ------------------------------------------------------------------------------------

        public int CrearNotificacionesInterrumpidas(string modulo, int minutosSinLatido)
        {
            // Una sola notificacion por borrador (tipo + refid unico): si ya tiene una de cualquier
            // tipo relacionado (interrumpido, descartado) no se duplica la alerta. Mismo criterio que
            // VentaBorradorPg.CrearNotificacionesVentasInterrumpidas.
            return DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO notificaciones (idempresa, idsucursal, tipo, severidad, titulo, mensaje, refid, creado, actualizado)
                SELECT b.idempresa, b.idsucursal, 'BORRADOR_INTERRUMPIDO_' || b.modulo, 'ADVERTENCIA',
                       'Borrador interrumpido (' ||
                       CASE b.modulo
                           WHEN 'COMPRA' THEN 'Compras' WHEN 'STOCK' THEN 'Stock' WHEN 'MOVIMIENTO' THEN 'Movimientos'
                           WHEN 'EMBUTIDO_CARGA' THEN 'Embutidos' WHEN 'EMBUTIDO_RAPIDO' THEN 'Embutidos - ingreso rápido'
                           ELSE b.modulo
                       END || ')',
                       COALESCE(u.nombre, 'Un operador') || ' dejó un formulario sin guardar'
                           || CASE WHEN b.resumen IS NOT NULL AND b.resumen <> '' THEN ' (' || b.resumen || ')' ELSE '' END
                           || ', última señal ' || to_char(b.ultimolatido, 'DD/MM/YYYY HH24:MI:SS') || '.',
                       b.id, now(), b.ultimolatido
                FROM borradorgenerico b
                LEFT JOIN usuarios u ON u.id = b.idoperador
                WHERE b.idempresa = @idEmpresa
                  AND b.modulo = @modulo
                  AND b.estado = 'ACTIVA'
                  AND b.cantlineas > 0
                  AND b.ultimolatido < now() - (@minutos * interval '1 minute')
                  AND NOT EXISTS (
                        SELECT 1 FROM notificaciones n
                        WHERE n.idempresa = b.idempresa AND n.refid = b.id
                          AND n.tipo IN ('BORRADOR_INTERRUMPIDO_' || b.modulo, 'BORRADOR_DESCARTADO_' || b.modulo))
                ON CONFLICT (idempresa, tipo, refid) DO NOTHING;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("modulo", modulo);
                    p.AddWithValue("minutos", minutosSinLatido);
                });
        }

        public void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir)
        {
            if (notificacion == null) throw new ArgumentNullException(nameof(notificacion));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO notificaciones (idempresa, idsucursal, tipo, severidad, titulo, mensaje, refid, creado, actualizado)
                VALUES (@idEmpresa, @idSucursal, @tipo, @severidad, @titulo, @mensaje, @refId, now(), now())
                ON CONFLICT (idempresa, tipo, refid) DO UPDATE SET
                    severidad   = EXCLUDED.severidad,
                    titulo      = EXCLUDED.titulo,
                    mensaje     = EXCLUDED.mensaje,
                    actualizado = now(),
                    atendidapor = CASE WHEN @reabrir THEN NULL ELSE notificaciones.atendidapor END,
                    atendidaen  = CASE WHEN @reabrir THEN NULL ELSE notificaciones.atendidaen END;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", ONulo(notificacion.IdSucursal));
                    p.AddWithValue("tipo", notificacion.Tipo);
                    p.AddWithValue("severidad", notificacion.Severidad ?? Entidades.Notificacion.SeveridadAdvertencia);
                    p.AddWithValue("titulo", notificacion.Titulo ?? "");
                    p.AddWithValue("mensaje", notificacion.Mensaje ?? "");
                    p.AddWithValue("refId", notificacion.RefId);
                    p.AddWithValue("reabrir", reabrir);
                });
        }
    }
}
