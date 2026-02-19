using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Usuario
    {
        private readonly IEmpresaContext _empresa;

        public Usuario(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        public DataTable obtenerUsuarios(bool soloActivos, bool filtroEmpresa = true, bool soloAdmin = false)
        {
            string sql;

            if (soloAdmin)
            {
                sql = "SELECT * FROM Usuarios WHERE usuario = @usuario";
                return Db.DataTable(
                    _empresa,
                    sql,
                    CommandType.Text,
                    setParams: p =>
                    {
                        p.Add("@usuario", SqlDbType.NVarChar, 50).Value = "admin";
                    }
                );
            }
            var where = new List<string>();

            if (filtroEmpresa)
                where.Add("idEmpresa = @idEmpresa");

            if (soloActivos)
                where.Add("activo = 1");

            sql = "SELECT * FROM Usuarios"
                + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "");

            return Db.DataTable(_empresa, sql, CommandType.Text, setParams: p =>  p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa);
        }

        public DataTable getUsuarioActivos()
        {
            const string sql = "SELECT nombre, usuario, clave FROM Usuarios WHERE activo = 1 AND idEmpresa = @idEmpresa";
            return Db.DataTable(_empresa, sql, CommandType.Text, setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa);
        }

        /// <summary>
        /// Obtiene usuario por ID.
        /// Nota: si querés conservar el "externalConn" para reutilizar una conexión abierta,
        /// conviene agregar overloads en Db que acepten (SqlConnection/SqlTransaction).
        /// Por ahora lo dejo simple con Db.Reader.
        /// </summary>
        public Entidades.Usuario getUsuarioById(int idUsuario)
        {

            var oSucursalD = new Datos.Sucursal(_empresa);

            const string sql = "SELECT * FROM Usuarios WHERE id = @id";

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr =>
                {
                    return new Entidades.Usuario
                    {
                        Id = Convert.ToInt32(dr["id"]),
                        Nombre = dr["nombre"] == DBNull.Value ? "" : Convert.ToString(dr["nombre"]),
                        User = dr["usuario"] == DBNull.Value ? "" : Convert.ToString(dr["usuario"]),
                        Clave = dr["clave"] == DBNull.Value ? "" : Convert.ToString(dr["clave"]),
                        Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                        Admin = dr["admin"] != DBNull.Value && Convert.ToBoolean(dr["admin"]),
                        Activo = dr["activo"] != DBNull.Value && Convert.ToBoolean(dr["activo"]),
                        ColorForm = dr["colorForm"] == DBNull.Value ? "" : Convert.ToString(dr["colorForm"]),
                        IdSucursal = dr["idSucursalUser"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idSucursalUser"]),
                        IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"]),
                        Sucursal = dr["idSucursalUser"] == DBNull.Value ? null : oSucursalD.findById(Convert.ToInt32(dr["idSucursalUser"])),
                        Empresa = dr["idEmpresa"] == DBNull.Value ? null : oSucursalD.findEmpresaById(Convert.ToInt32(dr["idEmpresa"]))
                    };
                },
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = idUsuario;
                }
            );

            return list.Count > 0 ? list[0] : null;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            if (oUsuarioE == null) throw new ArgumentNullException(nameof(oUsuarioE));

            Db.NonQuery(
                _empresa,
                "addOrEditUser",
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = oUsuarioE.Id;
                    p.Add("@nombre", SqlDbType.NVarChar, 120).Value = oUsuarioE.Nombre ?? "";
                    p.Add("@usuario", SqlDbType.NVarChar, 50).Value = oUsuarioE.User ?? "";
                    p.Add("@email", SqlDbType.NVarChar, 120).Value = oUsuarioE.Email ?? "";
                    p.Add("@clave", SqlDbType.NVarChar, 200).Value = oUsuarioE.Clave ?? "";
                    p.Add("@admin", SqlDbType.Bit).Value = oUsuarioE.Admin;
                    p.Add("@activo", SqlDbType.Bit).Value = oUsuarioE.Activo;
                    p.Add("@colorForm", SqlDbType.NVarChar, 50).Value = oUsuarioE.ColorForm ?? "";
                }
            );
        }

        public void setSucursalUsuario(Entidades.Usuario oUsuario)
        {
            if (oUsuario == null) throw new ArgumentNullException(nameof(oUsuario));

            const string sql = @"
                UPDATE Usuarios
                SET idSucursalUser = @idSucursal
                WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = oUsuario.Id;
                    p.Add("@idSucursal", SqlDbType.Int).Value = oUsuario.IdSucursal;
                }
            );
        }

        public List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario)
        {
            const string query = @"
                SELECT 
                    f.idForm,
                    f.nombreForm,
                    f.descripcion,
                    f.formConsulta,
                    f.formEdicion,
                    f.formEdicionExtra1,
                    f.formEdicionExtra2,
                    COALESCE(p.diasPermitidosVer, -1) AS diasPermitidosVer,
                    COALESCE(p.diasPermitidosEditar, -1) AS diasPermitidosEditar,
                    CAST(COALESCE(p.soloRegistrosPropios, 1) AS bit) AS soloRegistrosPropios
                FROM Formularios f
                LEFT JOIN PermisosUsuarios p 
                    ON f.idForm = p.idForm AND p.idUsuario = @idUsuario
                ORDER BY f.idForm;";

            return Db.Reader(
                _empresa,
                query,
                CommandType.Text,
                map: dr =>
                {
                    return new Entidades.PermisosUsuarios
                    {
                        IdUsuario = idUsuario,
                        IdForm = Convert.ToInt32(dr["idForm"]),
                        DiasPermitidosVer = dr["diasPermitidosVer"] == DBNull.Value ? -1 : Convert.ToInt32(dr["diasPermitidosVer"]),
                        DiasPermitidosEditar = dr["diasPermitidosEditar"] == DBNull.Value ? -1 : Convert.ToInt32(dr["diasPermitidosEditar"]),
                        SoloRegistrosPropios = dr["soloRegistrosPropios"] == DBNull.Value ? true : Convert.ToBoolean(dr["soloRegistrosPropios"]),
                        Formulario = new Entidades.Formulario
                        {
                            IdForm = Convert.ToInt32(dr["idForm"]),
                            NombreForm = Convert.ToString(dr["nombreForm"]),
                            Descripcion = dr["descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["descripcion"]),
                            FormConsulta = dr["formConsulta"] == DBNull.Value ? "" : Convert.ToString(dr["formConsulta"]),
                            FormEdicion = dr["formEdicion"] == DBNull.Value ? "" : Convert.ToString(dr["formEdicion"]),
                            FormEdicionExtra1 = dr["formEdicionExtra1"] == DBNull.Value ? "" : Convert.ToString(dr["formEdicionExtra1"]),
                            FormEdicionExtra2 = dr["formEdicionExtra2"] == DBNull.Value ? "" : Convert.ToString(dr["formEdicionExtra2"])
                        }
                    };
                },
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                }
            );
        }

        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos)
        {
            if (permisos == null) throw new ArgumentNullException(nameof(permisos));
            if (permisos.Count == 0) return;

            const string query = @"
                IF EXISTS (SELECT 1 FROM PermisosUsuarios WHERE idUsuario = @idUsuario AND idForm = @idForm)
                BEGIN
                    UPDATE PermisosUsuarios
                    SET diasPermitidosVer = @diasVer,
                        diasPermitidosEditar = @diasEditar,
                        soloRegistrosPropios = @soloPropios
                    WHERE idUsuario = @idUsuario AND idForm = @idForm
                END
                ELSE
                BEGIN
                    INSERT INTO PermisosUsuarios (idUsuario, idForm, diasPermitidosVer, diasPermitidosEditar, soloRegistrosPropios)
                    VALUES (@idUsuario, @idForm, @diasVer, @diasEditar, @soloPropios)
                END";

            // Una conexión, muchos updates (como tu código original)
            using (var con = Db.Open(_empresa))
            {
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = Conexion.timeOut;

                    var pIdUsuario = cmd.Parameters.Add("@idUsuario", SqlDbType.Int);
                    var pIdForm = cmd.Parameters.Add("@idForm", SqlDbType.Int);
                    var pDiasVer = cmd.Parameters.Add("@diasVer", SqlDbType.Int);
                    var pDiasEditar = cmd.Parameters.Add("@diasEditar", SqlDbType.Int);
                    var pSoloPropios = cmd.Parameters.Add("@soloPropios", SqlDbType.Bit);

                    foreach (var permiso in permisos)
                    {
                        pIdUsuario.Value = permiso.IdUsuario;
                        pIdForm.Value = permiso.IdForm;
                        pDiasVer.Value = permiso.DiasPermitidosVer;
                        pDiasEditar.Value = permiso.DiasPermitidosEditar;
                        pSoloPropios.Value = permiso.SoloRegistrosPropios;

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
