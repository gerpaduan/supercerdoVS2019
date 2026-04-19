(function (window, $) {
    'use strict';

    function createPOSCart(options) {
        const POSState = options.POSState;

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

        const CANT_DECIMALES = 3;
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
            let cant = parseFloat($("#inputCantidad").val().replace(",", "."));

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
            $("#lblSubtotal").text(`$ ${total.toFixed(2).replace(".", ",")}`);
        }

        // Compatibilidad con scripts viejos como forma-pago.js
        window.obtenerTotalVenta = function obtenerTotalVenta() {
            return POSState.getTotal();
        };

        function addProduct() {
            const productoSeleccionado = options.getProductoSeleccionado();

            if (!productoSeleccionado) {
                options.focusCodigo();
                return;
            }

            let cantidad = parseFloat($("#inputCantidad").val().replace(",", "."));
            if (isNaN(cantidad) || cantidad <= 0) {
                alert("Cantidad inválida");
                return;
            }

            const subtotal = cantidad * productoSeleccionado.precioKg;
            if (!Number.isFinite(subtotal) || subtotal <= 0) {
                alert("No se puede agregar: el producto no tiene precio cargado (debe ser mayor a 0).");
                options.focusCodigo();
                return;
            }

            const linea = {
                index: POSState.nextIndex(),
                producto: productoSeleccionado.nombre,
                codigo: productoSeleccionado.codigo,
                cant: cantidad.toFixed(3),
                precio: `$ ${productoSeleccionado.precioKg.toFixed(2)}`,
                precioOriginal: `$ ${productoSeleccionado.precioOriginal.toFixed(2)}`,
                subtotal: `$ ${(subtotal).toFixed(2)}`,
                bonificacion: 0,
                anulado: false,
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
                return;
            }

            lineas.forEach(function (l) {
                const claseAnulada = l.anulado ? "fila-anulada" : "";
                const bonificacion = l.bonificacion == 0
                    ? ""
                    : (l.bonificacion > 0
                        ? " | Bonif:" + l.bonificacion + "%"
                        : " | Recargo:" + (l.bonificacion * -1) + "%");

                $tbody.append(`
                    <tr data-id="${l.index}" class="fila-item ${claseAnulada}">
                        <td>
                            <div class="fw-bold">
                                #${l.index} <strong>${l.producto}</strong> (cod: ${l.codigo})
                            </div>
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
            hayVentaEnCurso = POSState.hasVentaEnCurso();
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
            updateSaleState();

            if (omitirAvisoSalidaPOS || !hayVentaEnCurso) return;

            e.preventDefault();
            e.returnValue = "";
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

            $("#modalLineaVenta").modal("show");
        }

        function setupLineModal() {
            if (window.__posLineaVentaModalInit) return;
            window.__posLineaVentaModalInit = true;

            if (typeof window.lineaSeleccionada === "undefined") window.lineaSeleccionada = null;

            $(document).on("click", "#tablaItems tr.fila-item", function () {
                $("#btnEliminarItem").removeClass("d-none");
                $("#btnMostrarBonificar").removeClass("d-none");
                $("#btnMostrarCantidad").removeClass("d-none");

                const index = parseInt($(this).data("id"), 10);
                window.lineaSeleccionada = POSState.findLineaByIndex(index);
                if (!window.lineaSeleccionada) return;

                loadLineModal(window.lineaSeleccionada);
            });

            window.cargarModalLineaVenta = loadLineModal;

            $("#btnMostrarBonificar").off("click").on("click", function () {
                if (!window.lineaSeleccionada) return;
                $("#bloqueBonificar").slideDown(120);
                $("#btnMostrarBonificar").addClass("d-none");
                $("#btnMostrarCantidad").removeClass("d-none");
                $("#btnEliminarItem").removeClass("d-none");
                setModoBonificacion(false);
            });

            $("#chkPorcentaje").off("change").on("change", function () {
                setModoBonificacion($(this).is(":checked"));
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
                if (!window.lineaSeleccionada) return;

                const precioNuevo = fnum($("#txtPrecioKg").val());
                if (!precioNuevo || precioNuevo <= 0) {
                    alert("Precio inválido");
                    return;
                }

                const cant = parseCant($("#modalCantidad").val());
                if (!isFinite(cant)) {
                    alert("Cantidad inválida");
                    return;
                }
                if (cant <= 0) {
                    alert("La cantidad debe ser mayor a 0");
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
                if (!window.lineaSeleccionada) return;
                if (!confirm("¿Eliminar el producto?")) return;

                POSState.anularLinea(window.lineaSeleccionada.index);
                renderTable(POSState.getLineas());
                recalculateTotal();

                $("#modalLineaVenta").modal("hide");
                requestAnimationFrame(function () { scrollLineVisible(window.lineaSeleccionada.index); });

                $("#inputCodigo").val("").focus();
            });

            $("#btnMostrarCantidad").off("click").on("click", function () {
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
                const actual = parseCant($("#modalCantidad").val());
                const base = isFinite(actual) ? actual : 0;
                $("#modalCantidad").val(fmtCant(roundCant(base + 1)));
                previewSubtotalDesdeCantidad();
            });

            $("#btnCantMenos").off("click").on("click", function () {
                const actual = parseCant($("#modalCantidad").val());
                const base = isFinite(actual) ? actual : 0;
                $("#modalCantidad").val(fmtCant(Math.max(0, roundCant(base - 1))));
                previewSubtotalDesdeCantidad();
            });

            $("#btnAplicarCantidad").off("click").on("click", function () {
                if (!window.lineaSeleccionada) return;

                const cant = parseCant($("#modalCantidad").val());
                if (!isFinite(cant)) {
                    alert("Cantidad inválida (ingresá un número).");
                    return;
                }
                if (cant <= 0) {
                    alert("La cantidad debe ser mayor a 0.");
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
            updateSaleState: updateSaleState
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
