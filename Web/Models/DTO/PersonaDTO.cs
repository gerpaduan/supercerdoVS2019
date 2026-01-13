using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.DTO
{
    public class PersonaDTO
    {
        public string RazonSocial { get; set; }
        public string Cuit { get; set; }
        public string Domicilio { get; set; }
        public string CondicionIva { get; set; }
    }
}