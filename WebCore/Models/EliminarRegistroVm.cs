// Datos para el boton "Eliminar" de Compras, Stock y Movimientos (parcial Views/Shared/_EliminarRegistro.cshtml) y su modal
// (wwwroot/Scripts/app/eliminar-registro.js). Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
namespace WebCore.Models
{
    public class EliminarRegistroVm
    {
        // "COMPRA" | "STOCK" | "MOVIMIENTO"
        public string Modulo { get; set; } = "";
        public int IdRegistro { get; set; }

        // "la compra", "el movimiento de stock", "el movimiento": va en los titulos y avisos.
        public string Sustantivo { get; set; } = "";

        public string UrlPrevisualizar { get; set; } = "";
        public string UrlEliminar { get; set; } = "";

        // Donde se vuelve despues de eliminar (el registro ya no existe: va al listado).
        public string UrlRedireccion { get; set; } = "";

        // Cuenta compartida de produccion: la persona real se elige de la lista y pone su clave. Si no, se confirma al usuario de la sesion.
        public bool EsCuentaProduccion { get; set; }
        public int IdUsuarioSesion { get; set; }
        public string NombreUsuarioSesion { get; set; } = "";
        public System.Collections.Generic.List<UsuarioOpcion> Usuarios { get; set; } = new System.Collections.Generic.List<UsuarioOpcion>();

        // Si no es null el boton se muestra deshabilitado con este texto (por ejemplo, replica de movimientos entre bases).
        public string? MotivoDeshabilitado { get; set; }

        public class UsuarioOpcion
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = "";
        }
    }
}
