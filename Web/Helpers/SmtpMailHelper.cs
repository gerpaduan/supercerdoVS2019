using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Web.Helpers
{
    public static class SmtpMailHelper
    {
        public static bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpHost"])
                && !string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["SmtpFromEmail"]);
        }

        public static void SendPasswordReset(string toEmail, string toName, string resetUrl, int expirationMinutes)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("El email destino es obligatorio.", nameof(toEmail));

            string host = ConfigurationManager.AppSettings["SmtpHost"];
            string fromEmail = ConfigurationManager.AppSettings["SmtpFromEmail"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
                throw new ConfigurationErrorsException("Falta configurar SMTP para recuperación de contraseña.");

            string fromName = ConfigurationManager.AppSettings["SmtpFromName"] ?? "CarniSys";
            string user = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
            string pass = NormalizeAppPassword(ConfigurationManager.AppSettings["SmtpPass"] ?? "");
            bool enableSsl = ParseBool(ConfigurationManager.AppSettings["SmtpEnableSsl"], true);
            int port = ParseInt(ConfigurationManager.AppSettings["SmtpPort"], 587);

            string subject = "Recuperación de contraseña - CarniSys";
            string safeName = string.IsNullOrWhiteSpace(toName) ? "usuario" : toName.Trim();
            string bodyHtml =
                "<p>Hola " + WebUtility.HtmlEncode(safeName) + ".</p>" +
                "<p>Recibimos una solicitud para restablecer tu contraseña.</p>" +
                "<p><a href=\"" + WebUtility.HtmlEncode(resetUrl) + "\">Hacé clic acá para crear una nueva contraseña</a></p>" +
                "<p>Este enlace vence en " + expirationMinutes + " minutos y solo puede usarse una vez.</p>" +
                "<p>Si no solicitaste este cambio, podés ignorar este mensaje.</p>";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(new MailAddress(toEmail, safeName));
                message.Subject = subject;
                message.Body = bodyHtml;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    client.UseDefaultCredentials = false;
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        client.Credentials = new NetworkCredential(user, pass);
                    }

                    client.Send(message);
                }
            }
        }

        private static int ParseInt(string value, int defaultValue)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? parsed : defaultValue;
        }

        private static bool ParseBool(string value, bool defaultValue)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : defaultValue;
        }

        private static string NormalizeAppPassword(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return Regex.Replace(value, "\\s+", "");
        }
    }
}
