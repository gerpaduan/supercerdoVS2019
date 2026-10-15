using System;

namespace Negocio
{
    // Regla unica del bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por
    // inactividad"): cuantos minutos sin actividad tolera una sesion antes de avisar y cerrarla
    // (web) o bloquearla (POS). Pura y sin dependencias para poder testearla.
    public static class PoliticaInactividad
    {
        // Duracion del cartel "Extender sesion" en la web normal; vencido, se cierra la sesion.
        public const int MinutosAvisoExtender = 5;

        // 0 = nunca bloquea. Exentos: el usuario de produccion (cuenta compartida de sala, siempre
        // abierta) y los dispositivos marcados "No bloquear sesion" (ej. la PC propia de un admin).
        // Un valor negativo (dato corrupto) se trata como 0 para no bloquear por error.
        public static int MinutosEfectivos(bool esUsuarioProduccion, int minutosInactividad, bool dispositivoSinBloqueo)
        {
            if (esUsuarioProduccion || dispositivoSinBloqueo)
                return 0;
            return Math.Max(0, minutosInactividad);
        }
    }
}
