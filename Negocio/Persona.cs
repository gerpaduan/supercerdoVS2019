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
            Entidades.Persona oPersonaE = oPersonaD.findById(id);
            if (oPersonaE != null && oPersonaE.Marca && oPersonaE.IdPropietario > 0)
            {
                oPersonaE.Propietario = oPersonaD.findById((int)oPersonaE.IdPropietario);
            }
            return oPersonaE;
        }

        public bool personaTieneCompras_Ventas(int idPersona)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.personaTieneCompras_Ventas(idPersona);
        }

        public DataTable buscarPersona(string texto, bool? marca)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.buscarPersona(texto, marca);
        }

        public DataTable getIva()
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.getIva();
        }
        public int existeCuit(string cuit)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.existeCuit(cuit);
        }
        public DataTable buscarProveedor(string buscarTexto)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.buscarProveedor(buscarTexto);

        }
        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            oPersonaD = new Datos.Persona();
            return oPersonaD.existenMarcasParecidas(buscarTexto, idMarca);

        }

     }
}
