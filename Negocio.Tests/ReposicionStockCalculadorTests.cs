using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre las reglas de "Qué pedir" (Negocio.ReposicionStockCalculador, docs/DECISIONS.md 2026-10-13): cuándo un producto
    // aparece, cuánto se sugiere pedir, y cuándo un "en cero" se considera ruido por falta de movimiento reciente.
    public class ReposicionStockCalculadorTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 10, 13, 10, 0, 0);

        private static ExistenciaStockPorSucursalPlanoVm Existencia(
            int idCorte, int idSucursal, float stock, float punto, bool pesable = true, string nombre = null, string sucursal = null, DateTime? cierre = null)
        {
            return new ExistenciaStockPorSucursalPlanoVm
            {
                IdCorte = idCorte,
                Codigo = idCorte,
                Corte = nombre ?? ("Producto " + idCorte),
                IdSucursal = idSucursal,
                Sucursal = sucursal ?? ("Sucursal " + idSucursal),
                StockActual = stock,
                PuntoStock = punto,
                Pesable = pesable,
                FechaUltimoCierre = cierre ?? Ahora.AddDays(-5)
            };
        }

        private static ActividadStockVm Actividad(int idCorte, int idSucursal, float ventas = 0f, float elaboracion = 0f, int diasUltimaVenta = 3)
        {
            return new ActividadStockVm
            {
                IdCorte = idCorte,
                IdSucursal = idSucursal,
                VentasVentana = ventas,
                ElaboracionVentana = elaboracion,
                UltimaVenta = ventas > 0f ? (DateTime?)Ahora.AddDays(-diasUltimaVenta) : null,
                UltimaElaboracion = elaboracion > 0f ? (DateTime?)Ahora.AddDays(-diasUltimaVenta) : null
            };
        }

        private static ReposicionFiltroVm Filtro(int diasVentana = 30, int cobertura = 0, bool mostrarSinMovimiento = false, params int[] sucursales)
        {
            return ReposicionStockCalculador.Normalizar(new ReposicionFiltroVm
            {
                DiasVentana = diasVentana,
                DiasCoberturaSinPunto = cobertura,
                MostrarSinMovimiento = mostrarSinMovimiento,
                IdsSucursales = sucursales.ToList()
            });
        }

        private static ReposicionVm Calcular(
            IList<ExistenciaStockPorSucursalPlanoVm> plano,
            IList<ActividadStockVm> actividad,
            ReposicionFiltroVm filtro,
            IList<ProveedorHabitualVm> proveedores = null)
        {
            return ReposicionStockCalculador.Calcular(plano, actividad, proveedores ?? new List<ProveedorHabitualVm>(), filtro, Ahora);
        }

        // ---- Bajo el punto de stock ----

        [Fact]
        public void BajoElPunto_SugierePedirLaDiferenciaHastaElPunto()
        {
            var r = Calcular(
                new[] { Existencia(1, 1, stock: 5f, punto: 20f) },
                new[] { Actividad(1, 1, ventas: 30f) },
                Filtro());

            var fila = Assert.Single(r.Filas);
            Assert.Equal(MotivoReposicion.BajoPuntoStock, fila.Motivo);
            Assert.Equal(15f, fila.Pedir);
            Assert.Equal(1, r.CantBajoPuntoStock);
        }

        [Fact]
        public void StockIgualAlPunto_NoHayNadaQuePedir()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: 20f, punto: 20f) }, new ActividadStockVm[0], Filtro());

            Assert.Empty(r.Filas);
            Assert.Equal(0, r.CantBajoPuntoStock);
        }

        [Fact]
        public void StockSobreElPunto_NoAparece()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: 35f, punto: 20f) }, new ActividadStockVm[0], Filtro());

            Assert.Empty(r.Filas);
        }

        [Fact]
        public void StockNegativoConPunto_CuentaCeroParaPedirYSeMarcaParaRevisar()
        {
            // Caso real (Carre, San Lorenzo): stock -38, punto 18. Se pide el punto completo, no 56.
            var r = Calcular(new[] { Existencia(1, 1, stock: -38.105f, punto: 18f) }, new[] { Actividad(1, 1, ventas: 197f) }, Filtro());

            var fila = Assert.Single(r.Filas);
            Assert.Equal(MotivoReposicion.BajoPuntoStock, fila.Motivo);
            Assert.True(fila.StockNegativo);
            Assert.Equal(18f, fila.Pedir);
        }

        [Fact]
        public void PedidoDePesable_SeRedondeaHaciaArribaA0Coma1()
        {
            // 20 - 5,734 = 14,266 -> 14,3
            var r = Calcular(new[] { Existencia(1, 1, stock: 5.734f, punto: 20f, pesable: true) }, new ActividadStockVm[0], Filtro());

            Assert.Equal(14.3f, Assert.Single(r.Filas).Pedir.Value, 3);
        }

        [Fact]
        public void PedidoPorUnidad_SeRedondeaHaciaArribaAlEntero()
        {
            // 20 - 12,777 = 7,223 -> 8 unidades
            var r = Calcular(new[] { Existencia(1, 1, stock: 12.777f, punto: 20f, pesable: false) }, new ActividadStockVm[0], Filtro());

            Assert.Equal(8f, Assert.Single(r.Filas).Pedir);
        }

        [Fact]
        public void PuntoConStockSobrante_DiceQueElPuntoParecaAlto()
        {
            // Caso real (Bracolor, San Martín): stock 5,7 con punto 7 pero consume 0,01/día -> alcanza ~800 días.
            var r = Calcular(new[] { Existencia(1, 1, stock: 5.734f, punto: 7f) }, new[] { Actividad(1, 1, ventas: 0.3f) }, Filtro());

            var fila = Assert.Single(r.Filas);
            Assert.True(fila.PuntoPareceAlto);
            Assert.True(fila.CoberturaDias.Value > ReposicionStockCalculador.DiasCoberturaPuntoAlto);
        }

        // ---- En cero y la ventana de actividad ----

        [Fact]
        public void EnCeroSinPuntoConVentasRecientes_SeAvisaSinCantidad()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 18.7f, diasUltimaVenta: 8) }, Filtro());

            var fila = Assert.Single(r.Filas);
            Assert.Equal(MotivoReposicion.EnCeroConMovimiento, fila.Motivo);
            Assert.Null(fila.Pedir);
            Assert.Equal(8, fila.DiasSinSalida);
            Assert.Equal(1, r.CantEnCeroConMovimiento);
        }

        [Fact]
        public void EnCeroSinSalidasEnLaVentana_NoSeAvisaPeroSeCuenta()
        {
            // Caso real: Codeguín en cero hace más de un año. Antes figuraba como "SIN STOCK" igual que Matambre.
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new ActividadStockVm[0], Filtro());

            Assert.Empty(r.Filas);
            Assert.Equal(1, r.CantEnCeroSinMovimiento);
            Assert.Equal(0, r.CantEnCeroConMovimiento);
        }

        [Fact]
        public void EnCeroSinMovimiento_SeListaSoloSiSePide()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new ActividadStockVm[0], Filtro(mostrarSinMovimiento: true));

            var fila = Assert.Single(r.Filas);
            Assert.Equal(MotivoReposicion.EnCeroSinMovimiento, fila.Motivo);
        }

        [Fact]
        public void InsumoQueSoloSeConsumeEnElaboracion_CuentaComoConMovimiento()
        {
            // Sal, madejas, condimentos: no se venden, se usan en elaborados. Sin esto parecerían muertos.
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 0f, elaboracion: 2f) }, Filtro());

            Assert.Equal(MotivoReposicion.EnCeroConMovimiento, Assert.Single(r.Filas).Motivo);
        }

        [Fact]
        public void DevolucionesQueDejanElNetoNegativo_NoCuentanComoMovimiento()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { Actividad(1, 1, ventas: -3f) }, Filtro());

            Assert.Empty(r.Filas);
            Assert.Equal(1, r.CantEnCeroSinMovimiento);
        }

        [Fact]
        public void SinPuntoYConStockPositivo_NoSeDiceNada()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: 3f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 90f) }, Filtro());

            Assert.Empty(r.Filas);
        }

        [Fact]
        public void UnPuntoCargadoSeRespetaAunqueNoHayaMovimiento()
        {
            // El punto es una decisión explícita del usuario: se lista igual, y la columna de actividad deja ver que no se mueve.
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 20f) }, new ActividadStockVm[0], Filtro());

            var fila = Assert.Single(r.Filas);
            Assert.Equal(MotivoReposicion.BajoPuntoStock, fila.Motivo);
            Assert.Null(fila.DiasSinSalida);
            Assert.Null(fila.CoberturaDias);
        }

        // ---- Sugerencia por días de venta (productos sin punto) ----

        [Fact]
        public void SinPuntoConDiasDeCobertura_SugiereConsumoDiarioPorLosDias()
        {
            // 60 kg en 30 días = 2 kg/día; para 7 días = 14 kg.
            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 60f) }, Filtro(cobertura: 7));

            Assert.Equal(14f, Assert.Single(r.Filas).Pedir);
        }

        [Fact]
        public void SinPuntoConDiasDeCobertura_ElStockNegativoNoSeSumaAlPedido()
        {
            var r = Calcular(new[] { Existencia(1, 1, stock: -10f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 60f) }, Filtro(cobertura: 7));

            Assert.Equal(14f, Assert.Single(r.Filas).Pedir);
        }

        // ---- Sucursales, otras sucursales ----

        [Fact]
        public void SoloSeMuestranLasSucursalesTildadas()
        {
            var plano = new[]
            {
                Existencia(1, 1, stock: 2f, punto: 10f),
                Existencia(1, 2, stock: 3f, punto: 10f)
            };

            var r = Calcular(plano, new ActividadStockVm[0], Filtro(sucursales: 2));

            Assert.Equal(2, Assert.Single(r.Filas).IdSucursal);
            Assert.Equal(1, r.CantProductosControlados);
        }

        [Fact]
        public void OtrasSucursalesConStock_SeInformanParaResolverConUnMovimiento()
        {
            // Caso real: Costilla en cero en San Lorenzo, 162 kg en San Martín.
            var plano = new[]
            {
                Existencia(1, 1, stock: 0f, punto: 0f, sucursal: "San Lorenzo"),
                Existencia(1, 2, stock: 162.3f, punto: 0f, sucursal: "San Martín")
            };

            var r = Calcular(plano, new[] { Actividad(1, 1, ventas: 60f) }, Filtro(sucursales: 1));

            var fila = Assert.Single(r.Filas);
            var otra = Assert.Single(fila.OtrasSucursales);
            Assert.Equal("San Martín", otra.Sucursal);
            Assert.Equal(162.3f, fila.StockOtrasSucursales, 3);
        }

        [Fact]
        public void OtrasSucursalesSinStockOEnNegativo_NoSeOfrecen()
        {
            var plano = new[]
            {
                Existencia(1, 1, stock: 0f, punto: 0f),
                Existencia(1, 2, stock: -4f, punto: 0f)
            };

            var r = Calcular(plano, new[] { Actividad(1, 1, ventas: 10f) }, Filtro(sucursales: 1));

            Assert.Empty(Assert.Single(r.Filas).OtrasSucursales);
        }

        // ---- Proveedor, importe y orden ----

        [Fact]
        public void ConProveedorYPrecio_CalculaElImporteEstimado()
        {
            var proveedores = new[]
            {
                new ProveedorHabitualVm { IdCorte = 1, IdProveedor = 9, Proveedor = "Cingolani", UltimoPrecio = 12700m, FechaUltimaCompra = Ahora.AddDays(-20) }
            };

            var r = Calcular(new[] { Existencia(1, 1, stock: 0f, punto: 7f) }, new ActividadStockVm[0], Filtro(), proveedores);

            var fila = Assert.Single(r.Filas);
            Assert.Equal("Cingolani", fila.Proveedor);
            Assert.Equal(88900m, fila.ImporteEstimado);
            Assert.Equal(88900m, r.ImporteEstimadoTotal);
        }

        [Fact]
        public void SinProveedorQuedaAlFinal_YDentroDeCadaProveedorPrimeroLosEnCero()
        {
            var plano = new[]
            {
                Existencia(1, 1, stock: 4f, punto: 10f, nombre: "Zeta"),
                Existencia(2, 1, stock: 0f, punto: 10f, nombre: "Alfa"),
                Existencia(3, 1, stock: 1f, punto: 10f, nombre: "Sin prov")
            };
            var proveedores = new[]
            {
                new ProveedorHabitualVm { IdCorte = 1, IdProveedor = 5, Proveedor = "Beta SA" },
                new ProveedorHabitualVm { IdCorte = 2, IdProveedor = 5, Proveedor = "Beta SA" }
            };

            var r = Calcular(plano, new ActividadStockVm[0], Filtro(), proveedores);

            Assert.Equal(new[] { "Alfa", "Zeta", "Sin prov" }, r.Filas.Select(x => x.Producto).ToArray());
        }

        // ---- Avisos de cierre, normalización ----

        [Fact]
        public void CierreDeStockViejo_SeAvisaPorSucursal()
        {
            var plano = new[]
            {
                Existencia(1, 1, stock: 0f, punto: 5f, sucursal: "San Lorenzo", cierre: Ahora.AddDays(-15)),
                Existencia(1, 2, stock: 0f, punto: 5f, sucursal: "San Martín", cierre: Ahora.AddDays(-45))
            };

            var r = Calcular(plano, new ActividadStockVm[0], Filtro());

            Assert.False(r.AvisosCierre.Single(x => x.Sucursal == "San Lorenzo").Viejo);
            var viejo = r.AvisosCierre.Single(x => x.Sucursal == "San Martín");
            Assert.True(viejo.Viejo);
            Assert.Equal(45, viejo.DiasDesdeCierre);
        }

        [Fact]
        public void SinNingunCierre_SeMarcaComoViejoSinFecha()
        {
            var plano = new[] { Existencia(1, 1, stock: 0f, punto: 5f, cierre: new DateTime(1900, 1, 1)) };

            var aviso = Assert.Single(Calcular(plano, new ActividadStockVm[0], Filtro()).AvisosCierre);

            Assert.True(aviso.Viejo);
            Assert.Null(aviso.FechaUltimoCierre);
            Assert.Null(aviso.DiasDesdeCierre);
        }

        [Theory]
        [InlineData(0, 30)]
        [InlineData(-5, 30)]
        [InlineData(3, 7)]
        [InlineData(45, 45)]
        [InlineData(9999, 180)]
        public void Normalizar_AcotaLosDiasDeActividad(int pedido, int esperado)
        {
            var f = ReposicionStockCalculador.Normalizar(new ReposicionFiltroVm { DiasVentana = pedido });

            Assert.Equal(esperado, f.DiasVentana);
        }

        [Theory]
        [InlineData(-3, 0)]
        [InlineData(7, 7)]
        [InlineData(500, 60)]
        public void Normalizar_AcotaLosDiasDeCobertura(int pedido, int esperado)
        {
            var f = ReposicionStockCalculador.Normalizar(new ReposicionFiltroVm { DiasCoberturaSinPunto = pedido });

            Assert.Equal(esperado, f.DiasCoberturaSinPunto);
        }

        [Fact]
        public void Normalizar_DescartaIdsInvalidosYDuplicados_YAceptaNulos()
        {
            var f = ReposicionStockCalculador.Normalizar(new ReposicionFiltroVm { IdsSucursales = new List<int> { 2, 2, 0, -1, 3 }, Texto = null, Tipo = null });

            Assert.Equal(new[] { 2, 3 }, f.IdsSucursales.ToArray());
            Assert.Equal("", f.Texto);
            Assert.Equal("", f.Tipo);
            Assert.NotNull(ReposicionStockCalculador.Normalizar(null));
        }

        [Theory]
        [InlineData(14.266f, true, 14.3f)]
        [InlineData(14.3f, true, 14.3f)]
        [InlineData(0.01f, true, 0.1f)]
        [InlineData(7.223f, false, 8f)]
        [InlineData(8f, false, 8f)]
        [InlineData(0f, true, 0f)]
        [InlineData(-2f, false, 0f)]
        public void RedondearPedido_SubeAlProximoPasoYNuncaDevuelveNegativos(float cantidad, bool pesable, float esperado)
        {
            Assert.Equal(esperado, ReposicionStockCalculador.RedondearPedido(cantidad, pesable), 3);
        }

        // ---- Sugerir puntos de stock ----

        private static ReposicionFiltroVm FiltroPuntos(int diasVentana = 30, int diasCobertura = 7, bool incluirConPunto = false, bool mostrarSinMovimiento = false, params int[] sucursales)
        {
            return ReposicionStockCalculador.Normalizar(new ReposicionFiltroVm
            {
                DiasVentana = diasVentana,
                DiasCoberturaPunto = diasCobertura,
                IncluirConPunto = incluirConPunto,
                MostrarSinMovimiento = mostrarSinMovimiento,
                IdsSucursales = sucursales.ToList()
            });
        }

        private static PuntosSugeridosVm CalcularPuntos(
            IList<ExistenciaStockPorSucursalPlanoVm> plano, IList<ActividadStockVm> actividad, ReposicionFiltroVm filtro)
        {
            return ReposicionStockCalculador.CalcularPuntosSugeridos(plano, actividad, new List<ProveedorHabitualVm>(), filtro, Ahora);
        }

        [Theory]
        [InlineData(2f, 7, 14)]          // 2 kg/día x 7 días
        [InlineData(0.6248333f, 7, 5)]   // 4,37 -> sube a 5 (Churrasquito real)
        [InlineData(0.001f, 7, 1)]       // consumo mínimo: nunca menos de 1
        [InlineData(1000000f, 30, ReposicionStockCalculador.MaxPuntoStock)]  // tope
        public void SugerirPunto_CubreLosDiasDeConsumo_ConMinimo1YTope(float consumoDiario, int dias, int esperado)
        {
            Assert.Equal(esperado, ReposicionStockCalculador.SugerirPunto(consumoDiario, dias));
        }

        [Fact]
        public void SinPuntoConConsumo_SeSugiereElPunto_YPuedeCargarse()
        {
            // 60 kg en 30 días = 2 kg/día; para 7 días = 14.
            var r = CalcularPuntos(new[] { Existencia(1, 1, stock: 3f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 60f) }, FiltroPuntos());

            var fila = Assert.Single(r.Filas);
            Assert.Equal(14, fila.PuntoSugerido);
            Assert.True(fila.PuedeCargarse);
            Assert.Equal(1.5f, fila.CoberturaDias.Value, 3);   // 3 kg alcanzan 1,5 días
            Assert.Equal(1, r.CantConSugerencia);
            Assert.Equal(1, r.CantSinPunto);
        }

        [Fact]
        public void LosDiasDeCobertura_CambianLaSugerencia()
        {
            var plano = new[] { Existencia(1, 1, stock: 0f, punto: 0f) };
            var act = new[] { Actividad(1, 1, ventas: 60f) };

            Assert.Equal(14, CalcularPuntos(plano, act, FiltroPuntos(diasCobertura: 7)).Filas.Single().PuntoSugerido);
            Assert.Equal(30, CalcularPuntos(plano, act, FiltroPuntos(diasCobertura: 15)).Filas.Single().PuntoSugerido);
        }

        [Fact]
        public void ConPuntoCargado_NoSeListaSalvoQueSePidaComparar_Y_NuncaPuedeCargarse()
        {
            var plano = new[] { Existencia(1, 1, stock: 3f, punto: 18f) };
            var act = new[] { Actividad(1, 1, ventas: 60f) };

            var sinComparar = CalcularPuntos(plano, act, FiltroPuntos());
            Assert.Empty(sinComparar.Filas);
            Assert.Equal(1, sinComparar.CantConPunto);

            var comparando = CalcularPuntos(plano, act, FiltroPuntos(incluirConPunto: true));
            var fila = Assert.Single(comparando.Filas);
            Assert.Equal(14, fila.PuntoSugerido);
            Assert.Equal(18f, fila.PuntoActual);
            Assert.False(fila.PuedeCargarse);   // un punto cargado no se pisa
        }

        [Fact]
        public void SinConsumo_NoSeSugiere_SeOcultaPeroSeCuenta_YSeListaSiSePide()
        {
            var plano = new[] { Existencia(1, 1, stock: 0f, punto: 0f) };

            var oculto = CalcularPuntos(plano, new ActividadStockVm[0], FiltroPuntos());
            Assert.Empty(oculto.Filas);
            Assert.Equal(1, oculto.CantSinConsumo);

            var visible = CalcularPuntos(plano, new ActividadStockVm[0], FiltroPuntos(mostrarSinMovimiento: true));
            var fila = Assert.Single(visible.Filas);
            Assert.Null(fila.PuntoSugerido);
            Assert.False(fila.PuedeCargarse);
        }

        [Fact]
        public void InsumoConsumidoSoloEnElaboracion_TambienTieneSugerencia()
        {
            var r = CalcularPuntos(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { Actividad(1, 1, ventas: 0f, elaboracion: 3f) }, FiltroPuntos());

            Assert.Equal(1, Assert.Single(r.Filas).PuntoSugerido);   // 0,1 kg/día x 7 = 0,7 -> 1
        }

        [Fact]
        public void LasFilasSeOrdenanPorSucursalYDentroPorConsumoDescendente()
        {
            var plano = new[]
            {
                Existencia(1, 2, stock: 0f, punto: 0f, nombre: "Poco", sucursal: "B"),
                Existencia(2, 2, stock: 0f, punto: 0f, nombre: "Mucho", sucursal: "B"),
                Existencia(3, 1, stock: 0f, punto: 0f, nombre: "Otro", sucursal: "A")
            };
            var act = new[] { Actividad(1, 2, ventas: 10f), Actividad(2, 2, ventas: 300f), Actividad(3, 1, ventas: 30f) };

            var r = CalcularPuntos(plano, act, FiltroPuntos());

            Assert.Equal(new[] { "Otro", "Mucho", "Poco" }, r.Filas.Select(x => x.Producto).ToArray());
        }

        [Theory]
        [InlineData(0, 7)]
        [InlineData(-4, 7)]
        [InlineData(1, 1)]
        [InlineData(15, 15)]
        [InlineData(5000, 90)]
        public void Normalizar_AcotaLosDiasDeCoberturaDelPunto(int pedido, int esperado)
        {
            Assert.Equal(esperado, ReposicionStockCalculador.Normalizar(new ReposicionFiltroVm { DiasCoberturaPunto = pedido }).DiasCoberturaPunto);
        }

        // ---- Validar lo que se guarda (input externo) ----

        private static List<PuntoAGuardarVm> Validar(IList<ExistenciaStockPorSucursalPlanoVm> plano, IEnumerable<PuntoAGuardarVm> pedidos, out List<string> rechazados)
        {
            rechazados = new List<string>();
            return ReposicionStockCalculador.ValidarPuntosAGuardar(plano, pedidos, rechazados);
        }

        [Fact]
        public void Guardar_AceptaUnPuntoValidoDeUnProductoSinPunto()
        {
            List<string> rechazados;
            var ok = Validar(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { new PuntoAGuardarVm { IdCorte = 1, IdSucursal = 1, PuntoStock = 14 } }, out rechazados);

            Assert.Single(ok);
            Assert.Empty(rechazados);
        }

        [Fact]
        public void Guardar_NuncaPisaUnPuntoYaCargado()
        {
            List<string> rechazados;
            var ok = Validar(new[] { Existencia(1, 1, stock: 0f, punto: 18f, nombre: "Carre", sucursal: "San Lorenzo") },
                new[] { new PuntoAGuardarVm { IdCorte = 1, IdSucursal = 1, PuntoStock = 99 } }, out rechazados);

            Assert.Empty(ok);
            var motivo = Assert.Single(rechazados);
            Assert.Contains("Carre (San Lorenzo)", motivo);
            Assert.Contains("no se pisa", motivo);
        }

        [Fact]
        public void Guardar_RechazaProductosOSucursalesQueNoLlevanStock()
        {
            List<string> rechazados;
            var ok = Validar(new[] { Existencia(1, 1, stock: 0f, punto: 0f) },
                new[] { new PuntoAGuardarVm { IdCorte = 777, IdSucursal = 1, PuntoStock = 5 }, new PuntoAGuardarVm { IdCorte = 1, IdSucursal = 9, PuntoStock = 5 } }, out rechazados);

            Assert.Empty(ok);
            Assert.Equal(2, rechazados.Count);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        [InlineData(1000000)]
        public void Guardar_RechazaValoresFueraDeRango(int valor)
        {
            List<string> rechazados;
            var ok = Validar(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { new PuntoAGuardarVm { IdCorte = 1, IdSucursal = 1, PuntoStock = valor } }, out rechazados);

            Assert.Empty(ok);
            Assert.Contains("entero entre 1 y", Assert.Single(rechazados));
        }

        [Fact]
        public void Guardar_UnRepetidoSoloSeAcepta1Vez()
        {
            List<string> rechazados;
            var pedido = new PuntoAGuardarVm { IdCorte = 1, IdSucursal = 1, PuntoStock = 5 };
            var ok = Validar(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new[] { pedido, new PuntoAGuardarVm { IdCorte = 1, IdSucursal = 1, PuntoStock = 6 } }, out rechazados);

            Assert.Equal(5, Assert.Single(ok).PuntoStock);
            Assert.Contains("repetido", Assert.Single(rechazados));
        }

        [Fact]
        public void Guardar_ProcesaComoMuchoElMaximoPorPedido_YAvisa()
        {
            var plano = Enumerable.Range(1, ReposicionStockCalculador.MaxPuntosPorGuardado + 20).Select(i => Existencia(i, 1, stock: 0f, punto: 0f)).ToList();
            var pedidos = plano.Select(f => new PuntoAGuardarVm { IdCorte = f.IdCorte, IdSucursal = 1, PuntoStock = 3 }).ToList();
            List<string> rechazados;

            var ok = Validar(plano, pedidos, out rechazados);

            Assert.Equal(ReposicionStockCalculador.MaxPuntosPorGuardado, ok.Count);
            Assert.Contains("solo se procesan los primeros", Assert.Single(rechazados));
        }

        [Fact]
        public void Guardar_ConPedidoNuloOConElementosNulos_NoFalla()
        {
            List<string> rechazados;
            Assert.Empty(Validar(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, null, out rechazados));
            Assert.Empty(Validar(new[] { Existencia(1, 1, stock: 0f, punto: 0f) }, new PuntoAGuardarVm[] { null }, out rechazados));
            Assert.Empty(rechazados);
        }
    }
}
