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
                       d.origen, d.emailalta, d.bloqueado,
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
                Bloqueado = dr["bloqueado"] != DBNull.Value && Convert.ToBoolean(dr["bloqueado"])
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
    }
}
