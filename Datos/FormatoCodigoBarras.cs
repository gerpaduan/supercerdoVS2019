using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Implementacion SQL Server de Contratos.IFormatoCodigoBarrasRepository (6/6 metodos).
    // Ver Datos/DB-Procedures/20260901-Create_FormatosCodigoBarras.sql para el schema.
    public class FormatoCodigoBarras : Contratos.IFormatoCodigoBarrasRepository
    {
        private readonly IEmpresaContext _empresa;

        public FormatoCodigoBarras(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        private static Entidades.FormatoCodigoBarras Map(SqlDataReader dr)
        {
            return new Entidades.FormatoCodigoBarras
            {
                Id = Convert.ToInt32(dr["Id"]),
                IdEmpresa = Convert.ToInt32(dr["IdEmpresa"]),
                Nombre = Convert.ToString(dr["Nombre"]),
                Prefijo = Convert.ToInt32(dr["Prefijo"]),
                LongitudTotal = Convert.ToInt32(dr["LongitudTotal"]),
                PosicionCodigo = Convert.ToInt32(dr["PosicionCodigo"]),
                LongitudCodigo = Convert.ToInt32(dr["LongitudCodigo"]),
                PosicionValor = Convert.ToInt32(dr["PosicionValor"]),
                LongitudValor = Convert.ToInt32(dr["LongitudValor"]),
                TipoValor = (Entidades.TipoValorCodigoBarras)Enum.Parse(typeof(Entidades.TipoValorCodigoBarras), Convert.ToString(dr["TipoValor"])),
                CantidadDecimales = Convert.ToInt32(dr["CantidadDecimales"]),
                Activo = Convert.ToBoolean(dr["Activo"]),
                Prioridad = Convert.ToInt32(dr["Prioridad"]),
                CreadoUtc = Convert.ToDateTime(dr["CreadoUtc"]),
                IdUsuarioCreador = dr["IdUsuarioCreador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdUsuarioCreador"]),
                ModificadoUtc = dr["ModificadoUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["ModificadoUtc"]),
                IdUsuarioModificador = dr["IdUsuarioModificador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["IdUsuarioModificador"])
            };
        }

        public List<Entidades.FormatoCodigoBarras> Listar(int idEmpresa)
        {
            const string sql = @"
                SELECT * FROM FormatosCodigoBarras
                WHERE IdEmpresa = @idEmpresa
                ORDER BY Prioridad, Prefijo;";

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: Map,
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa
            );
        }

        public Entidades.FormatoCodigoBarras ObtenerPorId(int id, int idEmpresa)
        {
            const string sql = "SELECT * FROM FormatosCodigoBarras WHERE Id = @id AND IdEmpresa = @idEmpresa;";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: Map,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = id;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                }
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.FormatoCodigoBarras ObtenerActivoPorPrefijo(int idEmpresa, int prefijo)
        {
            const string sql = @"
                SELECT TOP 1 * FROM FormatosCodigoBarras
                WHERE IdEmpresa = @idEmpresa AND Prefijo = @prefijo AND Activo = 1;";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: Map,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@prefijo", SqlDbType.SmallInt).Value = prefijo;
                }
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        public bool ExistePrefijo(int idEmpresa, int prefijo, int idExcluir)
        {
            const string sql = @"
                SELECT COUNT(1) FROM FormatosCodigoBarras
                WHERE IdEmpresa = @idEmpresa AND Prefijo = @prefijo AND Id <> @idExcluir;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@prefijo", SqlDbType.SmallInt).Value = prefijo;
                    p.Add("@idExcluir", SqlDbType.Int).Value = idExcluir;
                }
            );

            int count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            return count > 0;
        }

        public void Agregar(Entidades.FormatoCodigoBarras formato)
        {
            if (formato == null) throw new ArgumentNullException(nameof(formato));

            const string sql = @"
                INSERT INTO FormatosCodigoBarras
                    (IdEmpresa, Nombre, Prefijo, LongitudTotal, PosicionCodigo, LongitudCodigo,
                     PosicionValor, LongitudValor, TipoValor, CantidadDecimales, Activo, Prioridad,
                     CreadoUtc, IdUsuarioCreador)
                VALUES
                    (@idEmpresa, @nombre, @prefijo, @longitudTotal, @posicionCodigo, @longitudCodigo,
                     @posicionValor, @longitudValor, @tipoValor, @cantidadDecimales, @activo, @prioridad,
                     @creadoUtc, @idUsuarioCreador);";

            Db.NonQuery(_empresa, sql, CommandType.Text, setParams: p => SetParamsComunes(p, formato, incluirCreacion: true));
        }

        public void Actualizar(Entidades.FormatoCodigoBarras formato)
        {
            if (formato == null) throw new ArgumentNullException(nameof(formato));

            const string sql = @"
                UPDATE FormatosCodigoBarras SET
                    Nombre = @nombre,
                    LongitudTotal = @longitudTotal,
                    PosicionCodigo = @posicionCodigo,
                    LongitudCodigo = @longitudCodigo,
                    PosicionValor = @posicionValor,
                    LongitudValor = @longitudValor,
                    TipoValor = @tipoValor,
                    CantidadDecimales = @cantidadDecimales,
                    Activo = @activo,
                    Prioridad = @prioridad,
                    ModificadoUtc = @modificadoUtc,
                    IdUsuarioModificador = @idUsuarioModificador
                WHERE Id = @id AND IdEmpresa = @idEmpresa;";

            Db.NonQuery(_empresa, sql, CommandType.Text, setParams: p =>
            {
                SetParamsComunes(p, formato, incluirCreacion: false);
                p.Add("@id", SqlDbType.Int).Value = formato.Id;
                p.Add("@modificadoUtc", SqlDbType.DateTime2).Value = (object)formato.ModificadoUtc ?? DBNull.Value;
                p.Add("@idUsuarioModificador", SqlDbType.Int).Value = (object)formato.IdUsuarioModificador ?? DBNull.Value;
            });
        }

        // Prefijo NO se actualiza a proposito (ver Negocio/FormatoCodigoBarras.cs): cambiar el
        // prefijo de un formato existente se resuelve dando de baja y creando uno nuevo.
        private static void SetParamsComunes(SqlParameterCollection p, Entidades.FormatoCodigoBarras formato, bool incluirCreacion)
        {
            p.Add("@idEmpresa", SqlDbType.Int).Value = formato.IdEmpresa;
            p.Add("@nombre", SqlDbType.NVarChar, 100).Value = formato.Nombre ?? "";
            p.Add("@longitudTotal", SqlDbType.SmallInt).Value = formato.LongitudTotal;
            p.Add("@posicionCodigo", SqlDbType.SmallInt).Value = formato.PosicionCodigo;
            p.Add("@longitudCodigo", SqlDbType.SmallInt).Value = formato.LongitudCodigo;
            p.Add("@posicionValor", SqlDbType.SmallInt).Value = formato.PosicionValor;
            p.Add("@longitudValor", SqlDbType.SmallInt).Value = formato.LongitudValor;
            p.Add("@tipoValor", SqlDbType.NVarChar, 20).Value = formato.TipoValor.ToString();
            p.Add("@cantidadDecimales", SqlDbType.SmallInt).Value = formato.CantidadDecimales;
            p.Add("@activo", SqlDbType.Bit).Value = formato.Activo;
            p.Add("@prioridad", SqlDbType.Int).Value = formato.Prioridad;

            if (incluirCreacion)
            {
                p.Add("@prefijo", SqlDbType.SmallInt).Value = formato.Prefijo;
                p.Add("@creadoUtc", SqlDbType.DateTime2).Value = formato.CreadoUtc;
                p.Add("@idUsuarioCreador", SqlDbType.Int).Value = (object)formato.IdUsuarioCreador ?? DBNull.Value;
            }
        }
    }
}
