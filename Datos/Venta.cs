using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

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
            cmVenta.CommandText = "agregarVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaVenta",oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
            cmVenta.Parameters.AddWithValue("@turno",oVentaE.Turno);
            cmVenta.Parameters.AddWithValue("@diaFestivo",oVentaE.DiaFestivo);
            cmVenta.Parameters.AddWithValue("@observaciones",oVentaE.Observaciones);
            cmVenta.Parameters.AddWithValue("@idPersona",oVentaE.Persona.idPersona);
            cmVenta.Parameters.AddWithValue("@nroRemito",oVentaE.NroRemito);
            
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

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure;
            /// Se eliminan todas las LineaVenta, y se actualiza datos de Venta
            cmVenta.CommandText = "modificarVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", SucAnterior);
            cmVenta.Parameters.AddWithValue("@idSucNueva", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
            cmVenta.Parameters.AddWithValue("@idVendedor", oVentaE.Vendedor.Id);
            cmVenta.Parameters.AddWithValue("@turno", oVentaE.Turno);
            cmVenta.Parameters.AddWithValue("@diaFestivo", oVentaE.DiaFestivo);
            cmVenta.Parameters.AddWithValue("@observaciones", oVentaE.Observaciones);
            cmVenta.Parameters.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
            cmVenta.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito);
            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
            cmVenta = null;
        }

        public DataTable obtenerVentas(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
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
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "ventasVendedorCierreCaja";
            cmVenta.Parameters.AddWithValue("@idVendedor", oCierreE.UsuarioInicio.Id);
            cmVenta.Parameters.AddWithValue("@fechaDesde", oCierreE.FechaHoraInicio);
            cmVenta.Parameters.AddWithValue("@fechaHasta", oCierreE.FechaHoraCierre);
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

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure;
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

        public void agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "agregarLineaVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
            cmVenta.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
            cmVenta.Parameters.AddWithValue("@cantKg", oLineaE.CantKg);
            cmVenta.Parameters.AddWithValue("@precioKg", oLineaE.PrecioKg);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
        }

        public void modificarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            cmVenta = new SqlCommand();
            cmVenta.Connection = conn.conectar();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "modificarLineaVenta";

            cmVenta.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
            cmVenta.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
            cmVenta.Parameters.AddWithValue("@cantKg", oLineaE.CantKg);
            cmVenta.Parameters.AddWithValue("@precioKg", oLineaE.PrecioKg);

            cmVenta.Connection.Open();
            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();
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
                        oVentaE.Creado = Convert.ToDateTime(drVenta["creado"]);
                        oVentaE.Actualizado = drVenta["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drVenta["actualizado"]);

                        oVentaE.LineasVenta = obtenerLineasVenta(oVentaE.IdVenta);
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
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "obtenerLineasVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", idVenta);

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

                        //se crea y asiga la venta
                        Entidades.Venta oVenta=new Entidades.Venta();
                        oVenta.IdVenta= Convert.ToInt32(drLinea["idVenta"]);

                        oLinea.Venta=oVenta;

                        //se crea y asiga el corte
                        Entidades.Corte oCorte = new Entidades.Corte();
                        oCorte.idCorte = Convert.ToInt32(drLinea["idCorte"]);
                        oCorte.codigo = Convert.ToInt32(drLinea["codigo"]);
                        oCorte.corte = Convert.ToString(drLinea["corte"]);

                        oLinea.Corte = oCorte;

                        oLinea.CantKg = float.Parse(drLinea["cantKg"].ToString());
                        oLinea.PrecioKg = float.Parse(drLinea["precioKg"].ToString());

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
                        oCorte = null;
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
            cmVenta.CommandType = CommandType.StoredProcedure;
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
            cmVenta.CommandType = CommandType.StoredProcedure;
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
    }
}
