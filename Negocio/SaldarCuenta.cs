namespace Negocio
{
    // Regla del interruptor "Saldar cuenta" del alta de pagos (2026-10-22, ver docs/DECISIONS.md): a partir
    // del saldo de cuenta corriente de la persona decide que operacion y que importe la dejan en cero.
    // Pura y sin dependencias para poder testearla.
    //
    // Convencion de signo (la de MovCtaCte): saldo NEGATIVO = la persona nos debe -> se registra un COBRO;
    // saldo POSITIVO = le debemos -> se registra un PAGO. El importe del pago es siempre positivo.
    public static class SaldarCuenta
    {
        public sealed class Resultado
        {
            public Resultado(bool puedeSaldar, bool aProveedor, decimal importe)
            {
                PuedeSaldar = puedeSaldar;
                AProveedor = aProveedor;
                Importe = importe;
            }

            // false si la cuenta ya esta en cero (no hay nada que saldar).
            public bool PuedeSaldar { get; }

            // true = pago (entrego dinero), false = cobro (recibo dinero). Igual que Entidades.Pago.AProveedor.
            public bool AProveedor { get; }

            // Valor absoluto del saldo, redondeado a centavos; 0 si no se puede saldar.
            public decimal Importe { get; }
        }

        public static Resultado Resolver(decimal saldo)
        {
            // Un saldo que redondea a 0 centavos se considera saldado: el pago exige importe > 0.
            decimal redondeado = CalculoSaldoCliente.Redondear(saldo);
            if (redondeado == 0m)
                return new Resultado(false, false, 0m);

            return redondeado < 0m
                ? new Resultado(true, false, -redondeado)
                : new Resultado(true, true, redondeado);
        }
    }
}
