// Tests del ticket termico de un pedido (WebCore.Services.PedidoTicket): un ticket por sucursal, con el formato del ticket de Movimientos
// (Producto / Cant. / Kgs., codigo debajo en fuente B), SIN importes, y con el comando de corte (ESC/POS) o el salto de pagina (HTML) entre
// una sucursal y la siguiente. Ver docs/DECISIONS.md (2026-10-20, "Ticket de pedido por sucursal").
using System.Text.RegularExpressions;
using Entidades;
using WebCore.Services;

namespace WebCore.Tests
{
    public class PedidoTicketTests
    {
        private static readonly DateTime Fecha = new DateTime(2026, 10, 20, 11, 30, 0);

        private static PedidoLinea Linea(int idSucursal, string sucursal, long codigo, string producto, decimal cantidad, bool pesable = false)
        {
            return new PedidoLinea
            {
                IdCorte = (int)codigo,
                IdSucursal = idSucursal,
                Sucursal = sucursal,
                Codigo = codigo,
                Producto = producto,
                Cantidad = cantidad,
                Pesable = pesable,
                // Con precio a proposito: el ticket NO tiene que mostrarlo.
                PrecioUnitario = 5600m,
                Proveedor = "Proveedor SA",
                FechaPrecio = new DateTime(2026, 9, 1),
                Importe = 123456.78m
            };
        }

        private static Pedido Pedido(params PedidoLinea[] lineas)
        {
            return new Pedido { Id = 12, Fecha = Fecha, Usuario = "Juan", TotalEstimado = 999999.99m, LineasSinPrecio = 2, Lineas = lineas.ToList() };
        }

        // Quita los comandos ESC/POS de formato (negrita, fuente B) para poder medir y comparar el texto visible.
        private static string Visible(string linea)
        {
            return Regex.Replace(linea, "\x1B\x45[\x00\x01]|\x1B\x4D[\x00\x01]|\x1D\x21[\x00\x11]", "");
        }

        private static bool EsCorte(string linea) { return linea == EscPosFormato.Corte(); }

        [Fact]
        public void UnaSucursal_UnSoloTicket_SinCortes()
        {
            var lineas = PedidoTicket.ConstruirLineas(Pedido(Linea(1, "San Martín", 5, "Costilla", 5m)), 80, true);

            Assert.DoesNotContain(lineas, EsCorte);
            Assert.Single(lineas, l => Visible(l).Contains("Sucursal: San Martín"));
        }

        [Fact]
        public void DosSucursales_DosTickets_ConUnCorteEntreLosDos()
        {
            var pedido = Pedido(
                Linea(2, "San Martín", 5, "Costilla", 5m),
                Linea(1, "Central", 6, "Colita de Cuadril", 3m),
                Linea(1, "Central", 5, "Costilla", 2m));

            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, true);

            Assert.Single(lineas, EsCorte);
            int corte = lineas.FindIndex(l => EsCorte(l));
            var primero = lineas.Take(corte).Select(Visible).ToList();
            var segundo = lineas.Skip(corte + 1).Select(Visible).ToList();

            // Orden alfabetico de sucursales (Central antes que San Martín) y cada ticket solo con sus productos.
            Assert.Contains(primero, l => l.StartsWith("Sucursal: Central"));
            Assert.Contains(primero, l => l.StartsWith("Colita de Cuadril"));
            Assert.Contains(primero, l => l.StartsWith("Costilla"));
            Assert.DoesNotContain(primero, l => l.Contains("San Martín"));

            Assert.Contains(segundo, l => l.StartsWith("Sucursal: San Martín"));
            Assert.Contains(segundo, l => l.StartsWith("Costilla"));
            Assert.DoesNotContain(segundo, l => l.StartsWith("Colita"));
            Assert.DoesNotContain(segundo, l => l.Contains("Central"));
        }

        [Fact]
        public void TresSucursales_DosCortes_YNingunoAntesNiDespuesDeTodo()
        {
            var pedido = Pedido(Linea(1, "A", 1, "Uno", 1m), Linea(2, "B", 1, "Uno", 1m), Linea(3, "C", 1, "Uno", 1m));

            var lineas = PedidoTicket.ConstruirLineas(pedido, 58, true);

            Assert.Equal(2, lineas.Count(EsCorte));
            Assert.False(EsCorte(lineas[0]));
            Assert.False(EsCorte(lineas[lineas.Count - 1])); // el corte final lo pone el agente
        }

        [Fact]
        public void ElCorte_EsElMismoComandoQueElAgenteAgregaAlFinal()
        {
            // PrintAgent/EscPosTicketPrinter.cs termina cada trabajo con GS V 65 0 (1D 56 41 00).
            Assert.Equal(new[] { '\x1D', 'V', 'A', '\0' }, EscPosFormato.Corte().ToCharArray());
        }

        [Fact]
        public void ElPayloadViajaPorJson_ConElCorteIntacto()
        {
            // El controller devuelve ticketLines con Json(...): el byte 0x00 del corte tiene que salir escapado (\u0000) y volver igual,
            // porque el agente lo decodifica y lo manda tal cual a la impresora.
            var pedido = Pedido(Linea(1, "A", 1, "Uno", 1m), Linea(2, "B", 1, "Uno", 1m));
            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, true);

            string json = System.Text.Json.JsonSerializer.Serialize(new { ok = true, ticketMm = 80, ticketLines = lineas });
            Assert.Contains("\\u001DVA\\u0000", json);

            var devueltas = System.Text.Json.JsonDocument.Parse(json).RootElement.GetProperty("ticketLines")
                .EnumerateArray().Select(e => e.GetString()).ToList();
            Assert.Equal(lineas, devueltas);
        }

        [Fact]
        public void ElTicketNoLlevaNingunImporte()
        {
            var pedido = Pedido(Linea(1, "Central", 5, "Costilla", 5m), Linea(2, "Norte", 6, "Chorizo", 3m));

            foreach (bool escPos in new[] { true, false })
            {
                string texto = string.Join("\n", PedidoTicket.ConstruirLineas(pedido, 80, escPos));
                Assert.DoesNotContain("$", texto);
                Assert.DoesNotContain("Importe", texto, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("estimado", texto, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("precio", texto, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("5.600", texto);
                Assert.DoesNotContain("123.456", texto);
                Assert.DoesNotContain("999.999", texto);
                Assert.DoesNotContain("Proveedor SA", texto);
            }
        }

        [Fact]
        public void FormatoDeMovimientos_EncabezadoColumnasYCodigoDebajo()
        {
            var lineas = PedidoTicket.ConstruirLineas(Pedido(Linea(1, "Central", 5, "Costilla", 5m)), 80, true).Select(Visible).ToList();

            Assert.Equal(new string(' ', 18) + "Pedido", lineas[0]); // centrado en 43 columnas
            Assert.Equal("N°: 12", lineas[1]);
            Assert.Equal("Sucursal: Central", lineas[2]);
            Assert.Equal("Fecha: 20/10/2026 11:30", lineas[3]);
            Assert.Equal("Usuario: Juan", lineas[4]);
            // Columnas como en Movimientos: Producto (22) + espacio + Cant. (6) + 4 espacios + Kgs. (8).
            Assert.Equal("Producto".PadRight(22) + " " + "Cant.".PadLeft(6) + "    " + "Kgs.".PadLeft(8), lineas[6]);
            Assert.Equal(new string('-', 43), lineas[7]);
            Assert.Equal("Costilla".PadRight(22) + " " + "5".PadLeft(6) + "    " + new string(' ', 8), lineas[8]);
            Assert.Equal("Cod:5", lineas[9]);
        }

        [Fact]
        public void ElCodigoVaEnFuenteB_SoloEnElPayloadDelAgente()
        {
            var pedido = Pedido(Linea(1, "Central", 5, "Costilla", 5m));

            Assert.Contains(PedidoTicket.ConstruirLineas(pedido, 80, true), l => l == EscPosFormato.FuenteB("Cod:5"));
            Assert.Contains(PedidoTicket.ConstruirLineas(pedido, 80, false), l => l == "Cod:5");
        }

        [Fact]
        public void ElCodigoDeBarrasEntero_NoSeCorta()
        {
            var lineas = PedidoTicket.ConstruirLineas(Pedido(Linea(1, "Central", 7790001234567L, "Agua", 6m)), 58, true).Select(Visible).ToList();

            Assert.Contains("Cod:7790001234567", lineas);
        }

        [Fact]
        public void ProductoPesable_VaEnKgs_YElDeUnidades_EnCant()
        {
            var pedido = Pedido(Linea(1, "Central", 1, "Carre", 20.5m, pesable: true), Linea(1, "Central", 2, "Huevo", 12m));

            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, false);
            string carre = lineas.Single(l => l.StartsWith("Carre"));
            string huevo = lineas.Single(l => l.StartsWith("Huevo"));

            // 80 mm: nombre 22 + 1 + Cant. 6 + 4 + Kgs. 8.
            Assert.Equal("Carre".PadRight(22) + " " + "".PadLeft(6) + "    " + "20,5".PadLeft(8), carre);
            Assert.Equal("Huevo".PadRight(22) + " " + "12".PadLeft(6) + "    " + "".PadLeft(8), huevo);
        }

        [Fact]
        public void ElTotal_SumaUnidadesYKilosPorSeparado()
        {
            var pedido = Pedido(
                Linea(1, "Central", 1, "Carre", 20.5m, pesable: true),
                Linea(1, "Central", 3, "Peceto", 4.25m, pesable: true),
                Linea(1, "Central", 2, "Huevo", 12m),
                Linea(1, "Central", 4, "Salame", 3m));

            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, false);
            string total = lineas.Single(l => l.StartsWith("Total"));

            Assert.Equal("Total".PadRight(22) + " " + "15".PadLeft(6) + "    " + "24,75".PadLeft(8), total);
        }

        [Fact]
        public void ElNombreSeCortaAlAnchoDeLaColumna_ComoEnMovimientos()
        {
            var pedido = Pedido(Linea(1, "Central", 1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 3m));

            string ancho58 = PedidoTicket.ConstruirLineas(pedido, 58, false).Single(l => l.StartsWith("ABC"));
            string ancho80 = PedidoTicket.ConstruirLineas(pedido, 80, false).Single(l => l.StartsWith("ABC"));

            Assert.StartsWith("ABCDEFGHIJKLMNO ", ancho58);          // 15 caracteres + espacio
            Assert.DoesNotContain("ABCDEFGHIJKLMNOP", ancho58);
            Assert.StartsWith("ABCDEFGHIJKLMNOPQRSTUV ", ancho80);   // 22 caracteres + espacio
            Assert.DoesNotContain("ABCDEFGHIJKLMNOPQRSTUVW", ancho80);
        }

        [Fact]
        public void UnaCantidadGrande_NuncaPierdeUnDigito()
        {
            // 12345,678 kg no entra con decimales en 7 columnas (58 mm): se le sacan decimales, no digitos.
            var pedido = Pedido(Linea(1, "Central", 1, "Carre", 12345.678m, pesable: true));

            string carre = PedidoTicket.ConstruirLineas(pedido, 58, false).Single(l => l.StartsWith("Carre"));
            string kilos = carre.Substring(carre.Length - 7).Trim();

            Assert.StartsWith("12345", kilos);
        }

        [Theory]
        [InlineData(58, 32)]
        [InlineData(80, 43)]
        public void NingunaLineaSePasaDelAnchoDelPapel(int mm, int ancho)
        {
            var pedido = Pedido(
                Linea(1, "Una sucursal con un nombre larguisimo", 7790001234567L, "Un producto con un nombre larguisimo", 1234.5m, pesable: true),
                Linea(1, "Una sucursal con un nombre larguisimo", 2, "Huevo", 12m));
            pedido.Nota = "Una nota muy larga que hay que envolver porque no entra en una sola linea del ticket termico";

            foreach (string linea in PedidoTicket.ConstruirLineas(pedido, mm, true).Where(l => !EsCorte(l)).Select(Visible))
                Assert.True(linea.Length <= ancho, "Se pasa del ancho (" + linea.Length + " > " + ancho + "): " + linea);
        }

        [Fact]
        public void LaNota_SaleComoObservaciones()
        {
            var pedido = Pedido(Linea(1, "Central", 1, "Carre", 2m));
            pedido.Nota = "Entregar antes de las 8";

            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, false);

            int i = lineas.IndexOf("Observaciones:");
            Assert.True(i > 0);
            Assert.Equal("Entregar antes de las 8", lineas[i + 1]);
        }

        [Fact]
        public void ConVariasSucursales_CadaTicketDiceCualEsDeCuantos()
        {
            var pedido = Pedido(Linea(1, "A", 1, "Uno", 1m), Linea(2, "B", 1, "Uno", 1m));

            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, false);

            Assert.Contains("N°: 12  (1 de 2)", lineas);
            Assert.Contains("N°: 12  (2 de 2)", lineas);
        }

        [Fact]
        public void ConUnaSolaSucursal_NoHayNumeroDeTicket()
        {
            var lineas = PedidoTicket.ConstruirLineas(Pedido(Linea(1, "A", 1, "Uno", 1m)), 80, false);

            Assert.Contains("N°: 12", lineas);
            Assert.DoesNotContain(lineas, l => l.Contains(" de "));
        }

        [Fact]
        public void EnElHtmlDeRespaldo_ElSeparadorEsUnSaltoDePagina_YNoHayComandosDeControl()
        {
            var pedido = Pedido(Linea(1, "A", 1, "Uno", 1m), Linea(2, "B", 1, "Uno", 1m));

            var lineas = PedidoTicket.ConstruirLineas(pedido, 80, false);

            Assert.Single(lineas, PedidoTicket.SaltoDePagina);
            Assert.DoesNotContain(string.Join("", lineas), "\x1B");
            Assert.DoesNotContain(string.Join("", lineas), "\x1D");
        }

        [Fact]
        public void UnPedidoSinLineas_DevuelveUnTicketConElAviso()
        {
            var lineas = PedidoTicket.ConstruirLineas(Pedido(), 80, false);

            Assert.Contains("Sin productos", lineas);
            Assert.DoesNotContain(PedidoTicket.SaltoDePagina, lineas);
        }
    }
}
