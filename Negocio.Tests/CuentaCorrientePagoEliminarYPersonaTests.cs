using System;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Tests unitarios (sin base de datos) de lo agregado el 2026-09-24 a Negocio.CuentaCorriente:
    // eliminar un pago con asiento opuesto, cambio de persona (auditoria + egreso de caja) y la
    // advertencia de fecha distinta a la de carga. Ver docs/DECISIONS.md "Pagos: listado, cambio de
    // persona y eliminacion".
    public class CuentaCorrientePagoEliminarYPersonaTests
    {
        private static readonly Persona PersonaAna = new Persona { idPersona = 13, razonSocial = "Ana" };
        private static readonly Persona PersonaBeto = new Persona { idPersona = 19, razonSocial = "Beto" };
        private static readonly Sucursal SucursalTest = new Sucursal { IdSucursal = 1 };
        private static readonly Usuario UsuarioTest = new Usuario { Id = 2, Nombre = "Admin" };

        private static (Negocio.CuentaCorriente ctaCte, FakeCuentaCorrienteRepository repo, FakeCierreCajaRepository cierreRepo) CrearSut()
        {
            var repo = new FakeCuentaCorrienteRepository();
            var cierreRepo = new FakeCierreCajaRepository();
            var empresa = new EmpresaContextFake(1);
            var ctaCte = new Negocio.CuentaCorriente(repo, empresa)
            {
                CierreCajaFactory = () => new Negocio.CierreCaja(cierreRepo, empresa)
            };
            return (ctaCte, repo, cierreRepo);
        }

        private static Pago CrearPago(int id, Persona persona, float importe, bool aProveedor, DateTime fecha, DateTime creado) => new Pago
        {
            Id = id,
            Persona = persona,
            Fecha = fecha,
            NroRecibo = "TEST-001",
            FormaPago = "Efectivo",
            AProveedor = aProveedor,
            Importe = importe,
            Sucursal = SucursalTest,
            Creado = creado,
            CreadoPor = UsuarioTest,
            ActualizadoPor = UsuarioTest,
        };

        private static Pago CrearPagoDeHoy(int id, Persona persona, float importe, bool aProveedor) =>
            CrearPago(id, persona, importe, aProveedor, DateTime.Today.AddHours(10), DateTime.Today.AddHours(10));

        // ---------------- Eliminar ----------------

        [Fact]
        public void Eliminar_GeneraAsientoOpuestoYElSaldoNetoEsCero()
        {
            var (ctaCte, repo, _) = CrearSut();
            var pago = CrearPagoDeHoy(61, PersonaAna, 100, aProveedor: false);
            ctaCte.crearMovCtaCtePago(pago, oCierreCajaE: null, oPagoAnterior: null);
            repo.PagosGuardados[61] = pago;

            var (ok, mensaje) = ctaCte.eliminarPagoConContraasiento(61, "Se cargó dos veces", UsuarioTest);

            Assert.True(ok, mensaje);
            Assert.Equal(2, repo.Movimientos.Count);
            Assert.Equal(0f, repo.Movimientos.Sum(m => m.Importe));
            Assert.StartsWith("ELIMINADO - ", repo.Movimientos.Last().Detalle);
            Assert.Contains(61, repo.PagosMarcadosEliminados);
        }

        [Fact]
        public void Eliminar_DejaAuditoriaYNotificacionAlAdmin()
        {
            var (ctaCte, repo, _) = CrearSut();
            var pago = CrearPagoDeHoy(62, PersonaAna, 100, aProveedor: true);
            ctaCte.crearMovCtaCtePago(pago, null, null);
            repo.PagosGuardados[62] = pago;

            ctaCte.eliminarPagoConContraasiento(62, "Error de carga", UsuarioTest);

            var auditoria = Assert.Single(repo.Auditorias);
            Assert.Equal(AuditoriaPago.TipoEliminacion, auditoria.Tipo);
            Assert.Equal("Error de carga", auditoria.Detalle);
            var notificacion = Assert.Single(repo.Notificaciones);
            Assert.Equal(Notificacion.TipoPagoEliminado, notificacion.Tipo);
            Assert.Equal(62, notificacion.RefId);
        }

        [Fact]
        public void Eliminar_EgresoDeCaja_GeneraElOpuestoFechadoHoy()
        {
            var (ctaCte, repo, cierreRepo) = CrearSut();
            var pago = CrearPagoDeHoy(63, PersonaAna, 100, aProveedor: true);
            ctaCte.crearMovCtaCtePago(pago, null, null);
            repo.PagosGuardados[63] = pago;
            cierreRepo.EgresoCajaExistente = new EgresoCaja
            {
                Id = 7,
                Monto = 100,
                Descripcion = "Pago a Ana - ID:63",
                Fecha = DateTime.Today.AddDays(-3)
            };

            ctaCte.eliminarPagoConContraasiento(63, "Error", UsuarioTest);

            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.NotNull(egreso);
            Assert.Equal(0, egreso.Id);
            Assert.Equal(-100, egreso.Monto);
            Assert.StartsWith("ELIMINADO: ", egreso.Descripcion);
            Assert.True(egreso.Fecha >= DateTime.Today);
        }

        [Fact]
        public void Eliminar_DosVeces_LaSegundaSeRechaza()
        {
            var (ctaCte, repo, _) = CrearSut();
            var pago = CrearPagoDeHoy(64, PersonaAna, 100, aProveedor: false);
            ctaCte.crearMovCtaCtePago(pago, null, null);
            repo.PagosGuardados[64] = pago;

            Assert.True(ctaCte.eliminarPagoConContraasiento(64, "x", UsuarioTest).ok);
            pago.Eliminado = true; // getPagoById real devolveria el pago ya marcado

            var (ok, mensaje) = ctaCte.eliminarPagoConContraasiento(64, "x", UsuarioTest);

            Assert.False(ok);
            Assert.Contains("ya fue eliminado", mensaje);
            Assert.Equal(2, repo.Movimientos.Count); // sin un tercer asiento
        }

        [Fact]
        public void Eliminar_SinMotivo_SeRechazaSinTocarNada()
        {
            var (ctaCte, repo, _) = CrearSut();
            var pago = CrearPagoDeHoy(65, PersonaAna, 100, aProveedor: false);
            ctaCte.crearMovCtaCtePago(pago, null, null);
            repo.PagosGuardados[65] = pago;

            var (ok, _) = ctaCte.eliminarPagoConContraasiento(65, "   ", UsuarioTest);

            Assert.False(ok);
            Assert.Empty(repo.PagosMarcadosEliminados);
            Assert.Single(repo.Movimientos);
        }

        [Fact]
        public void Eliminar_PagoInexistente_SeRechaza()
        {
            var (ctaCte, _, _) = CrearSut();

            var (ok, mensaje) = ctaCte.eliminarPagoConContraasiento(999, "x", UsuarioTest);

            Assert.False(ok);
            Assert.Contains("no existe", mensaje);
        }

        // ---------------- Cambio de persona ----------------

        [Fact]
        public void CambiarPersona_MutaElMovimientoYDejaAuditoria()
        {
            var (ctaCte, repo, _) = CrearSut();
            var original = CrearPagoDeHoy(70, PersonaAna, 100, aProveedor: false);
            ctaCte.addOrEditPago(original, null, null);
            int idMovimiento = repo.Movimientos.Single().Id;

            var anterior = CrearPagoDeHoy(70, PersonaAna, 100, aProveedor: false);
            var editado = CrearPagoDeHoy(70, PersonaBeto, 100, aProveedor: false);
            ctaCte.addOrEditPago(editado, null, anterior);

            // decision del usuario: si cambia solo la persona se muta el movimiento, no se anula/recrea
            var mov = Assert.Single(repo.Movimientos);
            Assert.Equal(idMovimiento, mov.Id);
            Assert.Equal(PersonaBeto.idPersona, mov.Persona.idPersona);

            var auditoria = Assert.Single(repo.Auditorias);
            Assert.Equal(AuditoriaPago.TipoCambioPersona, auditoria.Tipo);
            Assert.Equal(PersonaAna.idPersona, auditoria.IdPersonaAnterior);
            Assert.Equal(PersonaBeto.idPersona, auditoria.IdPersonaNueva);
            Assert.Equal(UsuarioTest.Id, auditoria.IdUsuario);
        }

        [Fact]
        public void CambiarPersona_ActualizaLaDescripcionDelEgresoDeCaja()
        {
            var (ctaCte, _, cierreRepo) = CrearSut();
            ctaCte.addOrEditPago(CrearPagoDeHoy(71, PersonaAna, 100, aProveedor: false), null, null);
            cierreRepo.EgresoCajaExistente = new EgresoCaja { Id = 9, Monto = -100, Descripcion = "Cobro a Ana - ID:71 | Efectivo $100,00" };

            ctaCte.addOrEditPago(CrearPagoDeHoy(71, PersonaBeto, 100, aProveedor: false), null,
                CrearPagoDeHoy(71, PersonaAna, 100, aProveedor: false));

            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.NotNull(egreso);
            Assert.Equal(9, egreso.Id); // se edita el mismo egreso, no se crea otro
            Assert.Equal("Cobro a Beto - ID:71 | Efectivo $100,00", egreso.Descripcion);
            Assert.Equal(-100, egreso.Monto);
        }

        [Fact]
        public void EditarSinCambiarPersona_NoGeneraAuditoria()
        {
            var (ctaCte, repo, _) = CrearSut();
            ctaCte.addOrEditPago(CrearPagoDeHoy(72, PersonaAna, 100, aProveedor: false), null, null);

            ctaCte.addOrEditPago(CrearPagoDeHoy(72, PersonaAna, 100, aProveedor: false), null,
                CrearPagoDeHoy(72, PersonaAna, 100, aProveedor: false));

            Assert.Empty(repo.Auditorias);
        }

        [Fact]
        public void EditarPagoEliminado_SeRechaza()
        {
            var (ctaCte, repo, _) = CrearSut();
            var anterior = CrearPagoDeHoy(73, PersonaAna, 100, aProveedor: false);
            anterior.Eliminado = true;

            Assert.Throws<Exception>(() =>
                ctaCte.addOrEditPago(CrearPagoDeHoy(73, PersonaBeto, 100, aProveedor: false), null, anterior));
            Assert.Empty(repo.Movimientos);
        }

        // ---------------- Fecha distinta a la de carga ----------------

        [Fact]
        public void PagoConFechaDistintaALaCarga_NotificaAlAdmin()
        {
            var (ctaCte, repo, _) = CrearSut();
            var pago = CrearPago(80, PersonaAna, 100, aProveedor: false,
                fecha: DateTime.Today.AddDays(-2).AddHours(10), creado: DateTime.Today.AddHours(10));

            ctaCte.addOrEditPago(pago, null, null);

            var notificacion = Assert.Single(repo.Notificaciones);
            Assert.Equal(Notificacion.TipoPagoFechaDistinta, notificacion.Tipo);
            Assert.Equal(Notificacion.SeveridadAdvertencia, notificacion.Severidad);
            Assert.Equal(pago.Id, notificacion.RefId);
        }

        [Fact]
        public void PagoConLaFechaDeHoy_NoNotifica()
        {
            var (ctaCte, repo, _) = CrearSut();

            ctaCte.addOrEditPago(CrearPagoDeHoy(81, PersonaAna, 100, aProveedor: false), null, null);

            Assert.Empty(repo.Notificaciones);
        }

        [Fact]
        public void TieneFechaDistintaALaCarga_SoloParaPagosExistentesConFechaDistinta()
        {
            var distinta = CrearPago(1, PersonaAna, 1, false, new DateTime(2026, 9, 1), new DateTime(2026, 9, 5));
            var igual = CrearPago(1, PersonaAna, 1, false, new DateTime(2026, 9, 5, 8, 0, 0), new DateTime(2026, 9, 5, 20, 0, 0));
            var nuevo = CrearPago(0, PersonaAna, 1, false, new DateTime(2026, 9, 1), new DateTime(2026, 9, 5));

            Assert.True(Negocio.CuentaCorriente.TieneFechaDistintaALaCarga(distinta));
            Assert.False(Negocio.CuentaCorriente.TieneFechaDistintaALaCarga(igual));
            Assert.False(Negocio.CuentaCorriente.TieneFechaDistintaALaCarga(nuevo));
            Assert.False(Negocio.CuentaCorriente.TieneFechaDistintaALaCarga(null));
        }
    }
}
