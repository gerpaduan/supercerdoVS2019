using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class Venta
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public Venta(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        #region Helpers

        private static bool ColumnaExiste(SqlDataReader dr, string columna)
        {
            try { return dr.GetOrdinal(columna) >= 0; }
            catch { return false; }
        }

        private Entidades.Venta MapVenta(SqlDataReader drVenta, bool cargarLineas = true)
        {
            var oVentaE = new Entidades.Venta
            {
                IdVenta = Convert.ToInt32(drVenta["idVenta"]),
                FechaVenta = Convert.ToDateTime(drVenta["fechaVenta"]),
                Turno = Convert.ToString(drVenta["turno"]),
                DiaFestivo = Convert.ToString(drVenta["diaFestivo"]),
                Observaciones = Convert.ToString(drVenta["observaciones"]),
                NroRemito = Convert.ToString(drVenta["nroRemito"]),
                Estado = Convert.ToString(drVenta["estado"]),
                EnCtaCte = drVenta["enCtaCte"] != DBNull.Value && Convert.ToBoolean(drVenta["enCtaCte"]),
                Cuit = ColumnaExiste(drVenta, "cuit") && drVenta["cuit"] != DBNull.Value ? Convert.ToString(drVenta["cuit"]) : "",
                Email = ColumnaExiste(drVenta, "email") && drVenta["email"] != DBNull.Value ? Convert.ToString(drVenta["email"]) : "",
                FormaPago = Convert.ToString(drVenta["formaPago"]),
                TipoComprobante = drVenta["tipoComprobante"] == DBNull.Value ? 'X' : Convert.ToChar(drVenta["tipoComprobante"]),
                Creado = drVenta["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(drVenta["creado"]),
                Actualizado = drVenta["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(drVenta["actualizado"]),
                PagoMixtoEfectivo = drVenta["pagoMixtoEfectivo"] == DBNull.Value ? 0f : Convert.ToSingle(drVenta["pagoMixtoEfectivo"]),
                IdVendedor = drVenta["idVendedor"] == DBNull.Value ? 0 : Convert.ToInt32(drVenta["idVendedor"]),
                IdSucursal = drVenta["idSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(drVenta["idSucursal"]),
                IdPersona = drVenta["idPersona"] == DBNull.Value ? 0 : Convert.ToInt32(drVenta["idPersona"])
            };

            // Relacionados (carga individual: ojo N+1 si listás muchas ventas)
            var oUsuarioD = new Usuario(_empresa);
            oVentaE.Vendedor = oUsuarioD.getUsuarioById(oVentaE.IdVendedor);

            var oSucursalD = new Sucursal(_empresa);
            oVentaE.Sucursal = oSucursalD.findById(oVentaE.IdSucursal);

            var oPersonaD = new Datos.Persona(_empresa);
            oVentaE.Persona = oPersonaD.findById(oVentaE.IdPersona);

            if (cargarLineas)
            {
                oVentaE.LineasVenta = obtenerLineasVenta(oVentaE.IdVenta);
                oVentaE.CantItems = oVentaE.getCantItems(oVentaE).ToString();
            }

            oVentaE.TotalImporte = getTotalVenta(oVentaE.IdVenta);
            oVentaE.TotalImporteOriginal = oVentaE.TotalImporte;

            return oVentaE;
        }

        #endregion

        #region Ventas

        public Entidades.Venta getVentaById(int idVenta)
        {
            const string sql = "SELECT * FROM Ventas WHERE idVenta = @idVenta";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                        return MapVenta(dr, true);
                }
            }

            return null;
        }

        /// <summary>
        /// Lista ventas con filtros. Si soloAnulados = true => filtra solo estado = 'ANULADO'.
        /// Si soloAnulados = false => NO filtra por estado (incluye todas). Si vos querés excluir anulados, avisame y lo ajusto.
        /// </summary>
        public List<Entidades.Venta> getAllVentas(
            DateTime fechaDesde,
            DateTime fechaHasta,
            string texto,
            int? idVendedor,
            int? idCliente,
            int? idSucursal,
            bool soloAnulados,
            bool cargarLineas)
        {
            var lista = new List<Entidades.Venta>();

            // Armamos WHERE dinámico pero parametrizado
            string sql = @"
                SELECT *
                FROM Ventas
                WHERE fechaVenta >= @fechaDesde
                  AND fechaVenta <  @fechaHastaMas1
                  AND (@idVendedor = -1 OR idVendedor = @idVendedor)
                  AND (@idCliente  = -1 OR idPersona  = @idCliente)
                  AND (@idSucursal = -1 OR idSucursal = @idSucursal)
                  AND (@soloAnulados = 0 OR estado = 'ANULADO')
                  AND (
                        @texto = '' OR
                        nroRemito LIKE '%' + @texto + '%' OR
                        observaciones LIKE '%' + @texto + '%'
                      )
                ORDER BY fechaVenta DESC;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHastaMas1", fechaHasta.AddDays(1));
                cmd.Parameters.AddWithValue("@texto", (texto ?? "").Trim());
                cmd.Parameters.AddWithValue("@idVendedor", idVendedor ?? -1);
                cmd.Parameters.AddWithValue("@idCliente", idCliente ?? -1);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal ?? -1);
                cmd.Parameters.AddWithValue("@soloAnulados", soloAnulados ? 1 : 0);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        lista.Add(MapVenta(dr, cargarLineas));
                }
            }

            return lista;
        }

        public int agregarVenta(Entidades.Venta oVentaE)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarVenta", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
                cmd.Parameters.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
                cmd.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
                cmd.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
                cmd.Parameters.AddWithValue("@turno", oVentaE.Turno ?? "");
                cmd.Parameters.AddWithValue("@diaFestivo", oVentaE.DiaFestivo ?? "");
                cmd.Parameters.AddWithValue("@observaciones", oVentaE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
                cmd.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito ?? "");
                cmd.Parameters.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
                cmd.Parameters.AddWithValue("@formaPago", oVentaE.FormaPago ?? "");
                cmd.Parameters.AddWithValue("@cuit", oVentaE.Cuit ?? "");
                cmd.Parameters.AddWithValue("@email", oVentaE.Email ?? "");
                cmd.Parameters.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
                cmd.Parameters.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
                cmd.Parameters.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
                cmd.Parameters.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);
                cmd.Parameters.AddWithValue("@pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);

                if (con.State != ConnectionState.Open) con.Open();

                // Asumo que tu SP devuelve el idVenta como scalar (si devuelve reader, también funciona si hiciste SELECT idVenta)
                object scalar = cmd.ExecuteScalar();
                int idVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
                return idVenta;
            }
        }

        public void modificarVenta(Entidades.Venta oVentaE, int sucAnterior, bool eliminarLineas)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("modificarVenta", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
                cmd.Parameters.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
                cmd.Parameters.AddWithValue("@idSucursal", sucAnterior);
                cmd.Parameters.AddWithValue("@idSucNueva", oVentaE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
                cmd.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
                cmd.Parameters.AddWithValue("@turno", oVentaE.Turno ?? "");
                cmd.Parameters.AddWithValue("@diaFestivo", oVentaE.DiaFestivo ?? "");
                cmd.Parameters.AddWithValue("@observaciones", oVentaE.Observaciones ?? "");
                cmd.Parameters.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
                cmd.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito ?? "");
                cmd.Parameters.AddWithValue("@estado", oVentaE.Estado ?? "");
                cmd.Parameters.AddWithValue("@eliminarLineas", eliminarLineas);
                cmd.Parameters.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
                cmd.Parameters.AddWithValue("@formaPago", oVentaE.FormaPago ?? "");
                cmd.Parameters.AddWithValue("@cuit", oVentaE.Cuit ?? "");
                cmd.Parameters.AddWithValue("@email", oVentaE.Email ?? "");
                cmd.Parameters.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
                cmd.Parameters.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
                cmd.Parameters.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
                cmd.Parameters.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);
                cmd.Parameters.AddWithValue("@pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            var dt = new DataTable();

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerVentas", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idVendedor", idVendedor);
                cmd.Parameters.AddWithValue("@idCliente", idCliente);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@soloAnulados", soloAnulados);

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados)
        {
            var dt = new DataTable();

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("ventasVendedorCierreCaja", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
                cmd.Parameters.AddWithValue("@fechaDesde", oCierreE.FechaHoraInicio);
                cmd.Parameters.AddWithValue("@fechaHasta", oCierreE.FechaHoraCierre ?? DateTime.Now);
                cmd.Parameters.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@soloAnulados", soloAnulados);

                da.Fill(dt);
            }

            return dt;
        }

        public float getTotalVenta(int idVenta)
        {
            const string sql = @"
                SELECT SUM(cantKg * precioKg)
                FROM dbo.LineaVenta
                WHERE idVenta = @idVenta;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value) return 0f;
                return Convert.ToSingle(result);
            }
        }

        public float getTotalKgsVenta(int idVenta)
        {
            const string sql = @"
                SELECT SUM(cantKg)
                FROM dbo.LineaVenta
                WHERE idVenta = @idVenta;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                object result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value) return 0f;
                return Convert.ToSingle(result);
            }
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var dt = new DataTable();

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerTotalVentas", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVendedor", idVendedor);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@fechaDesde", (object)fechaDesde ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@fechaHasta", (object)fechaHasta ?? DBNull.Value);

                da.Fill(dt);
            }

            if (dt.Rows.Count == 0) return 0f;
            return string.IsNullOrEmpty(dt.Rows[0]["totalS"]?.ToString()) ? 0f : Convert.ToSingle(dt.Rows[0]["totalS"]);
        }

        public Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarLineaVenta", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
                cmd.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
                cmd.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
                cmd.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
                cmd.Parameters.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
                cmd.Parameters.AddWithValue("@idAlicuotaIva", oLineaE.Corte.IdAlicuotaIva);
                cmd.Parameters.AddWithValue("@alicuotaIva", oLineaE.Corte.AlicuotaIva);
                cmd.Parameters.AddWithValue("@kgsAjusteTarj", Math.Round(oLineaE.KgsAjusteTarj, 3));
                cmd.Parameters.AddWithValue("@porcKgsAjusteTarj", oLineaE.CantKg == 0 ? 0 : Math.Round(oLineaE.KgsAjusteTarj / oLineaE.CantKg, 3));
                cmd.Parameters.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));
                cmd.Parameters.AddWithValue("@ajustePrecio", Math.Round(oLineaE.AjustePrecio, 2));
                cmd.Parameters.AddWithValue("@bonificacion", oLineaE.Bonificacion);
                cmd.Parameters.AddWithValue("@idLineaVentaAnulado", oLineaE.IndexAnulado);

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                oLineaE.IdLineaVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
                return oLineaE;
            }
        }

        public Entidades.Venta getUltimaVentaVendedor(Entidades.CierreCaja oCierreE)
        {
            const string sql = @"SELECT TOP 1 idVenta
                                 FROM Ventas
                                 WHERE idVendedor = @idVendedor AND idSucursal = @idSucursal
                                 ORDER BY idVenta DESC;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
                cmd.Parameters.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                int idVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);

                return idVenta > 0 ? getVentaById(idVenta) : null;
            }
        }

        public List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
        {
            var lista = new List<Entidades.LineaVenta>();
            var oCorteD = new Datos.Corte(_empresa);

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerLineasVenta", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var oLinea = new Entidades.LineaVenta
                        {
                            IdLineaVenta = Convert.ToInt32(dr["idLineaVenta"]),
                            Venta = new Entidades.Venta { IdVenta = Convert.ToInt32(dr["idVenta"]) },
                            Corte = oCorteD.findCorteById(Convert.ToInt32(dr["idCorte"]), false),
                            CantKg = dr["cantKg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["cantKg"]),
                            IdAlicuotaIva = dr["idAlicuotaIva"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idAlicuotaIva"]),
                            AlicuotaIva = dr["alicuotaIva"] == DBNull.Value ? 0 : Convert.ToSingle(dr["alicuotaIva"]),
                            PrecioKg = dr["precioKg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["precioKg"]),
                            KgsAjusteTarj = dr["kgsAjusteTarj"] == DBNull.Value ? 0 : Convert.ToSingle(dr["kgsAjusteTarj"]),
                            Bonificacion = string.IsNullOrEmpty(dr["bonificacion"]?.ToString()) ? 0 : Convert.ToSingle(dr["bonificacion"]),
                            IndexAnulado = dr["idLineaVentaAnulado"] == DBNull.Value ? -1 : Convert.ToInt32(dr["idLineaVentaAnulado"])
                        };

                        oLinea.PrecioKgOriginal = oLinea.PrecioKg;

                        // pesoBalanza a veces no existe / no es bool
                        oLinea.PesoBalanza = (dr["pesoBalanza"] != DBNull.Value) && Convert.ToBoolean(dr["pesoBalanza"]);

                        // estado: tu lógica original
                        oLinea.Estado = string.IsNullOrEmpty(dr["estado"]?.ToString()) ? 0 : 1;

                        lista.Add(oLinea);
                    }
                }
            }

            return lista;
        }

        public void agregarStockVenta(Entidades.Venta oVentaE)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarStockVenta", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
                cmd.Parameters.AddWithValue("@estado", oVentaE.Estado ?? "");

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV)
        {
            const string sql = @"
                INSERT INTO TemporalLineaVenta
                (idVendedor, fechaInicioPesada, idCorte, cantKg, precioKg, totalCorte, ventaEnCurso, idSucursal, creado)
                VALUES
                (@idVendedor, @fechaInicioPesada, @idCorte, @cantKg, @precioKg, @totalCorte, @ventaEnCurso, @idSucursal, @creado);";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.Add("@idVendedor", SqlDbType.Int).Value = oTemporalLV.Vendedor.Id;
                cmd.Parameters.Add("@fechaInicioPesada", SqlDbType.DateTime2).Value = oTemporalLV.FechaInicioPesada;
                cmd.Parameters.Add("@idCorte", SqlDbType.Int).Value = oTemporalLV.Corte.idCorte;
                cmd.Parameters.Add("@cantKg", SqlDbType.Decimal).Value = oTemporalLV.CantKg;
                cmd.Parameters.Add("@precioKg", SqlDbType.Decimal).Value = oTemporalLV.Corte.PrecioKg;
                cmd.Parameters.Add("@totalCorte", SqlDbType.Decimal).Value = oTemporalLV.TotalCorte;
                cmd.Parameters.Add("@ventaEnCurso", SqlDbType.TinyInt).Value = oTemporalLV.VentaEnCurso;
                cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = oTemporalLV.Sucursal.idSucursal;
                cmd.Parameters.Add("@creado", SqlDbType.DateTime2).Value = DateTime.Now;

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
        {
            var dt = new DataTable();

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerTemporalLineaVenta", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idVendedor", idVendedor);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);
                cmd.Parameters.AddWithValue("@conVentas", conVentas);

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            var dt = new DataTable();

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("getAllLineasVenta", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@idVendedor", idVendedor);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
        {
            var dt = new DataTable();

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("ultimasVentasCliente", con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idPersona", idPersona);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                da.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Si no se factura debe guardarse X (remito).
        /// </summary>
        public void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte)
        {
            const string sql = "UPDATE Ventas SET tipoComprobante = @tipoComprobante WHERE idVenta = @idVenta;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@tipoComprobante", letraId_tipoCbte);
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void actualizarCliente(int idVenta, int idPersona)
        {
            const string sql = "UPDATE Ventas SET idPersona = @idPersona WHERE idVenta = @idVenta;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idPersona", idPersona);
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region EXPENDIO

        public int agregarExpendio(Entidades.Venta oVentaE)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarExpendio", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idExpendio", oVentaE.IdVenta);
                cmd.Parameters.AddWithValue("@fechaExpendio", oVentaE.FechaVenta);
                cmd.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
                cmd.Parameters.AddWithValue("@identificacionExpendio", oVentaE.IdentificacionExpendio ?? "");
                cmd.Parameters.AddWithValue("@sector", oVentaE.Sector ?? "");
                cmd.Parameters.AddWithValue("@cantItems", oVentaE.CantItems ?? "");
                cmd.Parameters.AddWithValue("@importe", oVentaE.TotalImporte);
                cmd.Parameters.AddWithValue("@serialCPU", oVentaE.SerialCPU ?? "");

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            }
        }

        public Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("agregarLineaExpendio", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idExpendio", oLineaE.Venta.IdVenta);
                cmd.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
                cmd.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
                cmd.Parameters.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
                cmd.Parameters.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                oLineaE.IdLineaVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
                return oLineaE;
            }
        }

        public void asignarVentaEnExpendio(int idVenta, int idExpendio)
        {
            const string sql = "UPDATE Expendios SET idVenta = @idVenta WHERE idExpendio = @idExpendio;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idVenta", idVenta);
                cmd.Parameters.AddWithValue("@idExpendio", idExpendio);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal)
        {
            var dt = new DataTable();
            DateTime fechaDesde = DateTime.Now.AddMinutes(-ultimosMinutos);

            const string sql = @"
                SELECT fechaExpendio,
                       e.idExpendio,
                       identificacionExpendio,
                       sector,
                       c.codigo,
                       c.corte,
                       le.cantKg,
                       le.precioKg,
                       (le.cantKg * le.precioKg) AS total,
                       idVenta,
                       u.nombre AS vendedor
                FROM dbo.Expendios e
                INNER JOIN dbo.LineaExpendio le ON e.idExpendio = le.idExpendio
                INNER JOIN dbo.Corte c ON le.idCorte = c.idCorte
                INNER JOIN dbo.Usuarios u ON e.idVendedor = u.id
                WHERE fechaExpendio > @fechaDesde AND e.idSucursal = @idSucursal
                ORDER BY fechaExpendio;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable obtenerSectores()
        {
            var dt = new DataTable();

            const string sql = "SELECT sector FROM Sectores;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                da.Fill(dt);
            }

            return dt;
        }

        public string getUltimoSectorSelect(string serialCPU)
        {
            const string sql = "SELECT sector FROM Licencias WHERE nroLicencia = @nroLicencia;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@nroLicencia", serialCPU ?? "");

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                return (scalar == null || scalar == DBNull.Value) ? "" : scalar.ToString().Trim();
            }
        }

        public Entidades.Venta getExpedioById(int idExpendio)
        {
            const string sql = "SELECT * FROM Expendios WHERE idExpendio = @idExpendio;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idExpendio", idExpendio);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    var oExpendioE = new Entidades.Venta
                    {
                        IdExpendio = Convert.ToInt32(dr["idExpendio"]),
                        IdVenta = int.TryParse(dr["idVenta"]?.ToString(), out int idv) ? idv : 0,
                        FechaVenta = Convert.ToDateTime(dr["fechaExpendio"]),
                        IdentificacionExpendio = Convert.ToString(dr["identificacionExpendio"]),
                        Sector = Convert.ToString(dr["sector"]),
                        CantItems = Convert.ToString(dr["cantItems"]),
                        TotalImporte = dr["importe"] == DBNull.Value ? 0f : Convert.ToSingle(dr["importe"])
                    };

                    var oUsuarioD = new Usuario(_empresa);
                    oExpendioE.Vendedor = oUsuarioD.getUsuarioById(Convert.ToInt32(dr["idVendedor"]));

                    var oSucursalD = new Sucursal(_empresa);
                    oExpendioE.Sucursal = oSucursalD.findById(Convert.ToInt32(dr["idSucursal"]));

                    oExpendioE.LineasVenta = obtenerLineasExpendio(oExpendioE.IdExpendio);

                    return oExpendioE;
                }
            }
        }

        public List<Entidades.LineaVenta> obtenerLineasExpendio(int idExpendio)
        {
            var lista = new List<Entidades.LineaVenta>();
            var oCorteD = new Datos.Corte(_empresa);

            const string sql = "SELECT * FROM LineaExpendio WHERE idExpendio = @idExpendio;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idExpendio", idExpendio);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var oLinea = new Entidades.LineaVenta
                        {
                            IdLineaVenta = Convert.ToInt32(dr["idLineaExpendio"]),
                            Corte = oCorteD.findCorteById(Convert.ToInt32(dr["idCorte"]), false),
                            CantKg = dr["cantKg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["cantKg"]),
                            PrecioKg = dr["precioKg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["precioKg"]),
                            PesoBalanza = dr["pesoBalanza"] != DBNull.Value && Convert.ToBoolean(dr["pesoBalanza"])
                        };

                        lista.Add(oLinea);
                    }
                }
            }

            return lista;
        }

        #endregion

        #region FACTURA ELECTRONICA

        public int esVentaSinFacturar(int idVenta, bool esNotaCredito)
        {
            int idFactElec = 0;

            string validarComprobantes = esNotaCredito
                ? $"(codTipoCbteAfip = {Entidades.FacturaElectronica.codNotaCreditoA_Afip} OR codTipoCbteAfip = {Entidades.FacturaElectronica.codNotaCreditoB_Afip} OR codTipoCbteAfip = {Entidades.FacturaElectronica.codNotaCreditoC_Afip})"
                : $"(codTipoCbteAfip = {Entidades.FacturaElectronica.codFacturaA_Afip} OR codTipoCbteAfip = {Entidades.FacturaElectronica.codFacturaB_Afip} OR codTipoCbteAfip = {Entidades.FacturaElectronica.codFacturaC_Afip})";

            string sql = $@"
                SELECT TOP(1) id
                FROM FacturaElectronica
                WHERE idVenta = @idVenta
                  AND CAE IS NOT NULL
                  AND {validarComprobantes}
                ORDER BY id DESC;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                idFactElec = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            }

            return idFactElec;
        }

        public int existeFacturaElect(int idVenta)
        {
            const string sql = "SELECT TOP 1 id FROM FacturaElectronica WHERE CAE <> '' AND idVenta = @idVenta ORDER BY id DESC;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idVenta", idVenta);

                if (con.State != ConnectionState.Open) con.Open();
                object scalar = cmd.ExecuteScalar();
                return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            }
        }

        public void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE)
        {
            using (SqlConnection con = conn.conectar(_empresa))
            {
                if (con.State != ConnectionState.Open) con.Open();

                using (SqlCommand cmd = new SqlCommand("addOrEditFacturaElectronica", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = conn.TimeOut();

                    cmd.Parameters.AddWithValue("@id", oFacturaElectronicaE.Id);
                    cmd.Parameters.AddWithValue("@ptoVtaAfip", oFacturaElectronicaE.PtoVtaAfip);
                    cmd.Parameters.AddWithValue("@fechaEmisionAfip",
                        oFacturaElectronicaE.FechaEmisionAfip < DateTime.Today.AddYears(-100) ? (object)DBNull.Value : (object)oFacturaElectronicaE.FechaEmisionAfip);
                    cmd.Parameters.AddWithValue("@descTipoCbteAfip", oFacturaElectronicaE.DescTipoCbteAfip ?? "");
                    cmd.Parameters.AddWithValue("@codTipoCbteAfip", oFacturaElectronicaE.CodTipoCbteAfip);
                    cmd.Parameters.AddWithValue("@nroCbteAfip", oFacturaElectronicaE.NroCbteAfip ?? "");
                    cmd.Parameters.AddWithValue("@tipoDocAfip", oFacturaElectronicaE.TipoDocAfip ?? "");
                    cmd.Parameters.AddWithValue("@nroDocAfip", oFacturaElectronicaE.NroDocAfip ?? "");
                    cmd.Parameters.AddWithValue("@razonSocialAFIP", oFacturaElectronicaE.RazonSocialAFIP ?? "");
                    cmd.Parameters.AddWithValue("@condicionIvaAFIP", oFacturaElectronicaE.CondicionIvaAFIP ?? "");
                    cmd.Parameters.AddWithValue("@domicilioAFIP", oFacturaElectronicaE.DomicilioAFIP ?? "");
                    cmd.Parameters.AddWithValue("@condicionVenta", oFacturaElectronicaE.CondicionVenta ?? "");
                    cmd.Parameters.AddWithValue("@formaPago", oFacturaElectronicaE.FormaPago ?? "");
                    cmd.Parameters.AddWithValue("@CAE", oFacturaElectronicaE.CAE1 ?? "");
                    cmd.Parameters.AddWithValue("@fecVtoCAE", oFacturaElectronicaE.FecVtoCAE ?? "");
                    cmd.Parameters.AddWithValue("@importeNetoGravado", oFacturaElectronicaE.ImporteNetoGravado);
                    cmd.Parameters.AddWithValue("@iva", oFacturaElectronicaE.Iva);
                    cmd.Parameters.AddWithValue("@importeTotal", oFacturaElectronicaE.ImporteTotal);
                    cmd.Parameters.AddWithValue("@PorcentajeFacturacion", oFacturaElectronicaE.PorcentajeFacturacion);
                    cmd.Parameters.AddWithValue("@descItemUnitario", oFacturaElectronicaE.DescItemUnitario ?? "");
                    cmd.Parameters.AddWithValue("@idVenta", oFacturaElectronicaE.IdVenta);
                    cmd.Parameters.AddWithValue("@error", oFacturaElectronicaE.Error);
                    cmd.Parameters.AddWithValue("@mensajeError", oFacturaElectronicaE.MensajeError ?? "");
                    cmd.Parameters.AddWithValue("@fechaError",
                        (oFacturaElectronicaE.FechaError == null || oFacturaElectronicaE.FechaError < DateTime.Today.AddYears(-100))
                            ? (object)DBNull.Value
                            : (object)oFacturaElectronicaE.FechaError);

                    object scalar = cmd.ExecuteScalar();
                    oFacturaElectronicaE.Id = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
                }

                // Alicuotas IVA
                if (oFacturaElectronicaE.ListaAlicuota != null && oFacturaElectronicaE.ListaAlicuota.Count > 0)
                {
                    const string sqlAlic = @"
                        INSERT INTO AlicuotaIvaPorFactura (idFacturaElectronica, idIva, baseImponible, importe)
                        VALUES (@idFacturaElectronica, @idIva, @baseImponible, @importe);";

                    using (SqlCommand cmdA = new SqlCommand(sqlAlic, con))
                    {
                        cmdA.CommandType = CommandType.Text;
                        cmdA.CommandTimeout = conn.TimeOut();

                        foreach (Entidades.AlicuotaIva a in oFacturaElectronicaE.ListaAlicuota)
                        {
                            cmdA.Parameters.Clear();
                            cmdA.Parameters.Add("@idFacturaElectronica", SqlDbType.Int).Value = oFacturaElectronicaE.Id;
                            cmdA.Parameters.Add("@idIva", SqlDbType.Int).Value = a.IdIva;
                            cmdA.Parameters.Add("@baseImponible", SqlDbType.Float).Value = a.BaseImponible;
                            cmdA.Parameters.Add("@importe", SqlDbType.Float).Value = a.Importe;

                            cmdA.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        public Entidades.FacturaElectronica getFactuElecById(int idFactuElec)
        {
            const string sql = "SELECT * FROM FacturaElectronica WHERE id = @id;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", idFactuElec);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (!dr.Read()) return null;

                    var o = new Entidades.FacturaElectronica
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        PtoVtaAfip = Convert.ToString(dr["ptoVtaAfip"]),
                        FechaEmisionAfip = dr["fechaEmisionAfip"] == DBNull.Value ? null : (DateTime?)dr["fechaEmisionAfip"],
                        DescTipoCbteAfip = Convert.ToString(dr["descTipoCbteAfip"]),
                        CodTipoCbteAfip = dr["codTipoCbteAfip"] == DBNull.Value ? 0 : Convert.ToInt32(dr["codTipoCbteAfip"]),
                        NroCbteAfip = Convert.ToString(dr["nroCbteAfip"]),
                        TipoDocAfip = Convert.ToString(dr["tipoDocAfip"]),
                        NroDocAfip = Convert.ToString(dr["NroDocAfip"]),
                        RazonSocialAFIP = Convert.ToString(dr["razonSocialAFIP"]),
                        CondicionIvaAFIP = Convert.ToString(dr["condicionIvaAFIP"]),
                        DomicilioAFIP = Convert.ToString(dr["domicilioAFIP"]),
                        CondicionVenta = Convert.ToString(dr["condicionVenta"]),
                        FormaPago = Convert.ToString(dr["formaPago"]),
                        CAE1 = Convert.ToString(dr["CAE"]),
                        FecVtoCAE = Convert.ToString(dr["fecVtoCAE"]),
                        ImporteNetoGravado = string.IsNullOrEmpty(dr["importeNetoGravado"]?.ToString()) ? 0 : Convert.ToSingle(dr["importeNetoGravado"]),
                        Iva = string.IsNullOrEmpty(dr["iva"]?.ToString()) ? 0 : Convert.ToSingle(dr["iva"]),
                        ImporteTotal = string.IsNullOrEmpty(dr["importeTotal"]?.ToString()) ? 0 : Convert.ToSingle(dr["importeTotal"]),
                        PorcentajeFacturacion = string.IsNullOrEmpty(dr["porcentajeFacturacion"]?.ToString()) ? 100 : Convert.ToSingle(dr["porcentajeFacturacion"]),
                        DescItemUnitario = Convert.ToString(dr["descItemUnitario"]),
                        IdVenta = dr["idVenta"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idVenta"]),
                        Error = dr["error"] != DBNull.Value && Convert.ToBoolean(dr["error"]),
                        MensajeError = Convert.ToString(dr["mensajeError"]),
                        FechaError = dr["fechaError"] == DBNull.Value ? null : (DateTime?)dr["fechaError"]
                    };

                    // Cierro reader antes de cargar relacionados
                    // (salimos del using dr al retornar, pero hacemos carga luego)
                    o.ListaAlicuota = null;
                    o.Venta = null;

                    // Retorno ahora y cargo relacionados afuera para evitar lector abierto
                    // (pero acá estamos adentro del using dr; mejor: guardamos y luego cargamos fuera)
                    // -> hacemos "return" después del using dr, con una variable.
                    // Para eso, usamos variable temporal:
                    // (ver abajo)
                    // NOTA: no retornamos acá.
                    var factura = o;

                    // Salimos del using dr
                    // (rompemos bucle)
                    // pero como no hay bucle, hacemos:
                    // - dejamos la variable y seguimos afuera
                    // (vamos a usar una variable externa)
                    // => implemento eso:
                }
            }

            // Para mantener el código simple y seguro (sin reader abierto), re-ejecuto la lectura y cargo todo bien:
            // (Preferí robustez antes que micro-optimización.)

            Entidades.FacturaElectronica fact = null;

            using (SqlConnection con2 = conn.conectar(_empresa))
            using (SqlCommand cmd2 = new SqlCommand(sql, con2))
            {
                cmd2.CommandType = CommandType.Text;
                cmd2.CommandTimeout = conn.TimeOut();
                cmd2.Parameters.AddWithValue("@id", idFactuElec);

                con2.Open();
                using (SqlDataReader dr2 = cmd2.ExecuteReader())
                {
                    if (!dr2.Read()) return null;

                    fact = new Entidades.FacturaElectronica
                    {
                        Id = Convert.ToInt32(dr2["id"]),
                        PtoVtaAfip = Convert.ToString(dr2["ptoVtaAfip"]),
                        FechaEmisionAfip = dr2["fechaEmisionAfip"] == DBNull.Value ? null : (DateTime?)dr2["fechaEmisionAfip"],
                        DescTipoCbteAfip = Convert.ToString(dr2["descTipoCbteAfip"]),
                        CodTipoCbteAfip = dr2["codTipoCbteAfip"] == DBNull.Value ? 0 : Convert.ToInt32(dr2["codTipoCbteAfip"]),
                        NroCbteAfip = Convert.ToString(dr2["nroCbteAfip"]),
                        TipoDocAfip = Convert.ToString(dr2["tipoDocAfip"]),
                        NroDocAfip = Convert.ToString(dr2["NroDocAfip"]),
                        RazonSocialAFIP = Convert.ToString(dr2["razonSocialAFIP"]),
                        CondicionIvaAFIP = Convert.ToString(dr2["condicionIvaAFIP"]),
                        DomicilioAFIP = Convert.ToString(dr2["domicilioAFIP"]),
                        CondicionVenta = Convert.ToString(dr2["condicionVenta"]),
                        FormaPago = Convert.ToString(dr2["formaPago"]),
                        CAE1 = Convert.ToString(dr2["CAE"]),
                        FecVtoCAE = Convert.ToString(dr2["fecVtoCAE"]),
                        ImporteNetoGravado = string.IsNullOrEmpty(dr2["importeNetoGravado"]?.ToString()) ? 0 : Convert.ToSingle(dr2["importeNetoGravado"]),
                        Iva = string.IsNullOrEmpty(dr2["iva"]?.ToString()) ? 0 : Convert.ToSingle(dr2["iva"]),
                        ImporteTotal = string.IsNullOrEmpty(dr2["importeTotal"]?.ToString()) ? 0 : Convert.ToSingle(dr2["importeTotal"]),
                        PorcentajeFacturacion = string.IsNullOrEmpty(dr2["porcentajeFacturacion"]?.ToString()) ? 100 : Convert.ToSingle(dr2["porcentajeFacturacion"]),
                        DescItemUnitario = Convert.ToString(dr2["descItemUnitario"]),
                        IdVenta = dr2["idVenta"] == DBNull.Value ? 0 : Convert.ToInt32(dr2["idVenta"]),
                        Error = dr2["error"] != DBNull.Value && Convert.ToBoolean(dr2["error"]),
                        MensajeError = Convert.ToString(dr2["mensajeError"]),
                        FechaError = dr2["fechaError"] == DBNull.Value ? null : (DateTime?)dr2["fechaError"]
                    };
                }
            }

            // relacionados
            fact.ListaAlicuota = getAlicuotaIvaFactura(fact.Id);
            fact.Venta = getVentaById(fact.IdVenta);

            return fact;
        }

        public List<Entidades.AlicuotaIva> getAlicuotaIvaFactura(int idFacturaElectronica)
        {
            var lista = new List<Entidades.AlicuotaIva>();

            const string sql = @"
                SELECT a.idIva, ai.iva, a.baseImponible, a.importe
                FROM dbo.AlicuotaIvaPorFactura a
                INNER JOIN dbo.AlicuotasIva ai ON a.idIva = ai.idIva
                WHERE a.idFacturaElectronica = @idFacturaElectronica;";

            using (SqlConnection con = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idFacturaElectronica", idFacturaElectronica);

                if (con.State != ConnectionState.Open) con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        lista.Add(new Entidades.AlicuotaIva
                        {
                            IdIva = Convert.ToInt32(dr["idIva"]),
                            Iva = dr["iva"] == DBNull.Value ? 0 : Convert.ToSingle(dr["iva"]),
                            BaseImponible = dr["baseImponible"] == DBNull.Value ? 0 : Convert.ToSingle(dr["baseImponible"]),
                            Importe = dr["importe"] == DBNull.Value ? 0 : Convert.ToSingle(dr["importe"])
                        });
                    }
                }
            }

            return lista;
        }

        #endregion
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data;
//using System.Data.SqlClient;
//using Entidades;
//using static System.Collections.Specialized.BitVector32;
//using System.Collections;
//using Utilidades;

//namespace Datos
//{
//    public class Venta
//    {
//        SqlCommand cmVenta;
//        SqlDataAdapter daVenta;

//        Utilidades.Conexion conn;
//        private readonly IEmpresaContext _empresa;
//        public Venta(IEmpresaContext empresa)
//        {
//            _empresa = empresa ??
//                throw new ArgumentNullException(nameof(empresa));

//            conn = new Utilidades.Conexion();
//        }


//        private Entidades.Venta MapVenta(SqlDataReader drVenta, bool cargarLineas = true)
//        {
//            var oVentaE = new Entidades.Venta
//            {
//                IdVenta = Convert.ToInt32(drVenta["idVenta"]),
//                FechaVenta = Convert.ToDateTime(drVenta["fechaVenta"]),
//                Turno = Convert.ToString(drVenta["turno"]),
//                DiaFestivo = Convert.ToString(drVenta["diaFestivo"]),
//                Observaciones = Convert.ToString(drVenta["observaciones"]),
//                NroRemito = Convert.ToString(drVenta["nroRemito"]),
//                Estado = Convert.ToString(drVenta["estado"]),
//                EnCtaCte = Convert.ToBoolean(drVenta["enCtaCte"]),
//                Cuit = ColumnaExiste(drVenta, "cuit") && drVenta["cuit"] != DBNull.Value
//                        ? Convert.ToString(drVenta["cuit"])
//                        : "",
//                Email = ColumnaExiste(drVenta, "email") && drVenta["email"] != DBNull.Value
//                        ? Convert.ToString(drVenta["email"])
//                        : "",
//                FormaPago = Convert.ToString(drVenta["formaPago"]),
//                TipoComprobante = Convert.ToChar(drVenta["tipoComprobante"]),
//                Creado = Convert.ToDateTime(drVenta["creado"]),
//                Actualizado = drVenta["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(drVenta["actualizado"]),
//                PagoMixtoEfectivo = drVenta["pagoMixtoEfectivo"] == DBNull.Value ? 0f : float.Parse(drVenta["pagoMixtoEfectivo"].ToString()),
//                IdVendedor = Convert.ToInt32(drVenta["idVendedor"]),
//                IdSucursal = Convert.ToInt32(drVenta["idSucursal"]),
//                IdPersona = Convert.ToInt32(drVenta["idPersona"])
//            };

//            // Cargar datos relacionados
//            Datos.Usuario oUsuarioD = new Usuario(_empresa);
//            oVentaE.Vendedor = oUsuarioD.getUsuarioById(oVentaE.IdVendedor);

//            Datos.Sucursal oSucursalD = new Sucursal(_empresa);
//            oVentaE.Sucursal = oSucursalD.findById(oVentaE.IdSucursal);

//            Datos.Persona oPersonaD = new Datos.Persona(_empresa);
//            oVentaE.Persona = oPersonaD.findById(oVentaE.IdPersona);

//            if (cargarLineas)
//            {
//                oVentaE.LineasVenta = obtenerLineasVenta(oVentaE.IdVenta);
//                oVentaE.CantItems = oVentaE.getCantItems(oVentaE).ToString();
//            }

//            oVentaE.TotalImporte = getTotalVenta(oVentaE.IdVenta);
//            oVentaE.TotalImporteOriginal = oVentaE.TotalImporte;

//            return oVentaE;
//        }

//        private bool ColumnaExiste(SqlDataReader dr, string columna)
//        {
//            try
//            {
//                return dr.GetOrdinal(columna) >= 0;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        public Entidades.Venta getVentaById(int idVenta)
//        {
//            using (SqlConnection connSql = this.conn.conectar(_empresa))
//            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Ventas WHERE idVenta = @idVenta", connSql))
//            {
//                cmd.Parameters.AddWithValue("@idVenta", idVenta);
//                connSql.Open();

//                using (SqlDataReader drVenta = cmd.ExecuteReader())
//                {
//                    if (drVenta.Read())
//                    {
//                        return MapVenta(drVenta);
//                    }
//                }
//            }

//            return null; // No se encontró
//        }

//        public List<Entidades.Venta> getAllVentas(
//                                            DateTime fechaDesde,
//                                            DateTime fechaHasta,
//                                            string texto,
//                                            int? idVendedor,
//                                            int? idCliente,
//                                            int? idSucursal,
//                                            bool soloAnulados,
//                                            bool cargarLineas)
//        {
//            var listaVentas = new List<Entidades.Venta>();

//            string consultaSQL = @"SELECT * FROM Ventas
//                                    WHERE fechaVenta >= @fechaDesde
//                                      AND fechaVenta <= @fechaHasta
//                                      AND (@idVendedor = -1 OR idVendedor = @idVendedor)
//                                      AND (@idCliente = -1 OR idPersona = @idCliente)
//                                      AND (@idSucursal = -1 OR idSucursal = @idSucursal)
//                                    ORDER BY fechaVenta DESC;
//                                    ";
//            using (SqlConnection connSql = this.conn.conectar(_empresa))
//            using (SqlCommand cmVenta = new SqlCommand(consultaSQL, connSql))
//            {
//                // Parámetros
//                cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//                cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//                cmVenta.Parameters.AddWithValue("@texto", texto ?? (object)DBNull.Value);
//                cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor ?? (object)DBNull.Value);
//                cmVenta.Parameters.AddWithValue("@idCliente", idCliente ?? (object)DBNull.Value);
//                cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal ?? (object)DBNull.Value);

//                connSql.Open();

//                using (SqlDataReader drVenta = cmVenta.ExecuteReader())
//                {
//                    while (drVenta.Read())
//                    {
//                        listaVentas.Add(MapVenta(drVenta, cargarLineas));
//                    }
//                }
//            }

//            return listaVentas;
//        }

//        public int agregarVenta(Entidades.Venta oVentaE)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; 
//            cmVenta.CommandTimeout = conn.TimeOut();            
//            cmVenta.CommandText = "agregarVenta";
//            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
//            cmVenta.Parameters.AddWithValue("@fechaVenta",oVentaE.FechaVenta);
//            cmVenta.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
//            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
//            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
//            cmVenta.Parameters.AddWithValue("@turno",oVentaE.Turno == null ? "" : oVentaE.Turno);
//            cmVenta.Parameters.AddWithValue("@diaFestivo",oVentaE.DiaFestivo);
//            cmVenta.Parameters.AddWithValue("@observaciones",oVentaE.Observaciones);
//            cmVenta.Parameters.AddWithValue("@idPersona",oVentaE.Persona.idPersona);
//            cmVenta.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito ?? "");
//            cmVenta.Parameters.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
//            cmVenta.Parameters.AddWithValue("@formaPago", oVentaE.FormaPago);
//            cmVenta.Parameters.AddWithValue("@cuit", oVentaE.Cuit);
//            cmVenta.Parameters.AddWithValue("@email", oVentaE.Email);
//            cmVenta.Parameters.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
//            cmVenta.Parameters.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
//            cmVenta.Parameters.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
//            cmVenta.Parameters.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);
//            cmVenta.Parameters.AddWithValue("@pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);

//            cmVenta.Connection.Open();
//            SqlDataReader drVenta = cmVenta.ExecuteReader();
//            int idVenta = 0;
//            while (drVenta.Read())
//            {
//                idVenta = Convert.ToInt32(drVenta["idVenta"].ToString());
//            }

//            cmVenta.Connection.Close();
//            return idVenta;
//        }

//        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior, bool eliminarLineas)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            /// Se eliminan todas las LineaVenta, y se actualiza datos de Venta
//            /// 
//            /// -Si tiene egreso de caja por venta cta cte se genera un registro opuesto.
//            /// 
//            cmVenta.CommandText = "modificarVenta";
//            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
//            cmVenta.Parameters.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
//            cmVenta.Parameters.AddWithValue("@idSucursal", SucAnterior);
//            cmVenta.Parameters.AddWithValue("@idSucNueva", oVentaE.Sucursal.idSucursal);
//            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
//            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
//            cmVenta.Parameters.AddWithValue("@turno", oVentaE.Turno == null ? "" : oVentaE.Turno);
//            cmVenta.Parameters.AddWithValue("@diaFestivo", oVentaE.DiaFestivo);
//            cmVenta.Parameters.AddWithValue("@observaciones", oVentaE.Observaciones);
//            cmVenta.Parameters.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
//            cmVenta.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito);
//            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);
//            cmVenta.Parameters.AddWithValue("@eliminarLineas", eliminarLineas);
//            cmVenta.Parameters.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
//            cmVenta.Parameters.AddWithValue("@formaPago", oVentaE.FormaPago);
//            cmVenta.Parameters.AddWithValue("@cuit", oVentaE.Cuit);
//            cmVenta.Parameters.AddWithValue("@email", oVentaE.Email);
//            cmVenta.Parameters.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
//            cmVenta.Parameters.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
//            cmVenta.Parameters.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
//            cmVenta.Parameters.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);
//            cmVenta.Parameters.AddWithValue("@pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);

//            cmVenta.Connection.Open();
//            cmVenta.ExecuteNonQuery();
//            cmVenta.Connection.Close();
//            cmVenta = null;
//        }

//        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
//        {
//            DataTable dtVentas = new DataTable();
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();
//            cmVenta.CommandType = CommandType.StoredProcedure;
//            cmVenta.CommandText="obtenerVentas";
//            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmVenta.Parameters.AddWithValue("@texto", texto);
//            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
//            cmVenta.Parameters.AddWithValue("@idCliente", idCliente);
//            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmVenta.Parameters.AddWithValue("@soloAnulados", soloAnulados);

//            cmVenta.ExecuteNonQuery();
//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtVentas);
//            cmVenta.Connection.Close();

//            return dtVentas;
//        }

//        public DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados)
//        {
//            DataTable dtVentasVendedorCierre = new DataTable();
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "ventasVendedorCierreCaja";
//            cmVenta.Parameters.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
//            cmVenta.Parameters.AddWithValue("@fechaDesde", oCierreE.FechaHoraInicio);
//            cmVenta.Parameters.AddWithValue("@fechaHasta", oCierreE.FechaHoraCierre == null ? DateTime.Now : oCierreE.FechaHoraCierre);
//            //cmVenta.Parameters.AddWithValue("@texto", texto);
//            cmVenta.Parameters.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);
//            cmVenta.Parameters.AddWithValue("@soloAnulados", soloAnulados);

//            cmVenta.Connection.Open();
//            cmVenta.ExecuteNonQuery();
//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtVentasVendedorCierre);
//            cmVenta.Connection.Close();

//            return dtVentasVendedorCierre;
//        }

//        public float getTotalVenta(int idVenta)
//        {
//            DataTable dtTotalVenta = new DataTable();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            string consulta = "SELECT SUM(cantKg * precioKg) AS total "+
//                                "FROM dbo.LineaVenta "+
//                                "WHERE     idVenta = "+idVenta+" "+
//                                "GROUP BY idVenta";
//            cmVenta.CommandText = consulta;
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.Connection.Open();
//            object result = cmVenta.ExecuteScalar();
//            double totalVentaD = (result == null || result == DBNull.Value)
//                ? 0
//                : Convert.ToDouble(result);

//            float totalVenta = (float)totalVentaD;
//            cmVenta.Connection.Close();
//            return totalVenta;
//        }

//        public float getTotalKgsVenta(int idVenta)
//        {
//            DataTable dtTotalVenta = new DataTable();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            string consulta = "SELECT SUM(cantKg) AS totalKgsVenta " +
//                                "FROM dbo.LineaVenta " +
//                                "WHERE     idVenta = " + idVenta + " " +
//                                "GROUP BY idVenta";
//            cmVenta.CommandText = consulta;
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.Connection.Open();
//            double totalKgsVentaD = (double)cmVenta.ExecuteScalar();
//            float totalKgsVenta = (float)totalKgsVentaD;
//            cmVenta.Connection.Close();
//            return totalKgsVenta;
//        }

//        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
//        {
//            DataTable dtVentas = new DataTable();
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "obtenerTotalVentas";
//            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
//            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);

//            cmVenta.Connection.Open();
//            cmVenta.ExecuteNonQuery();
//            cmVenta.Connection.Close();
//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtVentas);

//            float totalVentas = dtVentas.Rows[0]["totalS"].ToString().Equals("") ? 0 : float.Parse(dtVentas.Rows[0]["totalS"].ToString());
//            return totalVentas;
//        }

//        public Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "agregarLineaVenta";
//            cmVenta.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
//            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
//            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
//            cmVenta.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
//            cmVenta.Parameters.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
//            cmVenta.Parameters.AddWithValue("@idAlicuotaIva", oLineaE.Corte.IdAlicuotaIva);
//            cmVenta.Parameters.AddWithValue("@alicuotaIva", oLineaE.Corte.AlicuotaIva);
//            cmVenta.Parameters.AddWithValue("@kgsAjusteTarj", Math.Round(oLineaE.KgsAjusteTarj, 3));
//            cmVenta.Parameters.AddWithValue("@porcKgsAjusteTarj", oLineaE.CantKg == 0 ? 0 : Math.Round(oLineaE.KgsAjusteTarj / oLineaE.CantKg, 3));
//            cmVenta.Parameters.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));
//            cmVenta.Parameters.AddWithValue("@ajustePrecio", Math.Round(oLineaE.AjustePrecio, 2));
//            cmVenta.Parameters.AddWithValue("@bonificacion", oLineaE.Bonificacion);
//            cmVenta.Parameters.AddWithValue("@idLineaVentaAnulado", oLineaE.IndexAnulado);

//            cmVenta.Connection.Open();
//            oLineaE.IdLineaVenta = (int)cmVenta.ExecuteScalar();
//            cmVenta.Connection.Close();

//            return oLineaE;
//        }

//        //public Entidades.Venta getVentaById(int idVenta)
//        //{
//        //    Entidades.Venta oVentaE = null;

//        //    using (SqlConnection conn = this.conn.conectar(_empresa))
//        //    using (SqlCommand cmd = new SqlCommand("SELECT * FROM Ventas WHERE idVenta = @idVenta", conn))
//        //    {
//        //        cmd.Parameters.AddWithValue("@idVenta", idVenta);

//        //        conn.Open();
//        //        using (SqlDataReader drVenta = cmd.ExecuteReader())
//        //        {
//        //            if (drVenta.Read())
//        //            {
//        //                oVentaE = new Entidades.Venta(); // se crea el objeto solo si hay datos
//        //                oVentaE.IdVenta = Convert.ToInt32(drVenta["idVenta"]);
//        //                oVentaE.FechaVenta = Convert.ToDateTime(drVenta["fechaVenta"]);
//        //                oVentaE.Turno = Convert.ToString(drVenta["turno"]);
//        //                oVentaE.DiaFestivo = Convert.ToString(drVenta["diaFestivo"]);
//        //                oVentaE.Observaciones = Convert.ToString(drVenta["observaciones"]);
//        //                oVentaE.NroRemito = Convert.ToString(drVenta["nroRemito"]);
//        //                oVentaE.Estado = Convert.ToString(drVenta["estado"]);
//        //                oVentaE.EnCtaCte = Convert.ToBoolean(drVenta["enCtaCte"]);
//        //                oVentaE.Cuit = Convert.ToString(drVenta["cuit"]);
//        //                oVentaE.Email = Convert.ToString(drVenta["email"]);
//        //                oVentaE.Cuit = Convert.ToString(drVenta["cuit"]);
//        //                oVentaE.FormaPago = Convert.ToString(drVenta["formaPago"]);
//        //                oVentaE.TipoComprobante = Convert.ToChar(drVenta["tipoComprobante"]);
//        //                oVentaE.Creado = Convert.ToDateTime(drVenta["creado"]);
//        //                oVentaE.Actualizado = drVenta["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drVenta["actualizado"]);

//        //                oVentaE.PagoMixtoEfectivo = drVenta["pagoMixtoEfectivo"].Equals(DBNull.Value) ? 0f : float.Parse(drVenta["pagoMixtoEfectivo"].ToString());

//        //                oVentaE.IdVendedor = Convert.ToInt32(drVenta["idVendedor"]);
//        //                oVentaE.IdSucursal = Convert.ToInt32(drVenta["idSucursal"]);
//        //                oVentaE.Idpersona = Convert.ToInt32(drVenta["idPersona"]);
//        //            }
//        //        }
//        //    }
//        //    if (oVentaE != null)
//        //    {
//        //        Datos.Usuario oUsuarioD = new Usuario();
//        //        oVentaE.Vendedor = oUsuarioD.getUsuarioById(oVentaE.IdVendedor);

//        //        Datos.Sucursal oSucursalD = new Sucursal();
//        //        oVentaE.Sucursal = oSucursalD.findById(oVentaE.IdSucursal);

//        //        Datos.Persona oPersonaD = new Datos.Persona();
//        //        oVentaE.Persona = oPersonaD.findById(oVentaE.Idpersona);

//        //        oVentaE.LineasVenta = obtenerLineasVenta(oVentaE.IdVenta);
//        //        oVentaE.TotalImporte = getTotalVenta(idVenta);
//        //        oVentaE.TotalImporteOriginal = oVentaE.TotalImporte;
//        //        oVentaE.CantItems = oVentaE.getCantItems(oVentaE).ToString();
//        //    }

//        //    return oVentaE;
//        //}

//        public Entidades.Venta getUltimaVentaVendedor(Entidades.CierreCaja oCierreE)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandText = "Select top 1 Ventas.* from Ventas where idVendedor = @idVendedor AND idSucursal = @idSucursal order by idVenta desc";

//            cmVenta.Parameters.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
//            cmVenta.Parameters.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);

//            Entidades.Venta oVentaE = new Entidades.Venta();
//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drVenta = cmVenta.ExecuteReader();
//                using (drVenta)
//                {
//                    while (drVenta.Read())
//                    {
//                        oVentaE = getVentaById(Convert.ToInt32(drVenta["idVenta"]));
//                    }
//                    return oVentaE;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//                oVentaE = null;
//            }
//        }

//        public List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
//        {
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "obtenerLineasVenta";
//            cmVenta.Parameters.AddWithValue("@idVenta", idVenta);

//            Datos.Corte oCorteD = new Datos.Corte(_empresa);    
//            //creo lista de Lineas
//            List<Entidades.LineaVenta> listaLineasVenta = new List<Entidades.LineaVenta>();
//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drLinea = cmVenta.ExecuteReader();
//                using (drLinea)
//                {
//                    while (drLinea.Read())
//                    {
//                        Entidades.LineaVenta oLinea = new Entidades.LineaVenta();

//                        oLinea.IdLineaVenta = Convert.ToInt32(drLinea["idLineaVenta"]);
//                        //se crea y asiga la venta
//                        Entidades.Venta oVenta=new Entidades.Venta();
//                        oVenta.IdVenta= Convert.ToInt32(drLinea["idVenta"]);

//                        oLinea.Venta=oVenta;

//                        oLinea.Corte = oCorteD.findCorteById(Convert.ToInt32(drLinea["idCorte"]), false); //oCorte;

//                        oLinea.CantKg = float.Parse(drLinea["cantKg"].ToString());
//                        oLinea.IdAlicuotaIva = Convert.ToInt32(drLinea["idAlicuotaIva"]);
//                        oLinea.AlicuotaIva = float.Parse(drLinea["alicuotaIva"].ToString());
//                        oLinea.PrecioKg = float.Parse(drLinea["precioKg"].ToString());
//                        oLinea.PrecioKgOriginal = oLinea.PrecioKg;
//                        oLinea.KgsAjusteTarj = float.Parse(drLinea["kgsAjusteTarj"].ToString());
//                        oLinea.Bonificacion = string.IsNullOrEmpty(drLinea["bonificacion"].ToString()) ? 0 : float.Parse(drLinea["bonificacion"].ToString());
//                        oLinea.IndexAnulado = DBNull.Value.Equals(drLinea["idLineaVentaAnulado"]) ? -1 : Convert.ToInt32(drLinea["idLineaVentaAnulado"].ToString());

//                        try
//                        {
//                            oLinea.PesoBalanza = Convert.ToBoolean(drLinea["pesoBalanza"]);
//                        }
//                        catch (Exception)
//                        {
//                            oLinea.PesoBalanza = false;
//                        }

//                        if (drLinea["estado"].ToString()=="")
//                        {
//                            oLinea.Estado = 0;
//                        }
//                        else
//                        {
//                            oLinea.Estado = 1;
//                        }

//                        listaLineasVenta.Add(oLinea);

//                        oVenta = null;
//                        oLinea = null;
//                    }
//                    return listaLineasVenta;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//                listaLineasVenta = null;
//            }
//        }

//        public void agregarStockVenta(Entidades.Venta oVentaE)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "agregarStockVenta";
//            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
//            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);

//            cmVenta.Connection.Open();
//            cmVenta.ExecuteNonQuery();
//            cmVenta.Connection.Close();
//        }

//        public void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandText = "insert into TemporalLineaVenta (idVendedor, fechaInicioPesada, idCorte, cantKg, precioKg, totalCorte, ventaEnCurso, idSucursal, creado) values " +
//                "(" + oTemporalLV.Vendedor.Id + ", @fechaInicioPesada," + oTemporalLV.Corte.idCorte +
//                ",@cantKg,@precioKg,@totalCorte, @ventaEnCurso, @idSucursal, @creado)";
//            cmVenta.Parameters.Add("@fechaInicioPesada", SqlDbType.DateTime2).Value = oTemporalLV.FechaInicioPesada;
//            cmVenta.Parameters.Add("@cantKg", SqlDbType.Decimal).Value = oTemporalLV.CantKg;
//            cmVenta.Parameters.Add("@precioKg", SqlDbType.Decimal).Value = oTemporalLV.Corte.PrecioKg;
//            cmVenta.Parameters.Add("@totalCorte", SqlDbType.Decimal).Value = oTemporalLV.TotalCorte;
//            cmVenta.Parameters.Add("@ventaEnCurso", SqlDbType.TinyInt).Value = oTemporalLV.VentaEnCurso;
//            cmVenta.Parameters.Add("@idSucursal", SqlDbType.TinyInt).Value = oTemporalLV.Sucursal.idSucursal;
//            cmVenta.Parameters.Add("@creado", SqlDbType.DateTime2).Value = DateTime.Now;
//            try
//            {
//                cmVenta.Connection.Open();
//                cmVenta.ExecuteNonQuery();
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//            }
//        }

//        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
//        {
//            DataTable dtVentas = new DataTable();
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "obtenerTemporalLineaVenta";
//            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmVenta.Parameters.AddWithValue("@texto", texto);
//            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
//            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
//            cmVenta.Parameters.AddWithValue("@conVentas", conVentas);

//            cmVenta.ExecuteNonQuery();
//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtVentas);
//            cmVenta.Connection.Close();

//            return dtVentas;
//        }

//        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
//        {
//            DataTable dtVentas = new DataTable();
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();
//            cmVenta.CommandType = CommandType.StoredProcedure;
//            cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "getAllLineasVenta";
//            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
//            cmVenta.Parameters.AddWithValue("@texto", texto);
//            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
//            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);

//            cmVenta.ExecuteNonQuery();
//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtVentas);
//            cmVenta.Connection.Close();

//            return dtVentas;
//        }

//        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
//        {
//            DataTable dtVentas = new DataTable();
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();
//            cmVenta.CommandType = CommandType.StoredProcedure;
//            cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "ultimasVentasCliente";
//            cmVenta.Parameters.AddWithValue("@idPersona", idPersona);
//            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);

//            cmVenta.ExecuteNonQuery();
//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtVentas);
//            cmVenta.Connection.Close();

//            return dtVentas;
//        }

//        /// <summary>
//        /// Actualiza el campo tipo comprobante en la tabla Venta. 
//        /// Si no se factura debe guardarse X (remito). Para llevar control de lo facturado
//        /// </summary>
//        /// <param name="letraId_tipoCbte"></param>
//        public void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte)
//        {
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();

//            cmVenta.CommandType = CommandType.Text; 
//            cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "UPDATE Ventas SET tipoComprobante = @tipoComprobante WHERE idVenta = " + idVenta;
//            cmVenta.Parameters.AddWithValue("@tipoComprobante", letraId_tipoCbte);

//            cmVenta.ExecuteNonQuery();
//            cmVenta.Connection.Close();

//            cmVenta = null;
//        }
//        public void actualizarCliente(int idVenta, int idPersona)
//        {
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();

//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "UPDATE Ventas SET idPersona = @idPersona WHERE idVenta = " + idVenta;
//            cmVenta.Parameters.AddWithValue("@idPersona", idPersona);

//            cmVenta.ExecuteNonQuery();
//            cmVenta.Connection.Close();

//            cmVenta = null;
//        }

//        #region EXPENDIO
//        public int agregarExpendio(Entidades.Venta oVentaE)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure;
//            cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "agregarExpendio"; 

//            cmVenta.Parameters.AddWithValue("@idExpendio", oVentaE.IdVenta);
//            cmVenta.Parameters.AddWithValue("@fechaExpendio", oVentaE.FechaVenta);
//            cmVenta.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
//            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
//            cmVenta.Parameters.AddWithValue("@identificacionExpendio", oVentaE.IdentificacionExpendio);
//            cmVenta.Parameters.AddWithValue("@sector", oVentaE.Sector);
//            cmVenta.Parameters.AddWithValue("@cantItems", oVentaE.CantItems);
//            cmVenta.Parameters.AddWithValue("@importe", oVentaE.TotalImporte);
//            cmVenta.Parameters.AddWithValue("@serialCPU", oVentaE.SerialCPU);

//            cmVenta.Connection.Open();
//            SqlDataReader drVenta = cmVenta.ExecuteReader();
//            int idVenta = 0;
//            while (drVenta.Read())
//            {
//                idVenta = Convert.ToInt32(drVenta["idExpendio"].ToString());
//            }

//            cmVenta.Connection.Close();
//            return idVenta;
//        }

//        public Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "agregarLineaExpendio";
//            cmVenta.Parameters.AddWithValue("@idExpendio", oLineaE.Venta.IdVenta);
//            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
//            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
//            cmVenta.Parameters.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
//            cmVenta.Parameters.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));

//            cmVenta.Connection.Open();
//            oLineaE.IdLineaVenta = (int)cmVenta.ExecuteScalar();
//            cmVenta.Connection.Close();

//            return oLineaE;
//        }

//        public void asignarVentaEnExpendio(int idVenta, int idExpendio)
//        {
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();

//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandTimeout = conn.TimeOut();
//            cmVenta.CommandText = "UPDATE Expendios SET idVenta = @idVenta WHERE idExpendio = " + idExpendio;
//            cmVenta.Parameters.AddWithValue("@idVenta", idVenta);
//            cmVenta.Parameters.AddWithValue("@idExpendio", idExpendio);

//            cmVenta.ExecuteNonQuery();
//            cmVenta.Connection.Close();

//            cmVenta = null;
//        }
//        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal)
//        {
//            DataTable dtSectores = new DataTable();
//            daVenta = new SqlDataAdapter();

//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text; cmVenta.CommandTimeout = conn.TimeOut();
//            DateTime fechaDesde = DateTime.Now.AddMinutes(-ultimosMinutos); 
//            string consulta = "SELECT fechaExpendio, dbo.Expendios.idExpendio as idExpendio, identificacionExpendio, sector, dbo.Corte.codigo as codigo, dbo.Corte.corte as corte, dbo.LineaExpendio.cantKg as cantKg, dbo.LineaExpendio.precioKg as precioKg, (dbo.LineaExpendio.cantKg * dbo.LineaExpendio.precioKg) as total, idVenta, dbo.Usuarios.nombre as vendedor " +
//                "FROM dbo.Expendios INNER JOIN dbo.LineaExpendio ON dbo.Expendios.idExpendio = dbo.LineaExpendio.idExpendio INNER JOIN dbo.Corte ON dbo.LineaExpendio.idCorte = dbo.Corte.idCorte "+
//                "INNER JOIN dbo.Usuarios ON dbo.Expendios.idVendedor = dbo.Usuarios.id WHERE fechaExpendio > @fechaDesde AND idSucursal = @idSucursal ORDER BY fechaExpendio ;";
//            cmVenta.CommandText = consulta;
//            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
//            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);

//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtSectores);

//            cmVenta.Connection.Close();

//            return dtSectores;
//        }

//        public DataTable obtenerSectores()
//        {
//            DataTable dtSectores = new DataTable();
//            daVenta = new SqlDataAdapter();

//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text; cmVenta.CommandTimeout = conn.TimeOut();
//            string consulta = "SELECT  sector FROM  Sectores";
//            cmVenta.CommandText = consulta;

//            daVenta.SelectCommand = cmVenta;
//            daVenta.Fill(dtSectores);

//            cmVenta.Connection.Close();

//            return dtSectores;
//        }

//        public string getUltimoSectorSelect(string serialCPU)
//        {
//            string query = $"SELECT sector FROM Licencias WHERE nroLicencia = '{serialCPU}'";
//            string sector = "";
//            // Conexión a la base de datos
//            using (SqlConnection connection = conn.conectar(_empresa))
//            {
//                // Abrir conexión
//                connection.Open();

//                // Crear comando
//                using (SqlCommand command = new SqlCommand(query, connection))
//                {
//                    // Ejecutar el comando y leer los datos
//                    using (SqlDataReader reader = command.ExecuteReader())
//                    {
//                        while (reader.Read())
//                        {
//                            // Obtener el valor de la columna "sector"
//                            sector = reader["sector"] != DBNull.Value ? reader["sector"].ToString().Trim() : string.Empty;
//                        }
//                    }
//                }
//            }
//            return sector;
//        }

//        public Entidades.Venta getExpedioById(int idExpendio)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandText = "Select Expendios.* from Expendios where idExpendio =" + idExpendio;

//            Entidades.Venta oExpendioE = new Entidades.Venta();

//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drVenta = cmVenta.ExecuteReader();

//                using (drVenta)
//                {
//                    while (drVenta.Read())
//                    {
//                        oExpendioE.IdExpendio = Convert.ToInt32(drVenta["idExpendio"]);
//                        oExpendioE.IdVenta = int.TryParse(drVenta["idVenta"].ToString(), out int result) ? result : 0;
//                        Datos.Usuario oUsuarioD = new Usuario(_empresa);
//                        oExpendioE.Vendedor = oUsuarioD.getUsuarioById(Convert.ToInt32(drVenta["idVendedor"]));
//                        oExpendioE.FechaVenta = Convert.ToDateTime(drVenta["fechaExpendio"]);

//                        Datos.Sucursal oSucursalD = new Sucursal(_empresa);
//                        oExpendioE.Sucursal = oSucursalD.findById(Convert.ToInt32(drVenta["idSucursal"]));

//                        oExpendioE.IdentificacionExpendio = Convert.ToString(drVenta["identificacionExpendio"]);
//                        oExpendioE.Sector = Convert.ToString(drVenta["sector"]);

//                        oExpendioE.CantItems = Convert.ToString(drVenta["cantItems"]);
//                        oExpendioE.TotalImporte = float.TryParse(drVenta["importe"].ToString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float resultImporte) ? resultImporte : 0;
//                        oExpendioE.LineasVenta = obtenerLineasExpendio(oExpendioE.IdExpendio);

//                    }
//                    return oExpendioE;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//                oExpendioE = null;
//            }
//        }

//        public List<Entidades.LineaVenta> obtenerLineasExpendio(int idExpendio)
//        {
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandText = "Select LineaExpendio.* from LineaExpendio where idExpendio =" + idExpendio;

//            Datos.Corte oCorteD = new Datos.Corte(_empresa);
//            //creo lista de Lineas
//            List<Entidades.LineaVenta> listaLineasVenta = new List<Entidades.LineaVenta>();
//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drLinea = cmVenta.ExecuteReader();
//                using (drLinea)
//                {
//                    while (drLinea.Read())
//                    {
//                        Entidades.LineaVenta oLinea = new Entidades.LineaVenta();

//                        oLinea.IdLineaVenta = Convert.ToInt32(drLinea["idLineaExpendio"]);
//                        oLinea.Corte = oCorteD.findCorteById(Convert.ToInt32(drLinea["idCorte"]), false);
//                        oLinea.CantKg = float.Parse(drLinea["cantKg"].ToString());
//                        oLinea.PrecioKg = float.Parse(drLinea["precioKg"].ToString());
//                        try
//                        {
//                            oLinea.PesoBalanza = Convert.ToBoolean(drLinea["pesoBalanza"]);
//                        }
//                        catch (Exception)
//                        {
//                            oLinea.PesoBalanza = false;
//                        }
//                        listaLineasVenta.Add(oLinea);
//                        oLinea = null;
//                    }
//                    return listaLineasVenta;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//                listaLineasVenta = null;
//            }
//        }

//        #endregion


//        #region FACTURA ELECTRONICA        

//        /// <summary>
//        /// Pasando el idVenta busca en tabla Factura electronica. Si CAE is null -> Pendiente Facturacion.
//        /// Se Retorna Cero si está pendiente.
//        /// </summary>
//        /// <param name="idVenta"></param>
//        /// <returns></returns>
//        public int esVentaSinFacturar(int idVenta, bool esNotaCredito)
//        {
//            int maxDiasParaFacturar = 6;
//            int idFactElec = 0;
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            //cmVenta.CommandText = "Select TOP(1) id from FacturaElectronica where fechaEmisionAfip > @fechaEmisionAfip and idVenta = \'" +
//            //    idVenta.ToString() + "\' and CAE is not null ORDER BY id desc";

//            string validarComprobantes = esNotaCredito ? " (codTipoCbteAfip = " + Entidades.FacturaElectronica.codNotaCreditoA_Afip +
//            " OR " + "codTipoCbteAfip = " + Entidades.FacturaElectronica.codNotaCreditoB_Afip + " OR " +
//            "codTipoCbteAfip = " + Entidades.FacturaElectronica.codNotaCreditoC_Afip + ") " :
//                " (codTipoCbteAfip = " + Entidades.FacturaElectronica.codFacturaA_Afip +
//            " OR " + "codTipoCbteAfip = " + Entidades.FacturaElectronica.codFacturaB_Afip + " OR " + 
//            "codTipoCbteAfip = " + Entidades.FacturaElectronica.codFacturaC_Afip + ") ";

//        cmVenta.CommandText = "Select TOP(1) id from FacturaElectronica where idVenta = \'" +
//                idVenta.ToString() + "\' and CAE is not null and " +  validarComprobantes + "  ORDER BY id desc";

//            cmVenta.Parameters.Add("@fechaEmisionAfip", SqlDbType.DateTime2).Value = DateTime.Today.AddDays(-maxDiasParaFacturar);
//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drVenta = cmVenta.ExecuteReader();
//                using (drVenta)
//                {
//                    while (drVenta.Read())
//                    {
//                        idFactElec = Convert.ToInt32(drVenta["id"]);
//                    }
//                    return idFactElec;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//            }
//        }

//        public int existeFacturaElect(int idVenta)
//        {
//            int idFactElec = 0;
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandText = "Select id from FacturaElectronica where CAE <> '' and idVenta = " + idVenta;
//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drVenta = cmVenta.ExecuteReader();
//                using (drVenta)
//                {
//                    while (drVenta.Read())
//                    {
//                        idFactElec = Convert.ToInt32(drVenta["id"]);
//                    }
//                    return idFactElec;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//            }
//        }

//        public void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE)
//        {
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.Connection.Open();
//            cmVenta.CommandType = CommandType.StoredProcedure;
//            cmVenta.CommandText = "addOrEditFacturaElectronica";
//            cmVenta.Parameters.AddWithValue("@id", oFacturaElectronicaE.Id);
//            cmVenta.Parameters.AddWithValue("@ptoVtaAfip", oFacturaElectronicaE.PtoVtaAfip);
//            cmVenta.Parameters.AddWithValue("@fechaEmisionAfip", oFacturaElectronicaE.FechaEmisionAfip < DateTime.Today.AddYears(-100) ?
//                (DateTime?)null : oFacturaElectronicaE.FechaEmisionAfip);
//            cmVenta.Parameters.AddWithValue("@descTipoCbteAfip", oFacturaElectronicaE.DescTipoCbteAfip);
//            cmVenta.Parameters.AddWithValue("@codTipoCbteAfip", oFacturaElectronicaE.CodTipoCbteAfip);
//            cmVenta.Parameters.AddWithValue("@nroCbteAfip", oFacturaElectronicaE.NroCbteAfip);
//            cmVenta.Parameters.AddWithValue("@tipoDocAfip", oFacturaElectronicaE.TipoDocAfip);
//            cmVenta.Parameters.AddWithValue("@nroDocAfip", oFacturaElectronicaE.NroDocAfip);
//            cmVenta.Parameters.AddWithValue("@razonSocialAFIP", oFacturaElectronicaE.RazonSocialAFIP);
//            cmVenta.Parameters.AddWithValue("@condicionIvaAFIP", oFacturaElectronicaE.CondicionIvaAFIP);
//            cmVenta.Parameters.AddWithValue("@domicilioAFIP", oFacturaElectronicaE.DomicilioAFIP);
//            cmVenta.Parameters.AddWithValue("@condicionVenta", oFacturaElectronicaE.CondicionVenta);
//            cmVenta.Parameters.AddWithValue("@formaPago", oFacturaElectronicaE.FormaPago);
//            cmVenta.Parameters.AddWithValue("@CAE", oFacturaElectronicaE.CAE1);
//            cmVenta.Parameters.AddWithValue("@fecVtoCAE", oFacturaElectronicaE.FecVtoCAE);
//            cmVenta.Parameters.AddWithValue("@importeNetoGravado", oFacturaElectronicaE.ImporteNetoGravado);
//            cmVenta.Parameters.AddWithValue("@iva", oFacturaElectronicaE.Iva);
//            cmVenta.Parameters.AddWithValue("@importeTotal", oFacturaElectronicaE.ImporteTotal);
//            cmVenta.Parameters.AddWithValue("@PorcentajeFacturacion", oFacturaElectronicaE.PorcentajeFacturacion);
//            cmVenta.Parameters.AddWithValue("@descItemUnitario", oFacturaElectronicaE.DescItemUnitario);
//            cmVenta.Parameters.AddWithValue("@idVenta", oFacturaElectronicaE.IdVenta);
//            cmVenta.Parameters.AddWithValue("@error", oFacturaElectronicaE.Error);
//            cmVenta.Parameters.AddWithValue("@mensajeError", oFacturaElectronicaE.MensajeError);
//            cmVenta.Parameters.AddWithValue("@fechaError", oFacturaElectronicaE.FechaError.Equals(null) || oFacturaElectronicaE.FechaError < DateTime.Today.AddYears(-100) ?
//                (DateTime?)null : oFacturaElectronicaE.FechaError);

//            //cmVenta.ExecuteNonQuery();
//            oFacturaElectronicaE.Id = Convert.ToInt32(cmVenta.ExecuteScalar());

//            if (oFacturaElectronicaE.ListaAlicuota != null && oFacturaElectronicaE.ListaAlicuota.Count > 0)
//            {
//                cmVenta.CommandType = CommandType.Text;
//                cmVenta.CommandText = "INSERT INTO AlicuotaIvaPorFactura (idFacturaElectronica, idIva, baseImponible, importe) VALUES " +
//                                      "(@idFacturaElectronica, @idIva, @baseImponible, @importe)";

//                foreach (Entidades.AlicuotaIva alicuotaIva in oFacturaElectronicaE.ListaAlicuota)
//                {
//                    cmVenta.Parameters.Clear();  // Evita acumulación de parámetros

//                    cmVenta.Parameters.Add("@idFacturaElectronica", SqlDbType.Int).Value = oFacturaElectronicaE.Id;
//                    cmVenta.Parameters.Add("@idIva", SqlDbType.Int).Value = alicuotaIva.IdIva;
//                    cmVenta.Parameters.Add("@baseImponible", SqlDbType.Float).Value = alicuotaIva.BaseImponible;
//                    cmVenta.Parameters.Add("@importe", SqlDbType.Float).Value = alicuotaIva.Importe;

//                    cmVenta.ExecuteNonQuery();
//                }
//            }
//            cmVenta.Connection.Close();
//        }

//        public Entidades.FacturaElectronica getFactuElecById(int idFactuElec)
//        {
//            cmVenta = new SqlCommand();
//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text;
//            cmVenta.CommandText = "Select FacturaElectronica.* from FacturaElectronica where id =" + idFactuElec;

//            Entidades.FacturaElectronica oFacturaElectronicaE = new Entidades.FacturaElectronica();

//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drFactuElec = cmVenta.ExecuteReader();

//                using (drFactuElec)
//                {
//                    while (drFactuElec.Read())
//                    {
//                        oFacturaElectronicaE.Id = Convert.ToInt32(drFactuElec["id"]);
//                        oFacturaElectronicaE.PtoVtaAfip = Convert.ToString(drFactuElec["ptoVtaAfip"]);
//                        oFacturaElectronicaE.FechaEmisionAfip = drFactuElec["fechaEmisionAfip"].Equals(DBNull.Value) ? null : (DateTime?)(drFactuElec["fechaEmisionAfip"]);
//                        oFacturaElectronicaE.DescTipoCbteAfip = Convert.ToString(drFactuElec["descTipoCbteAfip"]);
//                        oFacturaElectronicaE.CodTipoCbteAfip = Convert.ToInt32(drFactuElec["codTipoCbteAfip"]);
//                        oFacturaElectronicaE.NroCbteAfip = Convert.ToString(drFactuElec["nroCbteAfip"]);
//                        oFacturaElectronicaE.TipoDocAfip = Convert.ToString(drFactuElec["tipoDocAfip"]);
//                        oFacturaElectronicaE.NroDocAfip = Convert.ToString(drFactuElec["NroDocAfip"]);
//                        oFacturaElectronicaE.RazonSocialAFIP = Convert.ToString(drFactuElec["razonSocialAFIP"]);
//                        oFacturaElectronicaE.CondicionIvaAFIP = Convert.ToString(drFactuElec["condicionIvaAFIP"]);
//                        oFacturaElectronicaE.DomicilioAFIP = Convert.ToString(drFactuElec["domicilioAFIP"]);
//                        oFacturaElectronicaE.CondicionVenta = Convert.ToString(drFactuElec["condicionVenta"]);
//                        oFacturaElectronicaE.FormaPago = Convert.ToString(drFactuElec["formaPago"]);
//                        oFacturaElectronicaE.CAE1 = Convert.ToString(drFactuElec["CAE"]);
//                        oFacturaElectronicaE.FecVtoCAE = Convert.ToString(drFactuElec["fecVtoCAE"]);
//                        oFacturaElectronicaE.ImporteNetoGravado = string.IsNullOrEmpty((drFactuElec["importeNetoGravado"]).ToString()) ? 0 : float.Parse((drFactuElec["importeNetoGravado"]).ToString());
//                        oFacturaElectronicaE.Iva = string.IsNullOrEmpty((drFactuElec["iva"]).ToString()) ? 0 : float.Parse((drFactuElec["iva"]).ToString());
//                        oFacturaElectronicaE.ImporteTotal = string.IsNullOrEmpty((drFactuElec["importeTotal"]).ToString()) ? 0 : float.Parse((drFactuElec["importeTotal"]).ToString());
//                        oFacturaElectronicaE.PorcentajeFacturacion = string.IsNullOrEmpty((drFactuElec["porcentajeFacturacion"]).ToString()) ? 100 : float.Parse((drFactuElec["porcentajeFacturacion"]).ToString());
//                        oFacturaElectronicaE.DescItemUnitario = Convert.ToString(drFactuElec["descItemUnitario"]);
//                        oFacturaElectronicaE.IdVenta = Convert.ToInt32(drFactuElec["idVenta"]);
//                        oFacturaElectronicaE.Error = Convert.ToBoolean(drFactuElec["error"]);
//                        oFacturaElectronicaE.MensajeError = Convert.ToString(drFactuElec["mensajeError"]);
//                        oFacturaElectronicaE.FechaError = drFactuElec["fechaError"].Equals(DBNull.Value) ? null : (DateTime?)(drFactuElec["actualizado"]);

//                        oFacturaElectronicaE.ListaAlicuota = getAlicuotaIvaFactura(oFacturaElectronicaE.Id);
//                        oFacturaElectronicaE.Venta = getVentaById(oFacturaElectronicaE.IdVenta);

//                    }
//                    return oFacturaElectronicaE;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//                oFacturaElectronicaE = null;
//            }
//        }

//        public List<Entidades.AlicuotaIva> getAlicuotaIvaFactura(int idFacturaElectronica)
//        {
//            daVenta = new SqlDataAdapter();
//            cmVenta = new SqlCommand();

//            cmVenta.Connection = conn.conectar(_empresa);
//            cmVenta.CommandType = CommandType.Text; cmVenta.CommandTimeout = conn.TimeOut();
//            //cmVenta.CommandText = "SELECT idFacturaElectronica, idIva, baseImponible, importe FROM AlicuotaIvaPorFactura WHERE  idFacturaElectronica = @idFacturaElectronica";
//            cmVenta.CommandText = @"SELECT dbo.AlicuotaIvaPorFactura.idFacturaElectronica, dbo.AlicuotaIvaPorFactura.idIva, 
//                                            dbo.AlicuotasIva.iva, dbo.AlicuotaIvaPorFactura.baseImponible, dbo.AlicuotaIvaPorFactura.importe
//                                    FROM dbo.AlicuotaIvaPorFactura INNER JOIN
//                                         dbo.AlicuotasIva ON dbo.AlicuotaIvaPorFactura.idIva = dbo.AlicuotasIva.idIva
//                                    WHERE  idFacturaElectronica = @idFacturaElectronica";
//            cmVenta.Parameters.AddWithValue("@idFacturaElectronica", idFacturaElectronica);

//            //creo lista de Lineas
//            List<Entidades.AlicuotaIva> listaAlicuotaIvaFactura = new List<Entidades.AlicuotaIva>();
//            try
//            {
//                cmVenta.Connection.Open();
//                SqlDataReader drLinea = cmVenta.ExecuteReader();
//                using (drLinea)
//                {
//                    while (drLinea.Read())
//                    {
//                        Entidades.AlicuotaIva oLinea = new Entidades.AlicuotaIva();

//                        oLinea.IdIva = Convert.ToInt32(drLinea["idIva"]);
//                        oLinea.Iva = float.Parse(drLinea["iva"].ToString());
//                        oLinea.BaseImponible = float.Parse(drLinea["baseImponible"].ToString());
//                        oLinea.Importe = float.Parse(drLinea["importe"].ToString());

//                        listaAlicuotaIvaFactura.Add(oLinea);
//                        oLinea = null;
//                    }
//                    return listaAlicuotaIvaFactura;
//                }
//            }
//            finally
//            {
//                cmVenta.Connection.Close();
//                listaAlicuotaIvaFactura = null;
//            }
//        }

//        #endregion
//    }
//}
