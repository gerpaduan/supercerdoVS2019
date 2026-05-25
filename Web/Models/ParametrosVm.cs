using System.Collections.Generic;

namespace Web.Models
{
    public class ParametrosEmpresaIndexVm
    {
        public ParametrosEmpresaIndexVm()
        {
            Items = new List<ParametroEmpresaItemVm>();
        }

        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; }
        public List<ParametroEmpresaItemVm> Items { get; set; }
    }

    public class ParametroEmpresaItemVm
    {
        public int IdParametro { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public int Tipo { get; set; }
        public string TipoDescripcion { get; set; }
        public string Valor { get; set; }
        public bool ValorBool { get; set; }
    }
}
