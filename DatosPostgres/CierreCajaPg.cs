using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Entidades;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres del bloque CierreCaja/EgresosCaja/TiposEgresoCaja de
    // Contratos.ICierreCajaRepository. cambiarSucursalCaja/obtenerPreviewCambioSucursalCaja
    // (operacion cross-cutting) no estan cubiertos -- se agregan en una etapa futura.
    // Ver docs/DECISIONS.md, Etapa 8.
    //
    // cierrecaja.id: en SQL Server NO es identity (esquema propio namespaced por sucursal,
    // 10_000_000 * idSucursal + N). En Postgres, decision del usuario, es autoincremental --
    // divergencia deliberada, solo de este lado. Los datos migrados preservan los ids viejos
    // (namespaced); las filas nuevas usan el autoincremental de Postgres, sin colision posible
    // (los ids viejos son mucho mas altos que el punto de partida del autoincremental).
    public class CierreCajaPg : Contratos.ICierreCajaRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public CierreCajaPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        #region Helpers

        private static bool ColumnaExiste(NpgsqlDataReader dr, string columna)
        {
            try { return dr.GetOrdinal(columna) >= 0; } catch { return false; }
        }

        private static string GetString(NpgsqlDataReader dr, string columna) =>
            ColumnaExiste(dr, columna) && dr[columna] != DBNull.Value ? Convert.ToString(dr[columna]) : "";

        private EgresoCaja MapEgresoCaja(NpgsqlDataReader dr)
        {
            var oEgresoCaja = new EgresoCaja
            {
                Id = Convert.ToInt32(dr["id"]),
                Fecha = Convert.ToDateTime(dr["fechahora"]),
                IdTipoEgresoCaja = Convert.ToInt32(dr["idtipoegresocaja"]),
                TipoEgresoCaja = GetString(dr, "tipoegresocaja"),
                Descripcion = GetString(dr, "descripcion"),
                Detalle = GetString(dr, "detalle"),
                Monto = dr["monto"] == DBNull.Value ? 0f : Convert.ToSingle(dr["monto"]),
                IdCompra = dr["idcompra"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idcompra"]),
                Sucursal = new Sucursal { idSucursal = Convert.ToInt32(dr["idsucursal"]) },
                Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"]),
                CreadoPor = dr["creadopor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["creadopor"]),
                Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"]),
                ActualizadoPor = dr["actualizadopor"] == DBNull.Value ? -1 : Convert.ToInt32(dr["actualizadopor"])
            };

            return oEgresoCaja;
        }

        #endregion

        #region CierreCaja

        public DataTable findCierreCaja(CierreCaja oCierreParam, CierreCaja.tipoBusqueda tipoBusquedaParam, string texto, DateTime? fechaDesde)
        {
            if (oCierreParam == null) throw new ArgumentNullException(nameof(oCierreParam));

            // Alias de columna identicos al SP/SQL original (con mayusculas/guion bajo donde
            // corresponde) porque Negocio/CierreCaja.cs.convertDatatableToList los consume por
            // nombre literal -- no es solo cosmetico como en otras tablas de reporte.
            string sql;
            Action<NpgsqlParameterCollection> setParams;

            switch (tipoBusquedaParam)
            {
                case CierreCaja.tipoBusqueda.FindAll:
                    sql = @"
                        SELECT cc.id, u.nombre AS ""Iniciada_Por"", fechahorainicio AS ""Inicio"", fechahoracierre AS ""Cierre"",
                            ROUND(CAST(cajainicio AS numeric), 2) AS ""Caja_Inicial"", ROUND(CAST(ventas AS numeric), 2) AS ""Ventas"",
                            ROUND(CAST(gastos AS numeric), 2) AS ""EgresosCaja"", ROUND(CAST(cajacierre AS numeric), 2) AS ""Caja_Cierre"",
                            ROUND(CAST(diferencia AS numeric), 2) AS ""Diferencia"", ROUND(CAST(cajainiciosiguiente AS numeric), 2) AS ""Caja_Ini_Sig"",
                            ROUND(CAST(importeretirado AS numeric), 2) AS ""Retirado"", uc.nombre AS ""Cerrada_Por""
                        FROM cierrecaja cc
                        INNER JOIN usuarios u ON cc.usuarioinicio = CAST(u.id AS text)
                        INNER JOIN usuarios uc ON cc.usuariocierre = CAST(uc.id AS text)
                        WHERE cc.idsucursal = @sucursal AND cc.fechahorainicio > @fechaDesde AND u.nombre ILIKE @texto
                        ORDER BY cc.id DESC;";
                    setParams = p =>
                    {
                        p.AddWithValue("sucursal", oCierreParam.Sucursal.idSucursal);
                        p.AddWithValue("fechaDesde", (object)fechaDesde ?? DateTime.MinValue);
                        p.AddWithValue("texto", "%" + (texto ?? "") + "%");
                    };
                    break;

                case CierreCaja.tipoBusqueda.FindOpen:
                    sql = @"
                        SELECT cc.id, cc.idsucursal, cc.usuarioinicio, u.nombre AS vendedor, s.sucursal, cc.fechahorainicio,
                            ROUND(CAST(cc.cajainicio AS numeric), 2) AS cajainicio
                        FROM cierrecaja cc
                        INNER JOIN usuarios u ON cc.usuarioinicio = CAST(u.id AS text)
                        INNER JOIN sucursal s ON cc.idsucursal = s.idsucursal
                        WHERE (@sucursal::int IS NULL OR @sucursal = 0 OR cc.idsucursal = @sucursal)
                          AND u.nombre ILIKE @texto AND cc.usuariocierre = '0';";
                    setParams = p =>
                    {
                        p.AddWithValue("sucursal", oCierreParam.Sucursal?.idSucursal ?? 0);
                        p.AddWithValue("texto", "%" + (texto ?? "") + "%");
                    };
                    break;

                case CierreCaja.tipoBusqueda.FindById:
                    sql = @"
                        SELECT cc.*, u.nombre AS vendedor, u.usuario AS vendedorusuario, s.sucursal
                        FROM cierrecaja cc
                        INNER JOIN usuarios u ON cc.usuarioinicio = CAST(u.id AS text)
                        INNER JOIN sucursal s ON cc.idsucursal = s.idsucursal
                        WHERE cc.id = @id;";
                    setParams = p => p.AddWithValue("id", oCierreParam.Id);
                    break;

                case CierreCaja.tipoBusqueda.FindLast:
                    sql = @"SELECT * FROM cierrecaja WHERE idsucursal = @sucursal AND usuarioinicio = @usuarioInicio ORDER BY id DESC LIMIT 1;";
                    setParams = p =>
                    {
                        p.AddWithValue("sucursal", oCierreParam.Sucursal.idSucursal);
                        p.AddWithValue("usuarioInicio", oCierreParam.UsuarioInicio.Id.ToString());
                    };
                    break;

                case CierreCaja.tipoBusqueda.FindLastOpen:
                    sql = @"SELECT * FROM cierrecaja WHERE usuarioinicio = @usuarioInicio AND id < @id ORDER BY id DESC LIMIT 1;";
                    setParams = p =>
                    {
                        p.AddWithValue("usuarioInicio", oCierreParam.UsuarioInicio.Id.ToString());
                        p.AddWithValue("id", oCierreParam.Id);
                    };
                    break;

                default:
                    sql = "SELECT * FROM cierrecaja LIMIT 0;";
                    setParams = null;
                    break;
            }

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, setParams);
        }

        public void addOrEditCierreCaja(CierreCaja oCierreCajaE)
        {
            if (oCierreCajaE == null) throw new ArgumentNullException(nameof(oCierreCajaE));

            // A diferencia del SP real (que genera el id con el esquema namespaced por
            // sucursal), en Postgres se usa el autoincremental nativo -- decision del usuario,
            // ver cabecera del archivo y docs/DECISIONS.md.
            if (oCierreCajaE.Id <= 0)
            {
                const string sqlInsert = @"
                    INSERT INTO cierrecaja (idsucursal, fechahorainicio, fechahoracierre, cajainicio, ventas, gastos,
                        cajacierre, diferencia, cajainiciosiguiente, importeretirado, usuarioinicio, usuariocierre, creado, actualizado, idempresa)
                    VALUES (@idSucursal, @fechaHoraInicio, @fechaHoraCierre, @cajaInicio, @ventas, @gastos,
                        @cajaCierre, @diferencia, @cajaInicioSiguiente, @importeRetirado, @usuarioInicio, @usuarioCierre, now(), NULL, @idEmpresa)
                    RETURNING id;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
                {
                    p.AddWithValue("idSucursal", oCierreCajaE.Sucursal.IdSucursal);
                    p.AddWithValue("fechaHoraInicio", oCierreCajaE.FechaHoraInicio);
                    p.AddWithValue("fechaHoraCierre", (object)oCierreCajaE.FechaHoraCierre ?? DBNull.Value);
                    p.AddWithValue("cajaInicio", oCierreCajaE.CajaInicio);
                    p.AddWithValue("ventas", oCierreCajaE.Ventas);
                    p.AddWithValue("gastos", oCierreCajaE.EgresosCaja);
                    p.AddWithValue("cajaCierre", oCierreCajaE.CajaCierre);
                    p.AddWithValue("diferencia", oCierreCajaE.Diferencia);
                    p.AddWithValue("cajaInicioSiguiente", oCierreCajaE.CajaInicioSiguiente);
                    p.AddWithValue("importeRetirado", oCierreCajaE.ImporteRetirado);
                    p.AddWithValue("usuarioInicio", oCierreCajaE.UsuarioInicio.Id.ToString());
                    p.AddWithValue("usuarioCierre", oCierreCajaE.UsuarioCierre != null ? oCierreCajaE.UsuarioCierre.Id.ToString() : "0");
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

                oCierreCajaE.Id = Convert.ToInt32(nuevoId);
                return;
            }

            const string sqlUpdate = @"
                UPDATE cierrecaja SET idsucursal=@idSucursal, fechahorainicio=@fechaHoraInicio, fechahoracierre=@fechaHoraCierre,
                    cajainicio=@cajaInicio, ventas=@ventas, gastos=@gastos, cajacierre=@cajaCierre, diferencia=@diferencia,
                    cajainiciosiguiente=@cajaInicioSiguiente, importeretirado=@importeRetirado, usuarioinicio=@usuarioInicio,
                    usuariocierre=@usuarioCierre, actualizado=now()
                WHERE id=@id;";

            DbPg.NonQuery(_connectionString, _idEmpresa, sqlUpdate, p =>
            {
                p.AddWithValue("id", oCierreCajaE.Id);
                p.AddWithValue("idSucursal", oCierreCajaE.Sucursal.IdSucursal);
                p.AddWithValue("fechaHoraInicio", oCierreCajaE.FechaHoraInicio);
                p.AddWithValue("fechaHoraCierre", (object)oCierreCajaE.FechaHoraCierre ?? DBNull.Value);
                p.AddWithValue("cajaInicio", oCierreCajaE.CajaInicio);
                p.AddWithValue("ventas", oCierreCajaE.Ventas);
                p.AddWithValue("gastos", oCierreCajaE.EgresosCaja);
                p.AddWithValue("cajaCierre", oCierreCajaE.CajaCierre);
                p.AddWithValue("diferencia", oCierreCajaE.Diferencia);
                p.AddWithValue("cajaInicioSiguiente", oCierreCajaE.CajaInicioSiguiente);
                p.AddWithValue("importeRetirado", oCierreCajaE.ImporteRetirado);
                p.AddWithValue("usuarioInicio", oCierreCajaE.UsuarioInicio.Id.ToString());
                p.AddWithValue("usuarioCierre", oCierreCajaE.UsuarioCierre != null ? oCierreCajaE.UsuarioCierre.Id.ToString() : "0");
            });
        }

        public DataTable findCierreCajaMultiples(List<CierreCaja> listaCierreCaja)
        {
            var dt = new DataTable();
            if (listaCierreCaja == null || listaCierreCaja.Count == 0) return dt;

            var ids = listaCierreCaja.Where(c => c != null && c.Id > 0).Select(c => c.Id).ToList();
            if (ids.Count == 0) return dt;

            const string sql = @"
                SELECT cc.*, u.*
                FROM cierrecaja cc
                INNER JOIN usuarios u ON cc.usuarioinicio = CAST(u.id AS text)
                WHERE cc.id = ANY(@ids);";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p => p.AddWithValue("ids", ids.ToArray()));
        }

        #endregion

        #region TiposEgresoCaja

        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso)
        {
            const string sql = @"
                SELECT id, tipoegresocaja, esgasto AS ""Es_Gasto"", creado AS ""Creado"", actualizado AS ""Actualizado"", reservadosistema AS ""Reservado""
                FROM tiposegresocaja
                WHERE (@id = 0 OR id = @id) AND (@txt = '' OR tipoegresocaja ILIKE @likeTxt)
                ORDER BY orden, tipoegresocaja;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("id", idTipoEgreso > 0 ? idTipoEgreso : 0);
                p.AddWithValue("txt", buscarText ?? "");
                p.AddWithValue("likeTxt", "%" + (buscarText ?? "") + "%");
            });
        }

        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto)
        {
            bool esInsert = id == -1;

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    if (esInsert)
                    {
                        // Mismo esquema manual que el original (MAX(id)+1) -- incluido el mismo
                        // riesgo de condicion de carrera ya reconocido en el codigo fuente.
                        using (var cmdMax = new NpgsqlCommand("SELECT COALESCE(MAX(id), 0) FROM tiposegresocaja;", con, tx))
                        {
                            object result = cmdMax.ExecuteScalar();
                            id = (result == null || result == DBNull.Value) ? 1 : Convert.ToInt32(result) + 1;
                        }

                        using (var cmdIns = new NpgsqlCommand(
                            "INSERT INTO tiposegresocaja (id, tipoegresocaja, esgasto, orden, reservadosistema, creado, idempresa) VALUES (@id, @tipo, @esGasto, 10, @reservado, @creado, @idEmpresa);", con, tx))
                        {
                            cmdIns.Parameters.AddWithValue("id", id);
                            cmdIns.Parameters.AddWithValue("tipo", tipoEgresoCaja ?? "");
                            cmdIns.Parameters.AddWithValue("esGasto", esGasto);
                            cmdIns.Parameters.AddWithValue("reservado", false);
                            cmdIns.Parameters.AddWithValue("creado", DateTime.Now);
                            cmdIns.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                            cmdIns.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var cmdUpd = new NpgsqlCommand(
                            "UPDATE tiposegresocaja SET tipoegresocaja=@tipo, esgasto=@esGasto, actualizado=@actualizado WHERE id=@id;", con, tx))
                        {
                            cmdUpd.Parameters.AddWithValue("id", id);
                            cmdUpd.Parameters.AddWithValue("tipo", tipoEgresoCaja ?? "");
                            cmdUpd.Parameters.AddWithValue("esGasto", esGasto);
                            cmdUpd.Parameters.AddWithValue("actualizado", DateTime.Now);
                            cmdUpd.ExecuteNonQuery();
                        }
                    }

                    tx?.Commit();
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public void eliminarTipoEgreso(int id)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, "DELETE FROM tiposegresocaja WHERE id = @id;", p => p.AddWithValue("id", id));
        }

        #endregion

        #region EgresosCaja

        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            // dbo.obtenerEgresosCaja (SP real, 5 ramas segun combinacion de parametros,
            // verificado con sp_helptext) se llama desde 4 puntos distintos del C# original con
            // distinta combinacion de parametros cada vez -- se replico cada punto de llamada
            // como su propio metodo con su propia query (mas claro que replicar el dispatch de
            // una sola mega-SP): esta interfaz (Datos.CierreCaja.obtenerEgresosCaja, sin @id ni
            // @montoEgresoCaja ni @verEgresoCaja) solo alcanza las ramas "por tipo" y "general"
            // de abajo. Las otras 3 ramas del SP real (@montoEgresoCaja, @id, @verEgresoCaja)
            // estan implementadas en getMontoEgresosCajaVendedor, getEgresoCajaById y
            // getEgresosCajaVendedor respectivamente, mas abajo en este archivo.
            string sql;
            Action<NpgsqlParameterCollection> setParams;

            const string sqlPorTipo = @"
                SELECT ec.id, ec.fechahora AS ""Fecha"", te.tipoegresocaja AS ""TipoEgresoCaja"",
                    ec.descripcion AS ""Descripcion"", ec.detalle AS ""Detalle"", ROUND(CAST(ec.monto AS numeric), 2) AS ""Monto"",
                    te.esgasto AS ""Gasto"", ec.creado AS ""Creado"", cp.nombre AS ""Creado Por"",
                    ec.actualizado AS ""Actualizado"", ap.nombre AS ""Actualizado Por""
                FROM egresoscaja ec
                INNER JOIN tiposegresocaja te ON ec.idtipoegresocaja = te.id
                LEFT JOIN usuarios cp ON ec.creadopor = cp.id
                LEFT JOIN usuarios ap ON ec.actualizadopor = ap.id
                WHERE ec.fechahora BETWEEN @fechaDesde AND @fechaHasta AND ec.idtipoegresocaja = @idTipoEgresoCaja
                  AND (@idSucursal = 0 OR ec.idsucursal = @idSucursal)
                  AND ec.descripcion ILIKE @textoLike
                  AND (@idVendedor <= 0 OR ec.creadopor = @idVendedor)
                ORDER BY ec.fechahora DESC, ec.id DESC;";

            const string sqlGeneral = @"
                SELECT ec.id, ec.fechahora AS ""Fecha"", te.tipoegresocaja AS ""TipoEgresoCaja"",
                    ec.descripcion AS ""Descripcion"", ec.detalle AS ""Detalle"", ROUND(CAST(ec.monto AS numeric), 2) AS ""Monto"",
                    te.esgasto AS ""Gasto"", ec.creado AS ""Creado"", cp.nombre AS ""Creado Por"",
                    ec.actualizado AS ""Actualizado"", ap.nombre AS ""Actualizado Por""
                FROM egresoscaja ec
                INNER JOIN tiposegresocaja te ON ec.idtipoegresocaja = te.id
                LEFT JOIN usuarios cp ON ec.creadopor = cp.id
                LEFT JOIN usuarios ap ON ec.actualizadopor = ap.id
                WHERE ec.fechahora BETWEEN @fechaDesde AND @fechaHasta
                  AND (@idSucursal = 0 OR ec.idsucursal = @idSucursal)
                  AND ((@idVendedor < 0 AND ec.creadopor >= 0) OR (@idVendedor >= 0 AND ec.creadopor = @idVendedor))
                  AND (ec.descripcion ILIKE @textoLike OR ap.nombre ILIKE @textoLike)
                ORDER BY ec.fechahora DESC, ec.id DESC;";

            if (idTipoEgresoCaja > 0)
            {
                sql = sqlPorTipo;
                setParams = p =>
                {
                    p.AddWithValue("fechaDesde", fechaDesde);
                    p.AddWithValue("fechaHasta", fechaHasta);
                    p.AddWithValue("idTipoEgresoCaja", idTipoEgresoCaja);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("textoLike", "%" + (texto ?? "") + "%");
                    p.AddWithValue("idVendedor", idUsuario);
                };
            }
            else
            {
                sql = sqlGeneral;
                setParams = p =>
                {
                    p.AddWithValue("fechaDesde", fechaDesde);
                    p.AddWithValue("fechaHasta", fechaHasta);
                    p.AddWithValue("idSucursal", idSucursal);
                    p.AddWithValue("idVendedor", idUsuario);
                    p.AddWithValue("textoLike", "%" + (texto ?? "") + "%");
                };
            }

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, setParams);
        }

        public DataTable obtenerEgresosCajaGastosBalance(int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT ec.id, ec.fechahora AS ""Fecha"", ec.idtipoegresocaja, te.tipoegresocaja AS ""TipoEgresoCaja"",
                    ec.descripcion AS ""Descripcion"", ec.detalle AS ""Detalle"", ROUND(CAST(ec.monto AS numeric), 2) AS ""Monto"", te.esgasto AS ""Gasto""
                FROM egresoscaja ec
                INNER JOIN tiposegresocaja te ON ec.idtipoegresocaja = te.id
                WHERE ec.fechahora BETWEEN @fechaDesde AND @fechaHasta AND ec.idsucursal = @idSucursal AND te.esgasto = true
                ORDER BY ec.fechahora DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("idSucursal", idSucursal);
            });
        }

        public DataTable obtenerGastosAgrupadosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal)
        {
            const string sql = @"
                SELECT te.tipoegresocaja AS ""Categoria"", SUM(ROUND(CAST(ec.monto AS numeric), 2)) AS ""Total""
                FROM egresoscaja ec
                INNER JOIN tiposegresocaja te ON te.id = ec.idtipoegresocaja
                WHERE ec.fechahora BETWEEN @fechaDesde AND @fechaHasta
                  AND (@idSucursal = -1 OR ec.idsucursal = @idSucursal)
                  AND te.esgasto = true
                GROUP BY te.tipoegresocaja
                ORDER BY SUM(ROUND(CAST(ec.monto AS numeric), 2)) DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("idSucursal", idSucursal ?? -1);
            });
        }

        public EgresoCaja addOrEditEgresoCaja(EgresoCaja oEgresoCaja, Contratos.IUnitOfWork unitOfWork = null)
        {
            if (oEgresoCaja == null) throw new ArgumentNullException(nameof(oEgresoCaja));

            if (oEgresoCaja.Id == 0)
            {
                object esGastoObj = DbPg.Scalar(_connectionString, _idEmpresa,
                    "SELECT esgasto FROM tiposegresocaja WHERE id = @idTipoEgresoCaja;",
                    p => p.AddWithValue("idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja), unitOfWork);
                bool esGasto = esGastoObj != null && esGastoObj != DBNull.Value && Convert.ToBoolean(esGastoObj);

                const string sqlInsert = @"
                    INSERT INTO egresoscaja (fechahora, idtipoegresocaja, descripcion, detalle, monto, idcompra, tabla, idtabla, esgasto, idsucursal, creado, creadopor, idempresa)
                    VALUES (@fecha, @idTipoEgresoCaja, @descripcion, @detalle, @monto, @idCompra, @tabla, @idTabla, @esGasto, @idSucursal, now(), @creadoPor, @idEmpresa)
                    RETURNING id;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sqlInsert, p =>
                {
                    p.AddWithValue("fecha", oEgresoCaja.Fecha);
                    p.AddWithValue("idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja);
                    p.AddWithValue("descripcion", oEgresoCaja.Descripcion ?? "");
                    p.AddWithValue("detalle", oEgresoCaja.Detalle ?? "");
                    p.AddWithValue("monto", oEgresoCaja.Monto);
                    p.AddWithValue("idCompra", (object)oEgresoCaja.IdCompra ?? DBNull.Value);
                    p.AddWithValue("tabla", oEgresoCaja.Tabla ?? "");
                    p.AddWithValue("idTabla", (object)oEgresoCaja.IdTabla ?? DBNull.Value);
                    p.AddWithValue("esGasto", esGasto);
                    p.AddWithValue("idSucursal", oEgresoCaja.Sucursal.idSucursal);
                    p.AddWithValue("creadoPor", oEgresoCaja.CreadoPor);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                }, unitOfWork);

                oEgresoCaja.Id = Convert.ToInt32(nuevoId);
            }
            else
            {
                bool? esGasto = null;
                if (oEgresoCaja.Id != 1)
                {
                    object esGastoObj = DbPg.Scalar(_connectionString, _idEmpresa,
                        "SELECT esgasto FROM tiposegresocaja WHERE id = @idTipoEgresoCaja;",
                        p => p.AddWithValue("idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja), unitOfWork);
                    esGasto = esGastoObj != null && esGastoObj != DBNull.Value && Convert.ToBoolean(esGastoObj);
                }

                const string sqlUpdate = @"
                    UPDATE egresoscaja SET fechahora=@fecha, idtipoegresocaja=@idTipoEgresoCaja, descripcion=@descripcion,
                        detalle=@detalle, monto=@monto, idcompra=@idCompra, tabla=@tabla, idtabla=@idTabla, esgasto=@esGasto,
                        idsucursal=@idSucursal, actualizado=now(), actualizadopor=@actualizadoPor
                    WHERE id=@id;";

                DbPg.NonQuery(_connectionString, _idEmpresa, sqlUpdate, p =>
                {
                    p.AddWithValue("fecha", oEgresoCaja.Fecha);
                    p.AddWithValue("idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja);
                    p.AddWithValue("descripcion", oEgresoCaja.Descripcion ?? "");
                    p.AddWithValue("detalle", oEgresoCaja.Detalle ?? "");
                    p.AddWithValue("monto", oEgresoCaja.Monto);
                    p.AddWithValue("idCompra", (object)oEgresoCaja.IdCompra ?? DBNull.Value);
                    p.AddWithValue("tabla", oEgresoCaja.Tabla ?? "");
                    p.AddWithValue("idTabla", (object)oEgresoCaja.IdTabla ?? DBNull.Value);
                    p.AddWithValue("esGasto", (object)esGasto ?? DBNull.Value);
                    p.AddWithValue("idSucursal", oEgresoCaja.Sucursal.idSucursal);
                    p.AddWithValue("actualizadoPor", oEgresoCaja.ActualizadoPor);
                    p.AddWithValue("id", oEgresoCaja.Id);
                }, unitOfWork);
            }

            return oEgresoCaja;
        }

        public EgresoCaja getEgresoCajaById(int idEgresoCaja)
        {
            const string sql = @"
                SELECT ec.id, ec.fechahora, ec.idtipoegresocaja, te.tipoegresocaja, ec.descripcion, ec.detalle,
                    ROUND(CAST(ec.monto AS numeric), 2) AS monto, ec.idcompra, ec.idsucursal, ec.creado, ec.creadopor, ec.actualizado, ec.actualizadopor
                FROM egresoscaja ec INNER JOIN tiposegresocaja te ON ec.idtipoegresocaja = te.id
                WHERE ec.id = @id;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, MapEgresoCaja, p => p.AddWithValue("id", idEgresoCaja));
            return lista.Count > 0 ? lista[0] : new EgresoCaja();
        }

        public List<EgresoCaja> getEgresosCajaByIds(List<int> ids)
        {
            var resultado = new List<EgresoCaja>();
            if (ids == null || ids.Count == 0) return resultado;

            var idsValidos = ids.Where(x => x > 0).Distinct().ToList();
            if (idsValidos.Count == 0) return resultado;

            const string sql = @"
                SELECT id, fechahora, idtipoegresocaja, descripcion, detalle, monto, idcompra, idsucursal, creado, creadopor, actualizado, actualizadopor, tabla, idtabla
                FROM egresoscaja WHERE id = ANY(@ids);";

            return DbPg.Reader(_connectionString, _idEmpresa, sql, dr =>
            {
                var egreso = MapEgresoCaja(dr);
                egreso.Tabla = GetString(dr, "tabla");
                egreso.IdTabla = dr["idtabla"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idtabla"]);
                return egreso;
            }, p => p.AddWithValue("ids", idsValidos.ToArray()));
        }

        public EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID)
        {
            const string sql = @"
                SELECT id, fechahora, idtipoegresocaja, descripcion, detalle, monto, idcompra, idsucursal, creado, creadopor, actualizado, actualizadopor, tabla, idtabla
                FROM egresoscaja WHERE tabla = @tabla AND idtabla = @idTabla ORDER BY id DESC LIMIT 1;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, dr =>
            {
                var egreso = MapEgresoCaja(dr);
                egreso.Tabla = GetString(dr, "tabla");
                egreso.IdTabla = dr["idtabla"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["idtabla"]);
                return egreso;
            }, p =>
            {
                p.AddWithValue("tabla", tabla ?? "");
                p.AddWithValue("idTabla", tablaID);
            });

            return lista.Count > 0 ? lista[0] : new EgresoCaja();
        }

        public float getMontoEgresosCajaVendedor(CierreCaja oCierre)
        {
            if (oCierre == null) throw new ArgumentNullException(nameof(oCierre));

            object obj = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT ROUND(CAST(SUM(monto) AS numeric), 2) FROM egresoscaja WHERE fechahora BETWEEN @fechaDesde AND @fechaHasta AND idsucursal = @idSucursal AND creadopor = @idVendedor;",
                p =>
                {
                    p.AddWithValue("fechaDesde", oCierre.FechaHoraInicio);
                    p.AddWithValue("fechaHasta", oCierre.FechaHoraCierre ?? DateTime.Now);
                    p.AddWithValue("idVendedor", oCierre.UsuarioInicio.Id);
                    p.AddWithValue("idSucursal", oCierre.Sucursal.idSucursal);
                });

            return obj == null || obj == DBNull.Value ? 0f : Convert.ToSingle(obj);
        }

        public DataTable getEgresosCajaVendedor(CierreCaja oCierre)
        {
            if (oCierre == null) throw new ArgumentNullException(nameof(oCierre));

            return obtenerEgresosCajaVendedorInterno(oCierre.Sucursal.idSucursal, oCierre.UsuarioInicio.Id, oCierre.FechaHoraInicio.Value, oCierre.FechaHoraCierre ?? DateTime.Now);
        }

        private DataTable obtenerEgresosCajaVendedorInterno(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql = @"
                SELECT ec.id, ec.fechahora AS ""Fecha"", te.tipoegresocaja AS ""TipoEgresoCaja"",
                    ec.descripcion AS ""Descripcion"", ec.detalle AS ""Detalle"", ROUND(CAST(ec.monto AS numeric), 2) AS ""Monto"",
                    te.esgasto AS ""Gasto"", ec.creado AS ""Creado"", cp.nombre AS ""Creado Por"",
                    ec.actualizado AS ""Actualizado"", ap.nombre AS ""Actualizado Por""
                FROM egresoscaja ec
                INNER JOIN tiposegresocaja te ON ec.idtipoegresocaja = te.id
                LEFT JOIN usuarios cp ON ec.creadopor = cp.id
                LEFT JOIN usuarios ap ON ec.actualizadopor = ap.id
                WHERE ec.fechahora BETWEEN @fechaDesde AND @fechaHasta
                  AND (@idSucursal = 0 OR ec.idsucursal = @idSucursal)
                  AND ec.creadopor = @idVendedor
                ORDER BY ec.fechahora DESC, ec.id DESC;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("fechaDesde", fechaDesde);
                p.AddWithValue("fechaHasta", fechaHasta);
                p.AddWithValue("idSucursal", idSucursal);
                p.AddWithValue("idVendedor", idVendedor);
            });
        }

        #endregion

        #region CambioSucursalCaja (Etapa 10)

        // Traduccion mecanica de Datos.CierreCaja.ConstruirPlanCambioSucursalCaja/
        // cambiarSucursalCaja/obtenerPreviewCambioSucursalCaja -- codigo C# puro en el original
        // (sin SPs), asi que este archivo es la fuente de verdad, ya leida completa.
        //
        // Unica divergencia de comportamiento (decidida en la Etapa 8, se concreta aca):
        // cierrecaja.id en Postgres es autoincremental, no namespaced por sucursal como en SQL
        // Server -- no hay nada que recalcular. Esta version solo actualiza idsucursal en
        // cierrecaja, nunca toca id. IdCierreCajaNuevo en el preview es siempre el mismo
        // IdCierreCaja (no hay renumeracion del lado Postgres).
        //
        // AuditoriaCambioSucursalCaja: en SQL Server el original la crea de forma perezosa (DDL
        // idempotente) la primera vez que se ejecuta un cambio real. En Postgres se provisiona
        // de entrada en la migracion de schema de esta etapa -- mismo estado final, sin DDL en
        // tiempo de ejecucion.
        private sealed class CambioSucursalCajaPlan
        {
            public Contratos.CambioSucursalCajaPreview Preview { get; set; }
            public List<int> VentasIds { get; set; } = new List<int>();
            public List<int> ComprasIds { get; set; } = new List<int>();
            public List<int> PagosIds { get; set; } = new List<int>();
            public List<int> EgresosIds { get; set; } = new List<int>();
            public List<int> MovimientosIds { get; set; } = new List<int>();
            public List<int> ExpendiosIds { get; set; } = new List<int>();
            public List<int> TemporalesIds { get; set; } = new List<int>();
            public int CortePorCompraCantidad { get; set; }
            public int MediaResCantidad { get; set; }
        }

        public Contratos.CambioSucursalCajaPreview obtenerPreviewCambioSucursalCaja(CierreCaja cierreCaja, int idSucursalNueva)
        {
            if (cierreCaja == null) throw new ArgumentNullException(nameof(cierreCaja));

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    var plan = ConstruirPlanCambioSucursalCaja(con, tx, cierreCaja, idSucursalNueva);
                    tx?.Commit();
                    return plan.Preview;
                }
                catch
                {
                    try { tx?.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public Contratos.CambioSucursalCajaResultado cambiarSucursalCaja(CierreCaja cierreCaja, int idSucursalNueva, int idUsuarioEjecutor, string usuarioEjecutor)
        {
            if (cierreCaja == null) throw new ArgumentNullException(nameof(cierreCaja));

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    var plan = ConstruirPlanCambioSucursalCaja(con, tx, cierreCaja, idSucursalNueva);
                    if (!plan.Preview.PuedeEjecutar)
                    {
                        tx?.Rollback();
                        return new Contratos.CambioSucursalCajaResultado
                        {
                            Ok = false,
                            Mensaje = plan.Preview.Mensaje,
                            Tablas = plan.Preview.Tablas
                        };
                    }

                    var counts = new List<Contratos.CambioSucursalCajaTabla>
                    {
                        new Contratos.CambioSucursalCajaTabla
                        {
                            Tabla = "CierreCaja",
                            Cantidad = EjecutarNonQueryCambioSucursal(con, tx,
                                "UPDATE cierrecaja SET idsucursal = @nuevaSucursal WHERE idempresa = @idEmpresa AND id = @idCierre AND idsucursal = @sucursalActual;",
                                cmd =>
                                {
                                    cmd.Parameters.AddWithValue("nuevaSucursal", plan.Preview.IdSucursalNueva);
                                    cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                                    cmd.Parameters.AddWithValue("idCierre", plan.Preview.IdCierreCaja);
                                    cmd.Parameters.AddWithValue("sucursalActual", plan.Preview.IdSucursalActual);
                                })
                        }
                    };
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "Ventas", Cantidad = ActualizarIdsCambioSucursal(con, tx, "ventas", "idventa", plan.VentasIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "Compras", Cantidad = ActualizarIdsCambioSucursal(con, tx, "compras", "idcompra", plan.ComprasIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "CortePorCompra", Cantidad = ActualizarPorComprasCambioSucursal(con, tx, "corteporcompra", plan.ComprasIds, plan.Preview.IdSucursalNueva, plan.Preview.IdSucursalActual) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "MediaRes", Cantidad = ActualizarPorComprasCambioSucursal(con, tx, "mediares", plan.ComprasIds, plan.Preview.IdSucursalNueva, plan.Preview.IdSucursalActual) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "Pagos", Cantidad = ActualizarIdsCambioSucursal(con, tx, "pagos", "id", plan.PagosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "EgresosCaja", Cantidad = ActualizarIdsCambioSucursal(con, tx, "egresoscaja", "id", plan.EgresosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "MovCtaCte", Cantidad = ActualizarIdsCambioSucursal(con, tx, "movctacte", "id", plan.MovimientosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "Expendios", Cantidad = ActualizarIdsCambioSucursal(con, tx, "expendios", "idexpendio", plan.ExpendiosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new Contratos.CambioSucursalCajaTabla { Tabla = "TemporalLineaVenta", Cantidad = ActualizarIdsCambioSucursal(con, tx, "temporallineaventa", "id", plan.TemporalesIds, plan.Preview.IdSucursalNueva) });

                    InsertarAuditoriaCambioSucursalCaja(con, tx, plan, counts, idUsuarioEjecutor, usuarioEjecutor);

                    tx?.Commit();

                    return new Contratos.CambioSucursalCajaResultado
                    {
                        Ok = true,
                        Mensaje = "La sucursal de la caja y sus operaciones asociadas se actualizó correctamente.",
                        Tablas = counts
                    };
                }
                catch (Exception ex)
                {
                    try { tx?.Rollback(); } catch { }
                    return new Contratos.CambioSucursalCajaResultado { Ok = false, Mensaje = ex.Message };
                }
            }
        }

        private CambioSucursalCajaPlan ConstruirPlanCambioSucursalCaja(NpgsqlConnection con, NpgsqlTransaction tx, CierreCaja cierreCaja, int idSucursalNueva)
        {
            var preview = new Contratos.CambioSucursalCajaPreview
            {
                PuedeEjecutar = false,
                Mensaje = "",
                IdCierreCaja = cierreCaja.Id,
                IdSucursalActual = cierreCaja.Sucursal != null ? cierreCaja.Sucursal.IdSucursal : 0,
                SucursalActual = cierreCaja.Sucursal != null ? cierreCaja.Sucursal.SucursalNombre : "",
                IdSucursalNueva = idSucursalNueva,
                IdUsuarioCaja = cierreCaja.UsuarioInicio != null ? cierreCaja.UsuarioInicio.Id : 0,
                UsuarioCaja = cierreCaja.UsuarioInicio != null ? cierreCaja.UsuarioInicio.Nombre : "",
                FechaDesde = cierreCaja.FechaHoraInicio ?? DateTime.Now,
                FechaHasta = cierreCaja.FechaHoraCierre ?? DateTime.Now
            };

            var plan = new CambioSucursalCajaPlan { Preview = preview };

            if (preview.IdCierreCaja <= 0 || preview.IdSucursalActual <= 0 || preview.IdUsuarioCaja <= 0)
            {
                preview.Mensaje = "No se pudo determinar la caja seleccionada.";
                return plan;
            }

            if (preview.IdSucursalNueva <= 0)
            {
                preview.Mensaje = "Seleccione la nueva sucursal.";
                return plan;
            }

            if (preview.IdSucursalNueva == preview.IdSucursalActual)
            {
                preview.Mensaje = "La sucursal nueva debe ser distinta de la sucursal actual.";
                return plan;
            }

            // Sin renumeracion en Postgres (id autoincremental, ver cabecera de la region).
            preview.IdCierreCajaNuevo = preview.IdCierreCaja;

            preview.SucursalNueva = ObtenerTextoCambioSucursal(con, tx,
                "SELECT sucursal FROM sucursal WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal LIMIT 1;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                    cmd.Parameters.AddWithValue("idSucursal", preview.IdSucursalNueva);
                });

            if (string.IsNullOrWhiteSpace(preview.SucursalNueva))
            {
                preview.Mensaje = "La sucursal destino no existe.";
                return plan;
            }

            int idCajaAbiertaDestino = ObtenerEnteroCambioSucursal(con, tx,
                "SELECT id FROM cierrecaja WHERE idempresa = @idEmpresa AND usuarioinicio = @usuarioInicio AND idsucursal = @idSucursal AND usuariocierre = '0' AND id <> @idCierre ORDER BY id DESC LIMIT 1;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                    cmd.Parameters.AddWithValue("usuarioInicio", preview.IdUsuarioCaja.ToString());
                    cmd.Parameters.AddWithValue("idSucursal", preview.IdSucursalNueva);
                    cmd.Parameters.AddWithValue("idCierre", preview.IdCierreCaja);
                });

            if (idCajaAbiertaDestino > 0)
            {
                preview.TieneCajaAbiertaEnDestino = true;
                preview.Mensaje = "El usuario ya tiene una caja abierta en la sucursal seleccionada. Debe cerrarla antes de mover los datos de la caja cargada en la sucursal incorrecta.";
                return plan;
            }

            plan.VentasIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT idventa FROM ventas WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND idvendedor = @idUsuario AND fechaventa BETWEEN @fechaDesde AND @fechaHasta;",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.ComprasIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT idcompra FROM compras WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND creadopor = @idUsuario AND fechacompra BETWEEN @fechaDesde AND @fechaHasta;",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.PagosIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT id FROM pagos WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND creadopor = @idUsuario AND fecha BETWEEN @fechaDesde AND @fechaHasta;",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.ExpendiosIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT idexpendio FROM expendios WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND idvendedor = @idUsuario AND fechaexpendio BETWEEN @fechaDesde AND @fechaHasta;",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.TemporalesIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT id FROM temporallineaventa WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND idvendedor = @idUsuario AND fechainiciopesada BETWEEN @fechaDesde AND @fechaHasta;",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.EgresosIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT DISTINCT id FROM egresoscaja WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND fechahora BETWEEN @fechaDesde AND @fechaHasta " +
                "AND (creadopor = @idUsuario OR idcompra = ANY(@comprasIds) OR (tabla = 'Ventas' AND idtabla = ANY(@ventasIds)) OR (tabla = 'Compras' AND idtabla = ANY(@comprasIds)) OR (tabla = 'Pagos' AND idtabla = ANY(@pagosIds)));",
                cmd =>
                {
                    AgregarParametrosBaseCambioSucursal(cmd, preview);
                    cmd.Parameters.AddWithValue("comprasIds", plan.ComprasIds.ToArray());
                    cmd.Parameters.AddWithValue("ventasIds", plan.VentasIds.ToArray());
                    cmd.Parameters.AddWithValue("pagosIds", plan.PagosIds.ToArray());
                });

            plan.MovimientosIds = ObtenerIdsCambioSucursal(con, tx,
                "SELECT DISTINCT id FROM movctacte WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND fecha BETWEEN @fechaDesde AND @fechaHasta " +
                "AND (creadopor = @idUsuario OR (tabla = 'Ventas' AND idtabla = ANY(@ventasIds)) OR (tabla = 'Compras' AND idtabla = ANY(@comprasIds)) OR (tabla = 'Pagos' AND idtabla = ANY(@pagosIds)));",
                cmd =>
                {
                    AgregarParametrosBaseCambioSucursal(cmd, preview);
                    cmd.Parameters.AddWithValue("ventasIds", plan.VentasIds.ToArray());
                    cmd.Parameters.AddWithValue("comprasIds", plan.ComprasIds.ToArray());
                    cmd.Parameters.AddWithValue("pagosIds", plan.PagosIds.ToArray());
                });

            plan.CortePorCompraCantidad = ObtenerCantidadPorComprasCambioSucursal(con, tx, "corteporcompra", plan.ComprasIds, preview.IdSucursalActual);
            plan.MediaResCantidad = ObtenerCantidadPorComprasCambioSucursal(con, tx, "mediares", plan.ComprasIds, preview.IdSucursalActual);

            preview.Tablas = new List<Contratos.CambioSucursalCajaTabla>
            {
                new Contratos.CambioSucursalCajaTabla { Tabla = "CierreCaja", Cantidad = 1 },
                new Contratos.CambioSucursalCajaTabla { Tabla = "Ventas", Cantidad = plan.VentasIds.Count },
                new Contratos.CambioSucursalCajaTabla { Tabla = "Compras", Cantidad = plan.ComprasIds.Count },
                new Contratos.CambioSucursalCajaTabla { Tabla = "CortePorCompra", Cantidad = plan.CortePorCompraCantidad },
                new Contratos.CambioSucursalCajaTabla { Tabla = "MediaRes", Cantidad = plan.MediaResCantidad },
                new Contratos.CambioSucursalCajaTabla { Tabla = "Pagos", Cantidad = plan.PagosIds.Count },
                new Contratos.CambioSucursalCajaTabla { Tabla = "EgresosCaja", Cantidad = plan.EgresosIds.Count },
                new Contratos.CambioSucursalCajaTabla { Tabla = "MovCtaCte", Cantidad = plan.MovimientosIds.Count },
                new Contratos.CambioSucursalCajaTabla { Tabla = "Expendios", Cantidad = plan.ExpendiosIds.Count },
                new Contratos.CambioSucursalCajaTabla { Tabla = "TemporalLineaVenta", Cantidad = plan.TemporalesIds.Count }
            };

            preview.PuedeEjecutar = true;
            preview.Mensaje = "Vista previa generada correctamente.";
            return plan;
        }

        private void AgregarParametrosBaseCambioSucursal(NpgsqlCommand cmd, Contratos.CambioSucursalCajaPreview preview)
        {
            cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
            cmd.Parameters.AddWithValue("idSucursal", preview.IdSucursalActual);
            cmd.Parameters.AddWithValue("idUsuario", preview.IdUsuarioCaja);
            cmd.Parameters.AddWithValue("fechaDesde", preview.FechaDesde);
            cmd.Parameters.AddWithValue("fechaHasta", preview.FechaHasta);
        }

        private List<int> ObtenerIdsCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string sql, Action<NpgsqlCommand> setParams)
        {
            var ids = new List<int>();
            using (var cmd = new NpgsqlCommand(sql, con, tx))
            {
                setParams?.Invoke(cmd);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        if (dr[0] != DBNull.Value)
                            ids.Add(Convert.ToInt32(dr[0]));
                    }
                }
            }
            return ids;
        }

        private int ObtenerEnteroCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string sql, Action<NpgsqlCommand> setParams)
        {
            using (var cmd = new NpgsqlCommand(sql, con, tx))
            {
                setParams?.Invoke(cmd);
                object obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
            }
        }

        private string ObtenerTextoCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string sql, Action<NpgsqlCommand> setParams)
        {
            using (var cmd = new NpgsqlCommand(sql, con, tx))
            {
                setParams?.Invoke(cmd);
                object obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? "" : Convert.ToString(obj);
            }
        }

        private int EjecutarNonQueryCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string sql, Action<NpgsqlCommand> setParams)
        {
            using (var cmd = new NpgsqlCommand(sql, con, tx))
            {
                setParams?.Invoke(cmd);
                return cmd.ExecuteNonQuery();
            }
        }

        private int ActualizarIdsCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string tabla, string campoId, List<int> ids, int idSucursalNueva)
        {
            if (ids == null || ids.Count == 0) return 0;

            using (var cmd = new NpgsqlCommand(
                "UPDATE " + tabla + " SET idsucursal = @idSucursalNueva WHERE idempresa = @idEmpresa AND " + campoId + " = ANY(@ids);", con, tx))
            {
                cmd.Parameters.AddWithValue("idSucursalNueva", idSucursalNueva);
                cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                cmd.Parameters.AddWithValue("ids", ids.ToArray());
                return cmd.ExecuteNonQuery();
            }
        }

        private int ActualizarPorComprasCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string tabla, List<int> comprasIds, int idSucursalNueva, int idSucursalActual)
        {
            if (comprasIds == null || comprasIds.Count == 0) return 0;

            using (var cmd = new NpgsqlCommand(
                "UPDATE " + tabla + " SET idsucursal = @idSucursalNueva WHERE idempresa = @idEmpresa AND idsucursal = @idSucursalActual AND idcompra = ANY(@ids);", con, tx))
            {
                cmd.Parameters.AddWithValue("idSucursalNueva", idSucursalNueva);
                cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                cmd.Parameters.AddWithValue("idSucursalActual", idSucursalActual);
                cmd.Parameters.AddWithValue("ids", comprasIds.ToArray());
                return cmd.ExecuteNonQuery();
            }
        }

        private int ObtenerCantidadPorComprasCambioSucursal(NpgsqlConnection con, NpgsqlTransaction tx, string tabla, List<int> comprasIds, int idSucursalActual)
        {
            if (comprasIds == null || comprasIds.Count == 0) return 0;

            using (var cmd = new NpgsqlCommand(
                "SELECT COUNT(1) FROM " + tabla + " WHERE idempresa = @idEmpresa AND idsucursal = @idSucursal AND idcompra = ANY(@ids);", con, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                cmd.Parameters.AddWithValue("idSucursal", idSucursalActual);
                cmd.Parameters.AddWithValue("ids", comprasIds.ToArray());
                object obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
            }
        }

        private void InsertarAuditoriaCambioSucursalCaja(NpgsqlConnection con, NpgsqlTransaction tx, CambioSucursalCajaPlan plan, List<Contratos.CambioSucursalCajaTabla> counts, int idUsuarioEjecutor, string usuarioEjecutor)
        {
            string detalle = "Esta accion corrigio la sucursal de la caja y de las operaciones asociadas." + Environment.NewLine +
                             "Caja anterior: " + plan.Preview.IdCierreCaja + Environment.NewLine +
                             "Caja nueva: " + plan.Preview.IdCierreCajaNuevo + Environment.NewLine +
                             "Sucursal anterior: " + plan.Preview.SucursalActual + " (" + plan.Preview.IdSucursalActual + ")" + Environment.NewLine +
                             "Sucursal nueva: " + plan.Preview.SucursalNueva + " (" + plan.Preview.IdSucursalNueva + ")" + Environment.NewLine +
                             "Usuario caja: " + plan.Preview.UsuarioCaja + " (" + plan.Preview.IdUsuarioCaja + ")" + Environment.NewLine +
                             "Rango: " + plan.Preview.FechaDesde.ToString("dd/MM/yyyy HH:mm:ss") + " - " + plan.Preview.FechaHasta.ToString("dd/MM/yyyy HH:mm:ss");

            foreach (var item in counts)
                detalle += Environment.NewLine + item.Tabla + ": " + item.Cantidad;

            using (var cmd = new NpgsqlCommand(@"
                INSERT INTO auditoriacambiosucursalcaja (idempresa, idcierrecaja, idusuarioejecutor, usuarioejecutor, idusuariocaja, usuariocaja,
                    idsucursalanterior, sucursalanterior, idsucursalnueva, sucursalnueva, fechadesde, fechahasta, fechacambio, detalle)
                VALUES (@idEmpresa, @idCierreCaja, @idUsuarioEjecutor, @usuarioEjecutor, @idUsuarioCaja, @usuarioCaja,
                    @idSucursalAnterior, @sucursalAnterior, @idSucursalNueva, @sucursalNueva, @fechaDesde, @fechaHasta, @fechaCambio, @detalle);", con, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", _idEmpresa);
                cmd.Parameters.AddWithValue("idCierreCaja", plan.Preview.IdCierreCaja);
                cmd.Parameters.AddWithValue("idUsuarioEjecutor", idUsuarioEjecutor);
                cmd.Parameters.AddWithValue("usuarioEjecutor", usuarioEjecutor ?? "");
                cmd.Parameters.AddWithValue("idUsuarioCaja", plan.Preview.IdUsuarioCaja);
                cmd.Parameters.AddWithValue("usuarioCaja", plan.Preview.UsuarioCaja ?? "");
                cmd.Parameters.AddWithValue("idSucursalAnterior", plan.Preview.IdSucursalActual);
                cmd.Parameters.AddWithValue("sucursalAnterior", plan.Preview.SucursalActual ?? "");
                cmd.Parameters.AddWithValue("idSucursalNueva", plan.Preview.IdSucursalNueva);
                cmd.Parameters.AddWithValue("sucursalNueva", plan.Preview.SucursalNueva ?? "");
                cmd.Parameters.AddWithValue("fechaDesde", plan.Preview.FechaDesde);
                cmd.Parameters.AddWithValue("fechaHasta", plan.Preview.FechaHasta);
                cmd.Parameters.AddWithValue("fechaCambio", DateTime.Now);
                cmd.Parameters.AddWithValue("detalle", detalle);
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
