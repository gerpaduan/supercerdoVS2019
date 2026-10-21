using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Reglas del cobro que el cajero registra JUNTO con la venta en el modal de forma de pago del POS
    // (2026-10-21, ver docs/DECISIONS.md "Cobrar el saldo del cliente al finalizar la venta"): la venta
    // va a cuenta corriente y el cliente paga (total o parcialmente) en el momento, con la forma de
    // pago elegida. Puras y sin dependencias para poder testearlas.
    //
    // Pago mixto: se registran DOS cobros (uno en Efectivo y otro en el otro medio) y no uno solo
    // "mixto": un cobro del POS solo suma a la caja cuando su forma de pago es Efectivo
    // (Negocio.CuentaCorriente.CargarEgresoCajaPorPago), asi que un cobro unico con el efectivo adentro
    // dejaria la caja con faltante.
    public static class CobroJuntoVenta
    {
        // Diferencia maxima (en pesos) aceptada entre la suma de las partes de un cobro mixto y el
        // importe total: absorbe el centavo que pueden perder las columnas float de la base.
        public const decimal ToleranciaCentavo = 0.01m;

        public const string Efectivo = "Efectivo";

        // Formas con las que se puede cobrar. CtaCte no esta: cobrar "a cuenta corriente" no es cobrar.
        private static readonly string[] FormasPermitidas = { "Efectivo", "Debito", "Credito", "Qr", "Transferencia" };

        public sealed class ParteCobro
        {
            public ParteCobro(string formaPago, decimal importe)
            {
                FormaPago = formaPago;
                Importe = importe;
            }

            public string FormaPago { get; }
            public decimal Importe { get; }
        }

        public sealed class Resultado
        {
            private Resultado(List<ParteCobro> partes, string error)
            {
                Partes = partes;
                Error = error;
            }

            public static Resultado Correcto(List<ParteCobro> partes) { return new Resultado(partes, null); }
            public static Resultado Invalido(string error) { return new Resultado(new List<ParteCobro>(), error); }

            public bool Ok { get { return Error == null; } }
            public string Error { get; }
            public List<ParteCobro> Partes { get; }
        }

        // Nombre canonico de la forma de pago (sin importar mayusculas) o null si no se puede cobrar con ella.
        public static string NormalizarForma(string formaPago)
        {
            return FormasPermitidas.FirstOrDefault(f => string.Equals(f, (formaPago ?? "").Trim(), StringComparison.OrdinalIgnoreCase));
        }

        // Importe que salda la cuenta: lo que el cliente debe despues de la venta (saldo negativo), o 0
        // si no debe nada. Es lo que completa el tilde "Paga todo".
        public static decimal ImportePagaTodo(decimal saldoTrasVenta)
        {
            decimal saldo = CalculoSaldoCliente.Redondear(saldoTrasVenta);
            return saldo < 0m ? -saldo : 0m;
        }

        // Parte el cobro en 1 o 2 cobros reales.
        //  - Sin mixto: un cobro por `importe` con `formaPago`.
        //  - Mixto (`esMixto`): `efectivo` en Efectivo y el resto (`importe - efectivo`) en `formaPago`,
        //    que es el "otro medio" y no puede ser Efectivo. Las dos partes tienen que ser > 0 y sumar
        //    el importe (con la tolerancia de un centavo).
        // Si algo no cierra devuelve Invalido con el motivo, para rechazar ANTES de guardar la venta.
        public static Resultado Partes(decimal importe, string formaPago, bool esMixto, decimal efectivo)
        {
            decimal total = CalculoSaldoCliente.Redondear(importe);
            if (total <= 0m)
                return Resultado.Invalido("Ingresá el importe que paga el cliente.");

            string forma = NormalizarForma(formaPago);
            if (forma == null)
                return Resultado.Invalido("Elegí con qué forma de pago paga el cliente (Efectivo, Débito, Crédito, QR o Transferencia).");

            if (!esMixto)
                return Resultado.Correcto(new List<ParteCobro> { new ParteCobro(forma, total) });

            if (string.Equals(forma, Efectivo, StringComparison.OrdinalIgnoreCase))
                return Resultado.Invalido("En pago mixto el otro medio no puede ser Efectivo.");

            decimal parteEfectivo = CalculoSaldoCliente.Redondear(efectivo);
            decimal parteOtro = CalculoSaldoCliente.Redondear(total - parteEfectivo);
            if (parteEfectivo <= 0m || parteOtro <= 0m)
                return Resultado.Invalido("En pago mixto el efectivo y el otro medio tienen que ser mayores a cero.");

            if (Math.Abs(parteEfectivo + parteOtro - total) > ToleranciaCentavo)
                return Resultado.Invalido("El efectivo y el otro medio no suman el importe que paga el cliente.");

            return Resultado.Correcto(new List<ParteCobro>
            {
                new ParteCobro(Efectivo, parteEfectivo),
                new ParteCobro(forma, parteOtro)
            });
        }
    }
}
