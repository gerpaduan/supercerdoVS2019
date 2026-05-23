(function (window, $) {
    'use strict';

    function createPOSCart(options) {
        const POSState = options.POSState;
        const soloFormaPago = options.soloFormaPago === true;
        const eliminarFisicoLineasPendientes = options.eliminarFisicoLineasPendientes === true;
        const puedeBonificar = options.puedeBonificar !== false;
        const mensajeSinPermisoBonificar = options.mensajeSinPermisoBonificar || "No tiene permisos para bonificar.";
        const deshabilitarAvisoSalida = options.deshabilitarAvisoSalida === true;

        function fnum(v) {
            return parseFloat(
                String(v ?? "")
                    .replace(/\$/g, "")
                    .trim()
                    .replace(",", ".")
            ) || 0.0;
        }

        function toFloatAR(v) {
            let s = String(v ?? "").trim();
            s = s.replace(/[^0-9,.\-]/g, "");

            const hasComma = s.includes(",");
            const hasDot = s.includes(".");

            if (hasComma && hasDot) {
                s = s.replace(/\./g, "").replace(",", ".");
            } else if (hasComma) {
                s = s.replace(",", ".");
            }

            const n = parseFloat(s);
            return isNaN(n) ? 0.0 : n;
        }

        function showMessage(icon, title, text) {
            if (window.Swal) {
                Swal.fire({ icon: icon, title: title, text: text });
                return;
            }

            alert(text || title);
        }

        const CANT_DECIMALES = 3;
        const CANT_MINIMA = 0.010;
        let hayVentaEnCurso = false;
        let omitirAvisoSalidaPOS = false;

        function roundCant(n) {
            const p = Math.pow(10, CANT_DECIMALES);
            return Math.round(n * p) / p;
        }

        function parseCant(v) {
            let s = String(v ?? "").trim();
            s = s.replace(/[^0-9.,]/g, "");
            if (s === "") return NaN;

            const m = s.match(/[.,]/);
            if (m) {
                const i = s.indexOf(m[0]);
                const intPart = s.slice(0, i).replace(/[.,]/g, "");
                const decPart = s.slice(i + 1).replace(/[.,]/g, "");
                s = intPart + "." + decPart;
            } else {
                s = s.replace(/[.,]/g, "");
            }

            const n = parseFloat(s);
            return isFinite(n) ? n : NaN;
        }

        function fmtCant(n) {
            n = roundCant(n);
            return n.toFixed(CANT_DECIMALES).replace(".", ",");
        }

        // Calcula el subtotal del producto actualmente seleccionado antes de
        // agregarlo al carrito.
        function calculateSubtotal() {
            const precioActual = options.getPrecioActual();
            let cant = parseCant($("#inputCantidad").val());

            if (isNaN(cant) || cant <= 0 || precioActual <= 0) {
                $("#prodSubtotal").text("$ 0,00");
                $("#btnAgregarProducto").prop("disabled", true);
                return;
            }

            const subtotal = cant * precioActual;
            $("#prodSubtotal").text(
                "$ " + subtotal.toLocaleString("es-AR", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                })
            );
            $("#btnAgregarProducto").prop("disabled", false);
        }

        // Recalcula el total usando POSState como fuente única de verdad.
        function recalculateTotal() {
            const total = POSState.getTotal();
            $("#lblSubtotal").text(
                "$ " + total.toLocaleString("es-AR", {
                    minimumFractionDigits: 2,
                    maximumFractionDigits: 2
                })
            );
            updateCartSummary();
        }

        function updateCartSummary() {
            const lineas = POSState.getLineas();
            const cargadas = lineas.filter(function (linea) { return !!linea; }).length;
            const activas = lineas.filter(function (linea) { return linea && !linea.anulado; }).length;
            const anuladas = lineas.filter(function (linea) { return linea && linea.anulado; }).length;
            const $resumen = $("#posVentaResumen");
            const $cancelar = $("#btnCancelarItem");

            if ($resumen.length) {
                if (!activas && !anuladas) {
                    $resumen.text("Sin productos");
                } else {
                    $resumen.text(activas + (activas === 1 ? " ítem activo" : " ítems activos") +
                        (!eliminarFisicoLineasPendientes && anuladas ? " | " + anuladas + (anuladas === 1 ? " anulado" : " anulados") : ""));
                }
            }

            if ($cancelar.length) {
                $cancelar
                    .prop("disabled", cargadas === 0)
                    .toggleClass("btn-outline-danger", cargadas > 0)
                    .toggleClass("btn-outline-secondary", cargadas === 0);
            }
            syncUIBonificacionTodos();
        }

        // Compatibilidad con scripts viejos como forma-pago.js
        window.obtenerTotalVenta = function obtenerTotalVenta() {
            return POSState.getTotal();
        };

        function addProduct() {
            if (soloFormaPago) return;
            const productoSeleccionado = options.getProductoSeleccionado();

            if (!productoSeleccionado) {
                options.focusCodigo();
                return;
            }

            let cantidad = parseCant($("#inputCantidad").val());
            if (isNaN(cantidad) || cantidad < CANT_MINIMA) {
                showMessage("warning", "Cantidad inválida", "La cantidad mínima es 0,010 kg.");
                return;
            }

            if (typeof options.beforeAddProduct === 'function') {
                const validation = options.beforeAddProduct(productoSeleccionado, cantidad);
                if (validation && validation.ok === false) {
                    showMessage("warning", "No se puede agregar", validation.message || "La línea no cumple las validaciones requeridas.");
                    return;
                }
            }

            const precioLinea = typeof options.getPrecioParaAgregar === 'function'
                ? options.getPrecioParaAgregar(productoSeleccionado)
                : productoSeleccionado.precioKg;
            const precioSeguro = Number.isFinite(precioLinea) ? precioLinea : productoSeleccionado.precioKg;
            const subtotal = cantidad * precioSeguro;
            if (!Number.isFinite(subtotal) || subtotal <= 0) {
                showMessage("warning", "No se puede agregar", "El producto no tiene precio cargado o el precio es 0.");
                options.focusCodigo();
                return;
            }

            const linea = {
                index: POSState.nextIndex(),
                idCorte: productoSeleccionado.id || productoSeleccionado.idCorte || 0,
                producto: productoSeleccionado.nombre,
                descripcion: productoSeleccionado.nombre,
                codigo: productoSeleccionado.codigo,
                cant: cantidad.toFixed(3),
                precio: `$ ${precioSeguro.toFixed(2)}`,
                precioOriginal: `$ ${productoSeleccionado.precioOriginal.toFixed(2)}`,
                subtotal: `$ ${(subtotal).toFixed(2)}`,
                bonificacion: 0,
                anulado: false,
                indexAnulado: -1,
                balanza: productoSeleccionado.balanza
            };

            POSState.addLinea(linea);
            renderTable(POSState.getLineas());
            recalculateTotal();
            options.beep();
            navigator.vibrate?.(80);

            requestAnimationFrame(function () {
                scrollTableToEnd();
                setTimeout(options.scrollPantallaMobile, 50);
            });

            $("#inputCodigo").val("").focus();
            $("#inputCantidad").val("");
            $("#prodSubtotal").text("$ 0,00");

            updateSaleState();
            options.setProductoSeleccionado(null);
            options.showWaiting();
        }

        // Pinta el contenido visual del carrito.
        function renderTable(lineas) {
            const $tbody = $("#tablaItems");
            $tbody.empty();

            if (!lineas || lineas.length === 0) {
                $tbody.append(`
                    <tr id="filaVacia">
                        <td class="text-muted text-center py-3">
                            No hay productos agregados
                        </td>
                    </tr>
                `);
                updateCartSummary();
                return;
            }

            lineas.forEach(function (l) {
                const claseAnulada = l.anulado ? "fila-anulada" : "";
                const bonificacion = l.bonificacion == 0
                    ? ""
                    : (l.bonificacion > 0
                        ? " | Bonif:" + l.bonificacion + "%"
                        : " | Recargo:" + (l.bonificacion * -1) + "%");
                const detalleExpendio = (parseInt(l.idExpendio, 10) || 0) > 0
                    ? `<div class="text-muted small">&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Exp.Nro: ${l.idExpendio}</div>`
                    : "";

                $tbody.append(`
                    <tr data-id="${l.index}" class="fila-item ${claseAnulada}">
                        <td>
                            <div class="fw-bold">
                                #${l.index} <strong>${l.producto}</strong> (cod: ${l.codigo})
                            </div>
                            ${detalleExpendio}
                            <div class="d-flex justify-content-between item-detalle">
                                <span>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;${l.cant} x ${l.precio} ${bonificacion}</span>
                                <span class="fw-bold">${l.subtotal}</span>
                            </div>
                        </td>
                    </tr>
                `);
            });

            $tbody.find("tr.fila-item").removeClass("fila-ultimo");
            $tbody.find("tr.fila-item:last").addClass("fila-ultimo");
            updateCartSummary();
        }

        function scrollTableToEnd() {
            const isDesktop = window.matchMedia("(min-width: 992px)").matches;
            const zonaTabla = document.querySelector(".carrito-col .zona-tabla");
            const tbody = document.getElementById("tablaItems");
            if (!tbody) return;

            const ultimaFila = tbody.querySelector("tr.fila-item:last-child");
            if (!ultimaFila) return;

            tbody.querySelectorAll("tr.fila-item").forEach(function (tr) {
                tr.classList.remove("fila-ultimo", "pos-last-added");
            });
            ultimaFila.classList.add("fila-ultimo", "pos-last-added");

            ultimaFila.tabIndex = -1;
            try { ultimaFila.focus({ preventScroll: true }); } catch { try { ultimaFila.focus(); } catch { } }

            const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
            const behavior = reduceMotion ? "auto" : "smooth";

            if (isDesktop && zonaTabla) {
                zonaTabla.scrollTo({ top: zonaTabla.scrollHeight, behavior: behavior });
            } else {
                ultimaFila.scrollIntoView({ behavior: behavior, block: "start" });
            }

            setTimeout(function () {
                ultimaFila.classList.remove("pos-last-added");
            }, 900);

            options.focusCodigo();
        }

        function scrollLineVisible(idLinea) {
            const fila = document.querySelector(`#tablaItems tr.fila-item[data-id="${idLinea}"]`);
            if (!fila) return;

            const isDesktop = window.matchMedia("(min-width: 992px)").matches;
            const zonaTabla = document.querySelector(".carrito-col .zona-tabla");

            fila.tabIndex = -1;
            try { fila.focus({ preventScroll: true }); } catch { }

            if (isDesktop && zonaTabla) fila.scrollIntoView({ block: "nearest" });
            else fila.scrollIntoView({ behavior: "smooth", block: "start" });
        }

        function updateSaleState() {
            const lineas = POSState.getLineas();
            hayVentaEnCurso = lineas.some(function (linea) {
                return !!linea;
            });
        }

        window.desactivarAvisoSalidaPOS = function () {
            omitirAvisoSalidaPOS = true;
            hayVentaEnCurso = false;
        };

        window.activarAvisoSalidaPOS = function () {
            omitirAvisoSalidaPOS = false;
            updateSaleState();
        };

        window.addEventListener("beforeunload", function (e) {
            if (deshabilitarAvisoSalida) return;

            updateSaleState();

            if (omitirAvisoSalidaPOS || window.POSFinalizandoVenta || (!hayVentaEnCurso && !window.POSCancelacionEnCurso)) return;

            window.POSDraft?.save?.();
            e.preventDefault();
            e.returnValue = 'Debe presionar "Cancelar Venta".';
        });

        function precioLineaAplicadoNum() {
            let p = fnum(window.lineaSeleccionada?.precio);
            if (p > 0) return p;

            p = fnum(window.lineaSeleccionada?.precioOriginal ?? 0);
            if (p > 0) return p;

            return fnum($("#modalPrecio").val());
        }

        function previewSubtotalDesdeCantidad() {
            if (!window.lineaSeleccionada) return;

            const raw = String($("#modalCantidad").val() ?? "").trim();
            if (raw === "") {
                $("#modalSubtotal").val("");
                return;
            }

            const cant = parseCant(raw);
            if (!isFinite(cant) || cant < 0) {
                $("#modalSubtotal").val("");
                return;
            }

            if ($("#bloqueBonificar").is(":visible")) {
                if ($("#chkPorcentaje").is(":checked")) syncDesdePorcentaje();
                else syncDesdePrecio();
                return;
            }

            const subtotal = precioLineaAplicadoNum() * cant;
            $("#modalSubtotal").val(subtotal.toFixed(2));
        }

        function setModoBonificacion(porcentajeMode) {
            $("#chkPorcentaje").prop("checked", porcentajeMode);
            $("#txtPorcentaje").prop("readonly", !porcentajeMode);
            $("#txtPrecioKg").prop("readonly", porcentajeMode);

            if (porcentajeMode) {
                $("#lblTipoPorcentaje").text("% bonificación (- para recargo)");
                requestAnimationFrame(function () { $("#txtPorcentaje").focus().select(); });
                syncDesdePorcentaje();
            } else {
                $("#lblTipoPorcentaje").text("% bonificación");
                requestAnimationFrame(function () { $("#txtPrecioKg").focus().select(); });
                syncDesdePrecio();
            }
            syncUIBonificacionTodos();
        }

        function syncUIBonificacionTodos() {
            const porcentajeMode = $("#chkPorcentaje").is(":checked");
            const aplicarTodos = porcentajeMode && $("#chkBonificarTodos").is(":checked");

            $("#bloqueAplicarBonificacionTodos").toggleClass("d-none", !porcentajeMode);
            $("#msgRecargoBonificacion").toggleClass("d-none", !porcentajeMode);
            $("#chkBonificarTodos").prop("disabled", !porcentajeMode);

            if (!porcentajeMode) {
                $("#chkBonificarTodos").prop("checked", false);
            }

            $("#msgBonificarTodos").toggleClass("d-none", !aplicarTodos);
            $("#modalProducto").toggleClass("d-none", aplicarTodos);
            $("#bloqueDatosLineaBonificacion").toggleClass("d-none", aplicarTodos);
            $("#bloquePrecioBonificado").toggleClass("d-none", aplicarTodos);
        }

        function aplicarBonificacionALinea(linea, cantidad, precioNuevo, porcentaje) {
            if (!linea) return false;

            const cant = roundCant(parseCant(cantidad));
            const precio = fnum(precioNuevo);
            const pct = fnum(porcentaje);

            if (!isFinite(cant) || cant < CANT_MINIMA) return false;
            if (!precio || precio <= 0) return false;

            linea.cant = cant;
            linea.precio = `$ ${precio.toFixed(2)}`;
            linea.subtotal = `$ ${(precio * cant).toFixed(2)}`;
            linea.bonificacion = pct;
            return true;
        }

        function aplicarBonificacionPorcentajeALinea(linea, porcentaje) {
            if (!linea) return false;

            const precioLista = fnum(linea.precioOriginal ?? linea.precio);
            const cant = parseCant(linea.cant);
            const pct = fnum(porcentaje);

            if (!precioLista || !isFinite(cant) || cant < CANT_MINIMA) return false;

            const precioNuevo = precioLista * (1 - (pct / 100.0));
            return aplicarBonificacionALinea(linea, cant, precioNuevo, pct);
        }

        function aplicarBonificacionPorcentajeATodoElCarrito(porcentaje) {
            const lineas = (POSState.getLineas() || []).filter(function (linea) {
                return linea && !linea.anulado;
            });

            if (!lineas.length) {
                showMessage("warning", "BonificaciÃ³n", "No hay productos en el carrito.");
                return false;
            }

            let aplicadas = 0;
            lineas.forEach(function (linea) {
                if (aplicarBonificacionPorcentajeALinea(linea, porcentaje)) {
                    aplicadas++;
                }
            });

            if (!aplicadas) {
                showMessage("warning", "BonificaciÃ³n", "No se pudo aplicar la bonificaciÃ³n a los productos del carrito.");
                return false;
            }

            return true;
        }

        function syncDesdePrecio() {
            if (!window.lineaSeleccionada) return;

            const precioLista = fnum($("#modalPrecio").data("precio"));
            const precioNuevo = fnum($("#txtPrecioKg").val());
            const cant = parseCant($("#modalCantidad").val());

            if (!precioLista || !precioNuevo || !isFinite(cant) || cant <= 0) {
                $("#txtPorcentaje").val("");
                $("#modalSubtotal").val("");
                return;
            }

            let pct = (1 - (precioNuevo / precioLista)) * 100.0;
            pct = Math.round(pct * 100) / 100.0;

            $("#txtPorcentaje").val(pct);

            if (pct > 0) $("#lblTipoPorcentaje").text("% bonificación");
            else if (pct < 0) $("#lblTipoPorcentaje").text("% recargo");
            else $("#lblTipoPorcentaje").text("%");

            $("#modalSubtotal").val((precioNuevo * cant).toFixed(2));
        }

        function syncDesdePorcentaje() {
            if (!window.lineaSeleccionada) return;

            const precioLista = fnum($("#modalPrecio").data("precio"));
            const pct = fnum($("#txtPorcentaje").val());
            const cant = parseCant($("#modalCantidad").val());

            if (!precioLista || !isFinite(cant) || cant <= 0) {
                $("#txtPrecioKg").val("");
                $("#modalSubtotal").val("");
                return;
            }

            const precioNuevo = precioLista * (1 - (pct / 100.0));
            const subtotal = precioNuevo * cant;

            $("#txtPrecioKg").val(precioNuevo.toFixed(2));
            $("#modalSubtotal").val(subtotal.toFixed(2));

            if (pct > 0) $("#lblTipoPorcentaje").text("% bonificación");
            else if (pct < 0) $("#lblTipoPorcentaje").text("% recargo");
            else $("#lblTipoPorcentaje").text("%");
        }

        function loadLineModal(linea) {
            if (soloFormaPago) return;
            $("#modalProducto").val(`#${linea.index} ${linea.producto}`);

            const cantNum = parseCant(linea.cant);
            $("#modalCantidad").val(isFinite(cantNum) ? fmtCant(cantNum) : String(linea.cant ?? ""));

            const precioListaNum = fnum(linea.precioOriginal ?? linea.precio);
            $("#modalPrecio").val(String(linea.precioOriginal ?? linea.precio).replace("$", "").trim());
            $("#modalPrecio").data("precio", precioListaNum);

            $("#modalSubtotal").val(String(linea.subtotal).replace("$", "").trim());

            const bonif = fnum(linea.bonificacion);
            let texto = "Total";
            if (bonif !== 0) texto += bonif > 0 ? ` | Desc:${bonif}%` : ` | Recargo:${Math.abs(bonif)}%`;
            $("#lblTotalModalLineaVenta").text(texto);

            $("#txtPrecioKg").val(String(linea.precio ?? "").replace("$", "").trim());
            $("#txtPorcentaje").val(linea.bonificacion ?? 0);

            $("#chkPorcentaje").prop("checked", false);
            $("#chkBonificarTodos").prop("checked", false);
            $("#chkBonificarTodos").prop("disabled", true);
            $("#txtPorcentaje").prop("readonly", true);
            $("#txtPrecioKg").prop("readonly", false);

            $("#bloqueCantidad").hide();
            $("#modalCantidad").prop("readonly", true);
            $("#btnCantMenos").addClass("d-none");
            $("#btnCantMas").addClass("d-none");

            if (bonif !== 0) {
                $("#bloqueBonificar").show();
                $("#btnMostrarCantidad").removeClass("d-none");
                $("#btnEliminarItem").removeClass("d-none");
                $("#btnMostrarBonificar").addClass("d-none");
                setModoBonificacion(true);
            } else {
                $("#bloqueBonificar").hide();
                setModoBonificacion(false);
            }

            syncUIBonificacionTodos();

            $("#modalLineaVenta").modal("show");
        }

        function setupLineModal() {
            if (window.__posLineaVentaModalInit) return;
            window.__posLineaVentaModalInit = true;

            if (typeof window.lineaSeleccionada === "undefined") window.lineaSeleccionada = null;

            $(document).on("click", "#tablaItems tr.fila-item", function () {
                if (soloFormaPago) return;
                $("#btnEliminarItem").removeClass("d-none");
                $("#btnMostrarBonificar").removeClass("d-none");
                $("#btnMostrarCantidad").removeClass("d-none");

                const index = parseInt($(this).data("id"), 10);
                window.lineaSeleccionada = POSState.findLineaByIndex(index);
                if (!window.lineaSeleccionada) return;
                if (window.lineaSeleccionada.anulado) {
                    if (window.Swal) {
                        Swal.fire({
                            icon: "info",
                            title: "Línea anulada",
                            text: "Esta línea ya fue anulada y no se puede modificar."
                        });
                    }
                    window.lineaSeleccionada = null;
                    return;
                }

                loadLineModal(window.lineaSeleccionada);
            });

            window.cargarModalLineaVenta = loadLineModal;

            $("#btnMostrarBonificar").off("click").on("click", function () {
                if (soloFormaPago) return;
                if (!window.lineaSeleccionada) return;
                if (!puedeBonificar) {
                    showMessage("warning", "Bonificación", mensajeSinPermisoBonificar);
                    return;
                }
                $("#bloqueBonificar").slideDown(120);
                $("#btnMostrarBonificar").addClass("d-none");
                $("#btnMostrarCantidad").removeClass("d-none");
                $("#btnEliminarItem").removeClass("d-none");
                setModoBonificacion(false);
            });

            $("#chkPorcentaje").off("change").on("change", function () {
                setModoBonificacion($(this).is(":checked"));
            });

            $("#chkBonificarTodos").off("change").on("change", function () {
                if ($(this).is(":checked")) {
                    $("#chkPorcentaje").prop("checked", true);
                    $("#txtPorcentaje").prop("readonly", false);
                    $("#txtPrecioKg").prop("readonly", true);
                }

                syncUIBonificacionTodos();
                if ($(this).is(":checked")) {
                    requestAnimationFrame(function () { $("#txtPorcentaje").focus().select(); });
                }
            });

            $("#txtPrecioKg").off("input").on("input", function () {
                if ($("#chkPorcentaje").is(":checked")) return;
                syncDesdePrecio();
            });

            $("#txtPorcentaje").off("input").on("input", function () {
                if (!$("#chkPorcentaje").is(":checked")) return;
                syncDesdePorcentaje();
            });

            $("#btnAplicarBonificacion").off("click").on("click", function () {
                if (soloFormaPago) return;
                if (!window.lineaSeleccionada) return;
                if (!puedeBonificar) {
                    showMessage("warning", "Bonificación", mensajeSinPermisoBonificar);
                    return;
                }

                if ($("#chkPorcentaje").is(":checked") && $("#chkBonificarTodos").is(":checked")) {
                    const porcentaje = fnum($("#txtPorcentaje").val());
                    if (!aplicarBonificacionPorcentajeATodoElCarrito(porcentaje)) {
                        return;
                    }

                    renderTable(POSState.getLineas());
                    recalculateTotal();
                    updateSaleState();

                    $("#modalLineaVenta").modal("hide");
                    requestAnimationFrame(function () {
                        if (window.lineaSeleccionada) scrollLineVisible(window.lineaSeleccionada.index);
                    });
                    return;
                }

                const precioNuevo = fnum($("#txtPrecioKg").val());
                if (!precioNuevo || precioNuevo <= 0) {
                    showMessage("warning", "Precio inválido", "Ingrese un precio mayor a 0.");
                    return;
                }

                const cant = parseCant($("#modalCantidad").val());
                if (!isFinite(cant)) {
                    showMessage("warning", "Cantidad inválida", "Ingrese una cantidad válida.");
                    return;
                }
                if (cant < CANT_MINIMA) {
                    showMessage("warning", "Cantidad inválida", "La cantidad mínima es 0,010 kg.");
                    return;
                }

                window.lineaSeleccionada.cant = roundCant(cant);
                window.lineaSeleccionada.precio = `$ ${precioNuevo.toFixed(2)}`;
                window.lineaSeleccionada.subtotal = `$ ${(precioNuevo * cant).toFixed(2)}`;
                window.lineaSeleccionada.bonificacion = fnum($("#txtPorcentaje").val());

                renderTable(POSState.getLineas());
                recalculateTotal();

                $("#modalLineaVenta").modal("hide");
                requestAnimationFrame(function () { scrollLineVisible(window.lineaSeleccionada.index); });
            });
            $("#btnEliminarItem").off("click").on("click", function () {
                if (soloFormaPago) return;
                if (!window.lineaSeleccionada) return;
                const linea = window.lineaSeleccionada;

                Swal.fire({
                    icon: "warning",
                    title: "¿Eliminar el producto?",
                    showCancelButton: true,
                    confirmButtonText: "Sí",
                    cancelButtonText: "No"
                }).then(function (result) {
                    if (!result.isConfirmed) return;

                    if (eliminarFisicoLineasPendientes) {
                        POSState.removeLinea(linea.index);
                    } else {
                        POSState.anularLinea(linea.index);
                    }
                    window.POSExpendiosCurrent?.syncAssignedFromCart?.();
                    renderTable(POSState.getLineas());
                    recalculateTotal();
                    updateSaleState();

                    $("#modalLineaVenta").modal("hide");
                    if (!eliminarFisicoLineasPendientes) {
                        requestAnimationFrame(function () { scrollLineVisible(linea.index); });
                    }

                    $("#inputCodigo").val("").focus();
                });
            });

            $("#btnMostrarCantidad").off("click").on("click", function () {
                if (soloFormaPago) return;
                if (!window.lineaSeleccionada) return;

                $("#bloqueCantidad").slideDown(120);
                $("#btnMostrarCantidad").addClass("d-none");
                $("#btnEliminarItem").removeClass("d-none");

                $("#modalCantidad").prop("readonly", false);
                $("#btnCantMenos").removeClass("d-none");
                $("#btnCantMas").removeClass("d-none");

                requestAnimationFrame(function () { $("#modalCantidad").focus(); });
                previewSubtotalDesdeCantidad();
            });

            $("#modalCantidad")
                .off("keydown.cant")
                .on("keydown.cant", function (e) {
                    if ($(this).prop("readonly")) return;

                    const k = e.key;
                    const ctrl = e.ctrlKey || e.metaKey;
                    const allow = ["Backspace", "Delete", "ArrowLeft", "ArrowRight", "Home", "End", "Tab", "Enter"];

                    if (allow.includes(k)) return;
                    if (ctrl && ["a", "c", "v", "x"].includes(k.toLowerCase())) return;
                    if (/^[0-9]$/.test(k)) return;
                    if (k === "," || k === ".") return;

                    e.preventDefault();
                })
                .off("input.cant")
                .on("input.cant", function () {
                    if ($(this).prop("readonly")) return;
                    previewSubtotalDesdeCantidad();
                });

            $("#btnCantMas").off("click").on("click", function () {
                if (soloFormaPago) return;
                const actual = parseCant($("#modalCantidad").val());
                const base = isFinite(actual) ? actual : 0;
                $("#modalCantidad").val(fmtCant(roundCant(base + 1)));
                previewSubtotalDesdeCantidad();
            });

            $("#btnCantMenos").off("click").on("click", function () {
                if (soloFormaPago) return;
                const actual = parseCant($("#modalCantidad").val());
                const base = isFinite(actual) ? actual : 0;
                $("#modalCantidad").val(fmtCant(Math.max(0, roundCant(base - 1))));
                previewSubtotalDesdeCantidad();
            });

            $("#btnAplicarCantidad").off("click").on("click", function () {
                if (soloFormaPago) return;
                if (!window.lineaSeleccionada) return;

                const cant = parseCant($("#modalCantidad").val());
                if (!isFinite(cant)) {
                    showMessage("warning", "Cantidad inválida", "Ingrese un número válido.");
                    return;
                }
                if (cant < CANT_MINIMA) {
                    showMessage("warning", "Cantidad inválida", "La cantidad mínima es 0,010 kg.");
                    return;
                }

                const cantOk = roundCant(cant);
                window.lineaSeleccionada.cant = cantOk;
                window.lineaSeleccionada.subtotal = `$ ${(precioLineaAplicadoNum() * cantOk).toFixed(2)}`;

                $("#modalCantidad").val(fmtCant(cantOk));

                renderTable(POSState.getLineas());
                recalculateTotal();

                $("#modalLineaVenta").modal("hide");
                requestAnimationFrame(function () { scrollLineVisible(window.lineaSeleccionada.index); });
            });

            $("#modalLineaVenta").on("hidden.bs.modal", function () {
                $("#btnEliminarItem").removeClass("d-none");
                $("#btnMostrarBonificar").removeClass("d-none");
                $("#btnMostrarCantidad").removeClass("d-none");

                $("#bloqueBonificar").hide();
                $("#chkPorcentaje").prop("checked", false);
                $("#txtPorcentaje").prop("readonly", true);
                $("#txtPrecioKg").prop("readonly", false);

                $("#bloqueCantidad").hide();
                $("#modalCantidad").prop("readonly", true);
                $("#btnCantMenos").addClass("d-none");
                $("#btnCantMas").addClass("d-none");

                window.lineaSeleccionada = null;
            });
        }

        function bindDomEvents() {
            if (soloFormaPago) return;
            $("#inputCantidad").on("input", calculateSubtotal);

            $("#inputCodigo").on("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    clearTimeout(options.getTypingTimer());
                    options.setEnterDesdeTecladoVirtual(false);
                    options.handleEnter();
                }
            });

            $("#inputCantidad").on("keydown", function (e) {
                if (e.key === "Enter") {
                    e.preventDefault();
                    addProduct();
                }
            });

            $("#btnAgregarProducto").on("click", function () {
                addProduct();
            });
        }

        const api = {
            init: function () {
                bindDomEvents();
                setupLineModal();
            },
            calculateSubtotal: calculateSubtotal,
            recalculateTotal: recalculateTotal,
            addProduct: addProduct,
            renderTable: renderTable,
            updateSaleState: updateSaleState,
            updateCartSummary: updateCartSummary
        };

        window.calcularSubtotal = calculateSubtotal;
        window.recalcularTotal = recalculateTotal;
        window.agregarProducto = addProduct;
        window.renderTablaProductos = renderTable;
        window.actualizarEstadoVentaEnCurso = updateSaleState;

        return api;
    }

    window.POSCart = {
        create: createPOSCart
    };
})(window, window.jQuery);
