using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CortePorMovimiento
    {
        private int idCorteMovimiento;
        private Movimiento movimiento;
        private Corte corte;
        private float cantKg;
        private bool pesoBalanza;

        public bool PesoBalanza
        {
            get { return pesoBalanza; }
            set { pesoBalanza = value; }
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

        public Movimiento Movimientos
        {
            get
            {
                return movimiento;
            }
            set
            {
                movimiento = value;
            }
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

        public int IdCorteMovimiento
        {
            get
            {
                return idCorteMovimiento;
            }
            set
            {
                idCorteMovimiento = value;
            }
        }
    }
}
