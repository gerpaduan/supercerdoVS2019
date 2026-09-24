// Entorno de AFIP/ARCA de una empresa (homologacion = pruebas, produccion = comprobantes reales).
// Un solo criterio para facturacion y padron: solo un valor que diga HOMO explicitamente es
// homologacion; vacio/null/cualquier otra cosa es produccion (es lo que hacia facturacion antes de
// unificar; el padron trataba el vacio como homologacion, ver docs/DECISIONS.md 2026-09-24).
namespace AFIP
{
    public static class AfipEntorno
    {
        public const string Produccion = "PROD";
        public const string Homologacion = "HOMO";

        // true si Empresa.Entorno_HOMO_PROD dice homologacion ("HOMO", "Homologacion", etc.).
        public static bool EsHomologacion(string entornoHomoProd)
        {
            return !string.IsNullOrWhiteSpace(entornoHomoProd)
                && entornoHomoProd.Trim().ToUpperInvariant().Contains("HOMO");
        }

        // Valor canonico ("HOMO" o "PROD") para mostrar/guardar.
        public static string Normalizar(string entornoHomoProd)
        {
            return EsHomologacion(entornoHomoProd) ? Homologacion : Produccion;
        }
    }
}
