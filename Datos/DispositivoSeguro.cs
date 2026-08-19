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

        public List<Entidades.DispositivoSeguro> Listar(int idEmpresa)
        {
            const string sql = @"
                SELECT d.Id, d.IdEmpresa, d.NumeroSerie, d.Descripcion, d.CreadoUtc, d.IdUsuarioCreador,
                       u.nombre AS NombreUsuarioCreador
                FROM DispositivosSeguros d
                LEFT JOIN Usuarios u ON u.id = d.IdUsuarioCreador
                WHERE d.IdEmpresa = @idEmpresa
                ORDER BY d.CreadoUtc DESC;";

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr => new Entidades.DispositivoSeguro
                {
                    Id = Convert.ToInt32(dr["Id"]),
                    IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                    NumeroSerie = Convert.ToString(dr["NumeroSerie"]),
                    Descripcion = dr["Descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["Descripcion"]),
                    CreadoUtc = Convert.ToDateTime(dr["CreadoUtc"]),
                    IdUsuarioCreador = dr["IdUsuarioCreador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdUsuarioCreador"]),
                    NombreUsuarioCreador = dr["NombreUsuarioCreador"] == DBNull.Value ? "" : Convert.ToString(dr["NombreUsuarioCreador"])
                },
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa
            );
        }

        public void Agregar(Entidades.DispositivoSeguro dispositivo)
        {
            if (dispositivo == null) throw new ArgumentNullException(nameof(dispositivo));

            const string sql = @"
                INSERT INTO DispositivosSeguros (IdEmpresa, NumeroSerie, Descripcion, CreadoUtc, IdUsuarioCreador)
                VALUES (@idEmpresa, @numeroSerie, @descripcion, @creadoUtc, @idUsuarioCreador);";

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

        // Usado en el login (antes de autenticar), para decidir si se saltea LoginRateLimiter.
        public bool ExisteSerieSegura(string numeroSerie, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(numeroSerie))
                return false;

            const string sql = "SELECT COUNT(1) FROM DispositivosSeguros WHERE IdEmpresa = @idEmpresa AND NumeroSerie = @numeroSerie;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@numeroSerie", SqlDbType.NVarChar, 200).Value = numeroSerie.Trim();
                }
            );

            int count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            return count > 0;
        }
    }
}
