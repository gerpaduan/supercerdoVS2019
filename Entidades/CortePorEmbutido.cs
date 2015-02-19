using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CortePorEmbutido
    {
        public float kgUtilizado;
         public Embutido embutido;
         public Corte corte;

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

        public Embutido Embutido
        {
            get
            {
                return embutido;
            }
            set
            {
                embutido = value;
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
    }
}
