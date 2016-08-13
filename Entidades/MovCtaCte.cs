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
