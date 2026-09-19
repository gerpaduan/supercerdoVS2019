using System;
using Negocio;
using Xunit;
using static Negocio.VerificacionDispositivoStore;

namespace NegocioTests
{
    // Login solo desde dispositivos seguros (2026-09-19): pedidos en memoria de "autorizar este
    // dispositivo por mail" -- codigo de 6 digitos, 10 min, 5 intentos, 1 mail por minuto -- y la
    // serie hasheada de la cookie del navegador. Sin BD ni web: reloj inyectado.
    public class VerificacionDispositivoStoreTests
    {
        private sealed class Reloj
        {
            public DateTime Ahora = new DateTime(2026, 9, 19, 10, 0, 0, DateTimeKind.Utc);
        }

        private static (VerificacionDispositivoStore store, Reloj reloj, string nonce) Crear()
        {
            var reloj = new Reloj();
            var store = new VerificacionDispositivoStore(() => reloj.Ahora);
            string nonce = store.CrearPedido(idUsuario: 7, idEmpresa: 1, serieDispositivo: "web:abc", returnUrl: "/Ventas");
            return (store, reloj, nonce);
        }

        [Fact]
        public void CrearPedido_GuardaUsuarioEmpresaSerieYReturnUrl()
        {
            var (store, _, nonce) = Crear();

            var pedido = store.Obtener(nonce);

            Assert.NotNull(pedido);
            Assert.Equal(7, pedido.IdUsuario);
            Assert.Equal(1, pedido.IdEmpresa);
            Assert.Equal("web:abc", pedido.SerieDispositivo);
            Assert.Equal("/Ventas", pedido.ReturnUrl);
        }

        [Fact]
        public void Obtener_NonceDesconocidoOVacio_DevuelveNull()
        {
            var (store, _, _) = Crear();

            Assert.Null(store.Obtener("no-existe"));
            Assert.Null(store.Obtener(""));
            Assert.Null(store.Obtener(null));
        }

        [Fact]
        public void Obtener_PedidoVencido_DevuelveNull()
        {
            var (store, reloj, nonce) = Crear();

            reloj.Ahora = reloj.Ahora + VidaPedido + TimeSpan.FromSeconds(1);

            Assert.Null(store.Obtener(nonce));
        }

        [Fact]
        public void GenerarCodigo_Devuelve6Digitos()
        {
            var (store, _, nonce) = Crear();

            string codigo = store.GenerarCodigo(nonce);

            Assert.Matches(@"^\d{6}$", codigo);
        }

        [Fact]
        public void Verificar_CodigoCorrecto_EsOkYConsumeElPedido()
        {
            var (store, _, nonce) = Crear();
            string codigo = store.GenerarCodigo(nonce);

            Assert.Equal(ResultadoVerificacion.Ok, store.Verificar(nonce, codigo));
            Assert.Equal(ResultadoVerificacion.Expirado, store.Verificar(nonce, codigo));
        }

        [Fact]
        public void Verificar_AceptaEspaciosAlrededorDelCodigo()
        {
            var (store, _, nonce) = Crear();
            string codigo = store.GenerarCodigo(nonce);

            Assert.Equal(ResultadoVerificacion.Ok, store.Verificar(nonce, " " + codigo.Substring(0, 3) + " " + codigo.Substring(3) + " "));
        }

        [Fact]
        public void Verificar_CodigoIncorrecto_EsIncorrectoYSigueValiendoElCorrecto()
        {
            var (store, _, nonce) = Crear();
            string codigo = store.GenerarCodigo(nonce);
            string otro = codigo == "000000" ? "111111" : "000000";

            Assert.Equal(ResultadoVerificacion.Incorrecto, store.Verificar(nonce, otro));
            Assert.Equal(ResultadoVerificacion.Ok, store.Verificar(nonce, codigo));
        }

        [Fact]
        public void Verificar_QuintoIntentoFallido_DescartaElPedido()
        {
            var (store, _, nonce) = Crear();
            string codigo = store.GenerarCodigo(nonce);
            string otro = codigo == "000000" ? "111111" : "000000";

            for (int i = 1; i < MaxIntentos; i++)
                Assert.Equal(ResultadoVerificacion.Incorrecto, store.Verificar(nonce, otro));

            Assert.Equal(ResultadoVerificacion.DemasiadosIntentos, store.Verificar(nonce, otro));
            // ya no sirve ni el codigo correcto: hay que volver a poner la clave
            Assert.Equal(ResultadoVerificacion.Expirado, store.Verificar(nonce, codigo));
        }

        [Fact]
        public void Verificar_CodigoVencido_EsExpirado()
        {
            var (store, reloj, nonce) = Crear();
            string codigo = store.GenerarCodigo(nonce);

            reloj.Ahora = reloj.Ahora + VidaCodigo + TimeSpan.FromSeconds(1);

            Assert.Equal(ResultadoVerificacion.Expirado, store.Verificar(nonce, codigo));
        }

        [Fact]
        public void Verificar_SinHaberPedidoCodigo_EsSinCodigo()
        {
            var (store, _, nonce) = Crear();

            Assert.Equal(ResultadoVerificacion.SinCodigo, store.Verificar(nonce, "123456"));
        }

        [Fact]
        public void PuedeEnviarCodigo_RespetaLaEsperaEntreEnvios()
        {
            var (store, reloj, nonce) = Crear();

            Assert.True(store.PuedeEnviarCodigo(nonce, out _));
            store.GenerarCodigo(nonce);

            Assert.False(store.PuedeEnviarCodigo(nonce, out var espera));
            Assert.True(espera > TimeSpan.Zero && espera <= EsperaEntreEnvios);

            reloj.Ahora = reloj.Ahora + EsperaEntreEnvios + TimeSpan.FromSeconds(1);
            Assert.True(store.PuedeEnviarCodigo(nonce, out _));
        }

        [Fact]
        public void GenerarCodigo_Nuevo_InvalidaElAnteriorYReiniciaIntentos()
        {
            var (store, reloj, nonce) = Crear();
            string primero = store.GenerarCodigo(nonce);
            string otro = primero == "000000" ? "111111" : "000000";
            store.Verificar(nonce, otro);
            reloj.Ahora = reloj.Ahora + EsperaEntreEnvios + TimeSpan.FromSeconds(1);

            string segundo = store.GenerarCodigo(nonce);

            Assert.Equal(ResultadoVerificacion.Ok, store.Verificar(nonce, segundo));
        }

        [Fact]
        public void GenerarCodigo_PedidoInexistente_DevuelveNull()
        {
            var (store, _, _) = Crear();

            Assert.Null(store.GenerarCodigo("no-existe"));
        }

        [Fact]
        public void SerieDeToken_EsHashConPrefijoYDeterministico()
        {
            string serie = Negocio.DispositivoSeguro.SerieDeToken("token-secreto");

            Assert.StartsWith("web:", serie);
            Assert.Equal(4 + 64, serie.Length);
            Assert.DoesNotContain("token-secreto", serie);
            Assert.Equal(serie, Negocio.DispositivoSeguro.SerieDeToken(" token-secreto "));
            Assert.NotEqual(serie, Negocio.DispositivoSeguro.SerieDeToken("otro-token"));
        }

        [Fact]
        public void SerieDeToken_Vacio_DevuelveVacio()
        {
            Assert.Equal("", Negocio.DispositivoSeguro.SerieDeToken(""));
            Assert.Equal("", Negocio.DispositivoSeguro.SerieDeToken(null));
        }
    }
}
