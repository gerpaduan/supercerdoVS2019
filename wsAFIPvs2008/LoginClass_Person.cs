using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Net;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Configuration;


namespace wsAFIPvs2008
{
    public class LoginClass_Person
    {
        private string rutaTA = Directory.GetCurrentDirectory() + "\\TicketAccesoPerson.txt";
        private static UInt32 _globalId = 0;

        public string serv { get; set; }
        public string url { get; set; }

        private string cert_path;
        private SecureString clave;
        private XmlDocument XmlLoginTicketRequest;
        private XmlDocument XmlLoginTicketResponse;
        private UInt32 uniqueId;

        public DateTime GenerationTime { get; set; }
        public DateTime ExpirationTime { get; set; }

        public bool Logeado
        {
            get
            {
                return !string.IsNullOrEmpty(Token);
            }
        }

        public X509Certificate2 certificado { get; set; }
        public XDocument XDocRequest { get; set; }
        public XDocument XDocResponse { get; set; }
        public string Token { get; private set; }
        public string Sign { get; private set; }

        public LoginClass_Person(string serv, string url, string cert_path, string clave)
        {
            this.serv = serv;
            this.url = url;
            this.cert_path = cert_path;
            this.clave = new SecureString();
            foreach (char character in clave)
                this.clave.AppendChar(character);
            this.clave.MakeReadOnly();
        }

        public string hacerLogin()
        {
            string cmsFirmadoBase64;
            string loginTicketResponse;
            XmlNode uniqueIdNode;
            XmlNode generationTimeNode;
            XmlNode ExpirationTimeNode;
            XmlNode ServiceNode;
            try
            {
                _globalId += 1;

                // Preparo el XML Request
                XmlLoginTicketRequest = new XmlDocument();
                XMLLoader.loadTemplate(XmlLoginTicketRequest, "LoginTemplate");
                uniqueIdNode = XmlLoginTicketRequest.SelectSingleNode("//uniqueId");
                generationTimeNode = XmlLoginTicketRequest.SelectSingleNode("//generationTime");
                ExpirationTimeNode = XmlLoginTicketRequest.SelectSingleNode("//expirationTime");
                ServiceNode = XmlLoginTicketRequest.SelectSingleNode("//service");
                generationTimeNode.InnerText = DateTime.Now.AddMinutes(-10).ToString("s");
                ExpirationTimeNode.InnerText = DateTime.Now.AddMinutes(+10).ToString("s");
                uniqueIdNode.InnerText = _globalId.ToString();
                ServiceNode.InnerText = serv;

                // Obtenemos el Cert
                certificado = new X509Certificate2();
                if (clave.IsReadOnly())
                {
                    certificado.Import(File.ReadAllBytes(cert_path), clave, X509KeyStorageFlags.PersistKeySet);
                }
                else
                {
                    certificado.Import(File.ReadAllBytes(cert_path));
                }

                var msgBytes = Encoding.UTF8.GetBytes(XmlLoginTicketRequest.OuterXml);

                // Firmamos
                var infoContenido = new ContentInfo(msgBytes);
                var cmsFirmado = new SignedCms(infoContenido);
                var cmsFirmante = new CmsSigner(certificado);
                cmsFirmante.IncludeOption = X509IncludeOption.EndCertOnly;
                cmsFirmado.ComputeSignature(cmsFirmante);
                cmsFirmadoBase64 = Convert.ToBase64String(cmsFirmado.Encode());

                // Hago el login
                var servicio = new WSAA.LoginCMSService();
                servicio.Url = url;

                ///1-Guardar ultimo loginTicketResponse e intentar loguearse (dura 12hs). Si error, generar uno nuevo
                ///{"El CEE ya posee un TA valido para el acceso al WSN solicitado"}
                ///Se lee el ultimo TA desde el archivo de texto 
                loginTicketResponse = leerTA();
                //Si está vencido se solicita otro TA a Afip
                if (!ultimoTA_activo(loginTicketResponse))
                {
                    loginTicketResponse = servicio.loginCms(cmsFirmadoBase64);
                    escribirTA(loginTicketResponse);
                }

                // Analizamos la respuesta
                XmlLoginTicketResponse = new XmlDocument();
                XmlLoginTicketResponse.LoadXml(loginTicketResponse);
                Token = XmlLoginTicketResponse.SelectSingleNode("//token").InnerText;
                Sign = XmlLoginTicketResponse.SelectSingleNode("//sign").InnerText;
                string exStr = XmlLoginTicketResponse.SelectSingleNode("//expirationTime").InnerText;
                string genStr = XmlLoginTicketResponse.SelectSingleNode("//generationTime").InnerText;
                ExpirationTime = DateTime.Parse(exStr);
                GenerationTime = DateTime.Parse(genStr);
                XDocRequest = XDocument.Parse(XmlLoginTicketRequest.OuterXml);
                XDocResponse = XDocument.Parse(XmlLoginTicketResponse.OuterXml);

                //se actualiza el valor de appConfig
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["token"].Value = Token;
                config.AppSettings.Settings["sign"].Value = Sign;
                config.AppSettings.Settings["certificado"].Value = certificado.ToString();
                config.AppSettings.Settings["cmsFirmadoBase64"].Value = cmsFirmadoBase64;
                config.AppSettings.Settings["ticketResponse"].Value = loginTicketResponse;
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return ex.Message;
            }

            return loginTicketResponse;
        }

        //Valida si ultimo TA está activo
        private bool ultimoTA_activo(string loginTicketResponse)
        {
            bool resp = false;
            try
            {
                // Analizamos la respuesta
                XmlLoginTicketResponse = new XmlDocument();
                XmlLoginTicketResponse.LoadXml(loginTicketResponse);
                string exStr = XmlLoginTicketResponse.SelectSingleNode("//expirationTime").InnerText;
                ExpirationTime = DateTime.Parse(exStr);

                //Si el tiempo expiracion (menos 2 hs) es menor a fechaHora actual => TA activo
                if (ExpirationTime.AddHours(-2) > DateTime.Now)
                    resp = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al validar ultimoTA_activo(): " + ex.Message);
            }
            return resp;
        }

        #region TicketAcceso
        // para escribir en el archivo
        private void escribirTA(string respuestaTA)
        {
            using (StreamWriter file = new StreamWriter(rutaTA, false))
            {
                file.WriteLine(respuestaTA);
                file.Close();
            }
        }

        // para leer la información el archivo
        private string leerTA()
        {
            string ultimoTA = "";
            using (StreamReader file = new StreamReader(rutaTA))
            {
                ultimoTA = file.ReadToEnd();
                file.Close();
            }
            return ultimoTA;
        }
        #endregion
    }

    public partial class XMLLoader_Person
    {
        public static void load(XmlDocument doc, string file)
        {
            doc.Load(Path.GetFullPath(Application.StartupPath + @"\" + file + ".xml"));
        }

        public static void loadTemplate(XmlDocument doc, string file)
        {
            load(doc, @"Templates\" + file);
        }
    }
}

