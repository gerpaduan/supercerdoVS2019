using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class EgresoCaja
    {
        public static int idPagoTarjeta = Entidades.Parametros.idPagoTarjetaEgresoCaja;
        public static int idCtaCte = Entidades.Parametros.idCtaCteEgresoCaja;

        public bool esEgresoCtaCte(int idTipoEgreso)
        {
            //100 es el IdTipoEgresoCaja para CtaCte
            return (idTipoEgreso.Equals(idCtaCte) || idTipoEgreso.Equals(Entidades.Parametros.idPagoCobroEgresoCaja));
        }

        public enum tablas
        {
            Compras,
            Ventas,
            Pagos,
            MovCtaCte,
        }

        public tablas getTablaEnum(string tabla)
        {
            tablas tablaEnum = tablas.Compras;
            switch (tabla)
            {
                case "Compras":
                    tablaEnum = tablas.Compras;
                    break;
                case "Ventas":
                    tablaEnum = tablas.Ventas;
                    break;
                case "Pagos":
                    tablaEnum = tablas.Pagos;
                    break;
                case "MovCtaCte":
                    tablaEnum = tablas.MovCtaCte;
                    break;
            }
            return tablaEnum;
        }

        int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        DateTime fecha;

        public DateTime Fecha
        {
            get { return fecha; }
            set { fecha = value; }
        }
        int idTipoEgresoCaja;

        public int IdTipoEgresoCaja
        {
            get { return idTipoEgresoCaja; }
            set { idTipoEgresoCaja = value; }
        }

        string tipoEgresoCaja;

        public string TipoEgresoCaja
        {
            get { return tipoEgresoCaja; }
            set { tipoEgresoCaja = value; }
        }

        string descripcion;

        public string Descripcion
        {
            get { return descripcion; }
            set { descripcion = value; }
        }
        string detalle;

        public string Detalle
        {
            get { return detalle; }
            set { detalle = value; }
        }
        float monto;

        public float Monto
        {
            get { return monto; }
            set { monto = value; }
        }

        private int? idCompra;

        public int? IdCompra
        {
            get { return idCompra; }
            set { idCompra = value; }
        }

        private string tabla;

        public string Tabla
        {
            get { return tabla; }
            set { tabla = value; }
        }

        private int? idTabla;

        public int? IdTabla
        {
            get { return idTabla; }
            set { idTabla = value; }
        }


        Sucursal sucursal;

        public Sucursal Sucursal
        {
            get { return sucursal; }
            set { sucursal = value; }
        }

        DateTime? creado;

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }
        int creadoPor;

        public int CreadoPor
        {
            get { return creadoPor; }
            set { creadoPor = value; }
        }

        Usuario creadoPorUser;

        public Usuario CreadoPorUser
        {
            get { return creadoPorUser; }
            set { creadoPorUser = value; }
        }

        DateTime? actualizado;

        public DateTime? Actualizado
        {
            get { return actualizado; }
            set { actualizado = value; }
        }
        int actualizadoPor;

        public int ActualizadoPor
        {
            get { return actualizadoPor; }
            set { actualizadoPor = value; }
        }

        Usuario actualizadoPorUser;

        public Usuario ActualizadoPorUser
        {
            get { return actualizadoPorUser; }
            set { actualizadoPorUser = value; }
        }
    }
}
