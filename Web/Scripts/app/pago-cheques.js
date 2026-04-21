(function (window, $) {
    if (!window || !$) return;

    window.PagoCheques = window.PagoCheques || (function () {
        var debounceBusqueda = null;
        var debounceSeleccion = null;
        var seedAplicado = false;
        var config = {
            desdePos: false
        };

        function mostrarMensaje(opciones) {
            var settings = opciones || {};

            if (typeof window.swalPago === "function") {
                return window.swalPago(settings);
            }

            if (window.Swal && typeof window.Swal.fire === "function") {
                return window.Swal.fire(settings);
            }

            if (settings.text) {
                alert(settings.text);
            }
        }

        function ofrecerAltaCheque(nroCheque) {
            var titulo = "No existe un cheque con ese numero";
            var texto = "Desea darlo de alta?";

            var abrir = function () {
                if (window.ModalAltaCheque) {
                    window.ModalAltaCheque.abrir(nroCheque);
                }
            };

            if (typeof window.swalPago === "function") {
                return window.swalPago({
                    title: titulo,
                    text: texto,
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Si, agregar",
                    cancelButtonText: "No"
                }).then(function (result) {
                    if (result && result.isConfirmed) {
                        abrir();
                    }
                });
            }

            if (window.Swal && typeof window.Swal.fire === "function") {
                return window.Swal.fire({
                    title: titulo,
                    text: texto,
                    icon: "warning",
                    showCancelButton: true,
                    confirmButtonText: "Si, agregar",
                    cancelButtonText: "No"
                }).then(function (result) {
                    if (result && result.isConfirmed) {
                        abrir();
                    }
                });
            }

            if (confirm(titulo + "\n" + texto)) {
                abrir();
            }
        }

        function escapeHtml(valor) {
            return String(valor == null ? "" : valor)
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#39;");
        }

        function normalizarImporte(valor) {
            var numero = Number(valor || 0);
            return isNaN(numero) ? 0 : numero;
        }

        function obtenerChequesActualesJson() {
            var lista = [];

            $("#tablaCheques tbody tr").each(function () {
                lista.push({
                    Id: $(this).data("id") || 0,
                    NroCheque: ($(this).find("td:eq(0)").text() || "").trim()
                });
            });

            return JSON.stringify(lista);
        }

        function reindexarCheques() {
            $("#tablaCheques tbody tr").each(function (index) {
                $(this).find("input[name]").each(function () {
                    var name = $(this).attr("name");
                    if (!name) return;

                    $(this).attr("name", name.replace(/\[\d+\]/, "[" + index + "]"));
                });
            });
        }

        function recalcularTotales() {
            var totalCheques = 0;

            $("#tablaCheques tbody tr").each(function () {
                var texto = $(this).find(".imp").text().replace("$", "").trim();
                var valor = parseFloat(texto);
                if (!isNaN(valor)) {
                    totalCheques += valor;
                }
            });

            $("#txtTotalCheques").val(totalCheques.toFixed(2));

            var efectivo = parseFloat($("#Efectivo").val() || "") || 0;
            if ($("#bloqueCheques").is(":visible")) {
                $("#importe").val((totalCheques + efectivo).toFixed(2));
            }
        }

        function clearCheques() {
            $("#tablaCheques tbody").empty();
            reindexarCheques();
            recalcularTotales();
        }

        function agregarFilaCheque(cheque, opciones) {
            var settings = $.extend({
                mostrarDuplicado: true,
                limpiarInput: true
            }, opciones || {});

            if (!cheque || !cheque.Id) {
                return false;
            }

            if ($("#tablaCheques tbody tr[data-id='" + cheque.Id + "']").length > 0) {
                if (settings.mostrarDuplicado) {
                    mostrarMensaje({
                        title: "Aviso",
                        text: "Este cheque ya fue agregado.",
                        icon: "info"
                    });
                }

                if (settings.limpiarInput) {
                    $("#txtNroCheque").val("").trigger("focus");
                }

                return false;
            }

            var index = $("#tablaCheques tbody tr").length;
            var importe = normalizarImporte(cheque.Importe);
            var fila = [
                '<tr data-id="' + escapeHtml(cheque.Id) + '">',
                '  <td>',
                "      " + escapeHtml(cheque.NroCheque),
                '      <input type="hidden" name="Cheques[' + index + '].Numero" value="' + escapeHtml(cheque.NroCheque) + '">',
                "  </td>",
                '  <td>',
                "      " + escapeHtml(cheque.Banco),
                '      <input type="hidden" name="Cheques[' + index + '].Banco" value="' + escapeHtml(cheque.Banco) + '">',
                "  </td>",
                '  <td>',
                "      " + escapeHtml(cheque.FechaPago),
                '      <input type="hidden" name="Cheques[' + index + '].FechaPago" value="' + escapeHtml(cheque.FechaPago) + '">',
                "  </td>",
                '  <td class="imp">',
                "      $" + importe.toFixed(2),
                '      <input type="hidden" name="Cheques[' + index + '].Importe" value="' + importe.toFixed(2) + '">',
                "  </td>",
                "  <td>",
                '      <input type="hidden" name="Cheques[' + index + '].Id" value="' + escapeHtml(cheque.Id) + '">',
                '      <button type="button" class="btn btn-danger btn-sm btnQuitarCheque">Quitar</button>',
                "  </td>",
                "</tr>"
            ].join("");

            $("#tablaCheques tbody").append(fila);
            reindexarCheques();
            recalcularTotales();

            if (settings.limpiarInput) {
                $("#txtNroCheque").val("").trigger("focus");
            }

            return true;
        }

        function filasChequesVisibles() {
            return $("#tablaBusquedaCheques tbody tr.fila-cheque-busqueda").filter(function () {
                return $(this).css("display") !== "none";
            });
        }

        function marcarChequeSeleccionado($fila) {
            $("#tablaBusquedaCheques tbody tr.fila-cheque-busqueda").removeClass("is-selected");
            if ($fila && $fila.length) {
                $fila.addClass("is-selected");
            }
        }

        function seleccionarPrimeraFila() {
            var $primera = filasChequesVisibles().first();
            if ($primera.length) {
                marcarChequeSeleccionado($primera);
            }
        }

        function enfocarBuscadorCheques() {
            setTimeout(function () {
                $("#filtroNroCheque").trigger("focus").trigger("select");
            }, 0);
        }

        function enfocarInputChequePrincipal() {
            setTimeout(function () {
                $("#txtNroCheque").trigger("focus").trigger("select");
            }, 0);
        }

        function programarSeleccionPrimeraFila(delay) {
            window.clearTimeout(debounceSeleccion);
            debounceSeleccion = window.setTimeout(function () {
                seleccionarPrimeraFila();
            }, delay || 120);
        }

        function abrirDetalleChequeDesdeFila(fila) {
            var $fila = $(fila);
            if (!$fila.length) return false;

            $("#detalleChequeNumero").text($fila.data("nro") || "-");
            $("#detalleChequeEstado").text($fila.data("estado") || "-");
            $("#detalleChequeOrigen").text($fila.data("origen") || "-");
            $("#detalleChequeBanco").text($fila.data("banco") || "-");
            $("#detalleChequeRecibidoDe").text($fila.data("recibidode") || "-");
            $("#detalleChequeEntregadoA").text($fila.data("entregadoa") || "-");
            $("#detalleChequeObservaciones").text($fila.data("observaciones") || "-");
            $("#modalDetalleCheque").modal("show");
            return false;
        }

        function agregarChequePorNumero(nroCheque, opciones) {
            var settings = $.extend({
                cerrarModal: false
            }, opciones || {});

            var nro = (nroCheque || $("#txtNroCheque").val() || "").trim();
            if (!nro) {
                return false;
            }

            $("#txtNroCheque").val(nro);

            $.get(window.urls.buscarChequePorNro, {
                numero: nro,
                pagoId: $("#Id").val(),
                esAProveedor: $("#AProveedor").val() === "true",
                chequesJson: obtenerChequesActualesJson()
            }, function (data) {
                if (!data || !data.ok || !data.cheque) {
                    var mensaje = (data && data.mensaje) || "No se pudo validar el cheque.";

                    if (mensaje.indexOf("No existe un cheque con ese numero") >= 0 ||
                        mensaje.indexOf("No existe un cheque con ese número") >= 0) {
                        ofrecerAltaCheque(nro);
                        return;
                    }

                    mostrarMensaje({
                        title: "Atencion",
                        text: mensaje,
                        icon: "warning"
                    });
                    return;
                }

                if (agregarFilaCheque(data.cheque)) {
                    if (settings.cerrarModal) {
                        $("#modalBuscarCheques").modal("hide");
                    }
                }
            }).fail(function () {
                mostrarMensaje({
                    title: "Error",
                    text: "No se pudo consultar el cheque.",
                    icon: "error"
                });
            });

            return false;
        }

        function seleccionarChequeDesdeFila(fila, opciones) {
            var $fila = $(fila);
            if (!$fila.length) return false;

            var nroCheque = ($fila.data("nro") || "").toString().trim();
            if (!nroCheque) return false;

            $("#txtNroCheque").val(nroCheque).trigger("input");
            return agregarChequePorNumero(nroCheque, opciones || { cerrarModal: true });
        }

        function renderizarTablaBusqueda(lista) {
            var $tbody = $("#tablaBusquedaCheques tbody");
            if (!$tbody.length || !Array.isArray(lista)) return;

            function badgeEstado(estado) {
                var css = "badge-secondary";
                switch ((estado || "").toUpperCase()) {
                    case "PENDIENTE":
                        css = "badge-warning";
                        break;
                    case "ENTREGADO":
                        css = "badge-info";
                        break;
                    case "DEPOSITADO":
                        css = "badge-primary";
                        break;
                    case "ACREDITADO":
                        css = "badge-success";
                        break;
                    case "RECHAZADO":
                        css = "badge-danger";
                        break;
                }

                return '<span class="badge ' + css + '">' + escapeHtml(estado || "-") + "</span>";
            }

            var html = "";
            lista.forEach(function (c) {
                var detalleId = "detChequePago_" + escapeHtml(c.Id);
                html += [
                    '<tr',
                    ' class="fila-cheque-busqueda"',
                    ' data-id="' + escapeHtml(c.Id) + '"',
                    ' data-nro="' + escapeHtml(c.NroCheque) + '"',
                    ' data-banco="' + escapeHtml(c.Banco) + '"',
                    ' data-fechaemision="' + escapeHtml(c.FechaEmision || "") + '"',
                    ' data-fechapago="' + escapeHtml(c.FechaPago) + '"',
                    ' data-importe="' + escapeHtml(normalizarImporte(c.Importe).toFixed(2)) + '"',
                    ' data-estado="' + escapeHtml(c.Estado) + '"',
                    ' data-origen="' + escapeHtml(c.Origen) + '"',
                    ' data-titular="' + escapeHtml(c.Titular || "") + '"',
                    ' data-recibidode="' + escapeHtml(c.RecibidoDeNombre || "-") + '"',
                    ' data-entregadoa="' + escapeHtml(c.EntregadoANombre || "-") + '"',
                    ' data-observaciones="' + escapeHtml(c.Observaciones || "-") + '"',
                    ' data-creado="' + escapeHtml(c.Creado || "") + '"',
                    ' data-creadopor="' + escapeHtml(c.CreadoPor || "") + '"',
                    ' data-actualizado="' + escapeHtml(c.Actualizado || "") + '"',
                    ' data-actualizadopor="' + escapeHtml(c.ActualizadoPor || "") + '"',
                    ">",
                    "  <td>" + escapeHtml(c.Id) + "</td>",
                    "  <td>" + escapeHtml(c.NroCheque) + "</td>",
                    "  <td>" + escapeHtml(c.Banco) + "</td>",
                    "  <td>$" + normalizarImporte(c.Importe).toFixed(2) + "</td>",
                    "  <td>" + escapeHtml(c.FechaPago) + "</td>",
                    "  <td>" + badgeEstado(c.Estado) + "</td>",
                    "  <td>" + escapeHtml(c.Titular || "") + "</td>",
                    '  <td><button type="button" class="btn btn-sm btn-link text-decoration-none btn-detalles-cheque" data-toggle="collapse" data-target="#' + detalleId + '" aria-expanded="false" aria-controls="' + detalleId + '"><i class="fas fa-chevron-down"></i> Detalles</button><button type="button" class="btn btn-primary btn-sm mr-1 btn-seleccionar-cheque">Seleccionar</button></td>',
                    "</tr>",
                    '<tr class="tr-detalles-cheque-busqueda">',
                    '  <td colspan="8" class="p-0">',
                    '    <div id="' + detalleId + '" class="collapse">',
                    '      <div class="p-3 bg-light border-top">',
                    '        <div class="row">',
                    '          <div class="col-lg-6">',
                    '            <div class="mb-2"><span class="text-muted">Origen:</span> <span class="font-weight-bold">' + escapeHtml(c.Origen || "-") + '</span></div>',
                    '            <div class="mb-2"><span class="text-muted">Fecha emision:</span> <span class="font-weight-bold">' + escapeHtml(c.FechaEmision || "-") + '</span></div>',
                    '            <div class="mb-2"><span class="text-muted">Recibido de:</span> <span class="font-weight-bold">' + escapeHtml(c.RecibidoDeNombre || "-") + '</span></div>',
                    '            <div class="mb-2"><span class="text-muted">Entregado a:</span> <span class="font-weight-bold">' + escapeHtml(c.EntregadoANombre || "-") + '</span></div>',
                    "          </div>",
                    '          <div class="col-lg-6">',
                    '            <div class="mb-2"><span class="text-muted">Creado:</span> <span class="font-weight-bold">' + escapeHtml(c.Creado || "-") + '</span></div>',
                    '            <div class="mb-2"><span class="text-muted">Creado por:</span> <span class="font-weight-bold">' + escapeHtml(c.CreadoPor || "-") + '</span></div>',
                    '            <div class="mb-2"><span class="text-muted">Actualizado:</span> <span class="font-weight-bold">' + escapeHtml(c.Actualizado || "-") + '</span></div>',
                    '            <div class="mb-2"><span class="text-muted">Actualizado por:</span> <span class="font-weight-bold">' + escapeHtml(c.ActualizadoPor || "-") + '</span></div>',
                    "          </div>",
                    "        </div>",
                    '        <div class="mt-2"><div class="text-muted mb-1">Observaciones</div><div class="font-weight-bold">' + escapeHtml(c.Observaciones || "-") + "</div></div>",
                    "      </div>",
                    "    </div>",
                    "  </td>",
                    "</tr>"
                ].join("");
            });

            $tbody.html(html);
            seleccionarPrimeraFila();
        }

        function cargarChequesMejorado() {
            $.ajax({
                url: window.urls.getCheques,
                type: "GET",
                dataType: "json",
                data: {
                    estado: $("#filtroEstado").val(),
                    nroCheque: $("#filtroNroCheque").val(),
                    desde: $("#filtroDesde").val()
                },
                success: function (lista) {
                    renderizarTablaBusqueda(lista);
                },
                error: function () {
                    mostrarMensaje({
                        title: "Error",
                        text: "No se pudieron cargar los cheques.",
                        icon: "error"
                    });
                }
            });
        }

        function aislarModalesHijosPago() {
            ["#modalBuscarCheques", "#modalAltaCheque", "#modalDetalleCheque"].forEach(function (selector) {
                var $modal = $(selector);
                if (!$modal.length) return;

                $("body").children(selector).not($modal).remove();

                if (!$modal.parent().is("body")) {
                    $modal.appendTo("body");
                }
            });
        }

        function obtenerModalesVisiblesOrdenados() {
            return $(".modal.show").filter(function () {
                return $(this).is(":visible");
            }).get().sort(function (a, b) {
                var zA = parseInt($(a).css("z-index"), 10) || 0;
                var zB = parseInt($(b).css("z-index"), 10) || 0;

                if (zA !== zB) {
                    return zA - zB;
                }

                return $(a).index() - $(b).index();
            });
        }

        function actualizarEstadoVisualModales() {
            var visibles = obtenerModalesVisiblesOrdenados();

            $(".modal.modal-fondo-activo")
                .removeClass("modal-fondo-activo")
                .find(".modal-content")
                .removeClass("modal-content-fondo-activo");

            if (visibles.length <= 1) {
                return;
            }

            visibles.slice(0, visibles.length - 1).forEach(function (modal) {
                var $modal = $(modal);
                $modal.addClass("modal-fondo-activo");
                $modal.find(".modal-content").first().addClass("modal-content-fondo-activo");
            });
        }

        function inicializarFiltros() {
            if (!$("#filtroDesde").val()) {
                var fecha = new Date();
                fecha.setDate(fecha.getDate() - 31);
                var year = fecha.getFullYear();
                var month = ("0" + (fecha.getMonth() + 1)).slice(-2);
                var day = ("0" + fecha.getDate()).slice(-2);
                $("#filtroDesde").val(year + "-" + month + "-" + day);
            }
        }

        function aplicarSeed() {
            if (seedAplicado || !Array.isArray(window.PagoChequeSeed)) {
                return;
            }

            window.PagoChequeSeed.forEach(function (cheque) {
                agregarFilaCheque(cheque, {
                    mostrarDuplicado: false,
                    limpiarInput: false
                });
            });

            seedAplicado = true;
        }

        function bindEvents() {
            $(document).off(".pagoCheques");

            $(document)
                .on("click.pagoCheques", "#btnBuscarCheques", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    $("#modalBuscarCheques").modal("show");
                    cargarChequesMejorado();
                    return false;
                })
                .on("click.pagoCheques", "#btnFiltrarCheques", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    cargarChequesMejorado();
                    return false;
                })
                .on("click.pagoCheques", "#btnAgregarCheque", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    return agregarChequePorNumero($("#txtNroCheque").val(), { cerrarModal: false });
                })
                .on("keydown.pagoCheques", "#txtNroCheque", function (e) {
                    if (e.key !== "Enter") return;
                    e.preventDefault();
                    e.stopPropagation();
                    return agregarChequePorNumero($("#txtNroCheque").val(), { cerrarModal: false });
                })
                .on("click.pagoCheques", ".btnQuitarCheque", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    $(this).closest("tr").remove();
                    reindexarCheques();
                    recalcularTotales();
                    return false;
                })
                .on("chequeCreado.pagoCheques", function (_evt, cheque) {
                    if (!cheque || !cheque.NroCheque) return;
                    agregarChequePorNumero(cheque.NroCheque, { cerrarModal: false });
                })
                .on("shown.bs.modal.pagoCheques", "#modalBuscarCheques", function (e) {
                    e.stopPropagation();
                    aislarModalesHijosPago();
                    actualizarEstadoVisualModales();
                    enfocarBuscadorCheques();
                    programarSeleccionPrimeraFila(50);
                })
                .on("hidden.bs.modal.pagoCheques", "#modalBuscarCheques", function (e) {
                    e.stopPropagation();
                    actualizarEstadoVisualModales();
                    enfocarInputChequePrincipal();

                    if (config.desdePos && ($("#modalPagoPOS").hasClass("show") || $("#modalFinanzasPOS").hasClass("show"))) {
                        setTimeout(function () {
                            $("body").addClass("modal-open");
                        }, 0);
                    }
                })
                .on("shown.bs.modal.pagoCheques hidden.bs.modal.pagoCheques hide.bs.modal.pagoCheques", "#modalAltaCheque, #modalDetalleCheque", function (e) {
                    e.stopPropagation();
                    aislarModalesHijosPago();
                    actualizarEstadoVisualModales();

                    if (e.type === "hidden" && config.desdePos && ($("#modalPagoPOS").hasClass("show") || $("#modalFinanzasPOS").hasClass("show"))) {
                        setTimeout(function () {
                            $("body").addClass("modal-open");
                        }, 0);
                    }
                })
                .on("click.pagoCheques", "#tablaBusquedaCheques tbody tr.fila-cheque-busqueda", function () {
                    marcarChequeSeleccionado($(this));
                })
                .on("dblclick.pagoCheques", "#tablaBusquedaCheques tbody tr.fila-cheque-busqueda", function (e) {
                    if ($(e.target).closest(".btn-detalles-cheque").length) {
                        return false;
                    }

                    e.preventDefault();
                    e.stopPropagation();
                    return seleccionarChequeDesdeFila(this, { cerrarModal: true });
                })
                .on("click.pagoCheques", ".btn-seleccionar-cheque", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    return seleccionarChequeDesdeFila($(this).closest("tr"), { cerrarModal: true });
                })
                .on("click.pagoCheques", ".btn-detalles-cheque", function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    var $icono = $(this).find("i");
                    window.setTimeout(function () {
                        var expanded = $(this).attr("aria-expanded") === "true";
                        $icono.toggleClass("fa-chevron-down", !expanded);
                        $icono.toggleClass("fa-chevron-up", expanded);
                    }.bind(this), 0);
                    return false;
                })
                .on("keydown.pagoCheques", "#filtroNroCheque", function (e) {
                    var $visibles = filasChequesVisibles();

                    if (e.key === "ArrowDown") {
                        e.preventDefault();
                        if (!$visibles.length) return;

                        var indexDown = $visibles.index($visibles.filter(".is-selected").first());
                        if (indexDown < 0) indexDown = -1;

                        var $nuevaDown = $visibles.eq(Math.min($visibles.length - 1, indexDown + 1));
                        marcarChequeSeleccionado($nuevaDown);
                        if ($nuevaDown.length && $nuevaDown[0].scrollIntoView) {
                            $nuevaDown[0].scrollIntoView({ block: "nearest" });
                        }
                        return;
                    }

                    if (e.key === "ArrowUp") {
                        e.preventDefault();
                        if (!$visibles.length) return;

                        var indexUp = $visibles.index($visibles.filter(".is-selected").first());
                        if (indexUp < 0) indexUp = 1;

                        var $nuevaUp = $visibles.eq(Math.max(0, indexUp - 1));
                        marcarChequeSeleccionado($nuevaUp);
                        if ($nuevaUp.length && $nuevaUp[0].scrollIntoView) {
                            $nuevaUp[0].scrollIntoView({ block: "nearest" });
                        }
                        return;
                    }

                    if (e.key === "Enter") {
                        e.preventDefault();
                        e.stopPropagation();

                        var $fila = filasChequesVisibles().filter(".is-selected").first();
                        if (!$fila.length) {
                            $fila = filasChequesVisibles().first();
                        }

                        if ($fila.length) {
                            return agregarChequePorNumero($fila.data("nro"), { cerrarModal: true });
                        }

                        return false;
                    }
                })
                .on("input.pagoCheques", "#filtroNroCheque", function () {
                    window.clearTimeout(debounceBusqueda);
                    debounceBusqueda = window.setTimeout(function () {
                        cargarChequesMejorado();
                    }, 220);

                    programarSeleccionPrimeraFila(240);
                })
                .on("change.pagoCheques", "#filtroEstado, #filtroDesde", function () {
                    cargarChequesMejorado();
                    programarSeleccionPrimeraFila(180);
                })
                .on("click.pagoCheques", "#modalBuscarCheques [data-dismiss='modal'], #modalAltaCheque [data-dismiss='modal'], #modalDetalleCheque [data-dismiss='modal']", function (e) {
                    e.stopPropagation();
                });
        }

        function init(options) {
            config = $.extend({}, config, options || {});
            seedAplicado = false;

            window.urls = window.urls || {};
            window.urls.buscarChequePorNro = config.buscarChequePorNro;
            window.urls.getCheques = config.getCheques;
            window.urls.guardarCheque = config.guardarCheque;

            inicializarFiltros();
            aislarModalesHijosPago();
            bindEvents();
            aplicarSeed();
            recalcularTotales();
            actualizarEstadoVisualModales();

            window.POSPagoSeleccionarChequeBusqueda = function (nroCheque) {
                return agregarChequePorNumero(nroCheque, { cerrarModal: true });
            };

            window.POSPagoSeleccionarChequeDesdeFila = function (fila) {
                return seleccionarChequeDesdeFila(fila, { cerrarModal: true });
            };

            window.POSPagoVerDetalleChequeDesdeFila = function (fila) {
                return abrirDetalleChequeDesdeFila(fila);
            };
        }

        return {
            init: init,
            agregarChequePorNumero: agregarChequePorNumero,
            agregarFilaCheque: agregarFilaCheque,
            recalcularTotales: recalcularTotales,
            reindexarCheques: reindexarCheques,
            clearCheques: clearCheques,
            renderizarTablaBusqueda: renderizarTablaBusqueda
        };
    })();
})(window, window.jQuery);
