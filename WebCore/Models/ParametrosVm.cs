using System.Collections.Generic;

namespace WebCore.Models
{
    public class ParametrosEmpresaIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; } = "";
        public List<ParametroEmpresaItemVm> Items { get; set; } = new List<ParametroEmpresaItemVm>();
    }

    public class ParametroEmpresaItemVm
    {
        public int IdParametro { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public int Tipo { get; set; }
        public string TipoDescripcion { get; set; } = "";
        public string Valor { get; set; } = "";
        public bool ValorBool { get; set; }

        // Texto de ayuda para mostrar la fila (no viaja en el POST; el controller lo completa en cada render).
        public ParametroAyuda? Ayuda { get; set; }

        // Solo para el parametro formasPagoFacturaAutomatica: claves tildadas (una casilla por forma de pago). Al
        // guardar, el controller las convierte en el Valor del parametro (FormasPagoFacturaAutomatica.Serializar).
        public List<string> FormasPagoSeleccionadas { get; set; } = new List<string>();
    }
}
