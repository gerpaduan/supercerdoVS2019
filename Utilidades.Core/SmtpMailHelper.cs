// TODO(claude): copia temporal, ver comentario en IEmpresaContext.cs de esta carpeta. Porta solo
// SendMail/IsValidEmail de Web/Helpers/SmtpMailHelper.cs (los 2 metodos que usan
// VentasController/PuntosExpendioController/FinanzasController de WebCore) -- SendPasswordReset/
// SendAccountUnlock no se portan, fuera de alcance (no los usa ningun controller de WebCore).
// SendMail porta los 3 adjuntos (factura + detalle + nota de credito) del original -- Ventas.
// EnviarComprobanteEmail los necesita simultaneos.
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Utilidades
{
    public static class SmtpMailHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var address = new MailAddress(email.Trim());
                return string.Equals(address.Address, email.Trim(), StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        public static void SendMail(
            string toEmail,
            string toName,
            string subject,
            string bodyHtml,
            string attachmentFileName = null,
            byte[] attachmentBytes = null,
            string attachmentContentType = "application/pdf",
            string attachmentFileName2 = null,
            byte[] attachmentBytes2 = null,
            string attachmentContentType2 = "application/pdf",
            string attachmentFileName3 = null,
            byte[] attachmentBytes3 = null,
            string attachmentContentType3 = "application/pdf",
            string fromNameOverride = null,
            string replyToEmail = null,
            string replyToName = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("El email destino es obligatorio.", nameof(toEmail));

            string host = ConfigurationManager.AppSettings["SmtpHost"];
            string fromEmail = ConfigurationManager.AppSettings["SmtpFromEmail"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(fromEmail))
                throw new ConfigurationErrorsException("Falta configurar SMTP.");

            string fromName = string.IsNullOrWhiteSpace(fromNameOverride)
                ? (ConfigurationManager.AppSettings["SmtpFromName"] ?? "CarniSys")
                : fromNameOverride.Trim();
            string user = ConfigurationManager.AppSettings["SmtpUser"] ?? "";
            string pass = NormalizeAppPassword(ConfigurationManager.AppSettings["SmtpPass"] ?? "");
            bool enableSsl = ParseBool(ConfigurationManager.AppSettings["SmtpEnableSsl"], true);
            int port = ParseInt(ConfigurationManager.AppSettings["SmtpPort"], 587);
            string safeName = string.IsNullOrWhiteSpace(toName) ? toEmail.Trim() : toName.Trim();

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(new MailAddress(toEmail.Trim(), safeName));
                message.Subject = subject ?? "";
                message.Body = bodyHtml ?? "";
                message.IsBodyHtml = true;

                if (!string.IsNullOrWhiteSpace(replyToEmail))
                {
                    message.ReplyToList.Add(new MailAddress(replyToEmail.Trim(), string.IsNullOrWhiteSpace(replyToName) ? replyToEmail.Trim() : replyToName.Trim()));
                }

                if (attachmentBytes != null && attachmentBytes.Length > 0)
                {
                    var stream = new MemoryStream(attachmentBytes);
                    var attachment = new Attachment(stream, attachmentFileName ?? "adjunto.pdf", attachmentContentType ?? "application/octet-stream");
                    message.Attachments.Add(attachment);
                }

                if (attachmentBytes2 != null && attachmentBytes2.Length > 0)
                {
                    var stream2 = new MemoryStream(attachmentBytes2);
                    var attachment2 = new Attachment(stream2, attachmentFileName2 ?? "adjunto-2.pdf", attachmentContentType2 ?? "application/octet-stream");
                    message.Attachments.Add(attachment2);
                }

                if (attachmentBytes3 != null && attachmentBytes3.Length > 0)
                {
                    var stream3 = new MemoryStream(attachmentBytes3);
                    var attachment3 = new Attachment(stream3, attachmentFileName3 ?? "adjunto-3.pdf", attachmentContentType3 ?? "application/octet-stream");
                    message.Attachments.Add(attachment3);
                }

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
