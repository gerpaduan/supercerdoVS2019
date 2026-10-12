using System;
using System.Linq;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Tests unitarios (sin base de datos) del egreso de caja de un pago/cobro al EDITARLO (2026-10-12, ver
    // docs/DECISIONS.md "Editar un pago/cobro reversa y regenera su egreso de caja"). La caja del vendedor suma TODAS las
    // filas de egresoscaja, asi que una edicion tiene que dejar: reversa del egreso viejo (signo opuesto) + egreso nuevo.
    public class CuentaCorrientePagoCajaEdicionTests
    {
        private static readonly Persona PersonaAna = new Persona { idPersona = 13, razonSocial = "Ana" };
        private static readonly Sucursal SucursalUno = new Sucursal { IdSucursal = 1 };
        private static readonly Sucursal SucursalDos = new Sucursal { IdSucursal = 2 };
        private static readonly Usuario Cajero = new Usuario { Id = 5, Nombre = "Cajero" };
        private static readonly Usuario Admin = new Usuario { Id = 2, Nombre = "Admin" };

        private const int IdPago = 90;
        private static readonly DateTime FechaPago = DateTime.Today.AddHours(10);

        private static (Negocio.CuentaCorriente ctaCte, FakeCierreCajaRepository cierreRepo) CrearSut()
        {
            var repo = new FakeCuentaCorrienteRepository();
            var cierreRepo = new FakeCierreCajaRepository();
            var empresa = new EmpresaContextFake(1);
            var ctaCte = new Negocio.CuentaCorriente(repo, empresa)
            {
                CierreCajaFactory = () => new Negocio.CierreCaja(cierreRepo, empresa)
            };
            return (ctaCte, cierreRepo);
        }

        private static Pago CrearPago(float importe, bool aProveedor, string formaPago = "Efectivo", float efectivo = 0,
            DateTime? fecha = null, Sucursal sucursal = null, string observaciones = "") => new Pago
        {
            Id = IdPago,
            Persona = PersonaAna,
            Fecha = fecha ?? FechaPago,
            NroRecibo = "TEST-090",
            FormaPago = formaPago,
            AProveedor = aProveedor,
            Importe = importe,
            Efectivo = efectivo,
            Sucursal = sucursal ?? SucursalUno,
            Observaciones = observaciones,
            Creado = FechaPago,
            CreadoPor = Cajero,
            ActualizadoPor = Admin,
        };

        private static Entidades.CierreCaja CajaAbierta() => new Entidades.CierreCaja
        {
            Id = 77,
            UsuarioInicio = Cajero,
            Sucursal = SucursalUno
        };

        // Egreso tal como lo deja el POS al crear un cobro en efectivo de $100 (a favor de la caja: monto negativo).
        private static EgresoCaja EgresoDeCobroEfectivo100() => new EgresoCaja
        {
            Id = 40,
            Fecha = FechaPago,
            IdTipoEgresoCaja = EgresoCaja.idPagoCobroEgresoCaja,
            Descripcion = "Cobro a Ana - ID:" + IdPago + " | Efectivo $" + (100f).ToString("N2"),
            Detalle = "",
            ConteoBilletes = "100x1",
            Monto = -100,
            Sucursal = SucursalUno,
            Tabla = "Pagos",
            IdTabla = IdPago,
            CreadoPor = Cajero.Id
        };

        // Guarda el pago original (alta desde POS) y deja el egreso vigente en el fake, listo para editar.
        private static (Negocio.CuentaCorriente ctaCte, FakeCierreCajaRepository cierreRepo) ConPagoYaGuardado(EgresoCaja egresoVigente)
        {
            var (ctaCte, cierreRepo) = CrearSut();
            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), CajaAbierta(), null);
            cierreRepo.EgresosRecibidos.Clear();
            cierreRepo.EgresoCajaExistente = egresoVigente;
            return (ctaCte, cierreRepo);
        }

        // Suma de la caja = suma de todas las filas (asi lo hace getMontoEgresosCajaVendedor, sin flag de anulado).
        // Una fila con Id != 0 es un UPDATE del egreso vigente, no una fila nueva: reemplaza su monto original.
        private static float SumaCaja(EgresoCaja original, FakeCierreCajaRepository cierreRepo)
        {
            float suma = original.Monto;
            foreach (var egreso in cierreRepo.EgresosRecibidos)
            {
                if (egreso.Id == original.Id) suma = suma - original.Monto + egreso.Monto;
                else suma += egreso.Monto;
            }
            return suma;
        }

        [Fact]
        public void Alta_DesdePos_InsertaElEgreso()
        {
            var (ctaCte, cierreRepo) = CrearSut();

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), CajaAbierta(), null);

            var egreso = Assert.Single(cierreRepo.EgresosRecibidos);
            Assert.Equal(0, egreso.Id);
            Assert.Equal(-100, egreso.Monto);
            Assert.Equal(Cajero.Id, egreso.CreadoPor);
            Assert.Equal("Pagos", egreso.Tabla);
            Assert.Equal(IdPago, egreso.IdTabla);
        }

        [Fact]
        public void EditarImporte_DesdePos_GeneraReversaYEgresoNuevo()
        {
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(150, aProveedor: false), CajaAbierta(), CrearPago(100, aProveedor: false));

            Assert.Equal(2, cierreRepo.EgresosRecibidos.Count);
            var reversa = cierreRepo.EgresosRecibidos[0];
            var nuevo = cierreRepo.EgresosRecibidos[1];
            Assert.Equal(0, reversa.Id);
            Assert.Equal(100, reversa.Monto);
            Assert.StartsWith("ANULACION: Cobro a Ana", reversa.Descripcion);
            Assert.Equal(original.Fecha, reversa.Fecha);
            Assert.Equal("100x1", reversa.ConteoBilletes); // la reversa conserva el conteo del original
            Assert.Equal(0, nuevo.Id);
            Assert.Equal(-150, nuevo.Monto);
            Assert.Equal(-150, SumaCaja(original, cierreRepo)); // -100 + 100 - 150: la caja refleja solo el valor nuevo
        }

        // Caso reportado: la cuenta corriente abre el pago SIN desdePos, o sea sin caja POS (oCierreCajaE == null).
        // El egreso existente es lo que manda: tiene que reversarse igual.
        [Fact]
        public void EditarImporte_SinCajaPos_PeroConEgresoPrevio_TambienReversa()
        {
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(150, aProveedor: false), null, CrearPago(100, aProveedor: false));

            Assert.Equal(2, cierreRepo.EgresosRecibidos.Count);
            Assert.Equal(100, cierreRepo.EgresosRecibidos[0].Monto);
            Assert.Equal(-150, cierreRepo.EgresosRecibidos[1].Monto);
            // Reversa y nuevo caen en la caja del egreso original (mismo dueño), no en la de quien edita.
            Assert.All(cierreRepo.EgresosRecibidos, e => Assert.Equal(Cajero.Id, e.CreadoPor));
        }

        [Fact]
        public void EditarFormaDePago_ConMismoImporte_ReversaElEfectivoViejo()
        {
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false, formaPago: "Transferencia"), null, CrearPago(100, aProveedor: false));

            Assert.Equal(2, cierreRepo.EgresosRecibidos.Count);
            Assert.Equal(100, cierreRepo.EgresosRecibidos[0].Monto);
            Assert.Equal(0, cierreRepo.EgresosRecibidos[1].Monto); // transferencia no mueve efectivo
            Assert.Equal(0, SumaCaja(original, cierreRepo));
        }

        [Fact]
        public void EditarEfectivoDeEftvoCheque_ConMismoImporteTotal_NoCuentaElEfectivoDosVeces()
        {
            // Cobro EftvoCheque: total 300, efectivo 100 -> la caja registra solo el efectivo.
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(300, aProveedor: false, formaPago: "EftvoCheque", efectivo: 150), null,
                CrearPago(300, aProveedor: false, formaPago: "EftvoCheque", efectivo: 100));

            Assert.Equal(-150, SumaCaja(original, cierreRepo));
        }

        [Fact]
        public void PagoACobroConMismoImporte_NoPisaLaReversa()
        {
            // Pago de $100 (sale de la caja: +100) editado a Cobro de $100 (entra: -100).
            var original = EgresoDeCobroEfectivo100();
            original.Monto = 100;
            original.Descripcion = "Pago a Ana - ID:" + IdPago + " | Efectivo $" + (100f).ToString("N2");
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), null, CrearPago(100, aProveedor: true));

            Assert.Equal(2, cierreRepo.EgresosRecibidos.Count);
            Assert.All(cierreRepo.EgresosRecibidos, e => Assert.Equal(0, e.Id)); // ninguna es UPDATE de la fila vieja
            Assert.Equal(-100, cierreRepo.EgresosRecibidos[0].Monto);
            Assert.Equal(-100, cierreRepo.EgresosRecibidos[1].Monto);
            Assert.Equal(-100, SumaCaja(original, cierreRepo));
        }

        [Fact]
        public void EditarSoloLaFecha_ReversaEnLaFechaOriginalYCreaEnLaNueva()
        {
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);
            var fechaNueva = FechaPago.AddDays(-1);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false, fecha: fechaNueva), null, CrearPago(100, aProveedor: false));

            Assert.Equal(2, cierreRepo.EgresosRecibidos.Count);
            Assert.Equal(FechaPago, cierreRepo.EgresosRecibidos[0].Fecha);
            Assert.Equal(100, cierreRepo.EgresosRecibidos[0].Monto);
            Assert.Equal(fechaNueva, cierreRepo.EgresosRecibidos[1].Fecha);
            Assert.Equal(-100, cierreRepo.EgresosRecibidos[1].Monto);
        }

        [Fact]
        public void EditarSoloLaSucursal_ReversaEnLaOriginalYCreaEnLaNueva()
        {
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false, sucursal: SucursalDos), null, CrearPago(100, aProveedor: false));

            Assert.Equal(2, cierreRepo.EgresosRecibidos.Count);
            Assert.Equal(SucursalUno.idSucursal, cierreRepo.EgresosRecibidos[0].Sucursal.idSucursal);
            Assert.Equal(SucursalDos.idSucursal, cierreRepo.EgresosRecibidos[1].Sucursal.idSucursal);
        }

        [Fact]
        public void EditarSoloObservaciones_ActualizaElMismoEgresoSinReversa()
        {
            var original = EgresoDeCobroEfectivo100();
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false, observaciones: "nota nueva"), null, CrearPago(100, aProveedor: false));

            var egreso = Assert.Single(cierreRepo.EgresosRecibidos);
            Assert.Equal(40, egreso.Id); // UPDATE del mismo registro
            Assert.Equal(-100, egreso.Monto);
            Assert.Equal("nota nueva", egreso.Detalle);
        }

        [Fact]
        public void EditarPagoDeFinanzas_SinEgresoNiCajaPos_NoMueveCaja()
        {
            var (ctaCte, cierreRepo) = CrearSut();
            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), null, null); // alta desde Finanzas: sin egreso
            cierreRepo.EgresosRecibidos.Clear();

            ctaCte.addOrEditPago(CrearPago(150, aProveedor: false), null, CrearPago(100, aProveedor: false));

            Assert.Empty(cierreRepo.EgresosRecibidos);
        }

        [Fact]
        public void EditarPagoDeFinanzas_DesdePos_SinEgresoPrevio_InsertaSinReversaNiExcepcion()
        {
            var (ctaCte, cierreRepo) = CrearSut();
            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), null, null);
            cierreRepo.EgresosRecibidos.Clear();

            ctaCte.addOrEditPago(CrearPago(150, aProveedor: false), CajaAbierta(), CrearPago(100, aProveedor: false));

            var egreso = Assert.Single(cierreRepo.EgresosRecibidos);
            Assert.Equal(0, egreso.Id);
            Assert.Equal(-150, egreso.Monto);
        }

        [Fact]
        public void EgresoVigenteEnCero_SeEditaAMonto_NoGeneraReversaDeCero()
        {
            var original = EgresoDeCobroEfectivo100();
            original.Monto = 0;
            original.Descripcion = "Cobro a Ana - ID:" + IdPago + " | Transferencia $100,00";
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(original);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), null, CrearPago(100, aProveedor: false, formaPago: "Transferencia"));

            var egreso = Assert.Single(cierreRepo.EgresosRecibidos);
            Assert.Equal(0, egreso.Id);
            Assert.Equal(-100, egreso.Monto);
        }

        [Fact]
        public void UltimoEgresoEsUnaReversa_NoSeReversaDeNuevoNiSePisa()
        {
            // Dato viejo (bug previo): quedo una ANULACION como ultimo egreso. No es el vigente: solo se inserta el nuevo.
            var reversaHuerfana = EgresoDeCobroEfectivo100();
            reversaHuerfana.Id = 41;
            reversaHuerfana.Monto = 100;
            reversaHuerfana.Descripcion = "ANULACION: Cobro a Ana - ID:" + IdPago;
            var (ctaCte, cierreRepo) = ConPagoYaGuardado(reversaHuerfana);

            ctaCte.addOrEditPago(CrearPago(100, aProveedor: false), null, CrearPago(100, aProveedor: false));

            var egreso = Assert.Single(cierreRepo.EgresosRecibidos);
            Assert.Equal(0, egreso.Id);
            Assert.Equal(-100, egreso.Monto);
        }
    }
}
