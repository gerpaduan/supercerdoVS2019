using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class Persona
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public Persona(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        #region Helpers (LIKE seguro + DBNull)

        private static object DbNullIfNull(object value) => value ?? DBNull.Value;

        private static string EscapeLike(string text)
        {
            // Escapa caracteres especiales de LIKE: %, _, [, y la barra invertida
            // Usamos ESCAPE '\'
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");
        }

        private static string LikePattern(string text) => "%" + EscapeLike((text ?? "").Trim()) + "%";

        private static string NormalizarCuit(string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit)) return "";
            return cuit.Trim().Replace("-", "");
        }

        #endregion

        #region ABM Persona (SP)

        public void agregarPersona(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("agregarPersona", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial ?? "");
                cmd.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos ?? "");
                cmd.Parameters.AddWithValue("@tipo", oPersonaE.tipo ?? "");

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Mantuve el nombre original por compatibilidad aunque conceptualmente sea "modificarPersona"
        public void modificarProveedor(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("modificarPersona", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idPersona", oPersonaE.idPersona);
                cmd.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos ?? "");
                cmd.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial ?? "");
                cmd.Parameters.AddWithValue("@tipo", oPersonaE.tipo ?? "");

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void addOrEditPersona(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("addOrEditPersona", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idPersona", oPersonaE.idPersona);
                cmd.Parameters.AddWithValue("@identificacion", oPersonaE.Identificacion ?? "");
                cmd.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial ?? "");
                cmd.Parameters.AddWithValue("@idIva", oPersonaE.IdIva);
                cmd.Parameters.AddWithValue("@cuit", oPersonaE.Cuit ?? "");
                cmd.Parameters.AddWithValue("@telefono", oPersonaE.Telefono ?? "");
                cmd.Parameters.AddWithValue("@domicilio", oPersonaE.Domicilio ?? "");
                cmd.Parameters.AddWithValue("@ciudad", oPersonaE.Ciudad ?? "");
                cmd.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos ?? "");
                cmd.Parameters.AddWithValue("@tipo", oPersonaE.tipo ?? "");
                cmd.Parameters.AddWithValue("@ctaCte", oPersonaE.CtaCte);
                cmd.Parameters.AddWithValue("@bonificacion", oPersonaE.Bonificacion);
                cmd.Parameters.AddWithValue("@marca", oPersonaE.Marca);

                // idPropietario nullable
                cmd.Parameters.AddWithValue("@idPropietario",
                    oPersonaE.Propietario != null ? (object)oPersonaE.Propietario.idPersona : DBNull.Value);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("eliminarPersona", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@idPersona", oPersonaE.idPersona);

                if (con.State != ConnectionState.Open) con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Find / Buscar

        public Entidades.Persona findById(int id)
        {
            Entidades.Persona oPersona = null;

            string sql = @"
                SELECT 
                    p.idPersona,
                    p.identificacion,
                    p.razonSocial,
                    p.tipo,
                    p.otrosDatos,
                    p.ctaCte,
                    p.bonificacion,
                    p.cuit,
                    p.telefono,
                    p.domicilio,
                    p.ciudad,
                    p.marca,
                    p.idPropietario,
                    p.creado,
                    p.idIva,
                    i.iva
                FROM dbo.Personas p
                LEFT JOIN dbo.Iva i ON i.id = p.idIva
                WHERE p.idPersona = @id;";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@id", id);

                if (con.State != ConnectionState.Open) con.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        oPersona = new Entidades.Persona
                        {
                            idPersona = Convert.ToInt32(dr["idPersona"]),
                            tipo = Convert.ToString(dr["tipo"]),
                            Identificacion = Convert.ToString(dr["identificacion"]),
                            razonSocial = Convert.ToString(dr["razonSocial"]),
                            Iva = dr["iva"] == DBNull.Value ? null : Convert.ToString(dr["iva"]),
                            IdIva = dr["idIva"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idIva"]),
                            Cuit = Convert.ToString(dr["cuit"]),
                            Telefono = Convert.ToString(dr["telefono"]),
                            Domicilio = Convert.ToString(dr["domicilio"]),
                            Ciudad = Convert.ToString(dr["ciudad"]),
                            CtaCte = dr["ctaCte"] != DBNull.Value && Convert.ToBoolean(dr["ctaCte"]),
                            Bonificacion = dr["bonificacion"] == DBNull.Value ? 0 : Convert.ToSingle(dr["bonificacion"]),
                            OtrosDatos = Convert.ToString(dr["otrosDatos"]),
                            Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                            Marca = dr["marca"] != DBNull.Value && Convert.ToBoolean(dr["marca"]),
                            IdPropietario = dr["idPropietario"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idPropietario"])
                        };
                    }
                }
            }

            return oPersona;
        }

        public DataTable buscarProveedor(string buscarTexto)
        {
            var dt = new DataTable();

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("buscarProveedor", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", buscarTexto ?? "");

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable buscarPersona(string buscarTexto, bool? marca)
        {
            var dt = new DataTable();

            string sql;
            if (marca.HasValue && marca.Value)
            {
                sql = @"
                    SELECT 
                        p.idPersona,
                        p.razonSocial AS Marca,
                        p.otrosDatos AS otrosDatos,
                        prop.razonSocial AS Propietario,
                        prop.cuit AS cuit,
                        prop.telefono AS telefono,
                        prop.domicilio AS domicilio,
                        prop.ciudad AS ciudad
                    FROM Personas p
                    LEFT JOIN Personas prop ON p.idPropietario = prop.idPersona
                    WHERE p.marca = 1
                      AND (p.identificacion LIKE @texto ESCAPE '\' OR p.razonSocial LIKE @texto ESCAPE '\');";
            }
            else
            {
                sql = @"
                    SELECT  
                        p.idPersona,
                        p.identificacion AS nombreIdentif,
                        p.razonSocial,
                        i.abrev AS iva,
                        p.cuit,
                        p.telefono,
                        p.ctaCte,
                        p.bonificacion,
                        p.domicilio,
                        p.ciudad,
                        p.otrosDatos
                    FROM dbo.Personas p
                    LEFT JOIN dbo.Iva i ON i.id = p.idIva
                    WHERE p.marca = 0
                      AND (
                            p.identificacion LIKE @texto ESCAPE '\'
                         OR p.razonSocial    LIKE @texto ESCAPE '\'
                         OR p.cuit           LIKE @texto ESCAPE '\'
                      );";
            }

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", LikePattern(buscarTexto));

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable getIva()
        {
            var dt = new DataTable();

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("SELECT * FROM Iva", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                da.Fill(dt);
            }

            return dt;
        }

        public int existeCuit(string cuit)
        {
            string cuitNorm = NormalizarCuit(cuit);
            if (string.IsNullOrEmpty(cuitNorm)) return 0;

            // Validar numérico como tenías
            if (!long.TryParse(cuitNorm, out _))
                return 0;

            string sql = "SELECT TOP 1 idPersona FROM Personas WHERE REPLACE(cuit, '-', '') = @cuit;";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@cuit", cuitNorm);

                if (con.State != ConnectionState.Open) con.Open();
                object result = cmd.ExecuteScalar();

                return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            }
        }

        public bool personaTieneCompras_Ventas(int idPersona)
        {
            string sql = @"
                SELECT 
                    CASE 
                        WHEN EXISTS (SELECT 1 FROM Ventas WHERE idPersona = @idPersona) 
                          OR EXISTS (SELECT 1 FROM Compras WHERE idProveedor = @idPersona)
                        THEN 1 ELSE 0
                    END;";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.AddWithValue("@idPersona", idPersona);

                if (con.State != ConnectionState.Open) con.Open();
                int existe = Convert.ToInt32(cmd.ExecuteScalar());
                return existe == 1;
            }
        }

        public DataTable obtenerProveedores()
        {
            var dt = new DataTable();

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand("buscarPersona", con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = conn.TimeOut();

                da.Fill(dt);
            }

            return dt;
        }

        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            var dt = new DataTable();

            string sql = @"
                SELECT 
                    p.idPersona,
                    p.razonSocial as Marca,
                    p.otrosDatos AS otrosDatos,
                    prop.razonSocial AS Propietario
                FROM Personas p
                LEFT JOIN Personas prop ON p.idPropietario = prop.idPersona
                WHERE p.idPersona <> @idMarca
                  AND p.marca = 1
                  AND p.razonSocial COLLATE Latin1_General_CI_AI LIKE @texto;";

            using (var con = conn.conectar(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.AddWithValue("@texto", LikePattern(buscarTexto));
                cmd.Parameters.AddWithValue("@idMarca", idMarca);

                da.Fill(dt);
            }

            return dt;
        }

        #endregion
    }
}
