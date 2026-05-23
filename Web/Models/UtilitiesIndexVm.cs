using System.Collections.Generic;

namespace Web.Models
{
    public sealed class UtilitiesIndexVm
    {
        public List<UtilityItemVm> Agentes { get; set; }
        public List<UtilityItemVm> OtrasUtilidades { get; set; }
    }
}
