// Datos de la celda "Creado" de los listados (Egresos de caja, Movimientos, Stock): fecha de creacion,
// usuario creador y, si el registro se modifico, fecha y usuario de la ultima modificacion.
// Se renderiza con Views/Shared/_CeldaCreadoModificado.cshtml (mismo aspecto que la columna Creado de Elaborados).
namespace WebCore.Models
{
    public class CeldaCreadoModificadoVm
    {
        // Textos ya formateados (dd/MM/yyyy HH:mm:ss); vacios si el dato no existe.
        public string Creado { get; set; } = "";
        public string CreadoPor { get; set; } = "";
        public string Actualizado { get; set; } = "";
        public string ActualizadoPor { get; set; } = "";

        // Permite que cada listado decida cuando mostrar la etiqueta "Modificado"
        // (ej. Egresos de caja la oculta en tipos reservados que se actualizan solos).
        public bool MostrarModificado { get; set; } = true;
    }
}
