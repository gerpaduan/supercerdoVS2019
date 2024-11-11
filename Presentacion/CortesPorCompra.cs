using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;

namespace Presentacion
{
    public class CortesPorCompra
    {
        public string corte;
        public float cantKgs;
        public float totalS;
        public float precioKg;
        public string sucursal;
        public long codigo;
        public int idSucursal;
        public int idCorte;
        private int index;
        private DateTime? creado;
        private bool balanza;

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public int IdCorte
        {
            get
            {
                return idCorte;
            }
            set
            {
                idCorte = value;
            }
        }
        public long Codigo
        {
            get
            {
                return codigo;
            }
            set
            {
                codigo = value;
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

        public float TotalS
        {
            get
            {
                return totalS;
            }
            set
            {
                totalS = value;
            }
        }

        public string Sucursal
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

        public string Corte
        {
            get
            {
                return corte; ;
            }
            set
            {
                corte = value;
            }
        }

        public int IdSucursal
        {
            get
            {
                return idSucursal;
            }
            set
            {
                idSucursal = value;
            }
        }

        public bool Balanza { get => balanza; set => balanza = value; }
    }
}
