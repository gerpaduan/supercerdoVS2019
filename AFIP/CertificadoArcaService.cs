// Gestion del certificado digital de AFIP/ARCA de una empresa, sin tocar el servidor a mano:
//   1) GenerarCsr: crea una clave RSA + el pedido (.csr) que se sube a ARCA. La clave privada queda en
//      AFIP/<cuit>/<prod|homo>/pendiente/clave.key (nunca se descarga).
//   2) Instalar: recibe el .crt que devuelve ARCA, verifica que corresponda a la clave pendiente y al
//      CUIT, arma el .pfx (con clave) y lo reemplaza dejando backup del anterior.
//   3) Leer: abre el .pfx vigente y devuelve su vencimiento/estado (para la pantalla y el aviso).
// Solo net10.0 (WebCore): usa CertificateRequest/PEM modernos; el Web clasico no lo necesita.
// Detalle y decisiones: docs/06-datos-e-integraciones/afip-y-facturacion.md y docs/DECISIONS.md.
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AFIP
{
    public enum EstadoCertificado
    {
        SinCertificado,
        Vigente,
        PorVencer,
        Vencido,
        Ilegible
    }

    // Resumen del certificado vigente de una empresa. Nunca incluye la clave privada ni la del pfx.
    public class InfoCertificado
    {
        public EstadoCertificado Estado { get; set; }
        public string NombreArchivo { get; set; }
        public DateTime? Desde { get; set; }
        public DateTime? Vence { get; set; }
        public int? DiasRestantes { get; set; }
        public string Subject { get; set; }
        public string Huella { get; set; }
        public string Mensaje { get; set; }
        // Hay un pedido (CSR) generado esperando el .crt de ARCA.
        public bool HayPedidoPendiente { get; set; }
        // Carpeta de donde se lee el certificado (para mostrarla en la pantalla) y si es el pfx historico de la raiz.
        public string Carpeta { get; set; }
        public bool EsLegado { get; set; }
    }

    public class ResultadoInstalacion
    {
        public string NombreArchivo { get; set; }
        public DateTime Vence { get; set; }
        public string Huella { get; set; }
        public string RutaBackup { get; set; }
    }

    // Error de validacion pensado para mostrarse al usuario tal cual (mensaje en castellano).
    public class CertificadoArcaException : Exception
    {
        public CertificadoArcaException(string mensaje) : base(mensaje) { }
        public CertificadoArcaException(string mensaje, Exception inner) : base(mensaje, inner) { }
    }

    public class CertificadoArcaService
    {
        private const int BitsClaveRsa = 2048;
        private const string ArchivoClavePendiente = "clave.key";
        private const string ArchivoCsrPendiente = "solicitud.csr";
        private const string ArchivoCuitPendiente = "cuit.txt";
        private const int LargoClavePfx = 24;
        private const int MaxBytesCertificado = 64 * 1024;

        private readonly string _baseDirectory;
        // true = certificado de la PLATAFORMA (AFIP/_plataforma/, el del padron compartido por todas las empresas);
        // false = certificado de una empresa (AFIP/<CUIT>/). Solo cambia la carpeta; el resto del flujo es igual.
        private readonly bool _plataforma;

        // baseDirectory: raiz que contiene la carpeta AFIP/ (ContentRoot en WebCore).
        public CertificadoArcaService(string baseDirectory) : this(baseDirectory, false)
        {
        }

        private CertificadoArcaService(string baseDirectory, bool plataforma)
        {
            if (string.IsNullOrWhiteSpace(baseDirectory))
                throw new ArgumentException("Falta la carpeta base.", nameof(baseDirectory));
            _baseDirectory = baseDirectory;
            _plataforma = plataforma;
        }

        // Servicio para el certificado de la plataforma (padron compartido): trabaja sobre AFIP/_plataforma/.
        // El CUIT que se pasa a GenerarCsr/Instalar es el de la plataforma (dueno del alias en ARCA).
        public static CertificadoArcaService ParaPlataforma(string baseDirectory)
        {
            return new CertificadoArcaService(baseDirectory, true);
        }

        // Donde leer el pfx (con su LoginTemplate.xml y tickets): AFIP/<cuit>/prod o /homo (produccion cae al pfx
        // historico de la raiz si todavia no tiene el suyo), o AFIP/_plataforma para el certificado de la plataforma.
        private AfipRutas.UbicacionAfip UbicacionDe(string cuitTexto, bool homologacion, string nombreLegado)
        {
            if (_plataforma)
            {
                string carpeta = AfipRutas.CarpetaPlataforma(_baseDirectory);
                return new AfipRutas.UbicacionAfip(carpeta, AfipRutas.Certificado(carpeta, nombreLegado), false);
            }
            return AfipRutas.Resolver(_baseDirectory, cuitTexto, homologacion, nombreLegado);
        }

        // Carpeta DONDE SE ESCRIBE (pfx nuevo, pedido pendiente, tickets): la del entorno, nunca la raiz historica.
        // Un certificado de homologacion jamas toca los archivos de produccion.
        private string CarpetaEscritura(string cuitTexto, bool homologacion)
        {
            return _plataforma
                ? AfipRutas.CarpetaPlataforma(_baseDirectory)
                : AfipRutas.CarpetaEntorno(_baseDirectory, cuitTexto, homologacion);
        }

        // ------------------------------------------------------------------ aviso

        // "Banda" de aviso segun los dias que faltan: el menor umbral que alcanza a cubrir ese plazo
        // (umbrales 60,30,15 y 20 dias -> 30; 10 dias -> 15). 0 = ya vencido. -1 = todavia no hay que
        // avisar. Sirve para avisar de nuevo solo cuando se cruza un umbral, no todos los dias.
        public static int BandaAviso(int diasRestantes, int[] umbrales)
        {
            if (diasRestantes <= 0) return 0;
            int banda = -1;
            if (umbrales != null)
                foreach (int u in umbrales)
                    if (u > 0 && diasRestantes <= u && (banda == -1 || u < banda)) banda = u;
            return banda;
        }

        // ------------------------------------------------------------------ lectura

        // Lee el pfx de la empresa y calcula su estado. No lanza: cualquier problema (archivo faltante,
        // clave incorrecta, formato) vuelve como estado SinCertificado/Ilegible con un mensaje.
        // clave: la del pfx (null/vacia = pfx historico sin clave). diasAviso: umbrales en dias; el
        // mayor define desde cuando el estado es PorVencer.
        // homologacion: que entorno se lee (cada uno tiene su carpeta). nombreCertificadoPfx: solo cuenta para el
        // pfx historico de produccion (raiz del CUIT) y para la plataforma; en prod/ y homo/ el nombre es fijo.
        public InfoCertificado Leer(long cuit, bool homologacion, string nombreCertificadoPfx, string clave, int[] diasAviso, DateTime? ahoraUtc = null)
        {
            string cuitTexto = cuit.ToString();
            var ubicacion = UbicacionDe(cuitTexto, homologacion, nombreCertificadoPfx);
            var info = new InfoCertificado
            {
                NombreArchivo = Path.GetFileName(ubicacion.RutaPfx),
                Carpeta = ubicacion.Carpeta,
                EsLegado = ubicacion.EsLegado,
                HayPedidoPendiente = File.Exists(Path.Combine(CarpetaPendiente(cuitTexto, homologacion), ArchivoClavePendiente))
            };

            string ruta = ubicacion.RutaPfx;
            if (!File.Exists(ruta))
            {
                info.Estado = EstadoCertificado.SinCertificado;
                info.Mensaje = "No hay un certificado cargado para esta empresa.";
                return info;
            }

            try
            {
                using (var cert = CargarPfx(File.ReadAllBytes(ruta), clave ?? ""))
                {
                    var ahora = ahoraUtc ?? DateTime.UtcNow;
                    var vence = cert.NotAfter.ToUniversalTime();
                    info.Desde = cert.NotBefore;
                    info.Vence = cert.NotAfter;
                    info.Subject = cert.Subject;
                    info.Huella = cert.Thumbprint;
                    int dias = (int)Math.Floor((vence - ahora).TotalDays);
                    info.DiasRestantes = dias;

                    int umbral = (diasAviso != null && diasAviso.Length > 0) ? diasAviso.Max() : 0;
                    if (vence <= ahora) info.Estado = EstadoCertificado.Vencido;
                    else if (dias <= umbral) info.Estado = EstadoCertificado.PorVencer;
                    else info.Estado = EstadoCertificado.Vigente;
                }
            }
            catch (Exception ex) when (ex is CryptographicException || ex is IOException || ex is UnauthorizedAccessException)
            {
                // El detalle tecnico va al llamador solo como texto generico: la clave puede estar mal
                // o el archivo danado; el controller loguea el tipo de excepcion, no el contenido.
                info.Estado = EstadoCertificado.Ilegible;
                info.Mensaje = "No se pudo leer el certificado (archivo dañado o clave incorrecta): " + ex.GetType().Name;
            }

            return info;
        }

        // ------------------------------------------------------------------ paso 1: pedido (CSR)

        // Genera clave + CSR para el CUIT y devuelve el .csr en PEM. Pisa un pedido pendiente anterior
        // (el .crt que devuelva ARCA solo sirve para el ultimo CSR generado).
        // alias: nombre del "computador fiscal" en ARCA (letras/numeros/guiones, hasta 40).
        public string GenerarCsr(long cuit, bool homologacion, string razonSocial, string alias)
        {
            string cuitTexto = cuit.ToString();
            if (cuitTexto.Length != 11)
                throw new CertificadoArcaException("El CUIT de la empresa no es válido (11 dígitos).");
            alias = (alias ?? "").Trim();
            if (alias.Length == 0 || alias.Length > 40 || !alias.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_'))
                throw new CertificadoArcaException("El alias debe tener hasta 40 caracteres: letras, números, guiones.");
            razonSocial = (razonSocial ?? "").Trim();
            if (razonSocial.Length == 0)
                throw new CertificadoArcaException("La empresa no tiene razón social cargada.");

            // Subject que espera ARCA: C=AR, O=<razon social>, CN=<alias>, serialNumber=CUIT <cuit>.
            // PENDIENTE verificar contra la doc vigente de ARCA (ver plan/tutorial).
            // Se arma con el builder (no con un string) para que comas, acentos, etc. de la razon social
            // no rompan ni inyecten atributos en el nombre.
            var nombre = new X500DistinguishedNameBuilder();
            nombre.AddCountryOrRegion("AR");
            nombre.AddOrganizationName(razonSocial);
            nombre.AddCommonName(alias);
            nombre.Add("2.5.4.5", "CUIT " + cuitTexto, System.Formats.Asn1.UniversalTagNumber.PrintableString); // serialNumber
            var subject = nombre.Build();

            using (var rsa = RSA.Create(BitsClaveRsa))
            {
                var pedido = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                string csrPem = PemEncoding.WriteString("CERTIFICATE REQUEST", pedido.CreateSigningRequest());

                // Las carpetas se crean al escribir (no al dar de alta la empresa): un solo lugar, idempotente.
                string carpetaPendiente = CarpetaPendiente(cuitTexto, homologacion);
                Directory.CreateDirectory(carpetaPendiente);
                EscribirAtomico(Path.Combine(carpetaPendiente, ArchivoClavePendiente), Encoding.ASCII.GetBytes(rsa.ExportPkcs8PrivateKeyPem()));
                EscribirAtomico(Path.Combine(carpetaPendiente, ArchivoCsrPendiente), Encoding.ASCII.GetBytes(csrPem));
                // Recuerda para que CUIT es el pedido (la plataforma no tiene un CUIT fijo por carpeta).
                EscribirAtomico(Path.Combine(carpetaPendiente, ArchivoCuitPendiente), Encoding.ASCII.GetBytes(cuitTexto));
                return csrPem;
            }
        }

        // CUIT para el que se genero el pedido pendiente (solo el de la plataforma lo necesita: el CUIT de una
        // empresa sale de la propia empresa), o null si no hay pedido.
        public long? LeerCuitPendiente()
        {
            string ruta = Path.Combine(CarpetaPendiente("0", false), ArchivoCuitPendiente);
            if (!File.Exists(ruta)) return null;
            return long.TryParse(File.ReadAllText(ruta, Encoding.ASCII).Trim(), out long cuit) ? cuit : (long?)null;
        }

        // Devuelve el CSR del pedido pendiente del entorno (para volver a descargarlo), o null si no hay.
        public string LeerCsrPendiente(long cuit, bool homologacion)
        {
            string ruta = Path.Combine(CarpetaPendiente(cuit.ToString(), homologacion), ArchivoCsrPendiente);
            return File.Exists(ruta) ? File.ReadAllText(ruta, Encoding.ASCII) : null;
        }

        public void DescartarPedidoPendiente(long cuit, bool homologacion)
        {
            string carpeta = CarpetaPendiente(cuit.ToString(), homologacion);
            if (Directory.Exists(carpeta))
                Directory.Delete(carpeta, recursive: true);
        }

        // ------------------------------------------------------------------ paso 2: instalar el .crt

        // Valida el .crt contra la clave pendiente y el CUIT, arma el .pfx con una clave nueva y lo deja
        // en AFIP/<cuit>/<nombreArchivo>, con backup del anterior y borrando los tickets WSAA (para que
        // el proximo login use el certificado nuevo). guardarClave persiste la clave del pfx (cifrada,
        // en la base); si falla, se restaura el pfx anterior y se relanza el error.
        // Instala SOLO en la carpeta del entorno (prod/ u homo/, nombre fijo certificado.pfx): nunca toca la raiz
        // historica ni el otro entorno, ni sus tickets. nombreArchivo solo cuenta para la plataforma.
        public ResultadoInstalacion Instalar(long cuit, bool homologacion, string nombreArchivo, byte[] crt, Action<string> guardarClave)
        {
            if (guardarClave == null) throw new ArgumentNullException(nameof(guardarClave));
            string cuitTexto = cuit.ToString();
            if (crt == null || crt.Length == 0 || crt.Length > MaxBytesCertificado)
                throw new CertificadoArcaException("El archivo del certificado está vacío o es demasiado grande.");

            string carpeta = CarpetaEscritura(cuitTexto, homologacion);
            string rutaClave = Path.Combine(CarpetaPendiente(cuitTexto, homologacion), ArchivoClavePendiente);
            if (!File.Exists(rutaClave))
                throw new CertificadoArcaException("No hay un pedido pendiente: generá primero el pedido (CSR) y subilo a ARCA.");

            using (var rsa = RSA.Create())
            {
                try { rsa.ImportFromPem(File.ReadAllText(rutaClave, Encoding.ASCII)); }
                catch (Exception ex) when (ex is CryptographicException || ex is ArgumentException)
                {
                    throw new CertificadoArcaException("La clave del pedido pendiente está dañada: generá un pedido nuevo.", ex);
                }

                using (var cert = CargarCrt(crt))
                {
                    ValidarCertificado(cert, rsa, cuitTexto);

                    string nombre = _plataforma ? AfipRutas.NombreCertificado(nombreArchivo) : AfipRutas.NombreCertificadoFijo;
                    string claveNueva = GenerarClaveAleatoria();
                    byte[] pfx;
                    using (var conClave = cert.CopyWithPrivateKey(rsa))
                        pfx = conClave.Export(X509ContentType.Pfx, claveNueva);

                    // Comprobar que el pfx recien armado se puede abrir con esa clave antes de reemplazar nada.
                    using (CargarPfx(pfx, claveNueva)) { }

                    Directory.CreateDirectory(carpeta);
                    AfipRutas.EscribirPlantillaEstandar(carpeta);
                    string destino = Path.Combine(carpeta, nombre);
                    string temporal = destino + ".nuevo";
                    string backup = null;

                    File.WriteAllBytes(temporal, pfx);
                    try
                    {
                        if (File.Exists(destino))
                        {
                            backup = destino + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bak";
                            File.Move(destino, backup);
                        }
                        File.Move(temporal, destino);
                    }
                    catch
                    {
                        // Dejar todo como estaba si el reemplazo falla a mitad.
                        if (backup != null && !File.Exists(destino) && File.Exists(backup)) File.Move(backup, destino);
                        if (File.Exists(temporal)) File.Delete(temporal);
                        throw;
                    }

                    try
                    {
                        guardarClave(claveNueva);
                    }
                    catch
                    {
                        // Sin la clave guardada el pfx nuevo no se podria abrir: volver al anterior.
                        File.Delete(destino);
                        if (backup != null) File.Move(backup, destino);
                        throw;
                    }

                    BorrarTickets(carpeta);
                    DescartarPedidoPendiente(cuit, homologacion);

                    return new ResultadoInstalacion
                    {
                        NombreArchivo = nombre,
                        Vence = cert.NotAfter,
                        Huella = cert.Thumbprint,
                        RutaBackup = backup
                    };
                }
            }
        }

        // ------------------------------------------------------------------ helpers

        private string CarpetaPendiente(string cuitTexto, bool homologacion)
        {
            return Path.Combine(CarpetaEscritura(cuitTexto, homologacion), AfipRutas.CarpetaClavePendiente);
        }

        // Reglas del .crt: vigente, con la misma clave publica que el pedido y del CUIT de la empresa.
        private static void ValidarCertificado(X509Certificate2 cert, RSA claveDelPedido, string cuitTexto)
        {
            var publica = cert.GetRSAPublicKey();
            if (publica == null || !publica.ExportSubjectPublicKeyInfo().SequenceEqual(claveDelPedido.ExportSubjectPublicKeyInfo()))
                throw new CertificadoArcaException("El certificado no corresponde al último pedido (CSR) generado. Generá un pedido nuevo, subilo a ARCA y usá el certificado que te devuelve.");

            if (cert.NotAfter.ToUniversalTime() <= DateTime.UtcNow)
                throw new CertificadoArcaException("El certificado ya está vencido.");

            // ARCA pone el CUIT en el serialNumber del subject ("CUIT 20123456789").
            if (cert.Subject.IndexOf(cuitTexto, StringComparison.Ordinal) < 0)
                throw new CertificadoArcaException("El certificado no es del CUIT de esta empresa (" + cuitTexto + ").");
        }

        // Acepta el .crt en PEM (texto) o DER (binario).
        private static X509Certificate2 CargarCrt(byte[] contenido)
        {
            try
            {
                string texto = Encoding.ASCII.GetString(contenido);
                if (texto.IndexOf("-----BEGIN CERTIFICATE-----", StringComparison.Ordinal) >= 0)
                    return X509Certificate2.CreateFromPem(texto);
                return X509CertificateLoader.LoadCertificate(contenido);
            }
            catch (CryptographicException ex)
            {
                throw new CertificadoArcaException("El archivo no es un certificado válido (.crt/.cer/.pem).", ex);
            }
        }

        private static X509Certificate2 CargarPfx(byte[] contenido, string clave)
        {
            try
            {
                return X509CertificateLoader.LoadPkcs12(contenido, clave, X509KeyStorageFlags.EphemeralKeySet);
            }
            catch (CryptographicException) when (string.IsNullOrEmpty(clave))
            {
                // Algunos pfx historicos sin clave estan armados sin contraseña (null) y no abren con "".
                return X509CertificateLoader.LoadPkcs12(contenido, null, X509KeyStorageFlags.EphemeralKeySet);
            }
        }

        // Los tickets vencen en ~12 h; borrarlos fuerza un login nuevo con el certificado recien instalado.
        private static void BorrarTickets(string carpeta)
        {
            foreach (string patron in new[] { "TicketAcceso*.txt" })
                foreach (string archivo in Directory.GetFiles(carpeta, patron))
                    File.Delete(archivo);
        }

        // Escribe a un temporal y lo mueve, para no dejar un archivo a medias.
        private static void EscribirAtomico(string ruta, byte[] contenido)
        {
            string temporal = ruta + ".tmp";
            File.WriteAllBytes(temporal, contenido);
            if (File.Exists(ruta)) File.Delete(ruta);
            File.Move(temporal, ruta);
        }

        // Clave aleatoria fuerte para el pfx (solo letras y numeros: sin problemas de escape).
        private static string GenerarClaveAleatoria()
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var sb = new StringBuilder(LargoClavePfx);
            for (int i = 0; i < LargoClavePfx; i++)
                sb.Append(alfabeto[RandomNumberGenerator.GetInt32(alfabeto.Length)]);
            return sb.ToString();
        }
    }
}
