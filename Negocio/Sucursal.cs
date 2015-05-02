using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Sucursal
    {
        Datos.Sucursal oSucursalD;

        public DataTable obtenerSucursales()
        {
            oSucursalD = new Datos.Sucursal();
            return oSucursalD.obtenerSucursales();
        }
        public Entidades.Sucursal findById(int id)
        {
            oSucursalD = new Datos.Sucursal();
            return oSucursalD.findById(id);
        }
        public DataTable obtenerSucursalSanMartin()
        {
            oSucursalD = new Datos.Sucursal();
            return oSucursalD.obtenerSucursalSanMartin();
        }
        public DataTable obtenerSucursalSanLorenzo()
        {
            oSucursalD = new Datos.Sucursal();
            return oSucursalD.obtenerSucursalSanLorenzo();
        }
    }
}
