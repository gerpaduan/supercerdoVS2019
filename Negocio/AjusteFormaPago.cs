using System;
using System.Collections.Generic;
using System.Globalization;

namespace Negocio
{
    // Ajuste de precio por forma de pago (recargo o descuento sobre el precio de lista).
    //
    // Se guarda por empresa en los parametros porcAj* como FACTOR (1 = sin ajuste, 1,10 = +10%),
    // que es lo que ya lee el POS (VentasController.ObtenerConfiguracionFormaPagoPOS y
    // pos-forma-pago-precios.js). El usuario lo ve y lo carga como PORCENTAJE con signo (+10 / -5)
    // desde Productos; este helper convierte entre ambas representaciones y valida el input.
    // Ver docs/DECISIONS.md (2026-09-25).
    public static class AjusteFormaPago
    {
        // Rango permitido del porcentaje: mayor a -100 (con -100 el precio seria 0 y el POS lo
        // descarta) y hasta +500. El limite superior es un tope de cordura para evitar typos
        // (ej. 1000 en vez de 10), no una regla de negocio.
        public const decimal PorcentajeMinimoExclusivo = -100m;
        public const decimal PorcentajeMaximo = 500m;

        // Decimales del porcentaje que se aceptan (el factor resultante tiene hasta 4).
        private const int DecimalesPorcentaje = 2;
        private const int DecimalesFactor = 4;

        public sealed class Forma
        {
            public Forma(string clave, string parametro)
            {
                Clave = clave;
                Parametro = parametro;
            }

            // Nombre que usa el POS para la forma de pago (mismo texto que Entidades.Venta.formaPagoEnum).
            public string Clave { get; private set; }

            // Nombre del parametro de empresa donde se guarda el factor.
            public string Parametro { get; private set; }
        }

        // Formas de pago con ajuste editable. CtaCte no esta: siempre es factor 1 en el POS.
        // Billetera tampoco: no existe como forma de pago (decision 2026-09-25).
        public static readonly IReadOnlyList<Forma> Formas = new List<Forma>
        {
            new Forma("Efectivo", Entidades.ParamKeys.PorcAjEfectivo),
            new Forma("Debito", Entidades.ParamKeys.PorcAjDebito),
            new Forma("Credito", Entidades.ParamKeys.PorcAjCredito),
            new Forma("Qr", Entidades.ParamKeys.PorcAjQr),
            new Forma("Transferencia", Entidades.ParamKeys.PorcAjTranf)
        }.AsReadOnly();

        // +10 -> 1,10 ; -5 -> 0,95 ; 0 -> 1.
        public static decimal PorcentajeAFactor(decimal porcentaje)
        {
            return Math.Round(1m + porcentaje / 100m, DecimalesFactor);
        }

        // 1,10 -> 10 ; 0,95 -> -5 ; 1 -> 0.
        public static decimal FactorAPorcentaje(decimal factor)
        {
            return Math.Round((factor - 1m) * 100m, DecimalesPorcentaje);
        }

        // Texto que se persiste en empresaparametros.valor: cultura invariante (punto decimal),
        // que es lo que Negocio.Parametros.GetDecimal espera.
        public static string FactorATexto(decimal factor)
        {
            return factor.ToString("0.####", CultureInfo.InvariantCulture);
        }

        // Interpreta el porcentaje tipeado por el usuario ("10", "+10", "-5,5", "3.25").
        // false si esta vacio, no es numero, tiene mas de 2 decimales o queda fuera de rango.
        public static bool TryParsePorcentaje(string texto, out decimal porcentaje)
        {
            porcentaje = 0m;
            if (string.IsNullOrWhiteSpace(texto)) return false;

            // La coma es el separador decimal habitual del usuario; internamente siempre punto.
            string normalizado = texto.Trim().Replace(',', '.');

            decimal valor;
            if (!decimal.TryParse(normalizado,
                    NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture,
                    out valor))
                return false;

            // Rechazamos (no redondeamos en silencio) mas precision de la soportada.
            if (Math.Round(valor, DecimalesPorcentaje) != valor) return false;

            if (valor <= PorcentajeMinimoExclusivo || valor > PorcentajeMaximo) return false;

            porcentaje = valor;
            return true;
        }
    }
}
