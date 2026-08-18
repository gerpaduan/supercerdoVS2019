using System.Data;

namespace Contratos
{
    // Espeja exactamente los metodos publicos de Datos.Persona (SQL Server) tal como
    // estan hoy, para que la extraccion sea mecanica y no cambie ningun comportamiento.
    // Ver docs/06-datos-e-integraciones/rls-postgres.md y el plan de la Etapa 2.
    public interface IPersonaRepository
    {
        void addOrEditPersona(Entidades.Persona oPersonaE);
        int addOrEditPersonaConId(Entidades.Persona oPersonaE);
        void eliminarPersona(Entidades.Persona oPersonaE);
        Entidades.Persona findById(int id);
        DataTable buscarProveedor(string buscarTexto);
        DataTable buscarPersona(string buscarTexto, bool? marca);
        DataTable getIva();
        int existeCuit(string cuit);
        bool personaTieneCompras_Ventas(int idPersona);
        DataTable obtenerProveedores();
        DataTable obtenerProveedoresConCompras();
        DataTable existenMarcasParecidas(string buscarTexto, int idMarca);
    }
}
