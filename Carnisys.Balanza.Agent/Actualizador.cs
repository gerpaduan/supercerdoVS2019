using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Carnisys.Balanza.Agent
{
    // Datos publicados junto al .exe (Carnisys.Balanza.Agent.json) por scripts/publicar-agente-balanza.ps1.
    [DataContract]
    internal sealed class ManifiestoAgente
    {
        [DataMember(Name = "version")] public string Version { get; set; }
        [DataMember(Name = "sha256")] public string Sha256 { get; set; }
        [DataMember(Name = "firma")] public string Firma { get; set; }
        [DataMember(Name = "tamano")] public long Tamano { get; set; }
        [DataMember(Name = "fecha")] public string Fecha { get; set; }
    }

    // Auto-actualizacion del agente (2026-11-20). La web (Utilidades) le pide actualizar con POST /actualizar y el agente:
    //  1) acepta el pedido solo si el Origin del navegador esta en la lista de origenes permitidos de config.json (por defecto
    //     https://carnisys.com): una pagina cualquiera que el usuario visite no puede dispararlo;
    //  2) baja el manifiesto y el .exe SOLO de ese origen (la URL no viene en el pedido);
    //  3) verifica el SHA-256 y la FIRMA RSA del .exe con la clave publica embebida (ActualizacionClave): un servidor
    //     comprometido no puede hacer ejecutar nada, porque no tiene la clave privada;
    //  4) exige que la version del propio .exe firmado sea mayor que la instalada (no se acepta un agente viejo firmado);
    //  5) ejecuta el .exe bajado, que como esta fuera de la carpeta del agente se instala solo (--actualizacion: sin clics).
    internal static class Actualizador
    {
        public const string ArchivoExe = "Carnisys.Balanza.Agent.exe";
        public const string ArchivoManifiesto = "Carnisys.Balanza.Agent.json";
        private const string RutaDescargas = "/Content/downloads/";
        private const int TamanoMaximoExe = 20 * 1024 * 1024;
        private const int TamanoMaximoManifiesto = 16 * 1024;
        private const int TimeoutMs = 30000;

        private static readonly object Sync = new object();
        private static int _enCurso;
        private static string _estado = "ninguna";
        private static string _mensaje = string.Empty;

        public static object EstadoActual()
        {
            lock (Sync)
            {
                return new { estado = _estado, mensaje = _mensaje };
            }
        }

        private static void Poner(string estado, string mensaje)
        {
            lock (Sync)
            {
                _estado = estado;
                _mensaje = mensaje;
            }
            AgentLog.Info("Actualizacion: " + estado + (string.IsNullOrEmpty(mensaje) ? string.Empty : " - " + mensaje));
        }

        // Valida el origen y arranca la actualizacion en un hilo. Devuelve false (con el motivo) si no se acepta el pedido.
        public static bool Iniciar(string origenPedido, AgentConfig config, out string motivo)
        {
            string origen;
            if (!ResolverOrigen(origenPedido, config, out origen, out motivo))
            {
                AgentLog.Warn("Actualizacion rechazada (origen '" + origenPedido + "'): " + motivo);
                return false;
            }

            if (Interlocked.CompareExchange(ref _enCurso, 1, 0) != 0)
            {
                motivo = "Ya hay una actualización en curso.";
                return false;
            }

            motivo = "Actualización iniciada.";
            new Thread(() => Ejecutar(origen)) { IsBackground = true, Name = "Carnisys.Balanza.Actualizador" }.Start();
            return true;
        }

        // Origen permitido: https (o http solo si es la propia PC) y presente en la lista de config.json. Sin Origin (pedido que no
        // viene de un navegador) se usa el primero de la lista.
        public static bool ResolverOrigen(string origenPedido, AgentConfig config, out string origen, out string motivo)
        {
            origen = null;
            motivo = null;
            List<string> permitidos = (config != null && config.Actualizacion != null ? config.Actualizacion.OrigenesPermitidos : null)
                ?? new List<string>();
            permitidos = permitidos.Select(NormalizarOrigen).Where(o => o != null).ToList();
            if (permitidos.Count == 0)
            {
                motivo = "No hay orígenes de actualización configurados.";
                return false;
            }

            string candidato = string.IsNullOrWhiteSpace(origenPedido) ? permitidos[0] : NormalizarOrigen(origenPedido);
            if (candidato == null || !permitidos.Contains(candidato))
            {
                motivo = "Este sitio no está autorizado para actualizar el agente.";
                return false;
            }

            Uri uri = new Uri(candidato);
            bool propiaPc = uri.IsLoopback;
            if (!propiaPc && !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                motivo = "La actualización solo se acepta por HTTPS.";
                return false;
            }

            origen = candidato;
            return true;
        }

        public static string NormalizarOrigen(string texto)
        {
            Uri uri;
            if (string.IsNullOrWhiteSpace(texto) || !Uri.TryCreate(texto.Trim(), UriKind.Absolute, out uri))
            {
                return null;
            }

            if (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
            {
                return null;
            }

            return uri.GetLeftPart(UriPartial.Authority).ToLowerInvariant();
        }

        private static void Ejecutar(string origen)
        {
            string archivoNuevo = Path.Combine(AgentPaths.CarpetaActualizacion, ArchivoExe);
            try
            {
                Poner("verificando", "Buscando una versión nueva…");
                ManifiestoAgente manifiesto = DescargarManifiesto(origen);
                if (manifiesto == null || string.IsNullOrWhiteSpace(manifiesto.Sha256) || string.IsNullOrWhiteSpace(manifiesto.Firma))
                {
                    Poner("error", "El servidor no publicó los datos de la versión nueva.");
                    return;
                }

                Version publicada = ParsearVersion(manifiesto.Version);
                Version instalada = VersionInstalada();
                if (publicada == null || publicada <= instalada)
                {
                    Poner("al-dia", "El agente ya está actualizado (" + AgentPaths.Version() + ").");
                    return;
                }

                Poner("descargando", "Descargando la versión " + manifiesto.Version + "…");
                byte[] datos = Descargar(origen + RutaDescargas + ArchivoExe, TamanoMaximoExe);

                string error;
                if (!Verificar(datos, manifiesto, ActualizacionClave.PublicaXml, out error))
                {
                    Poner("error", error);
                    return;
                }

                Directory.CreateDirectory(AgentPaths.CarpetaActualizacion);
                File.WriteAllBytes(archivoNuevo, datos);

                // La version que vale es la del propio .exe firmado, no la del manifiesto (que no esta firmado): evita que un
                // servidor sirva un agente viejo firmado diciendo que es nuevo.
                Version delArchivo = ParsearVersion(FileVersionInfo.GetVersionInfo(archivoNuevo).FileVersion);
                if (delArchivo == null || delArchivo <= instalada)
                {
                    Poner("error", "El archivo descargado no es más nuevo que el instalado.");
                    BorrarSilencioso(archivoNuevo);
                    return;
                }

                Poner("instalando", "Instalando la versión " + delArchivo + "…");
                // UseShellExecute = true: el proceso hijo NO hereda los handles de este (si no, heredaba el socket de escucha del
                // puerto 5100 y el puerto quedaba tomado por una copia que nadie atiende aunque este agente se cerrara).
                Process.Start(new ProcessStartInfo(archivoNuevo)
                {
                    Arguments = "--actualizacion",
                    WorkingDirectory = AgentPaths.CarpetaActualizacion,
                    UseShellExecute = true
                });
                // El instalador le pide salir a este agente (señal "Salir"); el estado queda "instalando" hasta que se cierre.
            }
            catch (Exception ex)
            {
                AgentLog.Error("Fallo la actualizacion.", ex);
                Poner("error", "No se pudo actualizar: " + ex.Message);
                BorrarSilencioso(archivoNuevo);
            }
            finally
            {
                string estadoFinal;
                lock (Sync) { estadoFinal = _estado; }
                if (estadoFinal != "instalando")
                {
                    Interlocked.Exchange(ref _enCurso, 0);
                }
            }
        }

        // Verifica integridad (SHA-256 del manifiesto) y autenticidad (firma RSA con SHA-256 sobre los bytes del .exe).
        public static bool Verificar(byte[] datos, ManifiestoAgente manifiesto, string clavePublicaXml, out string error)
        {
            error = null;
            if (manifiesto.Tamano > 0 && manifiesto.Tamano != datos.Length)
            {
                error = "El archivo descargado está incompleto.";
                return false;
            }

            string hash;
            using (var sha = SHA256.Create())
            {
                hash = BitConverter.ToString(sha.ComputeHash(datos)).Replace("-", string.Empty).ToLowerInvariant();
            }

            if (!string.Equals(hash, (manifiesto.Sha256 ?? string.Empty).Trim().ToLowerInvariant(), StringComparison.Ordinal))
            {
                error = "El archivo descargado no coincide con el publicado (hash distinto).";
                return false;
            }

            try
            {
                using (var rsa = new RSACryptoServiceProvider())
                using (var sha = SHA256.Create())
                {
                    rsa.FromXmlString(clavePublicaXml);
                    byte[] firma = Convert.FromBase64String(manifiesto.Firma);
                    if (!rsa.VerifyData(datos, sha, firma))
                    {
                        error = "La firma del archivo no es válida: no se instala.";
                        return false;
                    }
                }
            }
            catch (Exception ex) when (ex is CryptographicException || ex is FormatException)
            {
                error = "No se pudo verificar la firma del archivo: " + ex.Message;
                return false;
            }

            return true;
        }

        public static Version ParsearVersion(string texto)
        {
            Version v;
            if (!Version.TryParse((texto ?? string.Empty).Trim(), out v))
            {
                return null;
            }

            // 2.0.0 y 2.0.0.0 son la misma version (Version() trata la revision ausente como -1).
            return new Version(v.Major, v.Minor, Math.Max(0, v.Build), Math.Max(0, v.Revision));
        }

        public static Version VersionInstalada()
        {
            return ParsearVersion(typeof(Actualizador).Assembly.GetName().Version.ToString());
        }

        private static ManifiestoAgente DescargarManifiesto(string origen)
        {
            byte[] datos = Descargar(origen + RutaDescargas + ArchivoManifiesto, TamanoMaximoManifiesto);
            using (var memoria = new MemoryStream(datos))
            {
                return new DataContractJsonSerializer(typeof(ManifiestoAgente)).ReadObject(memoria) as ManifiestoAgente;
            }
        }

        private static byte[] Descargar(string url, int maximo)
        {
            var pedido = (HttpWebRequest)WebRequest.Create(url);
            pedido.Timeout = TimeoutMs;
            pedido.ReadWriteTimeout = TimeoutMs;
            pedido.AllowAutoRedirect = false; // una redireccion podria sacar la descarga del origen autorizado
            pedido.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);
            pedido.UserAgent = "CarnisysBalanzaAgent/" + AgentPaths.Version();

            using (var respuesta = (HttpWebResponse)pedido.GetResponse())
            {
                if (respuesta.StatusCode != HttpStatusCode.OK)
                {
                    throw new IOException("El servidor respondió " + (int)respuesta.StatusCode + ".");
                }

                if (respuesta.ContentLength > maximo)
                {
                    throw new IOException("El archivo es más grande de lo esperado.");
                }

                using (var flujo = respuesta.GetResponseStream())
                using (var memoria = new MemoryStream())
                {
                    var buffer = new byte[81920];
                    int leidos;
                    while ((leidos = flujo.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        memoria.Write(buffer, 0, leidos);
                        if (memoria.Length > maximo)
                        {
                            throw new IOException("El archivo es más grande de lo esperado.");
                        }
                    }
                    return memoria.ToArray();
                }
            }
        }

        private static void BorrarSilencioso(string ruta)
        {
            try { if (File.Exists(ruta)) File.Delete(ruta); }
            catch (Exception ex) { AgentLog.Warn("No se pudo borrar " + ruta + ": " + ex.Message); }
        }

        // Borra lo que dejo una actualizacion anterior (el .exe ya instalado no necesita la copia bajada). Devuelve true cuando ya no
        // queda nada. Al arrancar el instalador puede seguir abierto (corre desde esa carpeta): se reintenta cada segundo.
        public static bool LimpiarRestos()
        {
            try
            {
                if (Directory.Exists(AgentPaths.CarpetaActualizacion))
                {
                    Directory.Delete(AgentPaths.CarpetaActualizacion, true);
                }
                return true;
            }
            catch (IOException)
            {
                return false; // el instalador todavia no cerro: se vuelve a intentar
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
    }
}
