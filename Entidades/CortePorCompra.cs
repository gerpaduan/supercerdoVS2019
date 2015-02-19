using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CortePorCompra
    {
        public Compra compra;
        public Corte corte;
        public float cantKgs;
        public float precioKg;
        public Sucursal sucursal;

        public Compra Compra
        {
            get
            {
                return compra;
            }
            set
            {
                compra = value;
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

        public float CantKgs
        {
            get
            {
                return cantKgs;
            }
            set
            {
                cantKgs = value;
            }
        }

        public float precioKgs
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
    }
}
