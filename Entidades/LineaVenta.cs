using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class LineaVenta
    {
         float cantKg;
         float precioKg;
         Corte corte;
         Venta venta;
         int estado;

        public float CantKg
        {
            get
            {
                return cantKg;
            }
            set
            {
                cantKg = value;
            }
        }

        public float PrecioKg
        {
            get
            {
                return precioKg;
            }
            set
            {
                precioKg = value;
            }
        }

        public Corte Corte
        {
            get
            {
                return corte;
            }
            set
            {
                corte = value;
            }
        }

        public Venta Venta
        {
            get
            {
                return venta;
            }
            set
            {
                venta = value;
            }
        }

        public int Estado
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
