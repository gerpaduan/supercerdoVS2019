using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using static System.Collections.Specialized.BitVector32;
using System.Collections;

namespace Datos
{
    public class Venta
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlCommand cmVenta;
        SqlDataAdapter daVenta;

        public int agregarVenta(Entidades.Venta oVentaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; 
            cmVenta.CommandTimeout = 90;            
            cmVenta.CommandText = "agregarVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaVenta",oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
            cmVenta.Parameters.AddWithValue("@turno",oVentaE.Turno == null ? "" : oVentaE.Turno);
            cmVenta.Parameters.AddWithValue("@diaFestivo",oVentaE.DiaFestivo);
            cmVenta.Parameters.AddWithValue("@observaciones",oVentaE.Observaciones);
            cmVenta.Parameters.AddWithValue("@idPersona",oVentaE.Persona.idPersona);
            cmVenta.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito);
            cmVenta.Parameters.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
            cmVenta.Parameters.AddWithValue("@formaPago", oVentaE.FormaPago);
            cmVenta.Parameters.AddWithValue("@cuit", oVentaE.Cuit);
            cmVenta.Parameters.AddWithValue("@email", oVentaE.Email);
            cmVenta.Parameters.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
            cmVenta.Parameters.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
            cmVenta.Parameters.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
            cmVenta.Parameters.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);

            cmVenta.Connection.Open();
            SqlDataReader drVenta = cmVenta.ExecuteReader();
            int idVenta = 0;
            while (drVenta.Read())
            {
                idVenta = Convert.ToInt32(drVenta["idVenta"].ToString());
            }

            cmVenta.Connection.Close();
            return idVenta;
        }

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior, bool eliminarLineas)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            /// Se eliminan todas las LineaVenta, y se actualiza datos de Venta
            /// 
            /// -Si tiene egreso de caja por venta cta cte se genera un registro opuesto.
            /// 
            cmVenta.CommandText = "modificarVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", SucAnterior);
            cmVenta.Parameters.AddWithValue("@idSucNueva", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
            cmVenta.Parameters.AddWithValue("@turno", oVentaE.Turno == null ? "" : oVentaE.Turno);
            cmVenta.Parameters.AddWithValue("@diaFestivo", oVentaE.DiaFestivo);
            cmVenta.Parameters.AddWithValue("@observaciones", oVentaE.Observaciones);
            cmVenta.Parameters.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
            cmVenta.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito);
            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);
            cmVenta.Parameters.AddWithValue("@eliminarLineas", eliminarLineas);
            cmVenta.Parameters.AddWithValue("@enCtaCte", oVentaE.EnCtaCte);
            cmVenta.Parameters.AddWithValue("@formaPago", oVentaE.FormaPago);
            cmVenta.Parameters.AddWithValue("@cuit", oVentaE.Cuit);
            cmVenta.Parameters.AddWithValue("@email", oVentaE.Email);
            cmVenta.Parameters.AddWithValue("@tipoComprobante", oVentaE.TipoComprobante);
            cmVenta.Parameters.AddWithValue("@acumRedondeoKgs", oVentaE.AcumRedondeoKgs);
            cmVenta.Parameters.AddWithValue("@acumRedondeoImporte", oVentaE.AcumRedondeoImporte);
            cmVenta.Parameters.AddWithValue("@comisionTarjeta", oVentaE.ComisionTarjeta);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
            cmVenta = null;
        }

        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText="obtenerVentas";
            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmVenta.Parameters.AddWithValue("@texto", texto);
            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
            cmVenta.Parameters.AddWithValue("@idCliente", idCliente);
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmVenta.Parameters.AddWithValue("@soloAnulados", soloAnulados);

            cmVenta.ExecuteNonQuery();
            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtVentas);
            cmVenta.Connection.Close();

            return dtVentas;
        }

        public DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados)
        {
            DataTable dtVentasVendedorCierre = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "ventasVendedorCierreCaja";
            cmVenta.Parameters.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
            cmVenta.Parameters.AddWithValue("@fechaDesde", oCierreE.FechaHoraInicio);
            cmVenta.Parameters.AddWithValue("@fechaHasta", oCierreE.FechaHoraCierre == null ? DateTime.Now : oCierreE.FechaHoraCierre);
            //cmVenta.Parameters.AddWithValue("@texto", texto);
            cmVenta.Parameters.AddWithValue("@idSucursal", oCierreE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@soloAnulados", soloAnulados);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtVentasVendedorCierre);
            cmVenta.Connection.Close();

            return dtVentasVendedorCierre;
        }

        public float getTotalVenta(int idVenta)
        {
            DataTable dtTotalVenta = new DataTable();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            string consulta = "SELECT SUM(cantKg * precioKg) AS total "+
                                "FROM dbo.LineaVenta "+
                                "WHERE     idVenta = "+idVenta+" "+
                                "GROUP BY idVenta";
            cmVenta.CommandText = consulta;
            cmVenta.CommandType = CommandType.Text;
            cmVenta.Connection.Open();
            double totalVentaD = (double)cmVenta.ExecuteScalar();
            float totalVenta = (float)totalVentaD;
            cmVenta.Connection.Close();
            return totalVenta;
        }

        public float getTotalKgsVenta(int idVenta)
        {
            DataTable dtTotalVenta = new DataTable();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            string consulta = "SELECT SUM(cantKg) AS totalKgsVenta " +
                                "FROM dbo.LineaVenta " +
                                "WHERE     idVenta = " + idVenta + " " +
                                "GROUP BY idVenta";
            cmVenta.CommandText = consulta;
            cmVenta.CommandType = CommandType.Text;
            cmVenta.Connection.Open();
            double totalKgsVentaD = (double)cmVenta.ExecuteScalar();
            float totalKgsVenta = (float)totalKgsVentaD;
            cmVenta.Connection.Close();
            return totalKgsVenta;
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "obtenerTotalVentas";
            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtVentas);

            float totalVentas = dtVentas.Rows[0]["totalS"].ToString().Equals("") ? 0 : float.Parse(dtVentas.Rows[0]["totalS"].ToString());
            return totalVentas;
        }

        public Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "agregarLineaVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
            cmVenta.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
            cmVenta.Parameters.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
            cmVenta.Parameters.AddWithValue("@idAlicuotaIva", oLineaE.Corte.IdAlicuotaIva);
            cmVenta.Parameters.AddWithValue("@alicuotaIva", oLineaE.Corte.AlicuotaIva);
            cmVenta.Parameters.AddWithValue("@kgsAjusteTarj", Math.Round(oLineaE.KgsAjusteTarj, 3));
            cmVenta.Parameters.AddWithValue("@porcKgsAjusteTarj", oLineaE.CantKg == 0 ? 0 : Math.Round(oLineaE.KgsAjusteTarj / oLineaE.CantKg, 3));
            cmVenta.Parameters.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));
            cmVenta.Parameters.AddWithValue("@ajustePrecio", Math.Round(oLineaE.AjustePrecio, 2));
            cmVenta.Parameters.AddWithValue("@bonificacion", oLineaE.Bonificacion);
            cmVenta.Parameters.AddWithValue("@idLineaVentaAnulado", oLineaE.IndexAnulado);

            cmVenta.Connection.Open();
            oLineaE.IdLineaVenta = (int)cmVenta.ExecuteScalar();
            cmVenta.Connection.Close();

            return oLineaE;
        }

        public Entidades.Venta getVentaById(int idVenta)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandText = "Select Ventas.* from Ventas where idVenta =" + idVenta;

            Entidades.Venta oVentaE = new Entidades.Venta();

            try
            {
                cmVenta.Connection.Open();
                SqlDataReader drVenta = cmVenta.ExecuteReader();

                using (drVenta)
                {
                    while (drVenta.Read())
                    {
                        oVentaE.IdVenta = Convert.ToInt32(drVenta["idVenta"]);
                        Datos.Usuario oUsuarioD = new Usuario();
                        oVentaE.Vendedor = oUsuarioD.getUsuarioById(Convert.ToInt32(drVenta["idVendedor"]));
                        oVentaE.FechaVenta = Convert.ToDateTime(drVenta["fechaVenta"]);
                        oVentaE.Turno = Convert.ToString(drVenta["turno"]);
                        Datos.Sucursal oSucursalD = new Sucursal();
                        oVentaE.Sucursal = oSucursalD.findById(Convert.ToInt32(drVenta["idSucursal"]));
                        oVentaE.DiaFestivo = Convert.ToString(drVenta["diaFestivo"]);
                        oVentaE.Observaciones = Convert.ToString(drVenta["observaciones"]);
                        Datos.Persona oPersonaD = new Datos.Persona();
                        oVentaE.Persona = oPersonaD.findById(Convert.ToInt32(drVenta["idPersona"]));
                        oVentaE.NroRemito = Convert.ToString(drVenta["nroRemito"]);
                        oVentaE.Estado = Convert.ToString(drVenta["estado"]);
                        oVentaE.EnCtaCte = Convert.ToBoolean(drVenta["enCtaCte"]);
                        oVentaE.Cuit = Convert.ToString(drVenta["cuit"]);
                        oVentaE.Email = Convert.ToString(drVenta["email"]);
                        oVentaE.Cuit = Convert.ToString(drVenta["cuit"]);
                        oVentaE.FormaPago = Convert.ToString(drVenta["formaPago"]);
                        oVentaE.TipoComprobante = Convert.ToChar(drVenta["tipoComprobante"]);
                        oVentaE.Creado = Convert.ToDateTime(drVenta["creado"]);
                        oVentaE.Actualizado = drVenta["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drVenta["actualizado"]);
                        
                        oVentaE.LineasVenta = obtenerLineasVenta(oVentaE.IdVenta);
                        oVentaE.TotalImporte = getTotalVenta(idVenta);
                    }
                    return oVentaE;
                }
            }
            finally
            {
                cmVenta.Connection.Close();
                oVentaE = null;
            }
        }

        public Entidades.Venta getUltimaVentaVendedor(int idVendedor)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandText = "Select top 1 Ventas.* from Ventas where idVendedor = " + idVendedor + " order by idVenta desc";

            Entidades.Venta oVentaE = new Entidades.Venta();
            try
            {
                cmVenta.Connection.Open();
                SqlDataReader drVenta = cmVenta.ExecuteReader();
                using (drVenta)
                {
                    while (drVenta.Read())
                    {
                        oVentaE = getVentaById(Convert.ToInt32(drVenta["idVenta"]));
                    }
                    return oVentaE;
                }
            }
            finally
            {
                cmVenta.Connection.Close();
                oVentaE = null;
            }
        }

        public List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
        {
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "obtenerLineasVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", idVenta);

            Datos.Corte oCorteD = new Datos.Corte();    
            //creo lista de Lineas
            List<Entidades.LineaVenta> listaLineasVenta = new List<Entidades.LineaVenta>();
            try
            {
                cmVenta.Connection.Open();
                SqlDataReader drLinea = cmVenta.ExecuteReader();
                using (drLinea)
                {
                    while (drLinea.Read())
                    {
                        Entidades.LineaVenta oLinea = new Entidades.LineaVenta();

                        oLinea.IdLineaVenta = Convert.ToInt32(drLinea["idLineaVenta"]);
                        //se crea y asiga la venta
                        Entidades.Venta oVenta=new Entidades.Venta();
                        oVenta.IdVenta= Convert.ToInt32(drLinea["idVenta"]);

                        oLinea.Venta=oVenta;

                        oLinea.Corte = oCorteD.getCorteById(Convert.ToInt32(drLinea["idCorte"]), false); //oCorte;

                        oLinea.CantKg = float.Parse(drLinea["cantKg"].ToString());
                        oLinea.IdAlicuotaIva = Convert.ToInt32(drLinea["idAlicuotaIva"]);
                        oLinea.AlicuotaIva = float.Parse(drLinea["alicuotaIva"].ToString());
                        oLinea.PrecioKg = float.Parse(drLinea["precioKg"].ToString());
                        oLinea.KgsAjusteTarj = float.Parse(drLinea["kgsAjusteTarj"].ToString());
                        oLinea.Bonificacion = string.IsNullOrEmpty(drLinea["bonificacion"].ToString()) ? 0 : float.Parse(drLinea["bonificacion"].ToString());
                        oLinea.IndexAnulado = DBNull.Value.Equals(drLinea["idLineaVentaAnulado"]) ? -1 : Convert.ToInt32(drLinea["idLineaVentaAnulado"].ToString());
                        
                        try
                        {
                            oLinea.PesoBalanza = Convert.ToBoolean(drLinea["pesoBalanza"]);
                        }
                        catch (Exception)
                        {
                            oLinea.PesoBalanza = false;
                        }

                        if (drLinea["estado"].ToString()=="")
                        {
                            oLinea.Estado = 0;
                        }
                        else
                        {
                            oLinea.Estado = 1;
                        }

                        listaLineasVenta.Add(oLinea);

                        oVenta = null;
                        oLinea = null;
                    }
                    return listaLineasVenta;
                }
            }
            finally
            {
                cmVenta.Connection.Close();
                listaLineasVenta = null;
            }
        }

        public void agregarStockVenta(Entidades.Venta oVentaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "agregarStockVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
        }

        public void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandText = "insert into TemporalLineaVenta (idVendedor, fechaInicioPesada, idCorte, cantKg, precioKg, totalCorte, ventaEnCurso, idSucursal, creado) values " +
                "(" + oTemporalLV.Vendedor.Id + ", @fechaInicioPesada," + oTemporalLV.Corte.idCorte +
                ",@cantKg,@precioKg,@totalCorte, @ventaEnCurso, @idSucursal, @creado)";
            cmVenta.Parameters.Add("@fechaInicioPesada", SqlDbType.DateTime2).Value = oTemporalLV.FechaInicioPesada;
            cmVenta.Parameters.Add("@cantKg", SqlDbType.Decimal).Value = oTemporalLV.CantKg;
            cmVenta.Parameters.Add("@precioKg", SqlDbType.Decimal).Value = oTemporalLV.Corte.PrecioKg;
            cmVenta.Parameters.Add("@totalCorte", SqlDbType.Decimal).Value = oTemporalLV.TotalCorte;
            cmVenta.Parameters.Add("@ventaEnCurso", SqlDbType.TinyInt).Value = oTemporalLV.VentaEnCurso;
            cmVenta.Parameters.Add("@idSucursal", SqlDbType.TinyInt).Value = oTemporalLV.Sucursal.idSucursal;
            cmVenta.Parameters.Add("@creado", SqlDbType.DateTime2).Value = DateTime.Now;
            try
            {
                cmVenta.Connection.Open();
                cmVenta.ExecuteNonQuery();
            }
            finally
            {
                cmVenta.Connection.Close();
            }
        }

        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "obtenerTemporalLineaVenta";
            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmVenta.Parameters.AddWithValue("@texto", texto);
            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmVenta.Parameters.AddWithValue("@conVentas", conVentas);

            cmVenta.ExecuteNonQuery();
            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtVentas);
            cmVenta.Connection.Close();

            return dtVentas;
        }

        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "getAllLineasVenta";
            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmVenta.Parameters.AddWithValue("@texto", texto);
            cmVenta.Parameters.AddWithValue("@idVendedor", idVendedor);
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);

            cmVenta.ExecuteNonQuery();
            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtVentas);
            cmVenta.Connection.Close();

            return dtVentas;
        }

        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "ultimasVentasCliente";
            cmVenta.Parameters.AddWithValue("@idPersona", idPersona);
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);

            cmVenta.ExecuteNonQuery();
            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtVentas);
            cmVenta.Connection.Close();

            return dtVentas;
        }

        /// <summary>
        /// Actualiza el campo tipo comprobante en la tabla Venta. 
        /// Si no se factura debe guardarse X (remito). Para llevar control de lo facturado
        /// </summary>
        /// <param name="letraId_tipoCbte"></param>
        public void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.Text; 
            cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "UPDATE Ventas SET tipoComprobante = @tipoComprobante WHERE idVenta = " + idVenta;
            cmVenta.Parameters.AddWithValue("@tipoComprobante", letraId_tipoCbte);

            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();

            cmVenta = null;
        }
        public void actualizarCliente(int idVenta, int idPersona)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "UPDATE Ventas SET idPersona = @idPersona WHERE idVenta = " + idVenta;
            cmVenta.Parameters.AddWithValue("@idPersona", idPersona);

            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();

            cmVenta = null;
        }

        #region EXPENDIO
        public int agregarExpendio(Entidades.Venta oVentaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "agregarExpendio"; 

            cmVenta.Parameters.AddWithValue("@idExpendio", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaExpendio", oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
            cmVenta.Parameters.AddWithValue("@identificacionExpendio", oVentaE.IdentificacionExpendio);
            cmVenta.Parameters.AddWithValue("@sector", oVentaE.Sector);
            cmVenta.Parameters.AddWithValue("@cantItems", oVentaE.CantItems);
            cmVenta.Parameters.AddWithValue("@importe", oVentaE.TotalImporte);
            cmVenta.Parameters.AddWithValue("@serialCPU", oVentaE.SerialCPU);

            cmVenta.Connection.Open();
            SqlDataReader drVenta = cmVenta.ExecuteReader();
            int idVenta = 0;
            while (drVenta.Read())
            {
                idVenta = Convert.ToInt32(drVenta["idExpendio"].ToString());
            }

            cmVenta.Connection.Close();
            return idVenta;
        }

        public Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure; cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "agregarLineaExpendio";
            cmVenta.Parameters.AddWithValue("@idExpendio", oLineaE.Venta.IdVenta);
            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
            cmVenta.Parameters.AddWithValue("@cantKg", Math.Round(oLineaE.CantKg, 3));
            cmVenta.Parameters.AddWithValue("@precioKg", Math.Round(oLineaE.PrecioKg, 2));

            cmVenta.Connection.Open();
            oLineaE.IdLineaVenta = (int)cmVenta.ExecuteScalar();
            cmVenta.Connection.Close();

            return oLineaE;
        }

        public void asignarVentaEnExpendio(int idVenta, int idExpendio)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandTimeout = 90;
            cmVenta.CommandText = "UPDATE Expendios SET idVenta = @idVenta WHERE idExpendio = " + idExpendio;
            cmVenta.Parameters.AddWithValue("@idVenta", idVenta);
            cmVenta.Parameters.AddWithValue("@idExpendio", idExpendio);

            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();

            cmVenta = null;
        }
        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal)
        {
            DataTable dtSectores = new DataTable();
            daVenta = new SqlDataAdapter();

            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text; cmVenta.CommandTimeout = 90;
            DateTime fechaDesde = DateTime.Now.AddMinutes(-ultimosMinutos); 
            string consulta = "SELECT fechaExpendio, dbo.Expendios.idExpendio as idExpendio, identificacionExpendio, sector, dbo.Corte.codigo as codigo, dbo.Corte.corte as corte, dbo.LineaExpendio.cantKg as cantKg, dbo.LineaExpendio.precioKg as precioKg, (dbo.LineaExpendio.cantKg * dbo.LineaExpendio.precioKg) as total, idVenta, dbo.Usuarios.nombre as vendedor " +
                "FROM dbo.Expendios INNER JOIN dbo.LineaExpendio ON dbo.Expendios.idExpendio = dbo.LineaExpendio.idExpendio INNER JOIN dbo.Corte ON dbo.LineaExpendio.idCorte = dbo.Corte.idCorte "+
                "INNER JOIN dbo.Usuarios ON dbo.Expendios.idVendedor = dbo.Usuarios.id WHERE fechaExpendio > @fechaDesde AND idSucursal = @idSucursal ORDER BY fechaExpendio ;";
            cmVenta.CommandText = consulta;
            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);

            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtSectores);

            cmVenta.Connection.Close();

            return dtSectores;
        }

        public DataTable obtenerSectores()
        {
            DataTable dtSectores = new DataTable();
            daVenta = new SqlDataAdapter();

            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text; cmVenta.CommandTimeout = 90;
            string consulta = "SELECT  sector FROM  Sectores";
            cmVenta.CommandText = consulta;

            daVenta.SelectCommand = cmVenta;
            daVenta.Fill(dtSectores);

            cmVenta.Connection.Close();

            return dtSectores;
        }

        public string getUltimoSectorSelect(string serialCPU)
        {
            string query = $"SELECT sector FROM Licencias WHERE nroLicencia = '{serialCPU}'";
            string sector = "";
            // Conexión a la base de datos
            using (SqlConnection connection = conn.conectar())
            {
                // Abrir conexión
                connection.Open();

                // Crear comando
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // Ejecutar el comando y leer los datos
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Obtener el valor de la columna "sector"
                            sector = reader["sector"] != DBNull.Value ? reader["sector"].ToString().Trim() : string.Empty;
                        }
                    }
                }
            }
            return sector;
        }

        #endregion


        #region FACTURA ELECTRONICA        

        /// <summary>
        /// Pasando el idVenta busca en tabla Factura electronica. Si CAE is null -> Pendiente Facturacion.
        /// Se Retorna Cero si está pendiente.
        /// </summary>
        /// <param name="idVenta"></param>
        /// <returns></returns>
        public int esVentaSinFacturar(int idVenta)
        {
            int maxDiasParaFacturar = 6;
            int idFactElec = 0;
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text;
            //cmVenta.CommandText = "Select TOP(1) id from FacturaElectronica where fechaEmisionAfip > @fechaEmisionAfip and idVenta = \'" +
            //    idVenta.ToString() + "\' and CAE is not null ORDER BY id desc";
            cmVenta.CommandText = "Select TOP(1) id from FacturaElectronica where idVenta = \'" +
                idVenta.ToString() + "\' and CAE is not null ORDER BY id desc";
            cmVenta.Parameters.Add("@fechaEmisionAfip", SqlDbType.DateTime2).Value = DateTime.Today.AddDays(-maxDiasParaFacturar);
            try
            {
                cmVenta.Connection.Open();
                SqlDataReader drVenta = cmVenta.ExecuteReader();
                using (drVenta)
                {
                    while (drVenta.Read())
                    {
                        idFactElec = Convert.ToInt32(drVenta["id"]);
                    }
                    return idFactElec;
                }
            }
            finally
            {
                cmVenta.Connection.Close();
            }
        }

        public int existeFacturaElect(int idVenta)
        {
            int idFactElec = 0;
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandText = "Select id from FacturaElectronica where and idVenta = " + idVenta;
            try
            {
                cmVenta.Connection.Open();
                SqlDataReader drVenta = cmVenta.ExecuteReader();
                using (drVenta)
                {
                    while (drVenta.Read())
                    {
                        idFactElec = Convert.ToInt32(drVenta["id"]);
                    }
                    return idFactElec;
                }
            }
            finally
            {
                cmVenta.Connection.Close();
            }
        }

        public void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "addOrEditFacturaElectronica";
            cmVenta.Parameters.AddWithValue("@id", oFacturaElectronicaE.Id);
            cmVenta.Parameters.AddWithValue("@ptoVtaAfip", oFacturaElectronicaE.PtoVtaAfip);
            cmVenta.Parameters.AddWithValue("@fechaEmisionAfip", oFacturaElectronicaE.FechaEmisionAfip < DateTime.Today.AddYears(-100) ?
                (DateTime?)null : oFacturaElectronicaE.FechaEmisionAfip);
            cmVenta.Parameters.AddWithValue("@descTipoCbteAfip", oFacturaElectronicaE.DescTipoCbteAfip);
            cmVenta.Parameters.AddWithValue("@codTipoCbteAfip", oFacturaElectronicaE.CodTipoCbteAfip);
            cmVenta.Parameters.AddWithValue("@nroCbteAfip", oFacturaElectronicaE.NroCbteAfip);
            cmVenta.Parameters.AddWithValue("@tipoDocAfip", oFacturaElectronicaE.TipoDocAfip);
            cmVenta.Parameters.AddWithValue("@nroDocAfip", oFacturaElectronicaE.NroDocAfip);
            cmVenta.Parameters.AddWithValue("@razonSocialAFIP", oFacturaElectronicaE.RazonSocialAFIP);
            cmVenta.Parameters.AddWithValue("@condicionIvaAFIP", oFacturaElectronicaE.CondicionIvaAFIP);
            cmVenta.Parameters.AddWithValue("@domicilioAFIP", oFacturaElectronicaE.DomicilioAFIP);
            cmVenta.Parameters.AddWithValue("@condicionVenta", oFacturaElectronicaE.CondicionVenta);
            cmVenta.Parameters.AddWithValue("@formaPago", oFacturaElectronicaE.FormaPago);
            cmVenta.Parameters.AddWithValue("@CAE", oFacturaElectronicaE.CAE1);
            cmVenta.Parameters.AddWithValue("@fecVtoCAE", oFacturaElectronicaE.FecVtoCAE);
            cmVenta.Parameters.AddWithValue("@importeNetoGravado", oFacturaElectronicaE.ImporteNetoGravado);
            cmVenta.Parameters.AddWithValue("@iva", oFacturaElectronicaE.Iva);
            cmVenta.Parameters.AddWithValue("@importeTotal", oFacturaElectronicaE.ImporteTotal);
            cmVenta.Parameters.AddWithValue("@idVenta", oFacturaElectronicaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@error", oFacturaElectronicaE.Error);
            cmVenta.Parameters.AddWithValue("@mensajeError", oFacturaElectronicaE.MensajeError);
            cmVenta.Parameters.AddWithValue("@fechaError", oFacturaElectronicaE.FechaError.Equals(null) || oFacturaElectronicaE.FechaError < DateTime.Today.AddYears(-100) ? 
                (DateTime?)null : oFacturaElectronicaE.FechaError);

            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
        }

        public Entidades.FacturaElectronica getFactuElecById(int idFactuElec)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.Text;
            cmVenta.CommandText = "Select FacturaElectronica.* from FacturaElectronica where id =" + idFactuElec;

            Entidades.FacturaElectronica oFacturaElectronicaE = new Entidades.FacturaElectronica();

            try
            {
                cmVenta.Connection.Open();
                SqlDataReader drFactuElec = cmVenta.ExecuteReader();

                using (drFactuElec)
                {
                    while (drFactuElec.Read())
                    {
                        oFacturaElectronicaE.Id = Convert.ToInt32(drFactuElec["id"]);
                        oFacturaElectronicaE.PtoVtaAfip = Convert.ToString(drFactuElec["ptoVtaAfip"]);
                        oFacturaElectronicaE.FechaEmisionAfip = drFactuElec["fechaEmisionAfip"].Equals(DBNull.Value) ? null : (DateTime?)(drFactuElec["fechaEmisionAfip"]);
                        oFacturaElectronicaE.DescTipoCbteAfip = Convert.ToString(drFactuElec["descTipoCbteAfip"]);
                        oFacturaElectronicaE.CodTipoCbteAfip = Convert.ToInt32(drFactuElec["codTipoCbteAfip"]);
                        oFacturaElectronicaE.NroCbteAfip = Convert.ToString(drFactuElec["nroCbteAfip"]);
                        oFacturaElectronicaE.TipoDocAfip = Convert.ToString(drFactuElec["tipoDocAfip"]);
                        oFacturaElectronicaE.NroDocAfip = Convert.ToString(drFactuElec["NroDocAfip"]);
                        oFacturaElectronicaE.RazonSocialAFIP = Convert.ToString(drFactuElec["razonSocialAFIP"]);
                        oFacturaElectronicaE.CondicionIvaAFIP = Convert.ToString(drFactuElec["condicionIvaAFIP"]);
                        oFacturaElectronicaE.DomicilioAFIP = Convert.ToString(drFactuElec["domicilioAFIP"]);
                        oFacturaElectronicaE.CondicionVenta = Convert.ToString(drFactuElec["condicionVenta"]);
                        oFacturaElectronicaE.FormaPago = Convert.ToString(drFactuElec["formaPago"]);
                        oFacturaElectronicaE.CAE1 = Convert.ToString(drFactuElec["CAE"]);
                        oFacturaElectronicaE.FecVtoCAE = Convert.ToString(drFactuElec["fecVtoCAE"]);
                        oFacturaElectronicaE.ImporteNetoGravado = string.IsNullOrEmpty((drFactuElec["importeNetoGravado"]).ToString()) ? 0 : float.Parse((drFactuElec["importeNetoGravado"]).ToString());
                        oFacturaElectronicaE.Iva = string.IsNullOrEmpty((drFactuElec["iva"]).ToString()) ? 0 : float.Parse((drFactuElec["iva"]).ToString());
                        oFacturaElectronicaE.ImporteTotal = string.IsNullOrEmpty((drFactuElec["importeTotal"]).ToString()) ? 0 : float.Parse((drFactuElec["importeTotal"]).ToString());
                        oFacturaElectronicaE.IdVenta = Convert.ToInt32(drFactuElec["idVenta"]);
                        oFacturaElectronicaE.Error = Convert.ToBoolean(drFactuElec["error"]);
                        oFacturaElectronicaE.MensajeError = Convert.ToString(drFactuElec["mensajeError"]);
                        oFacturaElectronicaE.FechaError = drFactuElec["fechaError"].Equals(DBNull.Value) ? null : (DateTime?)(drFactuElec["actualizado"]);
                    
                        oFacturaElectronicaE.Venta = getVentaById(oFacturaElectronicaE.IdVenta);
                    }
                    return oFacturaElectronicaE;
                }
            }
            finally
            {
                cmVenta.Connection.Close();
                oFacturaElectronicaE = null;
            }
        }
        #endregion
    }
}
