using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Compra
    {
        public enum tipoCompra
        { 
                    
        }
        private int idCompra;
        private string nroRemito;
        private DateTime fechaCompra;
        private string observaciones;
        private string estado;
        private Persona proveedor;
        private string tipoCompra;

        public int IdCompra
        {
            get
            {
                return idCompra;
            }
            set
            {
                idCompra = value;
            }
        }

        public DateTime FechaCompra
        {
            get
            {
                return fechaCompra;
            }
            set
            {
                fechaCompra = value;
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

        public Persona Proveedor
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

        public string TipoCompra
        {
            get
            {
                return tipoCompra;
            }
            set
            {
                tipoCompra = value;
            }
        }
    }
}
