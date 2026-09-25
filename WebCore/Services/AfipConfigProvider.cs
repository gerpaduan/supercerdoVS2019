// Arma la configuracion de AFIP de una empresa para los servicios de facturacion y padron: URLs por
// entorno (App.config, ver AfipSettings) + clave del .pfx (guardada cifrada en la base). Tambien guarda
// la clave nueva al instalar/renovar un certificado. Solo Postgres guarda claves; con SQL Server (o
// sin fila) la clave es vacia, o sea el comportamiento historico (pfx sin clave).
using AFIP;
using Microsoft.AspNetCore.DataProtection;
using WebCore.Helpers;
using WebCore.Infrastructure;

namespace WebCore.Services
{
    public interface IAfipConfigProvider
    {
        // Config lista para pasar a GenerarFacturaService / ConsultarPadronService, con la clave del pfx del
        // ENTORNO indicado (cada entorno tiene su certificado y su clave). Lanza InvalidOperationException
        // (mensaje mostrable) si la clave guardada no se puede descifrar.
        AfipConfig Crear(bool homologacion);

        // Persiste (cifrada) la clave del pfx recien generado para ese entorno. Solo Postgres.
        void GuardarClave(bool homologacion, string claveEnClaro);

        // true si este ambiente puede guardar claves (Postgres); la pantalla de certificados lo exige.
        bool SoportaCertificados { get; }
    }

    public class AfipConfigProvider : IAfipConfigProvider
    {
        // Proposito de Data Protection: cambiarlo invalida todas las claves guardadas.
        private const string PropositoClave = "CarniSys.CertificadoArca.ClavePfx.v1";

        private readonly IUsuarioSesionService _sesion;
        private readonly IDataProtector _protector;
        private readonly ILogger<AfipConfigProvider> _log;

        public AfipConfigProvider(IUsuarioSesionService sesion, IDataProtectionProvider dataProtection, ILogger<AfipConfigProvider> log)
        {
            _sesion = sesion;
            _protector = dataProtection.CreateProtector(PropositoClave);
            _log = log;
        }

        public bool SoportaCertificados => NegocioFactory.UsarPostgres;

        public AfipConfig Crear(bool homologacion)
        {
            var config = AfipSettings.ConfigBase();
            if (!SoportaCertificados) return config;

            string protegida = Repo().ObtenerProtegida(homologacion);
            if (string.IsNullOrEmpty(protegida)) return config;

            try
            {
                return config.ConClave(_protector.Unprotect(protegida));
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                // Claves de Data Protection perdidas o cambiadas: la solucion es cargar el certificado de nuevo.
                _log.LogError(ex, "No se pudo descifrar la clave del certificado ARCA de la empresa {IdEmpresa} ({Entorno}).", _sesion.Empresa.IdEmpresa, homologacion ? "HOMO" : "PROD");
                throw new InvalidOperationException(
                    "No se pudo descifrar la clave del certificado de AFIP. Cargá el certificado de nuevo en Configuración > Certificado ARCA.", ex);
            }
        }

        public void GuardarClave(bool homologacion, string claveEnClaro)
        {
            if (!SoportaCertificados) throw new InvalidOperationException("La gestión de certificados requiere Postgres.");
            Repo().GuardarProtegida(homologacion, _protector.Protect(claveEnClaro), _sesion.UsuarioActual?.Id ?? 0);
        }

        private DatosPostgres.CertificadoArcaClavePg Repo()
        {
            return NegocioFactory.CrearCertificadoArcaClave(_sesion.Empresa);
        }
    }
}
