// Certificado de la PLATAFORMA para consultar el padron de ARCA (ws_sr_padron_a13) en nombre de todas las empresas.
// Lo administra solo el super-admin (PadronPlataformaController). Guarda en la base el CUIT dueno del alias, el
// nombre del .pfx (que vive en AFIP/_plataforma/) y su clave CIFRADA con Data Protection. Es un alias DEDICADO
// al padron: solo tiene autorizado ws_sr_padron_a13, asi que si se filtra no permite facturar.
// Detalle: docs/06-datos-e-integraciones/afip-y-facturacion.md.
using System.Collections.Concurrent;
using AFIP;
using Microsoft.AspNetCore.DataProtection;
using WebCore.Helpers;
using WebCore.Infrastructure;

namespace WebCore.Services
{
    public class PadronPlataformaConfig
    {
        public long Cuit { get; set; }
        public string NombreArchivo { get; set; } = "";
        // Clave del pfx en claro: solo vive en memoria durante la consulta, nunca se loguea ni se guarda asi.
        public string Clave { get; set; } = "";
        public string Carpeta { get; set; } = "";
    }

    public interface IPadronPlataforma
    {
        // Solo Postgres (la tabla y la clave cifrada viven ahi).
        bool Soportado { get; }

        // Configuracion lista para consultar, o null si no hay certificado de plataforma cargado / no se puede usar.
        PadronPlataformaConfig? Obtener();

        // CUIT del alias de la plataforma, si esta configurado.
        long? CuitConfigurado();

        // Estado del certificado (vencimiento, etc.) leyendo el .pfx real.
        InfoCertificado LeerEstado();

        // Igual que LeerEstado pero cacheado 1 hora (para el aviso del menu, en cada pagina).
        InfoCertificado EstadoCacheado();

        // Guarda (cifrada) la clave del pfx recien instalado. Invalida el cache.
        void Guardar(long cuit, string nombreArchivo, string claveEnClaro, int idUsuario);
    }

    public class PadronPlataformaService : IPadronPlataforma
    {
        private const string PropositoClave = "CarniSys.CertificadoArca.PadronPlataforma.v1";
        private static readonly TimeSpan VigenciaCacheRegistro = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan VigenciaCacheEstado = TimeSpan.FromHours(1);

        // Estado compartido entre requests: la tabla tiene una sola fila y el .pfx se lee poco.
        private static readonly object Candado = new object();
        private static DateTime _registroLeidoEn = DateTime.MinValue;
        private static DatosPostgres.PlataformaCertificadoArcaRegistro? _registro;
        private static DateTime _estadoLeidoEn = DateTime.MinValue;
        private static InfoCertificado? _estado;

        private readonly IWebHostEnvironment _env;
        private readonly IDataProtector _protector;
        private readonly ILogger<PadronPlataformaService> _log;

        public PadronPlataformaService(IWebHostEnvironment env, IDataProtectionProvider dataProtection, ILogger<PadronPlataformaService> log)
        {
            _env = env;
            _protector = dataProtection.CreateProtector(PropositoClave);
            _log = log;
        }

        public bool Soportado => NegocioFactory.UsarPostgres;

        public long? CuitConfigurado()
        {
            var registro = LeerRegistro();
            return registro?.Cuit;
        }

        public PadronPlataformaConfig? Obtener()
        {
            if (!Soportado) return null;
            var registro = LeerRegistro();
            if (registro == null) return null;

            string carpeta = AfipRutas.CarpetaPlataforma(_env.ContentRootPath);
            string nombre = AfipRutas.NombreCertificado(registro.NombreArchivo);
            if (!File.Exists(Path.Combine(carpeta, nombre)) || !File.Exists(AfipRutas.Template(carpeta)))
                return null;

            try
            {
                return new PadronPlataformaConfig
                {
                    Cuit = registro.Cuit,
                    NombreArchivo = nombre,
                    Clave = _protector.Unprotect(registro.ClaveProtegida),
                    Carpeta = carpeta
                };
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                // Claves de Data Protection perdidas: no se puede usar; se vuelve a cargar el certificado.
                _log.LogError(ex, "No se pudo descifrar la clave del certificado de padrón de la plataforma.");
                return null;
            }
        }

        public InfoCertificado LeerEstado()
        {
            var registro = LeerRegistro();
            if (registro == null)
                return new InfoCertificado { Estado = EstadoCertificado.SinCertificado, Mensaje = "La plataforma todavía no tiene certificado de padrón." };

            string clave;
            try
            {
                clave = _protector.Unprotect(registro.ClaveProtegida);
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                _log.LogError(ex, "No se pudo descifrar la clave del certificado de padrón de la plataforma.");
                return new InfoCertificado
                {
                    NombreArchivo = registro.NombreArchivo,
                    Estado = EstadoCertificado.Ilegible,
                    Mensaje = "No se pudo descifrar la clave del certificado. Cargá el certificado de nuevo."
                };
            }

            return CertificadoArcaService.ParaPlataforma(_env.ContentRootPath)
                .Leer(registro.Cuit, registro.NombreArchivo, clave, AfipSettings.DiasAviso);
        }

        public InfoCertificado EstadoCacheado()
        {
            lock (Candado)
            {
                if (_estado != null && DateTime.UtcNow - _estadoLeidoEn < VigenciaCacheEstado)
                    return _estado;
            }

            InfoCertificado estado;
            try { estado = Soportado ? LeerEstado() : new InfoCertificado { Estado = EstadoCertificado.SinCertificado }; }
            catch (Exception ex)
            {
                // El aviso del menu nunca debe romper una pagina.
                _log.LogError(ex, "No se pudo evaluar el estado del certificado de padrón de la plataforma.");
                estado = new InfoCertificado { Estado = EstadoCertificado.SinCertificado };
            }

            lock (Candado)
            {
                _estado = estado;
                _estadoLeidoEn = DateTime.UtcNow;
            }
            return estado;
        }

        public void Guardar(long cuit, string nombreArchivo, string claveEnClaro, int idUsuario)
        {
            if (!Soportado) throw new InvalidOperationException("El certificado de plataforma requiere Postgres.");
            NegocioFactory.CrearPlataformaCertificadoArca().Guardar(cuit, nombreArchivo, _protector.Protect(claveEnClaro), idUsuario);
            Invalidar();
        }

        // ------------------------------------------------------------------ helpers

        private DatosPostgres.PlataformaCertificadoArcaRegistro? LeerRegistro()
        {
            if (!Soportado) return null;
            lock (Candado)
            {
                if (DateTime.UtcNow - _registroLeidoEn < VigenciaCacheRegistro)
                    return _registro;
            }

            var registro = NegocioFactory.CrearPlataformaCertificadoArca().Obtener();
            lock (Candado)
            {
                _registro = registro;
                _registroLeidoEn = DateTime.UtcNow;
            }
            return registro;
        }

        private static void Invalidar()
        {
            lock (Candado)
            {
                _registroLeidoEn = DateTime.MinValue;
                _estadoLeidoEn = DateTime.MinValue;
            }
        }
    }
}
