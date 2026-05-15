using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CarniSys.PrintAgent
{
    [DataContract]
    public class TicketPrintRequest
    {
        [DataMember(Name = "printerName")]
        public string PrinterName { get; set; }

        [DataMember(Name = "ticketMm")]
        public int TicketMm { get; set; }

        [DataMember(Name = "barcodeValue")]
        public string BarcodeValue { get; set; }

        [DataMember(Name = "barcodeHeader")]
        public string BarcodeHeader { get; set; }

        [DataMember(Name = "ticketLines")]
        public List<string> TicketLines { get; set; }
    }

    [DataContract]
    public class SaveConfigRequest
    {
        [DataMember(Name = "printerName")]
        public string PrinterName { get; set; }

        [DataMember(Name = "ticketMm")]
        public int TicketMm { get; set; }
    }

    [DataContract]
    public class PrinterInfo
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "isDefault")]
        public bool IsDefault { get; set; }
    }
}
