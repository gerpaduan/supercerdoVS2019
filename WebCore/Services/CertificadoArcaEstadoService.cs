// Estado del certificado ARCA de la empresa actual y aviso de vencimiento en la campana del admin.
//  - Leer: abre el .pfx (con la clave guardada) y devuelve vencimiento/estado, para la pantalla.
//  - EvaluarAviso: si el certificado esta por vencer o vencido, crea/actualiza la notificacion
//    CERTIFICADO_ARCA_POR_VENCER; si ya esta bien, cierra las pendientes. No hay proceso en segundo plano:
//    lo dispara NotificacionesController.Resumen (la campana consulta cada ~60 s) y se limita a una
//    evaluacion por hora por empresa, para no releer el .pfx ni escribir en la base en cada consulta.
// Detalle: docs/06-datos-e-integraciones/afip-y-facturacion.md.
using System.Collections.Concurrent;
using System.Globalization;
using AFIP;
using WebCore.Helpers;
using WebCore.Infrastructure;

namespace WebCore.Services
{
    public interface ICertificadoArcaEstado
    {
        // Empresa del usuario actual (con CUIT, nombre del pfx y entorno), o null si no se pudo resolver.
        Entidades.Empresa EmpresaActual();

        InfoCertificado Leer(Entidades.Empresa empresa);

        // Crea/actualiza/cierra el aviso segun el estado. Solo Postgres (usa la tabla notificaciones).
        void EvaluarAviso(Entidades.Empresa empresa, bool forzar = false);
    }

    public class CertificadoArcaEstadoService : ICertificadoArcaEstado
    {
        // Minimo entre evaluaciones del aviso por empresa (la campana consulta cada ~60 s).
        private static readonly TimeSpan IntervaloEvaluacion = TimeSpan.FromHours(1);
        private static readonly ConcurrentDictionary<int, DateTime> UltimaEvaluacion = new ConcurrentDictionary<int, DateTime>();
        private static readonly CultureInfo CulturaAr = new CultureInfo("es-AR");

        private readonly IWebHostEnvironment _env;
        private readonly IAfipConfigProvider _afip;
        private readonly IUsuarioSesionService _sesion;
        private readonly ILogger<CertificadoArcaEstadoService> _log;

        public CertificadoArcaEstadoService(IWebHostEnvironment env, IAfipConfigProvider afip, IUsuarioSesionService sesion, ILogger<CertificadoArcaEstadoService> log)
        {
            _env = env;
            _afip = afip;
            _sesion = sesion;
            _log = log;
        }

        public Entidades.Empresa EmpresaActual()
        {
            var usuario = _sesion.UsuarioActual;
            var empresa = usuario?.Empresa;
            if (empresa == null || empresa.Cuit <= 0)
            {
                int idEmpresa = usuario?.IdEmpresa ?? 0;
                if (idEmpresa > 0)
                    empresa = NegocioFactory.CrearSucursal(_sesion.Empresa).findEmpresaById(idEmpresa);
            }
            return empresa;
        }

        public InfoCertificado Leer(Entidades.Empresa empresa)
        {
            var servicio = new CertificadoArcaService(_env.ContentRootPath);
            string clave;
            try
            {
                clave = _afip.Crear().ClaveCertificado;
            }
            catch (InvalidOperationException ex)
            {
                // Clave guardada que no se puede descifrar: se informa como certificado ilegible.
                return new InfoCertificado
                {
                    NombreArchivo = AfipRutas.NombreCertificado(empresa.NombreCertificado_pfx),
                    Estado = EstadoCertificado.Ilegible,
                    Mensaje = ex.Message
                };
            }

            return servicio.Leer(empresa.Cuit, empresa.NombreCertificado_pfx, clave, AfipSettings.DiasAviso);
        }

        public void EvaluarAviso(Entidades.Empresa empresa, bool forzar = false)
        {
            if (empresa == null || empresa.Cuit <= 0) return;
            if (!PosBorradorSettings.SoportaNotificaciones) return;

            var ahora = DateTime.UtcNow;
            if (!forzar && UltimaEvaluacion.TryGetValue(empresa.IdEmpresa, out var ultima) && ahora - ultima < IntervaloEvaluacion)
                return;
            UltimaEvaluacion[empresa.IdEmpresa] = ahora;

            try
            {
                var info = Leer(empresa);
                var negocio = NegocioFactory.CrearVentaBorrador(_sesion.Empresa);
                var pendientes = negocio.ListarNotificaciones(true, 200)
                    .Where(n => n.Tipo == Entidades.Notificacion.TipoCertificadoArcaPorVencer).ToList();

                // Sin cert, ilegible o dentro de plazo: no hay nada que avisar por vencimiento; se cierran los avisos viejos.
                bool hayQueAvisar = info.Vence.HasValue && info.DiasRestantes.HasValue
                    && (info.Estado == EstadoCertificado.PorVencer || info.Estado == EstadoCertificado.Vencido);
                if (!hayQueAvisar)
                {
                    foreach (var n in pendientes)
                        negocio.AtenderNotificacion(n.Id, _sesion.UsuarioActual.Id);
                    return;
                }

                long refId = long.Parse(info.Vence.Value.ToString("yyyyMMdd", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
                int banda = CertificadoArcaService.BandaAviso(info.DiasRestantes.Value, AfipSettings.DiasAviso);
                string titulo = banda == 0
                    ? "Certificado de AFIP/ARCA vencido"
                    : "Certificado de AFIP/ARCA vence en menos de " + banda.ToString(CultureInfo.InvariantCulture) + " días";
                string mensaje = banda == 0
                    ? "El certificado de facturación electrónica venció el " + info.Vence.Value.ToString("dd/MM/yyyy", CulturaAr)
                        + ". No se puede facturar hasta renovarlo (Configuración > Certificado ARCA)."
                    : "El certificado de facturación electrónica vence el " + info.Vence.Value.ToString("dd/MM/yyyy", CulturaAr)
                        + " (faltan " + info.DiasRestantes.Value.ToString(CultureInfo.InvariantCulture)
                        + " días). Renovarlo en Configuración > Certificado ARCA.";

                // Se vuelve a marcar como pendiente solo cuando cambia de banda (60 -> 30 -> 15 -> vencido).
                var existente = negocio.ListarNotificaciones(false, 200)
                    .FirstOrDefault(n => n.Tipo == Entidades.Notificacion.TipoCertificadoArcaPorVencer && n.RefId == refId);
                bool reabrir = existente == null || !string.Equals(existente.Titulo, titulo, StringComparison.Ordinal);

                negocio.RegistrarNotificacion(new Entidades.Notificacion
                {
                    Tipo = Entidades.Notificacion.TipoCertificadoArcaPorVencer,
                    Severidad = Entidades.Notificacion.SeveridadAdvertencia,
                    Titulo = titulo,
                    Mensaje = mensaje,
                    RefId = refId
                }, reabrir);

                // Avisos de un vencimiento anterior (certificado ya renovado): cerrarlos.
                foreach (var n in pendientes.Where(n => n.RefId != refId))
                    negocio.AtenderNotificacion(n.Id, _sesion.UsuarioActual.Id);
            }
            catch (Exception ex)
            {
                // El aviso es accesorio: un fallo (base, archivo) no debe romper la campana. Se reintenta en la
                // proxima hora y queda logueado con el tipo de error, sin datos del certificado.
                _log.LogError(ex, "No se pudo evaluar el aviso de vencimiento del certificado ARCA (empresa {IdEmpresa}).", empresa.IdEmpresa);
            }
        }
    }
}
