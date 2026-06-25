using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Utilidades
{
    /// <summary>
    /// Factory de conexiones. NO guarda conexiones compartidas.
    /// Crea una nueva SqlConnection, la abre, setea SESSION_CONTEXT('IdEmpresa') y la devuelve.
    /// El caller SIEMPRE debe usar using(...) para cerrar.
    /// </summary>
    public sealed class Conexion
    {
        private const string BaseDatosSinSessionContext = "SuperCerdo";

        public enum tipoConexion
        {
            local,
            sanMartin,
            sanMartinRemoto,
            sanLorenzo,
            sanLorenzoRemoto,
            servidor
        }

        public static tipoConexion tipoConn;

        // Lee el nombre de la cadena desde AppSettings
        public static string connStringActual = ConfigurationManager.AppSettings["connString"];

        // Usa ese nombre para buscar la cadena real en ConnectionStrings
        public static string conString = ConfigurationManager
            .ConnectionStrings[connStringActual]
            .ConnectionString;

        public static int idSucursalAppConfig = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"]);
        public static bool soyYo = ConfigurationManager.AppSettings["cuit"] == "20306210786";
        public static int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings["timeOut"]);

        // ---------------------------
        //  CONEXION (MULTI-TENANT)
        // ---------------------------

        public SqlConnection conectar(IEmpresaContext empresa)
        {
            return conectar(null, empresa);
        }

        public SqlConnection conectar(string conexionSucursal, IEmpresaContext empresa)
        {
            return conectar(conexionSucursal, empresa, true);
        }

        public SqlConnection conectarSinTenant(IEmpresaContext empresa)
        {
            return conectar(null, empresa, false);
        }

        public SqlConnection conectarSinTenant(string conexionSucursal, IEmpresaContext empresa)
        {
            return conectar(conexionSucursal, empresa, false);
        }

        private SqlConnection conectar(string conexionSucursal, IEmpresaContext empresa, bool setearSessionContext)
        {
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));

            string cs = GetConnectionStringFromConfig(conexionSucursal);

            var cn = new SqlConnection(cs);
            cn.Open();

            if (!EsBaseSinSessionContext(cn))
            {
                if (setearSessionContext)
                    SetEmpresaSession(cn, empresa.IdEmpresa);
                else
                    SetAdminSession(cn);
            }

            return cn;
        }

        // ---------------------------
        // SESSION_CONTEXT
        // ---------------------------
        private static void SetEmpresaSession(SqlConnection cn, int idEmpresa)
        {
            using (var cmd = new SqlCommand(
                "EXEC sys.sp_set_session_context @key=N'IdEmpresa', @value=@IdEmpresa, @read_only=1;",
                cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Add("@IdEmpresa", SqlDbType.Int).Value = idEmpresa;
                cmd.ExecuteNonQuery();
            }

        }

        private static void SetAdminSession(SqlConnection cn)
        {
            using (var cmd = new SqlCommand(
                "EXEC sys.sp_set_session_context @key=N'EsAdminCarniSys', @value=1, @read_only=1;",
                cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        private static bool EsBaseSinSessionContext(SqlConnection cn)
        {
            string databaseName = cn.Database;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                var csb = new SqlConnectionStringBuilder(cn.ConnectionString);
                databaseName = csb.InitialCatalog;
            }

            return string.Equals(databaseName, BaseDatosSinSessionContext, StringComparison.OrdinalIgnoreCase);
        }

        // ---------------------------
        // TIMEOUT
        // ---------------------------
        public int TimeOut()
        {
            return timeOut;
        }

        // ---------------------------
        // CONNECTION STRING HELPERS
        // ---------------------------
        private static string GetConnectionStringFromConfig(string csNameOverride = null)
        {
            // Si te pasan un nombre de connectionString explícito, usa ese.
            // Si no, usa el nombre guardado en AppSettings["connString"].
            string csName = csNameOverride;

            if (string.IsNullOrWhiteSpace(csName))
                csName = ConfigurationManager.AppSettings["connString"];

            if (string.IsNullOrWhiteSpace(csName))
                throw new ConfigurationErrorsException("Falta AppSettings key 'connString' en el .config.");

            var settings = ConfigurationManager.ConnectionStrings[csName];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("No existe la connectionString '" + csName + "' en <connectionStrings>.");

            return settings.ConnectionString;
        }

        // ---------------------------
        // LEGACY / TUS HELPERS
        // ---------------------------
        public static string getConnString()
        {
            string connString = "";
            switch (tipoConn)
            {
                case Conexion.tipoConexion.local:
                    connString = "local";
                    break;
                case Conexion.tipoConexion.sanMartin:
                    connString = "sanMartin";
                    break;
                case Conexion.tipoConexion.sanMartinRemoto:
                    connString = "sanMartinRemoto";
                    break;
                case Conexion.tipoConexion.sanLorenzo:
                    connString = "sanLorenzo";
                    break;
                case Conexion.tipoConexion.sanLorenzoRemoto:
                    connString = "sanLorenzoRemoto";
                    break;
                case Conexion.tipoConexion.servidor:
                    connString = "Servidor";
                    break;
            }
            connString = ConfigurationManager.ConnectionStrings[connString.ToString()].ToString();
            return connString;
        }

        public static tipoConexion getTipoConexion()
        {
            tipoConn = Conexion.tipoConexion.local;
            switch (connStringActual)
            {
                case "local":
                    tipoConn = Conexion.tipoConexion.local;
                    break;
                case "sanMartin":
                    tipoConn = Conexion.tipoConexion.sanMartin;
                    break;
                case "sanMartinRemoto":
                    tipoConn = Conexion.tipoConexion.sanMartinRemoto;
                    break;
                case "sanLorenzo":
                    tipoConn = Conexion.tipoConexion.sanLorenzo;
                    break;
                case "sanLorenzoRemoto":
                    tipoConn = Conexion.tipoConexion.sanLorenzoRemoto;
                    break;
                case "servidor":
                    tipoConn = Conexion.tipoConexion.servidor;
                    break;
            }
            return tipoConn;
        }

        public static string getSucursalConexion()
        {
            getTipoConexion();
            string sucursalConexion = " | Conn. ";
            switch (tipoConn)
            {
                case Conexion.tipoConexion.local:
                    sucursalConexion += "local";
                    break;
                case Conexion.tipoConexion.sanMartin:
                    sucursalConexion += "San Martín";
                    break;
                case Conexion.tipoConexion.sanMartinRemoto:
                    sucursalConexion += "San Martín";
                    break;
                case Conexion.tipoConexion.sanLorenzo:
                    sucursalConexion += "San Lorenzo";
                    break;
                case Conexion.tipoConexion.sanLorenzoRemoto:
                    sucursalConexion += "San Lorenzo";
                    break;
                case Conexion.tipoConexion.servidor:
                    sucursalConexion += "Servidor";
                    break;
            }
            return sucursalConexion;
        }

        //Se obtiene el id de Sucursal par a la conexión actual
        public static int getIdSucursalConexion()
        {
            int idSucursal = 0;
            switch (tipoConn)
            {
                case Conexion.tipoConexion.local:
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
                case Conexion.tipoConexion.sanMartin:
                    idSucursal = 2;
                    break;
                case Conexion.tipoConexion.sanMartinRemoto:
                    idSucursal = 2;
                    break;
                case Conexion.tipoConexion.sanLorenzo:
                    idSucursal = 1;
                    break;
                case Conexion.tipoConexion.sanLorenzoRemoto:
                    idSucursal = 1;
                    break;
                default:
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
            }
            return idSucursal;
        }

        //Se obtiene el id de Sucursal par a la conexión actual
        public static int getIdSucursalConexion(string conexionSucursal)
        {
            int idSucursal = 0;
            switch (conexionSucursal)
            {
                case "local":
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
                case "sanMartin":
                    idSucursal = 2;
                    break;
                case "sanMartinRemoto":
                    idSucursal = 2;
                    break;
                case "sanLorenzo":
                    idSucursal = 1;
                    break;
                case "sanLorenzoRemoto":
                    idSucursal = 1;
                    break;
                case "servidor":
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
                default:
                    idSucursal = idSucursalAppConfig;
                    break;
            }
            return idSucursal;
        }
    }
}
