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
        private readonly Contratos.ISucursalRepository oSucursalD;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        // Constructor existente: SIN CAMBIOS. Los 53 puntos de instanciacion actuales
        // (8 controllers de Web + ~44 formularios de Presentacion) siguen igual.
        public Sucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;
            _param = param;
            oSucursalD = new Datos.Sucursal(empresa);
        }

        // Constructor nuevo, aditivo: permite inyectar cualquier implementacion de
        // ISucursalRepository (ej. DatosPostgres.SucursalPg). Usado unicamente por el
        // controller de comparacion de la migracion a Postgres (Etapa 3).
        public Sucursal(Contratos.ISucursalRepository repositorio, IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;
            _param = param;
            oSucursalD = repositorio ?? throw new System.ArgumentNullException(nameof(repositorio));
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
