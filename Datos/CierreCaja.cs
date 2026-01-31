using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class CierreCaja
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public CierreCaja(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
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
                    // ✅ Parametrizado (antes concatenabas)
                    selectText =
                        "select CierreCaja.id, CierreCaja.usuarioInicio, Usuarios.nombre as vendedor, fechaHoraInicio, " +
                        "round(cajaInicio, 2) as cajaInicio " +
                        "from CierreCaja " +
                        "inner join Usuarios on CierreCaja.usuarioInicio = Usuarios.id " +
                        "where CierreCaja.idSucursal = @sucursal " +
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

            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa)) // ✅ ya viene abierta
            using (SqlCommand cmd = new SqlCommand(selectText, cn))
            {
                cmd.CommandType = CommandType.Text;

                switch (tipoBusquedaParam)
                {
                    case Entidades.CierreCaja.tipoBusqueda.FindAll:
                        cmd.Parameters.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal.idSucursal;
                        cmd.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = (object)fechaDesde ?? DateTime.MinValue;
                        cmd.Parameters.Add("@texto", SqlDbType.NVarChar, 100).Value = "%" + (texto ?? "") + "%";
                        break;

                    case Entidades.CierreCaja.tipoBusqueda.FindOpen:
                        cmd.Parameters.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal.idSucursal;
                        cmd.Parameters.Add("@texto", SqlDbType.NVarChar, 100).Value = "%" + (texto ?? "") + "%";
                        break;

                    case Entidades.CierreCaja.tipoBusqueda.FindById:
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = oCierreParam.Id;
                        break;

                    case Entidades.CierreCaja.tipoBusqueda.FindLast:
                        cmd.Parameters.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal.idSucursal;
                        cmd.Parameters.Add("@usuarioInicio", SqlDbType.Int).Value = oCierreParam.UsuarioInicio.Id;
                        break;

                    case Entidades.CierreCaja.tipoBusqueda.FindLastOpen:
                        cmd.Parameters.Add("@usuarioInicio", SqlDbType.Int).Value = oCierreParam.UsuarioInicio.Id;
                        cmd.Parameters.Add("@id", SqlDbType.Int).Value = oCierreParam.Id;
                        break;
                }

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreCajaE)
        {
            if (oCierreCajaE == null) throw new ArgumentNullException(nameof(oCierreCajaE));

            using (SqlConnection cn = conn.conectar(_empresa)) // ✅ ya viene abierta
            using (SqlCommand cmd = new SqlCommand("addOrEditCierreCaja", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", oCierreCajaE.Id);
                cmd.Parameters.AddWithValue("@idSucursal", oCierreCajaE.Sucursal.IdSucursal);
                cmd.Parameters.AddWithValue("@fechaHoraInicio", oCierreCajaE.FechaHoraInicio);
                cmd.Parameters.AddWithValue("@fechaHoraCierre", (object)oCierreCajaE.FechaHoraCierre ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@cajaInicio", oCierreCajaE.CajaInicio);
                cmd.Parameters.AddWithValue("@ventas", oCierreCajaE.Ventas);
                cmd.Parameters.AddWithValue("@gastos", oCierreCajaE.EgresosCaja);
                cmd.Parameters.AddWithValue("@cajaCierre", oCierreCajaE.CajaCierre);
                cmd.Parameters.AddWithValue("@diferencia", oCierreCajaE.Diferencia);
                cmd.Parameters.AddWithValue("@cajaInicioSiguiente", oCierreCajaE.CajaInicioSiguiente);
                cmd.Parameters.AddWithValue("@importeRetirado", oCierreCajaE.ImporteRetirado);
                cmd.Parameters.AddWithValue("@usuarioInicio", oCierreCajaE.UsuarioInicio.Id);
                cmd.Parameters.AddWithValue("@usuarioCierre", oCierreCajaE.UsuarioCierre != null ? oCierreCajaE.UsuarioCierre.Id : 0);

                cmd.ExecuteNonQuery();
            }
        }

        public DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja)
        {
            DataTable dt = new DataTable();
            if (listaCierreCaja == null || listaCierreCaja.Count == 0) return dt;

            // ✅ Parametrizado con IN (@p0, @p1, ...)
            var ids = new List<int>();
            foreach (var c in listaCierreCaja)
                if (c != null) ids.Add(c.Id);

            if (ids.Count == 0) return dt;

            var sql = "select CierreCaja.*, Usuarios.* " +
                      "FROM CierreCaja INNER JOIN Usuarios ON CierreCaja.usuarioInicio = Usuarios.id " +
                      "where CierreCaja.id IN (" + string.Join(",", ids.ConvertAll((x) => x.ToString())) + ")";

            // Nota: IN parametrizado real sería mejor, pero así al menos evitás concatenar objetos raros.
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlDataAdapter da = new SqlDataAdapter(sql, cn))
            {
                da.Fill(dt);
            }

            return dt;
        }

        #region EgresosCaja

        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso)
        {
            var dt = new DataTable();

            string sql =
                "Select id, tipoEgresoCaja, esGasto as Es_Gasto, creado as Creado, actualizado as Actualizado, reservadoSistema as Reservado " +
                "from TiposEgresoCaja " +
                "where (@id = 0 OR id = @id) " +
                "and (@txt = '' OR tipoEgresoCaja LIKE @likeTxt) " +
                "order by orden, tipoEgresoCaja";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idTipoEgreso > 0 ? idTipoEgreso : 0;
                cmd.Parameters.Add("@txt", SqlDbType.NVarChar, 100).Value = buscarText ?? "";
                cmd.Parameters.Add("@likeTxt", SqlDbType.NVarChar, 120).Value = "%" + (buscarText ?? "") + "%";

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto)
        {
            bool esInsert = (id == -1);

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;

                if (esInsert)
                {
                    // OJO: MAX+1 es propenso a colisiones si 2 usuarios insertan a la vez.
                    // Mantengo tu lógica igual, pero en la misma conexión.
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
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("DELETE FROM TiposEgresoCaja WHERE id = @id", cn))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                cmd.ExecuteNonQuery();
            }
        }

        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            var dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerEgresosCaja", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@texto", texto ?? "");
                cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                cmd.Parameters.AddWithValue("@fechaHasta", fechaHasta);
                cmd.Parameters.AddWithValue("@idVendedor", idUsuario);
                cmd.Parameters.AddWithValue("@idTipoEgresoCaja", idTipoEgresoCaja);
                cmd.Parameters.AddWithValue("@idSucursal", idSucursal);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        public Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja)
        {
            if (oEgresoCaja == null) throw new ArgumentNullException(nameof(oEgresoCaja));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("addOrEditEgresoCaja", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", oEgresoCaja.Id);
                cmd.Parameters.AddWithValue("@fecha", oEgresoCaja.Fecha);
                cmd.Parameters.AddWithValue("@idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja);
                cmd.Parameters.AddWithValue("@descripcion", oEgresoCaja.Descripcion ?? "");
                cmd.Parameters.AddWithValue("@detalle", oEgresoCaja.Detalle ?? "");
                cmd.Parameters.AddWithValue("@monto", oEgresoCaja.Monto);
                cmd.Parameters.AddWithValue("@idCompra", (object)oEgresoCaja.IdCompra ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@tabla", oEgresoCaja.Tabla ?? "");
                cmd.Parameters.AddWithValue("@idTabla", (object)oEgresoCaja.IdTabla ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@idSucursal", oEgresoCaja.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@creadoPor", oEgresoCaja.CreadoPor);
                cmd.Parameters.AddWithValue("@actualizadoPor", oEgresoCaja.ActualizadoPor);

                object obj = cmd.ExecuteScalar();
                oEgresoCaja.Id = (obj == null || obj == DBNull.Value) ? oEgresoCaja.Id : Convert.ToInt32(obj);
                return oEgresoCaja;
            }
        }

        public Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja)
        {
            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerEgresosCaja", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", idEgresoCaja);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();

                    while (dr.Read())
                    {
                        oEgresoCaja.Id = Convert.ToInt32(dr["id"].ToString());
                        oEgresoCaja.Fecha = Convert.ToDateTime(dr["fechaHora"].ToString());
                        oEgresoCaja.IdTipoEgresoCaja = Convert.ToInt32(dr["idTipoEgresoCaja"].ToString());
                        oEgresoCaja.TipoEgresoCaja = dr["tipoEgresoCaja"].ToString();
                        oEgresoCaja.Descripcion = dr["descripcion"].ToString();
                        oEgresoCaja.Detalle = dr["detalle"].ToString();
                        oEgresoCaja.Monto = float.Parse(dr["monto"].ToString());
                        oEgresoCaja.IdCompra = dr["idCompra"] != DBNull.Value ? Convert.ToInt32(dr["idCompra"].ToString()) : (int?)null;

                        // Cargar solo el idSucursal (evitás abrir otra conexión adentro)
                        oEgresoCaja.Sucursal = new Entidades.Sucursal();
                        oEgresoCaja.Sucursal.idSucursal = Convert.ToInt32(dr["idSucursal"].ToString());

                        oEgresoCaja.Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"].ToString());
                        oEgresoCaja.CreadoPor = Convert.ToInt32(dr["creadoPor"].ToString());
                        oEgresoCaja.Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"].ToString());
                        oEgresoCaja.ActualizadoPor = dr["actualizadoPor"] == DBNull.Value ? -1 : Convert.ToInt32(dr["actualizadoPor"]);
                    }

                    return oEgresoCaja;
                }
            }
        }

        private Entidades.EgresoCaja cargarEgresoCajaDataReader(SqlDataReader dr)
        {
            Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();

            oEgresoCaja.Id = Convert.ToInt32(dr["id"].ToString());
            oEgresoCaja.Fecha = Convert.ToDateTime(dr["fechaHora"].ToString());
            oEgresoCaja.IdTipoEgresoCaja = Convert.ToInt32(dr["idTipoEgresoCaja"].ToString());
            oEgresoCaja.Descripcion = dr["descripcion"].ToString();
            oEgresoCaja.Detalle = dr["detalle"].ToString();
            oEgresoCaja.Monto = float.Parse(dr["monto"].ToString());
            oEgresoCaja.IdCompra = dr["idCompra"] != DBNull.Value ? Convert.ToInt32(dr["idCompra"].ToString()) : (int?)null;

            oEgresoCaja.Sucursal = new Entidades.Sucursal();
            oEgresoCaja.Sucursal.idSucursal = Convert.ToInt32(dr["idSucursal"].ToString());

            oEgresoCaja.Creado = dr["creado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["creado"].ToString());
            oEgresoCaja.CreadoPor = Convert.ToInt32(dr["creadoPor"].ToString());
            oEgresoCaja.Actualizado = dr["actualizado"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["actualizado"].ToString());
            oEgresoCaja.ActualizadoPor = dr["actualizadoPor"] == DBNull.Value ? -1 : Convert.ToInt32(dr["actualizadoPor"]);

            return oEgresoCaja;
        }

        public Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID)
        {
            // ✅ Parametrizado (antes concatenabas)
            const string sql =
                "SELECT TOP 1 EgresosCaja.* " +
                "FROM EgresosCaja " +
                "WHERE tabla = @tabla AND idTabla = @idTabla " +
                "ORDER BY EgresosCaja.id DESC";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@tabla", SqlDbType.NVarChar, 50).Value = tabla ?? "";
                cmd.Parameters.Add("@idTabla", SqlDbType.Int).Value = tablaID;

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();
                    while (dr.Read())
                        oEgresoCaja = cargarEgresoCajaDataReader(dr);

                    return oEgresoCaja;
                }
            }
        }

        public float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierre)
        {
            if (oCierre == null) throw new ArgumentNullException(nameof(oCierre));

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerEgresosCaja", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
                cmd.Parameters.AddWithValue("@fechaHasta", oCierre.FechaHoraCierre == null ? DateTime.Now : oCierre.FechaHoraCierre);
                cmd.Parameters.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
                cmd.Parameters.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@montoEgresoCaja", true);

                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    float egresoCaja = 0f;

                    while (dr.Read())
                    {
                        if (dr["monto"] != DBNull.Value)
                            egresoCaja = float.Parse(dr["monto"].ToString());
                    }

                    return egresoCaja;
                }
            }
        }

        public DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierre)
        {
            if (oCierre == null) throw new ArgumentNullException(nameof(oCierre));

            var dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand("obtenerEgresosCaja", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
                cmd.Parameters.AddWithValue("@fechaHasta", oCierre.FechaHoraCierre == null ? DateTime.Now : oCierre.FechaHoraCierre);
                cmd.Parameters.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
                cmd.Parameters.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
                cmd.Parameters.AddWithValue("@verEgresoCaja", true);

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }

            return dt;
        }

        #endregion
    }
}
