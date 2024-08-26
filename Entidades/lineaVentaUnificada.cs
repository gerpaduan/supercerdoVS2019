using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class lineaVentaUnificada
    {
        int idLineaVenta;
        public int idCorte;
        public int codigo;
        public string corte;
        public float cantKgs;
        public float precioKg;
        private float idAlicuotaIva;
        private float alicuotaIva;
        public float totalS;
        public float bonificacion;
        public string estado;
        private bool pesoBalanza;
        int indexAnulado = -1;
        int random;
        //Variables de redondeo y ajuste por tarjeta   
        public float kgsAjusteTarj;
        public float kgsRedondeo;
        public float kgsTotalCalculado;

        public float KgsTotalCalculado
        {
            get { return kgsTotalCalculado; }
            set { kgsTotalCalculado = value; }
        }

        public float KgsRedondeo
        {
            get { return kgsRedondeo; }
            set { kgsRedondeo = value; }
        }

        public float KgsAjusteTarj
        {
            get { return kgsAjusteTarj; }
            set { kgsAjusteTarj = value; }
        }

        public int Random
        {
            get { return random; }
            set { random = value; }
        }

        public int IdLineaVenta
        {
            get { return idLineaVenta; }
            set { idLineaVenta = value; }
        }

        public int IndexAnulado
        {
            get { return indexAnulado; }
            set { indexAnulado = value; }
        }

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

        public float AlicuotaIva { get => alicuotaIva; set => alicuotaIva = value; }
        public float IdAlicuotaIva { get => idAlicuotaIva; set => idAlicuotaIva = value; }
    }
}
