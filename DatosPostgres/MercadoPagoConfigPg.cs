using System;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IMercadoPagoConfigRepository. Una fila por empresa
    // (mercadopago_config, PK idempresa) -- ver Entidades.MercadoPagoConfig. Los tokens llegan
    // ya cifrados desde Negocio.MercadoPagoConfig (Web/Helpers/MercadoPagoTokenCipher.cs); esta
    // clase nunca cifra/descifra, solo persiste lo que recibe.
    public class MercadoPagoConfigPg : Contratos.IMercadoPagoConfigRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public MercadoPagoConfigPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public Entidades.MercadoPagoConfig ObtenerPorEmpresa(int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT idempresa, clientstate, accesstokencifrado, refreshtokencifrado,
                       tokenexpirautc, conectado, idusuarioconexion, fechaconexionutc, fechaactualizacionutc, mpuserid
                FROM mercadopago_config
                WHERE idempresa = @idEmpresa;",
                dr => MapConfig(dr),
                p => p.AddWithValue("idEmpresa", idEmpresa));

            return lista.Count > 0 ? lista[0] : null;
        }

        public void GuardarClientState(int idEmpresa, string clientState)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mercadopago_config (idempresa, clientstate, conectado, fechaactualizacionutc)
                VALUES (@idEmpresa, @clientState, false, @ahora)
                ON CONFLICT (idempresa) DO UPDATE SET clientstate = EXCLUDED.clientstate, fechaactualizacionutc = EXCLUDED.fechaactualizacionutc;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("clientState", clientState ?? "");
                    p.AddWithValue("ahora", DateTime.UtcNow);
                });
        }

        public void GuardarTokens(int idEmpresa, string accessTokenCifrado, string refreshTokenCifrado,
            DateTime tokenExpiraUtc, int idUsuarioConexion, string mpUserId)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mercadopago_config
                    (idempresa, accesstokencifrado, refreshtokencifrado, tokenexpirautc, conectado,
                     idusuarioconexion, fechaconexionutc, fechaactualizacionutc, mpuserid)
                VALUES (@idEmpresa, @accessToken, @refreshToken, @tokenExpira, true, @idUsuario, @ahora, @ahora, @mpUserId)
                ON CONFLICT (idempresa) DO UPDATE SET
                    accesstokencifrado = EXCLUDED.accesstokencifrado,
                    refreshtokencifrado = EXCLUDED.refreshtokencifrado,
                    tokenexpirautc = EXCLUDED.tokenexpirautc,
                    conectado = true,
                    idusuarioconexion = EXCLUDED.idusuarioconexion,
                    fechaconexionutc = EXCLUDED.fechaconexionutc,
                    fechaactualizacionutc = EXCLUDED.fechaactualizacionutc,
                    mpuserid = EXCLUDED.mpuserid;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("accessToken", accessTokenCifrado ?? "");
                    p.AddWithValue("refreshToken", refreshTokenCifrado ?? "");
                    p.AddWithValue("tokenExpira", tokenExpiraUtc);
                    p.AddWithValue("idUsuario", idUsuarioConexion);
                    p.AddWithValue("ahora", DateTime.UtcNow);
                    p.AddWithValue("mpUserId", (object)mpUserId ?? DBNull.Value);
                });
        }

        public void Desconectar(int idEmpresa)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE mercadopago_config
                SET conectado = false, accesstokencifrado = NULL, refreshtokencifrado = NULL,
                    tokenexpirautc = NULL, fechaactualizacionutc = @ahora
                WHERE idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("ahora", DateTime.UtcNow);
                });
        }

        private static Entidades.MercadoPagoConfig MapConfig(Npgsql.NpgsqlDataReader dr)
        {
            return new Entidades.MercadoPagoConfig
            {
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                ClientState = dr["clientstate"] == DBNull.Value ? "" : Convert.ToString(dr["clientstate"]),
                AccessToken = dr["accesstokencifrado"] == DBNull.Value ? "" : Convert.ToString(dr["accesstokencifrado"]),
                RefreshToken = dr["refreshtokencifrado"] == DBNull.Value ? "" : Convert.ToString(dr["refreshtokencifrado"]),
                TokenExpiraUtc = dr["tokenexpirautc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["tokenexpirautc"]),
                Conectado = dr["conectado"] != DBNull.Value && Convert.ToBoolean(dr["conectado"]),
                IdUsuarioConexion = dr["idusuarioconexion"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idusuarioconexion"]),
                FechaConexionUtc = dr["fechaconexionutc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaconexionutc"]),
                FechaActualizacionUtc = dr["fechaactualizacionutc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaactualizacionutc"]),
                MpUserId = dr["mpuserid"] == DBNull.Value ? "" : Convert.ToString(dr["mpuserid"])
            };
        }
    }
}
