// Compila SOLO en net10.0 (ver AFIP.csproj) -- mismo criterio que WsfeCompat.cs/WsaaCompat.cs: shim
// sincrono sobre WSPSA13ServiceReference.cs (generado con `dotnet-svcutil` contra
// `Web References\WSPSA13\PersonaServiceA13.wsdl`, namespace AFIP.WSPSA13Core) para que
// ConsultarPadronService.cs no necesite ningun cambio para invocar el servicio de padron.
//
// namespace AFIP.WSPSA13 declarado real (no alias) a proposito: ConsultarPadronService.cs tiene
// `using AFIP.WSPSA13;` en los dos TFM -- en net472 ese using trae los tipos del proxy ASMX real
// (Web References\WSPSA13\Reference.cs), en net10.0 alcanza con que el namespace exista (con
// PersonaServiceA13 adentro) para que el using compile. El tipo de retorno `personaReturn` se
// resuelve via alias global en GlobalAliases.cs (mismo mecanismo que FEAuthRequest/FECAERequest
// para WSFE) porque es un tipo de datos puro, sin logica -- no hace falta un wrapper como el de
// PersonaServiceA13 (que si necesita reproducir la API sincrona Url/ClientCertificates/getPersona).
using System.ServiceModel;

namespace AFIP.WSPSA13
{
    public sealed class PersonaServiceA13
    {
        // Mismo uso que el ASMX viejo: se pisa a mano en ConsultarPadronService.cs
        // (servicePerson.Url = _urlPadron) para apuntar a homologacion/produccion segun la empresa.
        public string Url { get; set; }

        public System.Security.Cryptography.X509Certificates.X509CertificateCollection ClientCertificates { get; }
            = new System.Security.Cryptography.X509Certificates.X509CertificateCollection();

        private AFIP.WSPSA13Core.PersonaServiceA13Client CrearCliente()
        {
            var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024
            };
            // AFIP exige TLS mutuo (certificado de cliente) para ws_sr_padron_a13, igual que wsfev1
            // (WsfeCompat.cs) -- no WS-Security.
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Certificate;

            var client = new AFIP.WSPSA13Core.PersonaServiceA13Client(binding, new EndpointAddress(Url));
            if (ClientCertificates.Count > 0)
                client.ClientCredentials.ClientCertificate.Certificate =
                    (System.Security.Cryptography.X509Certificates.X509Certificate2)ClientCertificates[0];

            return client;
        }

        private static void CerrarCliente(AFIP.WSPSA13Core.PersonaServiceA13Client client)
        {
            try { client.Close(); }
            catch { client.Abort(); }
        }

        public AFIP.WSPSA13Core.personaReturn getPersona(string token, string sign, long cuitRepresentada, long idPersona)
        {
            var client = CrearCliente();
            try
            {
                return client.getPersonaAsync(token, sign, cuitRepresentada, idPersona)
                    .GetAwaiter().GetResult().personaReturn;
            }
            finally
            {
                CerrarCliente(client);
            }
        }
    }
}
