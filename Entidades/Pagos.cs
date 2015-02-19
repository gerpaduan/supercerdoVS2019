using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Pagos
    {
        private int idPago;
        private string nroRecibo;
        private DateTime fechaPago;
        private Persona proveedor;
        private decimal importe;
        private string observaciones;
        private string tipoPago;

        public Persona Persona
        {
            get
            {
                return proveedor;
            }
            set
            {
                proveedor = value;
            }
        }

        public int IdPago
        {
            get
            {
                return idPago;
            }
            set
            {
                idPago = value;
            }
        }

        public DateTime FechaPago
        {
            get
            {
                return fechaPago;
            }
            set
            {
                fechaPago = value;
            }
        }

        public string NroRecibo
        {
            get
            {
                return nroRecibo;
            }
            set
            {
                nroRecibo = value;
            }
        }

        public decimal Importe
        {
            get
            {
                return importe;
            }
            set
            {
                importe = value;
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

        public string TipoPago
        {
            get
            {
                return tipoPago;
            }
            set
            {
                tipoPago = value;
            }
        }
    }
}
