using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Corte
    {
        private SqlCommand cmCorte;
        Utilidades.Conexion conn=new Utilidades.Conexion();
        private SqlDataAdapter daCorte;
    
        public void agregarCorte(Entidades.Corte oCorteE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "agregarCorte";
            cmCorte.Parameters.AddWithValue("@codigo", oCorteE.codigo);
            cmCorte.Parameters.AddWithValue("@corte", oCorteE.corte);
            cmCorte.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
            cmCorte.Parameters.AddWithValue("@tipo", oCorteE.tipo);
            cmCorte.Parameters.AddWithValue("@independiente", oCorteE.independiente);
            cmCorte.Parameters.AddWithValue("@idCorteMaestro", oCorteE.corteMaestro.idCorte);
            cmCorte.Parameters.AddWithValue("@porcentaje", oCorteE.porcentaje);
            cmCorte.Parameters.AddWithValue("@porcentajeHueso", oCorteE.porcentajeHueso);
            cmCorte.Parameters.AddWithValue("@desvioEstandar", oCorteE.desvioEstandar);
 
            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;

        }

        public DataTable buscarCorte(string txtBusqueda)
        {
            DataTable dtCortes = new DataTable();
            
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "buscarCorte";
            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            return dtCortes;
        }
        
        public DataTable buscarCorteSinMaestro(string txtBusqueda)
        {
            DataTable dtCortes = new DataTable();
            
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "buscarCorteSinMaestro";
            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            return dtCortes;
        }

        public DataTable buscarCodigoCorte(int codigo)
        {
            DataTable dtCortes = new DataTable();

            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "buscarCodigoCorte";
            cmCorte.Parameters.AddWithValue("@codigo", codigo);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            return dtCortes;
        }

        public void modificarCorte(Entidades.Corte oCorteE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "modificarCorte";
            
            cmCorte.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);
            cmCorte.Parameters.AddWithValue("@codigo", oCorteE.codigo);
            cmCorte.Parameters.AddWithValue("@corte", oCorteE.corte);
            cmCorte.Parameters.AddWithValue("@tipo", oCorteE.tipo);
            cmCorte.Parameters.AddWithValue("@independiente", oCorteE.independiente);
            cmCorte.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
            cmCorte.Parameters.AddWithValue("@idCorteMaestro", oCorteE.corteMaestro.idCorte);
            cmCorte.Parameters.AddWithValue("@porcentaje", oCorteE.porcentaje);
            cmCorte.Parameters.AddWithValue("@porcentajeHueso", oCorteE.porcentajeHueso);
            cmCorte.Parameters.AddWithValue("@desvioEstandar", oCorteE.desvioEstandar);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "EliminarCorte";

            cmCorte.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public DataTable obtenerCortes()
        {
            DataTable dtCortes=new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "obtenerCortes";

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            cmCorte.Connection.Close();
            
            return dtCortes;

        }

        public DataTable obtenerEmbutidos(string txtBusqueda)
        {
            DataTable dtCortes = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "obtenerEmbutidos";
            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            cmCorte.Connection.Close();

            return dtCortes;

        }

        public DataTable buscarEmbutido(string sucursal,string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtCortes = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "buscarEmbutido";
            cmCorte.Parameters.AddWithValue("@sucursal", sucursal);
            cmCorte.Parameters.AddWithValue("@texto",texto);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            cmCorte.Connection.Close();

            return dtCortes;
        }

        public DataTable obtenerInfoCorte(int idCorte)
        {
            DataTable dtCorte = new DataTable();

            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "obtenerInfoCorte";
            cmCorte.Parameters.AddWithValue("@idCorte", idCorte);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCorte);

            cmCorte.Connection.Close();

            return dtCorte;
        }


        #region Embutidos

        public int agregarEmbutido(Entidades.Embutido oEmbutido)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "agregarEmbutido";
            cmCorte.Parameters.AddWithValue("@fechaEmbutido", oEmbutido.fechaEmbutido);
            cmCorte.Parameters.AddWithValue("@idCorte", oEmbutido.corte.idCorte);
            cmCorte.Parameters.AddWithValue("@idSucursal", oEmbutido.sucursal.IdSucursal);
            cmCorte.Parameters.AddWithValue("@observaciones", oEmbutido.observaciones);
            
            //cmCorte.ExecuteNonQuery();

            SqlDataReader drEmbutido=cmCorte.ExecuteReader();

            int idEmbutido=0;
            while (drEmbutido.Read())
            {
                idEmbutido =Convert.ToInt32( drEmbutido["idEmbutido"].ToString());// Convert.ToInt32();
                
            }

            cmCorte.Connection.Close();

            cmCorte = null;

            return idEmbutido;
        
        }

        public void anularEmbutido(Entidades.Embutido oEmbutidoE)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "anularEmbutido";
            cmCorte.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public DataTable obtenerCortesPorEmbutidos(Entidades.Embutido oEmbutidoE)
        {
            DataTable dtCortePorEmbutido = new DataTable();

            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "obtenerCortesPorEmbutidos";
            cmCorte.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortePorEmbutido);

            cmCorte.Connection.Close();

            return dtCortePorEmbutido;
        }

        public void agregarCortePorEmbutido(Entidades.CortePorEmbutido oCortePorEmbutido)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "agregarCortePorEmbutido";

            cmCorte.Parameters.AddWithValue("@idEmbutido", oCortePorEmbutido.embutido.idEmbutido);
            cmCorte.Parameters.AddWithValue("@idCorte", oCortePorEmbutido.corte.idCorte);
            cmCorte.Parameters.AddWithValue("@kgUtilizados", oCortePorEmbutido.kgUtilizado);
            cmCorte.Parameters.AddWithValue("@idSucursal", oCortePorEmbutido.embutido.sucursal.IdSucursal);
            cmCorte.Parameters.AddWithValue("@pesoBalanza", oCortePorEmbutido.PesoBalanza);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public void actualizarStockEmbutido(DataRow cortePorEmbutido, Entidades.Embutido oEmbutidoE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "actualizarStockEmbutido";

            cmCorte.Parameters.AddWithValue("@idEmbutido", cortePorEmbutido["idEmbutido"]);
            cmCorte.Parameters.AddWithValue("@idCorte", cortePorEmbutido["idCorte"]);
            cmCorte.Parameters.AddWithValue("@kgUtilizados", cortePorEmbutido["kgUtilizados"]);
            cmCorte.Parameters.AddWithValue("@idSucursal", oEmbutidoE.sucursal.idSucursal);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;
        }
        #endregion

        #region Movimiento

        public int agregarMovimiento(Entidades.Movimiento oMovimientoE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "agregarMovimiento";
            cmCorte.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
            cmCorte.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
            cmCorte.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
            cmCorte.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones);

            SqlDataReader drMovimiento = cmCorte.ExecuteReader();

            int idMovimiento = 0;

            while (drMovimiento.Read())
            {
                idMovimiento = Convert.ToInt32(drMovimiento["idMovimiento"].ToString());                
            }

            cmCorte.Connection.Close();

            cmCorte = null;

            return idMovimiento;
        }

        public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "modificarMovimiento";

            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
            cmCorte.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
            cmCorte.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
            cmCorte.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
            cmCorte.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones);

            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();
        }

        public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "agregarCortePorMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", cortePorMovimiento.Movimientos.IdMovimiento);
            cmCorte.Parameters.AddWithValue("@idCorte", cortePorMovimiento.Corte.IdCorte);
            cmCorte.Parameters.AddWithValue("@cantKg",cortePorMovimiento.CantKg );
            cmCorte.Parameters.AddWithValue("@pesoBalanza", cortePorMovimiento.PesoBalanza);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

        }

        public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "quitarCortesPorMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
           
            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

        }

        public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            DataTable dtMovimientos = new DataTable();

            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "obtenerMovimientos";
            cmCorte.Parameters.AddWithValue("@sucOrigen", sucOrigen);
            cmCorte.Parameters.AddWithValue("@sucDestino", sucDestino);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCorte.Parameters.AddWithValue("@texto", texto);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtMovimientos);

            return dtMovimientos;


        }

        

        public Entidades.Movimiento cargarMovimiento(int idMovimiento)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "cargarMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);

            SqlDataReader drMovimiento = cmCorte.ExecuteReader();

            Entidades.Movimiento oMovimiento = new Entidades.Movimiento();
            while (drMovimiento.Read())
            {
                oMovimiento.IdMovimiento = Convert.ToInt32(drMovimiento["idMovimiento"].ToString());
                oMovimiento.FechaMovimiento = Convert.ToDateTime(drMovimiento["fechaMovimiento"].ToString());

                Entidades.Sucursal origen = new Entidades.Sucursal();
                origen.idSucursal = Convert.ToInt32(drMovimiento["idOrigen"].ToString());
                origen.sucursal = drMovimiento["origen"].ToString();

                oMovimiento.SucursalOrigen = origen;

                Entidades.Sucursal destino = new Entidades.Sucursal();
                destino.idSucursal = Convert.ToInt32(drMovimiento["idDestino"].ToString());
                destino.sucursal = drMovimiento["destino"].ToString();

                oMovimiento.SucursalDestino = destino;

                oMovimiento.Observaciones = drMovimiento["observaciones"].ToString();
            }

            cmCorte.Connection.Close();

            return oMovimiento;
        }

        public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "cargarCortesPorMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);

            List<Entidades.CortePorMovimiento> listaCortesPorMovimiento = new List<Entidades.CortePorMovimiento>();

            SqlDataReader drMovimiento = cmCorte.ExecuteReader();

            while (drMovimiento.Read())
            {
                Entidades.CortePorMovimiento oCortePorMovimiento = new Entidades.CortePorMovimiento();

                oCortePorMovimiento.IdCorteMovimiento = Convert.ToInt32(drMovimiento["idCorteMovimiento"].ToString());

                Entidades.Corte corte =new Entidades.Corte();

                corte.idCorte = Convert.ToInt32(drMovimiento["idCorte"].ToString());
                corte.codigo = Convert.ToInt32(drMovimiento["codigo"].ToString());
                corte.corte = drMovimiento["corte"].ToString();

                oCortePorMovimiento.Corte = corte;

                oCortePorMovimiento.CantKg = float.Parse(drMovimiento["cantKg"].ToString());
                try
                {
                    oCortePorMovimiento.PesoBalanza = Convert.ToBoolean(drMovimiento["pesoBalanza"]);
                }
                catch (Exception)
                {

                    oCortePorMovimiento.PesoBalanza = false;
                }
                

                listaCortesPorMovimiento.Add(oCortePorMovimiento);

                oCortePorMovimiento = null;
               
            }

            cmCorte.Connection.Close();

            return listaCortesPorMovimiento;
        }

        public int agregarActualizacionStock(DateTime fechaActualizacion, string observaciones)
         {
             cmCorte = new SqlCommand();

             cmCorte.Connection = conn.conectar();
             cmCorte.Connection.Open();
             cmCorte.CommandType = CommandType.StoredProcedure;
             cmCorte.CommandText = "AgregarActualizacionStock";

             cmCorte.Parameters.AddWithValue("@fechaActualizacion", fechaActualizacion);
             cmCorte.Parameters.AddWithValue("@observaciones", observaciones);

             SqlDataReader drActualizacion = cmCorte.ExecuteReader();

             int idActualizacion = 0;
             while (drActualizacion.Read())
             {
                 idActualizacion = Convert.ToInt32(drActualizacion["idActualizacion"].ToString());
             }

             cmCorte.Connection.Close();

             cmCorte = null;

             return idActualizacion;
         }

        public void actualizarStockPorCorte(int idActualizacion, Entidades.StockCorteSucursal stockCorte)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "ActualizarStockPorCorte";

            cmCorte.Parameters.AddWithValue("@idActualizacion",idActualizacion);
            cmCorte.Parameters.AddWithValue("@idCorte",stockCorte.Corte.idCorte);
            cmCorte.Parameters.AddWithValue("@idSucursal",stockCorte.Sucursal.idSucursal);
            cmCorte.Parameters.AddWithValue("@stockActual",stockCorte.Stock);
            cmCorte.Parameters.AddWithValue("@stockTeoricoActual",stockCorte.StockTeorico);

            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();

            cmCorte = null;
            
        }


        public void actualizacionStockTotal(int idActualizacion)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "ActualizacionStockTotal";

            cmCorte.Parameters.AddWithValue("@idActualizacion", idActualizacion);
          

            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();

            cmCorte = null;

        }

        public void actualizacionStockTeoricoTotal(int idActualizacion)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "ActualizacionStockTotalTeorico";

            cmCorte.Parameters.AddWithValue("@idActualizacion", idActualizacion);


            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();

            cmCorte = null;

        }

        public void reiniciarStockReal(int idSucursal)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "reiniciarStock";

            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);

            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public void reiniciarStockTeorico(int idSucursal)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "reiniciarStockTeorico";
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);

            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtReporteTeoricoReal = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "StockTeoricoReal";
            cmCorte.Parameters.AddWithValue("@texto",texto);
            cmCorte.Parameters.AddWithValue("@idSucursal",idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde",fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta",fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtReporteTeoricoReal);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtReporteTeoricoReal;
        }

        public DataTable CierreStock(int nroCierre,string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtCierreStock = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            if (nroCierre==1)
            {
                cmCorte.CommandText = "StockCierre";
            }
            if (nroCierre == 2)
            {
                cmCorte.CommandText = "StockCierre_2";
            }
            cmCorte.Parameters.AddWithValue("@texto", texto);
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCierreStock);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtCierreStock;
        }

        public DataTable StockIngresoEgreso(string texto,int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtStockIngresoEgreso = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "StockIngresoEgreso";
            cmCorte.Parameters.AddWithValue("@texto", texto);
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtStockIngresoEgreso);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtStockIngresoEgreso;
        }

        public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtTotalPorCortesVendidos = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "TotalPorCortesVendidos";
            cmCorte.Parameters.AddWithValue("@texto", texto);
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtTotalPorCortesVendidos);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtTotalPorCortesVendidos;
        }

        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal,string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "StockTeoricoReal";
            cmCorte.Parameters.AddWithValue("@texto",texto);
            cmCorte.Parameters.AddWithValue("@idSucursal",idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde",fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta",fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtTeoricoReal);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtTeoricoReal;
        }

        public DataTable TotalKgsCortePorCompra(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtTotalKgsCortePorCompra = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "TotalKgsCortePorCompra";
            cmCorte.Parameters.AddWithValue("@texto", texto);
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtTotalKgsCortePorCompra);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtTotalKgsCortePorCompra;
        }

        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtTotalMovimientosPorCorte = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "TotalMovimientosPorCorte";
            cmCorte.Parameters.AddWithValue("@texto", texto);
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtTotalMovimientosPorCorte);
            cmCorte.Connection.Close();

            cmCorte = null;
            daCorte = null;

            return dtTotalMovimientosPorCorte;
        }

        #endregion
    }
}
