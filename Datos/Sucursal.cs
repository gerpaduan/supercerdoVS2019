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
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal", conn.conectar());
            daSucursal.Fill(dtSucursal);

            return dtSucursal;

        }

        public Entidades.Sucursal findById(int id)
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

        public Entidades.Sucursal findById(int id)
        {
            DataTable dtSucursal = new DataTable();
            daSucursal = new SqlDataAdapter("Select * from sucursal where idSucursal = "+id, conn.conectar());
            daSucursal.Fill(dtSucursal);

            Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
            if (dtSucursal.Rows.Count > 0)
            {
                oSucursalE.idSucursal = Convert.ToInt32(dtSucursal.Rows[0]["idSucursal"].ToString());
                oSucursalE.sucursal = dtSucursal.Rows[0]["sucursal"].ToString();
            }

            return oSucursalE;

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
    }
}
