using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Sucursal
    {
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public Sucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
        }

        public DataTable obtenerSucursales()
        {
            return Db.DataTable(
                _empresa,
                "SELECT * FROM Sucursal WHERE idEmpresa = @id",
                CommandType.Text,
                setParams: p => p.Add("@id", SqlDbType.Int).Value = _empresa.IdEmpresa
            );
        }

        public Entidades.Sucursal findById(int id)
        {
            const string sql = "SELECT * FROM Sucursal WHERE idSucursal = @id";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr => mapSucursal(dr),
                setParams: p => p.Add("@id", SqlDbType.Int).Value = id
            );

            var oSucursalE = (lista.Count > 0) ? lista[0] : null;

            // === Cargar Empresa si corresponde ===
            if (oSucursalE != null && oSucursalE.IdEmpresa > 0)
            {
                oSucursalE.Empresa = findEmpresaById(oSucursalE.IdEmpresa);
            }

            return oSucursalE;
        }

        public List<Entidades.Sucursal> findAll()
        {
            return Db.Reader(
                _empresa,
                "SELECT * FROM sucursal WHERE idEmpresa = @id",
                CommandType.Text,
                map: dr => mapSucursal(dr),
                setParams: p => p.Add("@id", SqlDbType.Int).Value = _empresa.IdEmpresa 
            );
        }

        public Entidades.Empresa findEmpresaById(int idEmpresa)
        {
            const string sql = "SELECT * FROM Empresas WHERE idEmpresa = @id";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr => mapEmpresa(dr),
                setParams: p => p.Add("@id", SqlDbType.Int).Value = idEmpresa
            );

            return (lista.Count > 0) ? lista[0] : null;
        }

        public Entidades.Empresa findEmpresaByCuit(long cuit)
        {
            const string sql = "SELECT * FROM Empresas WHERE cuit = @cuit";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr => mapEmpresa(dr),
                setParams: p => p.Add("@cuit", SqlDbType.BigInt).Value = cuit
            );

            return (lista.Count > 0) ? lista[0] : null;
        }

        // Actualiza los campos editables desde "Mis Sucursales" (admin de la propia empresa).
        // Nunca toca codPuntoVentaAfip (unico campo AFIP de la entidad) ni idEmpresa -- la
        // pertenencia a la empresa no se puede reasignar desde esta pantalla. Antes de esto, la
        // clase Datos/Sucursal.cs no tenia NINGUN metodo de escritura.
        public void ActualizarDatosBasicos(Entidades.Sucursal oSucursalE)
        {
            if (oSucursalE == null) throw new ArgumentNullException(nameof(oSucursalE));

            const string sql = @"
                UPDATE Sucursal
                SET sucursal = @SucursalNombre,
                    direccion = @Direccion,
                    localidad = @Localidad,
                    provincia = @Provincia,
                    pais = @Pais,
                    Latitud = @Latitud,
                    Longitud = @Longitud,
                    RadioLoginMetros = @RadioLoginMetros,
                    ValidarUbicacionLogin = @ValidarUbicacionLogin
                WHERE idSucursal = @IdSucursal AND idEmpresa = @IdEmpresa;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@IdSucursal", SqlDbType.Int).Value = oSucursalE.IdSucursal;
                    p.Add("@IdEmpresa", SqlDbType.Int).Value = oSucursalE.IdEmpresa;
                    p.Add("@SucursalNombre", SqlDbType.NVarChar).Value = (object)oSucursalE.SucursalNombre ?? DBNull.Value;
                    p.Add("@Direccion", SqlDbType.NVarChar).Value = (object)oSucursalE.Direccion ?? DBNull.Value;
                    p.Add("@Localidad", SqlDbType.NVarChar).Value = (object)oSucursalE.Localidad ?? DBNull.Value;
                    p.Add("@Provincia", SqlDbType.NVarChar).Value = (object)oSucursalE.Provincia ?? DBNull.Value;
                    p.Add("@Pais", SqlDbType.NVarChar).Value = (object)oSucursalE.Pais ?? DBNull.Value;
                    p.Add("@Latitud", SqlDbType.Decimal).Value = (object)oSucursalE.Latitud ?? DBNull.Value;
                    p.Add("@Longitud", SqlDbType.Decimal).Value = (object)oSucursalE.Longitud ?? DBNull.Value;
                    p.Add("@RadioLoginMetros", SqlDbType.Int).Value = oSucursalE.RadioLoginMetros;
                    p.Add("@ValidarUbicacionLogin", SqlDbType.Bit).Value = oSucursalE.ValidarUbicacionLogin;

                    p["@Latitud"].Precision = 10;
                    p["@Latitud"].Scale = 7;
                    p["@Longitud"].Precision = 10;
                    p["@Longitud"].Scale = 7;
                }
            );
        }

        public DataTable obtenerSucursalSanMartin()
        {
            return Db.DataTable(
                _empresa,
                "SELECT * FROM sucursal WHERE idSucursal = 2",
                CommandType.Text
            );
        }

        public DataTable obtenerSucursalSanLorenzo()
        {
            return Db.DataTable(
                _empresa,
                "SELECT * FROM sucursal WHERE idSucursal = 1",
                CommandType.Text
            );
        }

        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual)
        {
            const string sql = @"
                SELECT *
                FROM Conexiones
                WHERE (@mp IS NULL OR mostrarEnPrincipal = @mp)
                  AND (@ms IS NULL OR mostrarEnStockActual = @ms);";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@mp", SqlDbType.Bit).Value = (object)mostrarEnPrincipal ?? DBNull.Value;
                    p.Add("@ms", SqlDbType.Bit).Value = (object)mostrarEnStockActual ?? DBNull.Value;
                }
            );
        }

        public int getIdSucursalByConexion(string nameConnString)
        {
            if (string.IsNullOrWhiteSpace(nameConnString))
                return 0;

            const string sql = "SELECT TOP 1 idSucursal FROM Conexiones WHERE name = @name";

            object obj = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@name", SqlDbType.NVarChar, 100).Value = nameConnString.Trim()
            );

            return (obj == null || obj == DBNull.Value) ? 0 : Convert.ToInt32(obj);
        }

        // --------------------
        // Mapeos
        // --------------------
        private Entidades.Sucursal mapSucursal(SqlDataReader dr)
        {
            return new Entidades.Sucursal
            {
                IdSucursal = dr["idSucursal"] != DBNull.Value ? Convert.ToInt32(dr["idSucursal"]) : 0,
                SucursalNombre = dr["sucursal"] != DBNull.Value ? dr["sucursal"].ToString() : string.Empty,
                IdEmpresa = dr["idEmpresa"] != DBNull.Value ? Convert.ToInt32(dr["idEmpresa"]) : 0,
                CodPuntoVentaAfip = dr["codPuntoVentaAfip"] != DBNull.Value ? Convert.ToInt32(dr["codPuntoVentaAfip"]) : 0,
                Direccion = dr["direccion"] != DBNull.Value ? dr["direccion"].ToString() : string.Empty,
                Localidad = dr["localidad"] != DBNull.Value ? dr["localidad"].ToString() : string.Empty,
                Provincia = dr["provincia"] != DBNull.Value ? dr["provincia"].ToString() : string.Empty,
                Pais = dr["pais"] != DBNull.Value ? dr["pais"].ToString() : string.Empty,
                Latitud = GetOptionalDecimal(dr, "Latitud"),
                Longitud = GetOptionalDecimal(dr, "Longitud"),
                RadioLoginMetros = GetOptionalInt(dr, "RadioLoginMetros", 200),
                ValidarUbicacionLogin = GetOptionalBool(dr, "ValidarUbicacionLogin")
            };
        }

        private Entidades.Empresa mapEmpresa(SqlDataReader dr)
        {
            return new Entidades.Empresa
            {
                IdEmpresa = dr["idEmpresa"] != DBNull.Value ? Convert.ToInt32(dr["idEmpresa"]) : 0,
                RazonSocialAfip = dr["razonSocialAfip"]?.ToString(),
                Cuit = dr["cuit"] != DBNull.Value ? Convert.ToInt64(dr["cuit"]) : 0,
                NombreFantasia = dr["nombreFantasia"]?.ToString(),
                Slogan1 = dr["slogan1"]?.ToString(),
                Slogan2 = dr["slogan2"]?.ToString(),
                Slogan3 = dr["slogan3"]?.ToString(),
                Iibb = dr["iibb"] != DBNull.Value ? Convert.ToInt64(dr["iibb"]) : 0,
                CondicionIVA = dr["condicionIVA"]?.ToString(),
                InicioActividad = dr["inicioActividad"] != DBNull.Value ? Convert.ToDateTime(dr["inicioActividad"]) : DateTime.MinValue,
                TenantSlug = dr["tenantSlug"]?.ToString(),
                Domicilio = dr["domicilio"]?.ToString(),
                Ciudad = dr["ciudad"]?.ToString(),
                Pais = dr["pais"]?.ToString(),
                Telefono = dr["telefono"]?.ToString(),
                Email = dr["email"]?.ToString(),
                BasePath = dr["basePath"]?.ToString(),
                EsRRII = (dr["esRRII"] != DBNull.Value ? Convert.ToByte(dr["esRRII"]) : (byte)0) == 1,
                NombreCertificado_pfx = dr["nombreCertificado_pfx"]?.ToString(),
                Entorno_HOMO_PROD = dr["entorno_HOMO_PROD"]?.ToString(),
                BaseDatosNombre = dr["baseDatosNombre"]?.ToString(),
                Activa = dr["activa"] != DBNull.Value ? Convert.ToByte(dr["activa"]) : (byte)0
            };
        }

        private static bool HasColumn(IDataRecord dr, string columnName)
        {
            for (var i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(dr.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static decimal? GetOptionalDecimal(IDataRecord dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            object value = dr[columnName];
            return value == DBNull.Value ? (decimal?)null : Convert.ToDecimal(value);
        }

        private static int GetOptionalInt(IDataRecord dr, string columnName, int defaultValue)
        {
            if (!HasColumn(dr, columnName))
                return defaultValue;

            object value = dr[columnName];
            return value == DBNull.Value ? defaultValue : Convert.ToInt32(value);
        }

        private static bool GetOptionalBool(IDataRecord dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return false;

            object value = dr[columnName];
            return value != DBNull.Value && Convert.ToBoolean(value);
        }
    }
}
