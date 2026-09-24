using System;
using System.Linq;
using System.Threading;
using Contratos;
using Entidades;
using Xunit;

namespace Datos.BorradoresTests
{
    // Juego de pruebas del contrato IVentaBorradorRepository (POS), mismo criterio que
    // BorradorGenericoContratoTests: una sola suite contra SQL Server (Datos/VentaBorrador.cs) y Postgres
    // (DatosPostgres/VentaBorradorPg.cs). Solo se cubre lo que existe en ambos motores en esta etapa:
    // borradores y eventos. "Producto sin agregar" y notificaciones son etapa 2 en SQL Server.
    public abstract class VentaBorradorContratoTests
    {
        protected abstract BackendRepos Backend { get; }

        private const int Sucursal = 3;
        private const int Operador = 21;
        private const int OtroOperador = 22;

        private (IVentaBorradorRepository repo, int empresa) Nuevo()
        {
            string motivo = Backend.MotivoNoDisponible();
            Skip.If(motivo != null, motivo);
            int empresa = IdsDePrueba.NuevaEmpresa();
            return (Backend.Venta(empresa), empresa);
        }

        private static Entidades.VentaBorrador Venta(Guid? clientId = null, int operador = Operador, int sucursal = Sucursal,
            int cantLineas = 2, decimal total = 1234.56m, string payload = "{\"lineas\":[1,2]}")
        {
            return new Entidades.VentaBorrador
            {
                ClientId = clientId ?? Guid.NewGuid(),
                IdOperador = operador,
                IdUsuarioSesion = operador,
                IdSucursal = sucursal,
                PosInstanceId = "pos-1",
                IdCierreCaja = 9,
                IdPersona = 4,
                RazonSocial = "Cliente Ñandú S.A.",
                CantLineas = cantLineas,
                Total = total,
                Payload = payload
            };
        }

        [SkippableFact]
        public void Guardar_nuevo_persiste_campos_incluidos_acentos_y_decimales()
        {
            var (repo, empresa) = Nuevo();
            var v = Venta();

            Assert.Equal(ResultadoGuardarBorrador.Guardado, repo.Guardar(v));
            Assert.True(v.Id > 0);

            var leido = repo.ObtenerPorClientId(v.ClientId);
            Assert.NotNull(leido);
            Assert.Equal(empresa, leido.IdEmpresa);
            Assert.Equal("pos-1", leido.PosInstanceId);
            Assert.Equal(9, leido.IdCierreCaja);
            Assert.Equal(4, leido.IdPersona);
            Assert.Equal("Cliente Ñandú S.A.", leido.RazonSocial);
            Assert.Equal(2, leido.CantLineas);
            Assert.Equal(1234.56m, leido.Total);
            Assert.Equal(Entidades.VentaBorrador.EstadoActiva, leido.Estado);
            Assert.Contains("lineas", leido.Payload);
        }

        [SkippableFact]
        public void Guardar_actualiza_la_misma_fila_y_rechaza_ajeno_y_cerrado()
        {
            var (repo, _) = Nuevo();
            var v = Venta();
            repo.Guardar(v);

            var otra = Venta(v.ClientId, cantLineas: 5, total: 99m);
            Assert.Equal(ResultadoGuardarBorrador.Guardado, repo.Guardar(otra));
            Assert.Equal(v.Id, otra.Id);
            Assert.Equal(5, repo.ObtenerPorId(v.Id).CantLineas);

            Assert.Equal(ResultadoGuardarBorrador.Ajeno, repo.Guardar(Venta(v.ClientId, operador: OtroOperador)));
            Assert.Equal(ResultadoGuardarBorrador.Ajeno, repo.Guardar(Venta(v.ClientId, sucursal: Sucursal + 1)));

            Assert.True(repo.MarcarFinalizada(v.ClientId, 8000));
            Assert.Equal(ResultadoGuardarBorrador.YaCerrado, repo.Guardar(Venta(v.ClientId)));
            Assert.False(repo.MarcarFinalizada(v.ClientId, 8001));

            var cerrada = repo.ObtenerPorId(v.Id);
            Assert.Equal(Entidades.VentaBorrador.EstadoFinalizada, cerrada.Estado);
            Assert.Equal(8000, cerrada.IdVenta);
            Assert.True(string.IsNullOrEmpty(cerrada.Payload));
        }

        [SkippableFact]
        public void Latido_listados_toma_y_descarte()
        {
            var (repo, _) = Nuevo();
            var v = Venta(); repo.Guardar(v);
            var otraSucursal = Venta(sucursal: Sucursal + 1); repo.Guardar(otraSucursal);
            var ajena = Venta(operador: OtroOperador); repo.Guardar(ajena);

            Assert.True(repo.RegistrarLatido(v.ClientId, Operador, Sucursal));
            Assert.False(repo.RegistrarLatido(v.ClientId, OtroOperador, Sucursal));

            Assert.Equal(new[] { v.Id, ajena.Id }, repo.ListarActivasPorSucursal(Sucursal).Select(x => x.Id).ToArray());
            var propias = repo.ListarActivasPorOperador(Operador, Sucursal);
            Assert.Single(propias);
            Assert.True(string.IsNullOrEmpty(propias[0].Payload));
            Assert.Equal(2, repo.ListarActivasPorUsuarioSesion(Operador).Count);

            Assert.True(repo.TomarBorrador(ajena.Id, Operador, 77));
            Assert.Equal(Operador, repo.ObtenerPorId(ajena.Id).IdOperador);

            Assert.True(repo.MarcarDescartada(v.Id));
            Assert.False(repo.MarcarDescartada(v.Id));
            Assert.False(repo.TomarBorrador(v.Id, OtroOperador, 1));
        }

        [SkippableFact]
        public void Interrumpidas_por_rango_solo_las_ACTIVAS_sin_latido_reciente()
        {
            var (repo, _) = Nuevo();
            var v = Venta(); repo.Guardar(v);
            var reciente = Venta(); repo.Guardar(reciente);

            // Sin latido viejo no hay interrumpidas (umbral 5 min).
            var desde = DateTime.Now.AddDays(-2);
            var hasta = DateTime.Now.AddDays(1);
            Assert.Empty(repo.ListarInterrumpidasPorRango(desde, hasta, 5));

            // Con umbral 0 minutos entran las ACTIVAS cuyo ultimo latido es anterior a "ahora".
            Thread.Sleep(50);
            var todas = repo.ListarInterrumpidasPorRango(desde, hasta, 0);
            Assert.Equal(2, todas.Count);

            repo.MarcarDescartada(reciente.Id);
            Assert.Single(repo.ListarInterrumpidasPorRango(desde, hasta, 0));
        }

        [SkippableFact]
        public void Eventos_por_borrador_y_por_rango()
        {
            var (repo, _) = Nuevo();
            var v = Venta(); repo.Guardar(v);

            repo.AgregarEvento(new VentaBorradorEvento { IdBorrador = v.Id, Tipo = VentaBorradorEvento.TipoCierrePestana, IdUsuario = Operador });
            Thread.Sleep(20);
            repo.AgregarEvento(new VentaBorradorEvento { IdBorrador = v.Id, Tipo = VentaBorradorEvento.TipoDescartada, IdUsuario = OtroOperador, Detalle = "se cayo la luz" });

            var eventos = repo.ListarEventosPorBorrador(v.Id);
            Assert.Equal(new[] { "CIERRE_PESTANA", "DESCARTADA" }, eventos.Select(e => e.Tipo).ToArray());
            Assert.Equal("se cayo la luz", eventos[1].Detalle);
            Assert.Equal(1234.56m, eventos[0].Total);
            Assert.Equal(2, eventos[0].CantLineas);

            var porRango = repo.ListarEventosPorRango(DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1));
            Assert.Equal(2, porRango.Count);
            Assert.Equal("DESCARTADA", porRango[0].Tipo); // mas nuevos primero
        }

        [SkippableFact]
        public void Aislamiento_por_empresa_y_purga()
        {
            var (repo, _) = Nuevo();
            var v = Venta(); repo.Guardar(v);
            var fin = Venta(); repo.Guardar(fin); repo.MarcarFinalizada(fin.ClientId, 1);
            Thread.Sleep(50);

            var otraEmpresa = Backend.Venta(IdsDePrueba.NuevaEmpresa());
            Assert.Null(otraEmpresa.ObtenerPorId(v.Id));
            Assert.False(otraEmpresa.MarcarDescartada(v.Id));
            Assert.Equal(0, otraEmpresa.PurgarFinalizadasAntiguas(0));

            Assert.Equal(0, repo.PurgarFinalizadasAntiguas(30));
            Assert.Equal(1, repo.PurgarFinalizadasAntiguas(0));
            Assert.Null(repo.ObtenerPorId(fin.Id));
            Assert.NotNull(repo.ObtenerPorId(v.Id));
        }
    }

    public sealed class VentaBorradorSqlServerTests : VentaBorradorContratoTests
    {
        protected override BackendRepos Backend { get; } = new BackendSqlServer();
    }

    public sealed class VentaBorradorPostgresTests : VentaBorradorContratoTests
    {
        protected override BackendRepos Backend { get; } = new BackendPostgres();
    }
}
