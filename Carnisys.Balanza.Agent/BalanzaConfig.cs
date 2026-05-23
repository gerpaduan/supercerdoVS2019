using System.Runtime.Serialization;

namespace Carnisys.Balanza.Agent
{
    [DataContract]
    public sealed class BalanzaConfig
    {
        [DataMember(Name = "marca")]
        public string Marca { get; set; }

        [DataMember(Name = "modelo")]
        public string Modelo { get; set; }

        [DataMember(Name = "puerto")]
        public string Puerto { get; set; }

        [DataMember(Name = "baudRate")]
        public int BaudRate { get; set; }

        [DataMember(Name = "dataBits")]
        public int DataBits { get; set; }

        [DataMember(Name = "parity")]
        public string Parity { get; set; }

        [DataMember(Name = "stopBits")]
        public string StopBits { get; set; }

        [DataMember(Name = "intervaloLecturaMs")]
        public int IntervaloLecturaMs { get; set; }
    }
}
