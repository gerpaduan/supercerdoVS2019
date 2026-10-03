using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Negocio
{
    // IP real del cliente detras de un reverse proxy (2026-10-02, Fase 4 de docs/DECISIONS.md "Login
    // por CUIT, clave rapida (PIN) y politica de clave").
    //
    // Problema que resuelve: el login usaba la PRIMERA IP de X-Forwarded-For venga de donde venga. Un
    // atacante puede mandar su propio "X-Forwarded-For: 1.2.3.4" y cambiarlo en cada intento: la app
    // creia que eran IPs distintas y el limite por IP (LoginRateLimiter) nunca se activaba.
    //
    // Regla: X-Forwarded-For solo se mira si la conexion directa viene de un PROXY DE CONFIANZA
    // (loopback, redes privadas -- donde viven Caddy/IIS+ARR/el contenedor delante de la app -- y lo
    // que se agregue en Security:TrustedProxies). Aun asi no se toma la primera entrada (la puede
    // haber puesto el cliente) sino la primera NO confiable leyendo de derecha a izquierda: cada
    // proxy confiable agrega a la derecha la IP desde la que le llego la conexion. Si la conexion no
    // viene de un proxy de confianza, el header se ignora y se usa la IP de la conexion. La falla es
    // segura: ante la duda se usa la IP de la conexion directa, nunca un valor del header.
    public static class ClientIpResolver
    {
        // Rangos "privados" confiables por defecto (CIDR). Un proxy publico (balanceador, CDN) hay que
        // agregarlo a mano en Security:TrustedProxies del host que lo use.
        private static readonly string[] RangosPorDefecto =
        {
            "127.0.0.0/8", "::1/128",
            "10.0.0.0/8", "172.16.0.0/12", "192.168.0.0/16",
            "169.254.0.0/16", "fc00::/7", "fe80::/10"
        };

        // remote: IP de la conexion directa. forwardedFor: valor crudo del header X-Forwarded-For
        // (puede ser vacio). proxiesExtra: entradas adicionales "ip" o "ip/prefijo" de configuracion.
        // Devuelve la IP del cliente como texto, o "unknown" si no hay ninguna.
        public static string Resolve(IPAddress remote, string forwardedFor, IEnumerable<string> proxiesExtra = null)
        {
            if (remote == null)
                return "unknown";

            var redes = ConstruirRedesConfiables(proxiesExtra);
            string ipConexion = Normalizar(remote).ToString();

            if (!EsConfiable(Normalizar(remote), redes) || string.IsNullOrWhiteSpace(forwardedFor))
                return ipConexion;

            string[] entradas = forwardedFor.Split(',');
            for (int i = entradas.Length - 1; i >= 0; i--)
            {
                IPAddress ip;
                if (!TryParseIp(entradas[i], out ip))
                    return ipConexion; // entrada ilegible: no se confia en nada de este header

                ip = Normalizar(ip);
                if (!EsConfiable(ip, redes))
                    return ip.ToString();
            }

            return ipConexion;
        }

        // Entradas de configuracion: separadas por coma, punto y coma o espacio.
        public static IEnumerable<string> ParsearConfiguracion(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return new string[0];

            return valor.Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private sealed class Red
        {
            public byte[] Base;
            public int Prefijo;
        }

        private static List<Red> ConstruirRedesConfiables(IEnumerable<string> extra)
        {
            var redes = new List<Red>();
            foreach (string cidr in RangosPorDefecto)
                AgregarRed(redes, cidr);

            if (extra != null)
            {
                foreach (string entrada in extra)
                    AgregarRed(redes, entrada);
            }

            return redes;
        }

        private static void AgregarRed(List<Red> redes, string entrada)
        {
            if (string.IsNullOrWhiteSpace(entrada))
                return;

            string texto = entrada.Trim();
            int prefijo = -1;
            int barra = texto.IndexOf('/');
            if (barra >= 0)
            {
                if (!int.TryParse(texto.Substring(barra + 1), out prefijo) || prefijo < 0)
                    return;
                texto = texto.Substring(0, barra);
            }

            IPAddress ip;
            if (!IPAddress.TryParse(texto, out ip))
                return;

            ip = Normalizar(ip);
            byte[] bytes = ip.GetAddressBytes();
            if (prefijo < 0)
                prefijo = bytes.Length * 8; // IP suelta = host unico
            if (prefijo > bytes.Length * 8)
                return;

            redes.Add(new Red { Base = bytes, Prefijo = prefijo });
        }

        private static bool EsConfiable(IPAddress ip, List<Red> redes)
        {
            byte[] bytes = ip.GetAddressBytes();
            foreach (var red in redes)
            {
                if (red.Base.Length != bytes.Length)
                    continue;

                if (CoincidePrefijo(bytes, red.Base, red.Prefijo))
                    return true;
            }

            return false;
        }

        private static bool CoincidePrefijo(byte[] a, byte[] b, int prefijoBits)
        {
            int bytesCompletos = prefijoBits / 8;
            for (int i = 0; i < bytesCompletos; i++)
            {
                if (a[i] != b[i])
                    return false;
            }

            int bitsRestantes = prefijoBits % 8;
            if (bitsRestantes == 0)
                return true;

            int mascara = 0xFF << (8 - bitsRestantes) & 0xFF;
            return (a[bytesCompletos] & mascara) == (b[bytesCompletos] & mascara);
        }

        // "1.2.3.4", "1.2.3.4:5678", "[::1]:5678", "::1".
        private static bool TryParseIp(string texto, out IPAddress ip)
        {
            ip = null;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            texto = texto.Trim();

            if (texto.StartsWith("[", StringComparison.Ordinal))
            {
                int cierre = texto.IndexOf(']');
                if (cierre < 0) return false;
                texto = texto.Substring(1, cierre - 1);
            }
            else
            {
                int primerosDosPuntos = texto.IndexOf(':');
                if (primerosDosPuntos >= 0 && primerosDosPuntos == texto.LastIndexOf(':'))
                    texto = texto.Substring(0, primerosDosPuntos); // IPv4 con puerto
            }

            return IPAddress.TryParse(texto, out ip);
        }

        // ::ffff:10.0.0.5 (IPv4 mapeada en IPv6, lo que da Kestrel con dual-stack) -> 10.0.0.5.
        private static IPAddress Normalizar(IPAddress ip)
        {
            if (ip.AddressFamily == AddressFamily.InterNetworkV6 && ip.IsIPv4MappedToIPv6)
                return ip.MapToIPv4();

            return ip;
        }
    }
}
