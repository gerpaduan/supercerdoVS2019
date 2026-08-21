using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre el bloque "esEgresoCaja" de Negocio.Compra.AddOrEditCompra -- mismo espiritu que
    // VentaEgresoCajaPagoTarjetaTests, para el caso equivalente en Compra: cuando la compra es
    // CTA CTE no sale plata real de la caja (Monto=0), pero igual se deja un registro
    // informativo con el importe real en la descripcion.
    public class CompraEgresoCajaTests
    {
        private static Compra CrearCompra(bool enCtaCte) => new Compra
        {
            IdCompra = 0,
            TipoCompra = Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
            EnCtaCte = enCtaCte,
            LineasMediasReses = new List<MediaRes>(),
            LineasCortes = new List<CortePorCompra>
            {
                new CortePorCompra { CantKgs = 10, precioKg = 15, Corte = new Corte(), Sucursal = new Sucursal { IdSucursal = 1 }, ActualizarPrecioVenta = false },
            },
            Proveedor = new Persona { idPersona = 18, razonSocial = "Proveedor Test" },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaCompra = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            CreadoPor = new Usuario { Id = 2 },
            NroRemito = "R-001",
            Observaciones = "",
        };

        private static (Negocio.Compra compra, FakeCierreCajaRepository cierreRepo) CrearCompraN()
        {
            var cierreRepo = new FakeCierreCajaRepository();
            var compra = new Negocio.Compra(
                new FakeCompraRepository(),
                new EmpresaContextFake(1),
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)),
                cierreCajaN: new Negocio.CierreCaja(cierreRepo, new EmpresaContextFake(1)));
            return (compra, cierreRepo);
        }

        [Fact]
        public void EsEgresoCajaFalse_NoGeneraEgreso()
        {
            var (compra, cierreRepo) = CrearCompraN();
            var oCompraE = CrearCompra(enCtaCte: false);

            compra.AddOrEditCompra(oCompraE, Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
                new List<MediaRes>(), oCompraE.LineasCortes, esEgresoCaja: false, oEgresoCajaE: null);

            Assert.False(cierreRepo.AddOrEditEgresoCajaFueLlamado);
        }

        [Fact]
        public void CompraNormal_MontoEsElImporteCompleto()
        {
            var (compra, cierreRepo) = CrearCompraN();
            var oCompraE = CrearCompra(enCtaCte: false);

            // 1 linea: 10 Kg * $15 = $150.
            compra.AddOrEditCompra(oCompraE, Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
                new List<MediaRes>(), oCompraE.LineasCortes, esEgresoCaja: true, oEgresoCajaE: null);

            Assert.True(cierreRepo.AddOrEditEgresoCajaFueLlamado);
            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.Equal(EgresoCaja.idCompraEgresoCaja, egreso.IdTipoEgresoCaja);
            Assert.Equal(150, egreso.Monto);
            Assert.StartsWith("Compra a ", egreso.Descripcion);
        }

        [Fact]
        public void CompraCtaCte_MontoQuedaEnCeroPeroElImporteRealQuedaEnLaDescripcion()
        {
            var (compra, cierreRepo) = CrearCompraN();
            var oCompraE = CrearCompra(enCtaCte: true);

            compra.AddOrEditCompra(oCompraE, Compra.tipoCompraToString(Compra.tipoCompraEnum.Cortes),
                new List<MediaRes>(), oCompraE.LineasCortes, esEgresoCaja: true, oEgresoCajaE: null);

            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.Equal(0, egreso.Monto);
            Assert.StartsWith("Compra CTA CTE a ", egreso.Descripcion);
            Assert.Contains("$150", egreso.Descripcion);
        }
    }
}
