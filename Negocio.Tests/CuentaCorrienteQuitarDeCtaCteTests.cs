using System;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre la rama de "sacar de cta cte" de Negocio.CuentaCorriente.crearMovCtaCte (parametro
    // crearMovCtaCte=false), usada por Venta.crearMovCtaCteVenta (via oVentaE.EnCtaCte) y por
    // Compra.crearMovCtaCteCompra de la misma forma. Distinta de la rama que cubre
    // CuentaCorrienteAnulacionTests (cambio de tipo/importe): esta rama es "misma venta/compra,
    // se le saca la cta cte" -- deja el registro original intacto y crea un opuesto con
    // QuitadoCtaCta=true, sin insertar un registro nuevo "activo" despues (a diferencia del
    // camino de Pagos, que si crea uno nuevo tras la anulacion).
    //
    // Replica, con asserts, el resultado verificado a mano (HTTP + SQL directo, ambos motores)
    // para Compra el 2026-08-20 (ver docs/DECISIONS.md): "registro original intacto, mas un
    // segundo registro real y opuesto (Tipo=Debito, Importe=-160, QuitadoCtaCta=true,
    // Detalle='Quitado de Cta.Cte.')". Se prueba con tabla=Ventas (el caller real de
    // Negocio.Venta) porque la logica de CuentaCorriente.crearMovCtaCte no distingue por tabla.
    public class CuentaCorrienteQuitarDeCtaCteTests
    {
        private static readonly Persona PersonaTest = new Persona { idPersona = 13 };
        private static readonly Sucursal SucursalTest = new Sucursal { IdSucursal = 1 };
        private static readonly Usuario UsuarioTest = new Usuario { Id = 2 };

        private static (Negocio.CuentaCorriente ctaCte, FakeCuentaCorrienteRepository repo) CrearSut()
        {
            var repo = new FakeCuentaCorrienteRepository();
            var ctaCte = new Negocio.CuentaCorriente(repo, new EmpresaContextFake(1));
            return (ctaCte, repo);
        }

        // Mismos args posicionales que Negocio.Venta.crearMovCtaCteVenta pasa a
        // ctaCteN.crearMovCtaCte: tabla=Ventas, detalle="", tipoMov=Debito siempre (una venta
        // debita la cuenta del cliente), oCierreCajaE/oPagoE/oPagoAnterior=null (fuera de
        // alcance de este test, ver CargarEgresoCajaPorPago).
        private static void CrearOEditarVenta(Negocio.CuentaCorriente ctaCte, int idVenta, float importe, bool enCtaCte) =>
            ctaCte.crearMovCtaCte(PersonaTest, new DateTime(2026, 8, 20, 10, 0, 0), MovCtaCte.tablas.Ventas, idVenta,
                nroDoc: "R-001", detalle: "", tipoMov: MovCtaCte.tipoMov.Debito, importe: importe, oSucursalE: SucursalTest,
                creado: new DateTime(2026, 8, 20, 10, 0, 0), creadoPor: UsuarioTest, actualizado: null, actualizadoPor: null,
                crearMovCtaCte: enCtaCte, oCierreCajaE: null, oPagoE: null, oPagoAnterior: null);

        [Fact]
        public void SacarVentaDeCtaCte_DejaElOriginalIntactoYCreaUnOpuesto()
        {
            var (ctaCte, repo) = CrearSut();
            CrearOEditarVenta(ctaCte, idVenta: 100, importe: 160, enCtaCte: true);

            CrearOEditarVenta(ctaCte, idVenta: 100, importe: 160, enCtaCte: false);

            Assert.Equal(2, repo.Movimientos.Count);

            var original = repo.Movimientos[0];
            Assert.Equal(MovCtaCte.tipoMov.Debito.ToString(), original.Tipo);
            Assert.Equal(-160, original.Importe);
            Assert.False(original.QuitadoCtaCta);

            var opuesto = repo.Movimientos[1];
            Assert.Equal(MovCtaCte.tipoMov.Credito.ToString(), opuesto.Tipo);
            Assert.Equal(160, opuesto.Importe);
            Assert.True(opuesto.QuitadoCtaCta);
            Assert.Equal("Quitado de Cta.Cte.", opuesto.Detalle);
        }

        [Fact]
        public void VolverAPonerEnCtaCte_TrasHaberlaSacado_CreaUnNuevoRegistroActivo()
        {
            var (ctaCte, repo) = CrearSut();
            CrearOEditarVenta(ctaCte, idVenta: 100, importe: 160, enCtaCte: true);
            CrearOEditarVenta(ctaCte, idVenta: 100, importe: 160, enCtaCte: false);

            // El ultimo registro (QuitadoCtaCta=true) hace que crearMovCtaCte trate la venta
            // como si no tuviera cta cte activa -- vuelve a crear un registro nuevo, no
            // reutiliza ninguno de los 2 anteriores.
            CrearOEditarVenta(ctaCte, idVenta: 100, importe: 160, enCtaCte: true);

            Assert.Equal(3, repo.Movimientos.Count);
            var nuevo = repo.Movimientos[2];
            Assert.Equal(MovCtaCte.tipoMov.Debito.ToString(), nuevo.Tipo);
            Assert.Equal(-160, nuevo.Importe);
            Assert.False(nuevo.QuitadoCtaCta);
        }
    }
}
