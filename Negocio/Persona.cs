using Entidades;
using System.Data;
using Utilidades;

namespace Negocio
{
    public class Persona
    {
        private readonly Datos.Persona oPersonaD;
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public Persona(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oPersonaD = new Datos.Persona(empresa);
        }
        public void agregarPersona(Entidades.Persona oPersonaE)
        {
            
            oPersonaD.addOrEditPersona(oPersonaE);
        }

        public void modificarProveedor(Entidades.Persona oPersonaE)
        {
            
            oPersonaD.addOrEditPersona(oPersonaE);
        }

        public void addOrEditPersona(Entidades.Persona oPersonaE)
        {
            
            oPersonaD.addOrEditPersona(oPersonaE);
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            
            oPersonaD.eliminarPersona(oPersonaE);
        }

        public Entidades.Persona findById(int id)
        {
            
            Entidades.Persona oPersonaE = oPersonaD.findById(id);
            if (oPersonaE != null && oPersonaE.Marca && oPersonaE.IdPropietario > 0)
            {
                oPersonaE.Propietario = oPersonaD.findById((int)oPersonaE.IdPropietario);
            }
            return oPersonaE;
        }

        public Entidades.Persona getConsumidorFinal()
        {
            return findById(_param.GetInt(ParamKeys.IdConsumidorFinal, 0));// Entidades.Persona.idConsumidorFinal);
        }

        public bool personaTieneCompras_Ventas(int idPersona)
        {
            
            return oPersonaD.personaTieneCompras_Ventas(idPersona);
        }

        public DataTable buscarPersona(string texto, bool? marca)
        {
            
            return oPersonaD.buscarPersona(texto, marca);
        }

        public DataTable getIva()
        {
            
            return oPersonaD.getIva();
        }
        public int existeCuit(string cuit)
        {
            
            return oPersonaD.existeCuit(cuit);
        }
        public DataTable buscarProveedor(string buscarTexto)
        {
            
            return oPersonaD.buscarProveedor(buscarTexto);

        }
        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            
            return oPersonaD.existenMarcasParecidas(buscarTexto, idMarca);

        }

     }
}
