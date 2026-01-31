using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Presentacion.Ticket
{
    class Ticket_X
    {
        public Entidades.Venta oVentaE = null;
        Negocio.Venta oVentaN = new Negocio.Venta(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
        public void generarTicket_X()
        {
            Ticket.CreaTicket ticket = new Ticket.CreaTicket();

            //imprimir si está checked
            ticket.imprimir = true;
            ticket.DobleTamanoA();
            ticket.TextoCentro("x");
            ticket.DobleTamanoA(false);
            ticket.NoValidoComoFactura();
            ticket.DobleTamanoB();
            ticket.TextoCentro(ConfigurationManager.AppSettings["Negocio"].ToString());
            ticket.DobleTamanoB(false);
            string NegocioAgregado1 = ConfigurationManager.AppSettings["NegocioAgregado1"].ToString();
            string NegocioAgregado2 = ConfigurationManager.AppSettings["NegocioAgregado2"].ToString();
            string NegocioAgregado3 = ConfigurationManager.AppSettings["NegocioAgregado3"].ToString();
            string NegocioAgregado4 = ConfigurationManager.AppSettings["NegocioAgregado4"].ToString();

            if (!(NegocioAgregado1.Equals("-") || string.IsNullOrEmpty(NegocioAgregado1)))
                ticket.TextoCentro(NegocioAgregado1);
            if (!(NegocioAgregado2.Equals("-") || string.IsNullOrEmpty(NegocioAgregado2)))
                ticket.TextoCentro(NegocioAgregado2);
            if (!(NegocioAgregado3.Equals("-") || string.IsNullOrEmpty(NegocioAgregado3)))
                ticket.TextoIzquierda(NegocioAgregado3);
            //if (!(NegocioAgregado4.Equals("-") || string.IsNullOrEmpty(NegocioAgregado4)))
            //    ticket.TextoIzquierda(NegocioAgregado4);

            ticket.LineasEnBlanco(1);
            if (oVentaE.EnCtaCte && oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                ticket.TextoCentro("A Cta. Cte.");
            //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
            ticket.TextoIzquierda("A " + oVentaE.Persona.razonSocial);
            string formaPagoImprimir = oVentaE.PagoMixtoEfectivo > 0 ? oVentaE.FormaPago.ToString() + "|Efvo" : oVentaE.FormaPago.ToString();
            ticket.TextoIzquierda("Forma Pago: " + formaPagoImprimir);
            ticket.TextoIzquierda("Nro. T. " + oVentaE.IdVenta.ToString());
            ticket.TextoExtremos("Fecha: " + oVentaE.FechaVenta.Date.ToString(), "Hora: " + oVentaE.FechaVenta.TimeOfDay.ToString());
            //ticket.LineasEnBlanco(0);
            ticket.LineasGuion();

            //for (int index = 0; index < oVentaE.LineasVenta.Count; index++)
            //{
            //    //ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
            //    ticket.AgregaArticulo(linea.Corte.corte.ToString(),
            //        linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
            //}
            foreach (Entidades.LineaVenta linea in oVentaE.LineasVenta)
            {

                ticket.AgregaArticulo(linea.Corte.corte.ToString(),
                    linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
            }

            ticket.TextoDerecha("-------");

            ticket.Negrita();
            ticket.AgregaTotales("Total", oVentaE.TotalImporte);
            ticket.Negrita(false);
            //si se ingresa la cantidad del pago se imprime
            if (oVentaE.Abona > 0)
            {
                ticket.AgregaTotales("Pago", oVentaE.Abona);
                ticket.AgregaTotales("Vuelto", oVentaE.Cambio);
            }
            ticket.LineasEnBlanco(1);
            ticket.TextoIzquierda("Articulos: " + oVentaE.CantItems);
            ticket.TextoIzquierda("Cajero: " + oVentaE.Vendedor.Id);
            ticket.GraciasPorSuCompra();
            ticket.LineasEnBlanco(2);
            ticket.realizarImpresion();
        }
    }
}
