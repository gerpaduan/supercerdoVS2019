// Pruebas de las salvaguardas para usar ARCA (ticket compartido con candado, limitador, corte automatico,
// cache) y del certificado de la plataforma (carpeta AFIP/_plataforma).
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using AFIP;

namespace AFIP.Tests
{
    public sealed class AfipTicketCacheTests : IDisposable
    {
        private readonly string _carpeta = Path.Combine(Path.GetTempPath(), "ticket-tests-" + Guid.NewGuid().ToString("N"));

        public AfipTicketCacheTests() { Directory.CreateDirectory(_carpeta); }

        public void Dispose()
        {
            try { Directory.Delete(_carpeta, true); } catch (IOException) { /* limpieza best-effort */ }
        }

        [Fact]
        public void ConVariosPedidosSimultaneosConTicketVencido_SePideUnaSolaVez()
        {
            string ruta = Path.Combine(_carpeta, "TicketAccesoPerson.txt");
            int pedidos = 0;

            // 12 consultas "de distintas empresas" llegan a la vez y no hay ticket: solo una lo pide a ARCA.
            var resultados = Enumerable.Range(0, 12).Select(_ => Task.Run(() =>
                AfipTicketCache.ObtenerOPedir(ruta, t => t == "ticket-1", () =>
                {
                    Interlocked.Increment(ref pedidos);
                    Thread.Sleep(150); // simula la demora de ARCA para que los demas queden esperando
                    return "ticket-1";
                }))).ToArray();

            Task.WaitAll(resultados);

            Assert.Equal(1, pedidos);
            Assert.All(resultados, r => Assert.Equal("ticket-1", r.Result));
            Assert.Equal("ticket-1", File.ReadAllText(ruta));
        }

        [Fact]
        public void ConTicketVigente_NoSePideOtroAunqueLleguenMuchosPedidos()
        {
            string ruta = Path.Combine(_carpeta, "TicketAcceso.txt");
            File.WriteAllText(ruta, "vigente");
            int pedidos = 0;

            var tareas = Enumerable.Range(0, 20).Select(_ => Task.Run(() =>
                AfipTicketCache.ObtenerOPedir(ruta, t => t == "vigente", () => { Interlocked.Increment(ref pedidos); return "otro"; }))).ToArray();
            Task.WaitAll(tareas);

            Assert.Equal(0, pedidos);
            Assert.All(tareas, t => Assert.Equal("vigente", t.Result));
        }

        [Fact]
        public void TicketVencido_SeReemplazaYNoDejaTemporales()
        {
            string ruta = Path.Combine(_carpeta, "TicketAcceso.txt");
            File.WriteAllText(ruta, "viejo");

            string nuevo = AfipTicketCache.ObtenerOPedir(ruta, t => t == "nuevo", () => "nuevo");

            Assert.Equal("nuevo", nuevo);
            Assert.Equal("nuevo", File.ReadAllText(ruta));
            Assert.Empty(Directory.GetFiles(_carpeta, "*.tmp"));
        }

        [Fact]
        public void RutasDistintas_NoSeBloqueanEntreSi()
        {
            string a = Path.Combine(_carpeta, "a.txt");
            string b = Path.Combine(_carpeta, "b.txt");
            var entroA = new ManualResetEventSlim(false);
            var soltarA = new ManualResetEventSlim(false);

            var tareaA = Task.Run(() => AfipTicketCache.ObtenerOPedir(a, _ => false, () => { entroA.Set(); soltarA.Wait(5000); return "A"; }));
            Assert.True(entroA.Wait(5000));

            // Mientras A esta "esperando a ARCA", B (otra empresa) se resuelve sin esperar.
            string resultadoB = Task.Run(() => AfipTicketCache.ObtenerOPedir(b, _ => false, () => "B")).Result;
            Assert.Equal("B", resultadoB);

            soltarA.Set();
            Assert.Equal("A", tareaA.Result);
        }
    }

    public class AfipLimitadorTests
    {
        [Fact]
        public void PermiteHastaElTopeYLuegoNiega_YSeRecuperaPasadaLaVentana()
        {
            var ahora = new DateTime(2026, 9, 24, 10, 0, 0, DateTimeKind.Utc);
            var limitador = new AfipLimitador(3, TimeSpan.FromHours(1), () => ahora);

            Assert.True(limitador.IntentarUsar("1"));
            Assert.True(limitador.IntentarUsar("1"));
            Assert.True(limitador.IntentarUsar("1"));
            Assert.False(limitador.IntentarUsar("1"));
            Assert.Equal(3, limitador.UsosEnVentana("1"));

            ahora = ahora.AddMinutes(59);
            Assert.False(limitador.IntentarUsar("1"));
            ahora = ahora.AddMinutes(2);
            Assert.True(limitador.IntentarUsar("1"));
        }

        [Fact]
        public void CadaClaveTieneSuPropioTope()
        {
            var limitador = new AfipLimitador(1, TimeSpan.FromHours(1), () => DateTime.UtcNow);
            Assert.True(limitador.IntentarUsar("empresa-1"));
            Assert.False(limitador.IntentarUsar("empresa-1"));
            Assert.True(limitador.IntentarUsar("empresa-2"));
            Assert.Equal(0, limitador.UsosEnVentana("empresa-3"));
        }

        [Fact]
        public void ElTopeSeRespetaConHilosConcurrentes()
        {
            var limitador = new AfipLimitador(5, TimeSpan.FromHours(1));
            int concedidos = 0;
            Parallel.For(0, 50, _ => { if (limitador.IntentarUsar("x")) Interlocked.Increment(ref concedidos); });
            Assert.Equal(5, concedidos);
        }
    }

    public class AfipCorteAutomaticoTests
    {
        [Fact]
        public void CortaTrasLosFallosSeguidos_YVuelveAProbarPasadoElTiempo()
        {
            var ahora = new DateTime(2026, 9, 24, 10, 0, 0, DateTimeKind.Utc);
            var corte = new AfipCorteAutomatico(3, TimeSpan.FromMinutes(10), () => ahora);

            corte.RegistrarFallo();
            corte.RegistrarFallo();
            Assert.False(corte.Cortado);
            corte.RegistrarFallo();
            Assert.True(corte.Cortado);
            Assert.NotNull(corte.CortadoHasta);

            ahora = ahora.AddMinutes(11);
            Assert.False(corte.Cortado);

            // Si el reintento falla, corta de nuevo enseguida (el contador no se reinicia).
            corte.RegistrarFallo();
            Assert.True(corte.Cortado);
        }

        [Fact]
        public void UnExitoReiniciaElContador()
        {
            var corte = new AfipCorteAutomatico(2, TimeSpan.FromMinutes(10));
            corte.RegistrarFallo();
            corte.RegistrarExito();
            corte.RegistrarFallo();
            Assert.False(corte.Cortado);
            Assert.Equal(1, corte.FallosSeguidos);
        }
    }

    public class AfipCacheTemporalTests
    {
        [Fact]
        public void GuardaHastaQueVence()
        {
            var ahora = new DateTime(2026, 9, 24, 10, 0, 0, DateTimeKind.Utc);
            var cache = new AfipCacheTemporal<string>(TimeSpan.FromHours(24), () => ahora);

            Assert.False(cache.TryGet("k", out _));
            cache.Set("k", "valor");
            Assert.True(cache.TryGet("k", out var v));
            Assert.Equal("valor", v);

            ahora = ahora.AddHours(25);
            Assert.False(cache.TryGet("k", out _));
        }
    }

    public sealed class CertificadoPlataformaTests : IDisposable
    {
        private const long CuitPlataforma = 30719291909;
        private readonly string _raiz = Path.Combine(Path.GetTempPath(), "plataforma-tests-" + Guid.NewGuid().ToString("N"));

        public CertificadoPlataformaTests() { Directory.CreateDirectory(_raiz); }

        public void Dispose()
        {
            try { Directory.Delete(_raiz, true); } catch (IOException) { /* limpieza best-effort */ }
        }

        [Fact]
        public void CarpetaPlataforma_EsDistintaDeLasCarpetasPorCuit()
        {
            string plataforma = AfipRutas.CarpetaPlataforma(_raiz);
            Assert.Equal(Path.Combine(_raiz, "AFIP", "_plataforma"), plataforma);
            // Una carpeta por CUIT nunca puede llamarse igual: Carpeta() exige solo digitos.
            Assert.Throws<ArgumentException>(() => AfipRutas.Carpeta(_raiz, "_plataforma"));
        }

        [Fact]
        public void FlujoCompleto_TrabajaEnLaCarpetaDeLaPlataformaYNoEnLaDeUnaEmpresa()
        {
            var servicio = CertificadoArcaService.ParaPlataforma(_raiz);
            servicio.GenerarCsr(CuitPlataforma, "CarniSys SA", "carnisys-padron");

            string carpeta = AfipRutas.CarpetaPlataforma(_raiz);
            Assert.True(File.Exists(Path.Combine(carpeta, "pendiente", "clave.key")));
            Assert.False(Directory.Exists(Path.Combine(_raiz, "AFIP", CuitPlataforma.ToString())));

            // "ARCA" emite el certificado para esa clave.
            byte[] crt;
            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(File.ReadAllText(Path.Combine(carpeta, "pendiente", "clave.key")));
                var pedido = new CertificateRequest("CN=carnisys-padron, SERIALNUMBER=CUIT " + CuitPlataforma, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using var cert = pedido.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(700));
                crt = cert.Export(X509ContentType.Cert);
            }

            string? clave = null;
            var resultado = servicio.Instalar(CuitPlataforma, AfipRutas.NombreCertificadoPlataforma, crt, c => clave = c);

            Assert.Equal(AfipRutas.NombreCertificadoPlataforma, resultado.NombreArchivo);
            Assert.True(File.Exists(Path.Combine(carpeta, AfipRutas.NombreCertificadoPlataforma)));
            Assert.True(File.Exists(Path.Combine(carpeta, "LoginTemplate.xml")));
            Assert.False(Directory.Exists(Path.Combine(carpeta, "pendiente")));

            var info = servicio.Leer(CuitPlataforma, AfipRutas.NombreCertificadoPlataforma, clave, new[] { 60, 30, 15 });
            Assert.Equal(EstadoCertificado.Vigente, info.Estado);
            Assert.InRange(info.DiasRestantes!.Value, 698, 701);
        }
    }
}
