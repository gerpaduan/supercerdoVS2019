using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Utilidades;

namespace Negocio
{
    public class DispositivoSeguro
    {
        // Prefijo del "numero de serie" de un dispositivo identificado por la cookie del navegador
        // (celular / PC sin agente) -- se distingue del CPU ID que informa el agente de impresion.
        public const string PrefijoToken = "web:";

        private readonly Contratos.IDispositivoSeguroRepository oDispositivoD;

        public DispositivoSeguro(IEmpresaContext empresa)
        {
            oDispositivoD = new Datos.DispositivoSeguro(empresa);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de
        // IDispositivoSeguroRepository (ej. DatosPostgres.DispositivoSeguroPg).
        public DispositivoSeguro(Contratos.IDispositivoSeguroRepository repositorio)
        {
            oDispositivoD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        // Convierte el token secreto de la cookie del navegador en el "numero de serie" que se
        // guarda en BD: solo el hash SHA-256, nunca el token en claro (si alguien lee la tabla no
        // puede reconstruir la cookie de nadie).
        public static string SerieDeToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return "";

            using (var sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(token.Trim()));
                var sb = new StringBuilder(PrefijoToken, PrefijoToken.Length + 64);
                foreach (byte b in hash) sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa)
        {
            return oDispositivoD.Listar(idEmpresa);
        }

        public void Agregar(Entidades.DispositivoSeguro dispositivo)
        {
            if (dispositivo == null) throw new ArgumentNullException(nameof(dispositivo));
            if (string.IsNullOrWhiteSpace(dispositivo.NumeroSerie))
                throw new ArgumentException("El número de serie es obligatorio.", nameof(dispositivo));

            dispositivo.NumeroSerie = dispositivo.NumeroSerie.Trim();
            dispositivo.CreadoUtc = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(dispositivo.Origen)) dispositivo.Origen = "Manual";
            oDispositivoD.Agregar(dispositivo);
        }

        public void Eliminar(int id, int idEmpresa)
        {
            oDispositivoD.Eliminar(id, idEmpresa);
        }

        public void SetBloqueado(int id, int idEmpresa, bool bloqueado)
        {
            oDispositivoD.SetBloqueado(id, idEmpresa, bloqueado);
        }

        // true solo si existe y NO esta bloqueado.
        public bool ExisteSerieSegura(string numeroSerie, int idEmpresa)
        {
            return oDispositivoD.ExisteSerieSegura(numeroSerie, idEmpresa);
        }

        public Entidades.DispositivoSeguro ObtenerPorSerie(string numeroSerie, int idEmpresa)
        {
            return oDispositivoD.ObtenerPorSerie(numeroSerie, idEmpresa);
        }
    }
}
