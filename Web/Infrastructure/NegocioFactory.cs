using System;
using System.Configuration;
using Utilidades;

namespace Web.Infrastructure
{
    // Punto unico donde se decide, para las 14 clases Negocio ya migradas a Postgres, si
    // instanciarlas contra SQL Server (de siempre) o contra Postgres -- segun el appSetting
    // "DataEngine" de Web.config ("SqlServer" default, o "Postgres"). Vive en Web/ a proposito
    // (no adentro de Negocio/*.cs): asi Negocio.dll y WinForms (Presentacion/, wsAFIPvs2008/)
    // quedan con cero dependencia nueva de DatosPostgres.dll -- WinForms nunca puede terminar
    // en modo Postgres por accidente. Ver docs/DECISIONS.md (piloto: BaseController +
    // EmpresaController) y el plan aprobado en esta sesion.
    //
    // Cada metodo replica exactamente el mismo grafo de dependencias que ya arma
    // MigracionPostgresController para cada accion Comparar* (PersonaPg -> SucursalPg ->
    // CortePg -> VentaPg, etc.). WhatsApp.cs queda afuera a proposito -- feature no
    // implementada, sin interfaz ni DatosPostgres.WhatsAppPg.
    public static class NegocioFactory
    {
        private static bool UsarPostgres =>
            string.Equals(ConfigurationManager.AppSettings["DataEngine"], "Postgres", StringComparison.OrdinalIgnoreCase);

        private static string PgConnString =>
            ConfigurationManager.ConnectionStrings["ConexionPostgresPiloto"].ConnectionString;

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
            // Venta minimo, solo para obtenerTotalVentas (unico metodo que CierreCaja usa de
            // Venta) -- NO se arma via CrearVenta(): CrearVenta ya llama CrearCierreCaja() para
            // su propio cierreCajaN, así que recursar aca (CrearCierreCaja->CrearVenta->
            // CrearCierreCaja->...) produciria un StackOverflowException real (mismo ciclo
            // documentado en Negocio/Venta.cs). ctaCteN/cierreCajaN/personaN quedan en null a
            // proposito: obtenerTotalVentas no los usa.
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
            // Las 6 dependencias internas de Compra (Corte, Sucursal, Usuario, CierreCaja,
            // Persona, CuentaCorriente) se pasan ya cableadas a Postgres -- sin esto, Compra
            // solo escribia su propio bloque en Postgres pero crearMovCtaCteCompra (siempre se
            // llama) seguia pegando a SQL Server. Ver docs/DECISIONS.md, testeo profundo
            // 2026-08-20.
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

        // BarcodeInterpreter y FormatoCodigoBarras: motor de codigos de barra internos de
        // balanza (prefijo EAN 20-29), configurables por empresa. Ver docs/DECISIONS.md y
        // Negocio/BarcodeInterpreter.cs.
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

        // Sin modo SQL Server: mercadopago_config/terminales_mercadopago/mercadopago_sucursal_config
        // nacen directo en Postgres, no hay legado que comparar (ver docs/DECISIONS.md 2026-08-31).
        // La clave de cifrado de los tokens sale de Web.config, nunca de un literal en el codigo
        // (CLAUDE.md §1.1) -- Negocio.dll no lee ConfigurationManager directo porque tambien lo
        // referencia WinForms (Presentacion/), que no tiene este appSetting en su propio config.
        private static byte[] MercadoPagoClaveCifrado
        {
            get
            {
                string claveBase64 = ConfigurationManager.AppSettings["MercadoPagoTokenEncryptionKeyBase64"];
                if (string.IsNullOrWhiteSpace(claveBase64))
                    throw new InvalidOperationException(
                        "Falta el appSetting \"MercadoPagoTokenEncryptionKeyBase64\" en Web.config -- generarlo con " +
                        "Utilidades.MercadoPagoTokenCipher.GenerarClaveBase64() antes de usar la integracion con Mercado Pago.");

                return Convert.FromBase64String(claveBase64);
            }
        }

        public static Negocio.MercadoPagoConfig CrearMercadoPagoConfig(IEmpresaContext empresa)
        {
            var repo = new DatosPostgres.MercadoPagoConfigPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.MercadoPagoConfig(repo, MercadoPagoClaveCifrado);
        }

        public static Negocio.TerminalMercadoPago CrearTerminalMercadoPago(IEmpresaContext empresa)
        {
            var repo = new DatosPostgres.TerminalMercadoPagoPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.TerminalMercadoPago(repo);
        }

        public static Negocio.MercadoPagoSucursalConfig CrearMercadoPagoSucursalConfig(IEmpresaContext empresa)
        {
            var repo = new DatosPostgres.MercadoPagoSucursalConfigPg(PgConnString, empresa.IdEmpresa);
            return new Negocio.MercadoPagoSucursalConfig(repo);
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
            // Mismo motivo que en CrearCompra: las 3 dependencias internas de Venta
            // (CuentaCorriente, CierreCaja, Persona) se pasan ya cableadas a Postgres.
            return new Negocio.Venta(
                repo, empresa, param,
                ctaCteN: CrearCuentaCorriente(empresa, param),
                cierreCajaN: CrearCierreCaja(empresa, param),
                personaN: CrearPersona(empresa, param));
        }

        // Modulo de administracion de plataforma (alta/edicion de Empresas, Sucursales, Usuarios
        // cruzando todos los tenants, gate de superadmin) -- ultimo modulo 100% SQL Server hasta
        // 2026-08-25 (ver docs/DECISIONS.md). Sin IEmpresaContext (a diferencia de los 14 metodos
        // de arriba): el modulo es cross-tenant por diseno, igual que Web/Helpers/
        // SystemAdministrationRepository.cs usa EmpresaContextNulo internamente. Devuelve la
        // interfaz directo (no un Negocio.*): no existe capa Negocio intermedia para este modulo,
        // el propio Web/Helpers/SystemAdministrationRepository.cs siempre fue repositorio+
        // VM-mapper sin capa de dominio aparte.
        public static Web.Helpers.ISystemAdministrationRepository CrearSystemAdministrationRepository()
        {
            if (!UsarPostgres) return new Web.Helpers.SystemAdministrationRepository();

            return new Web.Helpers.SystemAdministrationRepositoryPg(new DatosPostgres.SystemAdministrationPg(PgConnString));
        }
    }
}
