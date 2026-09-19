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
                       d.Origen, d.EmailAlta, d.Bloqueado,
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
                Bloqueado = dr["Bloqueado"] != DBNull.Value && Convert.ToBoolean(dr["Bloqueado"])
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
    }
}
