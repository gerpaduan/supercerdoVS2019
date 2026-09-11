using System;
using System.IO;
using System.Security.Cryptography;

namespace Utilidades
{
    // Cifrado simetrico (AES-256-CBC) para los tokens OAuth de Mercado Pago
    // (access_token/refresh_token) antes de guardarlos en mercadopago_config. Decision
    // explicita del usuario (2026-08-31, ver docs/DECISIONS.md): son credenciales que mueven
    // dinero real, se cifran a nivel de aplicacion ademas del RLS de Postgres -- distinto del
    // precedente de ConfiguracionWhatsApp (texto plano).
    //
    // Funciones puras: no lee configuracion ni conoce de donde sale la clave -- la clave la
    // resuelve quien orqueste (Web/Infrastructure/NegocioFactory.cs, leyendo el appSetting
    // "MercadoPagoTokenEncryptionKeyBase64" de Web.config) para que Negocio.dll no dependa de
    // ConfigurationManager (Negocio.dll tambien lo referencia WinForms, que no tiene ese
    // appSetting en su propio .exe.config).
    //
    // Formato del texto cifrado: base64(IV de 16 bytes + ciphertext). Un IV nuevo por cada
    // llamada a Encrypt (nunca reusar IV con la misma clave).
    public static class MercadoPagoTokenCipher
    {
        private const int KeySizeBytes = 32; // AES-256
        private const int IvSizeBytes = 16;

        public static string Encrypt(string plainText, byte[] key)
        {
            if (plainText == null) plainText = "";
            ValidarClave(key);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (var sw = new StreamWriter(cs, System.Text.Encoding.UTF8))
                    {
                        sw.Write(plainText);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        public static string Decrypt(string cipherTextBase64, byte[] key)
        {
            if (string.IsNullOrEmpty(cipherTextBase64)) return "";
            ValidarClave(key);

            var buffer = Convert.FromBase64String(cipherTextBase64);
            if (buffer.Length < IvSizeBytes)
                throw new ArgumentException("Texto cifrado invalido (menor al tamaño del IV).", nameof(cipherTextBase64));

            var iv = new byte[IvSizeBytes];
            Array.Copy(buffer, 0, iv, 0, IvSizeBytes);

            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var decryptor = aes.CreateDecryptor())
                using (var ms = new MemoryStream(buffer, IvSizeBytes, buffer.Length - IvSizeBytes))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new StreamReader(cs, System.Text.Encoding.UTF8))
                {
                    return sr.ReadToEnd();
                }
            }
        }

        // Genera una clave nueva de 32 bytes (AES-256), lista para pegar en Web.config como
        // base64 -- uso unico, manual, al configurar el appSetting por primera vez.
        public static string GenerarClaveBase64()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var key = new byte[KeySizeBytes];
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        private static void ValidarClave(byte[] key)
        {
            if (key == null || key.Length != KeySizeBytes)
                throw new ArgumentException(
                    "La clave de cifrado de Mercado Pago debe tener " + KeySizeBytes + " bytes (AES-256). " +
                    "Verificar el appSetting \"MercadoPagoTokenEncryptionKeyBase64\" en Web.config.",
                    nameof(key));
        }
    }
}
