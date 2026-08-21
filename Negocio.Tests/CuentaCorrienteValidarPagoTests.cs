using System;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre Negocio.CuentaCorriente.ValidarPago -- validacion pura de reglas de negocio, sin
    // tocar ningun repositorio en el camino sin cheques (el unico que se prueba aca; el camino
    // con cheques llama a getChequePorIDorNro contra el repo real, fuera de alcance de este
    // archivo). El repo inyectado (FakeCuentaCorrienteRepository) nunca deberia ser tocado en
    // ninguno de estos tests -- si algun dia lo es, es un cambio real de comportamiento.
    public class CuentaCorrienteValidarPagoTests
    {
        private const int IdConsumidorFinal = 1;

        private static Negocio.CuentaCorriente CrearSut() =>
            new Negocio.CuentaCorriente(
                new FakeCuentaCorrienteRepository(),
                new EmpresaContextFake(1),
                new FakeParametrosContext().ConInt(ParamKeys.IdConsumidorFinal, IdConsumidorFinal));

        private static Pago CrearPagoValido() => new Pago
        {
            Persona = new Persona { idPersona = 13 },
            Fecha = DateTime.Now.AddMinutes(-5),
            Sucursal = new Sucursal { IdSucursal = 1 },
            FormaPago = Pago.formasPago.Efectivo.ToString(),
            Importe = 100,
        };

        [Fact]
        public void PagoValido_Pasa()
        {
            var (ok, mensaje) = CrearSut().ValidarPago(CrearPagoValido());

            Assert.True(ok);
            Assert.True(string.IsNullOrEmpty(mensaje));
        }

        [Fact]
        public void PagoNulo_NoPasa()
        {
            var (ok, _) = CrearSut().ValidarPago(null);
            Assert.False(ok);
        }

        [Fact]
        public void SinPersona_NoPasa()
        {
            var pago = CrearPagoValido();
            pago.Persona = null;

            var (ok, mensaje) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
            Assert.Contains("persona", mensaje, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void PersonaEsConsumidorFinal_NoPasa()
        {
            var pago = CrearPagoValido();
            pago.Persona = new Persona { idPersona = IdConsumidorFinal };

            var (ok, mensaje) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
            Assert.Contains("Consumidor Final", mensaje);
        }

        [Fact]
        public void FechaEnElFuturo_NoPasa()
        {
            var pago = CrearPagoValido();
            pago.Fecha = DateTime.Now.AddDays(1);

            var (ok, _) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
        }

        [Fact]
        public void SinSucursal_NoPasa()
        {
            var pago = CrearPagoValido();
            pago.Sucursal = null;

            var (ok, mensaje) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
            Assert.Contains("sucursal", mensaje, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void SinFormaDePago_NoPasa()
        {
            var pago = CrearPagoValido();
            pago.FormaPago = "";

            var (ok, _) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
        }

        [Fact]
        public void ImporteCero_ConFormaDePagoNormal_NoPasaPorElImporte()
        {
            var pago = CrearPagoValido();
            pago.Importe = 0;

            var (ok, mensaje) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
            Assert.Contains("importe", mensaje, StringComparison.OrdinalIgnoreCase);
        }

        // Hallazgo real leyendo el codigo: FormaPago="Otro" tiene una excepcion explicita en el
        // primer chequeo de Importe ("if FormaPago != Otro && Importe<=0"), que sugiere que un
        // pago "Otro" con Importe=0 deberia pasar. No es asi -- el bloque posterior de "campos
        // obligatorios" vuelve a exigir Importe>0 sin la misma excepcion, asi que termina
        // fallando igual, solo que con un mensaje distinto ("Complete los siguientes campos")
        // en vez de "Ingrese un importe mayor a 0.". Se documenta con un test para que quede
        // fijado en el codigo, no solo como hallazgo de esta sesion.
        [Fact]
        public void ImporteCero_ConFormaDePagoOtro_TampocoPasa_PeroConMensajeDistinto()
        {
            var pago = CrearPagoValido();
            pago.FormaPago = Pago.formasPago.Otro.ToString();
            pago.Importe = 0;

            var (ok, mensaje) = CrearSut().ValidarPago(pago);

            Assert.False(ok);
            Assert.Contains("Complete los siguientes campos", mensaje);
            Assert.DoesNotContain("Ingrese un importe mayor a 0", mensaje);
        }
    }
}
