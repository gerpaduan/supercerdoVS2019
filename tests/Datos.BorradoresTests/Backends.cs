using System;
using System.Threading;
using Contratos;
using Utilidades;

namespace Datos.BorradoresTests
{
    // Motores contra los que se corre el mismo juego de pruebas de los repositorios de borradores.
    // Cada uno decide si esta disponible en esta maquina (base scratch alcanzable); si no, las pruebas
    // se reportan SKIPPED con el motivo, nunca como verdes silenciosas.
    public abstract class BackendRepos
    {
        public abstract string Nombre { get; }

        // null si esta disponible; si no, el motivo por el que se saltean las pruebas.
        public abstract string MotivoNoDisponible();

        public abstract IBorradorGenericoRepository Generico(int idEmpresa);
        public abstract IVentaBorradorRepository Venta(int idEmpresa);
    }

    public sealed class EmpresaFake : IEmpresaContext
    {
        public EmpresaFake(int idEmpresa) { IdEmpresa = idEmpresa; }
        public int IdEmpresa { get; }
    }

    // SQL Server: repositorios de Datos/ sobre la base scratch de App.config (autenticacion Windows).
    public sealed class BackendSqlServer : BackendRepos
    {
        public override string Nombre => "SqlServer";

        public override string MotivoNoDisponible()
        {
            try
            {
                // Probe: las 4 tablas nuevas tienen que existir en la base scratch.
                var empresa = new EmpresaFake(1);
                using (var cn = Db.Open(empresa))
                using (var cmd = cn.CreateCommand())
                {
                    cmd.CommandText = "SELECT COUNT(*) FROM sys.tables WHERE name IN ('BorradorGenerico','BorradorGenericoEvento','VentaBorrador','VentaBorradorEvento')";
                    int tablas = Convert.ToInt32(cmd.ExecuteScalar());
                    return tablas == 4 ? null : "la base scratch de SQL Server no tiene las 4 tablas (correr Datos/DB-Procedures/20260923-Create_Borradores.sql)";
                }
            }
            catch (Exception ex)
            {
                return "SQL Server scratch no alcanzable: " + ex.GetType().Name + " - " + ex.Message;
            }
        }

        public override IBorradorGenericoRepository Generico(int idEmpresa) { return new Datos.BorradorGenerico(new EmpresaFake(idEmpresa)); }
        public override IVentaBorradorRepository Venta(int idEmpresa) { return new Datos.VentaBorrador(new EmpresaFake(idEmpresa)); }
    }

    // Postgres: repositorios de DatosPostgres/ sobre la base scratch indicada en CARNISYS_TEST_PG
    // (cadena Npgsql completa; rol NO superusuario para que aplique RLS). Sin la variable, se saltea.
    public sealed class BackendPostgres : BackendRepos
    {
        private const string VariableEntorno = "CARNISYS_TEST_PG";

        public override string Nombre => "Postgres";

        private static string Cadena => Environment.GetEnvironmentVariable(VariableEntorno);

        public override string MotivoNoDisponible()
        {
            if (string.IsNullOrWhiteSpace(Cadena)) return "falta la variable de entorno " + VariableEntorno + " (cadena Npgsql de una base scratch con las migraciones aplicadas)";
            try
            {
                var repo = new DatosPostgres.BorradorGenericoPg(Cadena, 1);
                repo.ObtenerPorId(0);
                return null;
            }
            catch (Exception ex)
            {
                return "Postgres scratch no alcanzable o sin las tablas: " + ex.GetType().Name + " - " + ex.Message;
            }
        }

        public override IBorradorGenericoRepository Generico(int idEmpresa) { return new DatosPostgres.BorradorGenericoPg(Cadena, idEmpresa); }
        public override IVentaBorradorRepository Venta(int idEmpresa) { return new DatosPostgres.VentaBorradorPg(Cadena, idEmpresa); }
    }

    internal static class IdsDePrueba
    {
        private static int _contador = new Random().Next(100000, 800000);

        // Empresa unica por prueba: las bases scratch no se limpian entre corridas y asi ninguna prueba
        // ve filas de otra (ni de corridas anteriores).
        public static int NuevaEmpresa() { return Interlocked.Increment(ref _contador); }
    }
}
