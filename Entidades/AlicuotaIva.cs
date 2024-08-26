using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class AlicuotaIva
    {
        private int idIva;
        private float iva;
        private int orden;
        private bool mostrar;
        private float baseImponible;
        private float importe;

        public int IdIva { get => idIva; set => idIva = value; }
        public float Iva { get => iva; set => iva = value; }
        public int Orden { get => orden; set => orden = value; }
        public bool Mostrar { get => mostrar; set => mostrar = value; }
        public float BaseImponible { get => baseImponible; set => baseImponible = value; }
        public float Importe { get => importe; set => importe = value; }
    }
}
