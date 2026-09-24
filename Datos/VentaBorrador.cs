using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Implementacion SQL Server de Contratos.IVentaBorradorRepository: VentaBorrador (venta en curso del POS)
    // y VentaBorradorEvento (append-only). Espejo de DatosPostgres.VentaBorradorPg; esquema en
    // Datos/DB-Procedures/20260923-Create_Borradores.sql y decision en docs/DECISIONS.md "Borradores en SQL Server".
    // Ver Datos/BorradorGenerico.cs para las diferencias deliberadas con la version Postgres (sin RLS ->
    // idEmpresa explicito, upsert UPDATE+INSERT con reintento, SYSDATETIME() de la base, payload NVARCHAR(MAX)).
    //
    // ETAPA 2 (no implementado en SQL Server): "producto sin agregar" (tabla VentaProductoSinAgregar) y
    // notificaciones al admin (tabla Notificaciones). Esos metodos devuelven vacio/0/false/null de forma
    // segura; el codigo web tampoco los invoca en SQL Server (ver WebCore/Helpers/PosBorradorSettings.cs).
    public class VentaBorrador : Contratos.IVentaBorradorRepository
    {
        private const string ColumnasBorradorSinPayload =
            "b.id, b.idEmpresa, b.idSucursal, b.idOperador, u.nombre AS nombreOperador, b.idUsuarioSesion, b.clientId, b.posInstanceId, " +
            "b.idCierreCaja, b.idPersona, b.razonSocial, b.cantLineas, b.total, CAST(NULL AS NVARCHAR(MAX)) AS payload, b.estado, b.idVenta, " +
            "b.creado, b.ultimoLatido, b.actualizado, b.finalizado, DATEDIFF(SECOND, b.ultimoLatido, SYSDATETIME()) AS segundosSinLatido";

        private const string ColumnasBorradorConPayload =
            "b.id, b.idEmpresa, b.idSucursal, b.idOperador, u.nombre AS nombreOperador, b.idUsuarioSesion, b.clientId, b.posInstanceId, " +
            "b.idCierreCaja, b.idPersona, b.razonSocial, b.cantLineas, b.total, b.payload AS payload, b.estado, b.idVenta, " +
            "b.creado, b.ultimoLatido, b.actualizado, b.finalizado, DATEDIFF(SECOND, b.ultimoLatido, SYSDATETIME()) AS segundosSinLatido";

        private const string ColumnasEvento =
            "e.id, e.idEmpresa, e.idBorrador, e.fecha, e.tipo, e.idUsuario, u.nombre AS nombreUsuario, e.detalle, " +
            "ub.nombre AS nombreOperador, b.total, b.cantLineas";

        private readonly IEmpresaContext _empresa;

        public VentaBorrador(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        // ------------------------------------------------------------------------------------
        // Mapeos
        // ------------------------------------------------------------------------------------

        private static Entidades.VentaBorrador MapearBorrador(SqlDataReader dr)
        {
            return new Entidades.VentaBorrador
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                IdSucursal = Convert.ToInt32(dr["idSucursal"]),
                IdOperador = Convert.ToInt32(dr["idOperador"]),
                NombreOperador = BorradoresSql.Texto(dr, "nombreOperador"),
                IdUsuarioSesion = Convert.ToInt32(dr["idUsuarioSesion"]),
                ClientId = (Guid)dr["clientId"],
                PosInstanceId = BorradoresSql.Texto(dr, "posInstanceId"),
                IdCierreCaja = BorradoresSql.EnteroNulo(dr, "idCierreCaja"),
                IdPersona = BorradoresSql.EnteroNulo(dr, "idPersona"),
                RazonSocial = BorradoresSql.Texto(dr, "razonSocial"),
                CantLineas = Convert.ToInt32(dr["cantLineas"]),
                Total = Convert.ToDecimal(dr["total"]),
                Payload = BorradoresSql.Texto(dr, "payload"),
                Estado = Convert.ToString(dr["estado"]),
                IdVenta = BorradoresSql.EnteroNulo(dr, "idVenta"),
                Creado = Convert.ToDateTime(dr["creado"]),
                UltimoLatido = Convert.ToDateTime(dr["ultimoLatido"]),
                Actualizado = BorradoresSql.FechaNula(dr, "actualizado"),
                Finalizado = BorradoresSql.FechaNula(dr, "finalizado"),
                SegundosSinLatido = Convert.ToInt32(dr["segundosSinLatido"])
            };
        }

        private static Entidades.VentaBorradorEvento MapearEvento(SqlDataReader dr)
        {
            return new Entidades.VentaBorradorEvento
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                IdBorrador = Convert.ToInt32(dr["idBorrador"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Tipo = Convert.ToString(dr["tipo"]),
                IdUsuario = Convert.ToInt32(dr["idUsuario"]),
                NombreUsuario = BorradoresSql.Texto(dr, "nombreUsuario"),
                Detalle = BorradoresSql.Texto(dr, "detalle"),
                NombreOperador = BorradoresSql.Texto(dr, "nombreOperador"),
                Total = dr["total"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["total"]),
                CantLineas = dr["cantLineas"] == DBNull.Value ? 0 : Convert.ToInt32(dr["cantLineas"])
            };
        }

        // ------------------------------------------------------------------------------------
        // Borradores
        // ------------------------------------------------------------------------------------

        public Entidades.ResultadoGuardarBorrador Guardar(Entidades.VentaBorrador borrador)
        {
            if (borrador == null) throw new ArgumentNullException(nameof(borrador));

            // Upsert en un solo lote/transaccion (ver BorradorGenerico.Guardar): el UPDATE solo aplica si la
            // fila es ACTIVA y del mismo operador y sucursal; si no existe ninguna con ese clientId se
            // inserta; si existe pero no coincide devuelve NULL y abajo se averigua el motivo.
            object id = BorradoresSql.ConReintentoPorCarrera(() => Db.Scalar(
                _empresa,
                @"SET XACT_ABORT ON;
                  BEGIN TRAN;
                  DECLARE @id INT;
                  UPDATE VentaBorrador WITH (UPDLOCK, HOLDLOCK)
                     SET posInstanceId = @posInstanceId, idCierreCaja = @idCierreCaja, idPersona = @idPersona,
                         razonSocial = @razonSocial, cantLineas = @cantLineas, total = @total, payload = @payload,
                         ultimoLatido = SYSDATETIME(), actualizado = SYSDATETIME(), @id = id
                   WHERE idEmpresa = @idEmpresa AND clientId = @clientId AND estado = 'ACTIVA'
                     AND idOperador = @idOperador AND idSucursal = @idSucursal;
                  IF @id IS NULL AND NOT EXISTS (SELECT 1 FROM VentaBorrador WITH (UPDLOCK, HOLDLOCK)
                                                  WHERE idEmpresa = @idEmpresa AND clientId = @clientId)
                  BEGIN
                      INSERT INTO VentaBorrador
                          (idEmpresa, idSucursal, idOperador, idUsuarioSesion, clientId, posInstanceId, idCierreCaja, idPersona,
                           razonSocial, cantLineas, total, payload, estado, creado, ultimoLatido)
                      VALUES
                          (@idEmpresa, @idSucursal, @idOperador, @idUsuarioSesion, @clientId, @posInstanceId, @idCierreCaja, @idPersona,
                           @razonSocial, @cantLineas, @total, @payload, 'ACTIVA', SYSDATETIME(), SYSDATETIME());
                      SET @id = SCOPE_IDENTITY();
                  END
                  COMMIT;
                  SELECT @id;",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idSucursal", SqlDbType.Int).Value = borrador.IdSucursal;
                    p.Add("@idOperador", SqlDbType.Int).Value = borrador.IdOperador;
                    p.Add("@idUsuarioSesion", SqlDbType.Int).Value = borrador.IdUsuarioSesion;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = borrador.ClientId;
                    p.Add("@posInstanceId", SqlDbType.NVarChar, 200).Value = BorradoresSql.ONulo(borrador.PosInstanceId);
                    p.Add("@idCierreCaja", SqlDbType.Int).Value = BorradoresSql.ONulo(borrador.IdCierreCaja);
                    p.Add("@idPersona", SqlDbType.Int).Value = BorradoresSql.ONulo(borrador.IdPersona);
                    p.Add("@razonSocial", SqlDbType.NVarChar, -1).Value = BorradoresSql.ONulo(borrador.RazonSocial);
                    p.Add("@cantLineas", SqlDbType.Int).Value = borrador.CantLineas;
                    var total = p.Add("@total", SqlDbType.Decimal);
                    total.Precision = 14;
                    total.Scale = 2;
                    total.Value = borrador.Total;
                    p.Add("@payload", SqlDbType.NVarChar, -1).Value = BorradoresSql.ONulo(borrador.Payload);
                }));

            if (id != null && id != DBNull.Value)
            {
                borrador.Id = Convert.ToInt32(id);
                return Entidades.ResultadoGuardarBorrador.Guardado;
            }

            // No se guardo: distinguir "ya cerrada" de "ajena".
            var existente = ObtenerPorClientId(borrador.ClientId);
            if (existente != null && (existente.IdOperador != borrador.IdOperador || existente.IdSucursal != borrador.IdSucursal))
                return Entidades.ResultadoGuardarBorrador.Ajeno;

            return Entidades.ResultadoGuardarBorrador.YaCerrado;
        }

        public bool RegistrarLatido(Guid clientId, int idOperador, int idSucursal)
        {
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE VentaBorrador SET ultimoLatido = SYSDATETIME()
                  WHERE idEmpresa = @idEmpresa AND clientId = @clientId AND idOperador = @idOperador
                    AND idSucursal = @idSucursal AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                    p.Add("@idOperador", SqlDbType.Int).Value = idOperador;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                });
            return filas > 0;
        }

        public Entidades.VentaBorrador ObtenerPorClientId(Guid clientId)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM VentaBorrador b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.clientId = @clientId;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.VentaBorrador ObtenerPorId(int id)
        {
            var lista = Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM VentaBorrador b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
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

        public List<Entidades.VentaBorrador> ListarActivasPorSucursal(int idSucursal)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM VentaBorrador b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.idSucursal = @idSucursal AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                });
        }

        public List<Entidades.VentaBorrador> ListarActivasPorOperador(int idOperador, int idSucursal)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM VentaBorrador b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.idOperador = @idOperador AND b.idSucursal = @idSucursal AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idOperador", SqlDbType.Int).Value = idOperador;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                });
        }

        public List<Entidades.VentaBorrador> ListarActivasPorUsuarioSesion(int idUsuarioSesion)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM VentaBorrador b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
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
                @"UPDATE VentaBorrador
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

        public bool MarcarFinalizada(Guid clientId, int idVenta)
        {
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE VentaBorrador
                  SET estado = 'FINALIZADA', idVenta = @idVenta, payload = NULL, finalizado = SYSDATETIME(), actualizado = SYSDATETIME()
                  WHERE idEmpresa = @idEmpresa AND clientId = @clientId AND estado = 'ACTIVA';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                    p.Add("@idVenta", SqlDbType.Int).Value = idVenta;
                });
            return filas > 0;
        }

        public bool MarcarDescartada(int id)
        {
            int filas = Db.NonQuery(
                _empresa,
                @"UPDATE VentaBorrador
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

        public int PurgarFinalizadasAntiguas(int dias)
        {
            return Db.NonQuery(
                _empresa,
                @"DELETE FROM VentaBorrador
                  WHERE idEmpresa = @idEmpresa AND estado = 'FINALIZADA' AND finalizado < DATEADD(DAY, -@dias, SYSDATETIME());",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@dias", SqlDbType.Int).Value = dias;
                });
        }

        public List<Entidades.VentaBorrador> ListarInterrumpidasPorRango(DateTime desde, DateTime hasta, int minutosSinLatido)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM VentaBorrador b LEFT JOIN Usuarios u ON u.id = b.idOperador " +
                "WHERE b.idEmpresa = @idEmpresa AND b.estado = 'ACTIVA' " +
                "AND b.ultimoLatido BETWEEN @desde AND @hasta " +
                "AND b.ultimoLatido < DATEADD(MINUTE, -@minutos, SYSDATETIME()) " +
                "ORDER BY b.ultimoLatido DESC;",
                CommandType.Text,
                MapearBorrador,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@desde", SqlDbType.DateTime2).Value = desde;
                    p.Add("@hasta", SqlDbType.DateTime2).Value = hasta;
                    p.Add("@minutos", SqlDbType.Int).Value = minutosSinLatido;
                });
        }

        // ------------------------------------------------------------------------------------
        // Eventos
        // ------------------------------------------------------------------------------------

        public void AgregarEvento(Entidades.VentaBorradorEvento evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            Db.NonQuery(
                _empresa,
                @"INSERT INTO VentaBorradorEvento (idEmpresa, idBorrador, fecha, tipo, idUsuario, detalle)
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

        public List<Entidades.VentaBorradorEvento> ListarEventosPorBorrador(int idBorrador)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasEvento + " FROM VentaBorradorEvento e LEFT JOIN Usuarios u ON u.id = e.idUsuario " +
                "LEFT JOIN VentaBorrador b ON b.id = e.idBorrador AND b.idEmpresa = e.idEmpresa LEFT JOIN Usuarios ub ON ub.id = b.idOperador " +
                "WHERE e.idEmpresa = @idEmpresa AND e.idBorrador = @idBorrador ORDER BY e.fecha, e.id;",
                CommandType.Text,
                MapearEvento,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idBorrador", SqlDbType.Int).Value = idBorrador;
                });
        }

        public List<Entidades.VentaBorradorEvento> ListarEventosPorRango(DateTime desde, DateTime hasta)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + ColumnasEvento + " FROM VentaBorradorEvento e LEFT JOIN Usuarios u ON u.id = e.idUsuario " +
                "LEFT JOIN VentaBorrador b ON b.id = e.idBorrador AND b.idEmpresa = e.idEmpresa LEFT JOIN Usuarios ub ON ub.id = b.idOperador " +
                "WHERE e.idEmpresa = @idEmpresa AND e.fecha BETWEEN @desde AND @hasta ORDER BY e.fecha DESC, e.id DESC;",
                CommandType.Text,
                MapearEvento,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@desde", SqlDbType.DateTime2).Value = desde;
                    p.Add("@hasta", SqlDbType.DateTime2).Value = hasta;
                });
        }

        // ------------------------------------------------------------------------------------
        // Producto sin agregar: ETAPA 2, sin tabla en SQL Server
        // ------------------------------------------------------------------------------------

        // ETAPA 2: la advertencia "producto sin agregar" queda apagada en SQL Server
        // (PosBorradorSettings.AdvertenciaProductoSinAgregarHabilitada = false), asi que nadie llama esto.
        // Devuelve 0 (id inexistente) en vez de lanzar para no romper un flujo de venta si alguien lo invoca.
        public int AgregarProductoSinAgregar(Entidades.ProductoSinAgregar producto)
        {
            return 0;
        }

        public Entidades.ProductoSinAgregar ObtenerProductoSinAgregar(int id)
        {
            return null;
        }

        public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarPorRango(DateTime desde, DateTime hasta)
        {
            return new List<Entidades.ProductoSinAgregar>();
        }

        public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarDelDia(int idOperador, DateTime dia)
        {
            return new List<Entidades.ProductoSinAgregar>();
        }

        public Entidades.ResumenProductoSinAgregar ResumirProductoSinAgregar(int idOperador, DateTime desde, DateTime hasta)
        {
            return new Entidades.ResumenProductoSinAgregar();
        }

        public bool RevisarProductoSinAgregar(int id, string revision, string comentario, int idUsuario)
        {
            return false;
        }

        // ------------------------------------------------------------------------------------
        // Notificaciones: ETAPA 2, sin tabla en SQL Server
        // ------------------------------------------------------------------------------------

        public void UpsertNotificacion(Entidades.Notificacion notificacion, bool reabrir)
        {
        }

        public List<Entidades.Notificacion> ListarNotificaciones(bool soloPendientes, int max)
        {
            return new List<Entidades.Notificacion>();
        }

        public int ContarNotificacionesPendientes()
        {
            return 0;
        }

        public Entidades.Notificacion ObtenerNotificacion(int id)
        {
            return null;
        }

        public bool AtenderNotificacion(int id, int idUsuario)
        {
            return false;
        }

        public int CrearNotificacionesVentasInterrumpidas(int minutosSinLatido)
        {
            return 0;
        }
    }
}
