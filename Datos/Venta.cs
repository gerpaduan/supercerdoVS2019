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
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "agregarVenta";

            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaVenta",oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
            cmVenta.Parameters.AddWithValue("@vendedor", oVentaE.Vendedor);
            cmVenta.Parameters.AddWithValue("@turno",oVentaE.Turno);
            cmVenta.Parameters.AddWithValue("@diaFestivo",oVentaE.DiaFestivo);
            cmVenta.Parameters.AddWithValue("@observaciones",oVentaE.Observaciones);
            cmVenta.Parameters.AddWithValue("@idPersona",oVentaE.Persona.idPersona);
            cmVenta.Parameters.AddWithValue("@nroRemito",oVentaE.NroRemito);


            SqlDataReader drVenta = cmVenta.ExecuteReader();

            int idVenta = 0;
            while (drVenta.Read())
            {
                idVenta = Convert.ToInt32(drVenta["idVenta"].ToString());
            }

            cmVenta.Connection.Close();
            cmVenta = null;

            return idVenta;

        }

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.StoredProcedure;
            /// Se eliminan todas las LineaVenta, y se actualiza datos de Venta
            cmVenta.CommandText = "modificarVenta";

            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);
            cmVenta.Parameters.AddWithValue("@fechaVenta", oVentaE.FechaVenta);
            cmVenta.Parameters.AddWithValue("@idSucursal", SucAnterior);
            cmVenta.Parameters.AddWithValue("@idSucNueva", oVentaE.Sucursal.idSucursal);
            cmVenta.Parameters.AddWithValue("@tipoVenta", oVentaE.TipoVenta);
            cmVenta.Parameters.AddWithValue("@vendedor", oVentaE.Vendedor);
            cmVenta.Parameters.AddWithValue("@turno", oVentaE.Turno);
            cmVenta.Parameters.AddWithValue("@diaFestivo", oVentaE.DiaFestivo);
            cmVenta.Parameters.AddWithValue("@observaciones", oVentaE.Observaciones);
            cmVenta.Parameters.AddWithValue("@idPersona", oVentaE.Persona.idPersona);
            cmVenta.Parameters.AddWithValue("@nroRemito", oVentaE.NroRemito);
            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);

            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();

            cmVenta = null;

        }


        public DataTable obtenerVentas(string sucursal, DateTime fechaDesde, DateTime fechaHasta, string texto)
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
            cmVenta.Parameters.AddWithValue("@sucursal", sucursal);

            cmVenta.ExecuteNonQuery();

            cmVenta.Connection.Close();

            daVenta.SelectCommand = cmVenta;

            daVenta.Fill(dtVentas);

            daVenta = null;
            cmVenta = null;

            return dtVentas;
        }

        public float obtenerTotalVentas(int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            DataTable dtVentas = new DataTable();
            daVenta = new SqlDataAdapter();

            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "obtenerTotalVentas";
            cmVenta.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmVenta.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmVenta.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            cmVenta.ExecuteNonQuery();

            cmVenta.Connection.Close();

            daVenta.SelectCommand = cmVenta;

            daVenta.Fill(dtVentas);

            daVenta = null;
            cmVenta = null;

            return float.Parse(dtVentas.Rows[0]["totalS"].ToString());
        }

        public void agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "agregarLineaVenta";

            cmVenta.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
            cmVenta.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
            cmVenta.Parameters.AddWithValue("@cantKg", oLineaE.CantKg);
            cmVenta.Parameters.AddWithValue("@precioKg", oLineaE.PrecioKg);

            cmVenta.ExecuteNonQuery();

            cmVenta.Connection.Close();

            cmVenta=null;
        }

        public void modificarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "modificarLineaVenta";

            cmVenta.Parameters.AddWithValue("@idVenta", oLineaE.Venta.IdVenta);
            cmVenta.Parameters.AddWithValue("@idCorte", oLineaE.Corte.idCorte);
            cmVenta.Parameters.AddWithValue("@pesoBalanza", oLineaE.PesoBalanza);
            cmVenta.Parameters.AddWithValue("@idAnulado", oLineaE.Estado);
            cmVenta.Parameters.AddWithValue("@cantKg", oLineaE.CantKg);
            cmVenta.Parameters.AddWithValue("@precioKg", oLineaE.PrecioKg);

            cmVenta.ExecuteNonQuery();

            cmVenta.Connection.Close();

            cmVenta = null;
        }


        public List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
        {
            //DataTable dtLineasVenta = new DataTable();

            daVenta = new SqlDataAdapter();
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();
            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "obtenerLineasVenta";
            cmVenta.Parameters.AddWithValue("@idVenta", idVenta);

            //creo lista de Lineas
            List<Entidades.LineaVenta> listaLineasVenta = new List<Entidades.LineaVenta>();

            try
            {
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
                listaLineasVenta = null;
            }
            //cmVenta.ExecuteNonQuery();

            //daVenta.SelectCommand = cmVenta;
            //daVenta.Fill(dtLineasVenta);

            daVenta = null;
            cmVenta = null;

            //return dtLineasVenta;
        }


        public void agregarStockVenta(Entidades.Venta oVentaE)
        {
            cmVenta = new SqlCommand();

            cmVenta.Connection = conn.conectar();
            cmVenta.Connection.Open();

            cmVenta.CommandType = CommandType.StoredProcedure;
            cmVenta.CommandText = "agregarStockVenta";

            cmVenta.Parameters.AddWithValue("@idVenta", oVentaE.IdVenta);           
            cmVenta.Parameters.AddWithValue("@estado", oVentaE.Estado);

            cmVenta.ExecuteNonQuery();
            cmVenta.Connection.Close();

            cmVenta = null;

        }

    }
}
