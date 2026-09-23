using System.Globalization;

namespace WebCore.Services
{
    // Extraido de ActividadesController.cs (2026-09-10, item 4 de la segunda ronda de pedidos --
    // ver docs/DECISIONS.md "Batch 8: Actividades en el dashboard"). Refactor de extraccion pura
    // (sin cambio de comportamiento, verificado con Playwright que /Actividades sigue mostrando
    // exactamente los mismos items antes/despues): junta las 7 fuentes heterogeneas (cambio de
    // precio, venta anulada, venta con bonificacion manual, egreso de caja, movimiento, compra,
    // formula) en una sola lista ordenada por fecha. Reusado por ActividadesController (pantalla
    // completa) y HomeController (bloque "Actividades" del dashboard, ultimas 10).
    //
    // Batch 9 de la quinta ronda (2026-09-10, ver docs/DECISIONS.md): para venta anulada/venta
    // bonificada/movimiento/compra, ademas de la fecha ya calcula OrigenFecha ("creación"/
    // "modificación") y EsAnomalia (>1 dia de diferencia con la fecha de negocio real del
    // registro) -- ver ResolverFechaMostrada mas abajo. El fin es poder filtrar registros con
    // comportamiento extraño (cargados/editados con fecha muy distinta a cuando ocurrieron).
    public class ActividadesFeedService
    {
        private readonly Utilidades.IEmpresaContext _empresa;
        private readonly Utilidades.IParametrosContext? _parametros;

        public ActividadesFeedService(Utilidades.IEmpresaContext empresa, Utilidades.IParametrosContext? parametros = null)
        {
            _empresa = empresa;
            _parametros = parametros;
        }

        // urlDetalle (opcional): arma la URL del detalle de una venta en curso / advertencia (accion, valores)
        // -> URL. Lo pasa el controller con Url.Action; sin el, los items no llevan boton "Ver detalle".
        public List<Models.ActividadItemVm> ObtenerActividades(DateTime desdeConHora, DateTime hastaConHora,
            Func<string, object, string?>? urlDetalle = null)
        {
            var repo = WebCore.Infrastructure.NegocioFactory.CrearActividadRepository(_empresa);
            var oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _parametros);

            var items = new List<Models.ActividadItemVm>();

            foreach (System.Data.DataRow row in repo.ObtenerCambiosPrecio(desdeConHora, hastaConHora).Rows)
            {
                decimal anterior = ToDecimal(row["precio_anterior"]);
                decimal nuevo = ToDecimal(row["precio_nuevo"]);
                double pct = anterior != 0 ? (double)((nuevo - anterior) / anterior * 100) : 0;
                string signo = pct >= 0 ? "" : "-";
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Precio",
                    Descripcion = $"Se modificó el precio de {row["corte"]} a $ {nuevo:N2}, {signo}{Math.Abs(pct):N1}% (precio anterior $ {anterior:N2})"
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerVentasConLineasAnuladas(desdeConHora, hastaConHora).Rows)
            {
                int idVenta = Convert.ToInt32(row["idventa"]);
                string vendedor = row["vendedor"] == DBNull.Value ? "" : Convert.ToString(row["vendedor"]) ?? "";
                var (fecha, origenFecha, esAnomalia, fechaNegocio) = ResolverFechaMostrada(row);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = fecha,
                    OrigenFecha = origenFecha,
                    EsAnomalia = esAnomalia,
                    FechaNegocio = fechaNegocio,
                    Tipo = "Venta anulada",
                    Descripcion = $"Hubo ítems anulados en la venta #{idVenta}" + (string.IsNullOrWhiteSpace(vendedor) ? "" : $" (vendedor: {vendedor})"),
                    IdVenta = idVenta
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerVentasConBonificacionManual(desdeConHora, hastaConHora).Rows)
            {
                int idVenta = Convert.ToInt32(row["idventa"]);
                string vendedor = row["vendedor"] == DBNull.Value ? "" : Convert.ToString(row["vendedor"]) ?? "";
                var (fecha, origenFecha, esAnomalia, fechaNegocio) = ResolverFechaMostrada(row);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = fecha,
                    OrigenFecha = origenFecha,
                    EsAnomalia = esAnomalia,
                    FechaNegocio = fechaNegocio,
                    Tipo = "Precio modificado en venta",
                    Descripcion = $"Se aplicó un descuento/recargo manual en la venta #{idVenta}" + (string.IsNullOrWhiteSpace(vendedor) ? "" : $" (vendedor: {vendedor})"),
                    IdVenta = idVenta
                });
            }

            // Egresos de caja que son gastos (idSucursal=0/idUsuario=-1/idTipoEgresoCaja=0 ->
            // trae todas las sucursales sin filtrar, ver DatosPostgres/CierreCajaPg.cs).
            var egresos = oCierreN.obtenerEgresosCaja(0, -1, 0, "", desdeConHora, hastaConHora);
            if (egresos != null)
            {
                foreach (System.Data.DataRow row in egresos.Rows)
                {
                    bool esGasto = row.Table.Columns.Contains("Gasto") && row["Gasto"] != DBNull.Value && Convert.ToBoolean(row["Gasto"]);
                    if (!esGasto) continue;

                    decimal monto = ToDecimal(row["Monto"]);
                    string desc = row["Descripcion"] == DBNull.Value ? "" : Convert.ToString(row["Descripcion"]) ?? "";
                    string tipo = row["TipoEgresoCaja"] == DBNull.Value ? "" : Convert.ToString(row["TipoEgresoCaja"]) ?? "";
                    items.Add(new Models.ActividadItemVm
                    {
                        Fecha = (DateTime)row["Fecha"],
                        Tipo = "Egreso de caja",
                        Descripcion = $"Egreso de caja ({tipo}): $ {monto:N2}" + (string.IsNullOrWhiteSpace(desc) ? "" : $" -- {desc}")
                    });
                }
            }

            foreach (System.Data.DataRow row in repo.ObtenerMovimientos(desdeConHora, hastaConHora).Rows)
            {
                string origen = row["sucursal_origen"] == DBNull.Value ? "?" : Convert.ToString(row["sucursal_origen"]) ?? "?";
                string destino = row["sucursal_destino"] == DBNull.Value ? "?" : Convert.ToString(row["sucursal_destino"]) ?? "?";
                string usuarioMov = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]) ?? "";
                var (fecha, origenFecha, esAnomalia, fechaNegocio) = ResolverFechaMostrada(row);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = fecha,
                    OrigenFecha = origenFecha,
                    EsAnomalia = esAnomalia,
                    FechaNegocio = fechaNegocio,
                    Tipo = "Movimiento",
                    Descripcion = $"Movimiento de stock de {origen} a {destino}" + (string.IsNullOrWhiteSpace(usuarioMov) ? "" : $" (por {usuarioMov})")
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerCompras(desdeConHora, hastaConHora).Rows)
            {
                // El sub-tipo real (Media Res/Cortes/Ingreso Stock/Egreso Stock/Cierre Stock/
                // Pesaje Cortes/Ajuste Stock -- Entidades.Compra.tipoCompraEnum) ya viaja tal cual
                // en la columna "tipocompra" (confirmado: CompraPg.cs filtra con ILIKE contra estos
                // mismos textos, no hay conversion de enum pendiente) -- antes solo se usaba en la
                // Descripcion, ahora tambien en el Tipo (2026-09-10, Batch 9 quinta ronda).
                string tipo = row["tipocompra"] == DBNull.Value ? "Compra" : Convert.ToString(row["tipocompra"]) ?? "Compra";
                string usuarioCompra = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]) ?? "";
                var (fecha, origenFecha, esAnomalia, fechaNegocio) = ResolverFechaMostrada(row);
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = fecha,
                    OrigenFecha = origenFecha,
                    EsAnomalia = esAnomalia,
                    FechaNegocio = fechaNegocio,
                    Tipo = string.IsNullOrWhiteSpace(tipo) ? "Compra/Stock" : tipo,
                    Descripcion = $"{tipo} #{row["idcompra"]}" + (string.IsNullOrWhiteSpace(usuarioCompra) ? "" : $" (por {usuarioCompra})")
                });
            }

            foreach (System.Data.DataRow row in repo.ObtenerFormulas(desdeConHora, hastaConHora).Rows)
            {
                string producto = row["producto"] == DBNull.Value ? $"#{row["idformula"]}" : Convert.ToString(row["producto"]) ?? "";
                string usuarioFormula = row["usuario"] == DBNull.Value ? "" : Convert.ToString(row["usuario"]) ?? "";
                items.Add(new Models.ActividadItemVm
                {
                    Fecha = (DateTime)row["fecha"],
                    Tipo = "Elaborado",
                    Descripcion = $"Se dio de alta o editó la fórmula de {producto}" + (string.IsNullOrWhiteSpace(usuarioFormula) ? "" : $" (por {usuarioFormula})")
                });
            }

            // Ventas en curso interrumpidas/descartadas/cortadas y productos pesados sin agregar (solo Postgres).
            if (WebCore.Helpers.PosBorradorSettings.Habilitado)
                AgregarActividadesPosBorrador(items, desdeConHora, hastaConHora, urlDetalle);

            return items.OrderByDescending(i => i.Fecha).ToList();
        }

        // Fuentes nuevas de "Ventas en curso: borrador en servidor y advertencias del POS" (ver
        // docs/DECISIONS.md): eventos de las ventas sin cerrar, ventas interrumpidas (sin senal) y productos
        // pesados que quedaron en pantalla sin agregarse. Las advertencias se marcan EsAdvertencia/EsAnomalia y
        // llevan la hora exacta del evento (no la de una venta). Una falla de estas consultas no debe romper la
        // pantalla de Actividades: se omiten y se deja el resto.
        private void AgregarActividadesPosBorrador(List<Models.ActividadItemVm> items, DateTime desde, DateTime hasta,
            Func<string, object, string?>? urlDetalle)
        {
            var es = new CultureInfo("es-AR");
            Negocio.VentaBorrador negocio;
            try
            {
                negocio = WebCore.Infrastructure.NegocioFactory.CrearVentaBorrador(_empresa);

                // 1) Eventos de las ventas en curso (cierre de pestaña, logout, cierre de caja, recuperada, descartada).
                foreach (var ev in negocio.ListarEventosPorRango(desde, hasta))
                {
                    string quien = string.IsNullOrWhiteSpace(ev.NombreOperador) ? "Un cajero" : ev.NombreOperador;
                    string detalle = string.IsNullOrWhiteSpace(ev.Detalle) ? "" : " — " + ev.Detalle;
                    items.Add(new Models.ActividadItemVm
                    {
                        Fecha = ev.Fecha,
                        Tipo = WebCore.Helpers.AdvertenciasTextos.TipoEvento(ev.Tipo),
                        Descripcion = quien + ": venta sin finalizar de $ " + ev.Total.ToString("N2", es) + " (" + ev.CantLineas + " ítem(s))" + detalle,
                        EsAdvertencia = ev.Tipo != Entidades.VentaBorradorEvento.TipoRecuperada,
                        EsAnomalia = ev.Tipo != Entidades.VentaBorradorEvento.TipoRecuperada,
                        DetalleUrl = urlDetalle?.Invoke("DetalleBorrador", new { id = ev.IdBorrador }) ?? ""
                    });
                }

                // 2) Ventas interrumpidas: siguen ACTIVAS pero sin senal (posible corte de luz o de red).
                foreach (var b in negocio.ListarInterrumpidasPorRango(desde, hasta, WebCore.Helpers.PosBorradorSettings.MinutosSinLatidoInterrumpida))
                {
                    if (b.CantLineas <= 0) continue;
                    string quien = string.IsNullOrWhiteSpace(b.NombreOperador) ? "Un cajero" : b.NombreOperador;
                    items.Add(new Models.ActividadItemVm
                    {
                        Fecha = b.UltimoLatido,
                        Tipo = "Venta en curso interrumpida",
                        Descripcion = quien + " dejó una venta sin finalizar de $ " + b.Total.ToString("N2", es) + " (" + b.CantLineas
                            + " ítem(s)); última señal " + b.UltimoLatido.ToString("dd/MM/yyyy HH:mm:ss", es) + " (posible corte de luz o de red)",
                        EsAdvertencia = true,
                        EsAnomalia = true,
                        DetalleUrl = urlDetalle?.Invoke("DetalleBorrador", new { id = b.Id }) ?? ""
                    });
                }

                // 3) Productos pesados que quedaron en pantalla y no se agregaron al carrito.
                if (WebCore.Helpers.PosBorradorSettings.AdvertenciaProductoSinAgregarHabilitada)
                {
                    foreach (var p in negocio.ListarProductoSinAgregarPorRango(desde, hasta))
                    {
                        string quien = string.IsNullOrWhiteSpace(p.NombreOperador) ? "Un cajero" : p.NombreOperador;
                        string salida = p.Motivo == Entidades.ProductoSinAgregar.MotivoCodigoBorrado ? "borrado" : "salió";
                        items.Add(new Models.ActividadItemVm
                        {
                            Fecha = p.Fin,
                            Tipo = "Producto pesado sin agregar",
                            Descripcion = quien + " — " + p.Producto + " " + p.CantidadKg.ToString("N3", es) + " kg ($ " + p.Importe.ToString("N2", es)
                                + ") · tipeado " + p.Inicio.ToString("dd/MM/yyyy HH:mm:ss", es) + " → " + salida + " " + p.Fin.ToString("HH:mm:ss", es)
                                + " (" + p.SegundosEnPantalla + " s en pantalla) · terminó: " + WebCore.Helpers.AdvertenciasTextos.Motivo(p.Motivo),
                            EsAdvertencia = true,
                            EsAnomalia = true,
                            Revision = WebCore.Helpers.AdvertenciasTextos.Revision(p.Revision),
                            DetalleUrl = urlDetalle?.Invoke("DetalleProductoSinAgregar",
                                new { idOperador = p.IdOperador, dia = p.Fin.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) }) ?? ""
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("Actividades: no se pudieron leer las advertencias del POS: " + ex.Message);
            }
        }

        // Batch 9 de la quinta ronda (2026-09-10, ver docs/DECISIONS.md): resuelve, para las 4
        // fuentes que tienen una fecha de negocio propia separada de creado/actualizado (ventas
        // anuladas/bonificadas, movimientos, compras), (a) que fecha mostrar (mismo criterio que
        // el COALESCE que antes vivia en SQL: actualizado > creado > fecha_negocio), (b) si esa
        // fecha viene de una creacion o una modificacion, y (c) si difiere de la fecha de negocio
        // real por mas de 1 dia -- señal de un registro cargado o editado fuera de tiempo. Precio/
        // Formula/Egreso de caja no llaman a este metodo (no tienen fecha de negocio separada del
        // todo, o no fue confirmado con el usuario incluirlos en la deteccion de anomalias).
        private static (DateTime fecha, string origenFecha, bool esAnomalia, DateTime fechaNegocio) ResolverFechaMostrada(System.Data.DataRow row)
        {
            DateTime fechaNegocio = (DateTime)row["fecha_negocio"];
            DateTime? creado = row["creado"] == DBNull.Value ? null : (DateTime?)row["creado"];
            DateTime? actualizado = row["actualizado"] == DBNull.Value ? null : (DateTime?)row["actualizado"];

            DateTime fecha = actualizado ?? creado ?? fechaNegocio;
            string origenFecha = actualizado.HasValue ? "modificación" : "creación";
            bool esAnomalia = Math.Abs((fecha - fechaNegocio).TotalDays) > 1;

            return (fecha, origenFecha, esAnomalia, fechaNegocio);
        }

        private static decimal ToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0m;
            return Convert.ToDecimal(value, CultureInfo.InvariantCulture);
        }
    }
}
