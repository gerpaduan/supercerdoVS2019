using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entidades
{
    public class Persona
    {
        public int idPersona;
        public string razonSocial;
        public string otrosDatos;
        public string tipo;
        private bool ctaCte;

        private int idIva;

        public int IdIva
        {
            get { return idIva; }
            set { idIva = value; }
        }

        private string iva;

        public string Iva
        {
            get { return iva; }
            set { iva = value; }
        }

        private string identificacion;

        public string Identificacion
        {
            get { return identificacion; }
            set { identificacion = value; }
        }

        private string cuit;

        public string Cuit
        {
            get { return cuit; }
            set { cuit = value; }
        }
        private string telefono;

        public string Telefono
        {
            get { return telefono; }
            set { telefono = value; }
        }
        private string domicilio;

        public string Domicilio
        {
            get { return domicilio; }
            set { domicilio = value; }
        }
        private string ciudad;

        public string Ciudad
        {
            get { return ciudad; }
            set { ciudad = value; }
        }

        public bool CtaCte
        {
            get { return ctaCte; }
            set { ctaCte = value; }
        }
        private float bonificacion;

        public float Bonificacion
        {
          get { return bonificacion; }
          set { bonificacion = value; }
        }
    
        public int IdPersona
        {
            get
            {
                return idPersona;
            }
            set
            {
                idPersona = value;
            }
        }

        public string RazonSocial
        {
            get
            {
                return razonSocial;
            }
            set
            {
                razonSocial = value;
            }
        }

        public string OtrosDatos
        {
            get
            {
                return otrosDatos;
            }
            set
            {
                otrosDatos = value;
            }
        }

        public string Tipo
        {
            get
            {
                return tipo;
            }
            set
            {
                tipo = value;
            }
        }

        public DateTime Creado { get => creado; set => creado = value; }

        DateTime creado;
    }
}
