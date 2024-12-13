using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Venta
    {
        //se bonifican sólo si no son precios ingresoRapidoEmbutidos
        public float bonificar(Entidades.Persona oCliente, float precio, bool esPrecioIngresoRapidoEmbutido)
        {
            return oCliente.Bonificacion != 0 && !esPrecioIngresoRapidoEmbutido ? (precio * (1 - (oCliente.Bonificacion / 100))) : precio;
        }

        public float getImporteVenta(Entidades.Venta oVentaE)
        {
            float importe = 0;

            foreach (Entidades.LineaVenta linea in oVentaE.lineasVenta)
            {
                importe += linea.CantKg * linea.PrecioKg;
            }

            return importe;
        }

        public float getKgsVenta(Entidades.Venta oVentaE)
        {
            float totalKgs = 0;

            foreach (Entidades.LineaVenta linea in oVentaE.lineasVenta)
            {
                totalKgs += linea.CantKg;
            }

            return totalKgs;
        }

        //0-SinTicket | 1-Ticket | 2-Factura
        public enum imprimirCbteEnum
        {
            SinTicket,
            Ticket,
            Factura,
            Nulo
        }

        public enum tipoComprobanteEnum
        {
            X,
            A,
            B
        }

        public enum formaPagoEnum //Al cambiar estos datos modificar los valores en formVentaCaja 
        {
            Efectivo,
            Debito,
            Credito,
            CtaCte,
            Qr,
            Transferencia,
            Nulo
        }

         int idVenta;
         DateTime fechaVenta;
         DateTime creado;
         DateTime? actualizado;
         string turno;
         string diaFestivo;
         string observaciones;
         Sucursal sucursal;
         Persona persona;
         private string nroRemito;
         private string estado;
         Usuario vendedor;
         private string tipoVenta;
         bool enCtaCte;
         private string formaPago;
         private string cuit;
         private string email;
         private char tipoComprobante;
         private float acumRedondeoKgs;
         private float acumRedondeoImporte;
         private float comisionTarjeta;
         private string imprimirTipoCbte;//0-SinTicket | 1-Ticket | 2-Factura.
         private float totalImporte;        
        //campos para punto de expendio
        private string identificacionExpendio;
        private string sector; 
        private string cantItems;
        private string serialCPU;

         public float TotalImporte
         {
             get { return totalImporte; }
             set { totalImporte = value; }
         }

         public string ImprimirTipoCbte
         {
             get { return imprimirTipoCbte; }
             set { imprimirTipoCbte = value; }
         }

         public float ComisionTarjeta
         {
             get { return comisionTarjeta; }
             set { comisionTarjeta = value; }
         }

         public float AcumRedondeoImporte
         {
             get { return acumRedondeoImporte; }
             set { acumRedondeoImporte = value; }
         }

         public float AcumRedondeoKgs
         {
             get { return acumRedondeoKgs; }
             set { acumRedondeoKgs = value; }
         }


         public char TipoComprobante
         {
             get { return tipoComprobante; }
             set { tipoComprobante = value; }
         }


         public string Email
         {
             get { return email; }
             set { email = value; }
         }

         public string Cuit
         {
             get { return cuit; }
             set { cuit = value; }
         }

         public string FormaPago
         {
             get { return formaPago; }
             set { formaPago = value; }
         }

         public bool EnCtaCte
         {
             get { return enCtaCte; }
             set { enCtaCte = value; }
         }

         List<Entidades.LineaVenta> lineasVenta;

         public List<Entidades.LineaVenta> LineasVenta
         {
             get { return lineasVenta; }
             set { lineasVenta = value; }
         }

         public string TipoVenta
         {
             get { return tipoVenta; }
             set { tipoVenta = value; }
         }

         public Usuario Vendedor
         {
             get { return vendedor; }
             set { vendedor = value; }
         }

        public int IdVenta
        {
            get
            {
                return idVenta;
            }
            set
            {
                idVenta = value;
            }
        }

        public DateTime FechaVenta
        {
            get
            {
                return fechaVenta;
            }
            set
            {
                fechaVenta = value;
            }
        }

        public string Turno
        {
            get
            {
                return turno;
            }
            set
            {
                turno = value;
            }
        }

        public String DiaFestivo
        {
            get
            {
                return diaFestivo;
                
            }
            set
            {
                diaFestivo = value;
            }
        }

        public DateTime Creado
        {
            get
            {
                return creado;
            }
            set
            {
                creado = value;
            }
        }

        public DateTime?  Actualizado
        {
            get
            {
                return actualizado;
            }
            set
            {
                actualizado = value;
            }
        }
        public string Observaciones
        {
            get
            {
                return observaciones;
            }
            set
            {
                observaciones = value;
            }
        }

        public Sucursal Sucursal
        {
            get
            {
                return sucursal;
            }
            set
            {
                sucursal = value;
            }
        }

        public Persona Persona
        {
            get
            {
               return persona;
            }
            set
            {
                persona = value;
            }
        }

        public string NroRemito
        {
            get
            {
                return nroRemito;

            }
            set
            {
                nroRemito = value;
            }
        }

        public string Estado
        {
            get
            {
                return estado;
            }
            set
            {
                estado = value;
            }
        }

        public string IdentificacionExpendio { get => identificacionExpendio; set => identificacionExpendio = value; }
        public string Sector { get => sector; set => sector = value; }
        public string CantItems { get => cantItems; set => cantItems = value; }
        public string SerialCPU { get => serialCPU; set => serialCPU = value; }
    }
}
