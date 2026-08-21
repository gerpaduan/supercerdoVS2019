using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre el loop de procesamiento de lineas dentro de Negocio.Venta.EjecutarAgregarVenta,
    // 3 ramas distintas segun esNotaCredito/Estado:
    //  - Nota de credito: CantKg/KgsTotalCalculado se invierten de signo (linea negativa).
    //  - Linea normal (no anulada): IndexAnulado se pisa con getIdEstado(NoAnulado)=0, sin
    //    importar que traiga el objeto de entrada.
    //  - Linea anulada: IndexAnulado (que llega apuntando al INDICE dentro de LineasVenta de la
    //    linea original) se reemplaza por el IdLineaVenta real de esa linea original -- un
    //    lookup dentro de la misma lista, antes de persistir.
    public class VentaLineaAnuladaTests
    {
        private static Venta CrearVentaBase(List<LineaVenta> lineas) => new Venta
        {
            IdVenta = 0,
            FormaPago = Venta.FormaPagoEnum.Efectivo.ToString(),
            EnCtaCte = true,
            LineasVenta = lineas,
            ListaExpendios = null,
            Vendedor = new Usuario { Id = 2 },
            Persona = new Persona { idPersona = 13 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaVenta = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            NroRemito = "R-001",
        };

        private static Negocio.Venta CrearVentaN() =>
            new Negocio.Venta(
                new FakeVentaRepository(),
                new EmpresaContextFake(1),
                new FakeParametrosContext(),
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)),
                cierreCajaN: new Negocio.CierreCaja(new FakeCierreCajaRepository(), new EmpresaContextFake(1)));

        [Fact]
        public void NotaDeCredito_InvierteElSignoDeLaCantidad()
        {
            var linea = new LineaVenta { KgsTotalCalculado = 10, CantKg = 10, PrecioKg = 20, Corte = new Corte() };
            var venta = CrearVentaBase(new List<LineaVenta> { linea });

            CrearVentaN().agregarVenta(venta, esNotaCredito: true);

            Assert.Equal(-10, linea.CantKg);
            Assert.Equal(-10, linea.KgsTotalCalculado);
        }

        [Fact]
        public void LineaNoAnulada_IndexAnuladoQuedaEnElEstadoNoAnulado()
        {
            // IndexAnulado arranca en un valor cualquiera (no deberia importar): la rama
            // "no anulada" siempre lo pisa con getIdEstado(NoAnulado), nunca lo deja como venia.
            var linea = new LineaVenta { KgsTotalCalculado = 10, PrecioKg = 20, Corte = new Corte(), Estado = LineaVenta.getIdEstado(LineaVenta.estados.NoAnulado), IndexAnulado = 99 };
            var venta = CrearVentaBase(new List<LineaVenta> { linea });

            CrearVentaN().agregarVenta(venta, esNotaCredito: false);

            Assert.Equal(10, linea.CantKg);
            Assert.Equal(LineaVenta.getIdEstado(LineaVenta.estados.NoAnulado), linea.IndexAnulado);
        }

        [Fact]
        public void LineaAnulada_IndexAnuladoPasaDeIndiceEnLaListaAIdLineaVentaReal()
        {
            var original = new LineaVenta { IdLineaVenta = 501, KgsTotalCalculado = 5, PrecioKg = 20, Corte = new Corte(), Estado = LineaVenta.getIdEstado(LineaVenta.estados.NoAnulado) };
            // IndexAnulado=0 en la linea de anulacion apunta al INDICE 0 de LineasVenta (la
            // linea "original" de arriba), no a su IdLineaVenta -- asi la recibe el metodo.
            var anulacion = new LineaVenta { KgsTotalCalculado = -5, PrecioKg = 20, Corte = new Corte(), Estado = LineaVenta.getIdEstado(LineaVenta.estados.Anulado), IndexAnulado = 0 };
            var venta = CrearVentaBase(new List<LineaVenta> { original, anulacion });

            CrearVentaN().agregarVenta(venta, esNotaCredito: false);

            Assert.Equal(501, anulacion.IndexAnulado);
        }
    }
}
