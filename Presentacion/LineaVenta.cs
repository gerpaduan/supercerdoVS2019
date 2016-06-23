using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Presentacion
{
    public class LineaVenta
    {
        public int idCorte;
        public int codigo;
        public string corte;
        public float cantKgs;
        public float precioKg;
        public float totalS;
        public float bonificacion;
        public string estado;
        private bool pesoBalanza;

        public float Bonificacion
        {
            get { return bonificacion; }
            set { bonificacion = value; }
        }

        public bool PesoBalanza
        {
            get { return pesoBalanza; }
            set { pesoBalanza = value; }
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

        public int Codigo
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

        public string Corte
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
    }
}
