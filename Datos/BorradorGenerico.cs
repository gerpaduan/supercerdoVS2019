using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Implementacion SQL Server de Contratos.IBorradorGenericoRepository: BorradorGenerico (formulario en
    // curso de Compras/Stock/Movimientos/Embutidos) y BorradorGenericoEvento (append-only). Espejo de
    // DatosPostgres.BorradorGenericoPg; esquema en Datos/DB-Procedures/20260923-Create_Borradores.sql y
    // decision en docs/DECISIONS.md "Borradores en SQL Server".
    //
    // Diferencias con la version Postgres (deliberadas, por SQL Server 2008 RTM y por SuperCerdo sin RLS):
    //  - Sin RLS: cada consulta filtra idEmpresa de forma EXPLICITA.
    //  - Sin ON CONFLICT: el upsert es UPDATE + INSERT bajo UPDLOCK/HOLDLOCK y se reintenta una vez si pierde
    //    una carrera (ver BorradoresSql.ConReintentoPorCarrera).
    //  - Relojes: SYSDATETIME() de la BASE, nunca el reloj del navegador ni del servidor web.
    //  - Payload NVARCHAR(MAX) en vez de jsonb (2008 no tiene JSON; el payload es opaco para Negocio).
    //  - Notificaciones al admin: NO-OP en esta etapa (tabla Notificaciones = etapa 2).
    public class BorradorGenerico : Contratos.IBorradorGenericoRepository
    {
        private const string ColumnasBorradorSinPayload =
            "b.id, b.idEmpresa, b.idSucursal, b.modulo, b.idRegistro, b.idOperador, u.nombre AS nombreOperador, b.idUsuarioSesion, " +
            "b.clientId, b.resumen, b.cantLineas, CAST(NULL AS NVARCHAR(MAX)) AS payload, b.estado, b.idResultado, " +
            "b.creado, b.ultimoLatido, b.actualizado, b.finalizado, DATEDIFF(SECOND, b.ultimoLatido, SYSDATETIME()) AS segundosSinLatido";

        private const string ColumnasBorradorConPayload =
            "b.id, b.idEmpresa, b.idSucursal, b.modulo, b.idRegistro, b.idOperador, u.nombre AS nombreOperador, b.idUsuarioSesion, " +
            "b.clientId, b.resumen, b.cantLineas, b.payload AS payload, b.estado, b.idResultado, " +
            "b.creado, b.ultimoLatido, b.actualizado, b.finalizado, DATEDIFF(SECOND, b.ultimoLatido, SYSDATETIME()) AS segundosSinLatido";

        private const string ColumnasEvento =
            "e.id, e.idEmpresa, e.idBorrador, e.fecha, e.tipo, e.idUsuario, u.nombre AS nombreUsuario, e.detalle, " +
            "b.modulo, ub.nombre AS nombreOperador, b.resumen, b.cantLineas";

        private readonly IEmpresaContext _empresa;

        public BorradorGenerico(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        // ------------------------------------------------------------------------------------
        // Mapeos
        // ------------------------------------------------------------------------------------

        private static Entidades.BorradorGenerico MapearBorrador(SqlDataReader dr)
        {
            return new Entidades.BorradorGenerico
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                IdSucursal = Convert.ToInt32(dr["idSucursal"]),
                Modulo = Convert.ToString(dr["modulo"]),
                IdRegistro = BorradoresSql.EnteroNulo(dr, "idRegistro"),
                IdOperador = Convert.ToInt32(dr["idOperador"]),
                NombreOperador = BorradoresSql.Texto(dr, "nombreOperador"),
                IdUsuarioSesion = Convert.ToInt32(dr["idUsuarioSesion"]),
                ClientId = (Guid)dr["clientId"],
                Resumen = BorradoresSql.Texto(dr, "resumen"),
                CantLineas = Convert.ToInt32(dr["cantLineas"]),
                Payload = BorradoresSql.Texto(dr, "payload"),
                Estado = Convert.ToString(dr["estado"]),
                IdResultado = BorradoresSql.EnteroNulo(dr, "idResultado"),
                Creado = Convert.ToDateTime(dr["creado"]),
                UltimoLatido = Convert.ToDateTime(dr["ultimoLatido"]),
                Actualizado = BorradoresSql.FechaNula(dr, "actualizado"),
                Finalizado = BorradoresSql.FechaNula(dr, "finalizado"),
                SegundosSinLatido = Convert.ToInt32(dr["segundosSinLatido"])
            };
        }

        private static Entidades.BorradorGenericoEvento MapearEvento(SqlDataReader dr)
        {
            return new Entidades.BorradorGenericoEvento
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                IdBorrador = Convert.ToInt32(dr["idBorrador"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Tipo = Convert.ToString(dr["tipo"]),
                IdUsuario = Convert.ToInt32(dr["idUsuario"]),
                NombreUsuario = BorradoresSql.Texto(dr, "nombreUsuario"),
                Detalle = BorradoresSql.Texto(dr, "detalle"),
                Modulo = BorradoresSql.Texto(dr, "modulo"),
                NombreOperador = BorradoresSql.Texto(dr, "nombreOperador"),
                Resumen = BorradoresSql.Texto(dr, "resumen"),
                CantLineas = dr["cantLineas"] == DBNull.Value ? 0 : Convert.ToInt32(dr["cantLineas"])
            };
        }

        // ------------------------------------------------------------------------------------
        // Borradores
        // ------------------------------------------------------------------------------------

        public Entidades.ResultadoGuardarBorradorGenerico Guardar(Entidades.BorradorGenerico borrador)
        {
            if (borrador == null) throw new ArgumentNullException(nameof(borrador));

            // Upsert en un solo lote/transaccion: el UPDATE solo aplica si la fila es ACTIVA y del mismo
            // operador, sucursal y modulo (deja el id en @id); si no hay ninguna fila con ese clientId se
            // inserta. Si existe pero no coincide (cerrada o ajena) no hace nada y devuelve NULL, y abajo se
            // averigua el motivo. UPDLOCK+HOLDLOCK serializa dos guardados concurrentes del mismo clientId.
            object id = BorradoresSql.ConReintentoPorCarrera(() => Db.Scalar(
                _empresa,
                @"SET XACT_ABORT ON;
                  BEGIN TRAN;
                  DECLARE @id INT;
                  UPDATE BorradorGenerico WITH (UPDLOCK, HOLDLOCK)
                     SET idRegistro = @idRegistro, resumen = @resumen, cantLineas = @cantLineas, payload = @payload,
                         ultimoLatido = SYSDATETIME(), actualizado = SYSDATETIME(), @id = id
                   WHERE idEmpresa = @idEmpresa AND clientId = @clientId AND estado = 'ACTIVA'
                     AND idOperador = @idOperador AND idSucursal = @idSucursal AND modulo = @modulo;
                  IF @id IS NULL AND NOT EXISTS (SELECT 1 FROM BorradorGenerico WITH (UPDLOCK, HOLDLOCK)
                                                  WHERE idEmpresa = @idEmpresa AND clientId = @clientId)
                  BEGIN
                      INSERT INTO BorradorGenerico
                          (idEmpresa, idSucursal, modulo, idRegistro, idOperador, idUsuarioSesion, clientId,
                           resumen, cantLineas, payload, estado, creado, ultimoLatido)
                      VALUES
                          (@idEmpresa, @idSucursal, @modulo, @idRegistro, @idOperador, @idUsuarioSesion, @clientId,
                           @resumen, @cantLineas, @payload, 'ACTIVA', SYSDATETIME(), SYSDATETIME());
                      SET @id = SCOPE_IDENTITY();
                  END
                  COMMIT;
                  SELECT @id;",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idSucursal", SqlDbType.Int).Value = borrador.IdSucursal;
                    p.Add("@modulo", SqlDbType.VarChar, 30).Value = borrador.Modulo;
                    p.Add("@idRegistro", SqlDbType.Int).Value = BorradoresSql.ONulo(borrador.IdRegistro);
                    p.Add("@idOperador", SqlDbType.Int).Value = borrador.IdOperador;
                    p.Add("@idUsuarioSesion", SqlDbType.Int).Value = borrador.IdUsuarioSesion;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = borrador.ClientId;
                    p.Add("@resumen", SqlDbType.NVarChar, -1).Value = BorradoresSql.ONulo(borrador.Resumen);
                    p.Add("@cantLineas", SqlDbType.Int).Value = borrador.CantLineas;
                    p.Add("@payload", SqlDbType.NVarChar, -1).Value = BorradoresSql.ONulo(borrador.Payload);
                }));

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
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE BorradorGenerico SET ultimoLatido = SYSDATETIME()
                  WHERE idEmpresa = @idEmpresa AND clientId = @clientId AND idOperador = @idOperador
                    AND idSucursal = @idSucursal AND modulo = @modulo AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                    p.Add("@idOperador", SqlDbType.Int).Value = idOperador;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                    p.Add("@modulo", SqlDbType.VarChar, 30).Value = modulo;
                });
            return filas > 0;
        }

        public bool EnvejecerLatidoPorCierre(int idBorrador)
        {
            // 1 dia alcanza y sobra para superar cualquier umbral configurado (maximo 240 min = 4 h):
            // queda "interrumpido" de inmediato en vez de esperar el latido, sin tocar el estado.
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE BorradorGenerico SET ultimoLatido = DATEADD(DAY, -1, SYSDATETIME())
                  WHERE idEmpresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@id", SqlDbType.Int).Value = idBorrador;
                });
            return filas > 0;
        }

        public Entidades.BorradorGenerico ObtenerPorClientId(Guid clientId, string modulo)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM BorradorGenerico b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.clientId = @clientId AND b.modulo = @modulo;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                    p.Add("@modulo", SqlDbType.VarChar, 30).Value = modulo;
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.BorradorGenerico ObtenerPorId(int id)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM BorradorGenerico b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.id = @id;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@id", SqlDbType.Int).Value = id;
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public List<Entidades.BorradorGenerico> ListarActivasPorSucursal(int idSucursal, string modulo)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM BorradorGenerico b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.idSucursal = @idSucursal AND b.modulo = @modulo AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                    p.Add("@modulo", SqlDbType.VarChar, 30).Value = modulo;
                });
        }

        public List<Entidades.BorradorGenerico> ListarActivasPorUsuarioSesion(int idUsuarioSesion)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM BorradorGenerico b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.idUsuarioSesion = @idUsuarioSesion AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idUsuarioSesion", SqlDbType.Int).Value = idUsuarioSesion;
                });
        }

        public bool TomarBorrador(int id, int idOperador, int idUsuarioSesion)
        {
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE BorradorGenerico
                  SET idOperador = @idOperador, idUsuarioSesion = @idUsuarioSesion, ultimoLatido = SYSDATETIME(), actualizado = SYSDATETIME()
                  WHERE idEmpresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idOperador", SqlDbType.Int).Value = idOperador;
                    p.Add("@idUsuarioSesion", SqlDbType.Int).Value = idUsuarioSesion;
                });
            return filas > 0;
        }

        public bool MarcarFinalizada(Guid clientId, string modulo, int idResultado)
        {
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE BorradorGenerico
                  SET estado = 'FINALIZADA', idResultado = @idResultado, payload = NULL, finalizado = SYSDATETIME(), actualizado = SYSDATETIME()
                  WHERE idEmpresa = @idEmpresa AND clientId = @clientId AND modulo = @modulo AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                    p.Add("@modulo", SqlDbType.VarChar, 30).Value = modulo;
                    p.Add("@idResultado", SqlDbType.Int).Value = idResultado;
                });
            return filas > 0;
        }

        public bool MarcarDescartada(int id)
        {
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE BorradorGenerico
                  SET estado = 'DESCARTADA', payload = NULL, finalizado = SYSDATETIME(), actualizado = SYSDATETIME()
                  WHERE idEmpresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@id", SqlDbType.Int).Value = id;
                });
            return filas > 0;
        }

        public int PurgarFinalizadasAntiguas(string modulo, int dias)
        {
            return Db.NonQuery(
                _empresa,
                @"DELETE FROM BorradorGenerico
                  WHERE idEmpresa = @idEmpresa AND modulo = @modulo AND estado = 'FINALIZADA'
                    AND finalizado < DATEADD(DAY, -@dias, SYSDATETIME());",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@modulo", SqlDbType.VarChar, 30).Value = modulo;
                    p.Add("@dias", SqlDbType.Int).Value = dias;
                });
        }

        // ------------------------------------------------------------------------------------
        // Eventos
        // ------------------------------------------------------------------------------------

        public void AgregarEvento(Entidades.BorradorGenericoEvento evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            Db.NonQuery(
                _empresa,
                @"INSERT INTO BorradorGenericoEvento (idEmpresa, idBorrador, fecha, tipo, idUsuario, detalle)
                  VALUES (@idEmpresa, @idBorrador, SYSDATETIME(), @tipo, @idUsuario, @detalle);",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idBorrador", SqlDbType.Int).Value = evento.IdBorrador;
                    p.Add("@tipo", SqlDbType.VarChar, 20).Value = evento.Tipo;
                    p.Add("@idUsuario", SqlDbType.Int).Value = evento.IdUsuario;
                    p.Add("@detalle", SqlDbType.NVarChar, -1).Value = BorradoresSql.ONulo(evento.Detalle);
                });
        }

        public List<Entidades.BorradorGenericoEvento> ListarEventosPorBorrador(int idBorrador)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasEvento + " FROM BorradorGenericoEvento e LEFT JOIN Usuarios u ON u.id = e.idUsuario " +
                "LEFT JOIN BorradorGenerico b ON b.id = e.idBorrador AND b.idEmpresa = e.idEmpresa LEFT JOIN Usuarios ub ON ub.id = b.idOperador " +
                "WHERE e.idEmpresa = @idEmpresa AND e.idBorrador = @idBorrador ORDER BY e.fecha, e.id;",
                CommandType.Text,
                MapearEvento,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idBorrador", SqlDbType.Int).Value = idBorrador;
                });
        }

        // ------------------------------------------------------------------------------------
        // Notificaciones: NO-OP en SQL Server (etapa 2)
        // ------------------------------------------------------------------------------------

        // ETAPA 2: cuando exista la tabla Notificaciones en SQL Server (campana del admin) se implementa
        // aca. Mientras tanto no se crea nada y el descarte queda registrado solo en BorradorGenericoEvento.
        public int CrearNotificacionesInterrumpidas(string modulo, int minutosSinLatido)
        {
            return 0;
        }

        // ETAPA 2: ver CrearNotificacionesInterrumpidas.
        public void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir)
        {
        }
    }
}
