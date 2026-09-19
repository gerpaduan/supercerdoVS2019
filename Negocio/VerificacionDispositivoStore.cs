using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Negocio
{
    // Pedidos de "autorizar este dispositivo por mail" (login solo desde dispositivos seguros,
    // 2026-09-19, ver docs/DECISIONS.md). Cuando un usuario que requiere dispositivo seguro pone
    // bien su clave desde un navegador no autorizado, se crea un pedido EN MEMORIA (sin
    // persistencia, mismo criterio que LoginRateLimiter: si el proceso reinicia, el usuario repite
    // el login -- el codigo dura solo minutos) identificado por un nonce que viaja en una cookie
    // del navegador. El codigo de 6 digitos se manda por mail y se escribe en la misma pantalla:
    // asi el alta queda atada al navegador que intenta entrar (un link en el mail podria abrirse
    // en otra app y autorizar el equipo equivocado). Sin dependencias web: testeable con reloj
    // inyectable.
    public class VerificacionDispositivoStore
    {
        public enum ResultadoVerificacion
        {
            Ok,
            Incorrecto,
            Expirado,
            DemasiadosIntentos,
            SinCodigo
        }

        public sealed class Pedido
        {
            public int IdUsuario { get; set; }
            public int IdEmpresa { get; set; }
            public string SerieDispositivo { get; set; }
            public string ReturnUrl { get; set; }
            public string Codigo { get; set; }
            public DateTime PedidoUtc { get; set; }
            public DateTime? CodigoExpiraUtc { get; set; }
            public DateTime? UltimoEnvioUtc { get; set; }
            public int Intentos { get; set; }
        }

        public static readonly TimeSpan VidaPedido = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan VidaCodigo = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan EsperaEntreEnvios = TimeSpan.FromMinutes(1);
        public const int MaxIntentos = 5;

        // Instancia compartida del proceso web; los tests crean la suya con un reloj propio.
        public static readonly VerificacionDispositivoStore Instancia = new VerificacionDispositivoStore();

        private readonly ConcurrentDictionary<string, Pedido> _pedidos = new ConcurrentDictionary<string, Pedido>();
        private readonly Func<DateTime> _ahoraUtc;
        private readonly object _sync = new object();

        public VerificacionDispositivoStore(Func<DateTime> ahoraUtc = null)
        {
            _ahoraUtc = ahoraUtc ?? (() => DateTime.UtcNow);
        }

        // Crea el pedido (clave ya validada por el login) y devuelve el nonce para la cookie.
        public string CrearPedido(int idUsuario, int idEmpresa, string serieDispositivo, string returnUrl)
        {
            Limpiar();
            string nonce = GenerarNonce();
            _pedidos[nonce] = new Pedido
            {
                IdUsuario = idUsuario,
                IdEmpresa = idEmpresa,
                SerieDispositivo = serieDispositivo,
                ReturnUrl = returnUrl ?? "",
                PedidoUtc = _ahoraUtc()
            };
            return nonce;
        }

        public Pedido Obtener(string nonce)
        {
            if (string.IsNullOrWhiteSpace(nonce))
                return null;

            Pedido pedido;
            if (!_pedidos.TryGetValue(nonce, out pedido))
                return null;

            if (_ahoraUtc() - pedido.PedidoUtc > VidaPedido)
            {
                Pedido descartado;
                _pedidos.TryRemove(nonce, out descartado);
                return null;
            }

            return pedido;
        }

        // Si todavia hay que esperar antes de mandar otro mail devuelve false (y cuanto falta).
        public bool PuedeEnviarCodigo(string nonce, out TimeSpan espera)
        {
            espera = TimeSpan.Zero;
            var pedido = Obtener(nonce);
            if (pedido == null)
                return false;

            lock (_sync)
            {
                if (pedido.UltimoEnvioUtc.HasValue)
                {
                    var restante = EsperaEntreEnvios - (_ahoraUtc() - pedido.UltimoEnvioUtc.Value);
                    if (restante > TimeSpan.Zero)
                    {
                        espera = restante;
                        return false;
                    }
                }
            }

            return true;
        }

        // Genera un codigo nuevo de 6 digitos (invalida el anterior y reinicia los intentos).
        // Devuelve null si el pedido no existe o vencio.
        public string GenerarCodigo(string nonce)
        {
            var pedido = Obtener(nonce);
            if (pedido == null)
                return null;

            lock (_sync)
            {
                var ahora = _ahoraUtc();
                pedido.Codigo = GenerarCodigoNumerico();
                pedido.CodigoExpiraUtc = ahora + VidaCodigo;
                pedido.UltimoEnvioUtc = ahora;
                pedido.Intentos = 0;
                return pedido.Codigo;
            }
        }

        // Ok consume el pedido (un codigo sirve una sola vez). Al 5to intento fallido el pedido se
        // descarta: hay que volver a poner la clave.
        public ResultadoVerificacion Verificar(string nonce, string codigoIngresado)
        {
            var pedido = Obtener(nonce);
            if (pedido == null)
                return ResultadoVerificacion.Expirado;

            lock (_sync)
            {
                if (string.IsNullOrEmpty(pedido.Codigo) || !pedido.CodigoExpiraUtc.HasValue)
                    return ResultadoVerificacion.SinCodigo;

                if (_ahoraUtc() > pedido.CodigoExpiraUtc.Value)
                    return ResultadoVerificacion.Expirado;

                pedido.Intentos++;
                string ingresado = (codigoIngresado ?? "").Trim().Replace(" ", "");

                if (IgualesEnTiempoConstante(pedido.Codigo, ingresado))
                {
                    Quitar(nonce);
                    return ResultadoVerificacion.Ok;
                }

                if (pedido.Intentos >= MaxIntentos)
                {
                    Quitar(nonce);
                    return ResultadoVerificacion.DemasiadosIntentos;
                }

                return ResultadoVerificacion.Incorrecto;
            }
        }

        public void Quitar(string nonce)
        {
            if (string.IsNullOrWhiteSpace(nonce))
                return;

            Pedido descartado;
            _pedidos.TryRemove(nonce, out descartado);
        }

        private void Limpiar()
        {
            var ahora = _ahoraUtc();
            foreach (var par in _pedidos)
            {
                if (ahora - par.Value.PedidoUtc > VidaPedido)
                {
                    Pedido descartado;
                    _pedidos.TryRemove(par.Key, out descartado);
                }
            }
        }

        private static string GenerarNonce()
        {
            byte[] bytes = new byte[24];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        // 6 digitos uniformes: se descartan valores que sesgarian el modulo.
        private static string GenerarCodigoNumerico()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] bytes = new byte[4];
                uint limite = uint.MaxValue - (uint.MaxValue % 1000000u);
                uint valor;
                do
                {
                    rng.GetBytes(bytes);
                    valor = BitConverter.ToUInt32(bytes, 0);
                } while (valor >= limite);

                return (valor % 1000000u).ToString("D6");
            }
        }

        private static bool IgualesEnTiempoConstante(string a, string b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}
