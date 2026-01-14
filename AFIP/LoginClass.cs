using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Security;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace AFIP
{
    public class LoginClass
    {
        private static UInt32 _globalId = (UInt32)DateTime.Now.Ticks;

        private readonly string _servicio;
        private readonly string _urlLogin;
        private readonly string _rutaCertificado;
        private readonly SecureString _claveCertificado;
        private readonly string _rutaTA;

        private XmlDocument _xmlLoginTicketRequest;
        private XmlDocument _xmlLoginTicketResponse;

        public string Token { get; private set; }
        public string Sign { get; private set; }

        public DateTime GenerationTime { get; private set; }
        public DateTime ExpirationTime { get; private set; }

        public bool Logeado => !string.IsNullOrEmpty(Token);

        public X509Certificate2 Certificado { get; private set; }
        public XDocument XDocRequest { get; private set; }
        public XDocument XDocResponse { get; private set; }

        public LoginClass(
            string servicio,
            string urlLogin,
            string rutaCertificado,
            string claveCertificado,
            string rutaTA)
        {
            _servicio = servicio;
            _urlLogin = urlLogin;
            _rutaCertificado = rutaCertificado;
            _rutaTA = rutaTA;

            _claveCertificado = new SecureString();
            foreach (char c in claveCertificado)
                _claveCertificado.AppendChar(c);
            _claveCertificado.MakeReadOnly();
        }

        public string HacerLogin()
        {
            try
            {
                _globalId++;

                PrepararLoginTicketRequest();
                string cmsFirmadoBase64 = FirmarLoginTicket();
                string loginTicketResponse = ObtenerLoginTicketResponse(cmsFirmadoBase64);

                ProcesarRespuesta(loginTicketResponse);

                return loginTicketResponse;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error WSAA Login (Servicio: {_servicio})", ex);
            }
        }

        #region Flujo principal

        private void PrepararLoginTicketRequest()
        {
            _xmlLoginTicketRequest = new XmlDocument();
            XMLLoader.LoadTemplate(_xmlLoginTicketRequest, "LoginTemplate");

            _xmlLoginTicketRequest.SelectSingleNode("//uniqueId").InnerText = _globalId.ToString();
            _xmlLoginTicketRequest.SelectSingleNode("//generationTime").InnerText =
                DateTime.Now.AddMinutes(-10).ToString("s");
            _xmlLoginTicketRequest.SelectSingleNode("//expirationTime").InnerText =
                DateTime.Now.AddMinutes(10).ToString("s");
            _xmlLoginTicketRequest.SelectSingleNode("//service").InnerText = _servicio;
        }

        private string FirmarLoginTicket()
        {
            Certificado = new X509Certificate2(
                File.ReadAllBytes(_rutaCertificado),
                _claveCertificado,
                X509KeyStorageFlags.PersistKeySet
            );

            byte[] msgBytes = Encoding.UTF8.GetBytes(_xmlLoginTicketRequest.OuterXml);
            ContentInfo infoContenido = new ContentInfo(msgBytes);

            SignedCms cmsFirmado = new SignedCms(infoContenido);
            CmsSigner cmsFirmante = new CmsSigner(Certificado)
            {
                IncludeOption = X509IncludeOption.EndCertOnly
            };

            cmsFirmado.ComputeSignature(cmsFirmante);
            return Convert.ToBase64String(cmsFirmado.Encode());
        }

        private string ObtenerLoginTicketResponse(string cmsFirmadoBase64)
        {
            // Intentar usar TA existente
            if (File.Exists(_rutaTA))
            {
                string ultimoTA = File.ReadAllText(_rutaTA);
                if (TAActivo(ultimoTA))
                    return ultimoTA;
            }

            // Solicitar nuevo TA
            var servicio = new WSAA.LoginCMSService
            {
                Url = _urlLogin
            };

            string respuesta = servicio.loginCms(cmsFirmadoBase64);

            Directory.CreateDirectory(Path.GetDirectoryName(_rutaTA));
            File.WriteAllText(_rutaTA, respuesta);

            return respuesta;
        }

        private void ProcesarRespuesta(string loginTicketResponse)
        {
            _xmlLoginTicketResponse = new XmlDocument();
            _xmlLoginTicketResponse.LoadXml(loginTicketResponse);

            Token = _xmlLoginTicketResponse.SelectSingleNode("//token").InnerText;
            Sign = _xmlLoginTicketResponse.SelectSingleNode("//sign").InnerText;

            GenerationTime = DateTime.Parse(
                _xmlLoginTicketResponse.SelectSingleNode("//generationTime").InnerText);

            ExpirationTime = DateTime.Parse(
                _xmlLoginTicketResponse.SelectSingleNode("//expirationTime").InnerText);

            XDocRequest = XDocument.Parse(_xmlLoginTicketRequest.OuterXml);
            XDocResponse = XDocument.Parse(_xmlLoginTicketResponse.OuterXml);
        }

        private bool TAActivo(string loginTicketResponse)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(loginTicketResponse);

                DateTime exp = DateTime.Parse(
                    xml.SelectSingleNode("//expirationTime").InnerText);

                return exp.AddHours(-2) > DateTime.Now;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }

    public static class XMLLoader
    {
        public static void LoadTemplate(XmlDocument doc, string file)
        {
            string path = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Templates",
                file + ".xml"
            );

            doc.Load(path);
        }
    }
}
