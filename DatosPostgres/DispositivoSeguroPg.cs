using System;
using System.Collections.Generic;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IDispositivoSeguroRepository (4/4 metodos).
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

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT d.id, d.idempresa, d.numeroserie, d.descripcion, d.creadoutc, d.idusuariocreador,
                       u.nombre AS nombreusuariocreador
                FROM dispositivosseguros d
                LEFT JOIN usuarios u ON u.id = d.idusuariocreador
                WHERE d.idempresa = @idEmpresa
                ORDER BY d.creadoutc DESC;",
                dr => new Entidades.DispositivoSeguro
                {
                    Id = Convert.ToInt32(dr["id"]),
                    IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                    NumeroSerie = Convert.ToString(dr["numeroserie"]),
                    Descripcion = dr["descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["descripcion"]),
                    CreadoUtc = Convert.ToDateTime(dr["creadoutc"]),
                    IdUsuarioCreador = dr["idusuariocreador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idusuariocreador"]),
                    NombreUsuarioCreador = dr["nombreusuariocreador"] == DBNull.Value ? "" : Convert.ToString(dr["nombreusuariocreador"])
                },
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public void Agregar(Entidades.DispositivoSeguro dispositivo)
        {
            if (dispositivo == null) throw new ArgumentNullException(nameof(dispositivo));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO dispositivosseguros (idempresa, numeroserie, descripcion, creadoutc, idusuariocreador)
                VALUES (@idEmpresa, @numeroSerie, @descripcion, @creadoUtc, @idUsuarioCreador);",
                p =>
                {
                    p.AddWithValue("idEmpresa", dispositivo.IdEmpresa);
                    p.AddWithValue("numeroSerie", dispositivo.NumeroSerie ?? "");
                    p.AddWithValue("descripcion", (object)dispositivo.Descripcion ?? DBNull.Value);
                    p.AddWithValue("creadoUtc", dispositivo.CreadoUtc);
                    p.AddWithValue("idUsuarioCreador", (object)dispositivo.IdUsuarioCreador ?? DBNull.Value);
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

        public bool ExisteSerieSegura(string numeroSerie, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(numeroSerie))
                return false;

            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT COUNT(1) FROM dispositivosseguros WHERE idempresa = @idEmpresa AND numeroserie = @numeroSerie;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("numeroSerie", numeroSerie.Trim());
                });

            long count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt64(result);
            return count > 0;
        }
    }
}
