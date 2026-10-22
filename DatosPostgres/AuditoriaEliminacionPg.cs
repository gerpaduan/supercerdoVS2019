using System;
using System.Collections.Generic;
using System.Data;
using Entidades;

namespace DatosPostgres
{
    // Auditoria de registros eliminados (Compras, Stock, Movimientos) en Postgres: tablas auditoriaeliminaciones /
    // auditoriaeliminacionlineas (migracion 20261022). La escritura vive en Insertar (la usan CompraPg y CortePg dentro de
    // la misma transaccion que borra el registro); la lectura implementa IAuditoriaEliminacionRepository.
    // Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
    public class AuditoriaEliminacionPg : Contratos.IAuditoriaEliminacionRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public AuditoriaEliminacionPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        // Inserta la cabecera y devuelve su id. Las lineas las copia quien llama con un INSERT ... SELECT desde las tablas
        // del registro, antes de borrarlas (asi lo auditado es exactamente lo que habia en la base).
        internal static int Insertar(string connectionString, int idEmpresa, AuditoriaEliminacion a, Contratos.IUnitOfWork unitOfWork)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));

            const string sql = @"
                INSERT INTO auditoriaeliminaciones
                    (idempresa, modulo, idregistro, tiporegistro, fecharegistro, idsucursal, idsucursaldestino, idpersona, nroremito, enctacte,
                     cantmedias, kgsmedias, importe, totalkg, cantlineas, observaciones, idusuariocreador, creado, idusuarioelimino, idusuariosesion,
                     fechaeliminacion, motivo, ip, efectos)
                VALUES
                    (@idEmpresa, @modulo, @idRegistro, @tipoRegistro, @fechaRegistro, @idSucursal, @idSucursalDestino, @idPersona, @nroRemito, @enCtaCte,
                     @cantMedias, @kgsMedias, @importe, @totalKg, @cantLineas, @observaciones, @idUsuarioCreador, @creado, @idUsuarioElimino, @idUsuarioSesion,
                     now(), @motivo, @ip, @efectos)
                RETURNING id;";

            object id = DbPg.Scalar(connectionString, idEmpresa, sql, p =>
            {
                p.AddWithValue("idEmpresa", idEmpresa);
                p.AddWithValue("modulo", a.Modulo);
                p.AddWithValue("idRegistro", a.IdRegistro);
                p.AddWithValue("tipoRegistro", (object)a.TipoRegistro ?? DBNull.Value);
                p.AddWithValue("fechaRegistro", (object)a.FechaRegistro ?? DBNull.Value);
                p.AddWithValue("idSucursal", (object)a.IdSucursal ?? DBNull.Value);
                p.AddWithValue("idSucursalDestino", (object)a.IdSucursalDestino ?? DBNull.Value);
                p.AddWithValue("idPersona", (object)a.IdPersona ?? DBNull.Value);
                p.AddWithValue("nroRemito", (object)a.NroRemito ?? DBNull.Value);
                p.AddWithValue("enCtaCte", (object)a.EnCtaCte ?? DBNull.Value);
                p.AddWithValue("cantMedias", (object)a.CantMedias ?? DBNull.Value);
                p.AddWithValue("kgsMedias", (object)a.KgsMedias ?? DBNull.Value);
                p.AddWithValue("importe", (object)a.Importe ?? DBNull.Value);
                p.AddWithValue("totalKg", (object)a.TotalKg ?? DBNull.Value);
                p.AddWithValue("cantLineas", a.CantLineas);
                p.AddWithValue("observaciones", (object)a.Observaciones ?? DBNull.Value);
                p.AddWithValue("idUsuarioCreador", (object)a.IdUsuarioCreador ?? DBNull.Value);
                p.AddWithValue("creado", (object)a.Creado ?? DBNull.Value);
                p.AddWithValue("idUsuarioElimino", a.IdUsuarioElimino);
                p.AddWithValue("idUsuarioSesion", a.IdUsuarioSesion);
                p.AddWithValue("motivo", a.Motivo);
                p.AddWithValue("ip", (object)a.Ip ?? DBNull.Value);
                p.AddWithValue("efectos", (object)a.Efectos ?? DBNull.Value);
            }, unitOfWork);

            return id == null || id == DBNull.Value ? 0 : Convert.ToInt32(id);
        }

        public DataTable listar(string modulo, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            const string sql = @"
                SELECT a.id, a.modulo, a.idregistro AS ""idRegistro"", a.tiporegistro AS ""tipoRegistro"", a.fecharegistro AS ""fechaRegistro"",
                       s.sucursal AS ""sucursal"", p.razonsocial AS ""persona"", a.importe, a.cantlineas AS ""cantLineas"",
                       ue.nombre AS ""usuarioElimino"", us.nombre AS ""usuarioSesion"", a.fechaeliminacion AS ""fechaEliminacion"", a.motivo
                FROM auditoriaeliminaciones a
                LEFT JOIN sucursal s ON s.idsucursal = a.idsucursal
                LEFT JOIN personas p ON p.idpersona = a.idpersona
                LEFT JOIN usuarios ue ON ue.id = a.idusuarioelimino
                LEFT JOIN usuarios us ON us.id = a.idusuariosesion
                WHERE a.fechaeliminacion >= @desde AND a.fechaeliminacion < @hasta
                  AND (@modulo = '' OR a.modulo = @modulo)
                  AND (@texto = '' OR LOWER(a.motivo) LIKE @patron OR LOWER(COALESCE(a.nroremito,'')) LIKE @patron
                       OR CAST(a.idregistro AS text) = @texto OR LOWER(COALESCE(ue.nombre,'')) LIKE @patron
                       OR LOWER(COALESCE(p.razonsocial,'')) LIKE @patron)
                ORDER BY a.fechaeliminacion DESC, a.id DESC
                LIMIT 1000;";

            string textoNormalizado = (texto ?? "").Trim().ToLowerInvariant();
            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("desde", fechaDesde.Date);
                p.AddWithValue("hasta", fechaHasta.Date.AddDays(1));
                p.AddWithValue("modulo", modulo ?? "");
                p.AddWithValue("texto", textoNormalizado);
                p.AddWithValue("patron", "%" + textoNormalizado.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%");
            });
        }

        public AuditoriaEliminacion obtener(int id)
        {
            if (id <= 0) return null;
            return LeerCabecera("WHERE a.id = @id", p => p.AddWithValue("id", id));
        }

        public AuditoriaEliminacion obtenerPorRegistro(string modulo, int idRegistro)
        {
            if (idRegistro <= 0 || string.IsNullOrEmpty(modulo)) return null;
            return LeerCabecera("WHERE a.modulo = @modulo AND a.idregistro = @idRegistro", p =>
            {
                p.AddWithValue("modulo", modulo);
                p.AddWithValue("idRegistro", idRegistro);
            });
        }

        private AuditoriaEliminacion LeerCabecera(string where, Action<Npgsql.NpgsqlParameterCollection> setParams)
        {
            string sql = @"
                SELECT a.id, a.modulo, a.idregistro, a.tiporegistro, a.fecharegistro, a.idsucursal, s.sucursal AS sucursalnombre,
                       a.idsucursaldestino, sd.sucursal AS sucursaldestinonombre, a.idpersona, p.razonsocial AS personanombre,
                       a.nroremito, a.enctacte, a.cantmedias, a.kgsmedias, a.importe, a.totalkg, a.cantlineas, a.observaciones, a.idusuariocreador, uc.nombre AS usuariocreadornombre,
                       a.creado, a.idusuarioelimino, ue.nombre AS usuarioeliminonombre, a.idusuariosesion, us.nombre AS usuariosesionnombre,
                       a.fechaeliminacion, a.motivo, a.ip, a.efectos
                FROM auditoriaeliminaciones a
                LEFT JOIN sucursal s ON s.idsucursal = a.idsucursal
                LEFT JOIN sucursal sd ON sd.idsucursal = a.idsucursaldestino
                LEFT JOIN personas p ON p.idpersona = a.idpersona
                LEFT JOIN usuarios uc ON uc.id = a.idusuariocreador
                LEFT JOIN usuarios ue ON ue.id = a.idusuarioelimino
                LEFT JOIN usuarios us ON us.id = a.idusuariosesion
                " + where + @"
                ORDER BY a.id DESC
                LIMIT 1;";

            var filas = DbPg.Reader(_connectionString, _idEmpresa, sql, dr => new AuditoriaEliminacion
            {
                Id = Convert.ToInt32(dr["id"]),
                Modulo = Convert.ToString(dr["modulo"]),
                IdRegistro = Convert.ToInt32(dr["idregistro"]),
                TipoRegistro = Texto(dr["tiporegistro"]),
                FechaRegistro = Fecha(dr["fecharegistro"]),
                IdSucursal = Entero(dr["idsucursal"]),
                SucursalNombre = Texto(dr["sucursalnombre"]),
                IdSucursalDestino = Entero(dr["idsucursaldestino"]),
                SucursalDestinoNombre = Texto(dr["sucursaldestinonombre"]),
                IdPersona = Entero(dr["idpersona"]),
                PersonaNombre = Texto(dr["personanombre"]),
                NroRemito = Texto(dr["nroremito"]),
                EnCtaCte = dr["enctacte"] == DBNull.Value ? (bool?)null : Convert.ToBoolean(dr["enctacte"]),
                CantMedias = Entero(dr["cantmedias"]),
                KgsMedias = Decimal(dr["kgsmedias"]),
                Importe = Decimal(dr["importe"]),
                TotalKg = Decimal(dr["totalkg"]),
                CantLineas = Convert.ToInt32(dr["cantlineas"]),
                Observaciones = Texto(dr["observaciones"]),
                IdUsuarioCreador = Entero(dr["idusuariocreador"]),
                UsuarioCreadorNombre = Texto(dr["usuariocreadornombre"]),
                Creado = Fecha(dr["creado"]),
                IdUsuarioElimino = Convert.ToInt32(dr["idusuarioelimino"]),
                UsuarioEliminoNombre = Texto(dr["usuarioeliminonombre"]),
                IdUsuarioSesion = Convert.ToInt32(dr["idusuariosesion"]),
                UsuarioSesionNombre = Texto(dr["usuariosesionnombre"]),
                FechaEliminacion = Convert.ToDateTime(dr["fechaeliminacion"]),
                Motivo = Texto(dr["motivo"]),
                Ip = Texto(dr["ip"]),
                Efectos = Texto(dr["efectos"])
            }, setParams);

            if (filas.Count == 0) return null;
            var auditoria = filas[0];

            auditoria.Lineas = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT id, idcorte, codigo, producto, idsucursal, cantkg, cantunidad, preciokg, detalle
                FROM auditoriaeliminacionlineas WHERE idauditoria = @id ORDER BY id;",
                dr => new AuditoriaEliminacionLinea
                {
                    Id = Convert.ToInt32(dr["id"]),
                    IdCorte = Entero(dr["idcorte"]),
                    Codigo = dr["codigo"] == DBNull.Value ? (long?)null : Convert.ToInt64(dr["codigo"]),
                    Producto = Texto(dr["producto"]),
                    IdSucursal = Entero(dr["idsucursal"]),
                    CantKg = Decimal(dr["cantkg"]),
                    CantUnidad = Entero(dr["cantunidad"]),
                    PrecioKg = Decimal(dr["preciokg"]),
                    Detalle = Texto(dr["detalle"])
                }, p => p.AddWithValue("id", auditoria.Id));

            return auditoria;
        }

        private static string Texto(object valor) => valor == null || valor == DBNull.Value ? null : Convert.ToString(valor);
        private static int? Entero(object valor) => valor == null || valor == DBNull.Value ? (int?)null : Convert.ToInt32(valor);
        private static double? Decimal(object valor) => valor == null || valor == DBNull.Value ? (double?)null : Convert.ToDouble(valor);
        private static DateTime? Fecha(object valor) => valor == null || valor == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(valor);
    }
}
