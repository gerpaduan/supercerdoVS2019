// Tests de la caducidad del operador de Egresos de Caja (WebCore.Helpers.PermisosHelper.LimpiarOperadorModuloSiSalioDeLaPantalla):
// el operador autorizado en una cuenta de produccion vive solo mientras se muestra Cajas/EgresosCaja; cualquier otra vista
// (incluso otra del mismo controller Cajas) lo borra. Ver docs/DECISIONS.md (2026-11-03).
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class OperadorEgresosCajaCaducidadTests
    {
        // ISession en memoria: SetString/GetString son extensiones sobre Set/TryGetValue.
        private sealed class SesionEnMemoria : ISession
        {
            private readonly Dictionary<string, byte[]> _valores = new Dictionary<string, byte[]>();

            public bool IsAvailable => true;
            public string Id => "sesion-de-prueba";
            public IEnumerable<string> Keys => _valores.Keys;
            public void Clear() => _valores.Clear();
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Remove(string key) => _valores.Remove(key);
            public void Set(string key, byte[] value) => _valores[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _valores.TryGetValue(key, out value!);
        }

        private static SesionEnMemoria SesionConOperador()
        {
            var sesion = new SesionEnMemoria();
            PermisosHelper.RegistrarOperadorModulo(sesion, PermisosHelper.ModuloOperadorEgresosCaja,
                new Entidades.Usuario { Id = 5, Nombre = "Operador de prueba", Activo = true });
            return sesion;
        }

        private static bool TieneOperador(ISession sesion) =>
            PermisosHelper.ObtenerOperadorModulo(sesion, PermisosHelper.ModuloOperadorEgresosCaja) != null;

        [Fact]
        public void SigueEnEgresosCaja_ConservaAlOperador()
        {
            var sesion = SesionConOperador();

            PermisosHelper.LimpiarOperadorModuloSiSalioDeLaPantalla(sesion, "Cajas", "EgresosCaja");

            Assert.True(TieneOperador(sesion));
        }

        [Fact]
        public void ElNombreDeControllerYAccionNoDistingueMayusculas()
        {
            var sesion = SesionConOperador();

            PermisosHelper.LimpiarOperadorModuloSiSalioDeLaPantalla(sesion, "cajas", "egresoscaja");

            Assert.True(TieneOperador(sesion));
        }

        [Theory]
        [InlineData("Cajas", "CajasAbiertas")]   // otra vista del mismo controller: tiene que caducar igual
        [InlineData("Cajas", "AutorizarModuloEgresosCaja")]
        [InlineData("Home", "Index")]
        [InlineData("Compras", "EgresosCaja")]   // misma accion, otro controller
        [InlineData("", "")]
        public void CualquierOtraVista_BorraAlOperador(string controller, string action)
        {
            var sesion = SesionConOperador();

            PermisosHelper.LimpiarOperadorModuloSiSalioDeLaPantalla(sesion, controller, action);

            Assert.False(TieneOperador(sesion));
        }

        [Fact]
        public void SinOperador_NoHaceNada()
        {
            var sesion = new SesionEnMemoria();

            PermisosHelper.LimpiarOperadorModuloSiSalioDeLaPantalla(sesion, "Home", "Index");

            Assert.False(TieneOperador(sesion));
        }

        [Fact]
        public void NoToca_ElOperadorDeOtrosModulos()
        {
            var sesion = SesionConOperador();
            PermisosHelper.RegistrarOperadorModulo(sesion, "Compras", new Entidades.Usuario { Id = 9, Nombre = "Otro", Activo = true });

            PermisosHelper.LimpiarOperadorModuloSiSalioDeLaPantalla(sesion, "Home", "Index");

            Assert.False(TieneOperador(sesion));
            Assert.NotNull(PermisosHelper.ObtenerOperadorModulo(sesion, "Compras"));
        }

        [Fact]
        public void LaReglaPorController_NoBorraAlOperadorDeEgresosCaja()
        {
            // La regla de siempre (por controller) no conoce este modulo: la caducidad es solo por pantalla.
            var sesion = SesionConOperador();

            PermisosHelper.LimpiarOperadorModuloSiSalioDelModulo(sesion, "Home");

            Assert.True(TieneOperador(sesion));
        }
    }
}
