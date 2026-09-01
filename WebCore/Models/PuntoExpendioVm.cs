// Port parcial de Web/Models/PuntoExpendioVm.cs -- solo el ABM de sectores (ver cabecera de
// WebCore/Controllers/PuntosExpendioController.cs). PuntoExpendioEditVm/PuntoExpendioLineaVm NO
// se portan: solo los usan Abrir/Guardar (crea una Venta/expendio real), fuera de alcance de este
// slice de solo lectura + catalogo de sectores.
using System.Collections.Generic;

namespace WebCore.Models
{
    public class SectorAbmVm
    {
        public SectorAbmVm()
        {
            Sectores = new List<SectorResumenVm>();
        }

        public string SectorOriginal { get; set; }
        public string Nombre { get; set; }
        public List<SectorResumenVm> Sectores { get; set; }
    }

    public class SectorResumenVm
    {
        public string Nombre { get; set; }
        public bool EnUso { get; set; }
    }
}
