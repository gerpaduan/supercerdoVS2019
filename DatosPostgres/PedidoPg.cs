using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IPedidoRepository (tablas pedido, pedidolinea y pedidoevento, migracion
    // 20261019-Create_pedidos.sql). La RLS por empresa se fija en cada conexion (DbPg / UnitOfWorkPg); ademas cada consulta
    // filtra por idempresa de forma explicita (defensa en profundidad, igual que LiquidacionSueldoPg).
    public class PedidoPg : Contratos.IPedidoRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public PedidoPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public Contratos.IUnitOfWork IniciarUnitOfWork()
        {
            return UnitOfWorkPg.Iniciar(_connectionString, _idEmpresa);
        }

        // ---------- Escritura (siempre dentro de la transaccion del unitOfWork) ----------

        public int Agregar(Entidades.Pedido pedido, Contratos.IUnitOfWork unitOfWork)
        {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));

            const string sqlCabecera = @"
                INSERT INTO pedido
                    (idempresa, clientid, fecha, idusuario, usuario, nota, totalestimado, lineassinprecio, creado)
                VALUES
                    (@idEmpresa, @clientId, @fecha, @idUsuario, @usuario, @nota, @total, @sinPrecio, now())
                RETURNING idpedido;";

            object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlCabecera, p =>
            {
                p.AddWithValue("idEmpresa", _idEmpresa);
                p.Add(new NpgsqlParameter("clientId", NpgsqlDbType.Uuid) { Value = pedido.ClientId.HasValue ? (object)pedido.ClientId.Value : DBNull.Value });
                p.AddWithValue("fecha", pedido.Fecha);
                p.AddWithValue("idUsuario", pedido.IdUsuario);
                p.AddWithValue("usuario", pedido.Usuario ?? "");
                p.Add(new NpgsqlParameter("nota", NpgsqlDbType.Text) { Value = (object)pedido.Nota ?? DBNull.Value });
                p.AddWithValue("total", pedido.TotalEstimado);
                p.AddWithValue("sinPrecio", pedido.LineasSinPrecio);
            }, unitOfWork);

            pedido.Id = Convert.ToInt32(nuevoId);
            InsertarLineas(pedido, unitOfWork);
            return pedido.Id;
        }

        public void Actualizar(Entidades.Pedido pedido, Contratos.IUnitOfWork unitOfWork)
        {
            if (pedido == null) throw new ArgumentNullException(nameof(pedido));

            int filas = DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE pedido
                SET nota = @nota, totalestimado = @total, lineassinprecio = @sinPrecio, modificado = now(), modificadopor = @modificadoPor
                WHERE idpedido = @id AND idempresa = @idEmpresa;", p =>
            {
                p.Add(new NpgsqlParameter("nota", NpgsqlDbType.Text) { Value = (object)pedido.Nota ?? DBNull.Value });
                p.AddWithValue("total", pedido.TotalEstimado);
                p.AddWithValue("sinPrecio", pedido.LineasSinPrecio);
                p.Add(new NpgsqlParameter("modificadoPor", NpgsqlDbType.Integer) { Value = pedido.ModificadoPor.HasValue ? (object)pedido.ModificadoPor.Value : DBNull.Value });
                p.AddWithValue("id", pedido.Id);
                p.AddWithValue("idEmpresa", _idEmpresa);
            }, unitOfWork);

            if (filas != 1) throw new InvalidOperationException("No se encontró el pedido a modificar.");

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM pedidolinea WHERE idpedido = @id AND idempresa = @idEmpresa;",
                p => { p.AddWithValue("id", pedido.Id); p.AddWithValue("idEmpresa", _idEmpresa); }, unitOfWork);

            InsertarLineas(pedido, unitOfWork);
        }

        private void InsertarLineas(Entidades.Pedido pedido, Contratos.IUnitOfWork unitOfWork)
        {
            const string sqlLinea = @"
                INSERT INTO pedidolinea
                    (idempresa, idpedido, idcorte, idsucursal, codigo, producto, sucursal, cantidad, pesable, promedio,
                     stockactual, puntostock, preciounitario, proveedor, fechaprecio, importe)
                VALUES
                    (@idEmpresa, @idPedido, @idCorte, @idSucursal, @codigo, @producto, @sucursal, @cantidad, @pesable, @promedio,
                     @stock, @punto, @precio, @proveedor, @fechaPrecio, @importe);";

            foreach (var linea in pedido.Lineas ?? new List<Entidades.PedidoLinea>())
            {
                var l = linea;
                DbPg.NonQuery(_connectionString, _idEmpresa, sqlLinea, p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idPedido", pedido.Id);
                    p.AddWithValue("idCorte", l.IdCorte);
                    p.AddWithValue("idSucursal", l.IdSucursal);
                    p.AddWithValue("codigo", l.Codigo);
                    p.AddWithValue("producto", l.Producto ?? "");
                    p.AddWithValue("sucursal", l.Sucursal ?? "");
                    p.AddWithValue("cantidad", l.Cantidad);
                    p.AddWithValue("pesable", l.Pesable);
                    p.Add(Decimal("promedio", l.Promedio));
                    p.Add(Decimal("stock", l.StockActual));
                    p.Add(Decimal("punto", l.PuntoStock));
                    p.Add(Decimal("precio", l.PrecioUnitario));
                    p.Add(new NpgsqlParameter("proveedor", NpgsqlDbType.Text) { Value = (object)l.Proveedor ?? DBNull.Value });
                    p.Add(new NpgsqlParameter("fechaPrecio", NpgsqlDbType.Timestamp) { Value = l.FechaPrecio.HasValue ? (object)l.FechaPrecio.Value : DBNull.Value });
                    p.Add(Decimal("importe", l.Importe));
                }, unitOfWork);
            }
        }

        // Parametro numerico que puede ser null (con tipo explicito: Postgres no infiere el tipo de un NULL suelto).
        private static NpgsqlParameter Decimal(string nombre, decimal? valor)
        {
            return new NpgsqlParameter(nombre, NpgsqlDbType.Numeric) { Value = valor.HasValue ? (object)valor.Value : DBNull.Value };
        }

        public void AgregarEvento(Entidades.PedidoEvento evento, Contratos.IUnitOfWork unitOfWork)
        {
            if (evento == null) throw new ArgumentNullException(nameof(evento));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO pedidoevento (idempresa, idpedido, fecha, tipo, idusuario, usuario, idautorizador, autorizador, detalle)
                VALUES (@idEmpresa, @idPedido, @fecha, @tipo, @idUsuario, @usuario, @idAutorizador, @autorizador, @detalle);", p =>
            {
                p.AddWithValue("idEmpresa", _idEmpresa);
                p.AddWithValue("idPedido", evento.IdPedido);
                p.AddWithValue("fecha", evento.Fecha);
                p.AddWithValue("tipo", evento.Tipo ?? "");
                p.AddWithValue("idUsuario", evento.IdUsuario);
                p.AddWithValue("usuario", evento.Usuario ?? "");
                p.Add(new NpgsqlParameter("idAutorizador", NpgsqlDbType.Integer) { Value = evento.IdAutorizador.HasValue ? (object)evento.IdAutorizador.Value : DBNull.Value });
                p.Add(new NpgsqlParameter("autorizador", NpgsqlDbType.Text) { Value = (object)evento.Autorizador ?? DBNull.Value });
                p.Add(new NpgsqlParameter("detalle", NpgsqlDbType.Text) { Value = (object)evento.Detalle ?? DBNull.Value });
            }, unitOfWork);
        }

        // ---------- Lectura ----------

        private const string SelectCabecera = @"
            SELECT idpedido, idempresa, clientid, fecha, idusuario, usuario, nota, totalestimado, lineassinprecio,
                   creado, modificado, modificadopor
            FROM pedido ";

        private static Entidades.Pedido MapearCabecera(NpgsqlDataReader dr)
        {
            return new Entidades.Pedido
            {
                Id = Convert.ToInt32(dr["idpedido"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                ClientId = dr["clientid"] == DBNull.Value ? (Guid?)null : (Guid)dr["clientid"],
                Fecha = Convert.ToDateTime(dr["fecha"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                Usuario = Convert.ToString(dr["usuario"]),
                Nota = dr["nota"] == DBNull.Value ? "" : Convert.ToString(dr["nota"]),
                TotalEstimado = Convert.ToDecimal(dr["totalestimado"]),
                LineasSinPrecio = Convert.ToInt32(dr["lineassinprecio"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                Modificado = dr["modificado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["modificado"]),
                ModificadoPor = dr["modificadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["modificadopor"])
            };
        }

        private static decimal? DecimalONull(NpgsqlDataReader dr, string columna)
        {
            return dr[columna] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr[columna]);
        }

        private static Entidades.PedidoLinea MapearLinea(NpgsqlDataReader dr)
        {
            return new Entidades.PedidoLinea
            {
                Id = Convert.ToInt32(dr["idpedidolinea"]),
                IdCorte = Convert.ToInt32(dr["idcorte"]),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                Codigo = Convert.ToInt64(dr["codigo"]),
                Producto = Convert.ToString(dr["producto"]),
                Sucursal = Convert.ToString(dr["sucursal"]),
                Cantidad = Convert.ToDecimal(dr["cantidad"]),
                Pesable = Convert.ToBoolean(dr["pesable"]),
                Promedio = DecimalONull(dr, "promedio"),
                StockActual = DecimalONull(dr, "stockactual"),
                PuntoStock = DecimalONull(dr, "puntostock"),
                PrecioUnitario = DecimalONull(dr, "preciounitario"),
                Proveedor = dr["proveedor"] == DBNull.Value ? null : Convert.ToString(dr["proveedor"]),
                FechaPrecio = dr["fechaprecio"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaprecio"]),
                Importe = DecimalONull(dr, "importe")
            };
        }

        private static Entidades.PedidoEvento MapearEvento(NpgsqlDataReader dr)
        {
            return new Entidades.PedidoEvento
            {
                Id = Convert.ToInt32(dr["idpedidoevento"]),
                IdPedido = Convert.ToInt32(dr["idpedido"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                Tipo = Convert.ToString(dr["tipo"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                Usuario = Convert.ToString(dr["usuario"]),
                IdAutorizador = dr["idautorizador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idautorizador"]),
                Autorizador = dr["autorizador"] == DBNull.Value ? null : Convert.ToString(dr["autorizador"]),
                Detalle = dr["detalle"] == DBNull.Value ? "" : Convert.ToString(dr["detalle"])
            };
        }

        public Entidades.Pedido ObtenerPorId(int idPedido)
        {
            var cabeceras = DbPg.Reader(_connectionString, _idEmpresa,
                SelectCabecera + "WHERE idpedido = @id AND idempresa = @idEmpresa;",
                MapearCabecera,
                p => { p.AddWithValue("id", idPedido); p.AddWithValue("idEmpresa", _idEmpresa); });

            if (cabeceras.Count == 0) return null;
            var pedido = cabeceras[0];

            pedido.Lineas = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT idpedidolinea, idcorte, idsucursal, codigo, producto, sucursal, cantidad, pesable, promedio,
                       stockactual, puntostock, preciounitario, proveedor, fechaprecio, importe
                FROM pedidolinea
                WHERE idpedido = @id AND idempresa = @idEmpresa
                ORDER BY codigo, producto, idsucursal;",
                MapearLinea,
                p => { p.AddWithValue("id", idPedido); p.AddWithValue("idEmpresa", _idEmpresa); });

            pedido.Eventos = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT idpedidoevento, idpedido, fecha, tipo, idusuario, usuario, idautorizador, autorizador, detalle
                FROM pedidoevento
                WHERE idpedido = @id AND idempresa = @idEmpresa
                ORDER BY fecha, idpedidoevento;",
                MapearEvento,
                p => { p.AddWithValue("id", idPedido); p.AddWithValue("idEmpresa", _idEmpresa); });

            return pedido;
        }

        public int? ObtenerIdPorClientId(Guid clientId)
        {
            object id = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idpedido FROM pedido WHERE idempresa = @idEmpresa AND clientid = @clientId LIMIT 1;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.Add(new NpgsqlParameter("clientId", NpgsqlDbType.Uuid) { Value = clientId });
                });
            return id == null || id == DBNull.Value ? (int?)null : Convert.ToInt32(id);
        }

        // Escapa % _ \ para usarlos dentro de un ILIKE ... ESCAPE '\' (el texto del usuario nunca es un patron).
        private static string PatronLike(string texto)
        {
            string limpio = (texto ?? "").Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
            return "%" + limpio + "%";
        }

        public List<Entidades.PedidoResumen> Listar(Entidades.PedidoFiltro filtro)
        {
            if (filtro == null) filtro = new Entidades.PedidoFiltro();
            int maximo = filtro.Maximo < 1 ? 500 : Math.Min(filtro.Maximo, 2000);
            string texto = (filtro.Texto ?? "").Trim();
            string usuario = (filtro.Usuario ?? "").Trim();

            const string sql = @"
                SELECT p.idpedido, p.fecha, p.usuario, p.nota, p.totalestimado, p.lineassinprecio, p.modificado,
                       COALESCE((SELECT string_agg(s.sucursal, ', ' ORDER BY s.sucursal)
                                 FROM (SELECT DISTINCT l.sucursal FROM pedidolinea l WHERE l.idpedido = p.idpedido) s), '') AS sucursales,
                       (SELECT COUNT(DISTINCT l.idcorte) FROM pedidolinea l WHERE l.idpedido = p.idpedido) AS productos,
                       (SELECT COUNT(*) FROM pedidolinea l WHERE l.idpedido = p.idpedido) AS lineas,
                       COALESCE((SELECT SUM(l.cantidad) FROM pedidolinea l WHERE l.idpedido = p.idpedido), 0) AS unidades
                FROM pedido p
                WHERE p.idempresa = @idEmpresa
                  AND (@desde::timestamp IS NULL OR p.fecha >= @desde::timestamp)
                  AND (@hastaExclusivo::timestamp IS NULL OR p.fecha < @hastaExclusivo::timestamp)
                  AND (@idSucursal = 0 OR EXISTS (SELECT 1 FROM pedidolinea l WHERE l.idpedido = p.idpedido AND l.idsucursal = @idSucursal))
                  AND (@usuario = '' OR p.usuario ILIKE @usuarioLike ESCAPE '\')
                  AND (@texto = '' OR p.nota ILIKE @textoLike ESCAPE '\' OR p.usuario ILIKE @textoLike ESCAPE '\'
                       OR EXISTS (SELECT 1 FROM pedidolinea l WHERE l.idpedido = p.idpedido
                                  AND (l.producto ILIKE @textoLike ESCAPE '\' OR l.codigo::text = @texto)))
                ORDER BY p.fecha DESC, p.idpedido DESC
                LIMIT @maximo;";

            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => new Entidades.PedidoResumen
                {
                    Id = Convert.ToInt32(dr["idpedido"]),
                    Fecha = Convert.ToDateTime(dr["fecha"]),
                    Usuario = Convert.ToString(dr["usuario"]),
                    Nota = dr["nota"] == DBNull.Value ? "" : Convert.ToString(dr["nota"]),
                    Sucursales = Convert.ToString(dr["sucursales"]),
                    Productos = Convert.ToInt32(dr["productos"]),
                    Lineas = Convert.ToInt32(dr["lineas"]),
                    Unidades = Convert.ToDecimal(dr["unidades"]),
                    TotalEstimado = Convert.ToDecimal(dr["totalestimado"]),
                    LineasSinPrecio = Convert.ToInt32(dr["lineassinprecio"]),
                    Modificado = dr["modificado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["modificado"])
                },
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.Add(new NpgsqlParameter("desde", NpgsqlDbType.Timestamp) { Value = filtro.Desde.HasValue ? (object)filtro.Desde.Value.Date : DBNull.Value });
                    // "Hasta" incluye todo ese dia: se compara contra el inicio del dia siguiente.
                    p.Add(new NpgsqlParameter("hastaExclusivo", NpgsqlDbType.Timestamp) { Value = filtro.Hasta.HasValue ? (object)filtro.Hasta.Value.Date.AddDays(1) : DBNull.Value });
                    p.AddWithValue("idSucursal", filtro.IdSucursal);
                    p.AddWithValue("usuario", usuario);
                    p.AddWithValue("usuarioLike", PatronLike(usuario));
                    p.AddWithValue("texto", texto);
                    p.AddWithValue("textoLike", PatronLike(texto));
                    p.AddWithValue("maximo", maximo);
                });
        }
    }
}
