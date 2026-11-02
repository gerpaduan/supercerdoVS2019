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

        // ===== Eliminar persona (docs/DECISIONS.md, 2026-11-02) =====

        // Devuelve "" si el usuario puede eliminar la persona; si no, el motivo para mostrarle. Reglas:
        // solo admin; solo personas de SU empresa (una global, idEmpresa = 0, o de otra empresa no); no el
        // Consumidor Final (el POS lo usa); no una marca (tiene su flujo en Productos); y ningun registro a su
        // nombre (la base no lo impide: el DELETE de una persona con pagos o ventas deja esos registros huerfanos).
        public string motivoNoEliminable(Entidades.Persona persona, Entidades.Usuario usuario)
        {
            if (persona == null || persona.IdPersona <= 0)
                return "No se encontró la persona.";
            if (usuario == null || !usuario.Admin)
                return "Solo un administrador puede eliminar personas.";
            if (persona.IdEmpresa != usuario.IdEmpresa)
                return "Solo se pueden eliminar personas de la empresa en la que estás trabajando.";
            if (_param != null && esConsumidorFinal(persona))
                return "No se puede eliminar al Consumidor Final: lo usa el punto de venta.";
            if (persona.Marca)
                return "Es una marca: se elimina desde Productos.";

            var registros = oPersonaD.registrosAsociados(persona.IdPersona);
            if (registros == null || registros.Rows.Count == 0)
                return "";

            var partes = new System.Collections.Generic.List<string>();
            foreach (DataRow fila in registros.Rows)
                partes.Add(System.Convert.ToString(fila["tipo"]) + " (" + System.Convert.ToInt32(fila["cantidad"]) + ")");
            return "No se puede eliminar: tiene registros a su nombre: " + string.Join(", ", partes) + ".";
        }

        // Elimina la persona si motivoNoEliminable lo permite. Devuelve "" si la elimino o el motivo si no.
        // Revalida aca (no confia en lo que la pantalla mostro) y lo hace justo antes del DELETE.
        public string eliminarPersonaSiCorresponde(Entidades.Persona persona, Entidades.Usuario usuario)
        {
            string motivo = motivoNoEliminable(persona, usuario);
            if (motivo.Length > 0)
                return motivo;

            oPersonaD.eliminarPersona(persona);
            return "";
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
