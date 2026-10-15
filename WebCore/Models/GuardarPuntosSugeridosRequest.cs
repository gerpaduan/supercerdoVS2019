// Cuerpo JSON que manda "Sugerir puntos de stock" (Reposicion/Puntos) al guardar: los puntos que el usuario tildó.
// Es input externo: Negocio.ReposicionStockCalculador.ValidarPuntosAGuardar lo revalida contra el stock real.
using System.Collections.Generic;

namespace WebCore.Models
{
    public class GuardarPuntosSugeridosRequest
    {
        public List<Entidades.PuntoAGuardarVm> Valores { get; set; } = new List<Entidades.PuntoAGuardarVm>();
    }
}
