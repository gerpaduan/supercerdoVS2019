using System;
using System.Collections.Generic;
using System.Linq;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Login por huella (2026-09-21, ver docs/DECISIONS.md "Login por huella (passkeys WebAuthn)"):
    // reglas de Negocio.UsuarioPasskey (nombre, tope por usuario, argumentos obligatorios) contra un
    // repositorio en memoria. La verificacion criptografica la hace Fido2 en WebCore, no se prueba aca.
    public class UsuarioPasskeyTests
    {
        private sealed class RepoEnMemoria : Contratos.IUsuarioPasskeyRepository
        {
            public readonly List<Entidades.UsuarioPasskey> Filas = new List<Entidades.UsuarioPasskey>();

            public List<Entidades.UsuarioPasskey> ListarPorUsuario(int idUsuario, int idEmpresa)
                => Filas.Where(p => p.IdUsuario == idUsuario && p.IdEmpresa == idEmpresa).ToList();

            public void Agregar(Entidades.UsuarioPasskey passkey) => Filas.Add(passkey);

            public bool Eliminar(int id, int idUsuario, int idEmpresa)
                => Filas.RemoveAll(p => p.Id == id && p.IdUsuario == idUsuario && p.IdEmpresa == idEmpresa) > 0;

            public Entidades.UsuarioPasskey ObtenerPorCredentialIdSinTenant(byte[] credentialId)
                => Filas.FirstOrDefault(p => p.CredentialId.SequenceEqual(credentialId));

            public void RegistrarUsoSinTenant(int id, long signCount, DateTime usoUtc) { }
        }

        private static readonly byte[] Handle = { 1, 2, 3, 4 };

        private static Entidades.UsuarioPasskey Registrar(UsuarioPasskey n, byte credencial, string nombre = "Notebook", int idUsuario = 7, int idEmpresa = 1)
            => n.Registrar(idUsuario, idEmpresa, new[] { credencial }, new byte[] { 9 }, 0, Handle, null, "internal", nombre);

        [Fact]
        public void Registrar_GuardaCredencialConLosDatosRecibidos()
        {
            var repo = new RepoEnMemoria();
            var passkey = Registrar(new UsuarioPasskey(repo), 1, "  Notebook de caja  ");

            Assert.Single(repo.Filas);
            Assert.Equal("Notebook de caja", passkey.Nombre); // se recorta
            Assert.Equal(7, passkey.IdUsuario);
            Assert.Equal(1, passkey.IdEmpresa);
            Assert.Equal(Handle, passkey.UserHandle);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Registrar_SinNombre_UsaNombrePorDefecto(string nombre)
        {
            var passkey = Registrar(new UsuarioPasskey(new RepoEnMemoria()), 1, nombre);
            Assert.Equal("Mi huella", passkey.Nombre);
        }

        [Fact]
        public void Registrar_NombreLargo_SeTrunca()
        {
            var passkey = Registrar(new UsuarioPasskey(new RepoEnMemoria()), 1, new string('x', 200));
            Assert.Equal(UsuarioPasskey.LargoMaximoNombre, passkey.Nombre.Length);
        }

        [Fact]
        public void Registrar_SuperandoElTope_Lanza()
        {
            var repo = new RepoEnMemoria();
            var n = new UsuarioPasskey(repo);
            for (byte i = 0; i < UsuarioPasskey.MaximoPorUsuario; i++)
                Registrar(n, i);

            Assert.Throws<InvalidOperationException>(() => Registrar(n, 200));
            Assert.Equal(UsuarioPasskey.MaximoPorUsuario, repo.Filas.Count);
        }

        [Fact]
        public void Registrar_ElTopeEsPorUsuario()
        {
            var repo = new RepoEnMemoria();
            var n = new UsuarioPasskey(repo);
            for (byte i = 0; i < UsuarioPasskey.MaximoPorUsuario; i++)
                Registrar(n, i, idUsuario: 7);

            // otro usuario (y otra empresa) no se ve afectado por el tope del primero
            Registrar(n, 100, idUsuario: 8);
            Registrar(n, 101, idUsuario: 7, idEmpresa: 2);
            Assert.Equal(UsuarioPasskey.MaximoPorUsuario + 2, repo.Filas.Count);
        }

        [Fact]
        public void Registrar_ArgumentosObligatoriosVacios_Lanza()
        {
            var n = new UsuarioPasskey(new RepoEnMemoria());
            Assert.Throws<ArgumentException>(() => n.Registrar(7, 1, null, new byte[] { 9 }, 0, Handle, null, "", "x"));
            Assert.Throws<ArgumentException>(() => n.Registrar(7, 1, new byte[] { 1 }, new byte[0], 0, Handle, null, "", "x"));
            Assert.Throws<ArgumentException>(() => n.Registrar(7, 1, new byte[] { 1 }, new byte[] { 9 }, 0, null, null, "", "x"));
        }

        [Fact]
        public void ObtenerUserHandleExistente_DevuelveElDeLaPrimeraOnullSiNoHay()
        {
            var repo = new RepoEnMemoria();
            var n = new UsuarioPasskey(repo);

            Assert.Null(n.ObtenerUserHandleExistente(7, 1));
            Registrar(n, 1);
            Assert.Equal(Handle, n.ObtenerUserHandleExistente(7, 1));
            Assert.Null(n.ObtenerUserHandleExistente(8, 1));
        }

        [Fact]
        public void Eliminar_SoloBorraSiPerteneceAlUsuario()
        {
            var repo = new RepoEnMemoria();
            var n = new UsuarioPasskey(repo);
            var passkey = Registrar(n, 1);
            passkey.Id = 50;

            Assert.False(n.Eliminar(50, idUsuario: 8, idEmpresa: 1)); // otro usuario: no borra
            Assert.Single(repo.Filas);
            Assert.True(n.Eliminar(50, idUsuario: 7, idEmpresa: 1));
            Assert.Empty(repo.Filas);
        }
    }
}
