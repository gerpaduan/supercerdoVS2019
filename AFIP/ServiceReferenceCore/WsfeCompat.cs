// Compila SOLO en net10.0 (ver AFIP.csproj). Los proxies ASMX viejos (Web References\WSFEHOMO\
// Reference.cs, herencia de System.Web.Services.Protocols.SoapHttpClientProtocol) no compilan en
// .NET Core -- WSFEServiceReference.cs (generado con `dotnet-svcutil` contra el mismo
// Web References\WSFEHOMO\service.wsdl`, namespace AFIP.WSFECore) es el reemplazo real. Esta clase
// es un shim de compatibilidad: reproduce la API sincrona vieja (Service.Url, ClientCertificates,
// FECompUltimoAutorizado/FECAESolicitar con firmas identicas a las del proxy ASMX) envolviendo el
// cliente WCF nuevo (AFIP.WSFECore.ServiceSoapClient) -- asi GenerarFacturaService.cs no necesita
// (casi) ningun cambio para compilar en net10.0 (mismo codigo fuente para los dos TFM; los tipos
// de request/response que usa sin calificar -- FEAuthRequest, FECAERequest, etc. -- se resuelven
// via alias globales en GlobalAliases.cs).
//
// namespace AFIP.WSFEHOMO declarado real (no alias) a proposito: GenerarFacturaService.cs tiene
// `using AFIP.WSFEHOMO;` en los dos TFM -- en net472 ese using trae los tipos del proxy ASMX real,
// en net10.0 alcanza con que el namespace exista (con Service adentro) para que el using compile.
namespace AFIP.WSFEHOMO
{
    public sealed class Service
    {
        // Mismo uso que el ASMX viejo: se pisa a mano en GenerarFacturaService.cs
        // (service.Url = urlWSFE) para apuntar a produccion, no al endpoint del WSDL.
        public string Url { get; set; }

        public System.Security.Cryptography.X509Certificates.X509CertificateCollection ClientCertificates { get; }
            = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();

        private AFIP.WSFECore.ServiceSoapClient CrearCliente()
        {
            var binding = new System.ServiceModel.BasicHttpsBinding(System.ServiceModel.BasicHttpsSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024
            };
            // AFIP exige TLS mutuo (certificado de cliente) para wsfev1, no WS-Security.
            binding.Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Certificate;

            var client = new AFIP.WSFECore.ServiceSoapClient(binding, new System.ServiceModel.EndpointAddress(Url));
            if (ClientCertificates.Count > 0)
                client.ClientCredentials.ClientCertificate.Certificate =
                    (System.Security.Cryptography.X509Certificates.X509Certificate2)ClientCertificates[0];

            return client;
        }

        private static void CerrarCliente(AFIP.WSFECore.ServiceSoapClient client)
        {
            try { client.Close(); }
            catch { client.Abort(); }
        }

        public AFIP.WSFECore.FERecuperaLastCbteResponse FECompUltimoAutorizado(
            AFIP.WSFECore.FEAuthRequest auth, int ptoVta, int cbteTipo)
        {
            var client = CrearCliente();
            try
            {
                return client.FECompUltimoAutorizadoAsync(auth, ptoVta, cbteTipo)
                    .GetAwaiter().GetResult().Body.FECompUltimoAutorizadoResult;
            }
            finally
            {
                CerrarCliente(client);
            }
        }

        public AFIP.WSFECore.FECAEResponse FECAESolicitar(
            AFIP.WSFECore.FEAuthRequest auth, AFIP.WSFECore.FECAERequest feCaeReq)
        {
            var client = CrearCliente();
            try
            {
                return client.FECAESolicitarAsync(auth, feCaeReq)
                    .GetAwaiter().GetResult().Body.FECAESolicitarResult;
            }
            finally
            {
                CerrarCliente(client);
            }
        }
    }
}
