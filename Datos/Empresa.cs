using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    // Acceso a datos de dbo.Empresas para la pantalla "Mi Empresa" (admin de la propia empresa,
    // no confundir con el CRUD cross-tenant de SystemAdministrationRepository, pensado para el
    // super-admin de la plataforma). Antes de esto no existia ningun metodo de ESCRITURA sobre
    // Empresas fuera de ese repositorio -- la unica lectura existente era
    // Datos/Sucursal.cs:findEmpresaById, que se deja intacta.
    public class Empresa
    {
        private readonly IEmpresaContext _empresa;

        public Empresa(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        public Entidades.Empresa findById(int idEmpresa)
        {
            const string sql = "SELECT * FROM Empresas WHERE idEmpresa = @id";

            var lista = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                map: dr => mapEmpresa(dr),
                setParams: p => p.Add("@id", SqlDbType.Int).Value = idEmpresa
            );

            return lista.Count > 0 ? lista[0] : null;
        }

        // Actualiza solo los campos editables desde "Mi Empresa" (datos basicos no-AFIP + horario
        // laboral) -- nunca toca RazonSocialAfip/Cuit/Iibb/CondicionIVA/InicioActividad/TenantSlug/
        // BasePath/EsRRII/NombreCertificado_pfx/Entorno_HOMO_PROD/BaseDatosNombre/Activa, que son
        // campos fiscales/de infraestructura del tenant reservados al super-admin de plataforma.
        public void ActualizarDatosBasicos(Entidades.Empresa oEmpresaE)
        {
            if (oEmpresaE == null) throw new ArgumentNullException(nameof(oEmpresaE));

            const string sql = @"
                UPDATE Empresas
                SET nombreFantasia = @NombreFantasia,
                    slogan1 = @Slogan1,
                    slogan2 = @Slogan2,
                    slogan3 = @Slogan3,
                    domicilio = @Domicilio,
                    ciudad = @Ciudad,
                    pais = @Pais,
                    telefono = @Telefono,
                    email = @Email,
                    HorarioDiurnoDesde = @HorarioDiurnoDesde,
                    HorarioDiurnoHasta = @HorarioDiurnoHasta,
                    HorarioTardeDesde = @HorarioTardeDesde,
                    HorarioTardeHasta = @HorarioTardeHasta
                WHERE idEmpresa = @IdEmpresa;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@IdEmpresa", SqlDbType.Int).Value = oEmpresaE.IdEmpresa;
                    p.Add("@NombreFantasia", SqlDbType.NVarChar).Value = (object)oEmpresaE.NombreFantasia ?? DBNull.Value;
                    p.Add("@Slogan1", SqlDbType.NVarChar).Value = (object)oEmpresaE.Slogan1 ?? DBNull.Value;
                    p.Add("@Slogan2", SqlDbType.NVarChar).Value = (object)oEmpresaE.Slogan2 ?? DBNull.Value;
                    p.Add("@Slogan3", SqlDbType.NVarChar).Value = (object)oEmpresaE.Slogan3 ?? DBNull.Value;
                    p.Add("@Domicilio", SqlDbType.NVarChar).Value = (object)oEmpresaE.Domicilio ?? DBNull.Value;
                    p.Add("@Ciudad", SqlDbType.NVarChar).Value = (object)oEmpresaE.Ciudad ?? DBNull.Value;
                    p.Add("@Pais", SqlDbType.NVarChar).Value = (object)oEmpresaE.Pais ?? DBNull.Value;
                    p.Add("@Telefono", SqlDbType.NVarChar).Value = (object)oEmpresaE.Telefono ?? DBNull.Value;
                    p.Add("@Email", SqlDbType.NVarChar).Value = (object)oEmpresaE.Email ?? DBNull.Value;
                    p.Add("@HorarioDiurnoDesde", SqlDbType.Time).Value = oEmpresaE.HorarioDiurnoDesde;
                    p.Add("@HorarioDiurnoHasta", SqlDbType.Time).Value = oEmpresaE.HorarioDiurnoHasta;
                    p.Add("@HorarioTardeDesde", SqlDbType.Time).Value = oEmpresaE.HorarioTardeDesde;
                    p.Add("@HorarioTardeHasta", SqlDbType.Time).Value = oEmpresaE.HorarioTardeHasta;
                }
            );
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
                Activa = dr["activa"] != DBNull.Value ? Convert.ToByte(dr["activa"]) : (byte)0,
                HorarioDiurnoDesde = GetOptionalTime(dr, "HorarioDiurnoDesde", TimeSpan.Zero),
                HorarioDiurnoHasta = GetOptionalTime(dr, "HorarioDiurnoHasta", new TimeSpan(23, 59, 59)),
                HorarioTardeDesde = GetOptionalTime(dr, "HorarioTardeDesde", TimeSpan.Zero),
                HorarioTardeHasta = GetOptionalTime(dr, "HorarioTardeHasta", new TimeSpan(23, 59, 59))
            };
        }

        // Lectura defensiva por si el script de migracion todavia no corrio en algun ambiente
        // (mismo patron que Datos/Sucursal.cs GetOptionalDecimal/GetOptionalInt/GetOptionalBool).
        private static bool HasColumn(IDataRecord dr, string columnName)
        {
            for (var i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(dr.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static TimeSpan GetOptionalTime(IDataRecord dr, string columnName, TimeSpan defaultValue)
        {
            if (!HasColumn(dr, columnName))
                return defaultValue;

            object value = dr[columnName];
            return value == DBNull.Value ? defaultValue : (TimeSpan)value;
        }
    }
}
