using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class MovCtaCte
    {
        public enum tipoMov
        { 
            Debito,
            Credito,
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

        public float getImporte(float importe, tipoMov tipo)
        {
            switch (tipo)
            {
                case tipoMov.Debito:
                    importe = tipo.Equals(tipoMov.Debito) && importe > 0 ? importe * -1 : importe;
                    break;
                case tipoMov.Credito:
                    importe = tipo.Equals(tipoMov.Credito) && importe < 0 ? importe * -1 : importe;
                    break;
                default:
                    break;
            }
            return importe;
        }

        public tipoMov getTipoMovEnum(string tipo)
        {
            return tipo.Equals("Debito") ? tipoMov.Debito : tipoMov.Credito;
        }

        public string getTipoMovOpuesto(tipoMov tipo)
        {
            string tipoMovOpuesto = "";
            switch (tipo)
            {
                case tipoMov.Debito:
                    tipoMovOpuesto = tipoMov.Credito.ToString();
                    break;
                case tipoMov.Credito:
                    tipoMovOpuesto = tipoMov.Debito.ToString();
                    break;
            }
            return tipoMovOpuesto;
        }

        public enum getBy
        {
            Id,
            TablaAndId,
        }

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        Persona persona;

        public Persona Persona
        {
            get { return persona; }
            set { persona = value; }
        }
        private DateTime fecha;

        public DateTime Fecha
        {
            get { return fecha; }
            set { fecha = value; }
        }

        private string tabla;

        public string Tabla
        {
            get { return tabla; }
            set { tabla = value; }
        }

        int idTabla;

        public int IdTabla
        {
            get { return idTabla; }
            set { idTabla = value; }
        }

        private string detalle;

        public string Detalle
        {
            get { return detalle; }
            set { detalle = value; }
        }

        private string tipo;

        public string Tipo
        {
            get { return tipo; }
            set { tipo = value; }
        }
	    
        private float importe;

        public float Importe
        {
            get { return importe; }
            set { importe = value; }
        }

	    Sucursal sucursal;

        public Sucursal Sucursal
        {
            get { return sucursal; }
            set { sucursal = value; }
        }

        private bool quitadoCtaCta;

        public bool QuitadoCtaCta
        {
            get { return quitadoCtaCta; }
            set { quitadoCtaCta = value; }
        }

        private DateTime? creado;

        public DateTime? Creado
        {
            get { return creado; }
            set { creado = value; }
        }
        private DateTime? actualizado;

        public DateTime? Actualizado
        {
            get { return actualizado; }
            set { actualizado = value; }
        }
        private Usuario creadoPor;

        public Usuario CreadoPor
        {
            get { return creadoPor; }
            set { creadoPor = value; }
        }
        private Usuario actualizadoPor;

        public Usuario ActualizadoPor
        {
            get { return actualizadoPor; }
            set { actualizadoPor = value; }
        }  
    }
}
