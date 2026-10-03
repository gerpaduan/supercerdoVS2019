using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class DispositivoSeguro : Contratos.IDispositivoSeguroRepository
    {
        private readonly IEmpresaContext _empresa;

        public DispositivoSeguro(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        private const string SelectBase = @"
                SELECT d.Id, d.IdEmpresa, d.NumeroSerie, d.Descripcion, d.CreadoUtc, d.IdUsuarioCreador,
                       d.Origen, d.EmailAlta, d.Bloqueado, d.HabilitadoFichaje,
                       u.nombre AS NombreUsuarioCreador
                FROM DispositivosSeguros d
                LEFT JOIN Usuarios u ON u.id = d.IdUsuarioCreador ";

        private static Entidades.DispositivoSeguro Mapear(IDataRecord dr)
        {
            return new Entidades.DispositivoSeguro
            {
                Id = Convert.ToInt32(dr["Id"]),
                IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                NumeroSerie = Convert.ToString(dr["NumeroSerie"]),
                Descripcion = dr["Descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["Descripcion"]),
                CreadoUtc = Convert.ToDateTime(dr["CreadoUtc"]),
                IdUsuarioCreador = dr["IdUsuarioCreador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdUsuarioCreador"]),
                NombreUsuarioCreador = dr["NombreUsuarioCreador"] == DBNull.Value ? "" : Convert.ToString(dr["NombreUsuarioCreador"]),
                Origen = dr["Origen"] == DBNull.Value ? "Manual" : Convert.ToString(dr["Origen"]),
                EmailAlta = dr["EmailAlta"] == DBNull.Value ? "" : Convert.ToString(dr["EmailAlta"]),
                Bloqueado = dr["Bloqueado"] != DBNull.Value && Convert.ToBoolean(dr["Bloqueado"]),
                HabilitadoFichaje = dr["HabilitadoFichaje"] != DBNull.Value && Convert.ToBoolean(dr["HabilitadoFichaje"])
            };
        }

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa)
        {
            return Db.Reader(
                _empresa,
                SelectBase + "WHERE d.IdEmpresa = @idEmpresa ORDER BY d.CreadoUtc DESC;",
                CommandType.Text,
                map: Mapear,
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa
            );
        }

        public void Agregar(Entidades.DispositivoSeguro dispositivo)
        {
            if (dispositivo == null) throw new ArgumentNullException(nameof(dispositivo));

            const string sql = @"
                INSERT INTO DispositivosSeguros (IdEmpresa, NumeroSerie, Descripcion, CreadoUtc, IdUsuarioCreador, Origen, EmailAlta)
                VALUES (@idEmpresa, @numeroSerie, @descripcion, @creadoUtc, @idUsuarioCreador, @origen, @emailAlta);";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = dispositivo.IdEmpresa;
                    p.Add("@numeroSerie", SqlDbType.NVarChar, 200).Value = dispositivo.NumeroSerie ?? "";
                    p.Add("@descripcion", SqlDbType.NVarChar, 200).Value = (object)dispositivo.Descripcion ?? DBNull.Value;
                    p.Add("@creadoUtc", SqlDbType.DateTime2).Value = dispositivo.CreadoUtc;
                    p.Add("@idUsuarioCreador", SqlDbType.Int).Value = (object)dispositivo.IdUsuarioCreador ?? DBNull.Value;
                    p.Add("@origen", SqlDbType.NVarChar, 20).Value = string.IsNullOrWhiteSpace(dispositivo.Origen) ? "Manual" : dispositivo.Origen;
                    p.Add("@emailAlta", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(dispositivo.EmailAlta) ? (object)DBNull.Value : dispositivo.EmailAlta;
                }
            );
        }

        public void Eliminar(int id, int idEmpresa)
        {
            const string sql = "DELETE FROM DispositivosSeguros WHERE Id = @id AND IdEmpresa = @idEmpresa;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                }
            );
        }

        public void SetBloqueado(int id, int idEmpresa, bool bloqueado)
        {
            const string sql = "UPDATE DispositivosSeguros SET Bloqueado = @bloqueado WHERE Id = @id AND IdEmpresa = @idEmpresa;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@bloqueado", SqlDbType.Bit).Value = bloqueado;
                }
            );
        }

        public Entidades.DispositivoSeguro ObtenerPorSerie(string numeroSerie, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(numeroSerie))
                return null;

            var lista = Db.Reader(
                _empresa,
                SelectBase + "WHERE d.IdEmpresa = @idEmpresa AND d.NumeroSerie = @numeroSerie;",
                CommandType.Text,
                map: Mapear,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@numeroSerie", SqlDbType.NVarChar, 200).Value = numeroSerie.Trim();
                }
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        // Usado en el login: un dispositivo bloqueado por el admin NO cuenta como seguro.
        public bool ExisteSerieSegura(string numeroSerie, int idEmpresa)
        {
            var dispositivo = ObtenerPorSerie(numeroSerie, idEmpresa);
            return dispositivo != null && !dispositivo.Bloqueado;
        }

        public void SetHabilitadoFichaje(int id, int idEmpresa, bool habilitado)
        {
            const string sql = "UPDATE DispositivosSeguros SET HabilitadoFichaje = @habilitado WHERE Id = @id AND IdEmpresa = @idEmpresa;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@habilitado", SqlDbType.Bit).Value = habilitado;
                }
            );
        }

        // Usado en el fichaje: un dispositivo bloqueado o no habilitado para fichaje NO sirve,
        // aunque este dado de alta como "seguro".
        public bool EsFichajeHabilitado(string numeroSerie, int idEmpresa)
        {
            var dispositivo = ObtenerPorSerie(numeroSerie, idEmpresa);
            return dispositivo != null && !dispositivo.Bloqueado && dispositivo.HabilitadoFichaje;
        }

        // ---- Solicitudes de autorizacion de dispositivo (2026-10-02, Fase 1c) ----
        // Tabla DispositivosSeguros_Solicitudes (script 20261002b-Create_DispositivosSeguros_Solicitudes.sql).

        private const string SelectSolicitud = @"
                SELECT s.Id, s.IdEmpresa, s.IdUsuario, s.Serie, s.Nombre, s.Mensaje, s.Ip, s.Estado,
                       s.CreadaUtc, s.ResueltaPor, s.ResueltaUtc,
                       u.nombre AS NombreUsuario, u.usuario AS Usuario
                FROM DispositivosSeguros_Solicitudes s
                LEFT JOIN Usuarios u ON u.id = s.IdUsuario ";

        private static Entidades.DispositivoSolicitud MapearSolicitud(IDataRecord dr)
        {
            return new Entidades.DispositivoSolicitud
            {
                Id = Convert.ToInt32(dr["Id"]),
                IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                NombreUsuario = dr["NombreUsuario"] == DBNull.Value ? "" : Convert.ToString(dr["NombreUsuario"]),
                Usuario = dr["Usuario"] == DBNull.Value ? "" : Convert.ToString(dr["Usuario"]),
                Serie = Convert.ToString(dr["Serie"]),
                Nombre = Convert.ToString(dr["Nombre"]),
                Mensaje = dr["Mensaje"] == DBNull.Value ? "" : Convert.ToString(dr["Mensaje"]),
                Ip = dr["Ip"] == DBNull.Value ? "" : Convert.ToString(dr["Ip"]),
                Estado = Convert.ToString(dr["Estado"]),
                CreadaUtc = Convert.ToDateTime(dr["CreadaUtc"]),
                ResueltaPor = dr["ResueltaPor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["ResueltaPor"]),
                ResueltaUtc = dr["ResueltaUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["ResueltaUtc"])
            };
        }

        // Si ya hay una pendiente de la misma serie, la actualiza en vez de duplicar.
        public int CrearSolicitud(Entidades.DispositivoSolicitud solicitud)
        {
            if (solicitud == null) throw new ArgumentNullException(nameof(solicitud));

            object existente = Db.Scalar(
                _empresa,
                "SELECT TOP 1 Id FROM DispositivosSeguros_Solicitudes WHERE IdEmpresa = @idEmpresa AND Serie = @serie AND Estado = 'pendiente' ORDER BY Id;",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = solicitud.IdEmpresa;
                    p.Add("@serie", SqlDbType.NVarChar, 200).Value = solicitud.Serie ?? "";
                });

            Action<SqlParameterCollection> comunes = p =>
            {
                p.Add("@idUsuario", SqlDbType.Int).Value = solicitud.IdUsuario;
                p.Add("@nombre", SqlDbType.NVarChar, 100).Value = solicitud.Nombre ?? "";
                p.Add("@mensaje", SqlDbType.NVarChar, 500).Value = string.IsNullOrWhiteSpace(solicitud.Mensaje) ? (object)DBNull.Value : solicitud.Mensaje;
                p.Add("@ip", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(solicitud.Ip) ? (object)DBNull.Value : solicitud.Ip;
                p.Add("@creadaUtc", SqlDbType.DateTime2).Value = solicitud.CreadaUtc;
            };

            if (existente != null && existente != DBNull.Value)
            {
                int id = Convert.ToInt32(existente);
                Db.NonQuery(
                    _empresa,
                    @"UPDATE DispositivosSeguros_Solicitudes
                      SET IdUsuario = @idUsuario, Nombre = @nombre, Mensaje = @mensaje, Ip = @ip, CreadaUtc = @creadaUtc
                      WHERE Id = @id;",
                    CommandType.Text,
                    setParams: p =>
                    {
                        p.Add("@id", SqlDbType.Int).Value = id;
                        comunes(p);
                    });
                return id;
            }

            object nuevo = Db.Scalar(
                _empresa,
                @"INSERT INTO DispositivosSeguros_Solicitudes (IdEmpresa, IdUsuario, Serie, Nombre, Mensaje, Ip, Estado, CreadaUtc)
                  VALUES (@idEmpresa, @idUsuario, @serie, @nombre, @mensaje, @ip, 'pendiente', @creadaUtc);
                  SELECT CAST(SCOPE_IDENTITY() AS int);",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = solicitud.IdEmpresa;
                    p.Add("@serie", SqlDbType.NVarChar, 200).Value = solicitud.Serie ?? "";
                    comunes(p);
                });
            return Convert.ToInt32(nuevo);
        }

        public List<Entidades.DispositivoSolicitud> ListarSolicitudesPendientes(int idEmpresa)
        {
            return Db.Reader(
                _empresa,
                SelectSolicitud + "WHERE s.IdEmpresa = @idEmpresa AND s.Estado = 'pendiente' ORDER BY s.CreadaUtc DESC;",
                CommandType.Text,
                map: MapearSolicitud,
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa
            );
        }

        public Entidades.DispositivoSolicitud ObtenerSolicitud(int id, int idEmpresa)
        {
            var lista = Db.Reader(
                _empresa,
                SelectSolicitud + "WHERE s.Id = @id AND s.IdEmpresa = @idEmpresa;",
                CommandType.Text,
                map: MapearSolicitud,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                }
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        public void ResolverSolicitud(int id, int idEmpresa, string estado, int idUsuarioResuelve)
        {
            Db.NonQuery(
                _empresa,
                @"UPDATE DispositivosSeguros_Solicitudes
                  SET Estado = @estado, ResueltaPor = @resueltaPor, ResueltaUtc = SYSUTCDATETIME()
                  WHERE Id = @id AND IdEmpresa = @idEmpresa AND Estado = 'pendiente';",
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@estado", SqlDbType.NVarChar, 20).Value = estado;
                    p.Add("@resueltaPor", SqlDbType.Int).Value = idUsuarioResuelve;
                }
            );
        }
    }
}
