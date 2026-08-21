using System;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Tests unitarios (sin base de datos real) de la logica de anulacion de MovCtaCte en
    // Negocio.CuentaCorriente.crearMovCtaCte -- misma logica que usan Venta, Compra y Pagos.
    // Replica, con asserts, los 3 escenarios que se verificaron a mano (HTTP + SQL directo,
    // ambos motores) el 2026-08-20 para el modulo de Pagos/Cobros: ver docs/DECISIONS.md.
    //
    // El repositorio real (FakeCuentaCorrienteRepository) no toca SQL Server ni Postgres --
    // solo replica la semantica exacta de CuentaCorrientePg.getMovCtaCteBy/addOrEditMovCtaCte
    // (ultimo registro por Tabla+IdTabla, insert si Id==0, update in-place si no).
    public class CuentaCorrienteAnulacionTests
    {
        private static readonly Persona PersonaA = new Persona { idPersona = 13 };
        private static readonly Persona PersonaB = new Persona { idPersona = 19 };
        private static readonly Sucursal SucursalTest = new Sucursal { IdSucursal = 1 };
        private static readonly Usuario UsuarioTest = new Usuario { Id = 2 };

        private static (Negocio.CuentaCorriente ctaCte, FakeCuentaCorrienteRepository repo) CrearSut()
        {
            var repo = new FakeCuentaCorrienteRepository();
            var empresa = new EmpresaContextFake(1);
            var ctaCte = new Negocio.CuentaCorriente(repo, empresa);
            return (ctaCte, repo);
        }

        private static Pago CrearPago(int id, Persona persona, float importe, bool aProveedor) => new Pago
        {
            Id = id,
            Persona = persona,
            Fecha = new DateTime(2026, 8, 20, 10, 0, 0),
            NroRecibo = "TEST-001",
            FormaPago = "Efectivo",
            AProveedor = aProveedor,
            Importe = importe,
            Sucursal = SucursalTest,
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            CreadoPor = UsuarioTest,
        };

        [Fact]
        public void PrimerPago_CreaUnSoloMovimiento()
        {
            var (ctaCte, repo) = CrearSut();
            var pago = CrearPago(id: 61, PersonaA, importe: 100, aProveedor: true);

            ctaCte.crearMovCtaCtePago(pago, oCierreCajaE: null, oPagoAnterior: null);

            var mov = Assert.Single(repo.Movimientos);
            Assert.Equal(MovCtaCte.tipoMov.Debito.ToString(), mov.Tipo);
            Assert.Equal(-100, mov.Importe);
            Assert.False(mov.QuitadoCtaCta);
        }

        [Fact]
        public void ModificarImporte_AnulaElViejoYCreaUnoNuevo()
        {
            var (ctaCte, repo) = CrearSut();
            ctaCte.crearMovCtaCtePago(CrearPago(61, PersonaA, 100, true), null, null);

            ctaCte.crearMovCtaCtePago(CrearPago(61, PersonaA, 150, true), null, null);

            Assert.Equal(3, repo.Movimientos.Count);

            var original = repo.Movimientos[0];
            Assert.Equal(MovCtaCte.tipoMov.Debito.ToString(), original.Tipo);
            Assert.Equal(-100, original.Importe);

            var anulacion = repo.Movimientos[1];
            Assert.Equal(MovCtaCte.tipoMov.Credito.ToString(), anulacion.Tipo);
            Assert.Equal(100, anulacion.Importe);
            Assert.Contains("ANULACION", anulacion.Detalle);

            var nuevo = repo.Movimientos[2];
            Assert.Equal(MovCtaCte.tipoMov.Debito.ToString(), nuevo.Tipo);
            Assert.Equal(-150, nuevo.Importe);
        }

        [Fact]
        public void PagoAConvertidoEnCobro_AnulaElViejoYCreaUnoNuevo()
        {
            var (ctaCte, repo) = CrearSut();
            ctaCte.crearMovCtaCtePago(CrearPago(61, PersonaA, 150, aProveedor: true), null, null);

            ctaCte.crearMovCtaCtePago(CrearPago(61, PersonaA, 150, aProveedor: false), null, null);

            Assert.Equal(3, repo.Movimientos.Count);

            var anulacion = repo.Movimientos[1];
            Assert.Equal(MovCtaCte.tipoMov.Credito.ToString(), anulacion.Tipo);
            Assert.Equal(150, anulacion.Importe);
            Assert.Contains("ANULACION", anulacion.Detalle);

            var nuevo = repo.Movimientos[2];
            Assert.Equal(MovCtaCte.tipoMov.Credito.ToString(), nuevo.Tipo);
            Assert.Equal(150, nuevo.Importe);
            Assert.DoesNotContain("ANULACION", nuevo.Detalle);
        }

        // Confirma el hallazgo de la sesion 2026-08-20 (docs/DECISIONS.md): un cambio de
        // persona con el mismo Tipo/Importe NO anula -- actualiza el registro existente
        // in-place. Comportamiento preexistente, ya decidido con el usuario (no un bug).
        [Fact]
        public void CambiarSoloLaPersona_ActualizaElMovimientoExistenteSinAnular()
        {
            var (ctaCte, repo) = CrearSut();
            ctaCte.crearMovCtaCtePago(CrearPago(61, PersonaA, 150, aProveedor: false), null, null);
            int cantidadAntes = repo.Movimientos.Count;

            ctaCte.crearMovCtaCtePago(CrearPago(61, PersonaB, 150, aProveedor: false), null, null);

            Assert.Equal(cantidadAntes, repo.Movimientos.Count);
            var ultimo = repo.Movimientos.Last();
            Assert.Equal(PersonaB.idPersona, ultimo.Persona.idPersona);
        }
    }
}
