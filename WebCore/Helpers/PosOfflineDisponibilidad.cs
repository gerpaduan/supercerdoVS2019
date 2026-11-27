// POS sin conexion (2026-11-26): dice si la funcion rige en este servidor: el freno de emergencia sin accionar
// (PosOfflineSettings.Habilitado) Y la tabla posclaveunica disponible en la base. Asi publicar el codigo en un servidor
// sin la migracion no rompe ninguna venta: la funcion queda apagada sola.
using System;
using Utilidades;
using WebCore.Infrastructure;

namespace WebCore.Helpers
{
    public static class PosOfflineDisponibilidad
    {
        // Si la tabla no esta (o la consulta fallo) se vuelve a mirar pasado este rato: al correr la migracion no hace
        // falta reiniciar. Una vez que se la vio, no se vuelve a consultar.
        public static readonly TimeSpan ReintentoSinTabla = TimeSpan.FromMinutes(5);

        // Una base por proceso (multi-empresa en la misma base).
        private static readonly Cache _cache = new Cache();

        public static bool Disponible(IEmpresaContext empresa)
        {
            if (!PosOfflineSettings.Habilitado) return false;
            return _cache.TablaDisponible(
                () => NegocioFactory.CrearPosClaveUnicaRepository(empresa).TablaDisponible(), DateTime.UtcNow);
        }

        // Recuerdo del chequeo de la tabla. Publica para poder probarla sin base.
        public sealed class Cache
        {
            private readonly object _candado = new object();
            private bool _vista;
            private DateTime _proximoIntento = DateTime.MinValue;

            public bool TablaDisponible(Func<bool> consultar, DateTime ahora)
            {
                lock (_candado)
                {
                    if (_vista) return true;
                    if (ahora < _proximoIntento) return false;
                    try
                    {
                        _vista = consultar();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Trace.TraceWarning("POS sin conexion: no se pudo comprobar la tabla posclaveunica: " + ex.Message);
                        _vista = false;
                    }
                    if (!_vista) _proximoIntento = ahora + ReintentoSinTabla;
                    return _vista;
                }
            }
        }
    }
}
