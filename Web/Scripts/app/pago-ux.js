(function (window, $) {
    if (!window || !$) return;

    // Mejora UX del formulario de pagos:
    // - foco inicial y foco segun forma de pago
    // - busqueda viva de cheques en el modal
    // - proteccion contra doble submit
    window.PagoUX = window.PagoUX || (function () {
        let submitEnCurso = false;
        let debounceBuscarCheques = null;

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
                .on("submit.pagoUX", "#formPago", function () {
                    return iniciarSubmit();
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
