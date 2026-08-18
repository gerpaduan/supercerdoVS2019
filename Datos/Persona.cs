using System;
using System.Data;
using System.Data.SqlClient;
using Entidades;
using Utilidades;

namespace Datos
{
    public class Persona : Contratos.IPersonaRepository
    {
        private readonly IParametrosContext _param;
        private readonly IEmpresaContext _empresa;

        public Persona(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
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

        public void addOrEditPersona(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            Db.NonQuery(
                _empresa,
                "addOrEditPersona",
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.AddWithValue("@idPersona", oPersonaE.idPersona);
                    p.AddWithValue("@identificacion", oPersonaE.Identificacion ?? "");
                    p.AddWithValue("@razonSocial", oPersonaE.razonSocial ?? "");
                    p.AddWithValue("@idIva", oPersonaE.IdIva);
                    p.AddWithValue("@cuit", oPersonaE.Cuit ?? "");
                    p.AddWithValue("@telefono", oPersonaE.Telefono ?? "");
                    p.AddWithValue("@email", DbNullIfNull(string.IsNullOrWhiteSpace(oPersonaE.Email) ? null : oPersonaE.Email.Trim()));
                    p.AddWithValue("@domicilio", oPersonaE.Domicilio ?? "");
                    p.AddWithValue("@ciudad", oPersonaE.Ciudad ?? "");
                    p.AddWithValue("@otrosDatos", oPersonaE.otrosDatos ?? "");
                    p.AddWithValue("@tipo", oPersonaE.tipo ?? "");
                    p.AddWithValue("@ctaCte", oPersonaE.CtaCte);
                    p.AddWithValue("@bonificacion", oPersonaE.Bonificacion);
                    p.AddWithValue("@marca", oPersonaE.Marca);

                    // idPropietario nullable
                    p.AddWithValue("@idPropietario",
                        oPersonaE.Propietario != null
                            ? (object)oPersonaE.Propietario.idPersona
                            : DBNull.Value);
                }
            );
        }

        // Variante de addOrEditPersona que devuelve el id de la persona creada o modificada.
        // El SP ya hace SELECT @idPersona al final; alcanza con Db.Scalar en vez de Db.NonQuery.
        // Metodo aditivo: no reemplaza a addOrEditPersona para no revisar todos sus llamadores actuales.
        public int addOrEditPersonaConId(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            object resultado = Db.Scalar(
                _empresa,
                "addOrEditPersona",
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.AddWithValue("@idPersona", oPersonaE.idPersona);
                    p.AddWithValue("@identificacion", oPersonaE.Identificacion ?? "");
                    p.AddWithValue("@razonSocial", oPersonaE.razonSocial ?? "");
                    p.AddWithValue("@idIva", oPersonaE.IdIva);
                    p.AddWithValue("@cuit", oPersonaE.Cuit ?? "");
                    p.AddWithValue("@telefono", oPersonaE.Telefono ?? "");
                    p.AddWithValue("@email", DbNullIfNull(string.IsNullOrWhiteSpace(oPersonaE.Email) ? null : oPersonaE.Email.Trim()));
                    p.AddWithValue("@domicilio", oPersonaE.Domicilio ?? "");
                    p.AddWithValue("@ciudad", oPersonaE.Ciudad ?? "");
                    p.AddWithValue("@otrosDatos", oPersonaE.otrosDatos ?? "");
                    p.AddWithValue("@tipo", oPersonaE.tipo ?? "");
                    p.AddWithValue("@ctaCte", oPersonaE.CtaCte);
                    p.AddWithValue("@bonificacion", oPersonaE.Bonificacion);
                    p.AddWithValue("@marca", oPersonaE.Marca);

                    p.AddWithValue("@idPropietario",
                        oPersonaE.Propietario != null
                            ? (object)oPersonaE.Propietario.idPersona
                            : DBNull.Value);
                }
            );

            return (resultado == null || resultado == DBNull.Value) ? 0 : Convert.ToInt32(resultado);
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            Db.NonQuery(
                _empresa,
                "eliminarPersona",
                CommandType.StoredProcedure,
                setParams: p => p.AddWithValue("@idPersona", oPersonaE.idPersona)
            );
        }

        #endregion

        #region Find / Buscar

        public Entidades.Persona findById(int id)
        {
            const string sql = @"
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
                    p.email,
                    p.domicilio,
                    p.ciudad,
                    p.marca,
                    p.idEmpresa,
                    p.idPropietario,
                    p.creado,
                    p.idIva,
                    i.iva
                FROM dbo.Personas p
                LEFT JOIN dbo.Iva i ON i.id = p.idIva
                WHERE p.idPersona = @id;";

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr =>
                {
                    return new Entidades.Persona
                    {
                        idPersona = Convert.ToInt32(dr["idPersona"]),
                        tipo = Convert.ToString(dr["tipo"]),
                        Identificacion = Convert.ToString(dr["identificacion"]),
                        razonSocial = Convert.ToString(dr["razonSocial"]),
                        Iva = dr["iva"] == DBNull.Value ? null : Convert.ToString(dr["iva"]),
                        IdIva = dr["idIva"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idIva"]),
                        Cuit = Convert.ToString(dr["cuit"]),
                        Telefono = Convert.ToString(dr["telefono"]),
                        Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                        Domicilio = Convert.ToString(dr["domicilio"]),
                        Ciudad = Convert.ToString(dr["ciudad"]),
                        IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"]),
                        CtaCte = dr["ctaCte"] != DBNull.Value && Convert.ToBoolean(dr["ctaCte"]),
                        Bonificacion = dr["bonificacion"] == DBNull.Value ? 0 : Convert.ToSingle(dr["bonificacion"]),
                        OtrosDatos = Convert.ToString(dr["otrosDatos"]),
                        Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                        Marca = dr["marca"] != DBNull.Value && Convert.ToBoolean(dr["marca"]),
                        IdPropietario = dr["idPropietario"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idPropietario"])
                    };
                },
                setParams: p => p.AddWithValue("@id", id)
            );

            var persona = (list.Count > 0) ? list[0] : null;
            if (persona != null) 
                persona.ConsumidorFinal = (persona.idPersona > 0 && _param.GetInt(ParamKeys.IdConsumidorFinal, 0) == persona.idPersona);

            return persona;
        }

        public DataTable buscarProveedor(string buscarTexto)
        {
            return Db.DataTable(
                _empresa,
                "buscarProveedor",
                CommandType.StoredProcedure,
                setParams: p => p.AddWithValue("@texto", buscarTexto ?? "")
            );
        }

        public DataTable buscarPersona(string buscarTexto, bool? marca)
        {
            string sql;

            if (marca.HasValue && marca.Value)
            {
                sql = @"
                    SELECT 
                        p.idPersona,
                        p.idEmpresa,
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
                        p.idEmpresa,
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

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.AddWithValue("@texto", LikePattern(buscarTexto))
            );
        }

        public DataTable getIva()
        {
            return Db.DataTable(
                _empresa,
                "SELECT * FROM Iva",
                CommandType.Text
            );
        }

        public int existeCuit(string cuit)
        {
            string cuitNorm = NormalizarCuit(cuit);
            if (string.IsNullOrEmpty(cuitNorm)) return 0;

            // Validar numérico como tenías
            if (!long.TryParse(cuitNorm, out _))
                return 0;

            const string sql = "SELECT TOP 1 idPersona FROM Personas WHERE REPLACE(cuit, '-', '') = @cuit;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.AddWithValue("@cuit", cuitNorm)
            );

            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }

        public bool personaTieneCompras_Ventas(int idPersona)
        {
            const string sql = @"
                SELECT 
                    CASE 
                        WHEN EXISTS (SELECT 1 FROM Ventas WHERE idPersona = @idPersona) 
                          OR EXISTS (SELECT 1 FROM Compras WHERE idProveedor = @idPersona)
                        THEN 1 ELSE 0
                    END;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.AddWithValue("@idPersona", idPersona)
            );

            int existe = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            return existe == 1;
        }

        public DataTable obtenerProveedores()
        {
            return Db.DataTable(
                _empresa,
                "buscarPersona",
                CommandType.StoredProcedure
            );
        }

        public DataTable obtenerProveedoresConCompras()
        {
            const string sql = @"
                SELECT DISTINCT
                    p.idPersona,
                    p.razonSocial
                FROM dbo.Compras c
                INNER JOIN dbo.Personas p ON p.idPersona = c.idProveedor
                WHERE ISNULL(c.estado, '') = ''
                ORDER BY p.razonSocial;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text
            );
        }

        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            const string sql = @"
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

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.AddWithValue("@texto", LikePattern(buscarTexto));
                    p.AddWithValue("@idMarca", idMarca);
                }
            );
        }

        #endregion
    }
}
