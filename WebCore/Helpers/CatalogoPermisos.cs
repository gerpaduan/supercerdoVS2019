// Catalogo de permisos para el asistente de usuarios (Usuarios/Asistente): para cada fila de la tabla `formularios` dice
// en que modulo se muestra, como se llama en llano, que habilita y QUE CONTROLES tienen efecto real (dias ver, dias
// editar, alcance Propios/Todos). Sale de revisar como chequea el codigo cada permiso (Negocio.Usuario.tienePermiso y
// sus llamadores, 2026-11-10): en la mayoria el chequeo usa DateTime.Today, asi que los dias no restringen nada y
// pedirlos solo confunde. Detalle y porques: docs/DECISIONS.md (2026-11-10, "Asistente de usuarios y permisos").
// Tambien define los perfiles (plantillas) y las claves que una cuenta de produccion nunca puede tener.
// Un idform que no esta aca (fila nueva de `formularios`) se muestra en "Otros" con todos los controles: default seguro.
using Entidades;

namespace WebCore.Helpers
{
    // Que niveles ofrece un permiso. SoloVer: Sin acceso / Si (se guarda como "ver"). SoloModificar: Sin acceso / Si (el
    // codigo solo mira la clave de edicion; se guarda ver + editar). VerYModificar: Sin acceso / Ver / Ver y modificar.
    public enum TipoPermiso
    {
        SoloVer = 0,
        SoloModificar = 1,
        VerYModificar = 2
    }

    public sealed class DefinicionPermiso
    {
        public int IdForm { get; set; }
        // Posicion dentro de su modulo (orden en que se declaran las definiciones).
        public int Orden { get; set; }
        public string Modulo { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Ayuda { get; set; } = "";
        public TipoPermiso Tipo { get; set; }
        // Que habilita "Ver" (o el "Si" en los de un solo nivel) y que agrega "Ver y modificar".
        public string TextoVer { get; set; } = "";
        public string TextoModificar { get; set; } = "";
        // Advertencia que se muestra junto al selector (efectos colaterales, limites conocidos).
        public string Aviso { get; set; } = "";
        // Controles que tienen efecto real en este permiso; el resto no se muestra.
        public bool UsaDiasVer { get; set; }
        public bool UsaDiasEditar { get; set; }
        public bool UsaAlcance { get; set; }
        // Da acceso al POS (VentasController.TieneAlgunPermisoPOS): con "Ver" y/o con "Modificar".
        public bool PosConVer { get; set; }
        public bool PosConModificar { get; set; }
        // != "" = efecto colateral serio de "Ver y modificar" (se muestra en el resumen "Atencion" del asistente).
        public string RiesgoAlModificar { get; set; } = "";
        // Cierres de Caja: "Ver" arrastra "Modificar" con los mismos dias (regla de negocio vieja, ver UsuariosController).
        public bool EspejaVerEnModificar { get; set; }
        // != "" = hoy ningun chequeo del codigo mira esta fila: el asistente la oculta (la vista avanzada la marca).
        public string SinEfectoHoy { get; set; } = "";
    }

    public sealed class PerfilPermisos
    {
        public string Clave { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public IReadOnlyList<EstadoPermisoAsistente> Estados { get; set; } = new List<EstadoPermisoAsistente>();
    }

    // idform de la tabla `formularios` (catalogo global; ids verificados contra la base local el 2026-11-10).
    public static class IdFormPermiso
    {
        public const int Movimientos = 1;
        public const int Pagos = 2;
        public const int Cheques = 3;
        public const int Stocks = 5;
        public const int Usuarios = 6;
        public const int Ventas = 7;
        public const int CerrarCaja = 8;
        public const int CierresDeCaja = 9;
        public const int EgresosCaja = 10;
        public const int TiposEgreso = 11;
        public const int Compras = 12;
        public const int Productos = 13;
        public const int TiposProducto = 14;
        public const int CostoPorTipoCobro = 15;
        public const int CuentasCorrientes = 17;
        public const int PosCtaCtePersona = 18;
        public const int Elaborados = 19;
        public const int Formulas = 20;
        public const int Etiquetas = 21;
        public const int ExportarListaPrecios = 22;
        public const int ImprimirListaPrecios = 24;
        public const int ReporteStockActual = 25;
        public const int ReporteCierreStock = 26;
        public const int ReporteStockRetroactivo = 27;
        public const int ReporteProyeccionVentasVsStock = 28;
        public const int ReporteVentasPorProducto = 29;
        public const int ReporteBalanceEconomico = 30;
        public const int PosBonificacion = 32;
        public const int PosUltimasVentas = 33;
        public const int PosUltimas5VentasCliente = 34;
        public const int Empleados = 9001;
        public const int Liquidaciones = 9002;
        public const int CuentasReservadas = 9003;
    }

    public static class CatalogoPermisos
    {
        public const string ModuloVentas = "Ventas y POS";
        public const string ModuloCaja = "Caja";
        public const string ModuloCompras = "Compras";
        public const string ModuloStock = "Stock y producción";
        public const string ModuloProductos = "Productos";
        public const string ModuloReportes = "Reportes";
        public const string ModuloFinanzas = "Finanzas";
        public const string ModuloPersonal = "Personal";
        public const string ModuloConfiguracion = "Configuración";
        public const string ModuloOtros = "Otros";

        // Orden en que se muestran los modulos.
        public static readonly string[] OrdenModulos =
        {
            ModuloVentas, ModuloCaja, ModuloCompras, ModuloStock, ModuloProductos, ModuloReportes, ModuloFinanzas,
            ModuloPersonal, ModuloConfiguracion, ModuloOtros
        };

        // Dias que se proponen al dar el permiso por primera vez ("0" = solo hoy).
        public const int DiasVerPorDefecto = 30;
        public const int DiasEditarPorDefecto = 0;

        private const string MotivoSinControl = "Esta pantalla solo existe en el Web clásico: WebCore todavía no la tiene, así que ningún chequeo mira este permiso.";

        private static readonly Dictionary<int, DefinicionPermiso> PorIdForm = Construir();

        private static Dictionary<int, DefinicionPermiso> Construir()
        {
            var lista = new List<DefinicionPermiso>
            {
                // ===== Ventas y POS =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Ventas, Modulo = ModuloVentas, Titulo = "Ventas (historial)",
                    Ayuda = "Historial de ventas.", Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver el historial de ventas.",
                    TextoModificar = "Además, registrar ventas en los puntos de expendio.",
                    Aviso = "No da acceso al POS: para vender hace falta alguno de los permisos \"POS - …\"."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.PosCtaCtePersona, Modulo = ModuloVentas, Titulo = "POS - Cuentas corrientes de clientes",
                    Ayuda = "Da acceso al POS.", Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver la cuenta corriente de un cliente desde el POS (F2).",
                    TextoModificar = "Además, cargar cobros y pagos desde el POS y modificar los que cargó él.",
                    Aviso = "\"Días editar\" nunca pasa del inicio de su caja abierta.",
                    UsaDiasVer = true, UsaDiasEditar = true, PosConVer = true, PosConModificar = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.PosBonificacion, Modulo = ModuloVentas, Titulo = "POS - Bonificar precios",
                    Ayuda = "Da acceso al POS.", Tipo = TipoPermiso.SoloVer,
                    TextoVer = "Bonificar (descontar) el precio de los productos en la venta.", PosConVer = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.PosUltimasVentas, Modulo = ModuloVentas, Titulo = "POS - Mis últimas ventas",
                    Ayuda = "Da acceso al POS.", Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver sus ventas desde que abrió la caja.",
                    TextoModificar = "Modificar esas ventas, vender sin tener la caja abierta y cambiar la fecha de la venta.",
                    Aviso = "Modificar también deja vender sin caja abierta y cambiar la fecha. \"Días editar\" nunca pasa del inicio de su caja.",
                    UsaDiasEditar = true, PosConVer = true, PosConModificar = true,
                    RiesgoAlModificar = "Puede vender sin caja abierta y cambiar la fecha de las ventas"
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.PosUltimas5VentasCliente, Modulo = ModuloVentas, Titulo = "POS - Últimas 5 ventas de un cliente",
                    Ayuda = "Da acceso al POS.", Tipo = TipoPermiso.SoloVer,
                    TextoVer = "Ver las últimas 5 ventas de un cliente en el POS.", PosConVer = true
                },

                // ===== Caja =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.CerrarCaja, Modulo = ModuloCaja, Titulo = "Cerrar caja",
                    Tipo = TipoPermiso.SoloVer,
                    TextoVer = "Ver las cajas abiertas y cerrarlas (pide la clave para confirmar)."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.CierresDeCaja, Modulo = ModuloCaja, Titulo = "Cierres de caja (historial)",
                    Tipo = TipoPermiso.SoloVer, EspejaVerEnModificar = true,
                    TextoVer = "Ver el historial de cierres de caja y modificar un cierre histórico."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.EgresosCaja, Modulo = ModuloCaja, Titulo = "Egresos de caja",
                    Ayuda = "Egresos (gastos) del negocio.", Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver los egresos de caja.",
                    TextoModificar = "Además, cargar egresos nuevos.",
                    Aviso = "Modificar un egreso ya cargado lo hace solo un administrador.",
                    UsaDiasVer = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.TiposEgreso, Modulo = ModuloCaja, Titulo = "Tipos de egreso",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver los tipos de egreso.",
                    TextoModificar = "Además, crear y modificar tipos de egreso.",
                    Aviso = "Eliminar un tipo de egreso lo hace solo un administrador."
                },

                // ===== Compras =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Compras, Modulo = ModuloCompras, Titulo = "Compras",
                    Ayuda = "Historial de compras.", Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver el historial de compras.",
                    TextoModificar = "Además, cargar compras nuevas y modificar las existentes.",
                    UsaDiasVer = true, UsaDiasEditar = true, UsaAlcance = true
                },

                // ===== Stock y produccion =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Movimientos, Modulo = ModuloStock, Titulo = "Movimientos de mercadería",
                    Ayuda = "Movimientos entre sucursales.", Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver los movimientos.",
                    TextoModificar = "Además, cargar movimientos nuevos y modificar los existentes.",
                    Aviso = "El alcance solo limita quién puede eliminar un movimiento.",
                    UsaDiasVer = true, UsaDiasEditar = true, UsaAlcance = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Stocks, Modulo = ModuloStock, Titulo = "Stock",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver el stock y sus registros. También habilita los reportes de stock.",
                    TextoModificar = "Además, cargar y modificar registros de stock.",
                    Aviso = "El ajuste de stock es solo para administradores.",
                    UsaDiasVer = true, UsaDiasEditar = true, UsaAlcance = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Elaborados, Modulo = ModuloStock, Titulo = "Elaborados (embutidos)",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver las elaboraciones.",
                    TextoModificar = "Además, cargar y modificar elaboraciones (incluye el ingreso rápido).",
                    UsaDiasVer = true, UsaDiasEditar = true, UsaAlcance = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Formulas, Modulo = ModuloStock, Titulo = "Fórmulas",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver las fórmulas (la fórmula secreta pide usuario y clave igual).",
                    TextoModificar = "Además, cargar y modificar fórmulas.",
                    UsaAlcance = true
                },

                // ===== Productos =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Productos, Modulo = ModuloProductos, Titulo = "Productos",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver los productos.",
                    TextoModificar = "Además, crear, modificar y eliminar productos, cambiar precios, administrar marcas y cargar puntos de stock.",
                    Aviso = "Modificar también deja dar de alta un producto rápido desde el POS."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.TiposProducto, Modulo = ModuloProductos, Titulo = "Tipos de producto",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver los tipos de producto.",
                    TextoModificar = "Además, crear, modificar y eliminar tipos de producto."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Etiquetas, Modulo = ModuloProductos, Titulo = "Etiquetas de productos",
                    Tipo = TipoPermiso.SoloVer,
                    TextoVer = "Generar etiquetas de productos en PDF (botón «Etiquetas» de Productos)."
                },

                // ===== Reportes (cada tipo de reporte tiene su fila; antes se habilitaban todos con "Stocks") =====
                Reporte(IdFormPermiso.ReporteStockActual, "Reporte: Stock actual", "Ver el reporte de stock actual y exportarlo a Excel."),
                Reporte(IdFormPermiso.ReporteCierreStock, "Reporte: Cierre de stock", "Ver el reporte de cierre de stock y exportarlo a Excel."),
                Reporte(IdFormPermiso.ReporteStockRetroactivo, "Reporte: Stock retroactivo", "Ver el reporte de stock retroactivo y exportarlo a Excel."),
                Reporte(IdFormPermiso.ReporteProyeccionVentasVsStock, "Reporte: Proyección de ventas vs stock", "Ver el reporte de proyección de ventas contra el stock."),
                Reporte(IdFormPermiso.ReporteVentasPorProducto, "Reporte: Ventas por producto", "Ver el reporte de ventas por producto (con su gráfico)."),
                Reporte(IdFormPermiso.ReporteBalanceEconomico, "Reporte: Balance económico", "Ver el reporte de balance económico."),

                // ===== Finanzas =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.CuentasCorrientes, Modulo = ModuloFinanzas, Titulo = "Cuentas corrientes",
                    Tipo = TipoPermiso.SoloVer, UsaDiasVer = true,
                    TextoVer = "Entrar a Cuentas corrientes (menú Finanzas) y ver la cuenta de cualquier persona.",
                    Aviso = "Las cuentas reservadas se ven completas solo con \"Ver cuentas corrientes reservadas\"."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.CuentasReservadas, Modulo = ModuloFinanzas, Titulo = "Ver cuentas corrientes reservadas",
                    Tipo = TipoPermiso.SoloVer,
                    TextoVer = "Ver completas las cuentas corrientes reservadas y marcar a una persona como reservada.",
                    Aviso = "Sin este permiso se ve a la persona pero no su saldo, y solo lo que cargó él desde que abrió su caja."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Pagos, Modulo = ModuloFinanzas, Titulo = "Pagos y cobros",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver el listado de pagos y cobros.",
                    TextoModificar = "Además, reasignar a otra persona y eliminar pagos y cobros.",
                    Aviso = "Modificar también da acceso al POS (comparte el permiso de cargar cobros).",
                    UsaDiasEditar = true, UsaAlcance = true, PosConModificar = true
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Cheques, Modulo = ModuloFinanzas, Titulo = "Cheques",
                    Tipo = TipoPermiso.VerYModificar,
                    TextoVer = "Ver los cheques.",
                    TextoModificar = "Además, cargar y modificar cheques.",
                    UsaAlcance = true
                },

                // ===== Personal =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Empleados, Modulo = ModuloPersonal, Titulo = "Empleados",
                    Tipo = TipoPermiso.SoloModificar,
                    TextoVer = "Dar de alta, modificar y dar de baja empleados (incluye vacaciones)."
                },
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Liquidaciones, Modulo = ModuloPersonal, Titulo = "Liquidación de sueldos",
                    Tipo = TipoPermiso.SoloModificar,
                    TextoVer = "Generar, consultar y eliminar liquidaciones de sueldo."
                },

                // ===== Configuracion =====
                new DefinicionPermiso
                {
                    IdForm = IdFormPermiso.Usuarios, Modulo = ModuloConfiguracion, Titulo = "Usuarios",
                    Tipo = TipoPermiso.SoloModificar,
                    TextoVer = "Crear y modificar usuarios y sus permisos, desbloquearlos y quitarles el PIN."
                }
            };

            // Filas de pantallas que solo existen en el Web clasico (WebCore no las tiene): ningun chequeo las mira, el asistente
            // las oculta y la vista avanzada las marca. Si WebCore las porta, hay que chequearlas y sacarlas de esta lista.
            foreach (int idForm in new[]
            {
                IdFormPermiso.CostoPorTipoCobro, IdFormPermiso.ExportarListaPrecios, IdFormPermiso.ImprimirListaPrecios
            })
                lista.Add(new DefinicionPermiso { IdForm = idForm, Modulo = ModuloOtros, Tipo = TipoPermiso.VerYModificar, SinEfectoHoy = MotivoSinControl });

            for (int i = 0; i < lista.Count; i++) lista[i].Orden = i;
            return lista.ToDictionary(d => d.IdForm);
        }

        // Permiso de un reporte de Reportes/Index (ver PermisosReportes): un solo nivel ("Ver") y los dias "ver" limitan hasta
        // cuantos dias atras se puede poner la fecha "desde".
        private static DefinicionPermiso Reporte(int idForm, string titulo, string textoVer)
        {
            return new DefinicionPermiso
            {
                IdForm = idForm, Modulo = ModuloReportes, Titulo = titulo, Tipo = TipoPermiso.SoloVer,
                TextoVer = textoVer, UsaDiasVer = true
            };
        }

        public static bool TieneDefinicion(int idForm) => PorIdForm.ContainsKey(idForm);

        public static IReadOnlyCollection<DefinicionPermiso> Todas => PorIdForm.Values;

        // Definicion del formulario. Si no esta en el catalogo (fila nueva), devuelve una generica con todos los controles.
        public static DefinicionPermiso Para(Formulario? formulario, int idFormSiNoHayFormulario = 0)
        {
            int idForm = formulario != null ? formulario.IdForm : idFormSiNoHayFormulario;
            if (PorIdForm.TryGetValue(idForm, out var definicion)) return definicion;

            return new DefinicionPermiso
            {
                IdForm = idForm,
                Orden = int.MaxValue,
                Modulo = ModuloOtros,
                Titulo = !string.IsNullOrWhiteSpace(formulario?.NombreForm) ? formulario!.NombreForm : ("Permiso " + idForm),
                Ayuda = formulario?.Descripcion ?? "",
                Tipo = TipoPermiso.VerYModificar,
                TextoVer = "Ver.",
                TextoModificar = "Además, modificar.",
                UsaDiasVer = true,
                UsaDiasEditar = true,
                UsaAlcance = true
            };
        }

        // ===== Cuenta de produccion =====
        // Claves que una cuenta de produccion (PC compartida de un sector) nunca puede tener, sin importar lo tildado.
        // Movido desde UsuariosController (2026-11-10) para que la grilla y el asistente usen la misma lista.
        public static readonly HashSet<string> ClavesBloqueadasUsuarioProduccion = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Permisos.Venta.Bonificar, Permisos.Venta.VerVentas, Permisos.Venta.VerBonificar,
            Permisos.Venta.VerVentasVendedor, Permisos.Venta.VerGetAllLineaVenta, Permisos.Venta.NuevaVenta,
            Permisos.Venta.UltimaVenta, Permisos.Venta.GetAllLineaVenta,
            Permisos.Finanza.VerCheques, Permisos.Finanza.VerPagos, Permisos.Finanza.AddOrEditPago,
            Permisos.Finanza.VerCtasCtes, Permisos.Finanza.VerCtaCtePersona, Permisos.Finanza.VerCtaCteReservada,
            Permisos.Elaborado.VerFormulas, Permisos.Elaborado.IngresoFormula,
            Permisos.Empleado.VerEmpleados, Permisos.Empleado.NuevoEmpleado,
            Permisos.Empleado.VerLiquidaciones, Permisos.Empleado.NuevaLiquidacion
        };

        public static bool EsFormularioBloqueadoParaProduccion(Formulario formulario)
        {
            return ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormConsulta ?? "")
                || ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormEdicion ?? "")
                || ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormEdicionExtra1 ?? "")
                || ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormEdicionExtra2 ?? "");
        }

        // ===== Perfiles (plantillas) =====
        // Punto de partida para ajustar permiso por permiso antes de guardar; no queda guardado el perfil, solo sus permisos.
        public const string PerfilVendedor = "vendedor";
        public const string PerfilVendedorEncargado = "vendedor-encargado";

        public static IReadOnlyList<PerfilPermisos> Perfiles { get; } = new List<PerfilPermisos>
        {
            new PerfilPermisos
            {
                Clave = PerfilVendedor,
                Nombre = "Vendedor / Cajero",
                Descripcion = "Vende en el POS, ve sus ventas (sin modificarlas), consulta la cuenta corriente de los clientes (30 días) y carga egresos de caja.",
                Estados = new List<EstadoPermisoAsistente>
                {
                    Estado(IdFormPermiso.PosUltimasVentas, 1),
                    Estado(IdFormPermiso.PosUltimas5VentasCliente, 1),
                    Estado(IdFormPermiso.PosCtaCtePersona, 2, diasVer: 30),
                    Estado(IdFormPermiso.EgresosCaja, 2)
                }
            },
            new PerfilPermisos
            {
                Clave = PerfilVendedorEncargado,
                Nombre = "Vendedor encargado",
                Descripcion = "Lo del vendedor, y además modifica sus ventas, bonifica precios, carga compras y recibe mercadería (movimientos). Modificar ventas también deja vender sin caja abierta y cambiar la fecha.",
                Estados = new List<EstadoPermisoAsistente>
                {
                    Estado(IdFormPermiso.PosUltimasVentas, 2),
                    Estado(IdFormPermiso.PosUltimas5VentasCliente, 1),
                    Estado(IdFormPermiso.PosCtaCtePersona, 2, diasVer: 30),
                    Estado(IdFormPermiso.EgresosCaja, 2),
                    Estado(IdFormPermiso.PosBonificacion, 1),
                    Estado(IdFormPermiso.Compras, 2, diasVer: 30, diasEditar: 7),
                    // "Recepcion" = movimientos de mercaderia que llegan de otra sucursal (supuesto, ver DECISIONS 2026-11-10).
                    Estado(IdFormPermiso.Movimientos, 2, diasVer: 30, diasEditar: 7)
                }
            }
        };

        private static EstadoPermisoAsistente Estado(int idForm, int nivel, int diasVer = DiasVerPorDefecto, int diasEditar = DiasEditarPorDefecto)
        {
            return new EstadoPermisoAsistente
            {
                IdForm = idForm,
                Nivel = nivel,
                DiasVer = diasVer,
                DiasEditar = diasEditar,
                SoloPropios = true
            };
        }
    }
}
