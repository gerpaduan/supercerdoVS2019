using System;

namespace Negocio
{
    // Reglas del saldo de cuenta corriente que el POS muestra al elegir un cliente y que el ticket
    // de la venta imprime si el cajero lo acepta (2026-10-19, ver docs/DECISIONS.md "Saldo del
    // cliente en el POS y en el ticket"). Puras y sin dependencias para poder testearlas.
    //
    // Convencion de signo (la misma de MovCtaCte): saldo NEGATIVO = el cliente debe; saldo
    // POSITIVO = a favor del cliente.
    public static class CalculoSaldoCliente
    {
        // Monto en pesos hasta el cual el saldo NO se informa (ni en pantalla ni en el ticket) si el
        // parametro ParamKeys.SaldoMinimoInformar no esta cargado. Con la inflacion, deudas de unos
        // pocos pesos no se cobran y solo generan roces con el cliente. $100 = mismo corte que usa
        // el dashboard de cuentas corrientes para contar deudores.
        public const decimal SaldoMinimoInformarPorDefecto = 100m;

        // Redondeo a centavos. AwayFromZero para que 0.005 no desaparezca en un lado y quede en otro.
        public static decimal Redondear(decimal importe)
        {
            return Math.Round(importe, 2, MidpointRounding.AwayFromZero);
        }

        // Saldo del cliente ANTES de la venta actual, a partir del saldo leido DESPUES de guardarla.
        // Una venta a cuenta corriente ya genero su Debito (negativo) por `importeMovimientoVenta`,
        // asi que el saldo actual ya la incluye: se le suma ese importe para sacarla y no contarla
        // dos veces en el ticket. Si la venta se pago de otra forma no hay movimiento y el saldo
        // actual ya es el previo.
        public static decimal SaldoPrevioVenta(decimal saldoActual, bool ventaEnCtaCte, decimal importeMovimientoVenta)
        {
            return ventaEnCtaCte ? saldoActual + importeMovimientoVenta : saldoActual;
        }

        // Total que figura debajo del saldo en el ticket: la venta mas lo que el cliente debia
        // (saldoPrevio negativo suma; saldoPrevio positivo, a favor, resta). Puede dar negativo si
        // el saldo a favor supera la venta: el ticket lo rotula como "a favor".
        public static decimal TotalConSaldo(decimal totalVenta, decimal saldoPrevio)
        {
            return totalVenta - saldoPrevio;
        }

        // true si el saldo merece informarse: su valor absoluto (a centavos) supera el umbral. Es
        // simetrico (un saldo a favor chico tampoco se informa). Un umbral negativo (dato corrupto
        // en Parametros) se trata como 0, o sea "informar cualquier saldo distinto de cero".
        public static bool SaldoSuperaUmbral(decimal saldo, decimal umbral)
        {
            decimal piso = umbral < 0m ? 0m : umbral;
            return Math.Abs(Redondear(saldo)) > piso;
        }
    }
}
