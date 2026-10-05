using System;
using System.Collections.Generic;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Formula secreta con re-login (2026-10-04, ver docs/DECISIONS.md "Formula secreta con re-login"):
    //  - estado de elevaciones de una sesion (Negocio.FormulaSecretaElevaciones): vence, se revoca, ignora basura;
    //  - quien puede ver una formula secreta (Negocio.Usuario.PuedeVerFormulaSecreta).
    // Sin base de datos ni HttpContext: logica pura / repositorio en memoria.
    public class FormulaSecretaElevacionesTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc);
        private static readonly TimeSpan CincoMinutos = TimeSpan.FromMinutes(5);

        [Fact]
        public void SinEstado_NingunProductoTieneElevacion()
        {
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente("", 7, Ahora));
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(null, 7, Ahora));
        }

        [Fact]
        public void Registrar_HabilitaSoloEseProducto()
        {
            string estado = Negocio.FormulaSecretaElevaciones.Registrar("", 7, 42, Ahora, CincoMinutos);

            Assert.True(Negocio.FormulaSecretaElevaciones.Vigente(estado, 7, Ahora));
            // Otro producto elaborado NO queda habilitado por la elevacion de este.
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, 8, Ahora));
        }

        [Fact]
        public void LaElevacionVenceAlCumplirseLaDuracion()
        {
            string estado = Negocio.FormulaSecretaElevaciones.Registrar("", 7, 42, Ahora, CincoMinutos);

            Assert.True(Negocio.FormulaSecretaElevaciones.Vigente(estado, 7, Ahora.AddMinutes(4).AddSeconds(59)));
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, 7, Ahora.AddMinutes(5)));
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, 7, Ahora.AddHours(1)));
        }

        [Fact]
        public void Registrar_DescartaLasVencidasYRenuevaLaDelMismoProducto()
        {
            string viejo = Negocio.FormulaSecretaElevaciones.Registrar("", 7, 42, Ahora, CincoMinutos);
            DateTime despues = Ahora.AddMinutes(10);

            // Pasados 10 minutos la de 7 ya vencio: registrar la de 8 la descarta del estado.
            string estado = Negocio.FormulaSecretaElevaciones.Registrar(viejo, 8, 43, despues, CincoMinutos);

            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, 7, despues));
            Assert.True(Negocio.FormulaSecretaElevaciones.Vigente(estado, 8, despues));
            Assert.Equal(new List<int> { 8 }, new List<int>(Negocio.FormulaSecretaElevaciones.CortesVigentes(estado, despues)));
        }

        [Fact]
        public void Revocar_QuitaSoloEseProductoYInformaSiExistia()
        {
            string estado = Negocio.FormulaSecretaElevaciones.Registrar("", 7, 42, Ahora, CincoMinutos);
            estado = Negocio.FormulaSecretaElevaciones.Registrar(estado, 8, 42, Ahora, CincoMinutos);

            string tras = Negocio.FormulaSecretaElevaciones.Revocar(estado, 7, Ahora, out bool existia);
            Assert.True(existia);
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(tras, 7, Ahora));
            Assert.True(Negocio.FormulaSecretaElevaciones.Vigente(tras, 8, Ahora));

            // Revocar algo que ya no esta vigente no "existia": el controller no audita un OCULTAR falso.
            Negocio.FormulaSecretaElevaciones.Revocar(tras, 7, Ahora, out bool segundaVez);
            Assert.False(segundaVez);
        }

        [Fact]
        public void RevocarTodas_VaciaElEstadoYCuentaLasVigentes()
        {
            string estado = Negocio.FormulaSecretaElevaciones.Registrar("", 7, 42, Ahora, CincoMinutos);
            estado = Negocio.FormulaSecretaElevaciones.Registrar(estado, 8, 42, Ahora, CincoMinutos);

            string tras = Negocio.FormulaSecretaElevaciones.RevocarTodas(estado, Ahora, out int cantidad);

            Assert.Equal(2, cantidad);
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(tras, 7, Ahora));
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(tras, 8, Ahora));
        }

        [Theory]
        [InlineData("basura")]
        [InlineData("7")]
        [InlineData("7,42")]
        [InlineData("a,b,c")]
        [InlineData("7,42,no-es-un-numero")]
        [InlineData("0,42,999999999999999999")]
        [InlineData("-3,42,999999999999999999")]
        [InlineData(";;;")]
        public void UnEstadoCorruptoONoValidoNuncaOtorgaElevacion(string estado)
        {
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, 7, Ahora));
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, 0, Ahora));
            Assert.False(Negocio.FormulaSecretaElevaciones.Vigente(estado, -3, Ahora));
        }

        [Fact]
        public void Registrar_ConProductoInvalido_Lanza()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Negocio.FormulaSecretaElevaciones.Registrar("", 0, 42, Ahora, CincoMinutos));
            Assert.Throws<ArgumentOutOfRangeException>(() => Negocio.FormulaSecretaElevaciones.Registrar("", -1, 42, Ahora, CincoMinutos));
        }

        [Fact]
        public void ElEstadoNoGuardaNadaDelUsuarioMasQueSuId()
        {
            string estado = Negocio.FormulaSecretaElevaciones.Registrar("", 7, 42, Ahora, CincoMinutos);

            // Formato "idCorte,idUsuario,ticks": solo numeros y separadores (ni nombre, ni hash, ni clave).
            Assert.Matches(@"^\d+,\d+,\d+$", estado);
        }
    }

    public class FormulaSecretaPermisoTests
    {
        private static Negocio.Usuario CrearSut()
        {
            return new Negocio.Usuario(new FakeUsuarioRepository(), new EmpresaContextFake(1));
        }

        private static Entidades.Usuario NuevoUsuario()
        {
            return new Entidades.Usuario { Id = 26, User = "ana", Activo = true, IdEmpresa = 1 };
        }

        // Permiso de CONSULTA de una pantalla (FormConsulta) o de EDICION (FormEdicion), sin limite de dias.
        private static Entidades.PermisosUsuarios Permiso(string formConsulta, string formEdicion)
        {
            return new Entidades.PermisosUsuarios
            {
                IdUsuario = 26,
                DiasPermitidosVer = 0,
                DiasPermitidosEditar = 0,
                Formulario = new Entidades.Formulario
                {
                    FormConsulta = formConsulta ?? "",
                    FormEdicion = formEdicion ?? "",
                    FormEdicionExtra1 = "",
                    FormEdicionExtra2 = ""
                }
            };
        }

        [Fact]
        public void Null_NoPuede()
        {
            Assert.False(CrearSut().PuedeVerFormulaSecreta(null));
        }

        [Fact]
        public void AdminActivo_Puede()
        {
            var admin = NuevoUsuario();
            admin.Admin = true;

            Assert.True(CrearSut().PuedeVerFormulaSecreta(admin));
        }

        [Fact]
        public void UsuarioSinPermisos_NoPuede()
        {
            Assert.False(CrearSut().PuedeVerFormulaSecreta(NuevoUsuario()));
        }

        [Fact]
        public void PermisoDeVerFormulas_Puede()
        {
            var usuario = NuevoUsuario();
            usuario.Permisos.Add(Permiso(Entidades.Permisos.Elaborado.VerFormulas, null));

            Assert.True(CrearSut().PuedeVerFormulaSecreta(usuario));
        }

        [Fact]
        public void PermisoDeIngresarFormulas_Puede()
        {
            var usuario = NuevoUsuario();
            usuario.Permisos.Add(Permiso(null, Entidades.Permisos.Elaborado.IngresoFormula));

            Assert.True(CrearSut().PuedeVerFormulaSecreta(usuario));
        }

        [Fact]
        public void PermisoSobreOtraPantalla_NoPuede()
        {
            // Tener permiso de ver/ingresar ELABORADOS no alcanza: tiene que ser sobre FORMULAS.
            var usuario = NuevoUsuario();
            usuario.Permisos.Add(Permiso(Entidades.Permisos.Elaborado.VerEmbutidos, Entidades.Permisos.Elaborado.IngresoEmbutido));

            Assert.False(CrearSut().PuedeVerFormulaSecreta(usuario));
        }

        [Fact]
        public void ConPermisoPeroInactivo_NoPuede()
        {
            var usuario = NuevoUsuario();
            usuario.Admin = true;
            usuario.Activo = false;

            Assert.False(CrearSut().PuedeVerFormulaSecreta(usuario));
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void ConPermisoPeroCuentaBloqueada_NoPuede(bool bloqueado, bool bloqueadoNoSeguro)
        {
            var usuario = NuevoUsuario();
            usuario.Admin = true;
            usuario.Bloqueado = bloqueado;
            usuario.BloqueadoNoSeguro = bloqueadoNoSeguro;

            Assert.False(CrearSut().PuedeVerFormulaSecreta(usuario));
        }
    }
}
