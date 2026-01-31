using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Usuario
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public Usuario(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        public DataTable obtenerUsuarios(bool soloActivos, bool soloAdmin = false)
        {
            var dt = new DataTable();

            string sql;
            if (soloAdmin)
            {
                sql = "SELECT * FROM Usuarios WHERE usuario = @usuario";
            }
            else
            {
                sql = "SELECT * FROM Usuarios" + (soloActivos ? " WHERE activo = 1" : "");
            }

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                if (soloAdmin)
                    cmd.Parameters.AddWithValue("@usuario", "admin");

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable getUsuarioActivos()
        {
            var dt = new DataTable();

            const string sql = "SELECT nombre, usuario, clave FROM Usuarios WHERE activo = 1";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                da.Fill(dt);
            }

            return dt;
        }

        /// <summary>
        /// Obtiene usuario por ID. Si pasás una SqlConnection abierta, no la cierra (ideal para reutilizar).
        /// Si no pasás conexión, abre/cierra una propia.
        /// </summary>
        public Entidades.Usuario getUsuarioById(int idUsuario, SqlConnection externalConn = null)
        {
            Entidades.Usuario user = null;
            bool ownConn = false;

            SqlConnection con = externalConn;
            try
            {
                if (con == null)
                {
                    con = conn.conectar(_empresa);
                    if (con.State != ConnectionState.Open) con.Open();
                    ownConn = true;
                }

                using (var cmd = new SqlCommand("SELECT * FROM Usuarios WHERE id = @id", con))
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = conn.TimeOut();
                    cmd.Parameters.AddWithValue("@id", idUsuario);

                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new Entidades.Usuario
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
                                IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"])
                            };
                        }
                    }
                }
            }
            finally
            {
                if (ownConn && con != null && con.State == ConnectionState.Open)
                    con.Close();
            }

            return user;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            if (oUsuarioE == null) throw new ArgumentNullException(nameof(oUsuarioE));

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("addOrEditUser", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@id", oUsuarioE.Id);
                cmd.Parameters.AddWithValue("@nombre", oUsuarioE.Nombre ?? "");
                cmd.Parameters.AddWithValue("@usuario", oUsuarioE.User ?? "");
                cmd.Parameters.AddWithValue("@email", oUsuarioE.Email ?? "");
                cmd.Parameters.AddWithValue("@clave", oUsuarioE.Clave ?? "");
                cmd.Parameters.AddWithValue("@admin", oUsuarioE.Admin);
                cmd.Parameters.AddWithValue("@activo", oUsuarioE.Activo);
                cmd.Parameters.AddWithValue("@colorForm", oUsuarioE.ColorForm ?? "");
                cmd.Parameters.AddWithValue("@idEmpresa", oUsuarioE.IdEmpresa);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void setSucursalUsuario(Entidades.Usuario oUsuario)
        {
            if (oUsuario == null) throw new ArgumentNullException(nameof(oUsuario));

            const string sql = @"
                UPDATE Usuarios
                SET idSucursalUser = @idSucursal
                WHERE id = @idUsuario;";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idUsuario", oUsuario.Id);
                cmd.Parameters.AddWithValue("@idSucursal", oUsuario.IdSucursal);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
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

            var list = new List<Entidades.PermisosUsuarios>();

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;

                if (con.State != ConnectionState.Open) con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var permisosUsuarios = new Entidades.PermisosUsuarios
                        {
                            IdUsuario = idUsuario,
                            IdForm = dr.GetInt32(dr.GetOrdinal("idForm")),
                            DiasPermitidosVer = dr.IsDBNull(dr.GetOrdinal("diasPermitidosVer")) ? -1 : dr.GetInt32(dr.GetOrdinal("diasPermitidosVer")),
                            DiasPermitidosEditar = dr.IsDBNull(dr.GetOrdinal("diasPermitidosEditar")) ? -1 : dr.GetInt32(dr.GetOrdinal("diasPermitidosEditar")),
                            SoloRegistrosPropios = dr.IsDBNull(dr.GetOrdinal("soloRegistrosPropios")) ? true : dr.GetBoolean(dr.GetOrdinal("soloRegistrosPropios")),
                            Formulario = new Entidades.Formulario
                            {
                                IdForm = dr.GetInt32(dr.GetOrdinal("idForm")),
                                NombreForm = dr.GetString(dr.GetOrdinal("nombreForm")),
                                Descripcion = dr.IsDBNull(dr.GetOrdinal("descripcion")) ? "" : dr.GetString(dr.GetOrdinal("descripcion")),
                                FormConsulta = dr.IsDBNull(dr.GetOrdinal("formConsulta")) ? "" : dr.GetString(dr.GetOrdinal("formConsulta")),
                                FormEdicion = dr.IsDBNull(dr.GetOrdinal("formEdicion")) ? "" : dr.GetString(dr.GetOrdinal("formEdicion")),
                                FormEdicionExtra1 = dr.IsDBNull(dr.GetOrdinal("formEdicionExtra1")) ? "" : dr.GetString(dr.GetOrdinal("formEdicionExtra1")),
                                FormEdicionExtra2 = dr.IsDBNull(dr.GetOrdinal("formEdicionExtra2")) ? "" : dr.GetString(dr.GetOrdinal("formEdicionExtra2"))
                            }
                        };

                        list.Add(permisosUsuarios);
                    }
                }
            }

            return list;
        }

        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos)
        {
            if (permisos == null) throw new ArgumentNullException(nameof(permisos));

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

            using (var con = conn.conectar(_empresa))
            {
                if (con.State != ConnectionState.Open) con.Open();

                foreach (var permiso in permisos)
                {
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = conn.TimeOut();

                        cmd.Parameters.AddWithValue("@idUsuario", permiso.IdUsuario);
                        cmd.Parameters.AddWithValue("@idForm", permiso.IdForm);
                        cmd.Parameters.AddWithValue("@diasVer", permiso.DiasPermitidosVer);
                        cmd.Parameters.AddWithValue("@diasEditar", permiso.DiasPermitidosEditar);
                        cmd.Parameters.AddWithValue("@soloPropios", permiso.SoloRegistrosPropios);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
