// Contrato de un escenario del generador: una pantalla (o grupo de acciones) cuyas capturas y clips se generan juntos.
using AyudaMedia.Comun;
using Microsoft.Playwright;

namespace AyudaMedia.Escenarios;

public interface IEscenario
{
    // Nombre corto con el que se elige desde la linea de comandos (ej. "usuarios", "postventa").
    string Nombre { get; }
    string Descripcion { get; }
    // args: lo que sigue al nombre del escenario en la linea de comandos (ej. la accion a grabar).
    Task EjecutarAsync(IBrowser browser, Config cfg, IReadOnlyList<string> args);
}
