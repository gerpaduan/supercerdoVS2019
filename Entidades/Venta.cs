using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Venta
    {
         int idVenta;
         DateTime fechaVenta;
         DateTime creado;
         DateTime actualizado;
         string turno;
         string diaFestivo;
         string observaciones;
         Sucursal sucursal;
         Persona persona;
         private string nroRemito;
         private string estado;
         private string vendedor;
         private string tipoVenta;

         public string TipoVenta
         {
             get { return tipoVenta; }
             set { tipoVenta = value; }
         }

         public string Vendedor
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

        public DateTime Actualizado
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
