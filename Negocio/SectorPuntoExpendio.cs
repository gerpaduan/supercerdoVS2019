using System;

namespace Negocio
{
    // Reglas de los sectores del Punto de Expendio que tienen comportamiento propio:
    //  - PRESUPUESTO: lista de precios para el cliente (precio editable, PDF/email de "nuevos
    //    precios"); admite fecha futura (precios que rigen a partir de un dia).
    //  - REMITOS: expendio que respalda un remito; admite fecha editable pero nunca futura.
    // Ambos son sectores GLOBALES (fila con idempresa = 0 en `sectores`, visibles a todas las
    // empresas) y estan reservados: no se pueden renombrar ni eliminar. El resto de los
    // sectores (Carniceria, Ramos Generales, etc.) sigue siendo por empresa y sin reglas propias.
    // Ver docs/DECISIONS.md (2026-09-26).
    public static class SectorPuntoExpendio
    {
        public const string Presupuesto = "PRESUPUESTO";
        public const string Remitos = "REMITOS";

        // Hasta cuantos dias a futuro se acepta la fecha de un presupuesto. Tope de cordura
        // contra typos (ej. anio 2062), no una regla de negocio.
        public const int DiasMaximosFuturoPresupuesto = 365;

        // Tolerancia por diferencia de reloj entre el navegador y el servidor: el JS manda la
        // hora local del cliente, que puede adelantar unos minutos respecto del servidor.
        public const int ToleranciaRelojMinutos = 5;

        // Caducidad de un presupuesto (solo sector PRESUPUESTO): cantidad de dias, contados desde
        // el dia de vigencia, durante los que sus precios se pueden copiar a una venta. Si el
        // usuario no elige, rige el valor por defecto; el tope evita typos (ej. 9999 dias).
        public const int DiasCaducidadPresupuestoPorDefecto = 90;
        public const int DiasMaximosCaducidadPresupuesto = 365;

        // Un cliente con algun presupuesto con vigencia dentro de este lapso (hacia atras) abre el
        // historial de precios en la solapa Presupuestos aunque ya hayan caducado. No es la
        // caducidad (que es por presupuesto), solo decide la solapa inicial del modal.
        public const int MesesPresupuestoRecienteSolapaInicial = 6;

        public static bool EsPresupuesto(string sector)
        {
            return string.Equals((sector ?? "").Trim(), Presupuesto, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EsRemitos(string sector)
        {
            return string.Equals((sector ?? "").Trim(), Remitos, StringComparison.OrdinalIgnoreCase);
        }

        // Reservado = no se puede crear, renombrar ni eliminar desde el ABM de sectores.
        public static bool EsReservado(string sector)
        {
            return EsPresupuesto(sector) || EsRemitos(sector);
        }

        // Sectores en los que el usuario puede cambiar la fecha/hora del expendio en el POS.
        public static bool PermiteFechaManual(string sector)
        {
            return EsPresupuesto(sector) || EsRemitos(sector);
        }

        // Fecha con la que se guarda el expendio. Devuelve null en `error` si es valida.
        //  - Sin fecha solicitada, o sector sin fecha manual: ahora (se ignora lo que mande el
        //    cliente, igual que hoy: el POS siempre manda la hora actual).
        //  - REMITOS: no puede ser futura.
        //  - PRESUPUESTO: puede ser futura hasta DiasMaximosFuturoPresupuesto.
        public static DateTime ResolverFecha(string sector, DateTime? fechaSolicitada, DateTime ahora, out string error)
        {
            error = null;

            if (!fechaSolicitada.HasValue || !PermiteFechaManual(sector))
                return ahora;

            DateTime fecha = fechaSolicitada.Value;

            if (EsRemitos(sector))
            {
                if (fecha > ahora.AddMinutes(ToleranciaRelojMinutos))
                {
                    error = "La fecha del remito no puede ser futura.";
                    return ahora;
                }

                // Con tolerancia de reloj: una hora apenas adelantada se guarda como "ahora".
                return fecha > ahora ? ahora : fecha;
            }

            if (fecha > ahora.AddDays(DiasMaximosFuturoPresupuesto))
            {
                error = "La fecha del presupuesto no puede superar " + DiasMaximosFuturoPresupuesto + " días a futuro.";
                return ahora;
            }

            return fecha;
        }

        // Fecha de caducidad con la que se guarda el expendio. La UI pide DIAS y el servidor
        // calcula la fecha (nunca se confia en una fecha enviada por el cliente): ultimo dia
        // inclusive = dia de vigencia + dias.
        //  - Sector distinto de PRESUPUESTO: no aplica, devuelve null y sin error.
        //  - Sin dias (null): se usan DiasCaducidadPresupuestoPorDefecto.
        //  - Dias fuera de 1..DiasMaximosCaducidadPresupuesto: devuelve null y `error`.
        public static DateTime? ResolverCaducidad(string sector, DateTime vigencia, int? dias, out string error)
        {
            error = null;

            if (!EsPresupuesto(sector))
                return null;

            int diasEfectivos = dias ?? DiasCaducidadPresupuestoPorDefecto;
            if (diasEfectivos < 1 || diasEfectivos > DiasMaximosCaducidadPresupuesto)
            {
                error = "Los días de validez del presupuesto deben estar entre 1 y " + DiasMaximosCaducidadPresupuesto + ".";
                return null;
            }

            return vigencia.Date.AddDays(diasEfectivos);
        }
    }
}
