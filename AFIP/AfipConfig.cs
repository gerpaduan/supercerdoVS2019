// Configuracion de los servicios web de AFIP/ARCA por entorno: URLs de WSAA (login), WSFE (factura
// electronica) y padron A13, y la clave del certificado .pfx. Los valores por defecto son los
// endpoints oficiales que el codigo tenia hardcodeados, asi que sin config el comportamiento es
// identico al anterior (Web clasico incluido). WebCore los pisa desde App.config (AfipSettings).
using System;

namespace AFIP
{
    public class AfipEndpoints
    {
        // Sin querystring: cada servicio agrega lo que necesite (GenerarFacturaService agrega "?wsdl"
        // / "?WSDL" tal como hacia antes, para no cambiar el request en produccion).
        public string WsaaUrl { get; set; }
        public string WsfeUrl { get; set; }
        public string PadronUrl { get; set; }

        public static AfipEndpoints ProduccionPorDefecto()
        {
            return new AfipEndpoints
            {
                WsaaUrl = "https://wsaa.afip.gov.ar/ws/services/LoginCms",
                WsfeUrl = "https://servicios1.afip.gov.ar/wsfev1/service.asmx",
                PadronUrl = "https://aws.afip.gov.ar/sr-padron/webservices/personaServiceA13"
            };
        }

        public static AfipEndpoints HomologacionPorDefecto()
        {
            return new AfipEndpoints
            {
                WsaaUrl = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms",
                WsfeUrl = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx",
                PadronUrl = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13"
            };
        }
    }

    public class AfipConfig
    {
        public AfipEndpoints Produccion { get; set; } = AfipEndpoints.ProduccionPorDefecto();
        public AfipEndpoints Homologacion { get; set; } = AfipEndpoints.HomologacionPorDefecto();

        // Clave del .pfx de la empresa. null/vacia = pfx sin clave (los certificados historicos).
        // La provee quien instancia el servicio (WebCore la lee cifrada de la base); nunca se loguea.
        public string ClaveCertificado { get; set; }

        public bool EsHomologacion(string entornoHomoProd)
        {
            return AfipEntorno.EsHomologacion(entornoHomoProd);
        }

        public AfipEndpoints Para(string entornoHomoProd)
        {
            return EsHomologacion(entornoHomoProd) ? Homologacion : Produccion;
        }

        // Copia con otra clave de certificado (la config base es compartida entre empresas).
        public AfipConfig ConClave(string claveCertificado)
        {
            return new AfipConfig
            {
                Produccion = Produccion,
                Homologacion = Homologacion,
                ClaveCertificado = claveCertificado
            };
        }

        public static AfipConfig PorDefecto()
        {
            return new AfipConfig();
        }
    }
}
