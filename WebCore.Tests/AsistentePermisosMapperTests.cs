// Tests del asistente de usuarios y permisos (docs/DECISIONS.md, 2026-11-10): la traduccion entre la fila guardada de
// permisosusuarios y el estado del asistente (AsistentePermisosMapper), el catalogo de permisos (CatalogoPermisos), los
// perfiles, el calculo de "entra al POS" y la ventana de dias entre dos permisos (PermisosHelper.FechaMinimaMasAmplia).
// Regla central que se prueba: guardar sin tocar nada no cambia ningun valor guardado.
using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class AsistentePermisosMapperTests
    {
        private static PermisosUsuarios Fila(int idForm, int ver, int editar, bool propios = true) => new PermisosUsuarios
        {
            IdUsuario = 5,
            IdForm = idForm,
            DiasPermitidosVer = ver,
            DiasPermitidosEditar = editar,
            SoloRegistrosPropios = propios
        };

        private static void MismaFila(PermisosUsuarios esperada, PermisosUsuarios real, string contexto)
        {
            Assert.True(esperada.DiasPermitidosVer == real.DiasPermitidosVer
                        && esperada.DiasPermitidosEditar == real.DiasPermitidosEditar
                        && esperada.SoloRegistrosPropios == real.SoloRegistrosPropios,
                $"{contexto}: esperada ({esperada.DiasPermitidosVer},{esperada.DiasPermitidosEditar},{esperada.SoloRegistrosPropios}) " +
                $"pero quedo ({real.DiasPermitidosVer},{real.DiasPermitidosEditar},{real.SoloRegistrosPropios})");
        }

        private static DefinicionPermiso Definicion(int idForm) => CatalogoPermisos.Para(new Formulario { IdForm = idForm });

        // Filas "canonicas" (las que el propio asistente produce) de cada tipo: ida y vuelta sin cambios.
        private static IEnumerable<PermisosUsuarios> FilasCanonicas(DefinicionPermiso def)
        {
            yield return Fila(def.IdForm, -1, -1, true);
            switch (def.Tipo)
            {
                case TipoPermiso.VerYModificar:
                    yield return Fila(def.IdForm, 30, -1, true);
                    yield return Fila(def.IdForm, 0, -1, true);
                    yield return Fila(def.IdForm, 30, 7, true);
                    yield return Fila(def.IdForm, 3650, 0, false);
                    break;
                case TipoPermiso.SoloModificar:
                    yield return Fila(def.IdForm, -1, 0, true);
                    yield return Fila(def.IdForm, 0, 10, true);
                    break;
                default:
                    if (def.EspejaVerEnModificar)
                    {
                        yield return Fila(def.IdForm, 0, 0, true);
                        yield return Fila(def.IdForm, 30, 30, true);
                    }
                    else
                    {
                        yield return Fila(def.IdForm, 0, -1, true);
                        yield return Fila(def.IdForm, 30, -1, true);
                    }
                    break;
            }
        }

        [Fact]
        public void GuardarSinTocarNada_NoCambiaNingunValor_EnTodoElCatalogo()
        {
            foreach (var def in CatalogoPermisos.Todas.Where(d => string.IsNullOrEmpty(d.SinEfectoHoy)))
            {
                foreach (var fila in FilasCanonicas(def))
                {
                    var estado = AsistentePermisosMapper.EstadoDesde(fila, def);
                    Assert.False(estado.Personalizado, $"idform {def.IdForm}: una fila canonica no debe ser 'personalizada'");
                    MismaFila(fila, AsistentePermisosMapper.Aplicar(5, estado, def, fila), $"idform {def.IdForm}");
                }
            }
        }

        [Fact]
        public void FilaSinNingunPermiso_ArrancaSinAcceso_ConLosDiasPorDefecto()
        {
            var def = Definicion(IdFormPermiso.Compras);
            var estado = AsistentePermisosMapper.EstadoDesde(null, def);

            Assert.Equal(0, estado.Nivel);
            Assert.Equal(CatalogoPermisos.DiasVerPorDefecto, estado.DiasVer);
            Assert.Equal(CatalogoPermisos.DiasEditarPorDefecto, estado.DiasEditar);
            Assert.True(estado.SoloPropios);
            Assert.False(estado.Personalizado);
        }

        [Fact]
        public void EditarSinVer_QuedaPersonalizado_YSeConservaTalCual()
        {
            var def = Definicion(IdFormPermiso.Compras);
            var guardada = Fila(def.IdForm, -1, 5, false);

            var estado = AsistentePermisosMapper.EstadoDesde(guardada, def);
            Assert.True(estado.Personalizado);

            MismaFila(guardada, AsistentePermisosMapper.Aplicar(5, estado, def, guardada), "personalizado");
        }

        [Fact]
        public void Personalizado_SePuedeReemplazar_ElegidoUnNivelNuevo()
        {
            var def = Definicion(IdFormPermiso.Compras);
            var guardada = Fila(def.IdForm, -1, 5, false);
            var estado = AsistentePermisosMapper.EstadoDesde(guardada, def);

            // La pantalla lo "reemplaza": personalizado = false y un nivel elegido.
            estado.Personalizado = false;
            estado.Nivel = 1;
            estado.DiasVer = 15;

            MismaFila(Fila(def.IdForm, 15, -1, true), AsistentePermisosMapper.Aplicar(5, estado, def, guardada), "reemplazado");
        }

        [Fact]
        public void Niveles_VerYModificar_EscribenLoEsperado()
        {
            var def = Definicion(IdFormPermiso.Compras); // dias ver, dias editar y alcance
            var estado = new EstadoPermisoAsistente { IdForm = def.IdForm, Nivel = 2, DiasVer = 30, DiasEditar = 7, SoloPropios = false };

            MismaFila(Fila(def.IdForm, 30, 7, false), AsistentePermisosMapper.Aplicar(5, estado, def, null), "ver y modificar");

            estado.Nivel = 1;
            MismaFila(Fila(def.IdForm, 30, -1, true), AsistentePermisosMapper.Aplicar(5, estado, def, null), "solo ver: editar -1 y alcance Propios");

            estado.Nivel = 0;
            MismaFila(Fila(def.IdForm, -1, -1, true), AsistentePermisosMapper.Aplicar(5, estado, def, null), "sin acceso");
        }

        [Fact]
        public void TodoElHistorial_SeGuardaComo3650Dias()
        {
            var def = Definicion(IdFormPermiso.Compras);
            var estado = new EstadoPermisoAsistente { IdForm = def.IdForm, Nivel = 2, DiasVer = 5, TodoVer = true, DiasEditar = 2, TodoEditar = true };

            var fila = AsistentePermisosMapper.Aplicar(5, estado, def, null);

            Assert.Equal(3650, AsistentePermisosMapper.DiasTodoElHistorial);
            Assert.Equal(3650, fila.DiasPermitidosVer);
            Assert.Equal(3650, fila.DiasPermitidosEditar);
        }

        [Fact]
        public void TodoElHistorial_NoReduceUnValorMasGrandeYaGuardado()
        {
            var def = Definicion(IdFormPermiso.Compras);
            var guardada = Fila(def.IdForm, 99999, -1, true);
            var estado = AsistentePermisosMapper.EstadoDesde(guardada, def);

            Assert.True(estado.TodoVer);
            MismaFila(guardada, AsistentePermisosMapper.Aplicar(5, estado, def, guardada), "valor legado grande");
        }

        [Fact]
        public void DiasAbsurdos_SeAcotan_YLosNegativosVanACero()
        {
            var def = Definicion(IdFormPermiso.Compras);
            var estado = new EstadoPermisoAsistente { IdForm = def.IdForm, Nivel = 2, DiasVer = int.MaxValue, DiasEditar = -50 };

            var fila = AsistentePermisosMapper.Aplicar(5, estado, def, null);

            Assert.Equal(AsistentePermisosMapper.DiasMaximos, fila.DiasPermitidosVer);
            Assert.Equal(0, fila.DiasPermitidosEditar);
        }

        [Fact]
        public void ControlesOcultos_ConservanLoGuardado_YUnaAltaNuevaVaA0()
        {
            // Cheques: el codigo usa DateTime.Today (los dias no importan) -> el catalogo no los muestra.
            var def = Definicion(IdFormPermiso.Cheques);
            Assert.False(def.UsaDiasVer);
            Assert.False(def.UsaDiasEditar);

            var estado = new EstadoPermisoAsistente { IdForm = def.IdForm, Nivel = 2, DiasVer = 99, DiasEditar = 99, SoloPropios = true };

            // Alta nueva: ni los 99 tipeados (oculto) ni -1: queda en 0 = hoy.
            MismaFila(Fila(def.IdForm, 0, 0, true), AsistentePermisosMapper.Aplicar(5, estado, def, Fila(def.IdForm, -1, -1)), "alta nueva");

            // Ya tenia 45 / 12: se conservan.
            MismaFila(Fila(def.IdForm, 45, 12, true), AsistentePermisosMapper.Aplicar(5, estado, def, Fila(def.IdForm, 45, 12)), "valores previos");
        }

        [Fact]
        public void CierresDeCaja_VerArrastraModificar_ConLosMismosDias()
        {
            var def = Definicion(IdFormPermiso.CierresDeCaja);
            var estado = new EstadoPermisoAsistente { IdForm = def.IdForm, Nivel = 1 };

            var fila = AsistentePermisosMapper.Aplicar(5, estado, def, Fila(def.IdForm, -1, -1));

            Assert.Equal(fila.DiasPermitidosVer, fila.DiasPermitidosEditar);
            Assert.True(fila.DiasPermitidosEditar >= 0);
        }

        [Fact]
        public void SoloVer_ConEditarYaOtorgado_LoConserva()
        {
            // Cerrar Caja con las dos casillas tildadas (la grilla vieja lo permitia): el asistente no lo rompe.
            var def = Definicion(IdFormPermiso.CerrarCaja);
            var guardada = Fila(def.IdForm, 0, 0, true);

            var estado = AsistentePermisosMapper.EstadoDesde(guardada, def);

            Assert.Equal(1, estado.Nivel);
            MismaFila(guardada, AsistentePermisosMapper.Aplicar(5, estado, def, guardada), "ver + editar tildados");
        }

        [Fact]
        public void NivelFueraDeRango_SeAcotaAlMaximoDelTipo()
        {
            var def = Definicion(IdFormPermiso.PosBonificacion); // un solo nivel
            var estado = new EstadoPermisoAsistente { IdForm = def.IdForm, Nivel = 2 };

            var fila = AsistentePermisosMapper.Aplicar(5, estado, def, null);

            Assert.Equal(0, fila.DiasPermitidosVer);
            Assert.Equal(-1, fila.DiasPermitidosEditar);
        }

        // ---------- Catalogo ----------

        [Fact]
        public void Catalogo_TodosLosIdFormConocidosTienenDefinicion()
        {
            var ids = typeof(IdFormPermiso).GetFields().Select(f => (int)f.GetRawConstantValue()!).ToList();
            Assert.NotEmpty(ids);
            foreach (int id in ids)
                Assert.True(CatalogoPermisos.TieneDefinicion(id), $"falta la definicion del idform {id}");
        }

        [Fact]
        public void Catalogo_LasFilasVisiblesTienenTituloYTexto()
        {
            foreach (var def in CatalogoPermisos.Todas.Where(d => string.IsNullOrEmpty(d.SinEfectoHoy)))
            {
                Assert.False(string.IsNullOrWhiteSpace(def.Titulo), $"idform {def.IdForm} sin titulo");
                Assert.False(string.IsNullOrWhiteSpace(def.TextoVer), $"idform {def.IdForm} sin texto");
                if (def.Tipo == TipoPermiso.VerYModificar)
                    Assert.False(string.IsNullOrWhiteSpace(def.TextoModificar), $"idform {def.IdForm} sin texto de modificar");
                Assert.Contains(def.Modulo, CatalogoPermisos.OrdenModulos);
            }
        }

        [Fact]
        public void Catalogo_UnIdFormDesconocido_CaeEnOtrosConTodosLosControles()
        {
            var def = CatalogoPermisos.Para(new Formulario { IdForm = 777, NombreForm = "Algo nuevo", Descripcion = "d" });

            Assert.Equal(CatalogoPermisos.ModuloOtros, def.Modulo);
            Assert.Equal("Algo nuevo", def.Titulo);
            Assert.Equal(TipoPermiso.VerYModificar, def.Tipo);
            Assert.True(def.UsaDiasVer && def.UsaDiasEditar && def.UsaAlcance);
            Assert.True(string.IsNullOrEmpty(def.SinEfectoHoy));
        }

        [Fact]
        public void Catalogo_LasFilasSinEfecto_SonLasQueNingunaPantallaControla()
        {
            var sinEfecto = CatalogoPermisos.Todas.Where(d => !string.IsNullOrEmpty(d.SinEfectoHoy)).Select(d => d.IdForm).OrderBy(i => i).ToArray();

            // Costo por tipo de cobro, Exportar lista de precios e Imprimir lista en ticket: pantallas que solo existen en el Web clasico.
            Assert.Equal(new[] { 15, 22, 24 }, sinEfecto);
        }

        [Fact]
        public void Produccion_BloqueaCuentasCorrientesYReservadas()
        {
            Assert.True(CatalogoPermisos.EsFormularioBloqueadoParaProduccion(new Formulario { FormConsulta = Permisos.Finanza.VerCtasCtes }));
            Assert.True(CatalogoPermisos.EsFormularioBloqueadoParaProduccion(new Formulario { FormConsulta = Permisos.Finanza.VerCtaCteReservada, FormEdicion = Permisos.Finanza.VerCtaCteReservada }));
            Assert.True(CatalogoPermisos.EsFormularioBloqueadoParaProduccion(new Formulario { FormConsulta = "x", FormEdicion = Permisos.Venta.NuevaVenta }));
            Assert.False(CatalogoPermisos.EsFormularioBloqueadoParaProduccion(new Formulario { FormConsulta = Permisos.Movimiento.VerMovimientos, FormEdicion = Permisos.Movimiento.NuevoMovimiento }));
        }

        // ---------- Acceso al POS (VentasController.TieneAlgunPermisoPOS) ----------

        private static IEnumerable<(DefinicionPermiso, EstadoPermisoAsistente)> Estados(params (int IdForm, int Nivel)[] filas) =>
            filas.Select(f => (Definicion(f.IdForm), new EstadoPermisoAsistente { IdForm = f.IdForm, Nivel = f.Nivel }));

        [Theory]
        [InlineData(IdFormPermiso.PosCtaCtePersona, 1, true)]
        [InlineData(IdFormPermiso.PosBonificacion, 1, true)]
        [InlineData(IdFormPermiso.PosUltimasVentas, 1, true)]
        [InlineData(IdFormPermiso.PosUltimas5VentasCliente, 1, true)]
        [InlineData(IdFormPermiso.Pagos, 1, false)]        // ver pagos no da acceso al POS...
        [InlineData(IdFormPermiso.Pagos, 2, true)]         // ...modificar si (comparte formAddOrEditPago con POS - Ctas.Ctes Persona)
        [InlineData(IdFormPermiso.Ventas, 2, false)]       // Ventas (historial) ya no da acceso al POS
        [InlineData(IdFormPermiso.Compras, 2, false)]
        [InlineData(IdFormPermiso.PosBonificacion, 0, false)]
        public void AccesoAlPos_SegunElPermiso(int idForm, int nivel, bool esperado)
        {
            Assert.Equal(esperado, AsistentePermisosMapper.DaAccesoAlPos(Estados((idForm, nivel))));
        }

        // ---------- Perfiles ----------

        [Fact]
        public void Perfiles_UsanSoloFilasVisiblesYNivelesValidos()
        {
            Assert.NotEmpty(CatalogoPermisos.Perfiles);
            foreach (var perfil in CatalogoPermisos.Perfiles)
            {
                foreach (var estado in perfil.Estados)
                {
                    var def = Definicion(estado.IdForm);
                    Assert.True(CatalogoPermisos.TieneDefinicion(estado.IdForm), $"{perfil.Nombre}: idform {estado.IdForm} sin definicion");
                    Assert.True(string.IsNullOrEmpty(def.SinEfectoHoy), $"{perfil.Nombre}: idform {estado.IdForm} no tiene efecto");
                    int maximo = def.Tipo == TipoPermiso.VerYModificar ? 2 : 1;
                    Assert.InRange(estado.Nivel, 1, maximo);
                }
                Assert.Equal(perfil.Estados.Count, perfil.Estados.Select(e => e.IdForm).Distinct().Count());
            }
        }

        private static PerfilPermisos Perfil(string clave) => CatalogoPermisos.Perfiles.Single(p => p.Clave == clave);

        [Fact]
        public void Perfil_Vendedor_EntraAlPos_PeroNoModificaSusVentas()
        {
            var perfil = Perfil(CatalogoPermisos.PerfilVendedor);

            Assert.True(AsistentePermisosMapper.DaAccesoAlPos(perfil.Estados.Select(e => (Definicion(e.IdForm), e))));
            // Hoy un cajero comun no modifica sus ventas: eso (y vender sin caja y cambiar la fecha) es del vendedor encargado.
            var ultimasVentas = perfil.Estados.Single(e => e.IdForm == IdFormPermiso.PosUltimasVentas);
            Assert.Equal(1, ultimasVentas.Nivel);
            Assert.DoesNotContain(perfil.Estados, e => e.IdForm == IdFormPermiso.PosBonificacion);
            Assert.DoesNotContain(perfil.Estados, e => e.IdForm == IdFormPermiso.Compras);
        }

        [Fact]
        public void Perfil_VendedorEncargado_ModificaVentas_Bonifica_CargaCompras_YRecibeMovimientos()
        {
            var perfil = Perfil(CatalogoPermisos.PerfilVendedorEncargado);

            Assert.Equal(2, perfil.Estados.Single(e => e.IdForm == IdFormPermiso.PosUltimasVentas).Nivel);
            Assert.Equal(1, perfil.Estados.Single(e => e.IdForm == IdFormPermiso.PosBonificacion).Nivel);
            Assert.Equal(2, perfil.Estados.Single(e => e.IdForm == IdFormPermiso.Compras).Nivel);
            Assert.Equal(2, perfil.Estados.Single(e => e.IdForm == IdFormPermiso.Movimientos).Nivel);
        }

        [Fact]
        public void Perfiles_AplicadosAlAsistente_ProducenFilasValidas()
        {
            foreach (var perfil in CatalogoPermisos.Perfiles)
            {
                foreach (var estado in perfil.Estados)
                {
                    var fila = AsistentePermisosMapper.Aplicar(5, estado, Definicion(estado.IdForm), null);
                    Assert.True(fila.DiasPermitidosVer >= 0 || fila.DiasPermitidosEditar >= 0, $"{perfil.Nombre}: idform {estado.IdForm} quedo sin acceso");
                }
            }
        }
    }

    // Los dos permisos de cuentas corrientes (2026-11-10) con las claves reales de `formularios` (idform 17 y 9003):
    // "Cuentas corrientes" deja entrar al listado y a la cuenta de una persona; solo "Ver cuentas corrientes reservadas" deja ver
    // completas las reservadas. Usa Negocio.Usuario.tienePermiso, el mismo chequeo que usan los controllers.
    public class PermisosCuentasCorrientesTests
    {
        private sealed class Empresa : Utilidades.IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private static PermisosUsuarios Otorgado(int idForm, string formConsulta, string formEdicion, int diasVer) => new PermisosUsuarios
        {
            IdForm = idForm,
            DiasPermitidosVer = diasVer,
            DiasPermitidosEditar = -1,
            SoloRegistrosPropios = true,
            Formulario = new Formulario { IdForm = idForm, FormConsulta = formConsulta, FormEdicion = formEdicion, FormEdicionExtra1 = "", FormEdicionExtra2 = "" }
        };

        private static Entidades.Usuario Usuario(params PermisosUsuarios[] filas) =>
            new Entidades.Usuario { Id = 40, Activo = true, Permisos = new List<PermisosUsuarios>(filas) };

        private static bool Tiene(Entidades.Usuario usuario, string permiso) =>
            new Negocio.Usuario(new Empresa()).tienePermiso(usuario, permiso, DateTime.Today, -1);

        private static readonly PermisosUsuarios CuentasCorrientes = Otorgado(IdFormPermiso.CuentasCorrientes, "formCtasCtes", "", 30);
        private static readonly PermisosUsuarios Reservadas = Otorgado(IdFormPermiso.CuentasReservadas, "formCtaCteReservada", "formCtaCteReservada", 0);

        [Fact]
        public void LasClavesDelCodigo_CoincidenConLasFilasDelCatalogo()
        {
            Assert.Equal("formCtasCtes", Permisos.Finanza.VerCtasCtes);
            Assert.Equal("formCtaCteReservada", Permisos.Finanza.VerCtaCteReservada);
        }

        [Fact]
        public void SoloCuentasCorrientes_Entra_PeroNoVeLasReservadas()
        {
            var usuario = Usuario(CuentasCorrientes);

            Assert.True(Tiene(usuario, Permisos.Finanza.VerCtasCtes));
            Assert.False(Tiene(usuario, Permisos.Finanza.VerCtaCteReservada));
        }

        [Fact]
        public void ConReservadas_LasVe_PeroNoEntraSinCuentasCorrientes()
        {
            var usuario = Usuario(Reservadas);

            Assert.True(Tiene(usuario, Permisos.Finanza.VerCtaCteReservada));
            Assert.False(Tiene(usuario, Permisos.Finanza.VerCtasCtes));
        }

        [Fact]
        public void SinNingunoDeLosDos_NoTieneNinguno_YElAdminTieneLosDos()
        {
            Assert.False(Tiene(Usuario(), Permisos.Finanza.VerCtasCtes));
            Assert.False(Tiene(Usuario(Otorgado(IdFormPermiso.CuentasCorrientes, "formCtasCtes", "", -1)), Permisos.Finanza.VerCtasCtes));

            var admin = new Entidades.Usuario { Id = 1, Admin = true, Permisos = new List<PermisosUsuarios>() };
            Assert.True(Tiene(admin, Permisos.Finanza.VerCtasCtes));
            Assert.True(Tiene(admin, Permisos.Finanza.VerCtaCteReservada));
        }
    }

    // Ventana de dias cuando una pantalla acepta mas de un permiso de ver (cuenta corriente de una persona: "Cuentas
    // corrientes" o "POS - Ctas.Ctes Persona"). PermisosHelper.FechaMinimaMasAmplia.
    public class FechaMinimaEntrePermisosTests
    {
        private static readonly DateTime Hoy = new DateTime(2026, 11, 10);

        private static PermisosUsuarios Fila(string formConsulta, int diasVer) => new PermisosUsuarios
        {
            DiasPermitidosVer = diasVer,
            DiasPermitidosEditar = -1,
            Formulario = new Formulario { FormConsulta = formConsulta, FormEdicion = "", FormEdicionExtra1 = "", FormEdicionExtra2 = "" }
        };

        private static readonly string[] Ambos = { Permisos.Finanza.VerCtaCtePersona, Permisos.Finanza.VerCtasCtes };

        [Fact]
        public void GanaLaVentanaMasAmplia()
        {
            var filas = new[] { Fila(Permisos.Finanza.VerCtaCtePersona, 30), Fila(Permisos.Finanza.VerCtasCtes, 90) };

            Assert.Equal(Hoy.AddDays(-90), PermisosHelper.FechaMinimaMasAmplia(filas, Ambos, Hoy));
        }

        [Fact]
        public void UnPermisoSinOtorgar_NoVuelveSinLimiteAlOtorgadoConLimite()
        {
            // "Cuentas corrientes" sin otorgar (-1) y "POS - Ctas.Ctes Persona" con 30 dias: el limite es de 30 dias, no "sin limite".
            var filas = new[] { Fila(Permisos.Finanza.VerCtasCtes, -1), Fila(Permisos.Finanza.VerCtaCtePersona, 30) };

            Assert.Equal(Hoy.AddDays(-30), PermisosHelper.FechaMinimaMasAmplia(filas, Ambos, Hoy));
        }

        [Fact]
        public void SinNingunPermisoOtorgado_DevuelveNull()
        {
            var filas = new[] { Fila(Permisos.Finanza.VerCtasCtes, -1), Fila(Permisos.Finanza.VerCtaCtePersona, -1) };

            Assert.Null(PermisosHelper.FechaMinimaMasAmplia(filas, Ambos, Hoy));
            Assert.Null(PermisosHelper.FechaMinimaMasAmplia(null, Ambos, Hoy));
            Assert.Null(PermisosHelper.FechaMinimaMasAmplia(filas, new string[0], Hoy));
        }

        [Fact]
        public void IgnoraLosPermisosQueNoSonLosPedidos_YNoDistingueMayusculas()
        {
            var filas = new[] { Fila("formCompras", 365), Fila("FORMCTASCTES", 10) };

            Assert.Equal(Hoy.AddDays(-10), PermisosHelper.FechaMinimaMasAmplia(filas, Ambos, Hoy));
        }

        [Fact]
        public void Admin_NoTieneLimite()
        {
            var admin = new Entidades.Usuario { Admin = true, Permisos = new List<PermisosUsuarios> { Fila(Permisos.Finanza.VerCtasCtes, 5) } };

            Assert.Null(PermisosHelper.ObtenerFechaMinimaPermitidaEntre(admin, Ambos));
        }

        [Fact]
        public void TodoElHistorial_DaUnaFechaMuyAntigua()
        {
            var filas = new[] { Fila(Permisos.Finanza.VerCtasCtes, AsistentePermisosMapper.DiasTodoElHistorial) };

            Assert.Equal(Hoy.AddDays(-3650), PermisosHelper.FechaMinimaMasAmplia(filas, Ambos, Hoy));
        }
    }
}
