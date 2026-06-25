using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class CortePorCompra
    {
        private int idCortePorCompra;
        public Compra compra;
        public Corte corte;
        public float cantKgs;
        public float precioKg;
        private bool balanza;
        public Sucursal sucursal;
        private DateTime? creado;
        private Usuario creadoPor;
        private float margen;
        private float precioVenta;
        private bool actualizarPrecioVenta;


        public int IdCortePorCompra
        {
            get { return idCortePorCompra; }
            set { idCortePorCompra = value; }
        }

        private float desc_recargo;
        private float iva_compra;

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

        public float CantKgs
        {
            get
            {
                return cantKgs;
            }
            set
            {
                cantKgs = value;
            }
        }

        public float precioKgs
        {
            get
            {
                return precioKg;
            }
            set
            {
                precioKg = value;
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

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }

        public Usuario CreadoPor
        {
            get { return creadoPor; }
            set { creadoPor = value; }
        }

        public bool Balanza { get => balanza; set => balanza = value; }
        public float Margen { get => margen; set => margen = value; }
        public float PrecioVenta { get => precioVenta; set => precioVenta = value; }
        public bool ActualizarPrecioVenta { get => actualizarPrecioVenta; set => actualizarPrecioVenta = value; }
        public float Desc_recargo { get => desc_recargo; set => desc_recargo = value; }
        public float Iva_compra { get => iva_compra; set => iva_compra = value; }
    }
}
