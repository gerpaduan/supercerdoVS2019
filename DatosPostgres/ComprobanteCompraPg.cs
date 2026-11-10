using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IComprobanteCompraRepository y de IResumenProveedorRepository.
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md). Tablas: comprobantescompra
    // (RLS por idempresa) y las columnas nuevas compras.idtitular / compras.estadofactura / movctacte.idtitular / pagos.idtitular.
    // Los comprobantes son el dato fiscal de la compra: NUNCA tocan movctacte (la deuda es el importe de la compra).
    public class ComprobanteCompraPg : Contratos.IComprobanteCompraRepository, Contratos.IResumenProveedorRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        // Titular efectivo: NULL en la base = titular principal de la empresa (la fila espredeterminado). 0 si no hay ninguno.
        // {0} = alias de la tabla (m, pg, c).
        private const string TitularEfectivo = "COALESCE({0}.idtitular, (SELECT t.idtitular FROM titulares_fiscales t WHERE t.espredeterminado LIMIT 1), 0)";

        public ComprobanteCompraPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // ------------------------------------------------------------------ compra

        public Entidades.Compra ObtenerCompraLiviana(int idCompra)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idcompra, idproveedor, tipocompra, idtitular, estadofactura, fechacompra, estado FROM compras WHERE idcompra = @idCompra;",
                dr => new Entidades.Compra
                {
                    IdCompra = Convert.ToInt32(dr["idcompra"]),
                    Proveedor = new Entidades.Persona { idPersona = Convert.ToInt32(dr["idproveedor"]) },
                    TipoCompra = Convert.ToString(dr["tipocompra"]),
                    IdTitular = dr["idtitular"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idtitular"]),
                    EstadoFactura = dr["estadofactura"] == DBNull.Value ? null : Convert.ToString(dr["estadofactura"]),
                    FechaCompra = Convert.ToDateTime(dr["fechacompra"]),
                    Estado = dr["estado"] == DBNull.Value ? "" : Convert.ToString(dr["estado"])
                },
                p => p.AddWithValue("idCompra", idCompra));
            return lista.Count > 0 ? lista[0] : null;
        }

        public decimal ObtenerTotalCompra(int idCompra, string tipoCompra)
        {
            string sql;
            switch (Entidades.Compra.tipoCompraToEnum(tipoCompra))
            {
                case Entidades.Compra.tipoCompraEnum.Cortes:
                    sql = "SELECT COALESCE(SUM(cantkg::numeric * preciokg::numeric), 0) FROM corteporcompra WHERE idcompra = @idCompra;";
                    break;
                case Entidades.Compra.tipoCompraEnum.MediaRes:
                    sql = "SELECT COALESCE(SUM(kgmedia::numeric * preciomedia::numeric), 0) FROM mediares WHERE idcompra = @idCompra;";
                    break;
                default:
                    return 0m; // las compras de stock no tienen importe ni comprobantes
            }
            object r = DbPg.Scalar(_connectionString, _idEmpresa, sql, p => p.AddWithValue("idCompra", idCompra));
            return r == null || r == DBNull.Value ? 0m : Math.Round(Convert.ToDecimal(r), 2);
        }

        public void SetEstadoFactura(int idCompra, string estado, int idUsuario)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE compras SET estadofactura = @estado, actualizado = now(), actualizadopor = @idUsuario WHERE idcompra = @idCompra;",
                p =>
                {
                    p.AddWithValue("idCompra", idCompra);
                    p.AddWithValue("estado", (object)estado ?? DBNull.Value);
                    p.AddWithValue("idUsuario", idUsuario);
                });
        }

        // El importe no cambia, asi que el titular de los movimientos se actualiza en el lugar (sin contra-asiento): todas las
        // filas de esa compra (incluidas sus anulaciones) pasan al titular nuevo y siguen netas dentro del mismo titular.
        public void SetTitularCompra(int idCompra, int? idTitular, int idUsuario)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE compras SET idtitular = @idTitular, actualizado = now(), actualizadopor = @idUsuario WHERE idcompra = @idCompra;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("idTitular", (object)idTitular ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("idUsuario", idUsuario);
                        cmd.Parameters.AddWithValue("idCompra", idCompra);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new NpgsqlCommand(
                        "UPDATE movctacte SET idtitular = @idTitular WHERE tabla = 'Compras' AND idtabla = @idCompra;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("idTitular", (object)idTitular ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("idCompra", idCompra);
                        cmd.ExecuteNonQuery();
                    }
                    tx?.Commit();
                }
                catch { tx?.Rollback(); throw; }
            }
        }

        // ------------------------------------------------------------------ comprobantes

        private const string SelectComprobante = @"
            SELECT idcomprobante, idcompra, idproveedor, tipocomprobante, ptovta, numero, fecha, netogravado, iva, otrosimpuestos,
                   total, alicuotaiva, creado, creadopor, actualizado, actualizadopor, eliminado
            FROM comprobantescompra ";

        private static Entidades.ComprobanteCompra Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.ComprobanteCompra
            {
                IdComprobante = Convert.ToInt32(dr["idcomprobante"]),
                IdCompra = Convert.ToInt32(dr["idcompra"]),
                IdProveedor = Convert.ToInt32(dr["idproveedor"]),
                TipoComprobante = Convert.ToString(dr["tipocomprobante"]),
                PtoVta = Convert.ToInt32(dr["ptovta"]),
                Numero = Convert.ToInt64(dr["numero"]),
                Fecha = Convert.ToDateTime(dr["fecha"]),
                NetoGravado = Convert.ToDecimal(dr["netogravado"]),
                Iva = Convert.ToDecimal(dr["iva"]),
                OtrosImpuestos = Convert.ToDecimal(dr["otrosimpuestos"]),
                Total = Convert.ToDecimal(dr["total"]),
                AlicuotaIva = dr["alicuotaiva"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["alicuotaiva"]),
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["creadopor"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                ActualizadoPor = dr["actualizadopor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["actualizadopor"]),
                Eliminado = Convert.ToBoolean(dr["eliminado"])
            };
        }

        public List<Entidades.ComprobanteCompra> ListarPorCompra(int idCompra)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectComprobante + "WHERE idcompra = @idCompra AND NOT eliminado ORDER BY fecha, idcomprobante;",
                Mapear, p => p.AddWithValue("idCompra", idCompra));
        }

        public Entidades.ComprobanteCompra Obtener(int idComprobante)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectComprobante + "WHERE idcomprobante = @id;", Mapear, p => p.AddWithValue("id", idComprobante));
            return lista.Count > 0 ? lista[0] : null;
        }

        public int Guardar(Entidades.ComprobanteCompra c, int idUsuario)
        {
            if (c == null) throw new ArgumentNullException(nameof(c));

            Action<NpgsqlParameterCollection> setParams = p =>
            {
                p.AddWithValue("idCompra", c.IdCompra);
                p.AddWithValue("idProveedor", c.IdProveedor);
                p.AddWithValue("tipo", c.TipoComprobante ?? "");
                p.AddWithValue("ptoVta", c.PtoVta);
                p.AddWithValue("numero", c.Numero);
                p.AddWithValue("fecha", c.Fecha.Date);
                p.AddWithValue("neto", c.NetoGravado);
                p.AddWithValue("iva", c.Iva);
                p.AddWithValue("otros", c.OtrosImpuestos);
                p.AddWithValue("total", c.Total);
                p.AddWithValue("alicuota", (object)c.AlicuotaIva ?? DBNull.Value);
                p.AddWithValue("idUsuario", idUsuario);
            };

            if (c.IdComprobante == 0)
            {
                object nuevo = DbPg.Scalar(_connectionString, _idEmpresa, @"
                    INSERT INTO comprobantescompra (idempresa, idcompra, idproveedor, tipocomprobante, ptovta, numero, fecha,
                        netogravado, iva, otrosimpuestos, total, alicuotaiva, creado, creadopor)
                    VALUES (@idEmpresa, @idCompra, @idProveedor, @tipo, @ptoVta, @numero, @fecha,
                        @neto, @iva, @otros, @total, @alicuota, now(), @idUsuario)
                    RETURNING idcomprobante;", p =>
                {
                    setParams(p);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
                c.IdComprobante = Convert.ToInt32(nuevo);
                return c.IdComprobante;
            }

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE comprobantescompra SET idproveedor = @idProveedor, tipocomprobante = @tipo, ptovta = @ptoVta, numero = @numero,
                    fecha = @fecha, netogravado = @neto, iva = @iva, otrosimpuestos = @otros, total = @total, alicuotaiva = @alicuota,
                    actualizado = now(), actualizadopor = @idUsuario
                WHERE idcomprobante = @id AND idcompra = @idCompra AND NOT eliminado;", p =>
            {
                setParams(p);
                p.AddWithValue("id", c.IdComprobante);
            });
            return c.IdComprobante;
        }

        public void Eliminar(int idComprobante, int idUsuario, string motivo)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE comprobantescompra SET eliminado = true, eliminadopor = @idUsuario, eliminadofecha = now(), motivoeliminacion = @motivo
                WHERE idcomprobante = @id AND NOT eliminado;", p =>
            {
                p.AddWithValue("id", idComprobante);
                p.AddWithValue("idUsuario", idUsuario);
                p.AddWithValue("motivo", motivo ?? "");
            });
        }

        public bool ExisteNumero(int idProveedor, string tipo, int ptoVta, long numero, int idComprobanteExcluir)
        {
            object r = DbPg.Scalar(_connectionString, _idEmpresa, @"
                SELECT 1 FROM comprobantescompra
                WHERE idproveedor = @idProveedor AND tipocomprobante = @tipo AND ptovta = @ptoVta AND numero = @numero
                  AND NOT eliminado AND idcomprobante <> @excluir LIMIT 1;", p =>
            {
                p.AddWithValue("idProveedor", idProveedor);
                p.AddWithValue("tipo", tipo ?? "");
                p.AddWithValue("ptoVta", ptoVta);
                p.AddWithValue("numero", numero);
                p.AddWithValue("excluir", idComprobanteExcluir);
            });
            return r != null && r != DBNull.Value;
        }

        // Estado, titular y totales de varias compras en una sola consulta (bandeja de Compras/Index).
        public Dictionary<int, Entidades.InfoFacturaCompra> ObtenerInfoPorCompras(IEnumerable<int> idsCompra)
        {
            var resultado = new Dictionary<int, Entidades.InfoFacturaCompra>();
            int[] ids = (idsCompra ?? Enumerable.Empty<int>()).Distinct().ToArray();
            if (ids.Length == 0) return resultado;

            var compras = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idcompra, idtitular, estadofactura FROM compras WHERE idcompra = ANY(@ids);",
                dr => new Entidades.InfoFacturaCompra
                {
                    IdCompra = Convert.ToInt32(dr["idcompra"]),
                    IdTitular = dr["idtitular"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idtitular"]),
                    EstadoFactura = dr["estadofactura"] == DBNull.Value ? null : Convert.ToString(dr["estadofactura"])
                },
                p => p.AddWithValue("ids", ids));
            foreach (var info in compras) resultado[info.IdCompra] = info;

            var comprobantes = DbPg.Reader(_connectionString, _idEmpresa,
                SelectComprobante + "WHERE idcompra = ANY(@ids) AND NOT eliminado ORDER BY fecha, idcomprobante;",
                Mapear, p => p.AddWithValue("ids", ids));
            foreach (var c in comprobantes)
            {
                Entidades.InfoFacturaCompra info;
                if (!resultado.TryGetValue(c.IdCompra, out info)) continue;
                info.FacturadoNeto += c.TotalConSigno;
                if (Entidades.TiposComprobanteCompra.EsFactura(c.TipoComprobante))
                {
                    info.TipoFactura = c.TipoComprobante;
                    info.NumeroFactura = c.NumeroCompleto;
                }
                if (Entidades.TiposComprobanteCompra.EsNotaCredito(c.TipoComprobante)) info.TieneNotaCredito = true;
                if (Entidades.TiposComprobanteCompra.EsNotaDebito(c.TipoComprobante)) info.TieneNotaDebito = true;
            }
            return resultado;
        }

        // ------------------------------------------------------------------ resumen por titular (IResumenProveedorRepository)

        public List<Entidades.MovimientoResumen> ObtenerMovimientos(int idPersona)
        {
            // Sin filtrar quitadoctacte: igual que obtenerSaldoPersona y el extracto (CuentaCorrientePg), para que el total del resumen
            // coincida con el saldo que muestra la pantalla (hallazgo del arnes de 2026-11-09: filtrarlo daba 31.624 de diferencia).
            string sql = "SELECT m.fecha, m.tabla, m.importe, " + string.Format(TitularEfectivo, "m") + " AS idtitular " +
                         "FROM movctacte m WHERE m.idpersona = @idPersona;";
            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => new Entidades.MovimientoResumen
                {
                    Fecha = Convert.ToDateTime(dr["fecha"]),
                    Tabla = Convert.ToString(dr["tabla"]),
                    Importe = Convert.ToDecimal(dr["importe"]),
                    IdTitular = Convert.ToInt32(dr["idtitular"])
                },
                p => p.AddWithValue("idPersona", idPersona));
        }

        // Mismo extracto que CuentaCorrientePg.getCtaCteByIdPersona (limite estricto de fecha, fila "Saldo Anterior" con id 0) pero con el
        // titular de cada movimiento y con filtro opcional por titular. Si se cambia el original, cambiar tambien este.
        public DataTable ObtenerExtractoPorTitular(int idPersona, DateTime desde, int idTitularFiltro)
        {
            string titularMovimiento = string.Format(TitularEfectivo, "m");
            string sql = @"
                SELECT * FROM (
                    SELECT
                        p.idpersona AS idpersona, p.razonsocial AS razonsocial,
                        0 AS id, @fechaDesde AS fecha, '-' AS tabla, 0 AS idtabla, '-' AS nrodoc,
                        'Saldo Anterior' AS detalle, '-' AS tipo, sat.saldoanterior AS importe,
                        0.00 AS saldo, '-' AS sucursal,
                        @fechaDesde AS creado, '-' AS creadopor,
                        @fechaDesde AS actualizado, '-' AS actualizadopor,
                        @idTitularFiltro AS idtitular
                    FROM (
                        SELECT m.idpersona, SUM(m.importe) AS saldoanterior
                        FROM movctacte m
                        WHERE m.idpersona = @idPersona AND m.fecha < @fechaDesde
                          AND (@idTitularFiltro = 0 OR " + titularMovimiento + @" = @idTitularFiltro)
                        GROUP BY m.idpersona
                    ) sat
                    INNER JOIN personas p ON sat.idpersona = p.idpersona

                    UNION

                    SELECT
                        p.idpersona, p.razonsocial,
                        m.id, m.fecha, m.tabla, m.idtabla, m.nrodoc,
                        m.detalle, m.tipo, m.importe,
                        0.00 AS saldo, s.sucursal,
                        m.creado, creadopor.nombre AS creadopor,
                        m.actualizado, actualizadopor.nombre AS actualizadopor,
                        " + titularMovimiento + @" AS idtitular
                    FROM movctacte m
                    INNER JOIN personas p ON m.idpersona = p.idpersona
                    LEFT JOIN sucursal s ON m.idsucursal = s.idsucursal
                    LEFT JOIN usuarios creadopor ON m.creadopor = creadopor.id
                    LEFT JOIN usuarios actualizadopor ON m.actualizadopor = actualizadopor.id
                    WHERE p.idpersona = @idPersona AND m.fecha > @fechaDesde
                      AND (@idTitularFiltro = 0 OR " + titularMovimiento + @" = @idTitularFiltro)
                ) t
                ORDER BY fecha;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idPersona", idPersona);
                p.AddWithValue("fechaDesde", desde);
                p.AddWithValue("idTitularFiltro", idTitularFiltro);
            });
        }

        public List<Entidades.PagoResumen> ObtenerPagos(int idPersona, DateTime desde)
        {
            string sql = "SELECT pg.fecha, pg.formapago, pg.importe, pg.efectivo, " + string.Format(TitularEfectivo, "pg") + " AS idtitular " +
                         "FROM pagos pg WHERE pg.idpersona = @idPersona AND pg.aproveedor = true AND COALESCE(pg.eliminado, false) = false " +
                         "AND pg.fecha >= @desde;";
            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => new Entidades.PagoResumen
                {
                    Fecha = Convert.ToDateTime(dr["fecha"]),
                    FormaPago = dr["formapago"] == DBNull.Value ? "" : Convert.ToString(dr["formapago"]),
                    Importe = Convert.ToDecimal(dr["importe"]),
                    Efectivo = dr["efectivo"] == DBNull.Value ? 0m : Convert.ToDecimal(dr["efectivo"]),
                    IdTitular = Convert.ToInt32(dr["idtitular"])
                },
                p =>
                {
                    p.AddWithValue("idPersona", idPersona);
                    p.AddWithValue("desde", desde);
                });
        }

        public List<Entidades.FacturadoResumen> ObtenerFacturado(int idPersona, DateTime desde)
        {
            string sql = @"SELECT c.fechacompra, cc.tipocomprobante, cc.total, " + string.Format(TitularEfectivo, "c") + @" AS idtitular
                           FROM comprobantescompra cc
                           JOIN compras c ON c.idcompra = cc.idcompra
                           WHERE c.idproveedor = @idPersona AND NOT cc.eliminado AND c.estado <> 'Anulado' AND c.fechacompra >= @desde;";
            return DbPg.Reader(_connectionString, _idEmpresa, sql,
                dr => new Entidades.FacturadoResumen
                {
                    FechaCompra = Convert.ToDateTime(dr["fechacompra"]),
                    TotalConSigno = Entidades.TiposComprobanteCompra.Signo(Convert.ToString(dr["tipocomprobante"])) * Convert.ToDecimal(dr["total"]),
                    IdTitular = Convert.ToInt32(dr["idtitular"])
                },
                p =>
                {
                    p.AddWithValue("idPersona", idPersona);
                    p.AddWithValue("desde", desde);
                });
        }
    }
}
