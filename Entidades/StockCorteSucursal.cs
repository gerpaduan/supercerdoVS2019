using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class StockCorteSucursal
    {
        private float stock;
        private Corte corte;
        private Sucursal sucursal;
        private float stockTeorico;

        public float Stock
        {
            get
            {
                return stock;
            }
            set
            {
                stock = value;
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

        public Sucursal Sucursal
        {
            get
            {
                return sucursal;
            }
            set
            {
                sucursal = value;
            }
        }

        public float StockTeorico
        {
            get
            {
                return stockTeorico;
            }
            set
            {
                stockTeorico = value;
            }
        }
    }
}
