using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Venta
    {
        //se bonifican sólo si no son precios mayoristas
        public float bonificar(Entidades.Persona oCliente, float precio, bool esPrecioMayorista)
        {
            return oCliente.Bonificacion != 0 && !esPrecioMayorista ? (precio * (1 - (oCliente.Bonificacion / 100))) : precio;
        }

        public enum tipoComprobanteEnum
        {
            X,
            A,
            B
        }

        public enum formaPagoEnum
        {
            Efectivo,
            Debito,
            Credito,
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
    }
}
