using System;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IEmpresaRepository (2/2 metodos, pantalla "Mi
    // Empresa"). "empresas" no tiene idempresa como columna de aislamiento (es la propia tabla
    // de tenants) ni RLS -- igual que el original.
    public class EmpresaPg : Contratos.IEmpresaRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public EmpresaPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        private static Entidades.Empresa MapEmpresa(NpgsqlDataReader dr)
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
                Activa = dr["activa"] == DBNull.Value ? (byte)0 : Convert.ToByte(dr["activa"]),
                HorarioDiurnoDesde = dr["horariodiurnodesde"] == DBNull.Value ? TimeSpan.Zero : (TimeSpan)dr["horariodiurnodesde"],
                HorarioDiurnoHasta = dr["horariodiurnohasta"] == DBNull.Value ? new TimeSpan(23, 59, 59) : (TimeSpan)dr["horariodiurnohasta"],
                HorarioTardeDesde = dr["horariotardedesde"] == DBNull.Value ? TimeSpan.Zero : (TimeSpan)dr["horariotardedesde"],
                HorarioTardeHasta = dr["horariotardehasta"] == DBNull.Value ? new TimeSpan(23, 59, 59) : (TimeSpan)dr["horariotardehasta"]
            };
        }

        public Entidades.Empresa findById(int idEmpresa)
        {
            var lista = DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT idempresa, razonsocialafip, cuit, nombrefantasia, slogan1, slogan2, slogan3,
                       iibb, condicioniva, inicioactividad, tenantslug, domicilio, ciudad, pais,
                       telefono, email, basepath, esrrii, nombrecertificado_pfx, entorno_homo_prod,
                       basedatosnombre, activa, horariodiurnodesde, horariodiurnohasta,
                       horariotardedesde, horariotardehasta
                FROM empresas WHERE idempresa = @id;",
                MapEmpresa,
                p => p.AddWithValue("id", idEmpresa));

            return lista.Count > 0 ? lista[0] : null;
        }

        public void ActualizarDatosBasicos(Entidades.Empresa oEmpresaE)
        {
            if (oEmpresaE == null) throw new ArgumentNullException(nameof(oEmpresaE));

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                UPDATE empresas SET
                    nombrefantasia = @nombreFantasia,
                    slogan1 = @slogan1,
                    slogan2 = @slogan2,
                    slogan3 = @slogan3,
                    domicilio = @domicilio,
                    ciudad = @ciudad,
                    pais = @pais,
                    telefono = @telefono,
                    email = @email,
                    horariodiurnodesde = @horarioDiurnoDesde,
                    horariodiurnohasta = @horarioDiurnoHasta,
                    horariotardedesde = @horarioTardeDesde,
                    horariotardehasta = @horarioTardeHasta
                WHERE idempresa = @idEmpresa;",
                p =>
                {
                    p.AddWithValue("idEmpresa", oEmpresaE.IdEmpresa);
                    p.AddWithValue("nombreFantasia", (object)oEmpresaE.NombreFantasia ?? DBNull.Value);
                    p.AddWithValue("slogan1", (object)oEmpresaE.Slogan1 ?? DBNull.Value);
                    p.AddWithValue("slogan2", (object)oEmpresaE.Slogan2 ?? DBNull.Value);
                    p.AddWithValue("slogan3", (object)oEmpresaE.Slogan3 ?? DBNull.Value);
                    p.AddWithValue("domicilio", (object)oEmpresaE.Domicilio ?? DBNull.Value);
                    p.AddWithValue("ciudad", (object)oEmpresaE.Ciudad ?? DBNull.Value);
                    p.AddWithValue("pais", (object)oEmpresaE.Pais ?? DBNull.Value);
                    p.AddWithValue("telefono", (object)oEmpresaE.Telefono ?? DBNull.Value);
                    p.AddWithValue("email", (object)oEmpresaE.Email ?? DBNull.Value);
                    p.AddWithValue("horarioDiurnoDesde", oEmpresaE.HorarioDiurnoDesde);
                    p.AddWithValue("horarioDiurnoHasta", oEmpresaE.HorarioDiurnoHasta);
                    p.AddWithValue("horarioTardeDesde", oEmpresaE.HorarioTardeDesde);
                    p.AddWithValue("horarioTardeHasta", oEmpresaE.HorarioTardeHasta);
                });
        }
    }
}
