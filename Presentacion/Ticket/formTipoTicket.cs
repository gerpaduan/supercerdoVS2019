using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Ticket
{
    public partial class formTipoTicket : Form
    {
        public formTipoTicket()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formTipoTicket_Load(object sender, EventArgs e)
        {

        }
        public void movimientoAcumulado(int idMovimiento)
        {
            try
            {
                Negocio.Corte oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
                Entidades.Movimiento oMovimientoE = oCorteN.cargarMovimiento(idMovimiento, true);

                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("Movimiento");
                ticket.LineasEnBlanco(1);
                string idMovOrigen = oMovimientoE.IdMovOrigen != 0 ? oMovimientoE.IdMovOrigen.ToString() : oMovimientoE.IdMovimiento.ToString();
                ticket.TextoIzquierda("Origen: " + oMovimientoE.SucursalOrigen.sucursal + " - ID: " + idMovOrigen);
                ticket.TextoIzquierda("Destino: " + oMovimientoE.SucursalDestino.sucursal);
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Fecha: " + Utilidades.Util_Form.fechaFormato24Horas(oMovimientoE.FechaMovimiento));
                ticket.LineasEnBlanco(1);
                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                ticket.TextoIzquierda("Descripcion     Cant.     Kgs.  ");

                ticket.LineasGuion();
                List<Entidades.CortePorMovimiento> lineasMovimiento = oCorteN.cargarCortesPorMovimiento(oMovimientoE.IdMovimiento, true);
                string descripcion, cantCorte, kgsCorte;
                int totalCant = 0;
                float totalKgs = 0;
                foreach (Entidades.CortePorMovimiento corteMov in lineasMovimiento)
                {
                    descripcion = corteMov.Corte.codigo.ToString() + " " + corteMov.Corte.CorteDesc.ToString();
                    if (descripcion.Length < 14)
                    {
                        for (int i = descripcion.Length; i < 15; i++)
                        {
                            descripcion += " ";
                        }
                    }
                    descripcion = descripcion.Length > 14 ? descripcion.Substring(0, 14) : descripcion;
                    cantCorte = corteMov.CantUnidad.ToString().Length.Equals(1) ? " " + corteMov.CantUnidad.ToString() : corteMov.CantUnidad.ToString();
                    kgsCorte = corteMov.CantKg.ToString("F3").Length.Equals(5) ? " " + corteMov.CantKg.ToString("F3") : corteMov.CantKg.ToString("F3");

                    ticket.TextoIzquierda(descripcion + "    " + cantCorte + "      " + kgsCorte);

                    totalCant += corteMov.CantUnidad;
                    totalKgs += corteMov.CantKg;
                    ticket.LineasEnBlanco(1);
                }
                ticket.TextoDerecha("---------------");
                string totalCantString = totalCant.ToString().Length.Equals(1) ? " " + totalCant.ToString() : totalCant.ToString();
                string totalKgsString = totalKgs.ToString("F3").Length.Equals(6) ? " " + totalKgs.ToString("F3") : totalKgs.ToString("F3");

                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                //ticket.TextoIzquierda("Descripcion     Cant.     Kgs.  ");
                ticket.TextoIzquierda("Total             " + totalCantString + "       " + totalKgsString);

                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Creado: ");
                ticket.TextoIzquierda("Creado Por: ");

                if (oMovimientoE.Observaciones.Length > 0)
                {
                    ticket.TextoIzquierda("Nota: ");
                    string observaciones = "";
                    int limit = 30;
                    int longObs = oMovimientoE.Observaciones.Length;
                    if (oMovimientoE.Observaciones.Length <= limit)
                    {
                        ticket.TextoIzquierda(oMovimientoE.Observaciones);
                    }
                    else
                    {
                        for (int i = 0; i < longObs; i++)
                        {
                            observaciones = oMovimientoE.Observaciones.Length > (limit + i) ?
                                oMovimientoE.Observaciones.Substring(i, limit) : oMovimientoE.Observaciones.Substring(i - 1);
                            ticket.TextoIzquierda(observaciones);
                            i += 30;
                        }
                    }
                }
                ticket.LineasEnBlanco(3);
                ticket.TextoIzquierda("Firma:_ _ _ _ _ _ _ _ _ ");
                ticket.LineasEnBlanco(4);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir ticket.\n\n"+ ex.Message+"\n\n"+ex.StackTrace); 
            }
        }

        public void cierreStock(string fechaDesde, string fechaHasta, DataGridView grilla)
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("Cierre Stock");
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Desde: " + fechaDesde);
                ticket.TextoIzquierda("Hasta: " + fechaHasta);
                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                ticket.TextoIzquierda("Desc.     S.Inic   Stock    Dif.");
                ticket.LineasGuion();
                string descripcion, stockIniString, stockRealString, faltanteString;
                decimal stockIni ,stockReal, faltante;
                int longDesc = 9;
                int longPeso = 7;
                foreach (DataGridViewRow  fila in grilla.Rows)
                {
                    descripcion = fila.Cells["Codigo"].Value.ToString().Trim() + " " + fila.Cells["Corte"].Value.ToString();
                    if (descripcion.Length < longDesc)
                    {
                        for (int i = descripcion.Length; i < (longDesc-1); i++)
                        {
                            descripcion += " ";
                        }
                    }
                    stockIni = Convert.ToDecimal(fila.Cells["Stock.Ini"].Value);
                    stockReal = Convert.ToDecimal(fila.Cells["Stock.Cierre"].Value);
                    faltante = Convert.ToDecimal(fila.Cells["Faltante"].Value);
                    descripcion = descripcion.Length > longDesc ? descripcion.Substring(0, longDesc) : agregarCamposEnblanco(descripcion, longDesc, false);
                    stockIniString = stockIni.ToString("F2").Length.Equals(longPeso) ? stockIni.ToString("F2") : agregarCamposEnblanco(stockIni.ToString("F2"), longPeso, true);
                    stockRealString = stockReal.ToString("F2").Length.Equals(longPeso) ? stockReal.ToString("F2") : agregarCamposEnblanco(stockReal.ToString("F2"), longPeso, true);
                    faltanteString = faltante.ToString("F2").Length.Equals(longPeso) ? faltante.ToString("F2") : agregarCamposEnblanco(faltante.ToString("F2"), longPeso, true);
                    //return;
                    ticket.TextoIzquierda(descripcion + stockIniString + " " + stockRealString + " " + faltanteString);
                }
                ticket.LineasEnBlanco(4);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir ticket.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }                    
        }

        public void stockActual(string fechaDesde, string fechaHasta, DataGridView grilla)
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("Stock Actual");
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Desde: " + fechaDesde);
                ticket.TextoIzquierda("Hasta: " + fechaHasta);
                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                ticket.TextoIzquierda("Descripcion     S.Inic     Stock");
                ticket.LineasGuion();
                string descripcion, stockIniString, stockRealString;
                decimal stockIni, stockReal;
                int longDesc = 12;
                int longPeso = 7;
                foreach (DataGridViewRow fila in grilla.Rows)
                {
                    descripcion = fila.Cells["Codigo"].Value.ToString().Trim() + " " + fila.Cells["Corte"].Value.ToString();
                    if (descripcion.Length < longDesc)
                    {
                        for (int i = descripcion.Length; i < (longDesc - 1); i++)
                        {
                            descripcion += " ";
                        }
                    }
                    stockIni = Convert.ToDecimal(fila.Cells["Stock.Ini"].Value);
                    //stockReal = Convert.ToDecimal(fila.Cells["Faltante"].Value);//en grilla col. Stock es Faltante
                    descripcion = descripcion.Length > longDesc ? descripcion.Substring(0, longDesc) : agregarCamposEnblanco(descripcion, longDesc, false);
                    stockIniString = stockIni.ToString("F2").Length.Equals(longPeso) ? stockIni.ToString("F2") : agregarCamposEnblanco(stockIni.ToString("F2"), longPeso, true);
                    //stockRealString = stockReal.ToString("F2").Length.Equals(longPeso) ? stockReal.ToString("F2") : agregarCamposEnblanco(stockReal.ToString("F2"), longPeso, true);
                    stockRealString = fila.Cells["Stock.Un"].Value.ToString().Length.Equals(longPeso) ? fila.Cells["Stock.Un"].Value.ToString() : agregarCamposEnblanco(fila.Cells["Stock.Un"].Value.ToString(), longPeso, true);
                   
                    //return;
                    ticket.TextoIzquierda(descripcion + "   " + stockIniString + "   " + stockRealString);
                }
                ticket.LineasEnBlanco(4);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir ticket.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        public void acumVentas(string fechaDesde, string fechaHasta, DataGridView grilla)
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("Acum ventas");
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Desde: " + fechaDesde);
                ticket.TextoIzquierda("Hasta: " + fechaHasta);
                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                ticket.TextoIzquierda("Descrip.   Stock  Ventas   Diff.");
                ticket.LineasGuion();
                string descripcion, stockActualString, cantVentasString, diffString;
                decimal stockActual, cantVentas, diff;
                int longDesc = 8;
                int longPeso = 7;
                foreach (DataGridViewRow fila in grilla.Rows)
                {
                    descripcion = fila.Cells["Codigo"].Value.ToString().Trim() + " " + fila.Cells["Corte"].Value.ToString();
                    if (descripcion.Length < longDesc)
                    {
                        for (int i = descripcion.Length; i < (longDesc - 1); i++)
                        {
                            descripcion += " ";
                        }
                    }
                    stockActual = Convert.ToDecimal(fila.Cells["StockActual"].Value);
                    cantVentas = Convert.ToDecimal(fila.Cells["Ventas"].Value);//en grilla col. Stock es Faltante
                    diff = Convert.ToDecimal(fila.Cells["DIF"].Value);//en grilla col. Stock es Faltante
                    descripcion = descripcion.Length > longDesc ? descripcion.Substring(0, longDesc) : agregarCamposEnblanco(descripcion, longDesc, false);
                    stockActualString = stockActual.ToString("F2").Length.Equals(longPeso) ? stockActual.ToString("F2") : agregarCamposEnblanco(stockActual.ToString("F2"), longPeso, true);
                    cantVentasString = cantVentas.ToString("F2").Length.Equals(longPeso) ? cantVentas.ToString("F2") : agregarCamposEnblanco(cantVentas.ToString("F2"), longPeso, true);
                    diffString = diff.ToString("F2").Length.Equals(longPeso) ? diff.ToString("F2") : agregarCamposEnblanco(diff.ToString("F2"), longPeso, true);
                    //return;
                    int leng = (descripcion + " " + stockActualString + " " + cantVentasString + " " + diffString).Length;
                    ticket.TextoIzquierda(descripcion + " " + stockActualString + " " + cantVentasString + " " + diffString);
                }
                ticket.LineasEnBlanco(4);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir ticket.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        //completa espacios en blanco al texto hasta llegar a la longitud seteada en longitudTexto
        private string agregarCamposEnblanco(string texto, int longitudTexto, bool agregarAntes)
        {
            string textoModif = texto;
            for (int i = 0; i < (longitudTexto - texto.Length); i++)
            {
                textoModif = agregarAntes ? " " + textoModif : textoModif + " ";
            }
            return textoModif;
        }

        public void cortesConPrecios(DataTable dtCorte)
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("Listado Cortes");
                ticket.LineasEnBlanco(1);
                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                ticket.TextoIzquierda("Descripcion               Precio");
                ticket.LineasGuion();
                foreach (DataRow fila in dtCorte.Rows)
                {
                    ticket.TextoExtremos(fila["corte"].ToString(), Convert.ToDecimal(fila["precioKg"]).ToString("F2"));
                }
                ticket.LineasEnBlanco(3);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir ticket.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }

        public void ctaCtePersona(Entidades.Persona oPersona,DataTable dtMov)
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("Cuenta Corriente");
                ticket.LineasEnBlanco(1);
                //ticket.TextoIzquier("123456789*123456789*123456789*12");
                ticket.TextoIzquierda(oPersona.razonSocial);
                ticket.LineasGuion();
                foreach (DataRow fila in dtMov.Rows)
                {

                    ticket.TextoIzquierda(fila["fecha"].ToString());
                    //string detalle = fila["tabla"].ToString()+" ";
                    //detalle += fila["detalle"].ToString().Length > 9 ? fila["detalle"].ToString().Remove(8) : fila["detalle"].ToString();
                    ticket.TextoIzquierda(fila["tabla"].ToString() + " " + fila["detalle"].ToString());
                    ticket.TextoExtremos("   " + Convert.ToDecimal(fila["importe"]).ToString("F2"), "   " + Convert.ToDecimal(fila["saldo"]).ToString("F2"));
                }
                ticket.LineasEnBlanco(3);
                ticket.realizarImpresion();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir ticket.\n\n" + ex.Message + "\n\n" + ex.StackTrace);
            }
        }
    }
}
