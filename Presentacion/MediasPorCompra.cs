using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Presentacion
{
    public class MediasPorCompra
    {
        public int idMedia;
        public float kgMedia;
        public string nroTropa;
        public float precioMedia;
        public int compra;
        public string sucursal;
        public float totalS;
        public int idSucursal;

        public int IdMedia
        {
            get
            {
                return idMedia;
            }
            set
            {
                idMedia = value;
            }
        }

        public float KgMedia
        {
            get
            {
                return kgMedia;
            }
            set
            {
                kgMedia = value;
            }
        }

        public string NroTropa
        {
            get
            {
                return nroTropa;
            }
            set
            {
                nroTropa = value;
            }
        }

        public float PrecioMedia
        {
            get
            {
                return precioMedia;
            }
            set
            {
                precioMedia = value;
            }
        }

        public int Compra
        {
            get
            {
                return compra;
            }
            set
            {
                compra = value;
            }
        }

        public string Sucursal
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

        public float TotalS
        {
            get
            {
                return totalS;
            }
            set
            {
                totalS = value;
            }
        }

        public int IdSucursal
        {
            get
            {
                return idSucursal;
            }
            set
            {
                idSucursal = value;
            }
        }


    }
}
