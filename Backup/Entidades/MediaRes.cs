using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class MediaRes
    {
        public int idMedia;
        public float kgMedia;
        public string nroTropa;
        public float precioMedia;
        public Compra compra;
        public Sucursal sucursal;

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

        public Compra Compra
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
    }
}
