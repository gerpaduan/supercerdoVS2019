using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class CierreCaja
    {
        private readonly IEmpresaContext _empresa;

        public CierreCaja(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
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
                            p.Add("@sucursal", SqlDbType.Int).Value = oCierreParam.Sucursal.idSucursal;
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
            oEgresoCaja.TipoEgresoCaja = dr["tipoEgresoCaja"] != DBNull.Value ? dr["tipoEgresoCaja"].ToString() : "";
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

        #endregion
    }
}
