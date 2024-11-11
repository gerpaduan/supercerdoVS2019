using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CortePorCompra
    {
        private int idCortePorCompra;
        public Compra compra;
        public Corte corte;
        public float cantKgs;
        public float precioKg;
        private bool balanza;
        public Sucursal sucursal;
        private DateTime? creado;
        private Usuario creadoPor;


        public int IdCortePorCompra
        {
            get { return idCortePorCompra; }
            set { idCortePorCompra = value; }
        }

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

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }

        public Usuario CreadoPor
        {
            get { return creadoPor; }
            set { creadoPor = value; }
        }

        public bool Balanza { get => balanza; set => balanza = value; }
    }
}
