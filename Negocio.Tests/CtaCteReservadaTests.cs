using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Tests unitarios (sin base de datos) de la capa de negocio de "cuenta corriente reservada"
    // (docs/DECISIONS.md, 2026-10-01): Negocio.Persona.idsRegistrosOcultos / idsPersonasReservadas.
    // El SQL de cada motor se verifico contra Postgres local (ver DECISIONS); aca se fija el contrato:
    // un usuario autorizado (restriccion null) nunca consulta ni oculta nada, y uno restringido
    // delega en el repositorio con su id y la hora de apertura de su caja.
    public class CtaCteReservadaTests
    {
        private static (Negocio.Persona persona, FakePersonaRepository repo) CrearSut()
        {
            var repo = new FakePersonaRepository();
            return (new Negocio.Persona(repo, new EmpresaContextFake(1)), repo);
        }

        [Fact]
        public void IdsRegistrosOcultos_SinRestriccion_NoOcultaNadaNiConsultaLaBase()
        {
            var (persona, repo) = CrearSut();
            repo.RegistrosOcultos = new HashSet<int> { 10, 11 };

            var ocultos = persona.idsRegistrosOcultos(RestriccionCtaCteReservada.TablaVentas, null);

            Assert.Empty(ocultos);
            Assert.Equal(0, repo.LlamadasIdsOcultos);
        }

        [Fact]
        public void IdsRegistrosOcultos_ConRestriccion_DelegaConTablaUsuarioYAperturaDeCaja()
        {
            var (persona, repo) = CrearSut();
            repo.RegistrosOcultos = new HashSet<int> { 10, 11 };
            var apertura = new DateTime(2026, 10, 1, 8, 30, 0);
            var restriccion = new RestriccionCtaCteReservada { IdUsuario = 22, Desde = apertura };

            var ocultos = persona.idsRegistrosOcultos(RestriccionCtaCteReservada.TablaPagos, restriccion);

            Assert.Equal(new HashSet<int> { 10, 11 }, ocultos);
            Assert.Equal(1, repo.LlamadasIdsOcultos);
            Assert.Equal(RestriccionCtaCteReservada.TablaPagos, repo.UltimaTabla);
            Assert.Equal(22, repo.UltimoIdUsuario);
            Assert.Equal(apertura, repo.UltimoDesde);
        }

        [Fact]
        public void IdsPersonasReservadas_DevuelveLasDelRepositorio()
        {
            var (persona, repo) = CrearSut();
            repo.PersonasReservadas = new HashSet<int> { 5, 7 };

            Assert.Equal(new HashSet<int> { 5, 7 }, persona.idsPersonasReservadas());
        }

        [Fact]
        public void SinCajaAbierta_EsUnaFechaQueNingunRegistroAlcanza()
        {
            // Sin caja abierta el usuario no ve nada propio: "creado >= Desde" nunca se cumple.
            Assert.True(RestriccionCtaCteReservada.SinCajaAbierta > DateTime.UtcNow.AddYears(1000));
            Assert.True(RestriccionCtaCteReservada.SinCajaAbierta <= new DateTime(9999, 12, 31));
        }
    }
}
