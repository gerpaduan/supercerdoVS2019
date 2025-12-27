using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Usuario
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daUsuario;
        SqlCommand cmUsuario;

        public DataTable obtenerUsuarios(bool soloActivos, bool soloAdmin = false)
        {
            DataTable dtUsuario = new DataTable();
            string sqlConsulta = "Select * from Usuarios" + (soloActivos ? " where activo = 1" : "");
            if (soloAdmin) 
                sqlConsulta = "Select * from Usuarios where usuario = 'admin'";
            daUsuario = new SqlDataAdapter(sqlConsulta, conn.conectar());
            daUsuario.Fill(dtUsuario);

            return dtUsuario;
        }

        public DataTable getUsuarioActivos()
        {
            DataTable dtUsuarios = new DataTable();
            daUsuario = new SqlDataAdapter();
            cmUsuario = new SqlCommand();
            cmUsuario.Connection = conn.conectar();
            cmUsuario.Connection.Open();
            cmUsuario.CommandType = CommandType.Text;
            cmUsuario.CommandText = "SELECT nombre,usuario,clave from Usuarios where activo = 1";

            cmUsuario.ExecuteNonQuery();
            daUsuario.SelectCommand = cmUsuario;
            daUsuario.Fill(dtUsuarios);
            cmUsuario.Connection.Close();

            return dtUsuarios;
        }

        public Entidades.Usuario getUsuarioById(int idUsuario, SqlConnection conn = null)//, SqlTransaction tran = null)
        {
            Entidades.Usuario oUsuarioE = null;
            bool conexionPropia = false;

            try
            {
                // Si no hay conexión pasada, creamos y abrimos una propia
                if (conn == null)
                {
                    conn = this.conn.conectar();
                    conn.Open();
                    conexionPropia = true;
                }

                using (SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios WHERE id = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);

                    using (SqlDataReader drUsuario = cmd.ExecuteReader())
                    {
                        if (drUsuario.Read())
                        {
                            oUsuarioE = new Entidades.Usuario
                            {
                                Id = Convert.ToInt32(drUsuario["id"]),
                                Nombre = drUsuario["nombre"]?.ToString(),
                                User = drUsuario["usuario"]?.ToString(),
                                Clave = drUsuario["clave"]?.ToString(),
                                Email = drUsuario["email"] != DBNull.Value ? drUsuario["email"]?.ToString() : "",
                                Admin = drUsuario["admin"] != DBNull.Value && Convert.ToBoolean(drUsuario["admin"]),
                                Activo = drUsuario["activo"] != DBNull.Value && Convert.ToBoolean(drUsuario["activo"]),
                                ColorForm = drUsuario["colorForm"]?.ToString(),
                                IdSucursal = drUsuario["idSucursalUser"] == DBNull.Value
                                                        ? 0
                                                        : Convert.ToInt32(drUsuario["idSucursalUser"]),
                                IdEmpresa = drUsuario["idEmpresa"] == DBNull.Value
                                                        ? 0
                                                        : Convert.ToInt32(drUsuario["idEmpresa"])
                        };
                        }
                    }
                }
            }
            finally
            {
                // Cerramos solo si fue conexión propia
                if (conexionPropia && conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return oUsuarioE;
        }

        public Entidades.Usuario getUsuarioById(int idUsuario)
        {
            Entidades.Usuario oUsuarioE = null;

            using (SqlConnection conn = this.conn.conectar()) 
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM Usuarios WHERE id = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", idUsuario);

                conn.Open();
                using (SqlDataReader drUsuario = cmd.ExecuteReader())
                {
                    if (drUsuario.Read())
                    {
                        oUsuarioE = new Entidades.Usuario();
                        oUsuarioE.Id = Convert.ToInt32(drUsuario["id"]);
                        oUsuarioE.Nombre = Convert.ToString(drUsuario["nombre"]);
                        oUsuarioE.User = Convert.ToString(drUsuario["usuario"]);
                        oUsuarioE.Clave = Convert.ToString(drUsuario["clave"]);
                        oUsuarioE.Email = Convert.ToString(drUsuario["email"]);
                        oUsuarioE.Admin = Convert.ToBoolean(drUsuario["admin"]);
                        oUsuarioE.Activo = Convert.ToBoolean(drUsuario["activo"]);
                        oUsuarioE.ColorForm = Convert.ToString(drUsuario["colorForm"]);
                        oUsuarioE.IdSucursal = drUsuario["idSucursalUser"] == DBNull.Value
                                                        ? 0
                                                        : Convert.ToInt32(drUsuario["idSucursalUser"]);
                        oUsuarioE.IdEmpresa = drUsuario["idEmpresa"] == DBNull.Value
                                                        ? 0
                                                        : Convert.ToInt32(drUsuario["idEmpresa"]);
                    }
                }
            }

            return oUsuarioE;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            cmUsuario = new SqlCommand();

            cmUsuario.Connection = conn.conectar();
            cmUsuario.Connection.Open();
            cmUsuario.CommandType = CommandType.StoredProcedure;
            cmUsuario.CommandText = "addOrEditUser";
            cmUsuario.Parameters.AddWithValue("@id", oUsuarioE.Id);
            cmUsuario.Parameters.AddWithValue("@nombre", oUsuarioE.Nombre);
            cmUsuario.Parameters.AddWithValue("@usuario", oUsuarioE.User);
            cmUsuario.Parameters.AddWithValue("@email", oUsuarioE.Email);
            cmUsuario.Parameters.AddWithValue("@clave", oUsuarioE.Clave);
            cmUsuario.Parameters.AddWithValue("@admin", oUsuarioE.Admin);
            cmUsuario.Parameters.AddWithValue("@activo", oUsuarioE.Activo);
            cmUsuario.Parameters.AddWithValue("@colorForm", oUsuarioE.ColorForm);
            cmUsuario.Parameters.AddWithValue("@idEmpresa", oUsuarioE.IdEmpresa);

            cmUsuario.ExecuteNonQuery();
            cmUsuario.Connection.Close();
        }

        public void setSucursalUsuario(Entidades.Usuario oUsuario)
        {

            using (SqlConnection con = conn.conectar())
            {
                con.Open();
                string query = @"UPDATE Usuarios
                                SET idSucursalUser = @idSucursal
                                WHERE id = @idUsuario";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@idUsuario", oUsuario.Id);
                    cmd.Parameters.AddWithValue("@idSucursal", oUsuario.IdSucursal);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario)
        {
            string query = @"
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
                FROM 
                    Formularios f
                LEFT JOIN 
                    PermisosUsuarios p 
                    ON f.idForm = p.idForm AND p.idUsuario = @idUsuario
                ORDER BY
                    f.idForm;
            ";
            List<Entidades.PermisosUsuarios> listPermisos = new List<Entidades.PermisosUsuarios>();

            using (SqlConnection con = conn.conectar())
            using (SqlCommand cmUsuario = new SqlCommand(query, con))
            {
                cmUsuario.CommandType = CommandType.Text;
                cmUsuario.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;

                con.Open();
                using (SqlDataReader drUsuario = cmUsuario.ExecuteReader())
                {
                    while (drUsuario.Read())
                    {
                        Entidades.PermisosUsuarios permisosUsuarios = new Entidades.PermisosUsuarios
                        {
                            IdUsuario = idUsuario,
                            IdForm = drUsuario.GetInt32(drUsuario.GetOrdinal("idForm")),
                            DiasPermitidosVer = drUsuario.IsDBNull(drUsuario.GetOrdinal("diasPermitidosVer")) ? -1 :
                                                                        drUsuario.GetInt32(drUsuario.GetOrdinal("diasPermitidosVer")),
                            DiasPermitidosEditar = drUsuario.IsDBNull(drUsuario.GetOrdinal("diasPermitidosEditar")) ? -1 :
                                                                           drUsuario.GetInt32(drUsuario.GetOrdinal("diasPermitidosEditar")),
                            SoloRegistrosPropios = drUsuario.IsDBNull(drUsuario.GetOrdinal("soloRegistrosPropios")) ? true :
                                                                           drUsuario.GetBoolean(drUsuario.GetOrdinal("soloRegistrosPropios")),
                            Formulario = new Entidades.Formulario
                            {
                                IdForm = drUsuario.GetInt32(drUsuario.GetOrdinal("idForm")),
                                NombreForm = drUsuario.GetString(drUsuario.GetOrdinal("nombreForm")),
                                Descripcion = drUsuario.IsDBNull(drUsuario.GetOrdinal("descripcion")) ? "" :
                                                                      drUsuario.GetString(drUsuario.GetOrdinal("descripcion")),
                                FormConsulta = drUsuario.IsDBNull(drUsuario.GetOrdinal("formConsulta")) ? "" :
                                                                       drUsuario.GetString(drUsuario.GetOrdinal("formConsulta")),
                                FormEdicion = drUsuario.IsDBNull(drUsuario.GetOrdinal("formEdicion")) ? "" :
                                                                      drUsuario.GetString(drUsuario.GetOrdinal("formEdicion")),
                                FormEdicionExtra1 = drUsuario.IsDBNull(drUsuario.GetOrdinal("formEdicionExtra1")) ? "" :
                                                                            drUsuario.GetString(drUsuario.GetOrdinal("formEdicionExtra1")),
                                FormEdicionExtra2 = drUsuario.IsDBNull(drUsuario.GetOrdinal("formEdicionExtra2")) ? "" :
                                                                            drUsuario.GetString(drUsuario.GetOrdinal("formEdicionExtra2"))
                            }
                        };
                        //var permiso = permisosUsuarios;

                        listPermisos.Add(permisosUsuarios);
                    }
                }
            }

            return listPermisos;

        }

        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos)
        {

            using (SqlConnection con = conn.conectar())
            {
                con.Open();

                foreach (var permiso in permisos)
                {
                    string query = @"
                IF EXISTS (
                    SELECT 1 FROM PermisosUsuarios 
                    WHERE idUsuario = @idUsuario AND idForm = @idForm
                )
                BEGIN
                    UPDATE PermisosUsuarios
                    SET diasPermitidosVer = @diasVer,
                        diasPermitidosEditar = @diasEditar,
                        soloRegistrosPropios = @soloPropios
                    WHERE idUsuario = @idUsuario AND idForm = @idForm
                END
                ELSE
                BEGIN
                    INSERT INTO PermisosUsuarios 
                    (idUsuario, idForm, diasPermitidosVer, diasPermitidosEditar, soloRegistrosPropios)
                    VALUES (@idUsuario, @idForm, @diasVer, @diasEditar, @soloPropios)
                END";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
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
