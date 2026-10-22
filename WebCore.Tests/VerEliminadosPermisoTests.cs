// Tests de quien puede ver los registros eliminados (WebCore.Helpers.EliminacionRegistroWeb.PuedeVerEliminados): el admin ve los tres
// modulos; quien puede MODIFICAR un modulo ve los eliminados de ese modulo; la cuenta de produccion no. Ver docs/DECISIONS.md (2026-10-22).
using System.Collections.Generic;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class VerEliminadosPermisoTests
    {
        private sealed class Empresa : Utilidades.IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private static Negocio.Usuario CrearUsuarioN() => new Negocio.Usuario(new Empresa());

        private static Entidades.PermisosUsuarios Permiso(string formEdicion, string extra1 = "")
        {
            return new Entidades.PermisosUsuarios
            {
                DiasPermitidosEditar = 30,
                Formulario = new Entidades.Formulario { FormEdicion = formEdicion, FormEdicionExtra1 = extra1, FormEdicionExtra2 = "", FormConsulta = "" }
            };
        }

        private static Entidades.Usuario Usuario(params Entidades.PermisosUsuarios[] permisos) =>
            new Entidades.Usuario { Id = 7, Activo = true, Permisos = new List<Entidades.PermisosUsuarios>(permisos) };

        [Fact]
        public void Admin_VeLosTres()
        {
            var admin = new Entidades.Usuario { Id = 1, Admin = true, Permisos = new List<Entidades.PermisosUsuarios>() };

            Assert.Equal(3, EliminacionRegistroWeb.ModulosConEliminadosVisibles(admin, CrearUsuarioN()).Count);
        }

        [Fact]
        public void QuienPuedeModificarSoloCompras_VeSoloCompras()
        {
            // En la base, "formNuevaCompra" es el permiso de edicion del formulario y "formModificarCompra" un extra.
            var usuario = Usuario(Permiso("formNuevaCompra", "formModificarCompra"));

            var modulos = EliminacionRegistroWeb.ModulosConEliminadosVisibles(usuario, CrearUsuarioN());

            Assert.Equal(new[] { Entidades.AuditoriaEliminacion.ModuloCompra }, modulos);
        }

        [Fact]
        public void QuienPuedeModificarStockYMovimientos_VeEsosDos()
        {
            var usuario = Usuario(Permiso("formAddOrEditStock"), Permiso("formNuevoMovimiento"));

            var modulos = EliminacionRegistroWeb.ModulosConEliminadosVisibles(usuario, CrearUsuarioN());

            Assert.Equal(new[] { Entidades.AuditoriaEliminacion.ModuloStock, Entidades.AuditoriaEliminacion.ModuloMovimiento }, modulos);
        }

        [Fact]
        public void SinPermisosDeModificar_NoVeNada()
        {
            var soloVer = new Entidades.PermisosUsuarios
            {
                Formulario = new Entidades.Formulario { FormConsulta = "formCompras", FormEdicion = "", FormEdicionExtra1 = "", FormEdicionExtra2 = "" }
            };

            Assert.Empty(EliminacionRegistroWeb.ModulosConEliminadosVisibles(Usuario(soloVer), CrearUsuarioN()));
            Assert.Empty(EliminacionRegistroWeb.ModulosConEliminadosVisibles(Usuario(), CrearUsuarioN()));
            Assert.Empty(EliminacionRegistroWeb.ModulosConEliminadosVisibles(null, CrearUsuarioN()));
        }

        [Fact]
        public void CuentaDeProduccion_NoVeNadaAunqueTengaPermisos()
        {
            var produccion = Usuario(Permiso("formAddOrEditStock"));
            produccion.EsUsuarioProduccion = true;

            Assert.Empty(EliminacionRegistroWeb.ModulosConEliminadosVisibles(produccion, CrearUsuarioN()));
        }
    }
}
