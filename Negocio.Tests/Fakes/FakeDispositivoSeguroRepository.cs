using System;
using System.Collections.Generic;
using System.Linq;

namespace NegocioTests.Fakes
{
    // Repositorio de dispositivos seguros en memoria para los tests de solicitudes de autorizacion.
    public sealed class FakeDispositivoSeguroRepository : Contratos.IDispositivoSeguroRepository
    {
        public readonly List<Entidades.DispositivoSeguro> Dispositivos = new List<Entidades.DispositivoSeguro>();
        public readonly List<Entidades.DispositivoSolicitud> Solicitudes = new List<Entidades.DispositivoSolicitud>();
        private int _proximoId = 1;

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa) => Dispositivos.Where(d => d.IdEmpresa == idEmpresa).ToList();
        public void Agregar(Entidades.DispositivoSeguro dispositivo) { dispositivo.Id = _proximoId++; Dispositivos.Add(dispositivo); }
        public void Eliminar(int id, int idEmpresa) => Dispositivos.RemoveAll(d => d.Id == id && d.IdEmpresa == idEmpresa);
        public bool ExisteSerieSegura(string numeroSerie, int idEmpresa) { var d = ObtenerPorSerie(numeroSerie, idEmpresa); return d != null && !d.Bloqueado; }
        public Entidades.DispositivoSeguro ObtenerPorSerie(string numeroSerie, int idEmpresa) => Dispositivos.FirstOrDefault(d => d.IdEmpresa == idEmpresa && d.NumeroSerie == numeroSerie);
        public void SetBloqueado(int id, int idEmpresa, bool bloqueado) { var d = Dispositivos.First(x => x.Id == id); d.Bloqueado = bloqueado; }
        public void SetHabilitadoFichaje(int id, int idEmpresa, bool habilitado) { var d = Dispositivos.First(x => x.Id == id); d.HabilitadoFichaje = habilitado; }
        public bool EsFichajeHabilitado(string numeroSerie, int idEmpresa) { var d = ObtenerPorSerie(numeroSerie, idEmpresa); return d != null && !d.Bloqueado && d.HabilitadoFichaje; }

        public int CrearSolicitud(Entidades.DispositivoSolicitud solicitud)
        {
            var existente = Solicitudes.FirstOrDefault(s => s.IdEmpresa == solicitud.IdEmpresa && s.Serie == solicitud.Serie && s.Estado == Entidades.DispositivoSolicitud.EstadoPendiente);
            if (existente != null)
            {
                existente.IdUsuario = solicitud.IdUsuario;
                existente.Nombre = solicitud.Nombre;
                existente.Mensaje = solicitud.Mensaje;
                return existente.Id;
            }

            solicitud.Id = _proximoId++;
            Solicitudes.Add(solicitud);
            return solicitud.Id;
        }

        public List<Entidades.DispositivoSolicitud> ListarSolicitudesPendientes(int idEmpresa) =>
            Solicitudes.Where(s => s.IdEmpresa == idEmpresa && s.Estado == Entidades.DispositivoSolicitud.EstadoPendiente).ToList();

        public Entidades.DispositivoSolicitud ObtenerSolicitud(int id, int idEmpresa) => Solicitudes.FirstOrDefault(s => s.Id == id && s.IdEmpresa == idEmpresa);

        public void ResolverSolicitud(int id, int idEmpresa, string estado, int idUsuarioResuelve)
        {
            var s = Solicitudes.FirstOrDefault(x => x.Id == id && x.IdEmpresa == idEmpresa && x.Estado == Entidades.DispositivoSolicitud.EstadoPendiente);
            if (s == null) return;
            s.Estado = estado;
            s.ResueltaPor = idUsuarioResuelve;
            s.ResueltaUtc = DateTime.UtcNow;
        }
    }
}
