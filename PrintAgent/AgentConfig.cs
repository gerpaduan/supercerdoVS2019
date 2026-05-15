using System.Runtime.Serialization;

namespace CarniSys.PrintAgent
{
    [DataContract]
    public class AgentConfig
    {
        [DataMember(Name = "printerName")]
        public string PrinterName { get; set; }

        [DataMember(Name = "ticketMm")]
        public int TicketMm { get; set; }
    }
}
