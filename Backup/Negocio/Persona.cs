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
            oPersonaD.addOrEditPersona(oPersonaE);
        }

        public void modificarProveedor(Entidades.Persona oPersonaE)
        {
            oPersonaD = new Datos.Persona();
            oPersonaD.addOrEditPersona(oPersonaE);
        }

        public void addOrEditPersona(Entidades.Persona oPersonaE)
        {
            oPersonaD = new Datos.Persona();
            oPersonaD.addOrEditPersona(oPersonaE);
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            oPersonaD = new Datos.Persona();
            oPersonaD.eliminarPersona(oPersonaE);
        }

        public Entidades.Persona findById(int id)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.findById(id);
        }

        public DataTable buscarPersona(string texto)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.buscarPersona(texto);
        }

        public DataTable getIva()
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.getIva();
        }

        public DataTable buscarProveedor(string buscarTexto)
        {
            oPersonaD = new Datos.Persona();
             return oPersonaD.buscarProveedor(buscarTexto);
           
        }

    }
}
