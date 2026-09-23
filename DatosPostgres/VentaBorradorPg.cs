using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IVentaBorradorRepository: ventaborrador (venta en curso del
    // POS), ventaborradorevento (eventos append-only), ventaproductosinagregar (advertencia de producto
    // pesado no agregado) y notificaciones (campana del admin). Ver migracion
    // 20260921b-Create_ventaborrador_productosinagregar_notificaciones.sql y docs/DECISIONS.md.
    // Todas las tablas tienen RLS por empresa: DbPg setea el tenant en cada operacion.
    // Relojes: inicio/fin/ultimolatido/creado se calculan con now() de la BASE, nunca con el reloj del
    // navegador ni del servidor web, para que un cajero no pueda falsear horas cambiando la de su PC.
    public class VentaBorradorPg : Contratos.IVentaBorradorRepository
    {
        private const string ColumnasBorradorSinPayload =
            "b.id, b.idempresa, b.idsucursal, b.idoperador, u.nombre AS nombreoperador, b.idusuariosesion, b.clientid, b.posinstanceid, " +
            "b.idcierrecaja, b.idpersona, b.razonsocial, b.cantlineas, b.total, NULL::text AS payload, b.estado, b.idventa, " +
            "b.creado, b.ultimolatido, b.actualizado, b.finalizado, EXTRACT(EPOCH FROM (now() - b.ultimolatido))::int AS segundossinlatido";

        private const string ColumnasBorradorConPayload =
            "b.id, b.idempresa, b.idsucursal, b.idoperador, u.nombre AS nombreoperador, b.idusuariosesion, b.clientid, b.posinstanceid, " +
            "b.idcierrecaja, b.idpersona, b.razonsocial, b.cantlineas, b.total, b.payload::text AS payload, b.estado, b.idventa, " +
            "b.creado, b.ultimolatido, b.actualizado, b.finalizado, EXTRACT(EPOCH FROM (now() - b.ultimolatido))::int AS segundossinlatido";

        private const string ColumnasEvento =
            "e.id, e.idempresa, e.idborrador, e.fecha, e.tipo, e.idusuario, u.nombre AS nombreusuario, e.detalle, " +
            "ub.nombre AS nombreoperador, b.total, b.cantlineas";

        private const string ColumnasProducto =
            "p.id, p.idempresa, p.idsucursal, p.idoperador, u.nombre AS nombreoperador, p.idusuariosesion, p.clientid, p.codigo, p.producto, " +
            "p.preciokg, p.cantidadkg, p.importe, p.segundosenpantalla, p.segundosestables, p.origen, p.motivo, p.inicio, p.fin, p.creado, " +
            "p.revision, p.revisadapor, r.nombre AS nombrerevisor, p.revisadaen, p.comentariorevision";

        private const string ColumnasNotificacion =
            "n.id, n.idempresa, n.idsucursal, n.tipo, n.severidad, n.titulo, n.mensaje, n.refid, n.creado, n.actualizado, " +
            "n.atendidapor, ua.nombre AS nombreatendidapor, n.atendidaen";

        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public VentaBorradorPg(string connectionString, int idEmpresa)
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

        private static Entidades.VentaBorrador MapearBorrador(System.Data.IDataRecord dr)
        {
            return new Entidades.VentaBorrador
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                IdOperador = Convert.ToInt32(dr["idoperador"]),
                NombreOperador = Texto(dr, "nombreoperador"),
                IdUsuarioSesion = Convert.ToInt32(dr["idusuariosesion"]),
                ClientId = (Guid)dr["clientid"],
                PosInstanceId = Texto(dr, "posinstanceid"),
                IdCierreCaja = EnteroNulo(dr, "idcierrecaja"),
                IdPersona = EnteroNulo(dr, "idpersona"),
                RazonSocial = Texto(dr, "razonsocial"),
                CantLineas = Convert.ToInt32(dr["cantlineas"]),
                Total = Convert.ToDecimal(dr["total"]),
                Payload = Texto(dr, "payload"),
                Estado = Convert.ToString(dr["estado"]),
                IdVenta = EnteroNulo(dr, "idventa"),
                Creado = Convert.ToDateTime(dr["creado"]),
                UltimoLatido = Convert.ToDateTime(dr["ultimolatido"]),
                Actualizado = FechaNula(dr, "actualizado"),
                Finalizado = FechaNula(dr, "finalizado"),
                SegundosSinLatido = Convert.ToInt32(dr["segundossinlatido"])
            };
        }

        private static Entidades.VentaBorradorEvento MapearEvento(System.Data.IDataRecord dr)
        {
            return new Entidades.VentaBorradorEvento
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdBorrador = Convert.ToInt32(dr["idborrador"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Tipo = Convert.ToString(dr["tipo"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                NombreUsuario = Texto(dr, "nombreusuario"),
                Detalle = Texto(dr, "detalle"),
                NombreOperador = Texto(dr, "nombreoperador"),
                Total = dr["total"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["total"]),
                CantLineas = dr["cantlineas"] == DBNull.Value ? 0 : Convert.ToInt32(dr["cantlineas"])
            };
        }

        private static Entidades.ProductoSinAgregar MapearProducto(System.Data.IDataRecord dr)
        {
            object clientId = dr["clientid"];
            return new Entidades.ProductoSinAgregar
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                IdOperador = Convert.ToInt32(dr["idoperador"]),
                NombreOperador = Texto(dr, "nombreoperador"),
                IdUsuarioSesion = Convert.ToInt32(dr["idusuariosesion"]),
                ClientId = clientId == DBNull.Value ? (Guid?)null : (Guid)clientId,
                Codigo = Convert.ToString(dr["codigo"]),
                Producto = Convert.ToString(dr["producto"]),
                PrecioKg = Convert.ToDecimal(dr["preciokg"]),
                CantidadKg = Convert.ToDecimal(dr["cantidadkg"]),
                Importe = Convert.ToDecimal(dr["importe"]),
                SegundosEnPantalla = Convert.ToInt32(dr["segundosenpantalla"]),
                SegundosEstables = Convert.ToInt32(dr["segundosestables"]),
                Origen = Convert.ToString(dr["origen"]),
                Motivo = Convert.ToString(dr["motivo"]),
                Inicio = Convert.ToDateTime(dr["inicio"]),
                Fin = Convert.ToDateTime(dr["fin"]),
                Creado = Convert.ToDateTime(dr["creado"]),
                Revision = Texto(dr, "revision"),
                RevisadaPor = EnteroNulo(dr, "revisadapor"),
                NombreRevisor = Texto(dr, "nombrerevisor"),
                RevisadaEn = FechaNula(dr, "revisadaen"),
                ComentarioRevision = Texto(dr, "comentariorevision")
            };
        }

        private static Entidades.Notificacion MapearNotificacion(System.Data.IDataRecord dr)
        {
            return new Entidades.Notificacion
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdSucursal = EnteroNulo(dr, "idsucursal"),
                Tipo = Convert.ToString(dr["tipo"]),
                Severidad = Convert.ToString(dr["severidad"]),
                Titulo = Convert.ToString(dr["titulo"]),
                Mensaje = Convert.ToString(dr["mensaje"]),
                RefId = Convert.ToInt64(dr["refid"]),
                Creado = Convert.ToDateTime(dr["creado"]),
                Actualizado = FechaNula(dr, "actualizado"),
                AtendidaPor = EnteroNulo(dr, "atendidapor"),
                NombreAtendidaPor = Texto(dr, "nombreatendidapor"),
                AtendidaEn = FechaNula(dr, "atendidaen")
            };
        }

        // Los timestamp de estas tablas son "sin zona" (hora local del servidor de base): se pasan con
        // Kind=Unspecified para que Npgsql los mande como timestamp y no como timestamptz (UTC).
        private static DateTime SinKind(DateTime valor)
        {
            return DateTime.SpecifyKind(valor, DateTimeKind.Unspecified);
        }

        private static object ONulo(object valor)
        {
            return valor ?? DBNull.Value;
        }

        // ------------------------------------------------------------------------------------
        // Borradores
        // ------------------------------------------------------------------------------------

        public Entidades.ResultadoGuardarBorrador Guardar(Entidades.VentaBorrador borrador)
        {
            if (borrador == null) throw new ArgumentNullException(nameof(borrador));

            // Upsert atomico: el DO UPDATE solo aplica si la fila es ACTIVA y del mismo operador y
            // sucursal; si no aplica no devuelve fila y abajo se averigua el motivo.
            object id = DbPg.Scalar(_connectionString, _idEmpresa, @"
                INSERT INTO ventaborrador
                    (idempresa, idsucursal, idoperador, idusuariosesion, clientid, posinstanceid, idcierrecaja, idpersona,
                     razonsocial, cantlineas, total, payload, estado, creado, ultimolatido)
                VALUES
                    (@idEmpresa, @idSucursal, @idOperador, @idUsuarioSesion, @clientId, @posInstanceId, @idCierreCaja, @idPersona,
                     @razonSocial, @cantLineas, @total, @payload, 'ACTIVA', now(), now())
                ON CONFLICT (idempresa, clientid) DO UPDATE SET
                    posinstanceid = EXCLUDED.posinstanceid,
                    idcierrecaja  = EXCLUDED.idcierrecaja,
                    idpersona     = EXCLUDED.idpersona,
                    razonsocial   = EXCLUDED.razonsocial,
                    cantlineas    = EXCLUDED.cantlineas,
                    total         = EXCLUDED.total,
                    payload       = EXCLUDED.payload,
                    ultimolatido  = now(),
                    actualizado   = now()
                WHERE ventaborrador.estado = 'ACTIVA'
                  AND ventaborrador.idoperador = EXCLUDED.idoperador
                  AND ventaborrador.idsucursal = EXCLUDED.idsucursal
                RETURNING id;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", borrador.IdSucursal);
                    p.AddWithValue("idOperador", borrador.IdOperador);
                    p.AddWithValue("idUsuarioSesion", borrador.IdUsuarioSesion);
                    p.AddWithValue("clientId", borrador.ClientId);
                    p.AddWithValue("posInstanceId", ONulo(borrador.PosInstanceId));
                    p.AddWithValue("idCierreCaja", ONulo(borrador.IdCierreCaja));
                    p.AddWithValue("idPersona", ONulo(borrador.IdPersona));
                    p.AddWithValue("razonSocial", ONulo(borrador.RazonSocial));
                    p.AddWithValue("cantLineas", borrador.CantLineas);
                    p.AddWithValue("total", borrador.Total);
                    p.Add(new NpgsqlParameter("payload", NpgsqlDbType.Jsonb) { Value = ONulo(borrador.Payload) });
                });

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
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE ventaborrador SET ultimolatido = now()
                WHERE idempresa = @idEmpresa AND clientid = @clientId AND idoperador = @idOperador
                  AND idsucursal = @idSucursal AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clientId", clientId);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("idSucursal", idSucursal);
                });
            return filas > 0;
        }

        public Entidades.VentaBorrador ObtenerPorClientId(Guid clientId)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM ventaborrador b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.clientid = @clientId;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clientId", clientId);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.VentaBorrador ObtenerPorId(int id)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM ventaborrador b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.id = @id;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public List<Entidades.VentaBorrador> ListarActivasPorSucursal(int idSucursal)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorConPayload + " FROM ventaborrador b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.idsucursal = @idSucursal AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", idSucursal);
                });
        }

        public List<Entidades.VentaBorrador> ListarActivasPorOperador(int idOperador, int idSucursal)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM ventaborrador b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.idoperador = @idOperador AND b.idsucursal = @idSucursal AND b.estado = 'ACTIVA' ORDER BY b.creado;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("idSucursal", idSucursal);
                });
        }

        public List<Entidades.VentaBorrador> ListarActivasPorUsuarioSesion(int idUsuarioSesion)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM ventaborrador b LEFT JOIN usuarios u ON u.id = b.idoperador " +
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
                UPDATE ventaborrador
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

        public bool MarcarFinalizada(Guid clientId, int idVenta)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE ventaborrador
                SET estado = 'FINALIZADA', idventa = @idVenta, payload = NULL, finalizado = now(), actualizado = now()
                WHERE idempresa = @idEmpresa AND clientid = @clientId AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("clientId", clientId);
                    p.AddWithValue("idVenta", idVenta);
                });
            return filas > 0;
        }

        public bool MarcarDescartada(int id)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE ventaborrador
                SET estado = 'DESCARTADA', payload = NULL, finalizado = now(), actualizado = now()
                WHERE idempresa = @idEmpresa AND id = @id AND estado = 'ACTIVA';",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                });
            return filas > 0;
        }

        public int PurgarFinalizadasAntiguas(int dias)
        {
            return DbPg.NonQuery(_connectionString, _idEmpresa, @"
                DELETE FROM ventaborrador
                WHERE idempresa = @idEmpresa AND estado = 'FINALIZADA' AND finalizado < now() - (@dias * interval '1 day');",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("dias", dias);
                });
        }

        public List<Entidades.VentaBorrador> ListarInterrumpidasPorRango(DateTime desde, DateTime hasta, int minutosSinLatido)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasBorradorSinPayload + " FROM ventaborrador b LEFT JOIN usuarios u ON u.id = b.idoperador " +
                "WHERE b.idempresa = @idEmpresa AND b.estado = 'ACTIVA' " +
                "AND b.ultimolatido BETWEEN @desde AND @hasta " +
                "AND b.ultimolatido < now() - (@minutos * interval '1 minute') " +
                "ORDER BY b.ultimolatido DESC;",
                MapearBorrador,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("desde", SinKind(desde));
                    p.AddWithValue("hasta", SinKind(hasta));
                    p.AddWithValue("minutos", minutosSinLatido);
                });
        }

        // ------------------------------------------------------------------------------------
        // Eventos
        // ------------------------------------------------------------------------------------

        public void AgregarEvento(Entidades.VentaBorradorEvento evento)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO ventaborradorevento (idempresa, idborrador, fecha, tipo, idusuario, detalle)
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

        public List<Entidades.VentaBorradorEvento> ListarEventosPorBorrador(int idBorrador)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasEvento + " FROM ventaborradorevento e LEFT JOIN usuarios u ON u.id = e.idusuario " +
                "LEFT JOIN ventaborrador b ON b.id = e.idborrador AND b.idempresa = e.idempresa LEFT JOIN usuarios ub ON ub.id = b.idoperador " +
                "WHERE e.idempresa = @idEmpresa AND e.idborrador = @idBorrador ORDER BY e.fecha, e.id;",
                MapearEvento,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idBorrador", idBorrador);
                });
        }

        public List<Entidades.VentaBorradorEvento> ListarEventosPorRango(DateTime desde, DateTime hasta)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasEvento + " FROM ventaborradorevento e LEFT JOIN usuarios u ON u.id = e.idusuario " +
                "LEFT JOIN ventaborrador b ON b.id = e.idborrador AND b.idempresa = e.idempresa LEFT JOIN usuarios ub ON ub.id = b.idoperador " +
                "WHERE e.idempresa = @idEmpresa AND e.fecha BETWEEN @desde AND @hasta ORDER BY e.fecha DESC, e.id DESC;",
                MapearEvento,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("desde", SinKind(desde));
                    p.AddWithValue("hasta", SinKind(hasta));
                });
        }

        // ------------------------------------------------------------------------------------
        // Producto sin agregar
        // ------------------------------------------------------------------------------------

        public int AgregarProductoSinAgregar(Entidades.ProductoSinAgregar producto)
        {
            if (producto == null) throw new ArgumentNullException(nameof(producto));

            // fin = ahora - lo que tardo el navegador en avisar; inicio = fin - tiempo en pantalla.
            // Todo con el reloj de la base.
            object id = DbPg.Scalar(_connectionString, _idEmpresa, @"
                INSERT INTO ventaproductosinagregar
                    (idempresa, idsucursal, idoperador, idusuariosesion, clientid, codigo, producto, preciokg, cantidadkg, importe,
                     segundosenpantalla, segundosestables, origen, motivo, inicio, fin, creado)
                VALUES
                    (@idEmpresa, @idSucursal, @idOperador, @idUsuarioSesion, @clientId, @codigo, @producto, @precioKg, @cantidadKg, @importe,
                     @segEnPantalla, @segEstables, @origen, @motivo,
                     now() - (@segDesdeSalida * interval '1 second') - (@segEnPantalla * interval '1 second'),
                     now() - (@segDesdeSalida * interval '1 second'),
                     now())
                RETURNING id;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idSucursal", producto.IdSucursal);
                    p.AddWithValue("idOperador", producto.IdOperador);
                    p.AddWithValue("idUsuarioSesion", producto.IdUsuarioSesion);
                    p.Add(new NpgsqlParameter("clientId", NpgsqlDbType.Uuid) { Value = producto.ClientId.HasValue ? (object)producto.ClientId.Value : DBNull.Value });
                    p.AddWithValue("codigo", producto.Codigo ?? "");
                    p.AddWithValue("producto", producto.Producto ?? "");
                    p.AddWithValue("precioKg", producto.PrecioKg);
                    p.AddWithValue("cantidadKg", producto.CantidadKg);
                    p.AddWithValue("importe", producto.Importe);
                    p.AddWithValue("segEnPantalla", producto.SegundosEnPantalla);
                    p.AddWithValue("segEstables", producto.SegundosEstables);
                    p.AddWithValue("origen", producto.Origen);
                    p.AddWithValue("motivo", producto.Motivo);
                    p.AddWithValue("segDesdeSalida", producto.SegundosDesdeSalida);
                });

            return Convert.ToInt32(id);
        }

        public Entidades.ProductoSinAgregar ObtenerProductoSinAgregar(int id)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasProducto + " FROM ventaproductosinagregar p " +
                "LEFT JOIN usuarios u ON u.id = p.idoperador LEFT JOIN usuarios r ON r.id = p.revisadapor " +
                "WHERE p.idempresa = @idEmpresa AND p.id = @id;",
                MapearProducto,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarPorRango(DateTime desde, DateTime hasta)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasProducto + " FROM ventaproductosinagregar p " +
                "LEFT JOIN usuarios u ON u.id = p.idoperador LEFT JOIN usuarios r ON r.id = p.revisadapor " +
                "WHERE p.idempresa = @idEmpresa AND p.fin BETWEEN @desde AND @hasta ORDER BY p.fin DESC, p.id DESC;",
                MapearProducto,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("desde", SinKind(desde));
                    p.AddWithValue("hasta", SinKind(hasta));
                });
        }

        public List<Entidades.ProductoSinAgregar> ListarProductoSinAgregarDelDia(int idOperador, DateTime dia)
        {
            DateTime desde = dia.Date;
            DateTime hasta = desde.AddDays(1);
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasProducto + " FROM ventaproductosinagregar p " +
                "LEFT JOIN usuarios u ON u.id = p.idoperador LEFT JOIN usuarios r ON r.id = p.revisadapor " +
                "WHERE p.idempresa = @idEmpresa AND p.idoperador = @idOperador AND p.fin >= @desde AND p.fin < @hasta " +
                "ORDER BY p.fin, p.id;",
                MapearProducto,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("desde", SinKind(desde));
                    p.AddWithValue("hasta", SinKind(hasta));
                });
        }

        public Entidades.ResumenProductoSinAgregar ResumirProductoSinAgregar(int idOperador, DateTime desde, DateTime hasta)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT COUNT(*) AS cantidad, COALESCE(SUM(importe), 0) AS importe FROM ventaproductosinagregar " +
                "WHERE idempresa = @idEmpresa AND idoperador = @idOperador AND fin >= @desde AND fin < @hasta;",
                dr => new Entidades.ResumenProductoSinAgregar
                {
                    Cantidad = Convert.ToInt32(dr["cantidad"]),
                    Importe = Convert.ToDecimal(dr["importe"])
                },
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idOperador", idOperador);
                    p.AddWithValue("desde", SinKind(desde));
                    p.AddWithValue("hasta", SinKind(hasta));
                });
            return lista.Count > 0 ? lista[0] : new Entidades.ResumenProductoSinAgregar();
        }

        public bool RevisarProductoSinAgregar(int id, string revision, string comentario, int idUsuario)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE ventaproductosinagregar
                SET revision = @revision, revisadapor = @idUsuario, revisadaen = now(), comentariorevision = @comentario
                WHERE idempresa = @idEmpresa AND id = @id;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                    p.AddWithValue("revision", ONulo(revision));
                    p.AddWithValue("idUsuario", idUsuario);
                    p.AddWithValue("comentario", ONulo(comentario));
                });
            return filas > 0;
        }

        // ------------------------------------------------------------------------------------
        // Notificaciones
        // ------------------------------------------------------------------------------------

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

        public List<Entidades.Notificacion> ListarNotificaciones(bool soloPendientes, int max)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasNotificacion + " FROM notificaciones n LEFT JOIN usuarios ua ON ua.id = n.atendidapor " +
                "WHERE n.idempresa = @idEmpresa" + (soloPendientes ? " AND n.atendidaen IS NULL" : "") +
                " ORDER BY COALESCE(n.actualizado, n.creado) DESC, n.id DESC LIMIT @max;",
                MapearNotificacion,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("max", max);
                });
        }

        public int ContarNotificacionesPendientes()
        {
            object total = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT COUNT(*) FROM notificaciones WHERE idempresa = @idEmpresa AND atendidaen IS NULL;",
                p => p.AddWithValue("idEmpresa", _idEmpresa));
            return total == null || total == DBNull.Value ? 0 : Convert.ToInt32(total);
        }

        public Entidades.Notificacion ObtenerNotificacion(int id)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + ColumnasNotificacion + " FROM notificaciones n LEFT JOIN usuarios ua ON ua.id = n.atendidapor " +
                "WHERE n.idempresa = @idEmpresa AND n.id = @id;",
                MapearNotificacion,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                });
            return lista.Count > 0 ? lista[0] : null;
        }

        public bool AtenderNotificacion(int id, int idUsuario)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE notificaciones SET atendidapor = @idUsuario, atendidaen = now()
                WHERE idempresa = @idEmpresa AND id = @id AND atendidaen IS NULL;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("id", id);
                    p.AddWithValue("idUsuario", idUsuario);
                });
            return filas > 0;
        }

        public int CrearNotificacionesVentasInterrumpidas(int minutosSinLatido)
        {
            // Una sola notificacion por venta en curso (tipo + refid unico): si la venta ya tiene una
            // de cualquier tipo relacionado (descartada, cierre de caja) no se duplica la alerta.
            return DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO notificaciones (idempresa, idsucursal, tipo, severidad, titulo, mensaje, refid, creado, actualizado)
                SELECT b.idempresa, b.idsucursal, 'VENTA_INTERRUMPIDA', 'ADVERTENCIA',
                       'Venta en curso interrumpida',
                       COALESCE(u.nombre, 'Un cajero') || ' dejó una venta sin cerrar de $ ' || to_char(b.total, 'FM999G999G990D00')
                           || ' (' || b.cantlineas || ' ítem(s)), última señal ' || to_char(b.ultimolatido, 'DD/MM/YYYY HH24:MI:SS') || '.',
                       b.id, now(), b.ultimolatido
                FROM ventaborrador b
                LEFT JOIN usuarios u ON u.id = b.idoperador
                WHERE b.idempresa = @idEmpresa
                  AND b.estado = 'ACTIVA'
                  AND b.cantlineas > 0
                  AND b.ultimolatido < now() - (@minutos * interval '1 minute')
                  AND NOT EXISTS (
                        SELECT 1 FROM notificaciones n
                        WHERE n.idempresa = b.idempresa AND n.refid = b.id
                          AND n.tipo IN ('VENTA_INTERRUMPIDA', 'VENTA_DESCARTADA', 'CIERRE_CAJA_CON_VENTA'))
                ON CONFLICT (idempresa, tipo, refid) DO NOTHING;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("minutos", minutosSinLatido);
                });
        }
    }
}
