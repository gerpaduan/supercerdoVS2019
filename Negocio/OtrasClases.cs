using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Datos;
using System.Data.SqlClient;
using Utilidades;

namespace Negocio
{
    public class OtrasClases
    {
        private readonly Datos.OtrasClases oOtrasClasesD;
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public OtrasClases(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oOtrasClasesD = new Datos.OtrasClases(empresa, param);
        }

        #region Licencia
        public bool existeLicencia(string nroLicencia)
        {
            
            return oOtrasClasesD.existeLicencia(nroLicencia);
        }

        public void agregarLicencia(string nroLicencia, string identificacion)
        {
            
            oOtrasClasesD.agregarLicencia(nroLicencia, identificacion);
        }
        #endregion

        #region VencimientosLicencia
        public DataTable obtenerVencimientoLicencia(DateTime fechaDesde)
        {
            
            return oOtrasClasesD.obtenerVencimientoLicencia(fechaDesde);
        }

        public DateTime fechaVencimientoLicencia()
        {
            
            return oOtrasClasesD.fechaVencimientoLicencia();
        }
        public bool existePagoLicenciaHoy()
        {
            
            return oOtrasClasesD.existePagoLicenciaHoy();
        }
        public void agregaVencimientosLicencia(DateTime fechaDesde)
        {
            
            oOtrasClasesD.agregaVencimientosLicencia(fechaDesde);
        }
        public void agregarPagoCuota(DateTime fechaVencimiento)
        {
            
            oOtrasClasesD.agregarPagoCuota(fechaVencimiento);
        }
        #endregion


        //public DataTable obtenerParametrosDt()
        //{
        //    return oOtrasClasesD.obtenerParametros();
        //}
        //public void actualizarParametros(DataTable dtParametros)
        //{
        //    oOtrasClasesD.actualizarParametros(dtParametros);
        //}
        //public void obtenerParametros()
        //{ 
        //    DataTable dtParametros = oOtrasClasesD.obtenerParametros();            

        //    for (int fila = 0; fila < dtParametros.Rows.Count; fila++)
        //    {
        //        switch (dtParametros.Rows[fila]["nombre"].ToString())
        //        {
        //            case "porcAjEfectivo":
        //                Entidades.ParamKeys.porcAjEfectivo = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "porcAjDebito":
        //                Entidades.ParamKeys.porcAjDebito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "porcAjCredito":
        //                Entidades.ParamKeys.porcAjCredito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "porcAjCtaCte":
        //                Entidades.ParamKeys.porcAjCtaCte = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "porcAjQr":
        //                Entidades.ParamKeys.porcAjQr = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "porcAjTranf":
        //                Entidades.ParamKeys.porcAjTranf = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "limiteKgParaAjuste":
        //                Entidades.ParamKeys.limiteKgParaAjuste = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "idConsumidorFinal":
        //                Entidades.Persona.idConsumidorFinal = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "comisionDebito":
        //                Entidades.ParamKeys.comisionDebito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "comisionCredito":
        //                Entidades.ParamKeys.comisionCredito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "idIndefinido":
        //                Entidades.Persona.idIndefinido = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()); 
        //                break;
        //                ///TODO: eliminar este parametro
        //            case "minAccesoUltimaVentaVendedor":
        //                Entidades.ParamKeys.minAccesoUltimaVentaVendedor = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()); 
        //                break;
        //            case "idPagoTarjetaEgresoCaja":
        //                Entidades.ParamKeys.idPagoTarjetaEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()); 
        //                break;
        //            case "idCtaCteEgresoCaja":
        //                Entidades.ParamKeys.idCtaCteEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "idCompraEgresoCaja":
        //                Entidades.EgresoCaja.idCompraEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "mayuscula":
        //                Entidades.ParamKeys.mayuscula = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()) == 1;
        //                break;
        //            case "loginRapidoMovimiento":
        //                Entidades.ParamKeys.loginRapidoMovimiento = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()) == 1;
        //                break;
        //            case "loginRapidoElaborado":
        //                Entidades.ParamKeys.loginRapidoElaborado = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()) == 1;
        //                break;
        //            case "loginRapidoStock":
        //                Entidades.ParamKeys.loginRapidoStock = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()) == 1;
        //                break;
        //            case "diasLimitFechaDesde":
        //                Entidades.ParamKeys.diasLimitFechaDesde = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "importeMaxRedondeo":
        //                Entidades.ParamKeys.importeMaxRedondeo = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "idPagoCobroEgresoCaja":
        //                Entidades.EgresoCaja.idPagoCobroEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            case "codProdGenerico":
        //                Entidades.ParamKeys.codProdGenerico = Convert.ToInt64(dtParametros.Rows[fila]["valor"].ToString());
        //                break;
        //            default:
        //                break;
        //        }
        //    }
        //}
    }
}
