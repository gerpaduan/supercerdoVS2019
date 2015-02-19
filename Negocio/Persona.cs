using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Persona
    {
        Datos.Persona oPersonaD;

        public void agregarPersona(Entidades.Persona oPersonaE)
        {
            oPersonaD = new Datos.Persona();
            oPersonaD.agregarPersona(oPersonaE);
        }

        public void modificarProveedor(Entidades.Persona oPersonaE)
        {
            oPersonaD = new Datos.Persona();
            oPersonaD.modificarProveedor(oPersonaE);
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            oPersonaD = new Datos.Persona();
            oPersonaD.eliminarPersona(oPersonaE);
        }

        public DataTable buscarPersona(string texto)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.buscarPersona(texto);
            
        }

        public DataTable buscarProveedor(string buscarTexto)
        {
            oPersonaD = new Datos.Persona();
             return oPersonaD.buscarProveedor(buscarTexto);
           
        }

    }
}
