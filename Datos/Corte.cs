using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class Corte
    {
        private SqlCommand cmCorte;
        Utilidades.Conexion conn=new Utilidades.Conexion();
        private SqlDataAdapter daCorte;

        public Entidades.Corte getCorteById(int id, bool cargarMaestro)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Select Corte.* from Corte where idCorte = " + id;
            Entidades.Corte oCorteE = new Entidades.Corte();
            try
            {
                cmCorte.Connection.Open();
                SqlDataReader drCorte = cmCorte.ExecuteReader();
                using (drCorte)
                {
                    while (drCorte.Read())
                    {
                        oCorteE.IdCorte = Convert.ToInt32(drCorte["idCorte"]);
                        oCorteE.Codigo = Convert.ToInt32(drCorte["codigo"]);
                        oCorteE.CorteDesc = Convert.ToString(drCorte["corte"]);
                        oCorteE.Tipo = Convert.ToString(drCorte["tipo"]);
                        oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
                        if (cargarMaestro) 
                            oCorteE.CorteMaestro = getCorteById(Convert.ToInt32(drCorte["idCorteMaestro"]), false);
                        oCorteE.Porcentaje = float.Parse(drCorte["porcentaje"].ToString());
                        oCorteE.PrecioKg = float.Parse(drCorte["precioKg"].ToString());
                        oCorteE.PrecioKgReferencia = float.Parse(drCorte["precioKg"].ToString());
                        oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
                        oCorteE.Habilitado = Convert.ToBoolean(drCorte["habilitado"]);
                        oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
                        oCorteE.PorcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
                        oCorteE.Independiente = Convert.ToInt32(drCorte["independiente"]);
                        oCorteE.DesvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());
                        oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
                        oCorteE.Actualizado = drCorte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drCorte["actualizado"]);
                        oCorteE.IdAlicuotaIva = Convert.ToInt32(drCorte["idAlicuotaIva"]);
                        oCorteE.AlicuotaIva = float.Parse(drCorte["alicuotaIva"].ToString());
                    }
                    return oCorteE;
                }
            }
            finally
            {
                cmCorte.Connection.Close();
                oCorteE = null;
            }
        }

        public void editPrecioCorte(Entidades.Corte oCorteE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;            
            cmCorte.CommandText = "UPDATE Corte SET precioKg = @precioKg WHERE idCorte = "+oCorteE.idCorte;
            cmCorte.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();

            cmCorte = null;
        }

        public void addOrEditCorte(Entidades.Corte oCorteE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "addOrEditCorte";

            cmCorte.Parameters.AddWithValue("@idCorte", oCorteE.idCorte);
            cmCorte.Parameters.AddWithValue("@codigo", oCorteE.codigo);
            cmCorte.Parameters.AddWithValue("@corte", oCorteE.corte);
            cmCorte.Parameters.AddWithValue("@tipo", oCorteE.tipo);
            cmCorte.Parameters.AddWithValue("@promedio", oCorteE.Promedio);
            cmCorte.Parameters.AddWithValue("@independiente", oCorteE.independiente);
            cmCorte.Parameters.AddWithValue("@precioKg", oCorteE.precioKg);
            cmCorte.Parameters.AddWithValue("@ingresoRapidoEmbutido", oCorteE.IngresoRapidoEmbutido);
            cmCorte.Parameters.AddWithValue("@habilitado", oCorteE.Habilitado);
            cmCorte.Parameters.AddWithValue("@enCierreStock", oCorteE.EnCierreStock);
            cmCorte.Parameters.AddWithValue("@idCorteMaestro", oCorteE.corteMaestro != null ? oCorteE.corteMaestro.idCorte : 0);
            cmCorte.Parameters.AddWithValue("@porcentaje", oCorteE.porcentaje);
            cmCorte.Parameters.AddWithValue("@porcentajeHueso", oCorteE.porcentajeHueso);
            cmCorte.Parameters.AddWithValue("@desvioEstandar", oCorteE.desvioEstandar);
            cmCorte.Parameters.AddWithValue("@idAlicuotaIva", oCorteE.IdAlicuotaIva);
            cmCorte.Parameters.AddWithValue("@alicuotaIva", oCorteE.AlicuotaIva);

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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "buscarCodigoCorte";
            cmCorte.Parameters.AddWithValue("@codigo", codigo);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            return dtCortes;
        }
       
        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            string consultaSQL = "SELECT     CorteP.idCorte, CorteP.codigo, CorteP.corte, CorteP.precioKg, CorteP.ingresoRapidoEmbutido, CorteP.enCierreStock, " +
                " CorteP.tipo, CorteP.idCorteMaestro, CorteM.corte AS corteMaestro, CorteP.porcentaje, CorteP.porcentajeHueso, CorteP.desvioEstandar, " +
                " CorteP.independiente, CorteP.promedio, CorteP.idAlicuotaIva, CorteP.alicuotaIva FROM  dbo.Corte AS CorteM RIGHT OUTER JOIN " +
                " dbo.Corte AS CorteP ON CorteM.idCorte = CorteP.idCorteMaestro";
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = consultaSQL;

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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "obtenerEmbutidos";
            cmCorte.Parameters.AddWithValue("@texto", txtBusqueda);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            cmCorte.Connection.Close();

            return dtCortes;

        }

        public DataTable getListaElegirEmbutido()
        {
            DataTable dtCortes = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; 
            cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "getListaElegirEmbutido";

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);

            cmCorte.Connection.Close();

            return dtCortes;

        }

        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtCortes = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "buscarEmbutido";
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@texto",texto);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            cmCorte.Connection.Open();
            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCortes);
            cmCorte.Connection.Close();

            return dtCortes;
        }

        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtCortes = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "obtenerLineasEmb";
            cmCorte.Parameters.AddWithValue("@idSucursal", idSucursal);
            cmCorte.Parameters.AddWithValue("@texto", texto);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);

            cmCorte.Connection.Open();
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "obtenerInfoCorte";
            cmCorte.Parameters.AddWithValue("@idCorte", idCorte);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtCorte);

            cmCorte.Connection.Close();

            return dtCorte;
        }

        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Select Corte.* from Corte where idCorte =" + idCorte;

            Entidades.Corte oCorteE = new Entidades.Corte();
            try
            {
                cmCorte.Connection.Open();
                SqlDataReader drCorte = cmCorte.ExecuteReader();

                using (drCorte)
                {
                    while (drCorte.Read())
                    {
                        oCorteE.idCorte = Convert.ToInt32(drCorte["idCorte"].ToString());
                        oCorteE.codigo = Convert.ToInt32(drCorte["codigo"].ToString());
                        oCorteE.corte = drCorte["corte"].ToString();
                        oCorteE.tipo = drCorte["tipo"].ToString();
                        oCorteE.Promedio = float.Parse(drCorte["promedio"].ToString());
                        oCorteE.CorteMaestro = buscarMaestro ? findCorteById(Convert.ToInt32(drCorte["idCorteMaestro"].ToString()), false) : null;
                        oCorteE.precioKg = float.Parse(drCorte["precioKg"].ToString());
                        oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(drCorte["ingresoRapidoEmbutido"]);
                        oCorteE.EnCierreStock = Convert.ToBoolean(drCorte["enCierreStock"]);
                        oCorteE.independiente = Convert.ToInt32(drCorte["independiente"].ToString());
                        oCorteE.porcentaje = float.Parse(drCorte["porcentaje"].ToString());
                        oCorteE.desvioEstandar = float.Parse(drCorte["desvioEstandar"].ToString());
                        oCorteE.porcentajeHueso = float.Parse(drCorte["porcentajeHueso"].ToString());
                        oCorteE.Creado = Convert.ToDateTime(drCorte["creado"]);
                        oCorteE.Actualizado = drCorte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drCorte["actualizado"]);                     
                    }
                    return oCorteE;
                }
            }
            finally
            {
                cmCorte.Connection.Close();
                oCorteE = null;
            }
        }

        #region formula 
        public DataTable buscarFormula(string texto)
        {
            DataTable dtFormulas = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;

            string consulta = "SELECT DISTINCT dbo.Formulas.idFormula, dbo.Corte.codigo, dbo.Corte.corte, dbo.Formulas.creado, dbo.Formulas.actualizado FROM" +
                " dbo.Corte INNER JOIN  dbo.Formulas ON dbo.Corte.idCorte = dbo.Formulas.idEmbutido " +
                " Where dbo.Corte.corte like '%" + texto + "%' "+// or dbo.Corte.codigo = " + texto + ""+
                " ORDER BY dbo.Corte.codigo ";  //dbo.Corte.corte  like '%\" + texto + \"%'";
            cmCorte.CommandText = consulta;
            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtFormulas);
            cmCorte.Parameters.AddWithValue("@texto", texto);

            return dtFormulas;
        }
        public Entidades.Formula findFormulaByID(int idFormula)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Select Formulas.* from Formulas where idFormula =" + idFormula;

            SqlDataReader drFormula = cmCorte.ExecuteReader();

            Entidades.Formula oFormula = new Entidades.Formula();
            while (drFormula.Read())
            {
                oFormula.IdFormula = Convert.ToInt32(drFormula["idFormula"].ToString());
                oFormula.Embutido = findCorteById(Convert.ToInt32(drFormula["idEmbutido"].ToString()), false);
                
                oFormula.Creado = Convert.ToDateTime(drFormula["creado"]);
                oFormula.Actualizado = drFormula["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drFormula["actualizado"]);

                Datos.Usuario oUsuarioD = new Usuario();
                oFormula.CreadoPor = string.IsNullOrEmpty(drFormula["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drFormula["creadoPor"]));
                oFormula.ActualizadoPor = string.IsNullOrEmpty(drFormula["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drFormula["actualizadoPor"]));

                oFormula.ListaCortesEnFormula = cargarCortesPorFormula(oFormula);
            }
            cmCorte.Connection.Close();
            return oFormula;
        }
        public List<Entidades.CortePorFormula> cargarCortesPorFormula(Entidades.Formula oFormula)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "SELECT * FROM CortePorFormula WHERE idFormula = " + oFormula.IdFormula;

            List<Entidades.CortePorFormula> listaCortesPorFormula = new List<Entidades.CortePorFormula>();
            SqlDataReader drFormula = cmCorte.ExecuteReader();

            while (drFormula.Read())
            {
                Entidades.CortePorFormula oCortePorFormula = new Entidades.CortePorFormula();

                oCortePorFormula.IdCorteEnFormula = Convert.ToInt32(drFormula["idCortePorFormula"].ToString());
                oCortePorFormula.Formula = oFormula;
                oCortePorFormula.CorteEnFormula = findCorteById(Convert.ToInt32(drFormula["idCorte"].ToString()),false);
                oCortePorFormula.Porcentaje = float.Parse(drFormula["porcentaje"].ToString());
                oCortePorFormula.AgregarAuto = Convert.ToBoolean(drFormula["agregarAuto"].ToString());

                listaCortesPorFormula.Add(oCortePorFormula);
                oCortePorFormula = null;
            }
            cmCorte.Connection.Close();
            return listaCortesPorFormula;
        }


        public int existeFormula(int idEmbutido)
        {
            int idFormula = 0;
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text;
            cmCorte.CommandText = "Select idFormula from Formulas where idEmbutido = " + idEmbutido;
            try
            {
                cmCorte.Connection.Open();
                SqlDataReader drCorte = cmCorte.ExecuteReader();
                using (drCorte)
                {
                    while (drCorte.Read())
                    {
                        idFormula = Convert.ToInt32(drCorte["idFormula"]);
                    }
                    return idFormula;
                }
            }
            finally
            {
                cmCorte.Connection.Close();
            }
        }

        public int addOrEditFormula(Entidades.Formula oFormula, List<Entidades.CortePorFormula> listaCortesPorFormula)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure;
            cmCorte.CommandText = "addOrEditFormula";
            cmCorte.Parameters.AddWithValue("@idFormula", oFormula.IdFormula);
            cmCorte.Parameters.AddWithValue("@idEmbutido", oFormula.Embutido.idCorte); 
            cmCorte.Parameters.AddWithValue("@creadoPor", oFormula.CreadoPor.Id);
            cmCorte.Parameters.AddWithValue("@actualizadoPor", oFormula.ActualizadoPor!=null ? oFormula.ActualizadoPor.Id : 0);

            oFormula.IdFormula = (int)cmCorte.ExecuteScalar();

            cmCorte.CommandText = "agregarCortePorFormula";
            cmCorte.Parameters.Clear();
            foreach (Entidades.CortePorFormula item in listaCortesPorFormula)
            {
                cmCorte.Parameters.AddWithValue("@idFormula", oFormula.IdFormula);
                cmCorte.Parameters.AddWithValue("@idCorte", item.CorteEnFormula1.idCorte );
                cmCorte.Parameters.AddWithValue("@porcentaje", item.Porcentaje);
                cmCorte.Parameters.AddWithValue("@agregarAuto", item.AgregarAuto);
                cmCorte.ExecuteNonQuery();

                cmCorte.Parameters.Clear();
            }

            cmCorte.Connection.Close();

            return oFormula.IdFormula;
        }

        public void eliminarFormula(int idFormula)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();

            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "DELETE FROM  CortePorFormula WHERE idFormula = " + idFormula;
            cmCorte.ExecuteNonQuery();

            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "DELETE FROM  Formulas WHERE idFormula = " + idFormula;
            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();
        }

        public DataTable getFormulaEmbutido(int idEmbutido)
        {
            DataTable dtFormula = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();

            cmCorte.CommandType = CommandType.Text;
            cmCorte.CommandText = "SELECT dbo.Corte.idCorte, dbo.Corte.codigo, dbo.Corte.corte, dbo.CortePorFormula.porcentaje, '' AS 'kgs', dbo.CortePorFormula.agregarAuto " +
                "FROM  dbo.Formulas INNER JOIN  dbo.CortePorFormula ON dbo.Formulas.idFormula = dbo.CortePorFormula.idFormula INNER JOIN "+
                "dbo.Corte ON dbo.CortePorFormula.idCorte = dbo.Corte.idCorte WHERE  dbo.Formulas.idEmbutido = " + idEmbutido + 
                " ORDER BY dbo.CortePorFormula.agregarAuto desc";

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtFormula);

            cmCorte.Connection.Close();
            return dtFormula;
        }
        

        #endregion

        #region Alicuota Iva
        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
        {
            DataTable dtAlicuotasIva = new DataTable();
            daCorte = new SqlDataAdapter();

            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            string consulta = "Select idIva, iva from AlicuotasIva";
            consulta += mostrarTodos ? " order by orden" : " where mostrar = 1 order by orden";
            cmCorte.CommandText =  consulta;

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtAlicuotasIva);

            cmCorte.Connection.Close();

            return dtAlicuotasIva;
        }

        public Entidades.AlicuotaIva findAlicuotaIvaById(int idIva)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Select * from AlicuotasIva where idIva =" + idIva;

            Entidades.AlicuotaIva oAlicuotaIvaE = new Entidades.AlicuotaIva();
            try
            {
                cmCorte.Connection.Open();
                SqlDataReader drCorte = cmCorte.ExecuteReader();

                using (drCorte)
                {
                    while (drCorte.Read())
                    {
                        oAlicuotaIvaE.IdIva = Convert.ToInt32(drCorte["idIva"].ToString());
                        oAlicuotaIvaE.Iva = Convert.ToInt32(drCorte["iva"].ToString());
                        oAlicuotaIvaE.Orden = Convert.ToInt32(drCorte["orden"].ToString());
                        oAlicuotaIvaE.Mostrar = bool.Parse(drCorte["mostrar"].ToString());
                    }
                    return oAlicuotaIvaE;
                }
            }
            finally
            {
                cmCorte.Connection.Close();
                oAlicuotaIvaE = null;
            }
        }
        #endregion

        #region Embutidos

        public Entidades.Embutido findEmbutidoById(int idEmbutido)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Select Embutidos.* from Embutidos where idEmbutido =" + idEmbutido;

            Entidades.Embutido oEmbutidoE = new Entidades.Embutido();

            try
            {
                cmCorte.Connection.Open();
                SqlDataReader drEmbutido = cmCorte.ExecuteReader();

                using (drEmbutido)
                {
                    while (drEmbutido.Read())
                    {
                        oEmbutidoE.IdEmbutido = Convert.ToInt32(drEmbutido["idEmbutido"]);
                        oEmbutidoE.FechaEmbutido = Convert.ToDateTime(drEmbutido["fechaEmbutido"]);
                        oEmbutidoE.Corte = findCorteById(Convert.ToInt32(drEmbutido["idCorte"]), true);
                        Datos.Sucursal oSucursalD = new Sucursal();
                        oEmbutidoE.Sucursal = oSucursalD.findById(Convert.ToInt32(drEmbutido["idSucursal"]));
                        oEmbutidoE.Observaciones = Convert.ToString(drEmbutido["observaciones"]);
                        oEmbutidoE.Estado = Convert.ToString(drEmbutido["estado"]);
                        oEmbutidoE.Creado = Convert.ToDateTime(drEmbutido["creado"]);
                        oEmbutidoE.Actualizado = drEmbutido["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drEmbutido["actualizado"]);
                        
                        Datos.Usuario oUsuarioD = new Usuario();
                        oEmbutidoE.CreadoPor = string.IsNullOrEmpty(drEmbutido["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drEmbutido["creadoPor"]));
                        oEmbutidoE.ActualizadoPor = string.IsNullOrEmpty(drEmbutido["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drEmbutido["actualizadoPor"]));

                        oEmbutidoE.CortesEnEmbutido = obtenerCortesEnEmbutido(oEmbutidoE);
                    }
                    return oEmbutidoE;
                }
            }
            finally
            {
                cmCorte.Connection.Close();
                oEmbutidoE = null;
            }
        }

        public List<Entidades.CortePorEmbutido> obtenerCortesEnEmbutido(Entidades.Embutido oEmbutidoParam)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.CommandType = CommandType.Text; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Select CortePorEmbutido.* from CortePorEmbutido where idEmbutido =" + oEmbutidoParam.idEmbutido;

            List<Entidades.CortePorEmbutido> cortesEnEmbutido = new List<Entidades.CortePorEmbutido>();
            Entidades.CortePorEmbutido oCorteEnEmbutido;
            try
            {
                cmCorte.Connection.Open();
                SqlDataReader drEmbutido = cmCorte.ExecuteReader();

                using (drEmbutido)
                {
                    while (drEmbutido.Read())
                    {
                        oCorteEnEmbutido = new Entidades.CortePorEmbutido();
                        oCorteEnEmbutido.IdCorteEmbutido = Convert.ToInt32(drEmbutido["idCorteEmbutido"]);
                        oCorteEnEmbutido.Embutido = oEmbutidoParam;
                        oCorteEnEmbutido.Corte = findCorteById(Convert.ToInt32(drEmbutido["idCorte"]), false);
                        oCorteEnEmbutido.KgUtilizado = float.Parse(drEmbutido["kgUtilizados"].ToString());
                        oCorteEnEmbutido.PesoBalanza = Convert.ToBoolean(drEmbutido["pesoBalanza"]);
                        
                        cortesEnEmbutido.Add(oCorteEnEmbutido);
                    }
                    return cortesEnEmbutido;
                }
            }
            finally
            {
                cmCorte.Connection.Close();
                cortesEnEmbutido = null;
            }
        }

        public int agregarEmbutido(Entidades.Embutido oEmbutido)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "agregarEmbutido";
            cmCorte.Parameters.AddWithValue("@fechaEmbutido", oEmbutido.fechaEmbutido);
            cmCorte.Parameters.AddWithValue("@idCorte", oEmbutido.corte.idCorte);
            cmCorte.Parameters.AddWithValue("@idSucursal", oEmbutido.sucursal.IdSucursal);
            cmCorte.Parameters.AddWithValue("@creadoPor", oEmbutido.CreadoPor.Id);
            cmCorte.Parameters.AddWithValue("@observaciones", oEmbutido.observaciones);

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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "anularEmbutido";
            cmCorte.Parameters.AddWithValue("@idEmbutido", oEmbutidoE.idEmbutido);
            cmCorte.Parameters.AddWithValue("@actualizadoPor", oEmbutidoE.ActualizadoPor.Id);

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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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

        public int addOrEditMovimiento(Entidades.Movimiento oMovimientoE)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "addOrEditMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
            cmCorte.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
            cmCorte.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
            cmCorte.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
            cmCorte.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones);
            cmCorte.Parameters.AddWithValue("@creadoPor", oMovimientoE.CreadoPor.Id);

            if (oMovimientoE.IdMovimiento.Equals(0))
            {
                SqlDataReader drMovimiento = cmCorte.ExecuteReader();
                while (drMovimiento.Read())
                {
                    oMovimientoE.IdMovimiento = Convert.ToInt32(drMovimiento["idMovimiento"].ToString());
                }                
            }
            else
            {
                cmCorte.Parameters.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);
                cmCorte.ExecuteNonQuery();
            }
            cmCorte.Connection.Close();
            cmCorte = null;

            return oMovimientoE.IdMovimiento;
        }

        public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "modificarMovimiento";

            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);
            cmCorte.Parameters.AddWithValue("@fechaMovimiento", oMovimientoE.FechaMovimiento);
            cmCorte.Parameters.AddWithValue("@sucursalOrigen", oMovimientoE.SucursalOrigen.idSucursal);
            cmCorte.Parameters.AddWithValue("@sucursalDestino", oMovimientoE.SucursalDestino.idSucursal);
            cmCorte.Parameters.AddWithValue("@observaciones", oMovimientoE.Observaciones);
            cmCorte.Parameters.AddWithValue("@actualizadoPor", oMovimientoE.ActualizadoPor.Id);

            cmCorte.ExecuteNonQuery();

            cmCorte.Connection.Close();
        }

        public void eliminarMovimiento(int idMovimiento, Entidades.Usuario oUsuario)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "eliminarMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);
            cmCorte.Parameters.AddWithValue("@actualizadoPor", oUsuario.Id);

            cmCorte.Connection.Open();
            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();
        }

        public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "agregarCortePorMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", cortePorMovimiento.Movimientos.IdMovimiento);
            cmCorte.Parameters.AddWithValue("@idCorte", cortePorMovimiento.Corte.IdCorte);
            cmCorte.Parameters.AddWithValue("@cantKg", cortePorMovimiento.CantKg);
            cmCorte.Parameters.AddWithValue("@cantUnidad", cortePorMovimiento.CantUnidad);
            cmCorte.Parameters.AddWithValue("@pesoBalanza", cortePorMovimiento.PesoBalanza);
            cmCorte.Parameters.AddWithValue("@permitirIngreso", cortePorMovimiento.PermitirIngreso);

            cmCorte.ExecuteNonQuery();
            cmCorte.Connection.Close();
        }

        public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
        {
            cmCorte = new SqlCommand();
            cmCorte.Connection = conn.conectar();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "quitarCortesPorMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", oMovimientoE.IdMovimiento);

            cmCorte.Connection.Open();           
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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

        public Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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

                oMovimiento.IdMovOrigen = !string.IsNullOrEmpty(drMovimiento["idMovOrigen"].ToString()) ? Convert.ToInt32(drMovimiento["idMovOrigen"].ToString()) : 0;
                
                Entidades.Sucursal destino = new Entidades.Sucursal();
                destino.idSucursal = Convert.ToInt32(drMovimiento["idDestino"].ToString());
                destino.sucursal = drMovimiento["destino"].ToString();

                oMovimiento.SucursalDestino = destino;

                oMovimiento.Observaciones = drMovimiento["observaciones"].ToString();

                ///Borrar si funciona el seteo nuevo
                ///
                //oMovimiento.Creado = drMovimiento["creado"].Equals(null) ? (DateTime?)null : (DateTime?)Convert.ToDateTime(drMovimiento["creado"].ToString());
                //DateTime fechaNull = Convert.ToDateTime("01/01/1990");
                //oMovimiento.Actualizado = !String.IsNullOrEmpty(drMovimiento["actualizado"].ToString()) ? (Convert.ToDateTime(drMovimiento["actualizado"].ToString())) : fechaNull;

                oMovimiento.Creado = Convert.ToDateTime(drMovimiento["creado"]);
                oMovimiento.Actualizado = drMovimiento["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drMovimiento["actualizado"]);          

                Datos.Usuario oUsuarioD = new Usuario();
                oMovimiento.CreadoPor = string.IsNullOrEmpty(drMovimiento["creadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drMovimiento["creadoPor"]));
                oMovimiento.ActualizadoPor = string.IsNullOrEmpty(drMovimiento["actualizadoPor"].ToString()) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drMovimiento["actualizadoPor"]));

                oMovimiento.ListaCortesPorMov = cargarCortesPorMovimiento(oMovimiento.IdMovimiento, acumulado);
            }
            cmCorte.Connection.Close();
            return oMovimiento;
        }

        public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();

            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "cargarCortesPorMovimiento";
            cmCorte.Parameters.AddWithValue("@idMovimiento", idMovimiento);
            cmCorte.Parameters.AddWithValue("@acumulado", acumulado);

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
                oCortePorMovimiento.CantUnidad = Convert.ToInt32(drMovimiento["cantUnidad"].ToString());
                //try
                //{
                //    oCortePorMovimiento.PesoBalanza = Convert.ToBoolean(drMovimiento["pesoBalanza"]);
                //}
                //catch (Exception)
                //{
                //    oCortePorMovimiento.PesoBalanza = false;
                //}
                ///si no es acumulado directamente se establece falso el Permitir ingreso
                ///porque no interesa agruparlo por cada valor de permitirIngreso
                if (!acumulado)
                {
                    oCortePorMovimiento.PesoBalanza = drMovimiento["pesoBalanza"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovimiento["pesoBalanza"]);
                    oCortePorMovimiento.PermitirIngreso = drMovimiento["permitirIngreso"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovimiento["permitirIngreso"]);
                }
                listaCortesPorMovimiento.Add(oCortePorMovimiento);
                oCortePorMovimiento = null;               
            }
            cmCorte.Connection.Close();
            return listaCortesPorMovimiento;
        }


        public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            DataTable dtLineasMov = new DataTable();

            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "obtenerLineasMov";
            cmCorte.Parameters.AddWithValue("@sucOrigen", sucOrigen);
            cmCorte.Parameters.AddWithValue("@sucDestino", sucDestino);
            cmCorte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCorte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCorte.Parameters.AddWithValue("@texto", texto);

            daCorte.SelectCommand = cmCorte;
            daCorte.Fill(dtLineasMov);

            return dtLineasMov;
        }

        public void reiniciarStockReal(int idSucursal)
        {
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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

        public DataTable CierreStock(int nroCierre,string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
        {
            DataTable dtCierreStock = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = string.IsNullOrEmpty(conexionSucursal) ? conn.conectar() : conn.conectar(conexionSucursal);

            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            if (nroCierre==1)
            {
                //cmCorte.CommandText = "a_InicioCierreStock";
                cmCorte.CommandText = "a_CierreStock";
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

            try
            {
                daCorte.Fill(dtCierreStock);
                cmCorte.Connection.Close();
            }
            catch (Exception)
            {

                cmCorte.Connection.Close();
            }

            cmCorte = null;
            daCorte = null;

            return dtCierreStock;
        }

        public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtStockIngresoEgreso = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            cmCorte.CommandText = "Acum_Ventas";
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

        public DataTable StockIngresoEgreso(string texto,int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtStockIngresoEgreso = new DataTable();
            daCorte = new SqlDataAdapter();
            cmCorte = new SqlCommand();

            cmCorte.Connection = conn.conectar();
            cmCorte.Connection.Open();
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
            //cmCorte.CommandText = "TotalKgsCortePorCompra";
            cmCorte.CommandText = "a_CierreStock"; // "a_IngresoStock";
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
            cmCorte.CommandType = CommandType.StoredProcedure; cmCorte.CommandTimeout = 90;
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
