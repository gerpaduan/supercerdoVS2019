using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class TemporalLineaVenta
    {

        DateTime fechaInicioPesada;

        public DateTime FechaInicioPesada
        {
            get { return fechaInicioPesada; }
            set { fechaInicioPesada = value; }
        }

        Usuario vendedor;

        public Usuario Vendedor
        {
            get { return vendedor; }
            set { vendedor = value; }
        }

        bool ventaEnCurso;

        public bool VentaEnCurso
        {
            get { return ventaEnCurso; }
            set { ventaEnCurso = value; }
        }

        Entidades.Sucursal sucursal;

        public Entidades.Sucursal Sucursal
        {
            get { return sucursal; }
            set { sucursal = value; }
        }

         float cantKg;
         float totalCorte;

         public float TotalCorte
         {
             get { return totalCorte; }
             set { totalCorte = value; }
         }
         Corte corte;

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
    }
}
