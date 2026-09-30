using System;

namespace WebCore.Helpers
{
    // Formato de nombre para mostrar un empleado en listas/combos (Empleados/Index, los combos de
    // Jornadas/Liquidaciones, las tarjetas de Fichaje). Evita mostrar "Nombre (Nombre)" cuando la
    // identificación quedó cargada igual a la razón social (2026-09-30, pedido explícito).
    public static class EmpleadoDisplay
    {
        public static string NombreConIdentificacion(string razonSocial, string identificacion)
        {
            razonSocial = (razonSocial ?? "").Trim();
            identificacion = (identificacion ?? "").Trim();

            if (string.IsNullOrEmpty(identificacion) || string.Equals(razonSocial, identificacion, StringComparison.OrdinalIgnoreCase))
                return razonSocial;

            return $"{razonSocial} ({identificacion})";
        }
    }
}
