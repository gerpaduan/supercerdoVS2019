using System;
using System.Collections.Generic;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IDispositivoSeguroRepository.
    // dispositivosseguros SI tiene RLS en Postgres (mejora deliberada -- el original en SQL
    // Server no la tiene, mismo criterio ya usado en empresaparametros/cortepuntostocksucursal).
    public class DispositivoSeguroPg : Contratos.IDispositivoSeguroRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public DispositivoSeguroPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private const string SelectBase = @"
                SELECT d.id, d.idempresa, d.numeroserie, d.descripcion, d.creadoutc, d.idusuariocreador,
                       d.origen, d.emailalta, d.bloqueado, d.habilitadofichaje,
                       u.nombre AS nombreusuariocreador
                FROM dispositivosseguros d
                LEFT JOIN usuarios u ON u.id = d.idusuariocreador ";

        private static Entidades.DispositivoSeguro Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.DispositivoSeguro
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                NumeroSerie = Convert.ToString(dr["numeroserie"]),
                Descripcion = dr["descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["descripcion"]),
                CreadoUtc = Convert.ToDateTime(dr["creadoutc"]),
                IdUsuarioCreador = dr["idusuariocreador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idusuariocreador"]),
                NombreUsuarioCreador = dr["nombreusuariocreador"] == DBNull.Value ? "" : Convert.ToString(dr["nombreusuariocreador"]),
                Origen = dr["origen"] == DBNull.Value ? "Manual" : Convert.ToString(dr["origen"]),
                EmailAlta = dr["emailalta"] == DBNull.Value ? "" : Convert.ToString(dr["emailalta"]),
                Bloqueado = dr["bloqueado"] != DBNull.Value && Convert.ToBoolean(dr["bloqueado"]),
                HabilitadoFichaje = dr["habilitadofichaje"] != DBNull.Value && Convert.ToBoolean(dr["habilitadofichaje"])
            };
        }

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE d.idempresa = @idEmpresa ORDER BY d.creadoutc DESC;",
                Mapear,
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public void Agregar(Entidades.DispositivoSeguro dispositivo)
        {
            if (dispositivo == null) throw new ArgumentNullException(nameof(dispositivo));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO dispositivosseguros (idempresa, numeroserie, descripcion, creadoutc, idusuariocreador, origen, emailalta)
                VALUES (@idEmpresa, @numeroSerie, @descripcion, @creadoUtc, @idUsuarioCreador, @origen, @emailAlta);",
                p =>
                {
                    p.AddWithValue("idEmpresa", dispositivo.IdEmpresa);
                    p.AddWithValue("numeroSerie", dispositivo.NumeroSerie ?? "");
                    p.AddWithValue("descripcion", (object)dispositivo.Descripcion ?? DBNull.Value);
                    p.AddWithValue("creadoUtc", dispositivo.CreadoUtc);
                    p.AddWithValue("idUsuarioCreador", (object)dispositivo.IdUsuarioCreador ?? DBNull.Value);
                    p.AddWithValue("origen", string.IsNullOrWhiteSpace(dispositivo.Origen) ? "Manual" : dispositivo.Origen);
                    p.AddWithValue("emailAlta", string.IsNullOrWhiteSpace(dispositivo.EmailAlta) ? (object)DBNull.Value : dispositivo.EmailAlta);
                });
        }

        public void Eliminar(int id, int idEmpresa)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM dispositivosseguros WHERE id = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });
        }

        public void SetBloqueado(int id, int idEmpresa, bool bloqueado)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE dispositivosseguros SET bloqueado = @bloqueado WHERE id = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("bloqueado", bloqueado);
                });
        }

        public Entidades.DispositivoSeguro ObtenerPorSerie(string numeroSerie, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(numeroSerie))
                return null;

            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectBase + "WHERE d.idempresa = @idEmpresa AND d.numeroserie = @numeroSerie;",
                Mapear,
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("numeroSerie", numeroSerie.Trim());
                });

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
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE dispositivosseguros SET habilitadofichaje = @habilitado WHERE id = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("habilitado", habilitado);
                });
        }

        // Usado en el fichaje: un dispositivo bloqueado o no habilitado para fichaje NO sirve,
        // aunque este dado de alta como "seguro".
        public bool EsFichajeHabilitado(string numeroSerie, int idEmpresa)
        {
            var dispositivo = ObtenerPorSerie(numeroSerie, idEmpresa);
            return dispositivo != null && !dispositivo.Bloqueado && dispositivo.HabilitadoFichaje;
        }

        // ---- Solicitudes de autorizacion de dispositivo (2026-10-02, Fase 1c) ----

        private const string SelectSolicitud = @"
                SELECT s.id, s.idempresa, s.idusuario, s.serie, s.nombre, s.mensaje, s.ip, s.estado,
                       s.creadautc, s.resueltapor, s.resueltautc,
                       u.nombre AS nombreusuario, u.usuario AS usuario
                FROM dispositivosseguros_solicitudes s
                LEFT JOIN usuarios u ON u.id = s.idusuario ";

        private static Entidades.DispositivoSolicitud MapearSolicitud(System.Data.IDataRecord dr)
        {
            return new Entidades.DispositivoSolicitud
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                NombreUsuario = dr["nombreusuario"] == DBNull.Value ? "" : Convert.ToString(dr["nombreusuario"]),
                Usuario = dr["usuario"] == DBNull.Value ? "" : Convert.ToString(dr["usuario"]),
                Serie = Convert.ToString(dr["serie"]),
                Nombre = Convert.ToString(dr["nombre"]),
                Mensaje = dr["mensaje"] == DBNull.Value ? "" : Convert.ToString(dr["mensaje"]),
                Ip = dr["ip"] == DBNull.Value ? "" : Convert.ToString(dr["ip"]),
                Estado = Convert.ToString(dr["estado"]),
                CreadaUtc = Convert.ToDateTime(dr["creadautc"]),
                ResueltaPor = dr["resueltapor"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["resueltapor"]),
                ResueltaUtc = dr["resueltautc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["resueltautc"])
            };
        }

        // Si ya hay una pendiente de la misma serie, la actualiza en vez de duplicar (el usuario puede
        // volver a tocar el boton o corregir el nombre). Dos pasos en una conexion con tenant.
        public int CrearSolicitud(Entidades.DispositivoSolicitud solicitud)
        {
            if (solicitud == null) throw new ArgumentNullException(nameof(solicitud));

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    int id;
                    using (var cmd = new Npgsql.NpgsqlCommand(
                        "SELECT id FROM dispositivosseguros_solicitudes WHERE idempresa = @idEmpresa AND serie = @serie AND estado = 'pendiente' ORDER BY id LIMIT 1;", con, tx))
                    {
                        cmd.Parameters.AddWithValue("idEmpresa", solicitud.IdEmpresa);
                        cmd.Parameters.AddWithValue("serie", solicitud.Serie ?? "");
                        object existente = cmd.ExecuteScalar();
                        id = existente == null || existente == DBNull.Value ? 0 : Convert.ToInt32(existente);
                    }

                    if (id > 0)
                    {
                        using (var cmd = new Npgsql.NpgsqlCommand(@"
                            UPDATE dispositivosseguros_solicitudes
                            SET idusuario = @idUsuario, nombre = @nombre, mensaje = @mensaje, ip = @ip, creadautc = @creadaUtc
                            WHERE id = @id;", con, tx))
                        {
                            cmd.Parameters.AddWithValue("id", id);
                            AgregarParamsSolicitud(cmd, solicitud);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmd = new Npgsql.NpgsqlCommand(@"
                            INSERT INTO dispositivosseguros_solicitudes (idempresa, idusuario, serie, nombre, mensaje, ip, estado, creadautc)
                            VALUES (@idEmpresa, @idUsuario, @serie, @nombre, @mensaje, @ip, 'pendiente', @creadaUtc)
                            RETURNING id;", con, tx))
                        {
                            cmd.Parameters.AddWithValue("idEmpresa", solicitud.IdEmpresa);
                            cmd.Parameters.AddWithValue("serie", solicitud.Serie ?? "");
                            AgregarParamsSolicitud(cmd, solicitud);
                            id = Convert.ToInt32(cmd.ExecuteScalar());
                        }
                    }

                    tx.Commit();
                    return id;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        private static void AgregarParamsSolicitud(Npgsql.NpgsqlCommand cmd, Entidades.DispositivoSolicitud s)
        {
            cmd.Parameters.AddWithValue("idUsuario", s.IdUsuario);
            cmd.Parameters.AddWithValue("nombre", s.Nombre ?? "");
            cmd.Parameters.AddWithValue("mensaje", string.IsNullOrWhiteSpace(s.Mensaje) ? (object)DBNull.Value : s.Mensaje);
            cmd.Parameters.AddWithValue("ip", string.IsNullOrWhiteSpace(s.Ip) ? (object)DBNull.Value : s.Ip);
            cmd.Parameters.AddWithValue("creadaUtc", s.CreadaUtc);
        }

        public List<Entidades.DispositivoSolicitud> ListarSolicitudesPendientes(int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                SelectSolicitud + "WHERE s.idempresa = @idEmpresa AND s.estado = 'pendiente' ORDER BY s.creadautc DESC;",
                MapearSolicitud,
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public Entidades.DispositivoSolicitud ObtenerSolicitud(int id, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                SelectSolicitud + "WHERE s.id = @id AND s.idempresa = @idEmpresa;",
                MapearSolicitud,
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });

            return lista.Count > 0 ? lista[0] : null;
        }

        public void ResolverSolicitud(int id, int idEmpresa, string estado, int idUsuarioResuelve)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE dispositivosseguros_solicitudes
                SET estado = @estado, resueltapor = @resueltaPor, resueltautc = @resueltaUtc
                WHERE id = @id AND idempresa = @idEmpresa AND estado = 'pendiente';",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("estado", estado);
                    p.AddWithValue("resueltaPor", idUsuarioResuelve);
                    p.AddWithValue("resueltaUtc", DateTime.UtcNow);
                });
        }
    }
}
