using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Presentacion
{
    public class CortesPorMovimiento
    {
        private int idCortePorMovimiento;
        private int idCorte;
        private int codigo;
        private string corte;
        private float cantKg;

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

        public int IdCortePorMovimiento
        {
            get
            {
                return idCortePorMovimiento;
            }
            set
            {
                idCortePorMovimiento = value;
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
    }
}
