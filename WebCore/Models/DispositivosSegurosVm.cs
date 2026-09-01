using System.Collections.Generic;

namespace WebCore.Models
{
    public class DispositivosSegurosIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<Entidades.DispositivoSeguro> Items { get; set; } = new List<Entidades.DispositivoSeguro>();
    }
}
