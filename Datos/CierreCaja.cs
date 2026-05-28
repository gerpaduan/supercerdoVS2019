using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class CierreCaja
    {
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public sealed class CambioSucursalCajaTabla
        {
            public string Tabla { get; set; }
            public int Cantidad { get; set; }
        }

        public sealed class CambioSucursalCajaPreview
        {
            public bool PuedeEjecutar { get; set; }
            public string Mensaje { get; set; }
            public int IdCierreCaja { get; set; }
            public int IdCierreCajaNuevo { get; set; }
            public int IdSucursalActual { get; set; }
            public string SucursalActual { get; set; }
            public int IdSucursalNueva { get; set; }
            public string SucursalNueva { get; set; }
            public int IdUsuarioCaja { get; set; }
            public string UsuarioCaja { get; set; }
            public DateTime FechaDesde { get; set; }
            public DateTime FechaHasta { get; set; }
            public bool TieneCajaAbiertaEnDestino { get; set; }
            public List<CambioSucursalCajaTabla> Tablas { get; set; } = new List<CambioSucursalCajaTabla>();
        }

        public sealed class CambioSucursalCajaResultado
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; }
            public List<CambioSucursalCajaTabla> Tablas { get; set; } = new List<CambioSucursalCajaTabla>();
        }

        private sealed class CambioSucursalCajaPlan
        {
            public CambioSucursalCajaPreview Preview { get; set; }
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

        private static bool ColumnaExiste(SqlDataReader dr, string columna)
        {
            try
            {
                return dr.GetOrdinal(columna) >= 0;
            }
            catch
            {
                return false;
            }
        }

        public CierreCaja(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
        }

        public DataTable findCierreCaja(
            Entidades.CierreCaja oCierreParam,
            Entidades.CierreCaja.tipoBusqueda tipoBusquedaParam,
            string texto,
            DateTime? fechaDesde)
        {
            if (oCierreParam == null) throw new ArgumentNullException(nameof(oCierreParam));

            string selectText;

            switch (tipoBusquedaParam)
            {
                case Entidades.CierreCaja.tipoBusqueda.FindAll:
                    selectText =
                        "select CierreCaja.id, Usuarios.nombre as Iniciada_Por, fechaHoraInicio as Inicio, fechaHoraCierre as Cierre, " +
                        "round(cajaInicio, 2) as Caja_Inicial, round(ventas, 2) as Ventas, round(gastos, 2) as EgresosCaja, round(cajaCierre, 2) as Caja_Cierre, round(diferencia, 2) as Diferencia, " +
                        "round(cajaInicioSiguiente, 2) as Caja_Ini_Sig, round(importeRetirado, 2) as Retirado, " +
                        "UsuarioCierre.nombre as Cerrada_Por " +
                        "from CierreCaja " +
                        "inner join Usuarios on CierreCaja.usuarioInicio = Usuarios.id " +
                        "inner join Usuarios as UsuarioCierre on CierreCaja.usuarioCierre = UsuarioCierre.id " +
                        "where idSucursal = @sucursal " +
                        "and fechaHoraInicio > @fechaDesde " +
                        "and Usuarios.nombre like @texto " +
                        "order by CierreCaja.id desc";
                    break;

                case Entidades.CierreCaja.tipoBusqueda.FindOpen:
                    selectText =
                        "select CierreCaja.id, CierreCaja.idSucursal, CierreCaja.usuarioInicio, Usuarios.nombre as vendedor, " +
                        "Sucursal.sucursal, fechaHoraInicio, " +
                        "round(cajaInicio, 2) as cajaInicio " +
                        "from CierreCaja " +
                        "inner join Usuarios on CierreCaja.usuarioInicio = Usuarios.id " +
                        "inner join Sucursal on CierreCaja.idSucursal = Sucursal.idSucursal " +
                        "where (@sucursal is null or @sucursal = 0 or CierreCaja.idSucursal = @sucursal) " +
                        "and Usuarios.nombre like @texto " +
                        "and CierreCaja.usuarioCierre = 0";
                    break;

                case Entidades.CierreCaja.tipoBusqueda.FindById:
                    selectText = "select * from CierreCaja where id = @id";
                    break;

                case Entidades.CierreCaja.tipoBusqueda.FindLast:
                    selectText =
                        "select top 1 * from CierreCaja " +
                        "where idSucursal = @sucursal and usuarioInicio = @usuarioInicio " +
                        "order by id desc";
                    break;

                case Entidades.CierreCaja.tipoBusqueda.FindLastOpen:
                    selectText =
                        "select top 1 * from CierreCaja " +
                        "where usuarioInicio = @usuarioInicio and id < @id " +
                        "order by id desc";
                    break;

                default:
                    selectText = "select top 0 * from CierreCaja";
                    break;
            }

            return Db.DataTable(
                _empresa,
                selectText,
                CommandType.Text,
                p =>
                {
                    switch (tipoBusquedaParam)
                    {
                        case Entidades.CierreCaja.tipoBusqueda.FindAll:
                            p.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal.idSucursal;
                            p.Add("@fechaDesde", SqlDbType.DateTime).Value = (object)fechaDesde ?? DateTime.MinValue;
                            p.Add("@texto", SqlDbType.NVarChar, 100).Value = "%" + (texto ?? "") + "%";
                            break;
                        case Entidades.CierreCaja.tipoBusqueda.FindOpen:
                            p.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal?.idSucursal ?? 0;
                            p.Add("@texto", SqlDbType.NVarChar, 100).Value = "%" + (texto ?? "") + "%";
                            break;
                        case Entidades.CierreCaja.tipoBusqueda.FindById:
                            p.Add("@id", SqlDbType.Int).Value = oCierreParam.Id;
                            break;

                        case Entidades.CierreCaja.tipoBusqueda.FindLast:
                            p.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal.idSucursal;
                            p.Add("@usuarioInicio", SqlDbType.Int).Value = oCierreParam.UsuarioInicio.Id;
                            break;

                        case Entidades.CierreCaja.tipoBusqueda.FindLastOpen:
                            p.Add("@usuarioInicio", SqlDbType.Int).Value = oCierreParam.UsuarioInicio.Id;
                            p.Add("@id", SqlDbType.Int).Value = oCierreParam.Id;
                            break;
                    }
                }
            );
        }

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreCajaE)
        {
            if (oCierreCajaE == null) throw new ArgumentNullException(nameof(oCierreCajaE));

            Db.NonQuery(
                _empresa,
                "addOrEditCierreCaja",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@id", oCierreCajaE.Id);
                    p.AddWithValue("@idSucursal", oCierreCajaE.Sucursal.IdSucursal);
                    p.AddWithValue("@fechaHoraInicio", oCierreCajaE.FechaHoraInicio);
                    p.AddWithValue("@fechaHoraCierre", (object)oCierreCajaE.FechaHoraCierre ?? DBNull.Value);
                    p.AddWithValue("@cajaInicio", oCierreCajaE.CajaInicio);
                    p.AddWithValue("@ventas", oCierreCajaE.Ventas);
                    p.AddWithValue("@gastos", oCierreCajaE.EgresosCaja);
                    p.AddWithValue("@cajaCierre", oCierreCajaE.CajaCierre);
                    p.AddWithValue("@diferencia", oCierreCajaE.Diferencia);
                    p.AddWithValue("@cajaInicioSiguiente", oCierreCajaE.CajaInicioSiguiente);
                    p.AddWithValue("@importeRetirado", oCierreCajaE.ImporteRetirado);
                    p.AddWithValue("@usuarioInicio", oCierreCajaE.UsuarioInicio.Id);
                    p.AddWithValue("@usuarioCierre", oCierreCajaE.UsuarioCierre != null ? oCierreCajaE.UsuarioCierre.Id : 0);
                }
            );
        }

        public DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja)
        {
            var dt = new DataTable();
            if (listaCierreCaja == null || listaCierreCaja.Count == 0) return dt;

            var ids = new List<int>();
            foreach (var c in listaCierreCaja)
            {
                if (c != null && c.Id > 0)
                    ids.Add(c.Id);
            }

            if (ids.Count == 0) return dt;

            // IN parametrizado: (@p0,@p1,...)
            var paramNames = new List<string>();
            for (int i = 0; i < ids.Count; i++)
                paramNames.Add("@p" + i);

            string sql =
                "select CierreCaja.*, Usuarios.* " +
                "FROM CierreCaja INNER JOIN Usuarios ON CierreCaja.usuarioInicio = Usuarios.id " +
                "where CierreCaja.id IN (" + string.Join(",", paramNames) + ")";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    for (int i = 0; i < ids.Count; i++)
                        p.Add(paramNames[i], SqlDbType.Int).Value = ids[i];
                }
            );
        }

        #region EgresosCaja

        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso)
        {
            string sql =
                "Select id, tipoEgresoCaja, esGasto as Es_Gasto, creado as Creado, actualizado as Actualizado, reservadoSistema as Reservado " +
                "from TiposEgresoCaja " +
                "where (@id = 0 OR id = @id) " +
                "and (@txt = '' OR tipoEgresoCaja LIKE @likeTxt) " +
                "order by orden, tipoEgresoCaja";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = idTipoEgreso > 0 ? idTipoEgreso : 0;
                    p.Add("@txt", SqlDbType.NVarChar, 100).Value = buscarText ?? "";
                    p.Add("@likeTxt", SqlDbType.NVarChar, 120).Value = "%" + (buscarText ?? "") + "%";
                }
            );
        }

        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto)
        {
            bool esInsert = (id == -1);

            using (SqlConnection cn = Db.Open(_empresa))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;

                if (esInsert)
                {
                    //TODO: ver sugerencia debajo
                    // Nota: MAX+1 puede colisionar si 2 insertan a la vez.
                    // Ideal: identity o sequence. Mantengo tu lógica.
                    cmd.CommandText = "SELECT ISNULL(MAX(id), 0) FROM TiposEgresoCaja";
                    object result = cmd.ExecuteScalar();
                    id = (result == null || result == DBNull.Value) ? 1 : (Convert.ToInt32(result) + 1);

                    cmd.Parameters.Clear();
                    cmd.CommandText =
                        "INSERT INTO TiposEgresoCaja (id, tipoEgresoCaja, esGasto, orden, reservadoSistema, creado) " +
                        "VALUES (@id, @tipoEgresoCaja, @esGasto, 10, @reservadoSistema, @creado)";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@tipoEgresoCaja", SqlDbType.NVarChar, 100).Value = tipoEgresoCaja ?? "";
                    cmd.Parameters.Add("@esGasto", SqlDbType.Bit).Value = esGasto;
                    cmd.Parameters.Add("@reservadoSistema", SqlDbType.Bit).Value = false;
                    cmd.Parameters.Add("@creado", SqlDbType.DateTime).Value = DateTime.Now;
                }
                else
                {
                    cmd.CommandText =
                        "UPDATE TiposEgresoCaja " +
                        "SET tipoEgresoCaja = @tipoEgresoCaja, esGasto = @esGasto, actualizado = @actualizado " +
                        "WHERE id = @id";
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@tipoEgresoCaja", SqlDbType.NVarChar, 100).Value = tipoEgresoCaja ?? "";
                    cmd.Parameters.Add("@esGasto", SqlDbType.Bit).Value = esGasto;
                    cmd.Parameters.Add("@actualizado", SqlDbType.DateTime).Value = DateTime.Now;
                }

                cmd.ExecuteNonQuery();
            }
        }

        public void eliminarTipoEgreso(int id)
        {
            const string sql = "DELETE FROM TiposEgresoCaja WHERE id = @id";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                p => p.Add("@id", SqlDbType.Int).Value = id
            );
        }

        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return Db.DataTable(
                _empresa,
                "obtenerEgresosCaja",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@texto", texto ?? "");
                    p.AddWithValue("@fechaDesde", fechaDesde);
                    p.AddWithValue("@fechaHasta", fechaHasta);
                    p.AddWithValue("@idVendedor", idUsuario);
                    p.AddWithValue("@idTipoEgresoCaja", idTipoEgresoCaja);
                    p.AddWithValue("@idSucursal", idSucursal);
                }
            );
        }

        public DataTable obtenerEgresosCajaGastosBalance(int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            const string sql =
                "SELECT " +
                "   dbo.EgresosCaja.id, " +
                "   dbo.EgresosCaja.fechaHora AS Fecha, " +
                "   dbo.EgresosCaja.idTipoEgresoCaja, " +
                "   dbo.TiposEgresoCaja.tipoEgresoCaja AS TipoEgresoCaja, " +
                "   dbo.EgresosCaja.descripcion AS Descripcion, " +
                "   dbo.EgresosCaja.detalle AS Detalle, " +
                "   ROUND(dbo.EgresosCaja.monto, 2) AS Monto, " +
                "   dbo.TiposEgresoCaja.esGasto AS Gasto " +
                "FROM dbo.EgresosCaja " +
                "INNER JOIN dbo.TiposEgresoCaja ON dbo.EgresosCaja.idTipoEgresoCaja = dbo.TiposEgresoCaja.id " +
                "WHERE dbo.EgresosCaja.fechaHora BETWEEN @fechaDesde AND @fechaHasta " +
                "  AND dbo.EgresosCaja.idSucursal = @idSucursal " +
                "  AND dbo.TiposEgresoCaja.esGasto = 1 " +
                "ORDER BY dbo.EgresosCaja.fechaHora DESC";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                p =>
                {
                    p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;
                    p.Add("@fechaHasta", SqlDbType.DateTime).Value = fechaHasta;
                    p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal;
                }
            );
        }

        public Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja)
        {
            if (oEgresoCaja == null) throw new ArgumentNullException(nameof(oEgresoCaja));

            object obj = Db.Scalar(
                _empresa,
                "addOrEditEgresoCaja",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@id", oEgresoCaja.Id);
                    p.AddWithValue("@fecha", oEgresoCaja.Fecha);
                    p.AddWithValue("@idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja);
                    p.AddWithValue("@descripcion", oEgresoCaja.Descripcion ?? "");
                    p.AddWithValue("@detalle", oEgresoCaja.Detalle ?? "");
                    p.AddWithValue("@monto", oEgresoCaja.Monto);
                    p.AddWithValue("@idCompra", (object)oEgresoCaja.IdCompra ?? DBNull.Value);
                    p.AddWithValue("@tabla", oEgresoCaja.Tabla ?? "");
                    p.AddWithValue("@idTabla", (object)oEgresoCaja.IdTabla ?? DBNull.Value);
                    p.AddWithValue("@idSucursal", oEgresoCaja.Sucursal.idSucursal);
                    p.AddWithValue("@creadoPor", oEgresoCaja.CreadoPor);
                    p.AddWithValue("@actualizadoPor", oEgresoCaja.ActualizadoPor);
                }
            );

            oEgresoCaja.Id = (obj == null || obj == DBNull.Value) ? oEgresoCaja.Id : Convert.ToInt32(obj);
            return oEgresoCaja;
        }

        public Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja)
        {
            var lista = Db.Reader(
                _empresa,
                "obtenerEgresosCaja",
                CommandType.StoredProcedure,
                dr => cargarEgresoCajaDataReader(dr),
                p => p.AddWithValue("@id", idEgresoCaja)
            );

            return lista.Count > 0 ? lista[0] : new Entidades.EgresoCaja();
        }

        private Entidades.EgresoCaja cargarEgresoCajaDataReader(SqlDataReader dr)
        {
            Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();

            oEgresoCaja.Id = Convert.ToInt32(dr["id"].ToString());
            oEgresoCaja.Fecha = Convert.ToDateTime(dr["fechaHora"].ToString());
            oEgresoCaja.IdTipoEgresoCaja = Convert.ToInt32(dr["idTipoEgresoCaja"].ToString());
            oEgresoCaja.TipoEgresoCaja = ColumnaExiste(dr, "tipoEgresoCaja") && dr["tipoEgresoCaja"] != DBNull.Value
                ? dr["tipoEgresoCaja"].ToString()
                : "";
            oEgresoCaja.Descripcion = dr["descripcion"] != DBNull.Value ? dr["descripcion"].ToString() : "";
            oEgresoCaja.Detalle = dr["detalle"] != DBNull.Value ? dr["detalle"].ToString() : "";
            oEgresoCaja.Monto = dr["monto"] == DBNull.Value ? 0f : float.Parse(dr["monto"].ToString());
            oEgresoCaja.IdCompra = dr["idCompra"] != DBNull.Value ? Convert.ToInt32(dr["idCompra"].ToString()) : (int?)null;

            oEgresoCaja.Sucursal = new Entidades.Sucursal();
            oEgresoCaja.Sucursal.idSucursal = Convert.ToInt32(dr["idSucursal"].ToString());

            oEgresoCaja.Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"].ToString());
            oEgresoCaja.CreadoPor = dr["creadoPor"] == DBNull.Value ? 0 : Convert.ToInt32(dr["creadoPor"].ToString());
            oEgresoCaja.Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"].ToString());
            oEgresoCaja.ActualizadoPor = dr["actualizadoPor"] == DBNull.Value ? -1 : Convert.ToInt32(dr["actualizadoPor"].ToString());

            return oEgresoCaja;
        }

        public Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID)
        {
            const string sql =
                "SELECT TOP 1 EgresosCaja.* " +
                "FROM EgresosCaja " +
                "WHERE tabla = @tabla AND idTabla = @idTabla " +
                "ORDER BY EgresosCaja.id DESC";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => cargarEgresoCajaDataReader(dr),
                p =>
                {
                    p.Add("@tabla", SqlDbType.NVarChar, 50).Value = tabla ?? "";
                    p.Add("@idTabla", SqlDbType.Int).Value = tablaID;
                }
            );

            return lista.Count > 0 ? lista[0] : new Entidades.EgresoCaja();
        }

        public float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierre)
        {
            if (oCierre == null) throw new ArgumentNullException(nameof(oCierre));

            object obj = Db.Scalar(
                _empresa,
                "obtenerEgresosCaja",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
                    p.AddWithValue("@fechaHasta", oCierre.FechaHoraCierre == null ? DateTime.Now : oCierre.FechaHoraCierre);
                    p.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
                    p.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
                    p.AddWithValue("@montoEgresoCaja", true);
                }
            );

            return (obj == null || obj == DBNull.Value) ? 0f : float.Parse(obj.ToString());
        }

        public DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierre)
        {
            if (oCierre == null) throw new ArgumentNullException(nameof(oCierre));

            return Db.DataTable(
                _empresa,
                "obtenerEgresosCaja",
                CommandType.StoredProcedure,
                p =>
                {
                    p.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
                    p.AddWithValue("@fechaHasta", oCierre.FechaHoraCierre == null ? DateTime.Now : oCierre.FechaHoraCierre);
                    p.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
                    p.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
                    p.AddWithValue("@verEgresoCaja", true);
                }
            );
        }

        public CambioSucursalCajaPreview obtenerPreviewCambioSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva)
        {
            if (cierreCaja == null) throw new ArgumentNullException(nameof(cierreCaja));

            using (SqlConnection cn = Db.Open(_empresa))
            {
                return ConstruirPlanCambioSucursalCaja(cierreCaja, idSucursalNueva, cn, null).Preview;
            }
        }

        public CambioSucursalCajaResultado cambiarSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva, int idUsuarioEjecutor, string usuarioEjecutor)
        {
            if (cierreCaja == null) throw new ArgumentNullException(nameof(cierreCaja));

            using (SqlConnection cn = Db.Open(_empresa))
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    var plan = ConstruirPlanCambioSucursalCaja(cierreCaja, idSucursalNueva, cn, tx);
                    if (!plan.Preview.PuedeEjecutar)
                    {
                        tx.Rollback();
                        return new CambioSucursalCajaResultado
                        {
                            Ok = false,
                            Mensaje = plan.Preview.Mensaje,
                            Tablas = plan.Preview.Tablas
                        };
                    }

                    var counts = new List<CambioSucursalCajaTabla>();
                    counts.Add(new CambioSucursalCajaTabla
                    {
                        Tabla = "CierreCaja",
                        Cantidad = EjecutarNonQuery(cn, tx,
                            "UPDATE CierreCaja SET id = @nuevoIdCierre, idSucursal = @nuevaSucursal WHERE idEmpresa = @idEmpresa AND id = @idCierre AND idSucursal = @sucursalActual",
                            cmd =>
                            {
                                cmd.Parameters.Add("@nuevoIdCierre", SqlDbType.Int).Value = plan.Preview.IdCierreCajaNuevo;
                                cmd.Parameters.Add("@nuevaSucursal", SqlDbType.Int).Value = plan.Preview.IdSucursalNueva;
                                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                                cmd.Parameters.Add("@idCierre", SqlDbType.Int).Value = plan.Preview.IdCierreCaja;
                                cmd.Parameters.Add("@sucursalActual", SqlDbType.Int).Value = plan.Preview.IdSucursalActual;
                            })
                    });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "Ventas", Cantidad = ActualizarIds(cn, tx, "Ventas", "idVenta", plan.VentasIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "Compras", Cantidad = ActualizarIds(cn, tx, "Compras", "idCompra", plan.ComprasIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "CortePorCompra", Cantidad = ActualizarPorCompras(cn, tx, "CortePorCompra", plan.ComprasIds, plan.Preview.IdSucursalNueva, plan.Preview.IdSucursalActual) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "MediaRes", Cantidad = ActualizarPorCompras(cn, tx, "MediaRes", plan.ComprasIds, plan.Preview.IdSucursalNueva, plan.Preview.IdSucursalActual) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "Pagos", Cantidad = ActualizarIds(cn, tx, "Pagos", "id", plan.PagosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "EgresosCaja", Cantidad = ActualizarIds(cn, tx, "EgresosCaja", "id", plan.EgresosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "MovCtaCte", Cantidad = ActualizarIds(cn, tx, "MovCtaCte", "id", plan.MovimientosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "Expendios", Cantidad = ActualizarIds(cn, tx, "Expendios", "idExpendio", plan.ExpendiosIds, plan.Preview.IdSucursalNueva) });
                    counts.Add(new CambioSucursalCajaTabla { Tabla = "TemporalLineaVenta", Cantidad = ActualizarIds(cn, tx, "TemporalLineaVenta", "id", plan.TemporalesIds, plan.Preview.IdSucursalNueva) });

                    AsegurarTablaAuditoriaCambioSucursalCaja(cn, tx);
                    InsertarAuditoriaCambioSucursalCaja(cn, tx, plan, counts, idUsuarioEjecutor, usuarioEjecutor);

                    tx.Commit();

                    return new CambioSucursalCajaResultado
                    {
                        Ok = true,
                        Mensaje = "La sucursal de la caja y sus operaciones asociadas se actualizó correctamente.",
                        Tablas = counts
                    };
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    return new CambioSucursalCajaResultado
                    {
                        Ok = false,
                        Mensaje = ex.Message
                    };
                }
            }
        }

        private CambioSucursalCajaPlan ConstruirPlanCambioSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva, SqlConnection cn, SqlTransaction tx)
        {
            var preview = new CambioSucursalCajaPreview
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

            preview.IdCierreCajaNuevo = CalcularNuevoIdCierreCaja(cn, tx, preview.IdCierreCaja, preview.IdSucursalNueva);
            if (preview.IdCierreCajaNuevo <= 0)
            {
                preview.Mensaje = "No se pudo calcular el nuevo identificador de la caja.";
                return plan;
            }

            preview.SucursalNueva = ObtenerTexto(
                cn,
                tx,
                "SELECT TOP 1 sucursal FROM Sucursal WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal",
                cmd =>
                {
                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = preview.IdSucursalNueva;
                });

            if (string.IsNullOrWhiteSpace(preview.SucursalNueva))
            {
                preview.Mensaje = "La sucursal destino no existe.";
                return plan;
            }

            int idCajaAbiertaDestino = ObtenerEntero(
                cn,
                tx,
                "SELECT TOP 1 id FROM CierreCaja WHERE idEmpresa = @idEmpresa AND usuarioInicio = @usuarioInicio AND idSucursal = @idSucursal AND usuarioCierre = 0 AND id <> @idCierre ORDER BY id DESC",
                cmd =>
                {
                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    cmd.Parameters.Add("@usuarioInicio", SqlDbType.Int).Value = preview.IdUsuarioCaja;
                    cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = preview.IdSucursalNueva;
                    cmd.Parameters.Add("@idCierre", SqlDbType.Int).Value = preview.IdCierreCaja;
                });

            if (idCajaAbiertaDestino > 0)
            {
                preview.TieneCajaAbiertaEnDestino = true;
                preview.Mensaje = "El usuario ya tiene una caja abierta en la sucursal seleccionada. Debe cerrarla antes de mover los datos de la caja cargada en la sucursal incorrecta.";
                return plan;
            }

            plan.VentasIds = ObtenerIds(
                cn,
                tx,
                "SELECT idVenta FROM Ventas WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND idVendedor = @idUsuario AND fechaVenta BETWEEN @fechaDesde AND @fechaHasta",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.ComprasIds = ObtenerIds(
                cn,
                tx,
                "SELECT idCompra FROM Compras WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND creadoPor = @idUsuario AND fechaCompra BETWEEN @fechaDesde AND @fechaHasta",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.PagosIds = ObtenerIds(
                cn,
                tx,
                "SELECT id FROM Pagos WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND creadoPor = @idUsuario AND fecha BETWEEN @fechaDesde AND @fechaHasta",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.ExpendiosIds = ObtenerIds(
                cn,
                tx,
                "SELECT idExpendio FROM Expendios WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND idVendedor = @idUsuario AND fechaExpendio BETWEEN @fechaDesde AND @fechaHasta",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.TemporalesIds = ObtenerIds(
                cn,
                tx,
                "SELECT id FROM TemporalLineaVenta WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND idVendedor = @idUsuario AND fechaInicioPesada BETWEEN @fechaDesde AND @fechaHasta",
                cmd => AgregarParametrosBaseCambioSucursal(cmd, preview));

            plan.EgresosIds = ObtenerIdsEgresosCaja(cn, tx, preview, plan.ComprasIds, plan.VentasIds, plan.PagosIds);
            plan.MovimientosIds = ObtenerIdsMovimientosCaja(cn, tx, preview, plan.ComprasIds, plan.VentasIds, plan.PagosIds);
            plan.CortePorCompraCantidad = ObtenerCantidadPorCompras(cn, tx, "CortePorCompra", plan.ComprasIds, preview.IdSucursalActual);
            plan.MediaResCantidad = ObtenerCantidadPorCompras(cn, tx, "MediaRes", plan.ComprasIds, preview.IdSucursalActual);

            preview.Tablas = new List<CambioSucursalCajaTabla>
            {
                new CambioSucursalCajaTabla { Tabla = "CierreCaja", Cantidad = 1 },
                new CambioSucursalCajaTabla { Tabla = "Ventas", Cantidad = plan.VentasIds.Count },
                new CambioSucursalCajaTabla { Tabla = "Compras", Cantidad = plan.ComprasIds.Count },
                new CambioSucursalCajaTabla { Tabla = "CortePorCompra", Cantidad = plan.CortePorCompraCantidad },
                new CambioSucursalCajaTabla { Tabla = "MediaRes", Cantidad = plan.MediaResCantidad },
                new CambioSucursalCajaTabla { Tabla = "Pagos", Cantidad = plan.PagosIds.Count },
                new CambioSucursalCajaTabla { Tabla = "EgresosCaja", Cantidad = plan.EgresosIds.Count },
                new CambioSucursalCajaTabla { Tabla = "MovCtaCte", Cantidad = plan.MovimientosIds.Count },
                new CambioSucursalCajaTabla { Tabla = "Expendios", Cantidad = plan.ExpendiosIds.Count },
                new CambioSucursalCajaTabla { Tabla = "TemporalLineaVenta", Cantidad = plan.TemporalesIds.Count }
            };

            preview.PuedeEjecutar = true;
            preview.Mensaje = "Vista previa generada correctamente.";
            return plan;
        }

        private void AgregarParametrosBaseCambioSucursal(SqlCommand cmd, CambioSucursalCajaPreview preview)
        {
            cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
            cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = preview.IdSucursalActual;
            cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = preview.IdUsuarioCaja;
            cmd.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = preview.FechaDesde;
            cmd.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = preview.FechaHasta;
        }

        private List<int> ObtenerIdsEgresosCaja(SqlConnection cn, SqlTransaction tx, CambioSucursalCajaPreview preview, List<int> comprasIds, List<int> ventasIds, List<int> pagosIds)
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;

                string sql = "SELECT DISTINCT id FROM EgresosCaja WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND fechaHora BETWEEN @fechaDesde AND @fechaHasta AND (creadoPor = @idUsuario";
                AgregarParametrosBaseCambioSucursal(cmd, preview);

                if (comprasIds.Count > 0)
                    sql += " OR idCompra IN (" + AgregarParametrosLista(cmd, "compra", comprasIds) + ")";
                if (ventasIds.Count > 0)
                    sql += " OR (tabla = 'Ventas' AND idTabla IN (" + AgregarParametrosLista(cmd, "venta", ventasIds) + "))";
                if (comprasIds.Count > 0)
                    sql += " OR (tabla = 'Compras' AND idTabla IN (" + AgregarParametrosLista(cmd, "compraTabla", comprasIds) + "))";
                if (pagosIds.Count > 0)
                    sql += " OR (tabla = 'Pagos' AND idTabla IN (" + AgregarParametrosLista(cmd, "pago", pagosIds) + "))";

                sql += ")";
                cmd.CommandText = sql;

                return LeerIds(cmd);
            }
        }

        private List<int> ObtenerIdsMovimientosCaja(SqlConnection cn, SqlTransaction tx, CambioSucursalCajaPreview preview, List<int> comprasIds, List<int> ventasIds, List<int> pagosIds)
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;

                string sql = "SELECT DISTINCT id FROM MovCtaCte WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND fecha BETWEEN @fechaDesde AND @fechaHasta AND (creadoPor = @idUsuario";
                AgregarParametrosBaseCambioSucursal(cmd, preview);

                if (ventasIds.Count > 0)
                    sql += " OR (tabla = 'Ventas' AND idTabla IN (" + AgregarParametrosLista(cmd, "venta", ventasIds) + "))";
                if (comprasIds.Count > 0)
                    sql += " OR (tabla = 'Compras' AND idTabla IN (" + AgregarParametrosLista(cmd, "compra", comprasIds) + "))";
                if (pagosIds.Count > 0)
                    sql += " OR (tabla = 'Pagos' AND idTabla IN (" + AgregarParametrosLista(cmd, "pago", pagosIds) + "))";

                sql += ")";
                cmd.CommandText = sql;

                return LeerIds(cmd);
            }
        }

        private List<int> ObtenerIds(SqlConnection cn, SqlTransaction tx, string sql, Action<SqlCommand> setParams)
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                setParams?.Invoke(cmd);
                return LeerIds(cmd);
            }
        }

        private List<int> LeerIds(SqlCommand cmd)
        {
            var ids = new List<int>();
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    if (dr[0] != DBNull.Value)
                        ids.Add(Convert.ToInt32(dr[0]));
                }
            }

            return ids;
        }

        private string AgregarParametrosLista(SqlCommand cmd, string prefijo, List<int> ids)
        {
            var nombres = new List<string>();
            for (int i = 0; i < ids.Count; i++)
            {
                string nombre = "@" + prefijo + i;
                nombres.Add(nombre);
                cmd.Parameters.Add(nombre, SqlDbType.Int).Value = ids[i];
            }

            return string.Join(",", nombres);
        }

        private int ObtenerCantidadPorCompras(SqlConnection cn, SqlTransaction tx, string tabla, List<int> comprasIds, int idSucursalActual)
        {
            if (comprasIds == null || comprasIds.Count == 0)
                return 0;

            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "SELECT COUNT(1) FROM " + tabla + " WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursal AND idCompra IN (" + AgregarParametrosLista(cmd, "compra", comprasIds) + ")";
                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = idSucursalActual;
                object obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
            }
        }

        private int CalcularNuevoIdCierreCaja(SqlConnection cn, SqlTransaction tx, int idCierreActual, int idSucursalNueva)
        {
            int baseSucursalNueva = idSucursalNueva * 100000000;
            int sufijoActual = idCierreActual % 100000000;
            if (sufijoActual <= 0)
                sufijoActual = 1;

            int candidato = baseSucursalNueva + sufijoActual;
            int existe = ObtenerEntero(
                cn,
                tx,
                "SELECT COUNT(1) FROM CierreCaja WHERE idEmpresa = @idEmpresa AND id = @id",
                cmd =>
                {
                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = candidato;
                });

            if (existe == 0)
                return candidato;

            int maxIdDestino = ObtenerEntero(
                cn,
                tx,
                "SELECT ISNULL(MAX(id), 0) FROM CierreCaja WHERE idEmpresa = @idEmpresa AND id >= @desde AND id < @hasta",
                cmd =>
                {
                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    cmd.Parameters.Add("@desde", SqlDbType.Int).Value = baseSucursalNueva;
                    cmd.Parameters.Add("@hasta", SqlDbType.Int).Value = baseSucursalNueva + 100000000;
                });

            if (maxIdDestino < baseSucursalNueva)
                return baseSucursalNueva + 1;

            return maxIdDestino + 1;
        }

        private int ActualizarIds(SqlConnection cn, SqlTransaction tx, string tabla, string campoId, List<int> ids, int idSucursalNueva)
        {
            if (ids == null || ids.Count == 0)
                return 0;

            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE " + tabla + " SET idSucursal = @idSucursalNueva WHERE idEmpresa = @idEmpresa AND " + campoId + " IN (" + AgregarParametrosLista(cmd, "id", ids) + ")";
                cmd.Parameters.Add("@idSucursalNueva", SqlDbType.Int).Value = idSucursalNueva;
                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                return cmd.ExecuteNonQuery();
            }
        }

        private int ActualizarPorCompras(SqlConnection cn, SqlTransaction tx, string tabla, List<int> comprasIds, int idSucursalNueva, int idSucursalActual)
        {
            if (comprasIds == null || comprasIds.Count == 0)
                return 0;

            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "UPDATE " + tabla + " SET idSucursal = @idSucursalNueva WHERE idEmpresa = @idEmpresa AND idSucursal = @idSucursalActual AND idCompra IN (" + AgregarParametrosLista(cmd, "idCompra", comprasIds) + ")";
                cmd.Parameters.Add("@idSucursalNueva", SqlDbType.Int).Value = idSucursalNueva;
                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                cmd.Parameters.Add("@idSucursalActual", SqlDbType.Int).Value = idSucursalActual;
                return cmd.ExecuteNonQuery();
            }
        }

        private int EjecutarNonQuery(SqlConnection cn, SqlTransaction tx, string sql, Action<SqlCommand> setParams)
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                setParams?.Invoke(cmd);
                return cmd.ExecuteNonQuery();
            }
        }

        private string ObtenerTexto(SqlConnection cn, SqlTransaction tx, string sql, Action<SqlCommand> setParams)
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                setParams?.Invoke(cmd);
                object obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? "" : Convert.ToString(obj);
            }
        }

        private int ObtenerEntero(SqlConnection cn, SqlTransaction tx, string sql, Action<SqlCommand> setParams)
        {
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.Transaction = tx;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;
                setParams?.Invoke(cmd);
                object obj = cmd.ExecuteScalar();
                return obj == null || obj == DBNull.Value ? 0 : Convert.ToInt32(obj);
            }
        }

        private void AsegurarTablaAuditoriaCambioSucursalCaja(SqlConnection cn, SqlTransaction tx)
        {
            const string sql = @"
IF OBJECT_ID('dbo.AuditoriaCambioSucursalCaja', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AuditoriaCambioSucursalCaja
    (
        id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        idEmpresa INT NOT NULL,
        idCierreCaja INT NOT NULL,
        idUsuarioEjecutor INT NOT NULL,
        usuarioEjecutor NVARCHAR(150) NULL,
        idUsuarioCaja INT NOT NULL,
        usuarioCaja NVARCHAR(150) NULL,
        idSucursalAnterior INT NOT NULL,
        sucursalAnterior NVARCHAR(150) NULL,
        idSucursalNueva INT NOT NULL,
        sucursalNueva NVARCHAR(150) NULL,
        fechaDesde DATETIME NOT NULL,
        fechaHasta DATETIME NOT NULL,
        fechaCambio DATETIME NOT NULL,
        detalle NVARCHAR(MAX) NULL
    );
END";

            EjecutarNonQuery(cn, tx, sql, null);
        }

        private void InsertarAuditoriaCambioSucursalCaja(SqlConnection cn, SqlTransaction tx, CambioSucursalCajaPlan plan, List<CambioSucursalCajaTabla> counts, int idUsuarioEjecutor, string usuarioEjecutor)
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

            EjecutarNonQuery(
                cn,
                tx,
                "INSERT INTO AuditoriaCambioSucursalCaja (idEmpresa, idCierreCaja, idUsuarioEjecutor, usuarioEjecutor, idUsuarioCaja, usuarioCaja, idSucursalAnterior, sucursalAnterior, idSucursalNueva, sucursalNueva, fechaDesde, fechaHasta, fechaCambio, detalle) VALUES (@idEmpresa, @idCierreCaja, @idUsuarioEjecutor, @usuarioEjecutor, @idUsuarioCaja, @usuarioCaja, @idSucursalAnterior, @sucursalAnterior, @idSucursalNueva, @sucursalNueva, @fechaDesde, @fechaHasta, @fechaCambio, @detalle)",
                cmd =>
                {
                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    cmd.Parameters.Add("@idCierreCaja", SqlDbType.Int).Value = plan.Preview.IdCierreCaja;
                    cmd.Parameters.Add("@idUsuarioEjecutor", SqlDbType.Int).Value = idUsuarioEjecutor;
                    cmd.Parameters.Add("@usuarioEjecutor", SqlDbType.NVarChar, 150).Value = (object)(usuarioEjecutor ?? "");
                    cmd.Parameters.Add("@idUsuarioCaja", SqlDbType.Int).Value = plan.Preview.IdUsuarioCaja;
                    cmd.Parameters.Add("@usuarioCaja", SqlDbType.NVarChar, 150).Value = (object)(plan.Preview.UsuarioCaja ?? "");
                    cmd.Parameters.Add("@idSucursalAnterior", SqlDbType.Int).Value = plan.Preview.IdSucursalActual;
                    cmd.Parameters.Add("@sucursalAnterior", SqlDbType.NVarChar, 150).Value = (object)(plan.Preview.SucursalActual ?? "");
                    cmd.Parameters.Add("@idSucursalNueva", SqlDbType.Int).Value = plan.Preview.IdSucursalNueva;
                    cmd.Parameters.Add("@sucursalNueva", SqlDbType.NVarChar, 150).Value = (object)(plan.Preview.SucursalNueva ?? "");
                    cmd.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = plan.Preview.FechaDesde;
                    cmd.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = plan.Preview.FechaHasta;
                    cmd.Parameters.Add("@fechaCambio", SqlDbType.DateTime).Value = DateTime.Now;
                    cmd.Parameters.Add("@detalle", SqlDbType.NVarChar).Value = detalle;
                });
        }

        #endregion
    }
}
