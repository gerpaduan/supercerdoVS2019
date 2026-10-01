using Entidades;
using System.Data;
using Utilidades;

namespace Negocio
{
    public class Persona
    {
        private readonly Contratos.IPersonaRepository oPersonaD;
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        // Constructor existente: SIN CAMBIOS de comportamiento. Sigue usando SQL Server
        // (Datos.Persona) tal como siempre -- todos los llamadores actuales de Web y
        // Presentacion/wsAFIP quedan intactos.
        public Persona(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oPersonaD = new Datos.Persona(empresa, param);
        }

        // Constructor nuevo, aditivo: permite inyectar cualquier implementacion de
        // IPersonaRepository (ej. DatosPostgres.PersonaPg). Usado unicamente por el
        // controller de comparacion de la migracion a Postgres (Etapa 2) -- ningun
        // llamador existente lo usa todavia.
        public Persona(Contratos.IPersonaRepository repositorio, IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa; _param = param;
            oPersonaD = repositorio ?? throw new System.ArgumentNullException(nameof(repositorio));
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

        public int addOrEditPersonaConId(Entidades.Persona oPersonaE)
        {
            return oPersonaD.addOrEditPersonaConId(oPersonaE);
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {

            oPersonaD.eliminarPersona(oPersonaE);
        }

        // ===== Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01) =====

        // Ids de las personas marcadas como "cuenta corriente reservada".
        public System.Collections.Generic.HashSet<int> idsPersonasReservadas()
        {
            return oPersonaD.idsPersonasReservadas();
        }

        // Ids de los registros de `tabla` (RestriccionCtaCteReservada.Tabla*) que el usuario
        // restringido NO puede ver: pertenecen a una persona reservada y no los cargo el mismo
        // desde la apertura de su caja. Con restriccion == null (usuario autorizado) no hay nada
        // oculto y no se consulta la base.
        public System.Collections.Generic.HashSet<int> idsRegistrosOcultos(string tabla, Entidades.RestriccionCtaCteReservada restriccion)
        {
            if (restriccion == null) return new System.Collections.Generic.HashSet<int>();
            return oPersonaD.idsRegistrosOcultos(tabla, restriccion.IdUsuario, restriccion.Desde);
        }

        public Entidades.Persona findById(int id)
        {
            
            Entidades.Persona oPersonaE = oPersonaD.findById(id);

            //oPersonaE.ConsumidorFinal = esConsumidorFinal(oPersonaE);

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

        public bool esConsumidorFinal(Entidades.Persona oPersonaE)
        {
            return (oPersonaE != null && oPersonaE.idPersona > 0 && _param.GetInt(ParamKeys.IdConsumidorFinal, 0) == oPersonaE.idPersona);
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
        public DataTable obtenerProveedoresConCompras()
        {
            return oPersonaD.obtenerProveedoresConCompras();
        }
        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            
            return oPersonaD.existenMarcasParecidas(buscarTexto, idMarca);

        }

     }
}
