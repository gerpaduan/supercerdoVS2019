using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class Empresa
    {
        public int IdEmpresa { get; set; }
        public string RazonSocialAfip { get; set; }
        public long Cuit { get; set; }
        public string NombreFantasia { get; set; }
        public string Slogan1 { get; set; }
        public string Slogan2 { get; set; }
        public string Slogan3 { get; set; }
        public long Iibb { get; set; }
        public string CondicionIVA { get; set; }
        public DateTime InicioActividad { get; set; }
        public string TenantSlug { get; set; }
        public string Domicilio { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string BasePath { get; set; }
        public byte EsRRII { get; set; }
        public string NombreCertificado_pfx { get; set; }
        public string Entorno_HOMO_PROD { get; set; }
        public string BaseDatosNombre { get; set; }
        public byte Activa { get; set; }
    }

}
