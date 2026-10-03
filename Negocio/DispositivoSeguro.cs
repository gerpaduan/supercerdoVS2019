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

        // ---------------------------------------------------------------------------------
        // Solicitudes de autorizacion al administrador (2026-10-02, ver docs/DECISIONS.md "Login por
        // CUIT, clave rapida (PIN) y politica de clave", Fase 1c). El usuario (que ya puso bien su
        // clave) pide autorizar su dispositivo; el admin aprueba (se da de alta como dispositivo
        // seguro, Origen="Solicitud") o rechaza.
        // ---------------------------------------------------------------------------------

        public const string OrigenSolicitud = "Solicitud";
        public const int LargoMaximoNombreSolicitud = 100;
        public const int LargoMaximoMensajeSolicitud = 500;

        // Devuelve el Id (de la nueva solicitud o de la pendiente existente para esa serie). Lanza
        // ArgumentException con mensaje para el usuario si el nombre no es valido.
        public int CrearSolicitud(Entidades.DispositivoSolicitud solicitud)
        {
            if (solicitud == null) throw new ArgumentNullException(nameof(solicitud));
            if (solicitud.IdEmpresa <= 0 || solicitud.IdUsuario <= 0 || string.IsNullOrWhiteSpace(solicitud.Serie))
                throw new ArgumentException("La solicitud está incompleta.", nameof(solicitud));

            solicitud.Nombre = (solicitud.Nombre ?? "").Trim();
            if (solicitud.Nombre.Length == 0 || solicitud.Nombre.Length > LargoMaximoNombreSolicitud)
                throw new ArgumentException("Ingresá un nombre para este dispositivo (máximo " + LargoMaximoNombreSolicitud + " caracteres).");

            solicitud.Mensaje = (solicitud.Mensaje ?? "").Trim();
            if (solicitud.Mensaje.Length > LargoMaximoMensajeSolicitud)
                solicitud.Mensaje = solicitud.Mensaje.Substring(0, LargoMaximoMensajeSolicitud);

            solicitud.Serie = solicitud.Serie.Trim();
            solicitud.Estado = Entidades.DispositivoSolicitud.EstadoPendiente;
            solicitud.CreadaUtc = DateTime.UtcNow;
            return oDispositivoD.CrearSolicitud(solicitud);
        }

        public List<Entidades.DispositivoSolicitud> ListarSolicitudesPendientes(int idEmpresa)
        {
            return oDispositivoD.ListarSolicitudesPendientes(idEmpresa);
        }

        // Aprueba: da de alta el dispositivo (si todavia no existe) y marca la solicitud como
        // aprobada. descripcion: nombre final del dispositivo (el admin puede editarlo antes de
        // aprobar; vacio = el que puso el usuario). Devuelve false si la solicitud ya no esta
        // pendiente (otro admin la resolvio) o no existe.
        public bool AprobarSolicitud(int idSolicitud, int idEmpresa, int idUsuarioAdmin, string descripcion)
        {
            var solicitud = oDispositivoD.ObtenerSolicitud(idSolicitud, idEmpresa);
            if (solicitud == null || solicitud.Estado != Entidades.DispositivoSolicitud.EstadoPendiente)
                return false;

            var existente = oDispositivoD.ObtenerPorSerie(solicitud.Serie, idEmpresa);
            if (existente == null)
            {
                string nombre = string.IsNullOrWhiteSpace(descripcion) ? solicitud.Nombre : descripcion.Trim();
                Agregar(new Entidades.DispositivoSeguro
                {
                    IdEmpresa = idEmpresa,
                    NumeroSerie = solicitud.Serie,
                    Descripcion = nombre.Length > LargoMaximoNombreSolicitud ? nombre.Substring(0, LargoMaximoNombreSolicitud) : nombre,
                    IdUsuarioCreador = idUsuarioAdmin,
                    Origen = OrigenSolicitud
                });
            }
            else if (existente.Bloqueado)
            {
                // Un dispositivo que el admin bloqueo no se re-autoriza por una solicitud: tiene que
                // desbloquearlo a proposito desde la pantalla de dispositivos.
                return false;
            }

            oDispositivoD.ResolverSolicitud(idSolicitud, idEmpresa, Entidades.DispositivoSolicitud.EstadoAprobada, idUsuarioAdmin);
            return true;
        }

        public bool RechazarSolicitud(int idSolicitud, int idEmpresa, int idUsuarioAdmin)
        {
            var solicitud = oDispositivoD.ObtenerSolicitud(idSolicitud, idEmpresa);
            if (solicitud == null || solicitud.Estado != Entidades.DispositivoSolicitud.EstadoPendiente)
                return false;

            oDispositivoD.ResolverSolicitud(idSolicitud, idEmpresa, Entidades.DispositivoSolicitud.EstadoRechazada, idUsuarioAdmin);
            return true;
        }

        public Entidades.DispositivoSolicitud ObtenerSolicitud(int idSolicitud, int idEmpresa)
        {
            return oDispositivoD.ObtenerSolicitud(idSolicitud, idEmpresa);
        }

        public void SetBloqueado(int id, int idEmpresa, bool bloqueado)
        {
            oDispositivoD.SetBloqueado(id, idEmpresa, bloqueado);
        }

        public void SetHabilitadoFichaje(int id, int idEmpresa, bool habilitado)
        {
            oDispositivoD.SetHabilitadoFichaje(id, idEmpresa, habilitado);
        }

        // true solo si existe, no esta bloqueado Y esta habilitado para fichaje.
        public bool EsFichajeHabilitado(string numeroSerie, int idEmpresa)
        {
            return oDispositivoD.EsFichajeHabilitado(numeroSerie, idEmpresa);
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
