using System;
using System.Collections.Generic;
using Npgsql;
using NpgsqlTypes;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IPosClaveUnicaRepository (tabla posclaveunica, migracion
    // 20261126-Create_posclaveunica.sql). RLS por empresa: DbPg / UnitOfWorkPg setean el tenant.
    // La unicidad la garantiza el indice ux_posclaveunica_empresa_clientid: un INSERT repetido falla con 23505 y se
    // traduce a ClaveDuplicadaException para que Negocio devuelva la operacion ya grabada.
    public class PosClaveUnicaPg : Contratos.IPosClaveUnicaRepository
    {
        // Codigo SQLSTATE de Postgres para violacion de indice unico.
        private const string SqlStateUniqueViolation = "23505";

        private const string Columnas =
            "id, clientid, tipo, idregistro, origen, idsucursal, idusuario, idusuariosync, fechalocal, recibido, " +
            "llegotarde, sincaja, pendientefacturar, difprecio, causacorte, versioncatalogo, detalle";

        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public PosClaveUnicaPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public Entidades.PosClaveUnica ObtenerPorClientId(Guid clientId)
        {
            List<Entidades.PosClaveUnica> filas = DbPg.Reader(
                _connectionString, _idEmpresa,
                "SELECT " + Columnas + " FROM posclaveunica WHERE idempresa = @idEmpresa AND clientid = @clientId",
                Mapear,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.Add("clientId", NpgsqlDbType.Uuid).Value = clientId;
                });
            return filas.Count > 0 ? filas[0] : null;
        }

        public void Insertar(Entidades.PosClaveUnica clave, Contratos.IUnitOfWork unitOfWork = null)
        {
            if (clave == null) throw new ArgumentNullException(nameof(clave));

            const string sql = @"
                INSERT INTO posclaveunica (idempresa, clientid, tipo, idregistro, origen, idsucursal, idusuario, idusuariosync,
                    fechalocal, llegotarde, sincaja, pendientefacturar, difprecio, causacorte, versioncatalogo, detalle)
                VALUES (@idEmpresa, @clientId, @tipo, @idRegistro, @origen, @idSucursal, @idUsuario, @idUsuarioSync,
                    @fechaLocal, @llegoTarde, @sinCaja, @pendienteFacturar, @difPrecio, @causaCorte, @versionCatalogo, @detalle);";

            try
            {
                DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.Add("clientId", NpgsqlDbType.Uuid).Value = clave.ClientId;
                    p.AddWithValue("tipo", clave.Tipo ?? "");
                    p.AddWithValue("idRegistro", clave.IdRegistro);
                    p.AddWithValue("origen", clave.Origen ?? "");
                    p.AddWithValue("idSucursal", clave.IdSucursal);
                    p.AddWithValue("idUsuario", clave.IdUsuario);
                    p.AddWithValue("idUsuarioSync", clave.IdUsuarioSync);
                    p.Add("fechaLocal", NpgsqlDbType.Timestamp).Value = clave.FechaLocal;
                    p.AddWithValue("llegoTarde", clave.LlegoTarde);
                    p.AddWithValue("sinCaja", clave.SinCaja);
                    p.AddWithValue("pendienteFacturar", clave.PendienteFacturar);
                    p.AddWithValue("difPrecio", clave.DifPrecio);
                    p.Add("causaCorte", NpgsqlDbType.Varchar).Value = (object)clave.CausaCorte ?? DBNull.Value;
                    p.Add("versionCatalogo", NpgsqlDbType.Varchar).Value = (object)clave.VersionCatalogo ?? DBNull.Value;
                    p.Add("detalle", NpgsqlDbType.Text).Value = (object)clave.Detalle ?? DBNull.Value;
                }, unitOfWork);
            }
            catch (PostgresException ex) when (ex.SqlState == SqlStateUniqueViolation)
            {
                // Reintento de una operacion ya grabada: quien llama deshace su transaccion y devuelve la existente.
                throw new Contratos.ClaveDuplicadaException(clave.ClientId, ex);
            }
        }

        public List<Entidades.PosClaveUnica> ListarVentasOfflineEnRango(int idUsuario, int idSucursal, DateTime desde, DateTime hasta)
        {
            return DbPg.Reader(
                _connectionString, _idEmpresa,
                "SELECT " + Columnas + " FROM posclaveunica WHERE idempresa = @idEmpresa AND tipo = 'venta' AND origen = 'offline' " +
                "AND idusuario = @idUsuario AND idsucursal = @idSucursal AND fechalocal BETWEEN @desde AND @hasta ORDER BY fechalocal",
                Mapear,
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idUsuario", idUsuario);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.Add("desde", NpgsqlDbType.Timestamp).Value = desde;
                    p.Add("hasta", NpgsqlDbType.Timestamp).Value = hasta;
                });
        }

        private static Entidades.PosClaveUnica Mapear(NpgsqlDataReader dr)
        {
            return new Entidades.PosClaveUnica
            {
                Id = Convert.ToInt32(dr["id"]),
                ClientId = (Guid)dr["clientid"],
                Tipo = Convert.ToString(dr["tipo"]),
                IdRegistro = Convert.ToInt32(dr["idregistro"]),
                Origen = Convert.ToString(dr["origen"]),
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                IdUsuario = Convert.ToInt32(dr["idusuario"]),
                IdUsuarioSync = Convert.ToInt32(dr["idusuariosync"]),
                FechaLocal = Convert.ToDateTime(dr["fechalocal"]),
                Recibido = Convert.ToDateTime(dr["recibido"]),
                LlegoTarde = Convert.ToBoolean(dr["llegotarde"]),
                SinCaja = Convert.ToBoolean(dr["sincaja"]),
                PendienteFacturar = Convert.ToBoolean(dr["pendientefacturar"]),
                DifPrecio = Convert.ToBoolean(dr["difprecio"]),
                CausaCorte = dr["causacorte"] == DBNull.Value ? null : Convert.ToString(dr["causacorte"]),
                VersionCatalogo = dr["versioncatalogo"] == DBNull.Value ? null : Convert.ToString(dr["versioncatalogo"]),
                Detalle = dr["detalle"] == DBNull.Value ? null : Convert.ToString(dr["detalle"])
            };
        }
    }
}
