using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class LineaVenta
    {
        public enum estados
        {
            NoAnulado = 0,
            Anulado = 1,
        }

        public static int getIdEstado(estados estadoParam)
        {
            return Convert.ToInt32(estadoParam);
        }

        public static bool esAnulado(int estadoLineaVenta)
        {
            return estadoLineaVenta.Equals(Convert.ToInt32(estados.Anulado));
        }

        int idLineaVenta;
         float cantKg;
         float precioKg;
         float bonificacion;
         float precioReal;
         Corte corte;
         Venta venta;
         int estado;
         int indexAnulado = -1;
         private bool pesoBalanza;
         int random;
        //Variables de redondeo y ajuste por tarjeta   
         float kgsAjusteTarj;
         float kgsRedondeo;
         float kgsTotalCalculado;

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

         public bool PesoBalanza
         {
             get { return pesoBalanza; }
             set { pesoBalanza = value; }
         }

         public int IndexAnulado
         {
             get { return indexAnulado; }
             set { indexAnulado = value; }
         }

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

        public float Bonificacion
        {
            get { return bonificacion; }
            set { bonificacion = value; }
        }

        public float PrecioReal
        {
            get { return precioReal; }
            set { precioReal = value; }
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
