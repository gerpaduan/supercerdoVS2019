using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Tests unitarios (sin base de datos) de las reglas de "Eliminar persona" (docs/DECISIONS.md, 2026-11-02):
    // Negocio.Persona.motivoNoEliminable / eliminarPersonaSiCorresponde. El SQL de cada motor se verifica
    // aparte contra las bases locales; aca se fija que cada regla bloquea y que solo el caso limpio borra.
    public class EliminarPersonaTests
    {
        private const int IdEmpresaUsuario = 1;

        private static (Negocio.Persona negocio, FakePersonaRepository repo) CrearSut(int idConsumidorFinal = 0)
        {
            var repo = new FakePersonaRepository();
            var param = new FakeParametrosContext();
            if (idConsumidorFinal > 0) param.ConInt(ParamKeys.IdConsumidorFinal, idConsumidorFinal);
            return (new Negocio.Persona(repo, new EmpresaContextFake(IdEmpresaUsuario), param), repo);
        }

        private static Entidades.Persona PersonaDeLaEmpresa(int id = 50) =>
            new Entidades.Persona { idPersona = id, IdEmpresa = IdEmpresaUsuario };

        private static Entidades.Usuario Usuario(bool admin = true, int idEmpresa = IdEmpresaUsuario) =>
            new Entidades.Usuario { Id = 7, Admin = admin, IdEmpresa = idEmpresa };

        [Fact]
        public void SinRegistros_AdminDeLaMismaEmpresa_Elimina()
        {
            var (negocio, repo) = CrearSut();

            string motivo = negocio.eliminarPersonaSiCorresponde(PersonaDeLaEmpresa(), Usuario());

            Assert.Equal("", motivo);
            Assert.Equal(new[] { 50 }, repo.PersonasEliminadas);
        }

        [Fact]
        public void UsuarioNoAdmin_NoElimina()
        {
            var (negocio, repo) = CrearSut();

            string motivo = negocio.eliminarPersonaSiCorresponde(PersonaDeLaEmpresa(), Usuario(admin: false));

            Assert.Contains("administrador", motivo);
            Assert.Empty(repo.PersonasEliminadas);
        }

        [Fact]
        public void PersonaGlobal_NoElimina()
        {
            var (negocio, repo) = CrearSut();
            var global = new Entidades.Persona { idPersona = 50, IdEmpresa = 0 };

            string motivo = negocio.eliminarPersonaSiCorresponde(global, Usuario());

            Assert.Contains("empresa", motivo);
            Assert.Empty(repo.PersonasEliminadas);
        }

        [Fact]
        public void PersonaDeOtraEmpresa_NoElimina()
        {
            var (negocio, repo) = CrearSut();
            var ajena = new Entidades.Persona { idPersona = 50, IdEmpresa = 2 };

            string motivo = negocio.eliminarPersonaSiCorresponde(ajena, Usuario());

            Assert.Contains("empresa", motivo);
            Assert.Empty(repo.PersonasEliminadas);
        }

        [Fact]
        public void ConsumidorFinal_NoElimina()
        {
            var (negocio, repo) = CrearSut(idConsumidorFinal: 50);

            string motivo = negocio.eliminarPersonaSiCorresponde(PersonaDeLaEmpresa(50), Usuario());

            Assert.Contains("Consumidor Final", motivo);
            Assert.Empty(repo.PersonasEliminadas);
        }

        [Fact]
        public void Marca_NoElimina()
        {
            var (negocio, repo) = CrearSut();
            var marca = PersonaDeLaEmpresa();
            marca.Marca = true;

            string motivo = negocio.eliminarPersonaSiCorresponde(marca, Usuario());

            Assert.Contains("marca", motivo);
            Assert.Empty(repo.PersonasEliminadas);
        }

        [Theory]
        [InlineData("ventas")]
        [InlineData("compras")]
        [InlineData("pagos")]
        [InlineData("movimientos de cuenta corriente")]
        [InlineData("registros eliminados (auditoria)")]
        public void ConRegistros_NoEliminaYDiceCuales(string tipo)
        {
            var (negocio, repo) = CrearSut();
            repo.RegistrosAsociadosPorTipo[tipo] = 3;
            repo.RegistrosAsociadosPorTipo["expendios"] = 1;

            string motivo = negocio.eliminarPersonaSiCorresponde(PersonaDeLaEmpresa(), Usuario());

            Assert.Contains(tipo + " (3)", motivo);
            Assert.Contains("expendios (1)", motivo);
            Assert.Empty(repo.PersonasEliminadas);
        }

        [Fact]
        public void PersonaInexistente_NoElimina()
        {
            var (negocio, repo) = CrearSut();

            Assert.Equal("No se encontró la persona.", negocio.eliminarPersonaSiCorresponde(null, Usuario()));
            Assert.Empty(repo.PersonasEliminadas);
        }
    }
}
