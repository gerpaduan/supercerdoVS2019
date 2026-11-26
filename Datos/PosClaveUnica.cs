using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Implementacion SQL Server de Contratos.IPosClaveUnicaRepository (tabla PosClaveUnica, script
    // Datos/DB-Procedures/20261126-Create_PosClaveUnica_Alter_agregarVenta.sql). Espejo de DatosPostgres.PosClaveUnicaPg:
    // sin RLS (idEmpresa explicito). Insertar se enlista en el TransactionScope ambiente de Negocio.Venta, asi que un
    // choque con el indice unico deshace tambien la venta.
    public class PosClaveUnica : Contratos.IPosClaveUnicaRepository
    {
        // Numeros de error de SQL Server para clave duplicada (indice unico / restriccion unique).
        private const int ErrorIndiceUnicoDuplicado = 2601;
        private const int ErrorRestriccionUnicaDuplicada = 2627;

        private const string Columnas =
            "id, clientId, tipo, idRegistro, origen, idSucursal, idUsuario, idUsuarioSync, fechaLocal, recibido, " +
            "llegoTarde, sinCaja, pendienteFacturar, difPrecio, causaCorte, versionCatalogo, detalle";

        private readonly IEmpresaContext _empresa;

        public PosClaveUnica(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        public Entidades.PosClaveUnica ObtenerPorClientId(Guid clientId)
        {
            List<Entidades.PosClaveUnica> filas = Db.Reader(
                _empresa,
                "SELECT " + Columnas + " FROM PosClaveUnica WHERE idEmpresa = @idEmpresa AND clientId = @clientId",
                CommandType.Text,
                Mapear,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clientId;
                });
            return filas.Count > 0 ? filas[0] : null;
        }

        public void Insertar(Entidades.PosClaveUnica clave, Contratos.IUnitOfWork unitOfWork = null)
        {
            if (clave == null) throw new ArgumentNullException(nameof(clave));

            const string sql = @"
                INSERT INTO PosClaveUnica (idEmpresa, clientId, tipo, idRegistro, origen, idSucursal, idUsuario, idUsuarioSync,
                    fechaLocal, llegoTarde, sinCaja, pendienteFacturar, difPrecio, causaCorte, versionCatalogo, detalle)
                VALUES (@idEmpresa, @clientId, @tipo, @idRegistro, @origen, @idSucursal, @idUsuario, @idUsuarioSync,
                    @fechaLocal, @llegoTarde, @sinCaja, @pendienteFacturar, @difPrecio, @causaCorte, @versionCatalogo, @detalle);";

            try
            {
                Db.NonQuery(_empresa, sql, CommandType.Text, p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@clientId", SqlDbType.UniqueIdentifier).Value = clave.ClientId;
                    p.Add("@tipo", SqlDbType.VarChar, 10).Value = clave.Tipo ?? "";
                    p.Add("@idRegistro", SqlDbType.Int).Value = clave.IdRegistro;
                    p.Add("@origen", SqlDbType.VarChar, 10).Value = clave.Origen ?? "";
                    p.Add("@idSucursal", SqlDbType.Int).Value = clave.IdSucursal;
                    p.Add("@idUsuario", SqlDbType.Int).Value = clave.IdUsuario;
                    p.Add("@idUsuarioSync", SqlDbType.Int).Value = clave.IdUsuarioSync;
                    p.Add("@fechaLocal", SqlDbType.DateTime).Value = clave.FechaLocal;
                    p.Add("@llegoTarde", SqlDbType.Bit).Value = clave.LlegoTarde;
                    p.Add("@sinCaja", SqlDbType.Bit).Value = clave.SinCaja;
                    p.Add("@pendienteFacturar", SqlDbType.Bit).Value = clave.PendienteFacturar;
                    p.Add("@difPrecio", SqlDbType.Bit).Value = clave.DifPrecio;
                    p.Add("@causaCorte", SqlDbType.VarChar, 20).Value = (object)clave.CausaCorte ?? DBNull.Value;
                    p.Add("@versionCatalogo", SqlDbType.VarChar, 40).Value = (object)clave.VersionCatalogo ?? DBNull.Value;
                    p.Add("@detalle", SqlDbType.NVarChar, -1).Value = (object)clave.Detalle ?? DBNull.Value;
                });
            }
            catch (SqlException ex) when (ex.Number == ErrorIndiceUnicoDuplicado || ex.Number == ErrorRestriccionUnicaDuplicada)
            {
                // Reintento de una operacion ya grabada: quien llama deshace su transaccion y devuelve la existente.
                throw new Contratos.ClaveDuplicadaException(clave.ClientId, ex);
            }
        }

        public List<Entidades.PosClaveUnica> ListarVentasOfflineEnRango(int idUsuario, int idSucursal, DateTime desde, DateTime hasta)
        {
            return Db.Reader(
                _empresa,
                "SELECT " + Columnas + " FROM PosClaveUnica WHERE idEmpresa = @idEmpresa AND tipo = 'venta' AND origen = 'offline' " +
                "AND idUsuario = @idUsuario AND idSucursal = @idSucursal AND fechaLocal BETWEEN @desde AND @hasta ORDER BY fechaLocal",
                CommandType.Text,
                Mapear,
                p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                    p.Add("@desde", SqlDbType.DateTime).Value = desde;
                    p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
                });
        }

        private static Entidades.PosClaveUnica Mapear(SqlDataReader dr)
        {
            return new Entidades.PosClaveUnica
            {
                Id = Convert.ToInt32(dr["id"]),
                ClientId = (Guid)dr["clientId"],
                Tipo = Convert.ToString(dr["tipo"]),
                IdRegistro = Convert.ToInt32(dr["idRegistro"]),
                Origen = Convert.ToString(dr["origen"]),
                IdSucursal = Convert.ToInt32(dr["idSucursal"]),
                IdUsuario = Convert.ToInt32(dr["idUsuario"]),
                IdUsuarioSync = Convert.ToInt32(dr["idUsuarioSync"]),
                FechaLocal = Convert.ToDateTime(dr["fechaLocal"]),
                Recibido = Convert.ToDateTime(dr["recibido"]),
                LlegoTarde = Convert.ToBoolean(dr["llegoTarde"]),
                SinCaja = Convert.ToBoolean(dr["sinCaja"]),
                PendienteFacturar = Convert.ToBoolean(dr["pendienteFacturar"]),
                DifPrecio = Convert.ToBoolean(dr["difPrecio"]),
                CausaCorte = dr["causaCorte"] == DBNull.Value ? null : Convert.ToString(dr["causaCorte"]),
                VersionCatalogo = dr["versionCatalogo"] == DBNull.Value ? null : Convert.ToString(dr["versionCatalogo"]),
                Detalle = dr["detalle"] == DBNull.Value ? null : Convert.ToString(dr["detalle"])
            };
        }
    }
}
