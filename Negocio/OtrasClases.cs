using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class OtrasClases
    {
        Datos.OtrasClases oOtrasClasesD = new Datos.OtrasClases();

        public void obtenerParametros()
        { 
            DataTable dtParametros = oOtrasClasesD.obtenerParametros();            

            for (int fila = 0; fila < dtParametros.Rows.Count; fila++)
            {
                switch (dtParametros.Rows[fila]["nombre"].ToString())
                {
                    case "porcAjEfectivo":
                        Entidades.Parametros.porcAjEfectivo = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjDebito":
                        Entidades.Parametros.porcAjDebito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjCredito":
                        Entidades.Parametros.porcAjCredito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjCtaCte":
                        Entidades.Parametros.porcAjCtaCte = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjQr":
                        Entidades.Parametros.porcAjQr = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjTranf":
                        Entidades.Parametros.porcAjTranf = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "limiteKgParaAjuste":
                        Entidades.Parametros.limiteKgParaAjuste = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "idConsumidorFinal":
                        Entidades.Parametros.idConsumidorFinal = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "comisionDebito":
                        Entidades.Parametros.comisionDebito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "comisionCredito":
                        Entidades.Parametros.comisionCredito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "idIndefinido":
                        Entidades.Parametros.idIndefinido = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "minAccesoUltimaVentaVendedor":
                        Entidades.Parametros.minAccesoUltimaVentaVendedor = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "idPagoTarjetaEgresoCaja":
                        Entidades.Parametros.idPagoTarjetaEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "idCtaCteEgresoCaja":
                        Entidades.Parametros.idCtaCteEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "idCompraEgresoCaja":
                        Entidades.Parametros.idCompraEgresoCaja = Convert.ToInt32(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                        
                    default:
                        break;
                }
            }
        }
    }
}
