using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Sucursal
    {
        Utilidades.Conexion conn= new Utilidades.Conexion();
        SqlDataAdapter daSucursal;
       
        public DataTable obtenerSucursales()
        {
            try
            {
                DataTable dtSucursal = new DataTable();
                daSucursal = new SqlDataAdapter("Select * from sucursal", conn.conectar());
                daSucursal.Fill(dtSucursal);

                return dtSucursal;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener sucursales.", ex);
            }

        }

        public Entidades.Sucursal findById(int id)
        {
            try
            {
                DataTable dtSucursal = new DataTable();
                daSucursal = new SqlDataAdapter("Select * from sucursal where idSucursal = " + id, conn.conectar());
                daSucursal.Fill(dtSucursal);

                Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
                if (dtSucursal.Rows.Count > 0)
                {
                    oSucursalE.idSucursal = Convert.ToInt32(dtSucursal.Rows[0]["idSucursal"].ToString());
                    oSucursalE.sucursal = dtSucursal.Rows[0]["sucursal"].ToString();
                }
                return oSucursalE;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener sucursales.", ex);
            }
        }

        public DataTable obtenerSucursalSanMartin()
        {
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal where idSucursal = 2", conn.conectar());
            daSucursal.Fill(dtSucursal);

            return dtSucursal;

        }

        public DataTable obtenerSucursalSanLorenzo()
        {
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal where idSucursal = 1", conn.conectar());
            daSucursal.Fill(dtSucursal);

            return dtSucursal;
        }

        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual)
        {
            DataTable dtConexiones = new DataTable();
            string consulta = "Select * from Conexiones WHERE 1=1 ";
            consulta += mostrarEnPrincipal == null ? " AND 1=1" : " AND mostrarEnPrincipal = " +
                (Convert.ToBoolean(mostrarEnPrincipal) ? 1 : 0);
            consulta += mostrarEnStockActual == null ? " AND 1=1" : " AND mostrarEnStockActual = " + 
                (Convert.ToBoolean(mostrarEnStockActual) ? 1 : 0);
            daSucursal = new SqlDataAdapter(consulta, conn.conectar());
            daSucursal.Fill(dtConexiones);

            return dtConexiones;
        }

        public int getIdSucursalByConexion(string nameConnString)
        {
            SqlCommand cmSucursal = new SqlCommand();

            cmSucursal.Connection = conn.conectar();

            cmSucursal.CommandText = "select top 1 idSucursal from Conexiones where name = \'" + nameConnString + "\'";
            cmSucursal.Connection.Open();
            SqlDataReader drSucursal = cmSucursal.ExecuteReader();

            int idSucursal = 0;
            while (drSucursal.Read())
            {
                idSucursal = Convert.ToInt32(drSucursal["idSucursal"].ToString());
            }

            conn.cerraConexion();
            return idSucursal;
        }

    }
}
