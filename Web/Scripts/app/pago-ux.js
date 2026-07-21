(function (window, $) {
    if (!window || !$) return;

    // Mejora UX del formulario de pagos:
    // - foco inicial y foco segun forma de pago
    // - busqueda viva de cheques en el modal
    // - proteccion contra doble submit
    window.PagoUX = window.PagoUX || (function () {
        let submitEnCurso = false;
        let debounceBuscarCheques = null;

        function mostrarMensaje(opciones) {
            if (typeof window.swalPago === "function") {
                return window.swalPago(opciones || {});
            }

            if (window.Swal && typeof window.Swal.fire === "function") {
                return window.Swal.fire(opciones || {});
            }

            const texto = (opciones && (opciones.text || opciones.title)) || "Ocurrió un error.";
            alert(texto);
            return Promise.resolve();
        }

        function esDesdePos() {
            return ($("#formPago input[name='desdePos']").val() || "").toString().toLowerCase() === "true";
        }

        function esLayoutPago() {
            return !esDesdePos();
        }

        function permitirSalidaSinAdvertencia() {
            const guardApi = $("#formPago").data("editPageGuardApi");
            if (guardApi && typeof guardApi.allowNavigation === "function") {
                guardApi.allowNavigation();
            }

            if (typeof window.desactivarProteccionSalida === "function") {
                window.desactivarProteccionSalida();
            } else {
                window.__protegerSalida = false;
            }
        }

        function construirChequesJson() {
            const lista = [];

            $("#tablaCheques tbody tr").each(function () {
                lista.push({
                    Id: $(this).data("id") || 0,
                    NroCheque: ($(this).find("td:eq(0)").text() || "").trim(),
                    Banco: ($(this).find("td:eq(1)").text() || "").trim(),
                    FechaPago: ($(this).find("td:eq(2)").text() || "").trim(),
                    Importe: parseFloat(($(this).find("td:eq(3)").text() || "").replace("$", "").trim()) || 0
                });
            });

            $("#ChequesJson").val(JSON.stringify(lista));
        }

        function normalizarImportes() {
            const importe = ($("#importe").val() || "").toString().trim();
            $("#importe").val(importe);

            const efectivo = ($("#Efectivo").val() || "").toString().trim();
            $("#Efectivo").val(efectivo || "0");
        }

        function validarAntesDeGuardar() {
            const sucursal = $("#SucursalId").val();

            if (!sucursal) {
                mostrarMensaje({
                    icon: "warning",
                    title: "Validación",
                    text: "Debe seleccionar una sucursal antes de guardar."
                });
                return false;
            }

            return true;
        }

        function abrirModalPostPago(resp, opciones) {
            if (!window.PostPagoModal || !resp || !resp.imprimirUrl) {
                return false;
            }

            const config = opciones || {};
            window.PostPagoModal.open({
                pagoId: resp.pagoId,
                redirectUrl: resp.redirectUrl || "",
                imprimirUrl: resp.imprimirUrl,
                imprimirPayloadUrl: resp.imprimirPayloadUrl,
                pdfUrl: resp.pdfUrl,
                emailConfigUrl: resp.emailConfigUrl,
                emailSendUrl: resp.emailSendUrl,
                stayOnPage: !!config.stayOnPage,
                returnInPos: !!config.returnInPos
            });
            return true;
        }

        function resolverRedirectNoPos(resp) {
            const redirectUrl = (resp && resp.redirectUrl) || $("#urlVolverPago").val() || window.location.href;

            if (abrirModalPostPago({
                    pagoId: resp && resp.pagoId,
                    redirectUrl: redirectUrl,
                    imprimirUrl: resp && resp.imprimirUrl,
                    imprimirPayloadUrl: resp && resp.imprimirPayloadUrl,
                    pdfUrl: resp && resp.pdfUrl,
                    emailConfigUrl: resp && resp.emailConfigUrl,
                    emailSendUrl: resp && resp.emailSendUrl
                }, {
                    stayOnPage: false,
                    returnInPos: false
                })) {
                return;
            }

            if ($("#modalEgresoCaja").hasClass("show") &&
                $("#contenedorActividadesCaja").length &&
                redirectUrl &&
                window.EgresosCaja &&
                typeof window.EgresosCaja.renderConScripts === "function") {
                mostrarMensaje({
                    icon: "success",
                    title: "Pago guardado correctamente"
                }).then(function () {
                    $.ajax({
                        url: redirectUrl,
                        type: "GET",
                        cache: false
                    }).done(function (html) {
                        $("#modalEgresoCaja").modal("hide");
                        window.EgresosCaja.renderConScripts(html, "#contenedorActividadesCaja");
                    }).fail(function () {
                        window.location.href = redirectUrl;
                    });
                });
                return;
            }

            mostrarMensaje({
                icon: "success",
                title: "Pago guardado correctamente"
            }).then(function () {
                window.location.href = redirectUrl;
            });
        }

        function resolverRedirectPos(resp) {
            if (window.POSFinanzasState && resp && resp.redirectUrl) {
                window.POSFinanzasState.redirectDespuesDePago = resp.redirectUrl;
                window.POSFinanzasState.tituloDespuesDePago = "Cuenta corriente";
            }

            if (abrirModalPostPago(resp, {
                    stayOnPage: false,
                    returnInPos: true
                })) {
                return;
            }

            mostrarMensaje({
                icon: "success",
                title: "Pago guardado correctamente"
            }).then(function () {
                if (resp && resp.cerrarModalPago && $("#modalPagoPOS").length) {
                    $("#modalPagoPOS").modal("hide");
                    return;
                }

                if (resp && resp.redirectUrl && window.POSFinanzas && typeof window.POSFinanzas.cargar === "function") {
                    window.POSFinanzas.cargar(resp.redirectUrl, "Cuenta corriente");
                }
            });
        }

        function enviarFormularioAjax(form) {
            $.ajax({
                url: form.action,
                type: "POST",
                data: $(form).serialize(),
                success: function (resp) {
                    if (!resp || !resp.ok) {
                        finalizarSubmit();
                        mostrarMensaje({
                            icon: "warning",
                            title: "Validación",
                            text: (resp && resp.mensaje) || "No se pudo guardar el pago."
                        });
                        return;
                    }

                    permitirSalidaSinAdvertencia();

                    if (esDesdePos()) {
                        resolverRedirectPos(resp);
                        return;
                    }

                    resolverRedirectNoPos(resp);
                },
                error: function (xhr) {
                    finalizarSubmit();

                    let mensaje = "Ocurrió un error inesperado al guardar el pago.";
                    if (xhr && xhr.responseJSON && xhr.responseJSON.mensaje) {
                        mensaje = xhr.responseJSON.mensaje;
                    }

                    mostrarMensaje({
                        icon: "error",
                        title: "Error",
                        text: mensaje
                    });
                }
            });
        }

        function obtenerModoPago() {
            const valor = ($("#formaPago").val() || "").toString().trim().toLowerCase();
            const texto = ($("#formaPago option:selected").text() || "").toString().trim().toLowerCase();
            const textoEsMixtoCheque = texto.indexOf("efectivo") >= 0 && texto.indexOf("cheque") >= 0;

            const esCheque =
                valor === "cheque" ||
                valor === "eftvocheque" ||
                valor === "efvtocheque" ||
                texto === "cheque" ||
                texto === "eftvocheque" ||
                texto === "efvtocheque" ||
                textoEsMixtoCheque;

            const esMixto =
                valor === "eftvocheque" ||
                valor === "efvtocheque" ||
                texto === "eftvocheque" ||
                texto === "efvtocheque" ||
                textoEsMixtoCheque;

            const esEfectivo = !esCheque && (valor === "efectivo" || texto === "efectivo");

            if (esMixto) return "mixto";
            if (esCheque) return "cheque";
            if (esEfectivo) return "efectivo";
            return "simple";
        }

        function actualizarResumenModo() {
            const modo = obtenerModoPago();
            const $resumen = $("#pagoModoResumen");
            const $badge = $("#pagoModoBadge");
            const $titulo = $("#pagoModoTitulo");
            const $descripcion = $("#pagoModoDescripcion");

            if (!$resumen.length) return;

            $resumen.removeClass("pago-modo-neutro pago-modo-efectivo pago-modo-cheque pago-modo-mixto");
            $("#bloqueEfectivo, #bloqueCheques").removeClass("activo");

            if (modo === "efectivo") {
                $resumen.addClass("pago-modo-efectivo");
                $badge.text("Efectivo");
                $titulo.text("Cobro simple en efectivo");
                $descripcion.text("Ingresá el efectivo y el total se toma de ese importe.");
                $("#bloqueEfectivo").addClass("activo");
                return;
            }

            if (modo === "cheque") {
                $resumen.addClass("pago-modo-cheque");
                $badge.text("Cheque");
                $titulo.text("Pago respaldado por cheques");
                $descripcion.text("Agregá uno o varios cheques y el total se calcula desde esa lista.");
                $("#bloqueCheques").addClass("activo");
                return;
            }

            if (modo === "mixto") {
                $resumen.addClass("pago-modo-mixto");
                $badge.text("Mixto");
                $titulo.text("Efectivo + cheque");
                $descripcion.text("Completá el efectivo y sumá los cheques; el importe total se recalcula automáticamente.");
                $("#bloqueEfectivo, #bloqueCheques").addClass("activo");
                return;
            }

            $resumen.addClass("pago-modo-neutro");
            $badge.text("Pago simple");
            $titulo.text("Importe manual");
            $descripcion.text("Ingresá el importe total del pago.");
        }

        function enfocar(selector) {
            setTimeout(function () {
                const $el = $(selector).filter(":visible").first();
                if (!$el.length) return;

                $el.trigger("focus");
                if ($el.is("input, textarea")) {
                    $el.trigger("select");
                }
            }, 0);
        }

        function enfocarPrimeroDisponible(selectores) {
            for (let i = 0; i < selectores.length; i++) {
                const selector = selectores[i];
                const $el = $(selector).filter(":visible:not(:disabled)").first();
                if ($el.length) {
                    enfocar(selector);
                    return true;
                }
            }

            return false;
        }

        function enfocarCampoPrincipal() {
            if ($("#bloqueCheques").is(":visible")) {
                enfocar("#txtNroCheque");
                return;
            }

            if ($("#bloqueEfectivo").is(":visible")) {
                enfocar("#Efectivo");
                return;
            }

            enfocar("#importe");
        }

        function enfocarSiguientePorEnter(selectorActual) {
            if (selectorActual === "#formaPago") {
                enfocarCampoPrincipal();
                return;
            }

            if (selectorActual === "#importe" || selectorActual === "#Efectivo" || selectorActual === "#txtNroCheque") {
                if (enfocarPrimeroDisponible(["#Observaciones"])) {
                    return;
                }
            }

            const orden = esLayoutPago()
                ? ["#SucursalId", "#Fecha", "#NroRecibo", "#formaPago", "#Observaciones", "#btnGuardarPago"]
                : ["#Fecha", "#NroRecibo", "#formaPago", "#Observaciones", "#btnGuardarPago"];

            const indice = orden.indexOf(selectorActual);
            if (indice < 0) return;

            for (let i = indice + 1; i < orden.length; i++) {
                if (enfocarPrimeroDisponible([orden[i]])) {
                    return;
                }
            }
        }

        function abrirContadorBilletes() {
            const $launcher = $(".js-calculadora-billetes-launch:visible:not(:disabled)").first();
            if (!$launcher.length) {
                return false;
            }

            $launcher.trigger("click");
            return true;
        }

        function guardarPagoDesdeAtajo() {
            const $modify = $("#btnHabilitarEdicionPago:visible:not(:disabled)").first();
            if ($modify.length) {
                $modify.trigger("click");
                return true;
            }

            const $save = $("#btnGuardarPago:visible:not(:disabled)").first();
            if ($save.length) {
                if ($save.closest("form").length && $save.closest("form")[0] && typeof $save.closest("form")[0].requestSubmit === "function") {
                    $save.closest("form")[0].requestSubmit($save[0]);
                } else {
                    $save.trigger("click");
                }

                return true;
            }

            return false;
        }

        function bindGlobalShortcutsCapture() {
            if (window.__pagoUxGlobalShortcutsBound) {
                return;
            }

            window.__pagoUxGlobalShortcutsBound = true;

            window.addEventListener("keydown", function (e) {
                if (e.defaultPrevented) return;
                if (!$("#formPago").length) return;
                if (!e.altKey || e.ctrlKey || e.metaKey || e.shiftKey || e.repeat) return;

                const key = String(e.key || "").toLowerCase();

                if (key === "enter") {
                    e.preventDefault();
                    e.stopPropagation();
                    guardarPagoDesdeAtajo();
                    return;
                }

                if (key === "s" && esLayoutPago()) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (typeof window.volverDesdePago === "function") {
                        window.volverDesdePago();
                    }
                    return;
                }

                if (key === "c" && abrirContadorBilletes()) {
                    e.preventDefault();
                    e.stopPropagation();
                }
            }, true);
        }

        function setGuardando(activo) {
            submitEnCurso = !!activo;

            $("#btnGuardarPago, #btnAgregarCheque, #btnBuscarCheques, #btnFiltrarCheques")
                .prop("disabled", submitEnCurso);
        }

        function iniciarSubmit() {
            if (submitEnCurso) return false;
            setGuardando(true);
            return true;
        }

        function finalizarSubmit() {
            setGuardando(false);
        }

        function programarBusquedaCheques(delay) {
            window.clearTimeout(debounceBuscarCheques);
            debounceBuscarCheques = window.setTimeout(function () {
                $("#btnFiltrarCheques").trigger("click");
            }, delay || 220);
        }

        function bindChequeSearch() {
            $(document)
                .off("input.pagoUX", "#filtroNroCheque")
                .on("input.pagoUX", "#filtroNroCheque", function () {
                    programarBusquedaCheques(260);
                })
                .off("change.pagoUX", "#filtroEstado, #filtroDesde")
                .on("change.pagoUX", "#filtroEstado, #filtroDesde", function () {
                    programarBusquedaCheques(120);
                });
        }

        function bindFormFocus() {
            $(document)
                .off("change.pagoUX", "#formaPago")
                .on("change.pagoUX", "#formaPago", function () {
                    actualizarResumenModo();
                    setTimeout(enfocarCampoPrincipal, 80);
                })
                .off("shown.bs.modal.pagoUX", "#modalBuscarCheques")
                .on("shown.bs.modal.pagoUX", "#modalBuscarCheques", function () {
                    enfocar("#filtroNroCheque");
                })
                .off("hidden.bs.modal.pagoUX", "#modalBuscarCheques")
                .on("hidden.bs.modal.pagoUX", "#modalBuscarCheques", function () {
                    enfocar("#txtNroCheque");
                });
        }

        function bindKeyboardUX() {
            $(document)
                .off("keydown.pagoUXFlow", "#SucursalId, #Fecha, #NroRecibo, #formaPago, #importe, #Efectivo, #Observaciones")
                .on("keydown.pagoUXFlow", "#SucursalId, #Fecha, #NroRecibo, #formaPago, #importe, #Efectivo, #Observaciones", function (e) {
                    if (e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
                    if (e.key !== "Enter") return;

                    e.preventDefault();
                    enfocarSiguientePorEnter("#" + (this.id || ""));
                })
                .off("keydown.pagoUXShortcuts")
                .on("keydown.pagoUXShortcuts", function (e) {
                    if (!e.altKey || e.ctrlKey || e.metaKey || e.shiftKey || e.repeat) return;

                    const key = String(e.key || "").toLowerCase();

                    if (key === "s" && esLayoutPago()) {
                        e.preventDefault();
                        if (typeof window.volverDesdePago === "function") {
                            window.volverDesdePago();
                        }
                        return;
                    }

                    if (key === "c" && abrirContadorBilletes()) {
                        e.preventDefault();
                    }
                });
        }

        function bindSubmitGuard() {
            $(document)
                .off("submit.pagoUX", "#formPago")
                .on("submit.pagoUX", "#formPago", function (e) {
                    e.preventDefault();

                    if (!iniciarSubmit()) return false;
                    if (!validarAntesDeGuardar()) {
                        finalizarSubmit();
                        return false;
                    }

                    normalizarImportes();
                    construirChequesJson();
                    enviarFormularioAjax(this);
                    return false;
                });

            $(document)
                .off("ajaxComplete.pagoUX ajaxError.pagoUX")
                .on("ajaxComplete.pagoUX ajaxError.pagoUX", function (_evt, _xhr, settings) {
                    const url = ((settings && settings.url) || "").toLowerCase();
                    if (url.indexOf("/finanzas/addoreditpagopost") >= 0) {
                        finalizarSubmit();
                    }
                });
        }

        function init() {
            bindChequeSearch();
            bindFormFocus();
            bindKeyboardUX();
            bindGlobalShortcutsCapture();
            bindSubmitGuard();
            actualizarResumenModo();
            if (esLayoutPago()) {
                enfocarPrimeroDisponible([".pago-operacion-opcion[data-operacion-valor='true']", ".pago-operacion-opcion[data-operacion-valor='false']"]);
            } else {
                enfocarCampoPrincipal();
            }

            $(document)
                .off("click.pagoUXImprimir", "#btnImprimirPago")
                .on("click.pagoUXImprimir", "#btnImprimirPago", function () {
                    if (!window.PostPagoModal) return;

                    const pdfUrl = ($(this).data("pdf-url") || "").toString();
                    const imprimirUrl = ($(this).data("imprimir-url") || "").toString();
                    const imprimirPayloadUrl = ($(this).data("imprimir-payload-url") || "").toString();
                    const pagoId = parseInt($(this).data("pago-id"), 10) || 0;
                    const emailConfigUrl = ($(this).data("email-config-url") || "").toString();
                    const emailSendUrl = ($(this).data("email-send-url") || "").toString();

                    abrirModalPostPago({
                        pagoId: pagoId,
                        redirectUrl: "",
                        imprimirUrl: imprimirUrl,
                        imprimirPayloadUrl: imprimirPayloadUrl,
                        pdfUrl: pdfUrl,
                        emailConfigUrl: emailConfigUrl,
                        emailSendUrl: emailSendUrl
                    }, {
                        stayOnPage: true,
                        returnInPos: false
                    });
                });
        }

        return {
            init: init,
            enfocarCampoPrincipal: enfocarCampoPrincipal,
            iniciarSubmit: iniciarSubmit,
            finalizarSubmit: finalizarSubmit,
            actualizarResumenModo: actualizarResumenModo
        };
    })();
})(window, window.jQuery);
