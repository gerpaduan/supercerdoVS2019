using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Utilidades;

namespace Negocio
{
    public class Sucursal
    {
        private readonly Datos.Sucursal oSucursalD;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        public Sucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;
            _param = param;
            oSucursalD = new Datos.Sucursal(empresa);
        }
        public DataTable obtenerSucursales()
        {            
            return oSucursalD.obtenerSucursales();
        }

        public DataTable obtenerSucursalesConTodas()
        {
            DataTable dtSucursales = obtenerSucursales();
            DataRow drSucursal = dtSucursales.NewRow();
            drSucursal["idSucursal"] = -1;
            drSucursal["sucursal"] = "Todas";
            dtSucursales.Rows.Add(drSucursal);
            dtSucursales.DefaultView.Sort = "idSucursal";
            return dtSucursales;
        }

        public Entidades.Sucursal findById(int id)
        {
            
            return oSucursalD.findById(id);
        }
        public List<Entidades.Sucursal> findAll()
        {
            
            return oSucursalD.findAll();
        }

        public Entidades.Empresa findEmpresaById(int idEmpresa)
        {
            
            return oSucursalD.findEmpresaById(idEmpresa);
        }

        public Entidades.Empresa findEmpresaByCuit(long cuit)
        {
            
            return oSucursalD.findEmpresaByCuit(cuit);
        }

        public void ActualizarDatosBasicos(Entidades.Sucursal oSucursalE)
        {
            oSucursalD.ActualizarDatosBasicos(oSucursalE);
        }

        public DataTable obtenerSucursalSanMartin()
        {
            
            return oSucursalD.obtenerSucursalSanMartin();
        }
        public DataTable obtenerSucursalSanLorenzo()
        {
            
            return oSucursalD.obtenerSucursalSanLorenzo();
        }

        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual)
        {
            
            return oSucursalD.obtenerConexiones(mostrarEnPrincipal, mostrarEnStockActual);
        }

        public int getIdSucursalByConexion(string nameConnString)
        {
            
            return oSucursalD.getIdSucursalByConexion(nameConnString);
        }
    }
}
