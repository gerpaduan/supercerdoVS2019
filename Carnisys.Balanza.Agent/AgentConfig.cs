using System.Runtime.Serialization;

namespace Carnisys.Balanza.Agent
{
    [DataContract]
    public sealed class AgentConfig
    {
        [DataMember(Name = "balanza")]
        public BalanzaConfig Balanza { get; set; }

        [DataMember(Name = "api")]
        public ApiConfig Api { get; set; }
    }
}
