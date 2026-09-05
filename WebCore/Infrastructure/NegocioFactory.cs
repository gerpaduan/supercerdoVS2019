using System;
using Utilidades;

namespace WebCore.Infrastructure
{
    // Port de Web/Infrastructure/NegocioFactory.cs -- ver docs/DECISIONS.md, entrada 2026-09-04
    // "WebCore hibrido SQL Server/Postgres". Punto unico donde se decide, para cada clase Negocio,
    // si instanciarla contra SQL Server (de siempre) o contra Postgres, segun el appSetting
    // "DataEngine" de WebCore/App.config ("SqlServer" default, o "Postgres") -- exactamente el
    // mismo mecanismo que ya usa Web/ clasico, replicado sin reinventar nada.
    //
    // GAP DE PROCESO CORREGIDO 2026-09-04: WebCore instanciaba Negocio.* directo (siempre SQL
    // Server) en cada controller desde el spike inicial. El propio Claude ya habia detectado esto
    // el 2026-09-01 (ver docs/DECISIONS.md, entrada de esa fecha: "Nota para cuando se implemente
    // el NegocioFactory real de WebCore") pero nunca lo escalo como pregunta ni lo agrego a
    // gaps.md, pese a que la finalidad del programa (segun el usuario, confirmado 2026-09-04) es
    // que la plataforma completa -- WebCore incluido -- sea hibrida SQL Server/Postgres, con
    // Postgres como objetivo final para independizarse de licencias de Microsoft. Se corrige
    // portando este factory y reemplazando cada "new Negocio.X(...)" de los controllers por
    // "NegocioFactory.CrearX(...)".
    //
    // Vive en WebCore/Infrastructure (no dentro de Negocio/*.cs), mismo motivo que el original:
    // Negocio.dll y WinForms (Presentacion/) quedan con cero dependencia nueva de
    // DatosPostgres.dll -- WinForms nunca puede terminar en modo Postgres por accidente.
    //
    public static class NegocioFactory
    {
        private static bool UsarPostgres =>
            string.Equals(System.Configuration.ConfigurationManager.AppSettings["DataEngine"], "Postgres", StringComparison.OrdinalIgnoreCase);

        private static string PgConnString =>
            System.Configuration.ConfigurationManager.ConnectionStrings["ConexionPostgresPiloto"].ConnectionString;

        // Portado 2026-09-05 (ver docs/DECISIONS.md "Postgres es la base oficial y unica"): gap
        // encontrado en la auditoria de paridad con Playwright -- SystemAdministrationController
        // instanciaba WebCore.Helpers.SystemAdministrationRepository directo (siempre SQL Server),
        // el unico controller que no seguia este switch todavia.
        public static WebCore.Helpers.ISystemAdministrationRepository CrearSystemAdministrationRepository()
        {
            if (!UsarPostgres) return new WebCore.Helpers.SystemAdministrationRepository();

            return new WebCore.Helpers.SystemAdministrationRepositoryPg(PgConnString);
        }

        public static Negocio.CatalogoGlobalProducto CrearCatalogoGlobalProducto(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.CatalogoGlobalProducto(empresa, param);

            var personaRepo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            var repo = new DatosPostgres.CatalogoGlobalProductoPg(PgConnString, empresa.IdEmpresa, personaRepo);
            return new Negocio.CatalogoGlobalProducto(repo);
        }

        public static Negocio.CierreCaja CrearCierreCaja(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.CierreCaja(empresa, param);

            var repo = new DatosPostgres.CierreCajaPg(PgConnString, empresa.IdEmpresa);
            var sucursalRepo = new DatosPostgres.SucursalPg(PgConnString, empresa.IdEmpresa);
            var personaRepo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            var corteRepo = new DatosPostgres.CortePg(PgConnString, empresa.IdEmpresa, personaRepo);
            var ventaRepo = new DatosPostgres.VentaPg(PgConnString, empresa.IdEmpresa, personaRepo, sucursalRepo, corteRepo);
            var ventaN = new Negocio.Venta(ventaRepo, empresa, param);
            return new Negocio.CierreCaja(repo, empresa, param, ventaN: ventaN, sucursalRepositorio: sucursalRepo);
        }

        public static Negocio.Compra CrearCompra(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.Compra(empresa, param);

            var repo = new DatosPostgres.CompraPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.Compra(
                repo, empresa, param,
                corteN: CrearCorte(empresa, param),
                sucursalN: CrearSucursal(empresa, param),
                usuarioN: CrearUsuario(empresa, param),
                cierreCajaN: CrearCierreCaja(empresa, param),
                personaN: CrearPersona(empresa, param),
                ctaCteN: CrearCuentaCorriente(empresa, param));
        }

        public static Negocio.Corte CrearCorte(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.Corte(empresa, param);

            var personaRepo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            var corteRepo = new DatosPostgres.CortePg(PgConnString, empresa.IdEmpresa, personaRepo);
            var puntoStockRepo = new DatosPostgres.CortePuntoStockSucursalPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.Corte(corteRepo, empresa, param, puntoStockRepo);
        }

        public static Negocio.CortePuntoStockSucursal CrearCortePuntoStockSucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.CortePuntoStockSucursal(empresa, param);

            var repo = new DatosPostgres.CortePuntoStockSucursalPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.CortePuntoStockSucursal(repo);
        }

        public static Negocio.CuentaCorriente CrearCuentaCorriente(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.CuentaCorriente(empresa, param);

            var personaRepo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            var repo = new DatosPostgres.CuentaCorrientePg(PgConnString, empresa.IdEmpresa, personaRepo);
            return new Negocio.CuentaCorriente(repo, empresa, param);
        }

        public static Negocio.BarcodeInterpreter CrearBarcodeInterpreter(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.BarcodeInterpreter(empresa, param);

            var formatoRepo = new DatosPostgres.FormatoCodigoBarrasPg(PgConnString, empresa.IdEmpresa);
            var personaRepo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            var corteRepo = new DatosPostgres.CortePg(PgConnString, empresa.IdEmpresa, personaRepo);
            return new Negocio.BarcodeInterpreter(formatoRepo, corteRepo);
        }

        public static Negocio.FormatoCodigoBarras CrearFormatoCodigoBarras(IEmpresaContext empresa)
        {
            if (!UsarPostgres) return new Negocio.FormatoCodigoBarras(empresa);

            var repo = new DatosPostgres.FormatoCodigoBarrasPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.FormatoCodigoBarras(repo);
        }

        public static Negocio.DispositivoSeguro CrearDispositivoSeguro(IEmpresaContext empresa)
        {
            if (!UsarPostgres) return new Negocio.DispositivoSeguro(empresa);

            var repo = new DatosPostgres.DispositivoSeguroPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.DispositivoSeguro(repo);
        }

        public static Negocio.Empresa CrearEmpresa(IEmpresaContext empresa)
        {
            if (!UsarPostgres) return new Negocio.Empresa(empresa);

            var repo = new DatosPostgres.EmpresaPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.Empresa(repo);
        }

        public static Negocio.OtrasClases CrearOtrasClases(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.OtrasClases(empresa, param);

            var repo = new DatosPostgres.OtrasClasesPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.OtrasClases(repo, empresa, param);
        }

        public static Negocio.Parametros CrearParametros(IEmpresaContext empresa)
        {
            if (!UsarPostgres) return new Negocio.Parametros(empresa);

            var repo = new DatosPostgres.ParametrosPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.Parametros(repo, empresa);
        }

        public static Negocio.Persona CrearPersona(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.Persona(empresa, param);

            var repo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.Persona(repo, empresa, param);
        }

        public static Negocio.Sucursal CrearSucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.Sucursal(empresa, param);

            var repo = new DatosPostgres.SucursalPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.Sucursal(repo, empresa, param);
        }

        public static Negocio.Usuario CrearUsuario(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.Usuario(empresa, param);

            var sucursalRepo = new DatosPostgres.SucursalPg(PgConnString, empresa.IdEmpresa);
            var repo = new DatosPostgres.UsuarioPg(PgConnString, empresa.IdEmpresa, sucursalRepo);
            return new Negocio.Usuario(repo, empresa, param, sucursalRepo);
        }

        public static Negocio.Venta CrearVenta(IEmpresaContext empresa, IParametrosContext param = null)
        {
            if (!UsarPostgres) return new Negocio.Venta(empresa, param);

            var personaRepo = new DatosPostgres.PersonaPg(PgConnString, empresa.IdEmpresa);
            var sucursalRepo = new DatosPostgres.SucursalPg(PgConnString, empresa.IdEmpresa);
            var corteRepo = new DatosPostgres.CortePg(PgConnString, empresa.IdEmpresa, personaRepo);
            var repo = new DatosPostgres.VentaPg(PgConnString, empresa.IdEmpresa, personaRepo, sucursalRepo, corteRepo);
            return new Negocio.Venta(
                repo, empresa, param,
                ctaCteN: CrearCuentaCorriente(empresa, param),
                cierreCajaN: CrearCierreCaja(empresa, param),
                personaN: CrearPersona(empresa, param));
        }
    }
}
