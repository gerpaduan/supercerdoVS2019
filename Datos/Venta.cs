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
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public Venta(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
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

            // Relacionados (ojo N+1 si listás muchas ventas)
            var oUsuarioD = new Usuario(_empresa);
            oVentaE.Vendedor = oUsuarioD.getUsuarioById(oVentaE.IdVendedor);

            var oSucursalD = new Sucursal(_empresa);
            oVentaE.Sucursal = oSucursalD.findById(oVentaE.IdSucursal);

            var oPersonaD = new Datos.Persona(_empresa, _param);
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
            const string sql = "SELECT * FROM Ventas WHERE idVenta = @idVenta;";

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => MapVenta(dr, true),
                p => p.Add("@idVenta", SqlDbType.Int).Value = idVenta
            );

            return list.Count > 0 ? list[0] : null;
        }

        /// <summary>
        /// Lista ventas con filtros. Si soloAnulados = true => filtra solo estado = 'ANULADO'.
        /// Si soloAnulados = false => NO filtra por estado (incluye todas).
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
            const string sql = @"
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

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => MapVenta(dr, cargarLineas),
                p =>
                {
                    p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
                    p.Add("@fechaHastaMas1", SqlDbType.DateTime).Value = fechaHasta.AddDays(1);
                    p.Add("@texto", SqlDbType.NVarChar, 200).Value = (texto ?? "").Trim();
                    p.Add("@idVendedor", SqlDbType.Int).Value = idVendedor ?? -1;
                    p.Add("@idCliente", SqlDbType.Int).Value = idCliente ?? -1;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal ?? -1;
                    p.Add("@soloAnulados", SqlDbType.Bit).Value = soloAnulados;
                }
            );
        }

        public int agregarVenta(Entidades.Venta oVentaE)
        {
            object scalar = Db.Scalar(
                _empresa,
                "agregarVenta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idVenta", oVentaE.IdVenta);
                    p.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
                    p.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
                    p.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
                    p.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
                    p.AddWithValue("@turno", oVentaE.Turno ?? "");
                    p.AddWithValue("@diaFestivo", oVentaE.DiaFestivo ?? "");
                    p.AddWithValue("@observaciones", oVentaE.Observaciones ?? "");
                    p.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
                    p.AddWithValue("@nroRemito", oVentaE.NroRemito ?? "");
                    p.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
                    p.AddWithValue("@formaPago", oVentaE.FormaPago ?? "");
                    p.AddWithValue("@cuit", oVentaE.Cuit ?? "");
                    p.AddWithValue("@email", oVentaE.Email ?? "");
                    p.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
                    p.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
                    p.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
                    p.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);
                    p.AddWithValue("@pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);
                }
            );

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        public void modificarVenta(Entidades.Venta oVentaE, int sucAnterior, bool eliminarLineas)
        {
            Db.NonQuery(
                _empresa,
                "modificarVenta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idVenta", oVentaE.IdVenta);
                    p.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
                    p.AddWithValue("@idSucursal", sucAnterior);
                    p.AddWithValue("@idSucNueva", oVentaE.Sucursal.idSucursal);
                    p.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
                    p.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
                    p.AddWithValue("@turno", oVentaE.Turno ?? "");
                    p.AddWithValue("@diaFestivo", oVentaE.DiaFestivo ?? "");
                    p.AddWithValue("@observaciones", oVentaE.Observaciones ?? "");
                    p.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
                    p.AddWithValue("@nroRemito", oVentaE.NroRemito ?? "");
                    p.AddWithValue("@estado", oVentaE.Estado ?? "");
                    p.AddWithValue("@eliminarLineas", eliminarLineas);
                    p.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
                    p.AddWithValue("@formaPago", oVentaE.FormaPago ?? "");
                    p.AddWithValue("@cuit", oVentaE.Cuit ?? "");
                    p.AddWithValue("@email", oVentaE.Email ?? "");
                    p.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
                    p.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
                    p.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
                    p.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);
                    p.AddWithValue("@pagoMixtoEfectivo", oVentaE.PagoMixtoEfectivo);
                }
            );
        }

        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            return Db.DataTable(
                _empresa,
                "obtenerVentas",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idVendedor", idVendedor);
                    p.AddWithValue("@idCliente", idCliente);
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@soloAnulados", soloAnulados);
                }
            );
        }

        public DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados)
        {
            return Db.DataTable(
                _empresa,
                "ventasVendedorCierreCaja",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
                    p.AddWithValue("@fechaDesde", oCierreE.FechaHoraInicio);
                    p.AddWithValue("@fechaHasta", oCierreE.FechaHoraCierre ?? DateTime.Now);
                    p.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);
                    p.AddWithValue("@soloAnulados", soloAnulados);
                }
            );
        }

        public float getTotalVenta(int idVenta)
        {
            const string sql = @"
                SELECT SUM(cantKg * precioKg)
                FROM dbo.LineaVenta
                WHERE idVenta = @idVenta;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p => p.Add("@idVenta", SqlDbType.Int).Value = idVenta
            );

            return (result == null || result == DBNull.Value) ? 0f : Convert.ToSingle(result);
        }

        public float getTotalKgsVenta(int idVenta)
        {
            const string sql = @"
                SELECT SUM(cantKg)
                FROM dbo.LineaVenta
                WHERE idVenta = @idVenta;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p => p.Add("@idVenta", SqlDbType.Int).Value = idVenta
            );

            return (result == null || result == DBNull.Value) ? 0f : Convert.ToSingle(result);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            DataTable dt = Db.DataTable(
                _empresa,
                "obtenerTotalVentas",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idVendedor", idVendedor);
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@fechaDesde", (object)fechaDesde ?? DBNull.Value);
                    p.AddWithValue("@fechaHasta", (object)fechaHasta ?? DBNull.Value);
                }
            );

            if (dt.Rows.Count == 0) return 0f;
            return string.IsNullOrEmpty(dt.Rows[0]["totalS"]?.ToString()) ? 0f : Convert.ToSingle(dt.Rows[0]["totalS"]);
        }

        public Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            object scalar = Db.Scalar(
                _empresa,
                "agregarLineaVenta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
                    p.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
                    p.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
                    p.AddWithValue("@idAnulado", oLineaE.Estado);
                    p.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
                    p.AddWithValue("@idAlicuotaIva", oLineaE.Corte.IdAlicuotaIva);
                    p.AddWithValue("@alicuotaIva", oLineaE.Corte.AlicuotaIva);
                    p.AddWithValue("@kgsAjusteTarj", Math.Round(oLineaE.KgsAjusteTarj, 3));
                    p.AddWithValue("@porcKgsAjusteTarj", oLineaE.CantKg == 0 ? 0 : Math.Round(oLineaE.KgsAjusteTarj / oLineaE.CantKg, 3));
                    p.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));
                    p.AddWithValue("@ajustePrecio", Math.Round(oLineaE.AjustePrecio, 2));
                    p.AddWithValue("@bonificacion", oLineaE.Bonificacion);
                    p.AddWithValue("@idLineaVentaAnulado", oLineaE.IndexAnulado);
                }
            );

            oLineaE.IdLineaVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            return oLineaE;
        }

        public Entidades.Venta getUltimaVentaVendedor(Entidades.CierreCaja oCierreE)
        {
            const string sql = @"
                SELECT TOP 1 idVenta
                FROM Ventas
                WHERE idVendedor = @idVendedor AND idSucursal = @idSucursal
                ORDER BY idVenta DESC;";

            object scalar = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
                    p.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);
                }
            );

            int idVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            return idVenta > 0 ? getVentaById(idVenta) : null;
        }

        public List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
        {
            var oCorteD = new Datos.Corte(_empresa, _param);

            return Db.Reader(
                _empresa,
                "obtenerLineasVenta",
                CommandType.StoredProcedure,
                dr =>
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
                    oLinea.PesoBalanza = (dr["pesoBalanza"] != DBNull.Value) && Convert.ToBoolean(dr["pesoBalanza"]);
                    oLinea.Estado = string.IsNullOrEmpty(dr["estado"]?.ToString()) ? 0 : 1;

                    return oLinea;
                },
                p => p.AddWithValue("@idVenta", idVenta)
            );
        }

        public void agregarStockVenta(Entidades.Venta oVentaE)
        {
            Db.NonQuery(
                _empresa,
                "agregarStockVenta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idVenta", oVentaE.IdVenta);
                    p.AddWithValue("@estado", oVentaE.Estado ?? "");
                }
            );
        }

        public void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV)
        {
            const string sql = @"
                INSERT INTO TemporalLineaVenta
                (idVendedor, fechaInicioPesada, idCorte, cantKg, precioKg, totalCorte, ventaEnCurso, idSucursal, creado)
                VALUES
                (@idVendedor, @fechaInicioPesada, @idCorte, @cantKg, @precioKg, @totalCorte, @ventaEnCurso, @idSucursal, @creado);";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.Add("@idVendedor", SqlDbType.Int).Value = oTemporalLV.Vendedor.Id;
                    p.Add("@fechaInicioPesada", SqlDbType.DateTime2).Value = oTemporalLV.FechaInicioPesada;
                    p.Add("@idCorte", SqlDbType.Int).Value = oTemporalLV.Corte.idCorte;
                    p.Add("@cantKg", SqlDbType.Decimal).Value = oTemporalLV.CantKg;
                    p.Add("@precioKg", SqlDbType.Decimal).Value = oTemporalLV.Corte.PrecioKg;
                    p.Add("@totalCorte", SqlDbType.Decimal).Value = oTemporalLV.TotalCorte;
                    p.Add("@ventaEnCurso", SqlDbType.TinyInt).Value = oTemporalLV.VentaEnCurso;
                    p.Add("@idSucursal", SqlDbType.Int).Value = oTemporalLV.Sucursal.idSucursal;
                    p.Add("@creado", SqlDbType.DateTime2).Value = DateTime.Now;
                }
            );
        }

        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
        {
            return Db.DataTable(
                _empresa,
                "obtenerTemporalLineaVenta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idVendedor", idVendedor);
                    p.AddWithValue("@idSucursal", idSucursal);
                    p.AddWithValue("@conVentas", conVentas);
                }
            );
        }

        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            return Db.DataTable(
                _empresa,
                "getAllLineasVenta",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@idVendedor", idVendedor);
                    p.AddWithValue("@idSucursal", idSucursal);
                }
            );
        }

        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
        {
            return Db.DataTable(
                _empresa,
                "ultimasVentasCliente",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idPersona", idPersona);
                    p.AddWithValue("@idSucursal", idSucursal);
                }
            );
        }

        /// <summary>
        /// Si no se factura debe guardarse X (remito).
        /// </summary>
        public void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte)
        {
            const string sql = "UPDATE Ventas SET tipoComprobante = @tipoComprobante WHERE idVenta = @idVenta;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.AddWithValue("@tipoComprobante", letraId_tipoCbte);
                    p.AddWithValue("@idVenta", idVenta);
                }
            );
        }

        public void actualizarCliente(int idVenta, int idPersona)
        {
            const string sql = "UPDATE Ventas SET idPersona = @idPersona WHERE idVenta = @idVenta;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.AddWithValue("@idPersona", idPersona);
                    p.AddWithValue("@idVenta", idVenta);
                }
            );
        }

        #endregion

        #region EXPENDIO

        public int agregarExpendio(Entidades.Venta oVentaE)
        {
            object scalar = Db.Scalar(
                _empresa,
                "agregarExpendio",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idExpendio", oVentaE.IdVenta);
                    p.AddWithValue("@fechaExpendio", oVentaE.FechaVenta);
                    p.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
                    p.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
                    p.AddWithValue("@identificacionExpendio", oVentaE.IdentificacionExpendio ?? "");
                    p.AddWithValue("@sector", oVentaE.Sector ?? "");
                    p.AddWithValue("@cantItems", oVentaE.CantItems ?? "");
                    p.AddWithValue("@importe", oVentaE.TotalImporte);
                    p.AddWithValue("@serialCPU", oVentaE.SerialCPU ?? "");
                }
            );

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        public Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE)
        {
            object scalar = Db.Scalar(
                _empresa,
                "agregarLineaExpendio",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@idExpendio", oLineaE.Venta.IdVenta);
                    p.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
                    p.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
                    p.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
                    p.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));
                }
            );

            oLineaE.IdLineaVenta = (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
            return oLineaE;
        }

        public void asignarVentaEnExpendio(int idVenta, int idExpendio)
        {
            const string sql = "UPDATE Expendios SET idVenta = @idVenta WHERE idExpendio = @idExpendio;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.AddWithValue("@idVenta", idVenta);
                    p.AddWithValue("@idExpendio", idExpendio);
                }
            );
        }

        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal)
        {
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

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@idSucursal", idSucursal);
                }
            );
        }

        public DataTable obtenerSectores()
        {
            const string sql = "SELECT sector FROM Sectores;";
            return Db.DataTable(_empresa, sql, CommandType.Text);
        }

        public string getUltimoSectorSelect(string serialCPU)
        {
            const string sql = "SELECT sector FROM Licencias WHERE nroLicencia = @nroLicencia;";

            object scalar = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@nroLicencia", serialCPU ?? "")
            );

            return (scalar == null || scalar == DBNull.Value) ? "" : scalar.ToString().Trim();
        }

        public Entidades.Venta getExpedioById(int idExpendio)
        {
            const string sql = "SELECT * FROM Expendios WHERE idExpendio = @idExpendio;";

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr =>
                {
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

                    // Carga de líneas (usa conexiones propias, OK)
                    oExpendioE.LineasVenta = obtenerLineasExpendio(oExpendioE.IdExpendio);

                    return oExpendioE;
                },
                p => p.AddWithValue("@idExpendio", idExpendio)
            );

            return list.Count > 0 ? list[0] : null;
        }

        public List<Entidades.LineaVenta> obtenerLineasExpendio(int idExpendio)
        {
            var oCorteD = new Datos.Corte(_empresa, _param);

            const string sql = "SELECT * FROM LineaExpendio WHERE idExpendio = @idExpendio;";

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr =>
                {
                    return new Entidades.LineaVenta
                    {
                        IdLineaVenta = Convert.ToInt32(dr["idLineaExpendio"]),
                        Corte = oCorteD.findCorteById(Convert.ToInt32(dr["idCorte"]), false),
                        CantKg = dr["cantKg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["cantKg"]),
                        PrecioKg = dr["precioKg"] == DBNull.Value ? 0 : Convert.ToSingle(dr["precioKg"]),
                        PesoBalanza = dr["pesoBalanza"] != DBNull.Value && Convert.ToBoolean(dr["pesoBalanza"])
                    };
                },
                p => p.AddWithValue("@idExpendio", idExpendio)
            );
        }

        #endregion

        #region FACTURA ELECTRONICA

        public int esVentaSinFacturar(int idVenta, bool esNotaCredito)
        {
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

            object scalar = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@idVenta", idVenta)
            );

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        public int existeFacturaElect(int idVenta)
        {
            const string sql = "SELECT TOP 1 id FROM FacturaElectronica WHERE CAE <> '' AND idVenta = @idVenta ORDER BY id DESC;";

            object scalar = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                p => p.AddWithValue("@idVenta", idVenta)
            );

            return (scalar == null || scalar == DBNull.Value) ? 0 : Convert.ToInt32(scalar);
        }

        public void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE)
        {
            // Mantengo la idea de hacer todo con la MISMA conexión
            using (SqlConnection con = Db.Open(_empresa))
            {
                // Cabecera
                using (SqlCommand cmd = new SqlCommand("addOrEditFacturaElectronica", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = Conexion.timeOut;

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
                        cmdA.CommandTimeout = Conexion.timeOut;

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

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr =>
                {
                    return new Entidades.FacturaElectronica
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
                },
                p => p.AddWithValue("@id", idFactuElec)
            );

            if (list.Count == 0) return null;

            var fact = list[0];

            // Relacionados (sin reader abierto)
            fact.ListaAlicuota = getAlicuotaIvaFactura(fact.Id);
            fact.Venta = getVentaById(fact.IdVenta);

            return fact;
        }

        public List<Entidades.AlicuotaIva> getAlicuotaIvaFactura(int idFacturaElectronica)
        {
            const string sql = @"
                SELECT a.idIva, ai.iva, a.baseImponible, a.importe
                FROM dbo.AlicuotaIvaPorFactura a
                INNER JOIN dbo.AlicuotasIva ai ON a.idIva = ai.idIva
                WHERE a.idFacturaElectronica = @idFacturaElectronica;";

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => new Entidades.AlicuotaIva
                {
                    IdIva = Convert.ToInt32(dr["idIva"]),
                    Iva = dr["iva"] == DBNull.Value ? 0 : Convert.ToSingle(dr["iva"]),
                    BaseImponible = dr["baseImponible"] == DBNull.Value ? 0 : Convert.ToSingle(dr["baseImponible"]),
                    Importe = dr["importe"] == DBNull.Value ? 0 : Convert.ToSingle(dr["importe"])
                },
                p => p.AddWithValue("@idFacturaElectronica", idFacturaElectronica)
            );
        }

        #endregion
    }
}
