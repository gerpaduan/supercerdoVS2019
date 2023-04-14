using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class StockMaestroSucursal
    {
        private float stock;
        private Sucursal sucursal;
        private CorteMaestro corteMaestro;
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

        public CorteMaestro CorteMaestro
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
