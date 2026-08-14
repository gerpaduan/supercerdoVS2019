using System.Collections.Generic;

namespace Web.Models
{
    public class DispositivosSegurosIndexVm
    {
        public DispositivosSegurosIndexVm()
        {
            Items = new List<Entidades.DispositivoSeguro>();
        }

        public bool PuedeAdministrar { get; set; }
        public List<Entidades.DispositivoSeguro> Items { get; set; }
    }
}
