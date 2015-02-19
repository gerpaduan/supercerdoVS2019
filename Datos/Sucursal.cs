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

    }
}
