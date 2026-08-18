using System;
using System.Data;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IPersonaRepository -- piloto de la Etapa 2
    // de la migracion (ver docs/DECISIONS.md, entrada 2026-08-18, y el plan de la sesion).
    //
    // Alcance deliberadamente acotado: de los 12 metodos de Datos.Persona (SQL Server),
    // solo se implementan de verdad los 6 que tocan unicamente Personas/Iva (sin joins a
    // Compras/Ventas, que quedan fuera de este piloto de una tabla). Los otros 6 quedan
    // marcados con NotImplementedException -- no se inventa un resultado plausible.
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

        // --- Fuera de alcance del piloto de una tabla (requieren Compras/Ventas, que no
        //     estan migradas, o dependen del problema de collation case-insensitive
        //     todavia pendiente -- ver docs/06-datos-e-integraciones/rls-postgres.md) ---

        public DataTable buscarProveedor(string buscarTexto)
        {
            throw new NotImplementedException("TODO(claude): requiere resolver collation case-insensitive (LIKE), fuera de alcance del piloto de una tabla.");
        }

        public DataTable buscarPersona(string buscarTexto, bool? marca)
        {
            throw new NotImplementedException("TODO(claude): requiere resolver collation case-insensitive (LIKE), fuera de alcance del piloto de una tabla.");
        }

        public bool personaTieneCompras_Ventas(int idPersona)
        {
            throw new NotImplementedException("TODO(claude): requiere las tablas Compras/Ventas migradas a Postgres, fuera de alcance del piloto de una tabla.");
        }

        public DataTable obtenerProveedores()
        {
            throw new NotImplementedException("TODO(claude): requiere resolver collation case-insensitive (LIKE), fuera de alcance del piloto de una tabla.");
        }

        public DataTable obtenerProveedoresConCompras()
        {
            throw new NotImplementedException("TODO(claude): requiere la tabla Compras migrada a Postgres, fuera de alcance del piloto de una tabla.");
        }

        public DataTable existenMarcasParecidas(string buscarTexto, int idMarca)
        {
            throw new NotImplementedException("TODO(claude): requiere resolver collation case-insensitive (LIKE), fuera de alcance del piloto de una tabla.");
        }
    }
}
