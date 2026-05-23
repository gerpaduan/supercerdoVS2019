using System.Runtime.Serialization;

namespace Carnisys.Balanza.Agent
{
    [DataContract]
    public sealed class ApiConfig
    {
        [DataMember(Name = "host")]
        public string Host { get; set; }

        [DataMember(Name = "port")]
        public int Port { get; set; }
    }
}
