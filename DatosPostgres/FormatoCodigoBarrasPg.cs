using System;
using System.Collections.Generic;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IFormatoCodigoBarrasRepository (6/6 metodos).
    // formatoscodigobarras SI tiene UNIQUE(idempresa, prefijo) real en Postgres (a diferencia
    // del gap conocido en dispositivosseguros, donde el UNIQUE de SQL Server no se replico) --
    // decision explicita del usuario. Ver DatosPostgres/DB-Migrations/
    // 20260901-Create_formatoscodigobarras.sql.
    public class FormatoCodigoBarrasPg : Contratos.IFormatoCodigoBarrasRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public FormatoCodigoBarrasPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private static Entidades.FormatoCodigoBarras Map(NpgsqlDataReader dr)
        {
            return new Entidades.FormatoCodigoBarras
            {
                Id = Convert.ToInt32(dr["id"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                Nombre = Convert.ToString(dr["nombre"]),
                Prefijo = Convert.ToInt32(dr["prefijo"]),
                LongitudTotal = Convert.ToInt32(dr["longitudtotal"]),
                PosicionCodigo = Convert.ToInt32(dr["posicioncodigo"]),
                LongitudCodigo = Convert.ToInt32(dr["longitudcodigo"]),
                PosicionValor = Convert.ToInt32(dr["posicionvalor"]),
                LongitudValor = Convert.ToInt32(dr["longitudvalor"]),
                TipoValor = (Entidades.TipoValorCodigoBarras)Enum.Parse(typeof(Entidades.TipoValorCodigoBarras), Convert.ToString(dr["tipovalor"])),
                CantidadDecimales = Convert.ToInt32(dr["cantidaddecimales"]),
                Activo = Convert.ToBoolean(dr["activo"]),
                Prioridad = Convert.ToInt32(dr["prioridad"]),
                CreadoUtc = Convert.ToDateTime(dr["creadoutc"]),
                IdUsuarioCreador = dr["idusuariocreador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idusuariocreador"]),
                ModificadoUtc = dr["modificadoutc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["modificadoutc"]),
                IdUsuarioModificador = dr["idusuariomodificador"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idusuariomodificador"])
            };
        }

        public List<Entidades.FormatoCodigoBarras> Listar(int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT * FROM formatoscodigobarras
                WHERE idempresa = @idEmpresa
                ORDER BY prioridad, prefijo;",
                Map,
                p => p.AddWithValue("idEmpresa", idEmpresa));
        }

        public Entidades.FormatoCodigoBarras ObtenerPorId(int id, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT * FROM formatoscodigobarras WHERE id = @id AND idempresa = @idEmpresa;",
                Map,
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });

            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.FormatoCodigoBarras ObtenerActivoPorPrefijo(int idEmpresa, int prefijo)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT * FROM formatoscodigobarras
                WHERE idempresa = @idEmpresa AND prefijo = @prefijo AND activo = true
                LIMIT 1;",
                Map,
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("prefijo", prefijo);
                });

            return lista.Count > 0 ? lista[0] : null;
        }

        public bool ExistePrefijo(int idEmpresa, int prefijo, int idExcluir)
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa, @"
                SELECT COUNT(1) FROM formatoscodigobarras
                WHERE idempresa = @idEmpresa AND prefijo = @prefijo AND id <> @idExcluir;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("prefijo", prefijo);
                    p.AddWithValue("idExcluir", idExcluir);
                });

            long count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt64(result);
            return count > 0;
        }

        public void Agregar(Entidades.FormatoCodigoBarras formato)
        {
            if (formato == null) throw new ArgumentNullException(nameof(formato));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO formatoscodigobarras
                    (idempresa, nombre, prefijo, longitudtotal, posicioncodigo, longitudcodigo,
                     posicionvalor, longitudvalor, tipovalor, cantidaddecimales, activo, prioridad,
                     creadoutc, idusuariocreador)
                VALUES
                    (@idEmpresa, @nombre, @prefijo, @longitudTotal, @posicionCodigo, @longitudCodigo,
                     @posicionValor, @longitudValor, @tipoValor, @cantidadDecimales, @activo, @prioridad,
                     @creadoUtc, @idUsuarioCreador);",
                p =>
                {
                    SetParamsComunes(p, formato);
                    p.AddWithValue("prefijo", formato.Prefijo);
                    p.AddWithValue("creadoUtc", formato.CreadoUtc);
                    p.AddWithValue("idUsuarioCreador", (object)formato.IdUsuarioCreador ?? DBNull.Value);
                });
        }

        public void Actualizar(Entidades.FormatoCodigoBarras formato)
        {
            if (formato == null) throw new ArgumentNullException(nameof(formato));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE formatoscodigobarras SET
                    nombre = @nombre,
                    longitudtotal = @longitudTotal,
                    posicioncodigo = @posicionCodigo,
                    longitudcodigo = @longitudCodigo,
                    posicionvalor = @posicionValor,
                    longitudvalor = @longitudValor,
                    tipovalor = @tipoValor,
                    cantidaddecimales = @cantidadDecimales,
                    activo = @activo,
                    prioridad = @prioridad,
                    modificadoutc = @modificadoUtc,
                    idusuariomodificador = @idUsuarioModificador
                WHERE id = @id AND idempresa = @idEmpresa;",
                p =>
                {
                    SetParamsComunes(p, formato);
                    p.AddWithValue("id", formato.Id);
                    p.AddWithValue("modificadoUtc", (object)formato.ModificadoUtc ?? DBNull.Value);
                    p.AddWithValue("idUsuarioModificador", (object)formato.IdUsuarioModificador ?? DBNull.Value);
                });
        }

        // Prefijo NO se actualiza a proposito (ver Negocio/FormatoCodigoBarras.cs): cambiar el
        // prefijo de un formato existente se resuelve dando de baja y creando uno nuevo.
        private static void SetParamsComunes(NpgsqlParameterCollection p, Entidades.FormatoCodigoBarras formato)
        {
            p.AddWithValue("idEmpresa", formato.IdEmpresa);
            p.AddWithValue("nombre", formato.Nombre ?? "");
            p.AddWithValue("longitudTotal", formato.LongitudTotal);
            p.AddWithValue("posicionCodigo", formato.PosicionCodigo);
            p.AddWithValue("longitudCodigo", formato.LongitudCodigo);
            p.AddWithValue("posicionValor", formato.PosicionValor);
            p.AddWithValue("longitudValor", formato.LongitudValor);
            p.AddWithValue("tipoValor", formato.TipoValor.ToString());
            p.AddWithValue("cantidadDecimales", formato.CantidadDecimales);
            p.AddWithValue("activo", formato.Activo);
            p.AddWithValue("prioridad", formato.Prioridad);
        }
    }
}
