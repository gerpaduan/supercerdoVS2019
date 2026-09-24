using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Contratos;
using Entidades;
using Xunit;

namespace Datos.BorradoresTests
{
    // Juego de pruebas del contrato IBorradorGenericoRepository: es UNO solo y corre contra cada motor
    // (SQL Server = Datos/BorradorGenerico.cs, Postgres = DatosPostgres/BorradorGenericoPg.cs), de modo que
    // cualquier divergencia de comportamiento entre las dos implementaciones aparece como una prueba roja.
    // Cada prueba usa una empresa propia (ver IdsDePrueba): las bases scratch no se limpian.
    public abstract class BorradorGenericoContratoTests
    {
        protected abstract BackendRepos Backend { get; }

        private const int Sucursal = 5;
        private const int Operador = 11;
        private const int OtroOperador = 12;

        private (IBorradorGenericoRepository repo, int empresa) Nuevo()
        {
            string motivo = Backend.MotivoNoDisponible();
            Skip.If(motivo != null, motivo);
            int empresa = IdsDePrueba.NuevaEmpresa();
            return (Backend.Generico(empresa), empresa);
        }

        private static Entidades.BorradorGenerico Borrador(Guid? clientId = null, int operador = Operador, int sucursal = Sucursal,
            string modulo = Entidades.BorradorGenerico.ModuloCompra, string payload = "{\"lineas\":[1]}", int cantLineas = 1, string resumen = "resumen 1")
        {
            return new Entidades.BorradorGenerico
            {
                ClientId = clientId ?? Guid.NewGuid(),
                IdOperador = operador,
                IdUsuarioSesion = operador,
                IdSucursal = sucursal,
                Modulo = modulo,
                Payload = payload,
                CantLineas = cantLineas,
                Resumen = resumen
            };
        }

        [SkippableFact]
        public void Guardar_nuevo_devuelve_Guardado_y_persiste_todos_los_campos()
        {
            var (repo, empresa) = Nuevo();
            var b = Borrador();
            b.IdRegistro = 77;

            Assert.Equal(ResultadoGuardarBorradorGenerico.Guardado, repo.Guardar(b));
            Assert.True(b.Id > 0);

            var leido = repo.ObtenerPorClientId(b.ClientId, b.Modulo);
            Assert.NotNull(leido);
            Assert.Equal(b.Id, leido.Id);
            Assert.Equal(empresa, leido.IdEmpresa);
            Assert.Equal(Sucursal, leido.IdSucursal);
            Assert.Equal(Operador, leido.IdOperador);
            Assert.Equal(77, leido.IdRegistro);
            Assert.Equal("resumen 1", leido.Resumen);
            Assert.Equal(1, leido.CantLineas);
            Assert.Equal(Entidades.BorradorGenerico.EstadoActiva, leido.Estado);
            Assert.NotNull(leido.Payload);
            Assert.Contains("lineas", leido.Payload);
            Assert.Null(leido.Actualizado);
            Assert.Null(leido.Finalizado);
        }

        [SkippableFact]
        public void Guardar_mismo_clientId_del_mismo_operador_actualiza_la_misma_fila()
        {
            var (repo, _) = Nuevo();
            var b = Borrador();
            repo.Guardar(b);
            int idOriginal = b.Id;

            var segundo = Borrador(b.ClientId, payload: "{\"lineas\":[1,2]}", cantLineas: 2, resumen: "resumen 2");
            Assert.Equal(ResultadoGuardarBorradorGenerico.Guardado, repo.Guardar(segundo));
            Assert.Equal(idOriginal, segundo.Id);

            var leido = repo.ObtenerPorId(idOriginal);
            Assert.Equal(2, leido.CantLineas);
            Assert.Equal("resumen 2", leido.Resumen);
            Assert.Contains("2", leido.Payload);
            Assert.NotNull(leido.Actualizado);
        }

        [SkippableFact]
        public void Guardar_de_otro_operador_o_sucursal_es_Ajeno_y_no_modifica_la_fila()
        {
            var (repo, _) = Nuevo();
            var b = Borrador();
            repo.Guardar(b);

            Assert.Equal(ResultadoGuardarBorradorGenerico.Ajeno,
                repo.Guardar(Borrador(b.ClientId, operador: OtroOperador, payload: "{\"x\":1}", cantLineas: 9)));
            Assert.Equal(ResultadoGuardarBorradorGenerico.Ajeno,
                repo.Guardar(Borrador(b.ClientId, sucursal: Sucursal + 1, payload: "{\"x\":1}", cantLineas: 9)));

            var leido = repo.ObtenerPorId(b.Id);
            Assert.Equal(1, leido.CantLineas);
            Assert.Equal(Operador, leido.IdOperador);
        }

        [SkippableFact]
        public void Guardar_con_clientId_de_otro_modulo_no_pisa_y_devuelve_YaCerrado()
        {
            var (repo, _) = Nuevo();
            var b = Borrador(modulo: Entidades.BorradorGenerico.ModuloCompra);
            repo.Guardar(b);

            // Mismo clientId pero otro modulo: no coincide, y ObtenerPorClientId(modulo) no lo encuentra.
            var r = repo.Guardar(Borrador(b.ClientId, modulo: Entidades.BorradorGenerico.ModuloStock, cantLineas: 9));
            Assert.Equal(ResultadoGuardarBorradorGenerico.YaCerrado, r);
            Assert.Equal(1, repo.ObtenerPorId(b.Id).CantLineas);
        }

        [SkippableFact]
        public void Guardar_sobre_borrador_ya_cerrado_devuelve_YaCerrado()
        {
            var (repo, _) = Nuevo();
            var b = Borrador();
            repo.Guardar(b);
            Assert.True(repo.MarcarFinalizada(b.ClientId, b.Modulo, 500));

            Assert.Equal(ResultadoGuardarBorradorGenerico.YaCerrado, repo.Guardar(Borrador(b.ClientId)));
        }

        [SkippableFact]
        public void Guardar_concurrente_del_mismo_clientId_deja_una_sola_fila_y_ningun_error()
        {
            var (repo, _) = Nuevo();
            Guid clientId = Guid.NewGuid();
            const int hilos = 8;
            var resultados = new ResultadoGuardarBorradorGenerico[hilos];
            var errores = new List<Exception>();
            using (var salida = new ManualResetEventSlim(false))
            {
                var tareas = Enumerable.Range(0, hilos).Select(i => Task.Run(() =>
                {
                    salida.Wait();
                    try { resultados[i] = repo.Guardar(Borrador(clientId, cantLineas: i + 1)); }
                    catch (Exception ex) { lock (errores) errores.Add(ex); }
                })).ToArray();
                salida.Set();
                Task.WaitAll(tareas);
            }

            Assert.Empty(errores);
            Assert.All(resultados, r => Assert.Equal(ResultadoGuardarBorradorGenerico.Guardado, r));
            var activas = repo.ListarActivasPorSucursal(Sucursal, Entidades.BorradorGenerico.ModuloCompra);
            Assert.Single(activas.Where(a => a.ClientId == clientId));
        }

        [SkippableFact]
        public void RegistrarLatido_solo_funciona_para_el_dueno_en_su_modulo_y_sucursal()
        {
            var (repo, _) = Nuevo();
            var b = Borrador();
            repo.Guardar(b);

            Assert.True(repo.RegistrarLatido(b.ClientId, Operador, Sucursal, b.Modulo));
            Assert.False(repo.RegistrarLatido(b.ClientId, OtroOperador, Sucursal, b.Modulo));
            Assert.False(repo.RegistrarLatido(b.ClientId, Operador, Sucursal + 1, b.Modulo));
            Assert.False(repo.RegistrarLatido(b.ClientId, Operador, Sucursal, Entidades.BorradorGenerico.ModuloStock));
            Assert.False(repo.RegistrarLatido(Guid.NewGuid(), Operador, Sucursal, b.Modulo));
        }

        [SkippableFact]
        public void EnvejecerLatidoPorCierre_deja_el_borrador_interrumpido_de_inmediato()
        {
            var (repo, _) = Nuevo();
            var b = Borrador();
            repo.Guardar(b);
            Assert.False(repo.ObtenerPorId(b.Id).EstaInterrumpida(5));

            Assert.True(repo.EnvejecerLatidoPorCierre(b.Id));
            var leido = repo.ObtenerPorId(b.Id);
            Assert.True(leido.EstaInterrumpida(5));
            Assert.True(leido.SegundosSinLatido >= 23 * 3600, "SegundosSinLatido=" + leido.SegundosSinLatido);

            // Un latido nuevo lo "revive".
            Assert.True(repo.RegistrarLatido(b.ClientId, Operador, Sucursal, b.Modulo));
            Assert.False(repo.ObtenerPorId(b.Id).EstaInterrumpida(5));

            // Sobre uno cerrado no hace nada.
            repo.MarcarFinalizada(b.ClientId, b.Modulo, 1);
            Assert.False(repo.EnvejecerLatidoPorCierre(b.Id));
        }

        [SkippableFact]
        public void ListarActivasPorSucursal_filtra_por_sucursal_modulo_y_estado_e_incluye_payload()
        {
            var (repo, _) = Nuevo();
            var a = Borrador(); repo.Guardar(a);
            Thread.Sleep(30);
            var b = Borrador(); repo.Guardar(b);
            var otraSucursal = Borrador(sucursal: Sucursal + 1); repo.Guardar(otraSucursal);
            var otroModulo = Borrador(modulo: Entidades.BorradorGenerico.ModuloStock); repo.Guardar(otroModulo);
            var cerrada = Borrador(); repo.Guardar(cerrada); repo.MarcarDescartada(cerrada.Id);

            var lista = repo.ListarActivasPorSucursal(Sucursal, Entidades.BorradorGenerico.ModuloCompra);
            Assert.Equal(new[] { a.Id, b.Id }, lista.Select(x => x.Id).ToArray());
            Assert.All(lista, x => Assert.False(string.IsNullOrEmpty(x.Payload)));
        }

        [SkippableFact]
        public void ListarActivasPorUsuarioSesion_cruza_modulos_y_no_trae_payload()
        {
            var (repo, _) = Nuevo();
            var a = Borrador(); repo.Guardar(a);
            var b = Borrador(modulo: Entidades.BorradorGenerico.ModuloMovimiento); repo.Guardar(b);
            var ajeno = Borrador(operador: OtroOperador); repo.Guardar(ajeno);

            var lista = repo.ListarActivasPorUsuarioSesion(Operador);
            Assert.Equal(2, lista.Count);
            Assert.All(lista, x => Assert.True(string.IsNullOrEmpty(x.Payload)));
        }

        [SkippableFact]
        public void TomarBorrador_transfiere_operador_y_sesion_y_renueva_el_latido()
        {
            var (repo, _) = Nuevo();
            var b = Borrador(); repo.Guardar(b);
            repo.EnvejecerLatidoPorCierre(b.Id);

            Assert.True(repo.TomarBorrador(b.Id, OtroOperador, 99));
            var leido = repo.ObtenerPorId(b.Id);
            Assert.Equal(OtroOperador, leido.IdOperador);
            Assert.Equal(99, leido.IdUsuarioSesion);
            Assert.False(leido.EstaInterrumpida(5));

            repo.MarcarDescartada(b.Id);
            Assert.False(repo.TomarBorrador(b.Id, Operador, 1));
        }

        [SkippableFact]
        public void MarcarFinalizada_vacia_payload_guarda_resultado_y_es_idempotente()
        {
            var (repo, _) = Nuevo();
            var b = Borrador(); repo.Guardar(b);

            Assert.True(repo.MarcarFinalizada(b.ClientId, b.Modulo, 4321));
            var leido = repo.ObtenerPorId(b.Id);
            Assert.Equal(Entidades.BorradorGenerico.EstadoFinalizada, leido.Estado);
            Assert.Equal(4321, leido.IdResultado);
            Assert.True(string.IsNullOrEmpty(leido.Payload));
            Assert.NotNull(leido.Finalizado);

            Assert.False(repo.MarcarFinalizada(b.ClientId, b.Modulo, 9999));
            Assert.Equal(4321, repo.ObtenerPorClientId(b.ClientId, b.Modulo).IdResultado);
            // Con otro modulo no lo encuentra.
            Assert.False(repo.MarcarFinalizada(Borrador().ClientId, Entidades.BorradorGenerico.ModuloStock, 1));
        }

        [SkippableFact]
        public void MarcarDescartada_vacia_payload_y_solo_aplica_a_ACTIVA()
        {
            var (repo, _) = Nuevo();
            var b = Borrador(); repo.Guardar(b);

            Assert.True(repo.MarcarDescartada(b.Id));
            var leido = repo.ObtenerPorId(b.Id);
            Assert.Equal(Entidades.BorradorGenerico.EstadoDescartada, leido.Estado);
            Assert.True(string.IsNullOrEmpty(leido.Payload));
            Assert.False(repo.MarcarDescartada(b.Id));
        }

        [SkippableFact]
        public void PurgarFinalizadasAntiguas_borra_solo_FINALIZADAS_del_modulo()
        {
            var (repo, _) = Nuevo();
            var fin = Borrador(); repo.Guardar(fin); repo.MarcarFinalizada(fin.ClientId, fin.Modulo, 1);
            var finOtroModulo = Borrador(modulo: Entidades.BorradorGenerico.ModuloStock); repo.Guardar(finOtroModulo); repo.MarcarFinalizada(finOtroModulo.ClientId, finOtroModulo.Modulo, 1);
            var activa = Borrador(); repo.Guardar(activa);
            var descartada = Borrador(); repo.Guardar(descartada); repo.MarcarDescartada(descartada.Id);
            Thread.Sleep(50);

            // Con 30 dias no se borra nada (recien finalizada).
            Assert.Equal(0, repo.PurgarFinalizadasAntiguas(Entidades.BorradorGenerico.ModuloCompra, 30));
            // Con 0 dias, todo lo finalizado antes de "ahora" del modulo pedido.
            Assert.Equal(1, repo.PurgarFinalizadasAntiguas(Entidades.BorradorGenerico.ModuloCompra, 0));

            Assert.Null(repo.ObtenerPorId(fin.Id));
            Assert.NotNull(repo.ObtenerPorId(finOtroModulo.Id));
            Assert.NotNull(repo.ObtenerPorId(activa.Id));
            Assert.NotNull(repo.ObtenerPorId(descartada.Id));
        }

        [SkippableFact]
        public void Eventos_se_listan_en_orden_con_datos_del_borrador()
        {
            var (repo, _) = Nuevo();
            var b = Borrador(); repo.Guardar(b);

            repo.AgregarEvento(new BorradorGenericoEvento { IdBorrador = b.Id, Tipo = BorradorGenericoEvento.TipoCierrePestana, IdUsuario = Operador });
            Thread.Sleep(20);
            repo.AgregarEvento(new BorradorGenericoEvento { IdBorrador = b.Id, Tipo = BorradorGenericoEvento.TipoDescartada, IdUsuario = OtroOperador, Detalle = "motivo de prueba" });

            var eventos = repo.ListarEventosPorBorrador(b.Id);
            Assert.Equal(new[] { "CIERRE_PESTANA", "DESCARTADA" }, eventos.Select(e => e.Tipo).ToArray());
            Assert.Equal("motivo de prueba", eventos[1].Detalle);
            Assert.Equal(OtroOperador, eventos[1].IdUsuario);
            Assert.Equal(Entidades.BorradorGenerico.ModuloCompra, eventos[0].Modulo);
            Assert.Equal(1, eventos[0].CantLineas);
            Assert.Null(eventos[0].Detalle);
        }

        [SkippableFact]
        public void Aislamiento_por_empresa_otra_empresa_no_ve_ni_modifica_borradores()
        {
            var (repo, _) = Nuevo();
            var b = Borrador(); repo.Guardar(b);

            var otraEmpresa = Backend.Generico(IdsDePrueba.NuevaEmpresa());
            Assert.Null(otraEmpresa.ObtenerPorId(b.Id));
            Assert.Null(otraEmpresa.ObtenerPorClientId(b.ClientId, b.Modulo));
            Assert.Empty(otraEmpresa.ListarActivasPorSucursal(Sucursal, b.Modulo));
            Assert.False(otraEmpresa.MarcarDescartada(b.Id));
            Assert.False(otraEmpresa.TomarBorrador(b.Id, OtroOperador, 1));
            Assert.Equal(Entidades.BorradorGenerico.EstadoActiva, repo.ObtenerPorId(b.Id).Estado);
        }

        [SkippableFact]
        public void Notificaciones_no_lanzan_excepcion()
        {
            // En Postgres crean filas; en SQL Server (etapa 2) son no-op: lo comun es que nunca fallen.
            var (repo, _) = Nuevo();
            var b = Borrador(); repo.Guardar(b); repo.EnvejecerLatidoPorCierre(b.Id);

            repo.UpsertNotificacion(new Notificacion
            {
                IdSucursal = Sucursal,
                Tipo = Notificacion.PrefijoBorradorGenericoDescartado + "COMPRA",
                RefId = b.Id,
                Titulo = "t",
                Mensaje = "m"
            }, reabrir: true);
            int creadas = repo.CrearNotificacionesInterrumpidas(Entidades.BorradorGenerico.ModuloCompra, 5);
            Assert.True(creadas >= 0);
        }
    }

    public sealed class BorradorGenericoSqlServerTests : BorradorGenericoContratoTests
    {
        protected override BackendRepos Backend { get; } = new BackendSqlServer();
    }

    public sealed class BorradorGenericoPostgresTests : BorradorGenericoContratoTests
    {
        protected override BackendRepos Backend { get; } = new BackendPostgres();
    }
}
