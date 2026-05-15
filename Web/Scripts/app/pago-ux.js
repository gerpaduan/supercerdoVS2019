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

        function resolverRedirectNoPos(resp) {
            const redirectUrl = (resp && resp.redirectUrl) || $("#urlVolverPago").val() || window.location.href;

            if (window.PostPagoModal && resp && resp.imprimirUrl) {
                    window.PostPagoModal.open({
                        redirectUrl: redirectUrl,
                        imprimirUrl: resp.imprimirUrl,
                        imprimirPayloadUrl: resp.imprimirPayloadUrl,
                        pdfUrl: resp.pdfUrl,
                        whatsappTexto: resp.whatsappTexto,
                        stayOnPage: false
                });
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

            if (window.PostPagoModal && resp && resp.imprimirUrl) {
                if (resp && resp.cerrarModalPago && $("#modalPagoPOS").length) {
                    $("#modalPagoPOS").modal("hide");
                }

                window.setTimeout(function () {
                    window.PostPagoModal.open({
                        redirectUrl: resp.redirectUrl,
                        imprimirUrl: resp.imprimirUrl,
                        imprimirPayloadUrl: resp.imprimirPayloadUrl,
                        pdfUrl: resp.pdfUrl,
                        whatsappTexto: resp.whatsappTexto,
                        stayOnPage: false
                    });
                }, 250);
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

            const esCheque =
                valor === "cheque" ||
                valor === "eftvocheque" ||
                valor === "efvtocheque" ||
                texto === "cheque" ||
                texto === "eftvocheque" ||
                texto === "efvtocheque";

            const esMixto =
                valor === "eftvocheque" ||
                valor === "efvtocheque" ||
                texto === "eftvocheque" ||
                texto === "efvtocheque";

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
            bindSubmitGuard();
            actualizarResumenModo();
            enfocarCampoPrincipal();

            $(document)
                .off("click.pagoUXImprimir", "#btnImprimirPago")
                .on("click.pagoUXImprimir", "#btnImprimirPago", function () {
                    if (!window.PostPagoModal) return;

                    const pdfUrl = ($(this).data("pdf-url") || "").toString();
                    const imprimirUrl = ($(this).data("imprimir-url") || "").toString();
                    const imprimirPayloadUrl = ($(this).data("imprimir-payload-url") || "").toString();
                    let whatsappTexto = "Recibo";

                    if (pdfUrl) {
                        whatsappTexto = "Recibo - " + new URL(pdfUrl, window.location.origin).toString();
                    }

                    window.PostPagoModal.open({
                        redirectUrl: "",
                        imprimirUrl: imprimirUrl,
                        imprimirPayloadUrl: imprimirPayloadUrl,
                        pdfUrl: pdfUrl,
                        whatsappTexto: whatsappTexto,
                        stayOnPage: true
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
