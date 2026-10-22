using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Utilidades;

namespace Datos
{
    // Auditoria de registros eliminados (Compras, Stock, Movimientos) en SQL Server: tablas AuditoriaEliminaciones /
    // AuditoriaEliminacionLineas (Datos/DB-Procedures/20261022-Create_AuditoriaEliminaciones.sql). La escritura vive en
    // SqlInsertarCabecera + AgregarParametros (las usan Datos.Compra y Datos.Corte dentro del mismo batch que borra el
    // registro); la lectura implementa IAuditoriaEliminacionRepository. Sin sintaxis posterior a SQL Server 2008.
    // Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
    public class AuditoriaEliminacionSql : Contratos.IAuditoriaEliminacionRepository
    {
        private readonly IEmpresaContext _empresa;

        public AuditoriaEliminacionSql(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        // Fragmento de batch: inserta la cabecera y deja el id en @idAuditoria (declarado aca).
        internal const string SqlInsertarCabecera = @"
            INSERT INTO dbo.AuditoriaEliminaciones
                (idEmpresa, modulo, idRegistro, tipoRegistro, fechaRegistro, idSucursal, idSucursalDestino, idPersona, nroRemito, enCtaCte,
                 cantMedias, kgsMedias, importe, totalKg, cantLineas, observaciones, idUsuarioCreador, creado, idUsuarioElimino, idUsuarioSesion,
                 fechaEliminacion, motivo, ip, efectos)
            VALUES
                (@idEmpresa, @modulo, @idRegistro, @tipoRegistro, @fechaRegistro, @idSucursal, @idSucursalDestino, @idPersona, @nroRemito, @enCtaCte,
                 @cantMedias, @kgsMedias, @importe, @totalKg, @cantLineas, @observaciones, @idUsuarioCreador, @creado, @idUsuarioElimino, @idUsuarioSesion,
                 GETDATE(), @motivo, @ip, @efectos);
            DECLARE @idAuditoria INT;
            SET @idAuditoria = SCOPE_IDENTITY();";

        internal static void AgregarParametros(SqlParameterCollection p, int idEmpresa, Entidades.AuditoriaEliminacion a)
        {
            p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
            p.Add("@modulo", SqlDbType.VarChar, 12).Value = a.Modulo;
            p.Add("@idRegistro", SqlDbType.Int).Value = a.IdRegistro;
            p.Add("@tipoRegistro", SqlDbType.NVarChar, 100).Value = (object)a.TipoRegistro ?? DBNull.Value;
            p.Add("@fechaRegistro", SqlDbType.DateTime).Value = (object)a.FechaRegistro ?? DBNull.Value;
            p.Add("@idSucursal", SqlDbType.Int).Value = (object)a.IdSucursal ?? DBNull.Value;
            p.Add("@idSucursalDestino", SqlDbType.Int).Value = (object)a.IdSucursalDestino ?? DBNull.Value;
            p.Add("@idPersona", SqlDbType.Int).Value = (object)a.IdPersona ?? DBNull.Value;
            p.Add("@nroRemito", SqlDbType.NVarChar, 100).Value = (object)a.NroRemito ?? DBNull.Value;
            p.Add("@enCtaCte", SqlDbType.Bit).Value = (object)a.EnCtaCte ?? DBNull.Value;
            p.Add("@cantMedias", SqlDbType.Int).Value = (object)a.CantMedias ?? DBNull.Value;
            p.Add("@kgsMedias", SqlDbType.Float).Value = (object)a.KgsMedias ?? DBNull.Value;
            p.Add("@importe", SqlDbType.Float).Value = (object)a.Importe ?? DBNull.Value;
            p.Add("@totalKg", SqlDbType.Float).Value = (object)a.TotalKg ?? DBNull.Value;
            p.Add("@cantLineas", SqlDbType.Int).Value = a.CantLineas;
            p.Add("@observaciones", SqlDbType.NVarChar, -1).Value = (object)a.Observaciones ?? DBNull.Value;
            p.Add("@idUsuarioCreador", SqlDbType.Int).Value = (object)a.IdUsuarioCreador ?? DBNull.Value;
            p.Add("@creado", SqlDbType.DateTime).Value = (object)a.Creado ?? DBNull.Value;
            p.Add("@idUsuarioElimino", SqlDbType.Int).Value = a.IdUsuarioElimino;
            p.Add("@idUsuarioSesion", SqlDbType.Int).Value = a.IdUsuarioSesion;
            p.Add("@motivo", SqlDbType.NVarChar, -1).Value = a.Motivo;
            p.Add("@ip", SqlDbType.VarChar, 60).Value = (object)a.Ip ?? DBNull.Value;
            p.Add("@efectos", SqlDbType.NVarChar, -1).Value = (object)a.Efectos ?? DBNull.Value;
        }

        public DataTable listar(string modulo, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            const string sql = @"
                SELECT TOP 1000 a.id, a.modulo, a.idRegistro AS idRegistro, a.tipoRegistro AS tipoRegistro, a.fechaRegistro AS fechaRegistro,
                       s.sucursal AS sucursal, p.razonSocial AS persona, a.importe, a.cantLineas AS cantLineas,
                       ue.nombre AS usuarioElimino, us.nombre AS usuarioSesion, a.fechaEliminacion AS fechaEliminacion, a.motivo
                FROM dbo.AuditoriaEliminaciones a
                LEFT JOIN dbo.Sucursal s ON s.idSucursal = a.idSucursal
                LEFT JOIN dbo.Personas p ON p.idPersona = a.idPersona
                LEFT JOIN dbo.Usuarios ue ON ue.id = a.idUsuarioElimino
                LEFT JOIN dbo.Usuarios us ON us.id = a.idUsuarioSesion
                WHERE a.idEmpresa = @idEmpresa AND a.fechaEliminacion >= @desde AND a.fechaEliminacion < @hasta
                  AND (@modulo = '' OR a.modulo = @modulo)
                  AND (@texto = '' OR a.motivo LIKE @patron ESCAPE '\' OR ISNULL(a.nroRemito, '') LIKE @patron ESCAPE '\'
                       OR CAST(a.idRegistro AS NVARCHAR(20)) = @texto OR ISNULL(ue.nombre, '') LIKE @patron ESCAPE '\'
                       OR ISNULL(p.razonSocial, '') LIKE @patron ESCAPE '\')
                ORDER BY a.fechaEliminacion DESC, a.id DESC;";

            string textoNormalizado = (texto ?? "").Trim();
            return Db.DataTable(_empresa, sql, CommandType.Text, p =>
            {
                p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                p.Add("@desde", SqlDbType.DateTime).Value = fechaDesde.Date;
                p.Add("@hasta", SqlDbType.DateTime).Value = fechaHasta.Date.AddDays(1);
                p.Add("@modulo", SqlDbType.VarChar, 12).Value = modulo ?? "";
                p.Add("@texto", SqlDbType.NVarChar, 200).Value = textoNormalizado;
                p.Add("@patron", SqlDbType.NVarChar, 210).Value = "%" + textoNormalizado.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[") + "%";
            });
        }

        public Entidades.AuditoriaEliminacion obtener(int id)
        {
            if (id <= 0) return null;
            return LeerCabecera("WHERE a.id = @id", p => p.Add("@id", SqlDbType.Int).Value = id);
        }

        public Entidades.AuditoriaEliminacion obtenerPorRegistro(string modulo, int idRegistro)
        {
            if (idRegistro <= 0 || string.IsNullOrEmpty(modulo)) return null;
            return LeerCabecera("WHERE a.modulo = @modulo AND a.idRegistro = @idRegistro", p =>
            {
                p.Add("@modulo", SqlDbType.VarChar, 12).Value = modulo;
                p.Add("@idRegistro", SqlDbType.Int).Value = idRegistro;
            });
        }

        private Entidades.AuditoriaEliminacion LeerCabecera(string where, Action<SqlParameterCollection> setParams)
        {
            string sql = @"
                SELECT TOP 1 a.id, a.modulo, a.idRegistro, a.tipoRegistro, a.fechaRegistro, a.idSucursal, s.sucursal AS sucursalNombre,
                       a.idSucursalDestino, sd.sucursal AS sucursalDestinoNombre, a.idPersona, p.razonSocial AS personaNombre,
                       a.nroRemito, a.enCtaCte, a.cantMedias, a.kgsMedias, a.importe, a.totalKg, a.cantLineas, a.observaciones, a.idUsuarioCreador, uc.nombre AS usuarioCreadorNombre,
                       a.creado, a.idUsuarioElimino, ue.nombre AS usuarioEliminoNombre, a.idUsuarioSesion, us.nombre AS usuarioSesionNombre,
                       a.fechaEliminacion, a.motivo, a.ip, a.efectos
                FROM dbo.AuditoriaEliminaciones a
                LEFT JOIN dbo.Sucursal s ON s.idSucursal = a.idSucursal
                LEFT JOIN dbo.Sucursal sd ON sd.idSucursal = a.idSucursalDestino
                LEFT JOIN dbo.Personas p ON p.idPersona = a.idPersona
                LEFT JOIN dbo.Usuarios uc ON uc.id = a.idUsuarioCreador
                LEFT JOIN dbo.Usuarios ue ON ue.id = a.idUsuarioElimino
                LEFT JOIN dbo.Usuarios us ON us.id = a.idUsuarioSesion
                " + where + @" AND a.idEmpresa = @idEmpresa
                ORDER BY a.id DESC;";

            var filas = Db.Reader(_empresa, sql, CommandType.Text, dr => new Entidades.AuditoriaEliminacion
            {
                Id = Convert.ToInt32(dr["id"]),
                Modulo = Convert.ToString(dr["modulo"]),
                IdRegistro = Convert.ToInt32(dr["idRegistro"]),
                TipoRegistro = Texto(dr["tipoRegistro"]),
                FechaRegistro = Fecha(dr["fechaRegistro"]),
                IdSucursal = Entero(dr["idSucursal"]),
                SucursalNombre = Texto(dr["sucursalNombre"]),
                IdSucursalDestino = Entero(dr["idSucursalDestino"]),
                SucursalDestinoNombre = Texto(dr["sucursalDestinoNombre"]),
                IdPersona = Entero(dr["idPersona"]),
                PersonaNombre = Texto(dr["personaNombre"]),
                NroRemito = Texto(dr["nroRemito"]),
                EnCtaCte = dr["enCtaCte"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["enCtaCte"]),
                CantMedias = Entero(dr["cantMedias"]),
                KgsMedias = Decimal(dr["kgsMedias"]),
                Importe = Decimal(dr["importe"]),
                TotalKg = Decimal(dr["totalKg"]),
                CantLineas = Convert.ToInt32(dr["cantLineas"]),
                Observaciones = Texto(dr["observaciones"]),
                IdUsuarioCreador = Entero(dr["idUsuarioCreador"]),
                UsuarioCreadorNombre = Texto(dr["usuarioCreadorNombre"]),
                Creado = Fecha(dr["creado"]),
                IdUsuarioElimino = Convert.ToInt32(dr["idUsuarioElimino"]),
                UsuarioEliminoNombre = Texto(dr["usuarioEliminoNombre"]),
                IdUsuarioSesion = Convert.ToInt32(dr["idUsuarioSesion"]),
                UsuarioSesionNombre = Texto(dr["usuarioSesionNombre"]),
                FechaEliminacion = Convert.ToDateTime(dr["fechaEliminacion"]),
                Motivo = Texto(dr["motivo"]),
                Ip = Texto(dr["ip"]),
                Efectos = Texto(dr["efectos"])
            }, p =>
            {
                p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                setParams(p);
            });

            if (filas.Count == 0) return null;
            var auditoria = filas[0];

            auditoria.Lineas = Db.Reader(_empresa, @"
                SELECT id, idCorte, codigo, producto, idSucursal, cantKg, cantUnidad, precioKg, detalle
                FROM dbo.AuditoriaEliminacionLineas WHERE idEmpresa = @idEmpresa AND idAuditoria = @id ORDER BY id;",
                CommandType.Text,
                dr => new Entidades.AuditoriaEliminacionLinea
                {
                    Id = Convert.ToInt32(dr["id"]),
                    IdCorte = Entero(dr["idCorte"]),
                    Codigo = dr["codigo"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["codigo"]),
                    Producto = Texto(dr["producto"]),
                    IdSucursal = Entero(dr["idSucursal"]),
                    CantKg = Decimal(dr["cantKg"]),
                    CantUnidad = Entero(dr["cantUnidad"]),
                    PrecioKg = Decimal(dr["precioKg"]),
                    Detalle = Texto(dr["detalle"])
                }, p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@id", SqlDbType.Int).Value = auditoria.Id;
                });

            return auditoria;
        }

        private static string Texto(object valor) => valor == null || valor == DBNull.Value ? null : Convert.ToString(valor);
        private static int? Entero(object valor) => valor == null || valor == DBNull.Value ? (int?)null : Convert.ToInt32(valor);
        private static double? Decimal(object valor) => valor == null || valor == DBNull.Value ? (double?)null : Convert.ToDouble(valor);
        private static DateTime? Fecha(object valor) => valor == null || valor == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(valor);
    }
}
