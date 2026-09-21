using System;
using System.Collections.Generic;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IUsuarioPasskeyRepository (tabla usuariopasskeys, ver
    // migracion 20260921-Create_usuariopasskeys.sql). La tabla tiene RLS por empresa: los metodos
    // normales van con el tenant (DbPg); los "SinTenant" son solo para el login por huella, que
    // busca la credencial ANTES de saber la empresa y por eso asume el rol carnisys_usuarios_bypass
    // con SET LOCAL ROLE (mismo mecanismo que UsuarioPg.AbrirSinRLS, scopeado a la transaccion).
    public class UsuarioPasskeyPg : Contratos.IUsuarioPasskeyRepository
    {
        private const string Columnas =
            "id, idusuario, idempresa, credentialid, publickey, signcount, userhandle, aaguid, transports, nombre, fechaaltautc, ultimousoutc";

        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public UsuarioPasskeyPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private static Entidades.UsuarioPasskey Mapear(System.Data.IDataRecord dr)
        {
            return new Entidades.UsuarioPasskey
            {
                Id = Convert.ToInt32(dr["id"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                CredentialId = (byte[])dr["credentialid"],
                PublicKey = (byte[])dr["publickey"],
                SignCount = Convert.ToInt64(dr["signcount"]),
                UserHandle = (byte[])dr["userhandle"],
                Aaguid = dr["aaguid"] == DBNull.Value ? (Guid?)null : (Guid)dr["aaguid"],
                Transports = dr["transports"] == DBNull.Value ? "" : Convert.ToString(dr["transports"]),
                Nombre = Convert.ToString(dr["nombre"]),
                FechaAltaUtc = Convert.ToDateTime(dr["fechaaltautc"]),
                UltimoUsoUtc = dr["ultimousoutc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["ultimousoutc"])
            };
        }

        public List<Entidades.UsuarioPasskey> ListarPorUsuario(int idUsuario, int idEmpresa)
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT " + Columnas + " FROM usuariopasskeys WHERE idusuario = @idUsuario AND idempresa = @idEmpresa ORDER BY fechaaltautc;",
                Mapear,
                p =>
                {
                    p.AddWithValue("idUsuario", idUsuario);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });
        }

        public void Agregar(Entidades.UsuarioPasskey passkey)
        {
            if (passkey == null) throw new ArgumentNullException(nameof(passkey));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO usuariopasskeys (idusuario, idempresa, credentialid, publickey, signcount, userhandle, aaguid, transports, nombre, fechaaltautc)
                VALUES (@idUsuario, @idEmpresa, @credentialId, @publicKey, @signCount, @userHandle, @aaguid, @transports, @nombre, @fechaAltaUtc);",
                p =>
                {
                    p.AddWithValue("idUsuario", passkey.IdUsuario);
                    p.AddWithValue("idEmpresa", passkey.IdEmpresa);
                    p.AddWithValue("credentialId", passkey.CredentialId);
                    p.AddWithValue("publicKey", passkey.PublicKey);
                    p.AddWithValue("signCount", passkey.SignCount);
                    p.AddWithValue("userHandle", passkey.UserHandle);
                    p.Add(new NpgsqlParameter("aaguid", NpgsqlTypes.NpgsqlDbType.Uuid) { Value = (object)passkey.Aaguid ?? DBNull.Value });
                    p.AddWithValue("transports", (object)passkey.Transports ?? DBNull.Value);
                    p.AddWithValue("nombre", passkey.Nombre ?? "");
                    p.AddWithValue("fechaAltaUtc", passkey.FechaAltaUtc);
                });
        }

        public bool Eliminar(int id, int idUsuario, int idEmpresa)
        {
            int filas = DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM usuariopasskeys WHERE id = @id AND idusuario = @idUsuario AND idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("id", id);
                    p.AddWithValue("idUsuario", idUsuario);
                    p.AddWithValue("idEmpresa", idEmpresa);
                });
            return filas > 0;
        }

        // Abre conexion+transaccion propia con el rol bypass (SET LOCAL ROLE muere con la
        // transaccion, una conexion reciclada del pool no arrastra el rol a la proxima request).
        private NpgsqlConnection AbrirSinRLS(out NpgsqlTransaction tx)
        {
            var cn = new NpgsqlConnection(_connectionString);
            cn.Open();
            tx = cn.BeginTransaction();
            using (var cmd = new NpgsqlCommand("SET LOCAL ROLE carnisys_usuarios_bypass;", cn, tx))
                cmd.ExecuteNonQuery();
            return cn;
        }

        public Entidades.UsuarioPasskey ObtenerPorCredentialIdSinTenant(byte[] credentialId)
        {
            if (credentialId == null || credentialId.Length == 0)
                return null;

            using (var cn = AbrirSinRLS(out var tx))
            {
                try
                {
                    Entidades.UsuarioPasskey resultado = null;
                    using (var cmd = new NpgsqlCommand("SELECT " + Columnas + " FROM usuariopasskeys WHERE credentialid = @credentialId;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("credentialId", credentialId);
                        using (var dr = cmd.ExecuteReader())
                        {
                            if (dr.Read())
                                resultado = Mapear(dr);
                        }
                    }
                    tx.Commit();
                    return resultado;
                }
                catch { tx.Rollback(); throw; }
            }
        }

        public void RegistrarUsoSinTenant(int id, long signCount, DateTime usoUtc)
        {
            using (var cn = AbrirSinRLS(out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand("UPDATE usuariopasskeys SET signcount = @signCount, ultimousoutc = @usoUtc WHERE id = @id;", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("id", id);
                        cmd.Parameters.AddWithValue("signCount", signCount);
                        cmd.Parameters.AddWithValue("usoUtc", usoUtc);
                        cmd.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
                catch { tx.Rollback(); throw; }
            }
        }
    }
}
