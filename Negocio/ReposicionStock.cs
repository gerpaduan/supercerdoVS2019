// "Qué pedir": arma la lista de productos a reponer por sucursal a partir del stock actual, el punto de stock y la
// actividad reciente (ventas + consumo por elaboración). Orquesta las consultas y delega las reglas en
// ReposicionStockCalculador (puro, sin base de datos, con tests: Negocio.Tests/ReposicionStockCalculadorTests.cs).
// Depende de Negocio.Corte (existencia por sucursal, la misma que muestra Existencia por sucursales) y de
// Contratos.IActividadStockRepository (solo Postgres). Reglas y porqués: docs/11-manual-de-uso/pantallas/Reposicion.Index.md
// y docs/DECISIONS.md (2026-10-13, "Qué pedir").
//
// Sintaxis a propósito conservadora (C# 7.3): Negocio también compila para net472.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Contratos;
using Entidades;

namespace Negocio
{
    public class ReposicionStock
    {
        private readonly Corte _corteN;
        private readonly IActividadStockRepository _actividadRepo;
        // Solo hace falta para guardar puntos sugeridos; null = esa operación no está disponible.
        private readonly CortePuntoStockSucursal _puntoStockN;

        public ReposicionStock(Corte corteN, IActividadStockRepository actividadRepo, CortePuntoStockSucursal puntoStockN = null)
        {
            if (corteN == null) throw new ArgumentNullException(nameof(corteN));
            if (actividadRepo == null) throw new ArgumentNullException(nameof(actividadRepo));
            _corteN = corteN;
            _actividadRepo = actividadRepo;
            _puntoStockN = puntoStockN;
        }

        // "Sugerir puntos de stock": para cada producto y sucursal, el punto que cubriría DiasCoberturaPunto días del consumo
        // reciente. Solo lectura (no guarda nada: eso es GuardarPuntosSugeridos, a pedido explícito del usuario).
        public PuntosSugeridosVm ArmarPuntosSugeridos(ReposicionFiltroVm filtro)
        {
            return ArmarPuntosSugeridos(filtro, DateTime.Now);
        }

        public PuntosSugeridosVm ArmarPuntosSugeridos(ReposicionFiltroVm filtro, DateTime ahora)
        {
            var filtroNormalizado = ReposicionStockCalculador.Normalizar(filtro);
            var plano = _corteN.ObtenerExistenciaPlano(FiltroExistenciaTodas(filtroNormalizado, ahora)) ?? new List<ExistenciaStockPorSucursalPlanoVm>();
            var actividad = _actividadRepo.ObtenerActividadStock(0, filtroNormalizado.DiasVentana, ReposicionStockCalculador.DiasHistoriaUltimaSalida)
                ?? new List<ActividadStockVm>();
            var proveedores = _actividadRepo.ObtenerProveedoresHabituales(filtroNormalizado.IdProveedor)
                ?? new List<ProveedorHabitualVm>();

            return ReposicionStockCalculador.CalcularPuntosSugeridos(plano, actividad, proveedores, filtroNormalizado, ahora);
        }

        // Guarda los puntos de stock pedidos. NUNCA pisa un punto ya cargado ni inventa productos: cada pedido se valida contra el
        // stock actual (ReposicionStockCalculador.ValidarPuntosAGuardar) y lo rechazado vuelve con su motivo. Se escribe un producto
        // por vez (todas sus sucursales juntas, igual que el guardado de Productos); si la base falla se corta y lo ya escrito queda.
        public GuardarPuntosResultadoVm GuardarPuntosSugeridos(IEnumerable<PuntoAGuardarVm> pedidos, int idEmpresa)
        {
            if (_puntoStockN == null) throw new InvalidOperationException("Guardar puntos de stock no está disponible en esta instancia.");

            var resultado = new GuardarPuntosResultadoVm();
            var plano = _corteN.ObtenerExistenciaPlano(FiltroExistenciaTodas(ReposicionStockCalculador.Normalizar(null), DateTime.Now))
                ?? new List<ExistenciaStockPorSucursalPlanoVm>();

            var aceptados = ReposicionStockCalculador.ValidarPuntosAGuardar(plano, pedidos, resultado.Rechazados);

            foreach (var grupo in aceptados.GroupBy(x => x.IdCorte))
            {
                try
                {
                    var valores = grupo.Select(x => (idSucursal: x.IdSucursal, puntoStock: x.PuntoStock)).ToList();
                    _puntoStockN.GuardarPuntosStockLote(idEmpresa, grupo.Key, valores);
                    resultado.Guardados += valores.Count;
                }
                catch (Exception ex)
                {
                    resultado.Error = ex;
                    break;
                }
            }
            return resultado;
        }

        // Existencia de TODAS las sucursales con los filtros de producto del usuario (hace falta ver todas para ofrecer "otras sucursales"
        // y para validar al guardar).
        private static ExistenciaStockPorSucursalFiltroVm FiltroExistenciaTodas(ReposicionFiltroVm f, DateTime ahora)
        {
            return new ExistenciaStockPorSucursalFiltroVm
            {
                Texto = f.Texto,
                Tipo = f.Tipo,
                IdProveedor = f.IdProveedor,
                IdMarca = f.IdMarca,
                FechaHasta = ahora,
                SoloConStock = false,
                IdsSucursales = new List<int>()
            };
        }

        // Consulta todo lo necesario y devuelve el resultado listo para mostrar. Solo lectura.
        // Siempre se calcula la existencia de TODAS las sucursales (cuesta lo mismo que una, ver Corte.ObtenerExistenciaPlano)
        // porque hace falta ver el stock de las otras sucursales; las que no se tildaron se descartan en el cálculo.
        public ReposicionVm Armar(ReposicionFiltroVm filtro)
        {
            return Armar(filtro, DateTime.Now);
        }

        public ReposicionVm Armar(ReposicionFiltroVm filtro, DateTime ahora)
        {
            var filtroNormalizado = ReposicionStockCalculador.Normalizar(filtro);

            var plano = _corteN.ObtenerExistenciaPlano(FiltroExistenciaTodas(filtroNormalizado, ahora)) ?? new List<ExistenciaStockPorSucursalPlanoVm>();
            var actividad = _actividadRepo.ObtenerActividadStock(0, filtroNormalizado.DiasVentana, ReposicionStockCalculador.DiasHistoriaUltimaSalida)
                ?? new List<ActividadStockVm>();
            var proveedores = _actividadRepo.ObtenerProveedoresHabituales(filtroNormalizado.IdProveedor)
                ?? new List<ProveedorHabitualVm>();

            return ReposicionStockCalculador.Calcular(plano, actividad, proveedores, filtroNormalizado, ahora);
        }
    }

    // Reglas de "Qué pedir", sin acceso a datos: recibe lo ya consultado y devuelve el resultado. Estático y puro para
    // poder probarlo con casos armados a mano.
    public static class ReposicionStockCalculador
    {
        // Límites del parámetro "días de actividad": menos de una semana es ruido, más de medio año deja de ser "reciente".
        public const int MinDiasVentana = 7;
        public const int MaxDiasVentana = 180;
        public const int DiasVentanaPorDefecto = 30;
        // Tope de "días de venta a cubrir" para productos sin punto de stock.
        public const int MaxDiasCobertura = 60;
        // "Sugerir puntos de stock": días de consumo que cubre el punto sugerido (por defecto 7; entre 1 y 90).
        public const int MinDiasCoberturaPunto = 1;
        public const int MaxDiasCoberturaPunto = 90;
        public const int DiasCoberturaPuntoPorDefecto = 7;
        // El punto de stock se guarda como entero: techo razonable para frenar un valor mal tipeado (o un consumo disparatado).
        public const int MaxPuntoStock = 999999;
        // Máximo de puntos por pedido de guardado (frena un pedido armado a mano con miles de filas).
        public const int MaxPuntosPorGuardado = 500;
        // Hasta dónde se mira hacia atrás para mostrar la fecha de la última salida (más viejo = "más de un año").
        public const int DiasHistoriaUltimaSalida = 365;
        // El stock parte del último cierre: pasado este tiempo sin cerrar, los números pueden estar desactualizados.
        public const int DiasCierreViejo = 30;
        // Si el stock que queda cubre más que esto al ritmo reciente, el punto de stock cargado parece alto (solo se informa).
        public const int DiasCoberturaPuntoAlto = 90;
        // Las fechas anteriores a esta son el "sin cierre" que usa la consulta de existencia (1900-01-01).
        private static readonly DateTime FechaSinCierre = new DateTime(1901, 1, 1);

        // Deja el filtro dentro de límites válidos (input de la pantalla = no confiable). Devuelve una copia.
        public static ReposicionFiltroVm Normalizar(ReposicionFiltroVm filtro)
        {
            var origen = filtro ?? new ReposicionFiltroVm();
            int dias = origen.DiasVentana <= 0 ? DiasVentanaPorDefecto : origen.DiasVentana;
            int cobertura = origen.DiasCoberturaSinPunto;

            return new ReposicionFiltroVm
            {
                IdsSucursales = (origen.IdsSucursales ?? new List<int>()).Where(x => x > 0).Distinct().ToList(),
                Texto = (origen.Texto ?? "").Trim(),
                Tipo = (origen.Tipo ?? "").Trim(),
                IdProveedor = origen.IdProveedor > 0 ? origen.IdProveedor : 0,
                IdMarca = origen.IdMarca > 0 ? origen.IdMarca : 0,
                DiasVentana = Math.Min(MaxDiasVentana, Math.Max(MinDiasVentana, dias)),
                DiasCoberturaSinPunto = Math.Min(MaxDiasCobertura, Math.Max(0, cobertura)),
                MostrarSinMovimiento = origen.MostrarSinMovimiento,
                // 0 o negativo = no lo mandaron: queda el valor por defecto en vez de pegar contra el mínimo.
                DiasCoberturaPunto = Math.Min(MaxDiasCoberturaPunto, Math.Max(MinDiasCoberturaPunto,
                    origen.DiasCoberturaPunto <= 0 ? DiasCoberturaPuntoPorDefecto : origen.DiasCoberturaPunto)),
                IncluirConPunto = origen.IncluirConPunto
            };
        }

        // plano: existencia de TODAS las sucursales (una fila por producto de stock y sucursal, con el punto de stock real).
        // actividad / proveedores: salidas recientes y proveedor habitual. filtro: ya normalizado. ahora: fecha del cálculo.
        public static ReposicionVm Calcular(
            IList<ExistenciaStockPorSucursalPlanoVm> plano,
            IList<ActividadStockVm> actividad,
            IList<ProveedorHabitualVm> proveedores,
            ReposicionFiltroVm filtro,
            DateTime ahora)
        {
            var resultado = new ReposicionVm
            {
                Filtro = filtro,
                FechaCalculo = ahora,
                ConsultaRealizada = true
            };
            var filas = plano ?? new List<ExistenciaStockPorSucursalPlanoVm>();

            // Lo que se muestra son las sucursales tildadas (vacío = todas).
            Func<int, bool> sucursalElegida = id => filtro.IdsSucursales.Count == 0 || filtro.IdsSucursales.Contains(id);

            // Actividad y proveedor por clave. Si hubiera duplicados se queda con el primero (no debería pasar: la consulta agrupa).
            var actividadPor = new Dictionary<Tuple<int, int>, ActividadStockVm>();
            foreach (var a in actividad ?? new List<ActividadStockVm>())
            {
                var clave = Tuple.Create(a.IdSucursal, a.IdCorte);
                if (!actividadPor.ContainsKey(clave)) actividadPor[clave] = a;
            }
            var proveedorPor = new Dictionary<int, ProveedorHabitualVm>();
            foreach (var p in proveedores ?? new List<ProveedorHabitualVm>())
            {
                if (!proveedorPor.ContainsKey(p.IdCorte)) proveedorPor[p.IdCorte] = p;
            }

            // Stock de cada producto en cada sucursal, para ofrecer "hay en otra sucursal".
            var porProducto = filas.GroupBy(x => x.IdCorte).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var r in filas)
            {
                if (!sucursalElegida(r.IdSucursal)) continue;
                resultado.CantProductosControlados++;
                if (r.PuntoStock <= 0f) resultado.CantSinPuntoStock++;

                ActividadStockVm act;
                actividadPor.TryGetValue(Tuple.Create(r.IdSucursal, r.IdCorte), out act);

                var fila = ClasificarFila(r, act, filtro, ahora);
                if (fila == null) continue;

                switch (fila.Motivo)
                {
                    case MotivoReposicion.BajoPuntoStock: resultado.CantBajoPuntoStock++; break;
                    case MotivoReposicion.EnCeroConMovimiento: resultado.CantEnCeroConMovimiento++; break;
                    default: resultado.CantEnCeroSinMovimiento++; break;
                }

                if (fila.Motivo == MotivoReposicion.EnCeroSinMovimiento && !filtro.MostrarSinMovimiento)
                    continue;

                // Otras sucursales con stock del mismo producto.
                List<ExistenciaStockPorSucursalPlanoVm> mismoProducto;
                if (porProducto.TryGetValue(r.IdCorte, out mismoProducto))
                {
                    fila.OtrasSucursales = mismoProducto
                        .Where(x => x.IdSucursal != r.IdSucursal && x.StockActual > 0f)
                        .OrderBy(x => x.Sucursal)
                        .Select(x => new ReposicionStockSucursalVm { IdSucursal = x.IdSucursal, Sucursal = x.Sucursal ?? "", Stock = x.StockActual })
                        .ToList();
                }

                // Proveedor y precio de la última compra.
                ProveedorHabitualVm prov;
                if (proveedorPor.TryGetValue(r.IdCorte, out prov))
                {
                    fila.IdProveedor = prov.IdProveedor;
                    fila.Proveedor = string.IsNullOrWhiteSpace(prov.Proveedor) ? "" : prov.Proveedor.Trim();
                    fila.UltimoPrecio = prov.UltimoPrecio;
                    fila.FechaUltimaCompra = prov.FechaUltimaCompra;
                    if (fila.Pedir.HasValue && prov.UltimoPrecio.HasValue && prov.UltimoPrecio.Value > 0m)
                        fila.ImporteEstimado = Math.Round((decimal)fila.Pedir.Value * prov.UltimoPrecio.Value, 2);
                }

                resultado.Filas.Add(fila);
            }

            // Orden: proveedor (A-Z, "sin proveedor" al final); dentro, primero lo que ya está en cero, luego lo que menos
            // días de stock tiene, y por último por nombre.
            resultado.Filas = resultado.Filas
                .OrderBy(x => string.IsNullOrEmpty(x.Proveedor) ? 1 : 0)
                .ThenBy(x => x.Proveedor, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.StockActual <= 0f ? 0 : 1)
                .ThenBy(x => x.CoberturaDias.HasValue ? x.CoberturaDias.Value : float.MaxValue)
                .ThenBy(x => x.Producto, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.Sucursal, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            resultado.ImporteEstimadoTotal = resultado.Filas.Where(x => x.ImporteEstimado.HasValue).Sum(x => x.ImporteEstimado.Value);
            resultado.AvisosCierre = ArmarAvisosCierre(filas.Where(x => sucursalElegida(x.IdSucursal)), ahora);
            return resultado;
        }

        // Decide si una fila de existencia requiere reposición y con qué motivo. null = no hace falta nada.
        // Reglas (en este orden):
        //  1. Con punto de stock cargado: si el stock es MENOR al punto -> BajoPuntoStock, pedir = punto - stock
        //     (el stock negativo cuenta como 0: casi siempre es un ingreso sin cargar, se marca para revisar).
        //     Igual al punto no hay nada que pedir.
        //  2. Sin punto de stock y con stock <= 0: si tuvo salidas dentro de la ventana -> EnCeroConMovimiento (aviso);
        //     si no -> EnCeroSinMovimiento (no se avisa, solo se cuenta). La cantidad solo se sugiere si el usuario pidió
        //     "días de venta": consumo diario x días - stock.
        //  3. Sin punto de stock y con stock positivo: no se dice nada.
        public static ReposicionFilaVm ClasificarFila(ExistenciaStockPorSucursalPlanoVm r, ActividadStockVm act, ReposicionFiltroVm filtro, DateTime ahora)
        {
            // Mismo criterio que las pantallas de stock: se compara con los 3 decimales que se ven.
            float stock = (float)Math.Round(r.StockActual, 3);
            float punto = r.PuntoStock;
            float stockParaCalculo = Math.Max(stock, 0f);

            float consumoVentana = 0f;
            DateTime? ultimaSalida = null;
            if (act != null)
            {
                // Una devolución puede dejar el neto negativo: sin salidas reales no hay consumo.
                consumoVentana = Math.Max(0f, act.VentasVentana) + Math.Max(0f, act.ElaboracionVentana);
                ultimaSalida = MaxFecha(act.UltimaVenta, act.UltimaElaboracion);
            }
            float consumoDiario = filtro.DiasVentana > 0 ? consumoVentana / filtro.DiasVentana : 0f;
            float? cobertura = consumoDiario > 0f ? (float?)(stockParaCalculo / consumoDiario) : null;
            bool hayMovimiento = consumoVentana > 0f;

            MotivoReposicion motivo;
            float? pedir = null;

            if (punto > 0f)
            {
                if (stock >= punto) return null;
                motivo = MotivoReposicion.BajoPuntoStock;
                pedir = RedondearPedido(punto - stockParaCalculo, r.Pesable);
            }
            else if (stock <= 0f)
            {
                if (hayMovimiento)
                {
                    motivo = MotivoReposicion.EnCeroConMovimiento;
                    if (filtro.DiasCoberturaSinPunto > 0)
                        pedir = RedondearPedido(consumoDiario * filtro.DiasCoberturaSinPunto - stockParaCalculo, r.Pesable);
                }
                else
                {
                    motivo = MotivoReposicion.EnCeroSinMovimiento;
                }
            }
            else
            {
                return null;
            }

            // Un pedido calculado que no llega a nada no se muestra como cantidad.
            if (pedir.HasValue && pedir.Value <= 0f) pedir = null;

            return new ReposicionFilaVm
            {
                IdCorte = r.IdCorte,
                Codigo = r.Codigo,
                Producto = r.Corte ?? "",
                IdSucursal = r.IdSucursal,
                Sucursal = r.Sucursal ?? "",
                Pesable = r.Pesable,
                Promedio = r.Promedio,
                StockActual = stock,
                PuntoStock = punto,
                Motivo = motivo,
                StockNegativo = stock < 0f,
                PuntoPareceAlto = motivo == MotivoReposicion.BajoPuntoStock && cobertura.HasValue && cobertura.Value > DiasCoberturaPuntoAlto,
                Pedir = pedir,
                ConsumoVentana = consumoVentana,
                ConsumoDiario = consumoDiario,
                CoberturaDias = cobertura,
                UltimaSalida = ultimaSalida,
                DiasSinSalida = ultimaSalida.HasValue ? (int?)Math.Max(0, (int)(ahora.Date - ultimaSalida.Value.Date).TotalDays) : null
            };
        }

        // ---- Sugerir puntos de stock ----

        // Una fila por producto y sucursal tildada con el punto que cubriría filtro.DiasCoberturaPunto días del consumo de los
        // últimos filtro.DiasVentana días (ventas + elaboración, igual que "Qué pedir"). Se listan los que hoy no tienen punto y
        // tienen consumo; con IncluirConPunto también los que ya lo tienen (solo para comparar) y con MostrarSinMovimiento los que
        // no tuvieron consumo (sin sugerencia). Los contadores cubren todo lo consultado, se liste o no.
        public static PuntosSugeridosVm CalcularPuntosSugeridos(
            IList<ExistenciaStockPorSucursalPlanoVm> plano,
            IList<ActividadStockVm> actividad,
            IList<ProveedorHabitualVm> proveedores,
            ReposicionFiltroVm filtro,
            DateTime ahora)
        {
            var resultado = new PuntosSugeridosVm { Filtro = filtro, FechaCalculo = ahora, ConsultaRealizada = true };
            Func<int, bool> sucursalElegida = id => filtro.IdsSucursales.Count == 0 || filtro.IdsSucursales.Contains(id);

            var actividadPor = new Dictionary<Tuple<int, int>, ActividadStockVm>();
            foreach (var a in actividad ?? new List<ActividadStockVm>())
            {
                var clave = Tuple.Create(a.IdSucursal, a.IdCorte);
                if (!actividadPor.ContainsKey(clave)) actividadPor[clave] = a;
            }
            var proveedorPor = new Dictionary<int, ProveedorHabitualVm>();
            foreach (var p in proveedores ?? new List<ProveedorHabitualVm>())
            {
                if (!proveedorPor.ContainsKey(p.IdCorte)) proveedorPor[p.IdCorte] = p;
            }

            foreach (var r in plano ?? new List<ExistenciaStockPorSucursalPlanoVm>())
            {
                if (!sucursalElegida(r.IdSucursal)) continue;
                resultado.CantProductosControlados++;

                bool tienePunto = r.PuntoStock > 0f;
                if (tienePunto) resultado.CantConPunto++; else resultado.CantSinPunto++;

                ActividadStockVm act;
                actividadPor.TryGetValue(Tuple.Create(r.IdSucursal, r.IdCorte), out act);

                float consumoVentana = 0f;
                DateTime? ultimaSalida = null;
                int diasConVenta = 0;
                if (act != null)
                {
                    // Una devolución puede dejar el neto negativo: sin salidas reales no hay consumo.
                    consumoVentana = Math.Max(0f, act.VentasVentana) + Math.Max(0f, act.ElaboracionVentana);
                    ultimaSalida = MaxFecha(act.UltimaVenta, act.UltimaElaboracion);
                    diasConVenta = act.DiasConVentaVentana;
                }
                float consumoDiario = filtro.DiasVentana > 0 ? consumoVentana / filtro.DiasVentana : 0f;
                int? sugerido = consumoDiario > 0f ? (int?)SugerirPunto(consumoDiario, filtro.DiasCoberturaPunto) : null;

                if (!tienePunto)
                {
                    if (sugerido.HasValue) resultado.CantConSugerencia++; else resultado.CantSinConsumo++;
                }

                bool listar = (!tienePunto || filtro.IncluirConPunto) && (sugerido.HasValue || filtro.MostrarSinMovimiento);
                if (!listar) continue;

                float stock = (float)Math.Round(r.StockActual, 3);
                var fila = new PuntoSugeridoFilaVm
                {
                    IdCorte = r.IdCorte,
                    Codigo = r.Codigo,
                    Producto = r.Corte ?? "",
                    IdSucursal = r.IdSucursal,
                    Sucursal = r.Sucursal ?? "",
                    Pesable = r.Pesable,
                    Promedio = r.Promedio,
                    StockActual = stock,
                    PuntoActual = r.PuntoStock,
                    ConsumoVentana = consumoVentana,
                    ConsumoDiario = consumoDiario,
                    DiasConVenta = diasConVenta,
                    CoberturaDias = consumoDiario > 0f ? (float?)(Math.Max(stock, 0f) / consumoDiario) : null,
                    UltimaSalida = ultimaSalida,
                    DiasSinSalida = ultimaSalida.HasValue ? (int?)Math.Max(0, (int)(ahora.Date - ultimaSalida.Value.Date).TotalDays) : null,
                    PuntoSugerido = sugerido
                };

                ProveedorHabitualVm prov;
                if (proveedorPor.TryGetValue(r.IdCorte, out prov))
                {
                    fila.IdProveedor = prov.IdProveedor;
                    fila.Proveedor = string.IsNullOrWhiteSpace(prov.Proveedor) ? "" : prov.Proveedor.Trim();
                }
                resultado.Filas.Add(fila);
            }

            // Lo que más se mueve primero (es donde más importa tener el punto bien), dentro de cada sucursal.
            resultado.Filas = resultado.Filas
                .OrderBy(x => x.Sucursal, StringComparer.CurrentCultureIgnoreCase)
                .ThenByDescending(x => x.ConsumoDiario)
                .ThenBy(x => x.Producto, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
            return resultado;
        }

        // Punto de stock que cubre `dias` días de consumo: consumo x días, hacia arriba, mínimo 1 y con tope MaxPuntoStock.
        // Entero porque el punto de stock se guarda como entero (en kg para pesables, en unidades para el resto).
        public static int SugerirPunto(float consumoDiario, int dias)
        {
            double bruto = Math.Round((double)consumoDiario * dias, 3);
            double techo = Math.Ceiling(bruto);
            if (techo < 1) techo = 1;
            if (techo > MaxPuntoStock) techo = MaxPuntoStock;
            return (int)techo;
        }

        // Separa los puntos que se pueden guardar de los que no, contra el stock real (plano = existencia de todas las sucursales).
        // Es la defensa del lado servidor: lo que llega de la pantalla es input externo. Rechaza (y explica en `rechazados`):
        //  - producto/sucursal que no llevan stock; - repetidos en el mismo pedido; - valores fuera de 1..MaxPuntoStock;
        //  - productos que YA tienen punto de stock (nunca se pisa un punto cargado por el usuario).
        // Procesa como mucho MaxPuntosPorGuardado. Devuelve los aceptados.
        public static List<PuntoAGuardarVm> ValidarPuntosAGuardar(
            IList<ExistenciaStockPorSucursalPlanoVm> plano, IEnumerable<PuntoAGuardarVm> pedidos, IList<string> rechazados)
        {
            var aceptados = new List<PuntoAGuardarVm>();
            var lista = (pedidos ?? new PuntoAGuardarVm[0]).Where(p => p != null).ToList();

            var existencia = new Dictionary<Tuple<int, int>, ExistenciaStockPorSucursalPlanoVm>();
            foreach (var f in plano ?? new List<ExistenciaStockPorSucursalPlanoVm>())
            {
                var clave = Tuple.Create(f.IdCorte, f.IdSucursal);
                if (!existencia.ContainsKey(clave)) existencia[clave] = f;
            }

            var vistos = new HashSet<Tuple<int, int>>();
            foreach (var p in lista.Take(MaxPuntosPorGuardado))
            {
                var clave = Tuple.Create(p.IdCorte, p.IdSucursal);
                ExistenciaStockPorSucursalPlanoVm fila;
                if (!existencia.TryGetValue(clave, out fila))
                {
                    rechazados.Add("Producto " + p.IdCorte + " en la sucursal " + p.IdSucursal + ": no lleva stock en esa sucursal.");
                    continue;
                }

                string nombre = (string.IsNullOrWhiteSpace(fila.Corte) ? "Producto " + p.IdCorte : fila.Corte.Trim()) + " (" + (fila.Sucursal ?? "") + ")";
                if (!vistos.Add(clave))
                {
                    rechazados.Add(nombre + ": figura repetido en el pedido.");
                    continue;
                }
                if (p.PuntoStock < 1 || p.PuntoStock > MaxPuntoStock)
                {
                    rechazados.Add(nombre + ": el punto de stock debe ser un número entero entre 1 y " + MaxPuntoStock + ".");
                    continue;
                }
                if (fila.PuntoStock > 0f)
                {
                    rechazados.Add(nombre + ": ya tiene punto de stock cargado ("
                        + fila.PuntoStock.ToString("0.###", CultureInfo.InvariantCulture) + "); no se pisa.");
                    continue;
                }
                aceptados.Add(p);
            }

            if (lista.Count > MaxPuntosPorGuardado)
                rechazados.Add("Se recibieron " + lista.Count + " puntos; solo se procesan los primeros " + MaxPuntosPorGuardado + ".");
            return aceptados;
        }

        // Redondeo hacia arriba: pesables a 0,1 (no se pide 12,347 kg); por unidad, al entero.
        // El Round(.., 3) previo es una tolerancia a propósito: un float como 14,3 es en realidad 14,3000002, y sin ella
        // un valor que ya es múltiplo del paso se subiría un paso de más (14,3 -> 14,4). 3 decimales = los que se ven.
        public static float RedondearPedido(float cantidad, bool pesable)
        {
            if (cantidad <= 0f) return 0f;
            if (pesable) return (float)(Math.Ceiling(Math.Round(cantidad * 10.0, 3)) / 10.0);
            return (float)Math.Ceiling(Math.Round((double)cantidad, 3));
        }

        // Un aviso por sucursal con la fecha de su último cierre de stock (de donde parte el cálculo).
        private static List<ReposicionAvisoCierreVm> ArmarAvisosCierre(IEnumerable<ExistenciaStockPorSucursalPlanoVm> filas, DateTime ahora)
        {
            var avisos = new List<ReposicionAvisoCierreVm>();
            foreach (var grupo in filas.GroupBy(x => x.IdSucursal).OrderBy(g => g.First().Sucursal))
            {
                DateTime? fecha = grupo.Max(x => x.FechaUltimoCierre);
                bool hayCierre = fecha.HasValue && fecha.Value >= FechaSinCierre;
                int? dias = hayCierre ? (int?)Math.Max(0, (int)(ahora.Date - fecha.Value.Date).TotalDays) : null;
                avisos.Add(new ReposicionAvisoCierreVm
                {
                    IdSucursal = grupo.Key,
                    Sucursal = grupo.First().Sucursal ?? "",
                    FechaUltimoCierre = hayCierre ? fecha : null,
                    DiasDesdeCierre = dias,
                    // Sin ningún cierre también es motivo de aviso: el stock no tiene punto de partida.
                    Viejo = !hayCierre || dias.Value > DiasCierreViejo
                });
            }
            return avisos;
        }

        private static DateTime? MaxFecha(DateTime? a, DateTime? b)
        {
            if (!a.HasValue) return b;
            if (!b.HasValue) return a;
            return a.Value >= b.Value ? a : b;
        }
    }
}
