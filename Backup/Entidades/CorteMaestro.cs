using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CorteMaestro
    {
        private int idCorteMaestro;
        private string corteMaestro;
        private float porcentaje;

        public int IdCorteMestro
        {
            get
            {
                return idCorteMaestro;                
            }
            set
            {
                idCorteMaestro = value;
            }
        }

        public string CorteMaestroDesc
        {
            get
            {
                return corteMaestro;
            }
            set
            {
                corteMaestro = value;
            }
        }

        public float Porcentaje
        {
            get
            {
                return porcentaje;                
            }
            set
            {
                porcentaje = value;
            }
        }
    }
}
