// Pedidos de compra armados desde Stock > Existencia por sucursales ("Ingresar pedido"): alta (con idempotencia), modificacion con
// historial y listado. Solo Postgres. Las reglas son puras y estan separadas de la base para poder probarlas (Negocio.Tests/PedidoTests.cs):
// Armar* valida y calcula sin persistir; Guardar/Modificar persisten cabecera + lineas + historial en UNA transaccion.
// Reglas y porques: docs/DECISIONS.md (2026-10-19, Pedidos).
//
// Reglas principales:
//  - Se guardan SOLO las lineas con cantidad > 0; sin ninguna linea no hay pedido. Una linea es un par producto x sucursal (sin repetir).
//  - El nombre/codigo del producto y de la sucursal salen del catalogo del servidor, no de lo que manda el navegador (que solo aporta
//    ids, cantidades y el contexto informativo: precio de ultima compra, stock y punto).
//  - Importe = cantidad x precio de la ultima compra, redondeado a 2 decimales, SOLO si hay precio; una linea sin precio no suma al total y
//    se cuenta en LineasSinPrecio (nunca vale $0 en silencio, CLAUDE.md 2.7). Es una estimacion informativa.
//  - Al modificar, las lineas que se conservan mantienen su precio y su contexto guardados; solo cambia la cantidad.
//  - Si quien modifica es un usuario de PRODUCCION (cuenta compartida), hace falta un autorizador (otra persona con su clave, validada
//    en el controlador); queda en el historial.
//  - Modificar usa control optimista (Version): si otra persona modifico el pedido mientras se editaba, se rechaza.
//
// Sintaxis a proposito conservadora (C# 7.3): Negocio tambien compila para net472.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Negocio
{
    public class Pedido
    {
        // Topes defensivos contra entradas absurdas (input externo).
        public const int MaxLineas = 5000;
        public const int MaxLargoNota = 500;
        public const decimal MaxCantidad = 1000000000m;

        private readonly Contratos.IPedidoRepository _repo;

        public Pedido(Contratos.IPedidoRepository repositorio)
        {
            _repo = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        // ---------- Lo que manda el navegador ----------

        public class LineaSolicitada
        {
            public int IdCorte { get; set; }
            public int IdSucursal { get; set; }
            public decimal Cantidad { get; set; }

            // Contexto informativo del momento (opcional). El precio es una estimacion provista por la pantalla.
            public decimal? PrecioUnitario { get; set; }
            public string Proveedor { get; set; }
            public DateTime? FechaPrecio { get; set; }
            public decimal? StockActual { get; set; }
            public decimal? PuntoStock { get; set; }
        }

        public class Solicitud
        {
            // Id por pedido en curso para que reintentar el guardado no duplique (solo alta).
            public Guid? ClientId { get; set; }
            public string Nota { get; set; }
            // Solo modificacion: version del pedido que se estaba editando (Pedido.Version al abrirlo).
            public long? Version { get; set; }
            public List<LineaSolicitada> Lineas { get; set; } = new List<LineaSolicitada>();
        }

        public class Resultado
        {
            public bool Ok { get; set; }
            public string Error { get; set; }
            public int IdPedido { get; set; }
            // Alta repetida con el mismo ClientId: devuelve el pedido que ya estaba, sin crear otro.
            public bool YaExistia { get; set; }
            public Entidades.Pedido Pedido { get; set; }

            public static Resultado Fallo(string mensaje)
            {
                return new Resultado { Ok = false, Error = mensaje };
            }
        }

        // Version para el control optimista: cambia cada vez que se guarda una modificacion. Va en MILISEGUNDOS y no en Ticks (~6,4e17)
        // porque la pantalla la lee con parseInt de JavaScript, que solo representa enteros exactos hasta 2^53 (~9,0e15): con Ticks se
        // redondeaba y el servidor rechazaba todo guardado como "modificado por otra persona". En ms vale ~6,4e13.
        public static long Version(Entidades.Pedido pedido)
        {
            if (pedido == null) return 0;
            return (pedido.Modificado ?? pedido.Creado ?? pedido.Fecha).Ticks / TimeSpan.TicksPerMillisecond;
        }

        // ---------- Armado y validacion (puro) ----------

        // Valida la solicitud y arma el pedido (lineas, importes y totales) sin tocar la base. Devuelve null y deja el motivo en `error`
        // si no es valido. existentes: lineas del pedido que se esta modificando (por producto y sucursal); las que se conservan mantienen
        // su snapshot (precio, proveedor, contexto) y solo toman la cantidad nueva.
        public static Entidades.Pedido Armar(
            Solicitud solicitud,
            IEnumerable<Entidades.Corte> catalogo,
            IEnumerable<Entidades.Sucursal> sucursales,
            IDictionary<string, Entidades.PedidoLinea> existentes,
            out string error)
        {
            error = null;
            if (solicitud == null) { error = "Pedido inválido."; return null; }

            string nota = (solicitud.Nota ?? "").Trim();
            if (nota.Length > MaxLargoNota)
            {
                error = "La nota no puede tener más de " + MaxLargoNota + " caracteres.";
                return null;
            }

            var lineasSolicitadas = solicitud.Lineas ?? new List<LineaSolicitada>();
            if (lineasSolicitadas.Count > MaxLineas)
            {
                error = "El pedido tiene demasiadas líneas (máximo " + MaxLineas + ").";
                return null;
            }

            var productos = new Dictionary<int, Entidades.Corte>();
            foreach (var corte in catalogo ?? Enumerable.Empty<Entidades.Corte>())
            {
                if (corte != null && !productos.ContainsKey(corte.IdCorte)) productos[corte.IdCorte] = corte;
            }

            var nombresSucursal = new Dictionary<int, string>();
            foreach (var sucursal in sucursales ?? Enumerable.Empty<Entidades.Sucursal>())
            {
                if (sucursal != null && !nombresSucursal.ContainsKey(sucursal.IdSucursal)) nombresSucursal[sucursal.IdSucursal] = sucursal.SucursalNombre ?? "";
            }

            var pedido = new Entidades.Pedido { Nota = nota };
            var vistas = new HashSet<string>();

            foreach (var solicitada in lineasSolicitadas)
            {
                if (solicitada == null) continue;
                // Lo que quedo sin cantidad (o en 0) se descarta: el pedido lleva solo lo que se pidio.
                if (solicitada.Cantidad <= 0m) continue;

                if (solicitada.Cantidad > MaxCantidad)
                {
                    error = "Hay una cantidad demasiado grande.";
                    return null;
                }

                Entidades.Corte producto;
                if (!productos.TryGetValue(solicitada.IdCorte, out producto))
                {
                    error = "Hay un producto que no existe (id " + solicitada.IdCorte + ").";
                    return null;
                }

                string nombreSucursal;
                if (!nombresSucursal.TryGetValue(solicitada.IdSucursal, out nombreSucursal))
                {
                    error = "Hay una sucursal que no existe (id " + solicitada.IdSucursal + ").";
                    return null;
                }

                string clave = Clave(solicitada.IdCorte, solicitada.IdSucursal);
                if (!vistas.Add(clave))
                {
                    error = "El producto \"" + producto.CorteDesc + "\" está repetido para la misma sucursal.";
                    return null;
                }

                decimal cantidad = Math.Round(solicitada.Cantidad, 3, MidpointRounding.AwayFromZero);
                if (cantidad <= 0m) continue; // 0,0004 se redondea a 0: no es una cantidad pedida

                Entidades.PedidoLinea guardada = null;
                if (existentes != null) existentes.TryGetValue(clave, out guardada);

                var linea = new Entidades.PedidoLinea
                {
                    IdCorte = solicitada.IdCorte,
                    IdSucursal = solicitada.IdSucursal,
                    Cantidad = cantidad
                };

                if (guardada != null)
                {
                    // Linea que se conserva: mismo snapshot (el nombre/codigo y el precio de cuando se guardo), solo cambia la cantidad.
                    linea.Codigo = guardada.Codigo;
                    linea.Producto = guardada.Producto;
                    linea.Sucursal = guardada.Sucursal;
                    linea.Pesable = guardada.Pesable;
                    linea.Promedio = guardada.Promedio;
                    linea.StockActual = guardada.StockActual;
                    linea.PuntoStock = guardada.PuntoStock;
                    linea.PrecioUnitario = guardada.PrecioUnitario;
                    linea.Proveedor = guardada.Proveedor;
                    linea.FechaPrecio = guardada.FechaPrecio;
                }
                else
                {
                    linea.Codigo = producto.Codigo;
                    linea.Producto = producto.CorteDesc ?? "";
                    linea.Sucursal = nombreSucursal;
                    linea.Pesable = producto.Pesable;
                    linea.Promedio = producto.Pesable && producto.Promedio > 0f ? (decimal?)Math.Round((decimal)producto.Promedio, 3) : null;
                    linea.StockActual = Finito(solicitada.StockActual);
                    linea.PuntoStock = Finito(solicitada.PuntoStock);
                    linea.PrecioUnitario = solicitada.PrecioUnitario.HasValue && solicitada.PrecioUnitario.Value > 0m ? solicitada.PrecioUnitario : null;
                    linea.Proveedor = string.IsNullOrWhiteSpace(solicitada.Proveedor) || linea.PrecioUnitario == null ? null : solicitada.Proveedor.Trim();
                    linea.FechaPrecio = linea.PrecioUnitario == null ? null : solicitada.FechaPrecio;
                }

                linea.Importe = linea.PrecioUnitario.HasValue
                    ? (decimal?)Math.Round(linea.Cantidad * linea.PrecioUnitario.Value, 2, MidpointRounding.AwayFromZero)
                    : null;
                pedido.Lineas.Add(linea);
            }

            if (pedido.Lineas.Count == 0)
            {
                error = "El pedido no tiene ninguna cantidad cargada.";
                return null;
            }

            // Mismo orden que en pantalla: por codigo y, dentro del producto, por sucursal.
            pedido.Lineas = pedido.Lineas.OrderBy(l => l.Codigo).ThenBy(l => l.Producto, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(l => l.Sucursal, StringComparer.CurrentCultureIgnoreCase).ToList();

            pedido.TotalEstimado = pedido.Lineas.Where(l => l.Importe.HasValue).Sum(l => l.Importe.Value);
            pedido.LineasSinPrecio = pedido.Lineas.Count(l => !l.Importe.HasValue);
            return pedido;
        }

        private static string Clave(int idCorte, int idSucursal)
        {
            return idCorte.ToString(CultureInfo.InvariantCulture) + "|" + idSucursal.ToString(CultureInfo.InvariantCulture);
        }

        // Los datos de contexto son informativos: valores absurdos se descartan, no rechazan el pedido.
        private static decimal? Finito(decimal? valor)
        {
            if (!valor.HasValue) return null;
            return Math.Abs(valor.Value) <= MaxCantidad ? (decimal?)Math.Round(valor.Value, 3, MidpointRounding.AwayFromZero) : null;
        }

        // ---------- Alta ----------

        public Resultado Guardar(Solicitud solicitud, IEnumerable<Entidades.Corte> catalogo, IEnumerable<Entidades.Sucursal> sucursales,
            Entidades.Usuario usuario, DateTime ahora)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            // Idempotencia: el mismo pedido en curso (mismo ClientId) ya guardado devuelve ese, no crea otro.
            if (solicitud != null && solicitud.ClientId.HasValue)
            {
                int? existente = _repo.ObtenerIdPorClientId(solicitud.ClientId.Value);
                if (existente.HasValue) return new Resultado { Ok = true, IdPedido = existente.Value, YaExistia = true };
            }

            string error;
            var pedido = Armar(solicitud, catalogo, sucursales, null, out error);
            if (pedido == null) return Resultado.Fallo(error);

            pedido.ClientId = solicitud.ClientId;
            pedido.Fecha = ahora;
            pedido.IdUsuario = usuario.Id;
            pedido.Usuario = usuario.Nombre ?? "";

            try
            {
                using (var uow = _repo.IniciarUnitOfWork())
                {
                    _repo.Agregar(pedido, uow);
                    _repo.AgregarEvento(new Entidades.PedidoEvento
                    {
                        IdPedido = pedido.Id,
                        Fecha = ahora,
                        Tipo = Entidades.PedidoEvento.TipoAlta,
                        IdUsuario = usuario.Id,
                        Usuario = usuario.Nombre ?? "",
                        Detalle = TextoAlta(pedido)
                    }, uow);
                    uow.Completar();
                }
            }
            catch (Exception)
            {
                // Dos guardados casi simultaneos del mismo pedido en curso: el indice unico de ClientId rechaza el segundo. Si el pedido
                // ya esta, no es un error para el usuario.
                if (solicitud.ClientId.HasValue)
                {
                    int? existente = _repo.ObtenerIdPorClientId(solicitud.ClientId.Value);
                    if (existente.HasValue) return new Resultado { Ok = true, IdPedido = existente.Value, YaExistia = true };
                }
                throw;
            }

            return new Resultado { Ok = true, IdPedido = pedido.Id, Pedido = pedido };
        }

        private static string TextoAlta(Entidades.Pedido pedido)
        {
            return Contar(pedido.Lineas.Count, "línea", "líneas") + ", " + Contar(pedido.Lineas.Select(l => l.IdCorte).Distinct().Count(), "producto", "productos")
                + ", total estimado " + Dinero(pedido.TotalEstimado)
                + (pedido.LineasSinPrecio > 0 ? " (" + Contar(pedido.LineasSinPrecio, "línea", "líneas") + " sin precio)" : "");
        }

        // "1 línea" / "3 líneas": para que los textos del pedido (historial, ticket, PDF, Excel, mail) no digan "1 líneas".
        public static string Contar(int cantidad, string singular, string plural)
        {
            return cantidad + " " + (cantidad == 1 ? singular : plural);
        }

        // Concordancia del verbo de las líneas sin precio de compra: "1 línea ... no suma" / "3 líneas ... no suman".
        public static string NoSuman(int cantidadLineas)
        {
            return cantidadLineas == 1 ? "no suma" : "no suman";
        }

        // ---------- Modificacion ----------

        // autorizador: otra persona que autorizo con su clave (solo hace falta si `usuario` es de produccion). Null si no hizo falta.
        public Resultado Modificar(int idPedido, Solicitud solicitud, IEnumerable<Entidades.Corte> catalogo, IEnumerable<Entidades.Sucursal> sucursales,
            Entidades.Usuario usuario, Entidades.Usuario autorizador, DateTime ahora)
        {
            if (usuario == null) throw new ArgumentNullException(nameof(usuario));

            if (usuario.EsUsuarioProduccion && autorizador == null)
                return Resultado.Fallo("Para modificar un pedido hace falta el usuario y la contraseña de una persona autorizada.");

            var actual = _repo.ObtenerPorId(idPedido);
            if (actual == null) return Resultado.Fallo("No se encontró el pedido.");

            if (solicitud != null && solicitud.Version.HasValue && solicitud.Version.Value != Version(actual))
                return Resultado.Fallo("El pedido fue modificado por otra persona mientras lo editabas. Volvé a abrirlo para ver los cambios.");

            var existentes = new Dictionary<string, Entidades.PedidoLinea>();
            foreach (var linea in actual.Lineas) existentes[Clave(linea.IdCorte, linea.IdSucursal)] = linea;

            string error;
            var nuevo = Armar(solicitud, catalogo, sucursales, existentes, out error);
            if (nuevo == null) return Resultado.Fallo(error);

            string cambios = DescribirCambios(actual, nuevo);
            if (cambios.Length == 0) return Resultado.Fallo("No hay cambios para guardar.");

            nuevo.Id = actual.Id;
            nuevo.ModificadoPor = usuario.Id;

            using (var uow = _repo.IniciarUnitOfWork())
            {
                _repo.Actualizar(nuevo, uow);
                _repo.AgregarEvento(new Entidades.PedidoEvento
                {
                    IdPedido = actual.Id,
                    Fecha = ahora,
                    Tipo = Entidades.PedidoEvento.TipoModificacion,
                    IdUsuario = usuario.Id,
                    Usuario = usuario.Nombre ?? "",
                    IdAutorizador = autorizador != null ? (int?)autorizador.Id : null,
                    Autorizador = autorizador != null ? autorizador.Nombre : null,
                    Detalle = cambios
                }, uow);
                uow.Completar();
            }

            return new Resultado { Ok = true, IdPedido = actual.Id, Pedido = nuevo };
        }

        // Texto con lo que cambio entre el pedido guardado y el nuevo (para el historial). Vacio = no hay cambios.
        public static string DescribirCambios(Entidades.Pedido anterior, Entidades.Pedido nuevo)
        {
            const int MaxRenglones = 60;
            const int MaxLargo = 4000;

            var partes = new List<string>();

            if (!string.Equals((anterior.Nota ?? "").Trim(), (nuevo.Nota ?? "").Trim(), StringComparison.Ordinal))
                partes.Add("Nota: \"" + (anterior.Nota ?? "") + "\" → \"" + (nuevo.Nota ?? "") + "\"");

            var viejas = anterior.Lineas.ToDictionary(l => Clave(l.IdCorte, l.IdSucursal));
            var nuevas = nuevo.Lineas.ToDictionary(l => Clave(l.IdCorte, l.IdSucursal));

            foreach (var linea in nuevo.Lineas)
            {
                Entidades.PedidoLinea vieja;
                if (!viejas.TryGetValue(Clave(linea.IdCorte, linea.IdSucursal), out vieja))
                    partes.Add("Se agregó " + Nombre(linea) + ": " + Cantidad(linea.Cantidad));
                else if (vieja.Cantidad != linea.Cantidad)
                    partes.Add("Cambió " + Nombre(linea) + ": " + Cantidad(vieja.Cantidad) + " → " + Cantidad(linea.Cantidad));
            }

            foreach (var vieja in anterior.Lineas)
            {
                if (!nuevas.ContainsKey(Clave(vieja.IdCorte, vieja.IdSucursal)))
                    partes.Add("Se quitó " + Nombre(vieja) + ": " + Cantidad(vieja.Cantidad));
            }

            if (partes.Count == 0) return "";

            var texto = new StringBuilder();
            for (int i = 0; i < partes.Count && i < MaxRenglones; i++)
            {
                if (texto.Length > 0) texto.Append("\n");
                texto.Append(partes[i]);
            }
            if (partes.Count > MaxRenglones) texto.Append("\n… y " + (partes.Count - MaxRenglones) + " cambios más");
            texto.Append("\nTotal estimado: " + Dinero(anterior.TotalEstimado) + " → " + Dinero(nuevo.TotalEstimado));

            string resultado = texto.ToString();
            return resultado.Length > MaxLargo ? resultado.Substring(0, MaxLargo - 1) + "…" : resultado;
        }

        private static string Nombre(Entidades.PedidoLinea linea)
        {
            return linea.Producto + " (" + linea.Sucursal + ")";
        }

        private static string Cantidad(decimal valor)
        {
            return valor.ToString("0.###", CultureInfo.GetCultureInfo("es-AR"));
        }

        private static string Dinero(decimal valor)
        {
            return "$ " + valor.ToString("N2", CultureInfo.GetCultureInfo("es-AR"));
        }

        // ---------- Lectura ----------

        public Entidades.Pedido ObtenerPorId(int idPedido)
        {
            return _repo.ObtenerPorId(idPedido);
        }

        public List<Entidades.PedidoResumen> Listar(Entidades.PedidoFiltro filtro)
        {
            return _repo.Listar(filtro) ?? new List<Entidades.PedidoResumen>();
        }
    }
}
