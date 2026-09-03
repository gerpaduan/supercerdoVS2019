// Compila SOLO en net10.0 (ver AFIP.csproj) -- mismo criterio que WsfeCompat.cs: shim sincrono
// sobre WSAAServiceReference.cs (generado con `dotnet-svcutil` contra Web References\WSAA\
// LoginCms.wsdl, namespace AFIP.WSAACore) para que LoginClass.cs no necesite ningun cambio.
using System.ServiceModel;

namespace AFIP.WSAA
{
    public sealed class LoginCMSService
    {
        public string Url { get; set; }

        public string loginCms(string in0)
        {
            var binding = new BasicHttpsBinding(BasicHttpsSecurityMode.Transport)
            {
                MaxReceivedMessageSize = 10 * 1024 * 1024
            };

            var client = new AFIP.WSAACore.LoginCMSClient(binding, new EndpointAddress(Url));
            try
            {
                return client.loginCmsAsync(in0).GetAwaiter().GetResult().loginCmsReturn;
            }
            finally
            {
                try { client.Close(); }
                catch { client.Abort(); }
            }
        }
    }
}
