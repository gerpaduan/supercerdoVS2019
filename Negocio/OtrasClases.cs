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
                    case "salChorizo": 
                        Entidades.Parametros.salChorizo = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "pimientaChorizo": 
                        Entidades.Parametros.pimientaChorizo = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "nuezChorizo": 
                        Entidades.Parametros.nuezChorizo = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "bracolorChorizo": 
                        Entidades.Parametros.bracolorChorizo = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "salSalame": 
                        Entidades.Parametros.salSalame = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "pimientaSalame": 
                        Entidades.Parametros.pimientaSalame = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "nuezSalame": 
                        Entidades.Parametros.nuezSalame = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "productoSalame": 
                        Entidades.Parametros.productoSalame = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "salSalchicha": 
                        Entidades.Parametros.salSalchicha = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "pimientaSalchicha": 
                        Entidades.Parametros.pimientaSalchicha = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "bracolorSalchicha": 
                        Entidades.Parametros.bracolorSalchicha = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "pimentonSalchicha": 
                        Entidades.Parametros.pimentonSalchicha = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); 
                        break;
                    case "salQueso": 
                        Entidades.Parametros.salQueso = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "pimientaQueso":
                        Entidades.Parametros.pimientaQueso = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "nuezQueso":
                        Entidades.Parametros.salQueso = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "bracolorQueso": 
                        Entidades.Parametros.pimientaQueso = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "salMorcilla": 
                        Entidades.Parametros.salMorcilla = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "pimientaMorcilla": 
                        Entidades.Parametros.pimientaMorcilla = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "nuezMorcilla": 
                        Entidades.Parametros.nuezMorcilla = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "bracolorMorcilla":
                        Entidades.Parametros.bracolorMorcilla = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "salCodeguin":
                        Entidades.Parametros.salCodeguin = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "pimientaCodeguin":
                        Entidades.Parametros.pimientaCodeguin = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "nuezCodeguin":
                        Entidades.Parametros.nuezCodeguin = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "bracolorCodeguin":
                        Entidades.Parametros.bracolorCodeguin = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "salMilanesa":
                        Entidades.Parametros.salMilanesa = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    case "pimientaMilanesa":
                        Entidades.Parametros.pimientaMilanesa = float.Parse(dtParametros.Rows[fila]["valor"].ToString()); break;
                    default:
                        break;
                }
            }
        }
    }
}
