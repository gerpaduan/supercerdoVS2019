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
    }
}
