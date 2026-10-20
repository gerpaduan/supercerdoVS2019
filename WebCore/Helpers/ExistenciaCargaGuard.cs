// Guard de carga de Existencia por sucursales: decide si una actualizacion AUTOMATICA puede calcular el stock ahora o conviene
// postergarla (servidor ocupado). La logica vive en Negocio.LimitadorCalculo (pura y con tests); aca solo se mantiene la
// instancia unica del proceso y se le pasan los limites configurables (ExistenciaCargaSettings).
// Uso en StockController.BuscarExistenciaPorSucursales: TryEntrar -> calcular -> Salir (try/finally).
// Detalle: docs/DECISIONS.md 2026-10-19 y docs/11-manual-de-uso/pantallas/Stock.ExistenciaPorSucursales.md.
using System;

namespace WebCore.Helpers
{
    public static class ExistenciaCargaGuard
    {
        // Una sola instancia por proceso: el estado (calculos en curso, ultima duracion) es del servidor, no de la sesion.
        private static readonly Negocio.LimitadorCalculo Limitador = new Negocio.LimitadorCalculo();

        // true = puede calcular (y DEBE llamar a Salir). false = servidor ocupado: segundosReintento dice cuando volver a probar.
        public static bool TryEntrar(bool automatica, out int segundosReintento)
        {
            return Limitador.TryEntrar(
                automatica,
                ExistenciaCargaSettings.MaxConcurrentes,
                TimeSpan.FromMilliseconds(ExistenciaCargaSettings.UmbralLatenciaMs),
                TimeSpan.FromSeconds(ExistenciaCargaSettings.ReintentoSegundos),
                out segundosReintento);
        }

        public static void Salir(TimeSpan duracion)
        {
            Limitador.Salir(duracion);
        }
    }
}
