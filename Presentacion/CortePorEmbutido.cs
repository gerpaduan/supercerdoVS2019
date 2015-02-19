using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Presentacion
{
    public class CortePorEmbutido
    {
        public float kgUtilizado;
        public int codigo;
        public string corte;
        public int idCorte;

        public float KgUtilizado
        {
            get
            {
                return kgUtilizado;
            }
            set
            {
                kgUtilizado = value;
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
    }
}
