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
