using System;
using System.Data;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IPersonaRepository -- arranco como piloto de la
    // Etapa 2 (ver docs/DECISIONS.md, entrada 2026-08-18) y se fue cerrando a medida que se
    // cableo cada controller real (PersonasController, ProductosController).
    //
    // De los 12 metodos de Datos.Persona (SQL Server), 10 estan implementados. Quedan sin
    // implementar `buscarProveedor` y `obtenerProveedores`: sin caller en ningun controller
    // ya cableado a NegocioFactory (los usa StockController, todavia no cableado).
    public class PersonaPg : Contratos.IPersonaRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public PersonaPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private const string ColumnasFindById = @"
            p.idpersona,
            p.identificacion,
            p.razonsocial,
            p.tipo,
            p.otrosdatos,
            p.ctacte,
            p.bonificacion,
            p.cuit,
            p.telefono,
            p.email,
            p.domicilio,
            p.ciudad,
            p.marca,
            p.idempresa,
            p.idpropietario,
            p.creado,
            p.idiva,
            i.iva";

        public Entidades.Persona findById(int id)
        {
            string sql = $@"
                SELECT {ColumnasFindById}
                FROM personas p
                LEFT JOIN iva i ON i.id = p.idiva
                WHERE p.idpersona = @id;";

            Entidades.Persona persona = null;

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql,
                map: dr => new Entidades.Persona
                {
                    idPersona = Convert.ToInt32(dr["idpersona"]),
                    tipo = dr["tipo"] as string,
                    Identificacion = dr["identificacion"] as string,
                    razonSocial = dr["razonsocial"] as string,
                    Iva = dr["iva"] == DBNull.Value ? null : Convert.ToString(dr["iva"]),
                    IdIva = dr["idiva"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idiva"]),
                    Cuit = dr["cuit"] as string,
                    Telefono = dr["telefono"] as string,
                    Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                    Domicilio = dr["domicilio"] as string,
                    Ciudad = dr["ciudad"] as string,
                    IdEmpresa = dr["idempresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idempresa"]),
                    CtaCte = dr["ctacte"] != DBNull.Value && Convert.ToBoolean(dr["ctacte"]),
                    Bonificacion = dr["bonificacion"] == DBNull.Value ? 0 : Convert.ToSingle(dr["bonificacion"]),
                    OtrosDatos = dr["otrosdatos"] as string,
                    Creado = dr["creado"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["creado"]),
                    Marca = dr["marca"] != DBNull.Value && Convert.ToBoolean(dr["marca"]),
                    IdPropietario = dr["idpropietario"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idpropietario"])
                },
                setParams: p => p.AddWithValue("id", id));

            if (lista.Count > 0) persona = lista[0];
            return persona;
        }

        public void addOrEditPersona(Entidades.Persona oPersonaE)
        {
            addOrEditPersonaConId(oPersonaE);
        }

        public int addOrEditPersonaConId(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            if (oPersonaE.idPersona == 0)
            {
                // Alta: idEmpresa siempre sale del tenant de la sesion, nunca de lo que
                // venga en el objeto -- mismo criterio que el fix del bug real de SQL
                // Server (docs/09-cambios-y-pendientes/bitacora-de-cambios.md, 2026-08-18).
                const string sql = @"
                    INSERT INTO personas
                        (identificacion, razonsocial, idiva, cuit, telefono, email, domicilio,
                         ciudad, otrosdatos, tipo, ctacte, bonificacion, marca, idpropietario, idempresa)
                    VALUES
                        (@identificacion, @razonSocial, @idIva, @cuit, @telefono, @email, @domicilio,
                         @ciudad, @otrosDatos, @tipo, @ctaCte, @bonificacion, @marca, @idPropietario, @idEmpresa)
                    RETURNING idpersona;";

                object nuevoId = DbPg.Scalar(_connectionString, _idEmpresa, sql, p =>
                {
                    p.AddWithValue("identificacion", oPersonaE.Identificacion ?? "");
                    p.AddWithValue("razonSocial", oPersonaE.razonSocial ?? "");
                    p.AddWithValue("idIva", oPersonaE.IdIva);
                    p.AddWithValue("cuit", oPersonaE.Cuit ?? "");
                    p.AddWithValue("telefono", oPersonaE.Telefono ?? "");
                    p.AddWithValue("email", string.IsNullOrWhiteSpace(oPersonaE.Email) ? (object)DBNull.Value : oPersonaE.Email.Trim());
                    p.AddWithValue("domicilio", oPersonaE.Domicilio ?? "");
                    p.AddWithValue("ciudad", oPersonaE.Ciudad ?? "");
                    p.AddWithValue("otrosDatos", oPersonaE.otrosDatos ?? "");
                    p.AddWithValue("tipo", oPersonaE.tipo ?? "");
                    p.AddWithValue("ctaCte", oPersonaE.CtaCte);
                    p.AddWithValue("bonificacion", oPersonaE.Bonificacion);
                    p.AddWithValue("marca", oPersonaE.Marca);
                    p.AddWithValue("idPropietario", oPersonaE.Propietario != null ? (object)oPersonaE.Propietario.idPersona : DBNull.Value);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });

                return Convert.ToInt32(nuevoId);
            }
            else
            {
                const string sql = @"
                    UPDATE personas SET
                        identificacion = @identificacion,
                        razonsocial = @razonSocial,
                        idiva = @idIva,
                        cuit = @cuit,
                        telefono = @telefono,
                        email = @email,
                        domicilio = @domicilio,
                        ciudad = @ciudad,
                        tipo = @tipo,
                        otrosdatos = @otrosDatos,
                        ctacte = @ctaCte,
                        bonificacion = @bonificacion,
                        marca = @marca,
                        idpropietario = @idPropietario
                    WHERE idpersona = @idPersona;";

                DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
                {
                    p.AddWithValue("identificacion", oPersonaE.Identificacion ?? "");
                    p.AddWithValue("razonSocial", oPersonaE.razonSocial ?? "");
                    p.AddWithValue("idIva", oPersonaE.IdIva);
                    p.AddWithValue("cuit", oPersonaE.Cuit ?? "");
                    p.AddWithValue("telefono", oPersonaE.Telefono ?? "");
                    p.AddWithValue("email", string.IsNullOrWhiteSpace(oPersonaE.Email) ? (object)DBNull.Value : oPersonaE.Email.Trim());
                    p.AddWithValue("domicilio", oPersonaE.Domicilio ?? "");
                    p.AddWithValue("ciudad", oPersonaE.Ciudad ?? "");
                    p.AddWithValue("otrosDatos", oPersonaE.otrosDatos ?? "");
                    p.AddWithValue("tipo", oPersonaE.tipo ?? "");
                    p.AddWithValue("ctaCte", oPersonaE.CtaCte);
                    p.AddWithValue("bonificacion", oPersonaE.Bonificacion);
                    p.AddWithValue("marca", oPersonaE.Marca);
                    p.AddWithValue("idPropietario", oPersonaE.Propietario != null ? (object)oPersonaE.Propietario.idPersona : DBNull.Value);
                    p.AddWithValue("idPersona", oPersonaE.idPersona);
                });

                return oPersonaE.idPersona;
            }
        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            if (oPersonaE == null) throw new ArgumentNullException(nameof(oPersonaE));

            DbPg.NonQuery(_connectionString, _idEmpresa,
                "DELETE FROM personas WHERE idpersona = @idPersona;",
                p => p.AddWithValue("idPersona", oPersonaE.idPersona));
        }

        public int existeCuit(string cuit)
        {
            string cuitNorm = (cuit ?? "").Trim().Replace("-", "");
            if (string.IsNullOrEmpty(cuitNorm) || !long.TryParse(cuitNorm, out _)) return 0;

            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idpersona FROM personas WHERE REPLACE(cuit, '-', '') = @cuit LIMIT 1;",
                p => p.AddWithValue("cuit", cuitNorm));

            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }

        public DataTable getIva()
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, "SELECT * FROM iva;");
        }

        // Mismo escapado que Datos/Persona.cs (EscapeLike/LikePattern): % _ [ y la barra
        // invertida, con ESCAPE '\' -- Postgres soporta ESCAPE en ILIKE identico a SQL Server.
        private static string EscapeLike(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            return text
                .Replace(@"\", @"\\")
                .Replace("%", @"\%")
                .Replace("_", @"\_")
                .Replace("[", @"\[");
        }

        private static string LikePattern(string text) => "%" + EscapeLike((text ?? "").Trim()) + "%";

        // Cerrado (ver docs/DECISIONS.md): el bloqueo original ("Compras/Ventas sin migrar")
        // ya no aplica -- CompraPg/VentaPg existen. LIKE -> ILIKE, mismo patron ya usado y
        // verificado en CatalogoGlobalProductoPg.
        public DataTable buscarPersona(string buscarTexto, bool? marca)
        {
            string sql;

            if (marca.HasValue && marca.Value)
            {
                sql = @"
                    SELECT
                        p.idpersona AS ""idPersona"",
                        p.idempresa AS ""idEmpresa"",
                        p.razonsocial AS ""Marca"",
                        p.otrosdatos AS ""otrosDatos"",
                        prop.razonsocial AS ""Propietario"",
                        prop.cuit AS ""cuit"",
                        prop.telefono AS ""telefono"",
                        prop.domicilio AS ""domicilio"",
                        prop.ciudad AS ""ciudad""
                    FROM personas p
                    LEFT JOIN personas prop ON p.idpropietario = prop.idpersona
                    WHERE p.marca = true
                      AND (p.identificacion ILIKE @texto ESCAPE '\' OR p.razonsocial ILIKE @texto ESCAPE '\');";
            }
            else
            {
                sql = @"
                    SELECT
                        p.idpersona AS ""idPersona"",
                        p.idempresa AS ""idEmpresa"",
                        p.identificacion AS ""nombreIdentif"",
                        p.razonsocial AS ""razonSocial"",
                        i.abrev AS ""iva"",
                        p.cuit AS ""cuit"",
                        p.telefono AS ""telefono"",
                        p.ctacte AS ""ctaCte"",
                        p.bonificacion AS ""bonificacion"",
                        p.domicilio AS ""domicilio"",
                        p.ciudad AS ""ciudad"",
                        p.otrosdatos AS ""otrosDatos""
                    FROM personas p
                    LEFT JOIN iva i ON i.id = p.idiva
                    WHERE p.marca = false
                      AND (
                            p.identificacion ILIKE @texto ESCAPE '\'
                         OR p.razonsocial    ILIKE @texto ESCAPE '\'
                         OR p.cuit           ILIKE @texto ESCAPE '\'
                      );";
            }

            return DbPg.DataTable(_connectionString, _idEmpresa, sql,
                p => p.AddWithValue("texto", LikePattern(buscarTexto)));
        }

        // Cerrado (ver docs/DECISIONS.md): compras/ventas ya existen en Postgres.
        public bool personaTieneCompras_Ventas(int idPersona)
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa, @"
                SELECT
                    CASE
                        WHEN EXISTS (SELECT 1 FROM ventas WHERE idpersona = @idPersona)
                          OR EXISTS (SELECT 1 FROM compras WHERE idproveedor = @idPersona)
                        THEN true ELSE false
                    END;",
                p => p.AddWithValue("idPersona", idPersona));

            return result != null && result != DBNull.Value && Convert.ToBoolean(result);
        }

        public DataTable buscarProveedor(string buscarTexto)
        {
            // Original SQL Server: EXEC buscarProveedor (stored procedure). El SP no existe en
            // la base local de dev (sp_helptext confirma "no existe") -- no hay definicion
            // verificada para traducir. Decision del usuario 2026-08-20: dejar sin implementar
            // en vez de adivinar. El unico caller (StockController.ObtenerProveedoresExistencia)
            // ya envuelve la llamada en try/catch y devuelve lista vacia ante cualquier error --
            // mismo comportamiento degradado que ya tiene SqlServer en esta base local hoy.
            throw new NotImplementedException("TODO(claude): el SP 'buscarProveedor' de SQL Server no existe en la base local de dev -- sin definicion verificada para traducir. Si se confirma que existe en ServidorSM/San Lorenzo, traducir desde ahi.");
        }

        public DataTable obtenerProveedores()
        {
            throw new NotImplementedException("TODO(claude): sin caller todavia (StockController, no cableado a NegocioFactory).");
        }

        public DataTable obtenerProveedoresConCompras()
        {
            const string sql = @"
                SELECT DISTINCT
                    p.idpersona AS ""idPersona"",
                    p.razonsocial AS ""razonSocial""
                FROM compras c
                INNER JOIN personas p ON p.idpersona = c.idproveedor
                WHERE COALESCE(c.estado, '') = ''
                ORDER BY p.razonsocial;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql);
        }

        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            // SQL Server usa COLLATE Latin1_General_CI_AI (case- Y accent-insensitive) --
            // distinto del resto de esta clase, que traduce LIKE a ILIKE (solo case-insensitive,
            // acorde a la collation default Modern_Spanish_CI_AS de la base). Para igualar el
            // accent-insensitive puntual de este metodo se usa la extension unaccent()
            // (requiere CREATE EXTENSION unaccent, ya instalada en la base -- ver docs/DECISIONS.md).
            const string sql = @"
                SELECT
                    p.idpersona AS ""idPersona"",
                    p.razonsocial AS ""Marca"",
                    p.otrosdatos AS ""otrosDatos"",
                    prop.razonsocial AS ""Propietario""
                FROM personas p
                LEFT JOIN personas prop ON p.idpropietario = prop.idpersona
                WHERE p.idpersona <> @idMarca
                  AND p.marca = true
                  AND unaccent(p.razonsocial) ILIKE unaccent(@texto);";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("texto", LikePattern(buscarTexto));
                p.AddWithValue("idMarca", idMarca);
            });
        }
    }
}
