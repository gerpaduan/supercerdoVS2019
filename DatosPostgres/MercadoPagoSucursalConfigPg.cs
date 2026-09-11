using System;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IMercadoPagoSucursalConfigRepository. Una fila por
    // sucursal (mercadopago_sucursal_config, PK idsucursal) -- guarda el default fijado por un
    // admin y la ultima eleccion del cajero para el toggle "Conectado con Point" (ver
    // Entidades.MercadoPagoSucursalConfig y docs/DECISIONS.md sobre por que es por Sucursal).
    public class MercadoPagoSucursalConfigPg : Contratos.IMercadoPagoSucursalConfigRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public MercadoPagoSucursalConfigPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public Entidades.MercadoPagoSucursalConfig ObtenerPorSucursal(int idSucursal, int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT idsucursal, idempresa, conectadopointdefault, conectadopointultimaeleccion, fechaactualizacionutc, mpstoreid
                FROM mercadopago_sucursal_config
                WHERE idsucursal = @idSucursal;",
                MapConfig,
                p => p.AddWithValue("idSucursal", idSucursal));

            if (lista.Count > 0) return lista[0];

            return new Entidades.MercadoPagoSucursalConfig
            {
                IdSucursal = idSucursal,
                IdEmpresa = idEmpresa,
                ConectadoPointDefault = false,
                ConectadoPointUltimaEleccion = false,
                FechaActualizacionUtc = null
            };
        }

        public void GuardarDefault(int idSucursal, int idEmpresa, bool conectadoDefault)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mercadopago_sucursal_config (idsucursal, idempresa, conectadopointdefault, conectadopointultimaeleccion, fechaactualizacionutc)
                VALUES (@idSucursal, @idEmpresa, @conectadoDefault, @conectadoDefault, @ahora)
                ON CONFLICT (idsucursal) DO UPDATE SET
                    conectadopointdefault = EXCLUDED.conectadopointdefault,
                    fechaactualizacionutc = EXCLUDED.fechaactualizacionutc;",
                p =>
                {
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("conectadoDefault", conectadoDefault);
                    p.AddWithValue("ahora", DateTime.UtcNow);
                });
        }

        public void GuardarUltimaEleccion(int idSucursal, int idEmpresa, bool conectado)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mercadopago_sucursal_config (idsucursal, idempresa, conectadopointdefault, conectadopointultimaeleccion, fechaactualizacionutc)
                VALUES (@idSucursal, @idEmpresa, @conectado, @conectado, @ahora)
                ON CONFLICT (idsucursal) DO UPDATE SET
                    conectadopointultimaeleccion = EXCLUDED.conectadopointultimaeleccion,
                    fechaactualizacionutc = EXCLUDED.fechaactualizacionutc;",
                p =>
                {
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("conectado", conectado);
                    p.AddWithValue("ahora", DateTime.UtcNow);
                });
        }

        public void GuardarStoreId(int idSucursal, int idEmpresa, string mpStoreId)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO mercadopago_sucursal_config (idsucursal, idempresa, conectadopointdefault, conectadopointultimaeleccion, fechaactualizacionutc, mpstoreid)
                VALUES (@idSucursal, @idEmpresa, false, false, @ahora, @mpStoreId)
                ON CONFLICT (idsucursal) DO UPDATE SET
                    mpstoreid = EXCLUDED.mpstoreid,
                    fechaactualizacionutc = EXCLUDED.fechaactualizacionutc;",
                p =>
                {
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("mpStoreId", mpStoreId ?? "");
                    p.AddWithValue("ahora", DateTime.UtcNow);
                });
        }

        private static Entidades.MercadoPagoSucursalConfig MapConfig(Npgsql.NpgsqlDataReader dr)
        {
            return new Entidades.MercadoPagoSucursalConfig
            {
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                ConectadoPointDefault = Convert.ToBoolean(dr["conectadopointdefault"]),
                ConectadoPointUltimaEleccion = Convert.ToBoolean(dr["conectadopointultimaeleccion"]),
                FechaActualizacionUtc = dr["fechaactualizacionutc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaactualizacionutc"]),
                MpStoreId = dr["mpstoreid"] == DBNull.Value ? "" : Convert.ToString(dr["mpstoreid"])
            };
        }
    }
}
