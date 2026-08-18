using System;
using System.Collections.Generic;
using System.Data;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ISucursalRepository -- segunda entidad de la
    // migracion (Etapa 3, ver docs/DECISIONS.md). Mismo criterio de alcance que PersonaPg:
    // solo se implementan de verdad los metodos que tocan Sucursal/Empresas; los que dependen
    // de la topologia legacy de 3 servidores (San Martin/San Lorenzo, tabla Conexiones) quedan
    // marcados con NotImplementedException -- esa topologia esta fuera de alcance desde la
    // Etapa 1.
    public class SucursalPg : Contratos.ISucursalRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public SucursalPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private const string ColumnasSucursal = @"
            idsucursal, sucursal, idempresa, codpuntoventaafip, direccion, localidad,
            provincia, pais, latitud, longitud, radiologinmetros, validarubicacionlogin";

        private Entidades.Sucursal MapSucursal(IDataRecord dr)
        {
            return new Entidades.Sucursal
            {
                IdSucursal = Convert.ToInt32(dr["idsucursal"]),
                SucursalNombre = dr["sucursal"] as string,
                IdEmpresa = dr["idempresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idempresa"]),
                CodPuntoVentaAfip = dr["codpuntoventaafip"] == DBNull.Value ? 0 : Convert.ToInt32(dr["codpuntoventaafip"]),
                Direccion = dr["direccion"] as string,
                Localidad = dr["localidad"] as string,
                Provincia = dr["provincia"] as string,
                Pais = dr["pais"] as string,
                Latitud = dr["latitud"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["latitud"]),
                Longitud = dr["longitud"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(dr["longitud"]),
                RadioLoginMetros = dr["radiologinmetros"] == DBNull.Value ? 200 : Convert.ToInt32(dr["radiologinmetros"]),
                ValidarUbicacionLogin = dr["validarubicacionlogin"] != DBNull.Value && Convert.ToBoolean(dr["validarubicacionlogin"])
            };
        }

        private Entidades.Empresa MapEmpresa(IDataRecord dr)
        {
            return new Entidades.Empresa
            {
                IdEmpresa = Convert.ToInt32(dr["idempresa"]),
                RazonSocialAfip = dr["razonsocialafip"] as string,
                Cuit = dr["cuit"] == DBNull.Value ? 0 : Convert.ToInt64(dr["cuit"]),
                NombreFantasia = dr["nombrefantasia"] as string,
                Slogan1 = dr["slogan1"] as string,
                Slogan2 = dr["slogan2"] as string,
                Slogan3 = dr["slogan3"] as string,
                Iibb = dr["iibb"] == DBNull.Value ? 0 : Convert.ToInt64(dr["iibb"]),
                CondicionIVA = dr["condicioniva"] as string,
                InicioActividad = dr["inicioactividad"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dr["inicioactividad"]),
                TenantSlug = dr["tenantslug"] as string,
                Domicilio = dr["domicilio"] as string,
                Ciudad = dr["ciudad"] as string,
                Pais = dr["pais"] as string,
                Telefono = dr["telefono"] as string,
                Email = dr["email"] as string,
                BasePath = dr["basepath"] as string,
                EsRRII = dr["esrrii"] != DBNull.Value && Convert.ToBoolean(dr["esrrii"]),
                NombreCertificado_pfx = dr["nombrecertificado_pfx"] as string,
                Entorno_HOMO_PROD = dr["entorno_homo_prod"] as string,
                BaseDatosNombre = dr["basedatosnombre"] as string,
                Activa = dr["activa"] == DBNull.Value ? (byte)0 : Convert.ToByte(dr["activa"])
            };
        }

        public DataTable obtenerSucursales()
        {
            return DbPg.DataTable(_connectionString, _idEmpresa,
                $"SELECT {ColumnasSucursal} FROM sucursal WHERE idempresa = @id;",
                p => p.AddWithValue("id", _idEmpresa));
        }

        public List<Entidades.Sucursal> findAll()
        {
            return DbPg.Reader(_connectionString, _idEmpresa,
                $"SELECT {ColumnasSucursal} FROM sucursal WHERE idempresa = @id;",
                MapSucursal,
                p => p.AddWithValue("id", _idEmpresa));
        }

        public Entidades.Sucursal findById(int id)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa,
                $"SELECT {ColumnasSucursal} FROM sucursal WHERE idsucursal = @id;",
                MapSucursal,
                p => p.AddWithValue("id", id));

            var oSucursalE = lista.Count > 0 ? lista[0] : null;

            if (oSucursalE != null && oSucursalE.IdEmpresa > 0)
            {
                oSucursalE.Empresa = findEmpresaById(oSucursalE.IdEmpresa);
            }

            return oSucursalE;
        }

        public Entidades.Empresa findEmpresaById(int idEmpresa)
        {
            const string sql = @"
                SELECT idempresa, razonsocialafip, cuit, nombrefantasia, slogan1, slogan2, slogan3,
                       iibb, condicioniva, inicioactividad, tenantslug, domicilio, ciudad, pais,
                       telefono, email, basepath, esrrii, nombrecertificado_pfx, entorno_homo_prod,
                       basedatosnombre, activa
                FROM empresas WHERE idempresa = @id;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, MapEmpresa,
                p => p.AddWithValue("id", idEmpresa));

            return lista.Count > 0 ? lista[0] : null;
        }

        public Entidades.Empresa findEmpresaByCuit(long cuit)
        {
            const string sql = @"
                SELECT idempresa, razonsocialafip, cuit, nombrefantasia, slogan1, slogan2, slogan3,
                       iibb, condicioniva, inicioactividad, tenantslug, domicilio, ciudad, pais,
                       telefono, email, basepath, esrrii, nombrecertificado_pfx, entorno_homo_prod,
                       basedatosnombre, activa
                FROM empresas WHERE cuit = @cuit;";

            var lista = DbPg.Reader(_connectionString, _idEmpresa, sql, MapEmpresa,
                p => p.AddWithValue("cuit", cuit));

            return lista.Count > 0 ? lista[0] : null;
        }

        public void ActualizarDatosBasicos(Entidades.Sucursal oSucursalE)
        {
            if (oSucursalE == null) throw new ArgumentNullException(nameof(oSucursalE));

            const string sql = @"
                UPDATE sucursal SET
                    sucursal = @sucursalNombre,
                    direccion = @direccion,
                    localidad = @localidad,
                    provincia = @provincia,
                    pais = @pais,
                    latitud = @latitud,
                    longitud = @longitud,
                    radiologinmetros = @radioLoginMetros,
                    validarubicacionlogin = @validarUbicacionLogin
                WHERE idsucursal = @idSucursal AND idempresa = @idEmpresa;";

            DbPg.NonQuery(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("idSucursal", oSucursalE.IdSucursal);
                p.AddWithValue("idEmpresa", oSucursalE.IdEmpresa);
                p.AddWithValue("sucursalNombre", (object)oSucursalE.SucursalNombre ?? DBNull.Value);
                p.AddWithValue("direccion", (object)oSucursalE.Direccion ?? DBNull.Value);
                p.AddWithValue("localidad", (object)oSucursalE.Localidad ?? DBNull.Value);
                p.AddWithValue("provincia", (object)oSucursalE.Provincia ?? DBNull.Value);
                p.AddWithValue("pais", (object)oSucursalE.Pais ?? DBNull.Value);
                p.AddWithValue("latitud", (object)oSucursalE.Latitud ?? DBNull.Value);
                p.AddWithValue("longitud", (object)oSucursalE.Longitud ?? DBNull.Value);
                p.AddWithValue("radioLoginMetros", oSucursalE.RadioLoginMetros);
                p.AddWithValue("validarUbicacionLogin", oSucursalE.ValidarUbicacionLogin);
            });
        }

        // --- Fuera de alcance: topologia legacy de 3 servidores (San Martin/San Lorenzo,
        //     tabla Conexiones), excluida desde la Etapa 1 ---

        public DataTable obtenerSucursalSanMartin()
        {
            throw new NotImplementedException("TODO(claude): atado a la topologia legacy de 3 servidores (idSucursal=2 hardcodeado = San Martin), fuera de alcance de esta migracion.");
        }

        public DataTable obtenerSucursalSanLorenzo()
        {
            throw new NotImplementedException("TODO(claude): atado a la topologia legacy de 3 servidores (idSucursal=1 hardcodeado = San Lorenzo), fuera de alcance de esta migracion.");
        }

        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual)
        {
            // Migrada de verdad en la Etapa 4 (docs/DECISIONS.md 2026-08-18) -- la tabla
            // Conexiones en si no es exclusiva de la topologia legacy, solo lo eran los otros
            // 2 metodos hardcodeados a San Martin/San Lorenzo (siguen NotImplementedException).
            const string sql = @"
                SELECT name, connectionstring, nombre, idsucursal, mostrarenprincipal, mostrarenstockactual, idempresa
                FROM conexiones
                WHERE (@mp::boolean IS NULL OR mostrarenprincipal = @mp)
                  AND (@ms::boolean IS NULL OR mostrarenstockactual = @ms);";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql, p =>
            {
                p.AddWithValue("mp", (object)mostrarEnPrincipal ?? DBNull.Value);
                p.AddWithValue("ms", (object)mostrarEnStockActual ?? DBNull.Value);
            });
        }

        public int getIdSucursalByConexion(string nameConnString)
        {
            if (string.IsNullOrWhiteSpace(nameConnString)) return 0;

            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idsucursal FROM conexiones WHERE name = @name LIMIT 1;",
                p => p.AddWithValue("name", nameConnString.Trim()));

            return (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
        }
    }
}
