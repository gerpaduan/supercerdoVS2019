using System;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Estado de las autorizaciones provisorias del POS (2026-11-03). Ver docs/DECISIONS.md.
    public class AutorizacionesProvisoriasPosTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 11, 3, 10, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan Duracion = TimeSpan.FromMinutes(30);
        private const string Pos = "abc123";

        private static string Registrar(string estado, string accion = AutorizacionesProvisoriasPos.AccionModificarVenta,
            int idRegistro = 77, int autorizador = 5, int auditoria = 9, string pos = Pos) =>
            AutorizacionesProvisoriasPos.Registrar(estado, pos, accion, idRegistro, autorizador, auditoria, Ahora, Duracion);

        [Fact]
        public void Registrada_QuedaVigenteConQuienAutorizo()
        {
            var estado = Registrar("");
            var v = AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora.AddMinutes(1));
            Assert.NotNull(v);
            Assert.Equal(5, v.IdAutorizador);
            Assert.Equal(9, v.IdAuditoria);
        }

        [Fact]
        public void SinRegistrar_NoHayAutorizacion() =>
            Assert.Null(AutorizacionesProvisoriasPos.Vigente("", Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora));

        [Fact]
        public void Vencida_YaNoVale()
        {
            var estado = Registrar("");
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora.Add(Duracion).AddSeconds(1)));
        }

        [Fact]
        public void OtraVenta_NoVale() =>
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(Registrar(""), Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 78, Ahora));

        [Fact]
        public void OtraAccion_NoVale() =>
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(Registrar(""), Pos, AutorizacionesProvisoriasPos.AccionBonificar, 77, Ahora));

        [Fact]
        public void OtraPestana_NoVale() =>
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(Registrar(""), "otra", AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora));

        [Fact]
        public void RegistrarDeNuevo_RenuevaYNoDuplica()
        {
            var estado = Registrar("", autorizador: 5, auditoria: 9);
            estado = Registrar(estado, autorizador: 6, auditoria: 10);
            var v = AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora);
            Assert.Equal(6, v.IdAutorizador);
            Assert.DoesNotContain(";", estado);
        }

        [Fact]
        public void Revocar_UnaAccionYRegistro_QuitaSoloEsaYDevuelveSuAuditoria()
        {
            var estado = Registrar("", idRegistro: 77, auditoria: 9);
            estado = Registrar(estado, accion: AutorizacionesProvisoriasPos.AccionBonificar, idRegistro: 0, auditoria: 10);

            estado = AutorizacionesProvisoriasPos.Revocar(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora, out var ids);

            Assert.Equal(new[] { 9 }, ids);
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora));
            Assert.NotNull(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionBonificar, 0, Ahora));
        }

        [Fact]
        public void Revocar_PorPestana_QuitaTodasLasDeEsaPestanaYNoLasDeOtra()
        {
            var estado = Registrar("", idRegistro: 77, auditoria: 9);
            estado = Registrar(estado, accion: AutorizacionesProvisoriasPos.AccionCtaCte, idRegistro: 0, auditoria: 10);
            estado = Registrar(estado, pos: "otra", auditoria: 11);

            estado = AutorizacionesProvisoriasPos.Revocar(estado, Pos, null, null, Ahora, out var ids);

            Assert.Equal(2, ids.Count);
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionCtaCte, 0, Ahora));
            Assert.NotNull(AutorizacionesProvisoriasPos.Vigente(estado, "otra", AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora));
        }

        [Fact]
        public void Revocar_AccionSinRegistroConcreto_QuitaTodosLosRegistrosDeEsaAccion()
        {
            var estado = Registrar("", idRegistro: 77, auditoria: 9);
            estado = Registrar(estado, idRegistro: 78, auditoria: 10);
            estado = AutorizacionesProvisoriasPos.Revocar(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, null, Ahora, out var ids);
            Assert.Equal(2, ids.Count);
            Assert.Equal("", estado);
        }

        [Fact]
        public void ExtraerVencidas_SeparaLasVencidasParaCerrarSuAuditoria()
        {
            var estado = Registrar("", idRegistro: 77, auditoria: 9);
            var despues = Ahora.Add(Duracion).AddMinutes(1);

            // El llamador (AutorizacionProvisoriaPosHelper) extrae las vencidas ANTES de registrar la nueva.
            estado = AutorizacionesProvisoriasPos.ExtraerVencidas(estado, despues, out var vencidas);
            estado = AutorizacionesProvisoriasPos.Registrar(estado, Pos, AutorizacionesProvisoriasPos.AccionCtaCte, 0, 5, 10, despues, Duracion);

            Assert.Equal(new[] { 9 }, vencidas);
            Assert.NotNull(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionCtaCte, 0, despues));
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, despues));
        }

        [Theory]
        [InlineData("basura")]
        [InlineData("abc123,ModificarVenta,77,5,9")]
        [InlineData("abc123,Inventada,77,5,9,999999999999999999")]
        [InlineData("abc123,ModificarVenta,x,5,9,999999999999999999")]
        [InlineData("abc123,ModificarVenta,77,0,9,999999999999999999")]
        public void EstadoManipulado_NuncaDaUnaAutorizacion(string estado) =>
            Assert.Null(AutorizacionesProvisoriasPos.Vigente(estado, Pos, AutorizacionesProvisoriasPos.AccionModificarVenta, 77, Ahora));

        [Fact]
        public void PestanaConSeparadores_SeRechaza() =>
            Assert.Throws<ArgumentException>(() => Registrar("", pos: "a,b"));

        [Fact]
        public void AccionInventada_SeRechaza() =>
            Assert.Throws<ArgumentException>(() => Registrar("", accion: "Cualquiera"));
    }
}
