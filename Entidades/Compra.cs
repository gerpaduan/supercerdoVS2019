using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Compra
    {
        public enum accion
        { 
            Modificar,
            Agregar,
        }
        public enum tipoCompraEnum
        {
            MediaRes = 0,
            Cortes = 6,
            IngresoStock = 1,
            EgresoStock = 2,
            CierreStock = 3,
        }

        public static string tipoCompraToString(tipoCompraEnum tipoCompraToConvert)
        {
            string tipoCompraToString = "";
            switch (tipoCompraToConvert)
            {
                case Compra.tipoCompraEnum.MediaRes:
                    tipoCompraToString = "Media Res";
                    break;
                case Compra.tipoCompraEnum.Cortes:
                    tipoCompraToString = "Cortes";
                    break;
                case Compra.tipoCompraEnum.IngresoStock:
                    tipoCompraToString = "Ingreso Stock";
                    break;
                case Compra.tipoCompraEnum.EgresoStock:
                    tipoCompraToString = "Egreso Stock";
                    break;
                case Compra.tipoCompraEnum.CierreStock:
                    tipoCompraToString = "Cierre Stock";
                    break;
            }
            return tipoCompraToString;
        }

        public static tipoCompraEnum tipoCompraToEnum(string  tipoCompraToConvert)
        {
            tipoCompraEnum tipoCompraToEnum = tipoCompraEnum.IngresoStock;
            switch (tipoCompraToConvert)
            {
                case "Media Res":
                    tipoCompraToEnum = tipoCompraEnum.MediaRes;
                    break;
                case "Cortes":
                    tipoCompraToEnum = tipoCompraEnum.Cortes;
                    break;
                case "Ingreso Stock":
                    tipoCompraToEnum = tipoCompraEnum.IngresoStock;
                    break;
                case "Egreso Stock":
                    tipoCompraToEnum = tipoCompraEnum.EgresoStock;
                    break;
                case "Cierre Stock":
                    tipoCompraToEnum = tipoCompraEnum.CierreStock;
                    break;
            }
            return tipoCompraToEnum;
        }

        private int idCompra;
        private string nroRemito;
        private DateTime fechaCompra;
        private string observaciones;
        private string estado;
        private Persona proveedor;
        private string tipoCompra;
        private Sucursal sucursal;
        private DateTime? creado;
        private DateTime? actualizado;

        //private Usuario creadoPor;
        //private Usuario actualizadoPor;
        
        //public Usuario CreadoPor
        //{
        //    get { return creadoPor; }
        //    set { creadoPor = value; }
        //}

        //public Usuario ActualizadoPor
        //{
        //    get { return actualizadoPor; }
        //    set { actualizadoPor = value; }
        //}

        public DateTime? Actualizado
        {
            get { return actualizado; }
            set { actualizado = value; }
        }

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }

        public int IdCompra
        {
            get
            {
                return idCompra;
            }
            set
            {
                idCompra = value;
            }
        }

        public DateTime FechaCompra
        {
            get
            {
                return fechaCompra;
            }
            set
            {
                fechaCompra = value;
            }
        }

        public string Estado
        {
            get
            {
                return estado;
            }
            set
            {
                estado = value;
            }
        }

        public string NroRemito
        {
            get
            {
                return nroRemito;
            }
            set
            {
                nroRemito = value;
            }
        }

        public string Observaciones
        {
            get
            {
                return observaciones;
            }
            set
            {
                observaciones = value;
            }
        }

        public Persona Proveedor
        {
            get
            {
                return proveedor;
            }
            set
            {
                proveedor = value;
            }
        }

        public string TipoCompra
        {
            get
            {
                return tipoCompra;
            }
            set
            {
                tipoCompra = value;
            }
        }
        
        public Sucursal Sucursal
        {
            get { return sucursal; }
            set { sucursal = value; }
        }
    }
}
