// Version reducida de Web/Scripts/app/modal-postmovimiento.js -- sin la UI de estado/config del
// agente local (indicador "Impresion local activa", "Configurar impresora", ver
// _ModalPostMovimiento.cshtml). Mantiene "Generar PDF" y "Enviar a WhatsApp" (oculto, ver abajo).
// Ticket termico (opcion 2, ver docs/DECISIONS.md "Batch 7" y la entrada de 2026-09-23): recuerda
// el tamaño en localStorage (clave propia de este modulo, no compartida con postventa_ticket_mm
// de Ventas) e imprime via TicketPrint (ticket-print.js): directo por el agente local si esta
// instalado, o por iframe oculto con el dialogo del navegador si no -- nunca pestaña nueva ni
// redireccion como efecto de imprimir.
(function () {
    var KEY_TICKET_MM = 'postmovimiento_ticket_mm';

    // Tope de espera al cerrar el modal antes de seguir de todos modos (red de seguridad, ver cerrarModal).
    var ESPERA_MAXIMA_CIERRE_MS = 1000;

    var state = {
        redirectUrl: '',
        pdfUrl: '',
        imprimirUrl: '',
        imprimirPayloadUrl: '',
        whatsappTexto: '',
        stayOnPage: false,
        accionElegida: false    // ya se eligio Continuar / Imprimir / PDF / WhatsApp: las demas se ignoran
    };

    // true desde open() hasta que Bootstrap termina de ocultar el modal. movimientos.js lo consulta para no
    // dejar guardar de nuevo el formulario mientras este aviso sigue arriba (ver estaAbierto).
    var abierto = false;

    function getUltimoTicketMm() {
        var v = null;
        try { v = window.localStorage.getItem(KEY_TICKET_MM); } catch (e) { }
        var mm = parseInt(v, 10);
        return (mm === 58 || mm === 80) ? mm : null;
    }

    function setUltimoTicketMm(mm) {
        try { window.localStorage.setItem(KEY_TICKET_MM, String(mm)); } catch (e) { }
    }

    function actualizarTextoTicket() {
        var mm = getUltimoTicketMm();
        var $lbl = $('#lblMovimientoTicketAccion');
        if (!mm) $lbl.text('Imprimir');
        else $lbl.text('Imprimir (' + mm + ' mm)');
    }

    function getBloqueTicketCollapse() {
        var el = document.getElementById('bloqueTicketOpcionesMovimiento');
        if (!el || !window.bootstrap || !window.bootstrap.Collapse) return null;
        return window.bootstrap.Collapse.getOrCreateInstance(el, { toggle: false });
    }

    function permitirSalidaSinAdvertencia() {
        $('form').each(function () {
            var guardApi = $(this).data('editPageGuardApi');
            if (guardApi && typeof guardApi.allowNavigation === 'function') {
                guardApi.allowNavigation();
            }
        });
        if (typeof window.desactivarProteccionSalida === 'function') {
            window.desactivarProteccionSalida();
        } else {
            window.__protegerSalida = false;
        }
    }

    // Registra que ya se eligio una accion final. Devuelve false si ya habia una: durante la animacion de cierre
    // (~300 ms) los botones siguen siendo clickeables y un doble click imprimia dos tickets o abria dos PDF.
    function tomarAccion() {
        if (state.accionElegida) return false;
        state.accionElegida = true;
        return true;
    }

    // Cierra el modal. Si se pasa `alCerrar`, lo ejecuta UNA sola vez cuando el modal ya quedo oculto de verdad
    // (evento hidden.bs.modal), o a los ESPERA_MAXIMA_CIERRE_MS como red de seguridad si ese evento no llegara.
    // Dos cuidados que antes no estaban:
    //  - Bootstrap ignora hide() mientras el modal todavia anima su APERTURA y lo dejaba abierto para siempre (mismo
    //    cuidado que post-comprobante.js): en ese caso se espera al 'shown' y recien ahi se cierra.
    //  - Ocultar tarda ~300 ms y window.print() congela la pagina: si la impresion arranca antes de que termine, el
    //    dialogo de impresion se abre con el modal a medio cerrar y, al cancelarlo, "Movimiento guardado" parece
    //    seguir ahi. Por eso imprimirTicket() imprime desde `alCerrar`.
    // _isShown / _isTransitioning son internos de Bootstrap (los usan tambien modal-stack.js y post-comprobante.js).
    function cerrarModal(alCerrar) {
        var $modal = $('#modalPostMovimiento');
        var instancia = window.bootstrap && window.bootstrap.Modal ? window.bootstrap.Modal.getInstance($modal[0]) : null;
        var mostrado = !!(instancia && instancia._isShown);
        var animando = !!(instancia && instancia._isTransitioning);

        if (typeof alCerrar === 'function') {
            if (!mostrado && !animando) {
                // Ya estaba cerrado del todo: no va a llegar ningun 'hidden'.
                alCerrar();
            } else {
                var ejecutado = false;
                var ejecutar = function () {
                    if (ejecutado) return;
                    ejecutado = true;
                    alCerrar();
                };
                $modal.one('hidden.bs.modal', ejecutar);
                window.setTimeout(ejecutar, ESPERA_MAXIMA_CIERRE_MS);
            }
        }

        // Cerrado o cerrandose: el 'hidden' llega solo.
        if (!mostrado) return;

        // Todavia abriendose: hide() seria ignorado.
        if (animando) {
            $modal.one('shown.bs.modal', function () { $modal.modal('hide'); });
            return;
        }

        $modal.modal('hide');
    }

    function cerrarYRedirigir() {
        cerrarModal();
        if (state.stayOnPage) {
            return;
        }
        if (state.redirectUrl) {
            window.location.href = state.redirectUrl;
        }
    }

    function abrirNuevaVentana(url) {
        if (!url) return;
        window.open(url, '_blank', 'noopener');
    }

    function conMm(url, mm) {
        if (!url) return '';
        return url + (url.indexOf('?') >= 0 ? '&' : '?') + 'mm=' + mm;
    }

    // Imprime el ticket sin salir de la vista: solo cierra el modal (no redirige, a diferencia de
    // "Continuar"/"No imprimir"). Pedido explicito del usuario, ver docs/DECISIONS.md 2026-09-23.
    // Primero cierra el modal y recien cuando quedo oculto manda a imprimir (ver cerrarModal).
    function imprimirTicket(mm) {
        if (!tomarAccion()) return;

        // Las URLs se leen ahora, no dentro del callback: el modal podria reabrirse con otro movimiento antes.
        var opcionesTicket = {
            ticketUrl: conMm(state.imprimirUrl, mm),
            payloadUrl: conMm(state.imprimirPayloadUrl, mm)
        };

        cerrarModal(function () {
            window.TicketPrint.imprimir(opcionesTicket);
        });
    }

    function abrirWhatsapp() {
        var texto = state.whatsappTexto || '';
        var url = 'https://wa.me/?text=' + encodeURIComponent(texto);
        window.open(url, '_blank', 'noopener');
    }

    window.PostMovimientoModal = {
        open: function (resp) {
            permitirSalidaSinAdvertencia();
            state.redirectUrl = resp.redirectUrl || '';
            state.pdfUrl = resp.pdfUrl || '';
            state.imprimirUrl = resp.imprimirUrl || '';
            state.imprimirPayloadUrl = resp.imprimirPayloadUrl || '';
            state.whatsappTexto = resp.whatsappTexto || '';
            state.stayOnPage = !!resp.stayOnPage;
            state.accionElegida = false;
            abierto = true;
            actualizarTextoTicket();
            var collapse = getBloqueTicketCollapse();
            if (collapse) collapse.hide();

            // Bootstrap 5: .modal(objetoOpciones) solo crea/configura la instancia, NO la muestra
            // (a diferencia de BS4) -- hace falta el .modal('show') explicito de abajo. Mismo bug
            // ya documentado y corregido en calculadora-billetes.js; 2da aparicion => regla, no
            // parche puntual (CLAUDE.md §5.1): en WebCore, todo .modal({...}) va seguido de
            // .modal('show') si el objetivo es abrirlo.
            $('#modalPostMovimiento').modal({
                backdrop: 'static',
                keyboard: false
            });
            $('#modalPostMovimiento').modal('show');
        },

        // true mientras el aviso "Movimiento guardado" sigue en pantalla (desde open() hasta que termina de ocultarse).
        estaAbierto: function () {
            return abierto;
        }
    };

    $(function () {
        $('#modalPostMovimiento').on('shown.bs.modal', function () {
            setTimeout(function () {
                $('#btnPostMovimientoNoImprimir').trigger('focus');
            }, 50);
        });

        $('#modalPostMovimiento').on('hidden.bs.modal', function () {
            abierto = false;
        });

        $('#btnPostMovimientoNoImprimir').on('click', function () {
            if (!tomarAccion()) return;
            cerrarYRedirigir();
        });

        // Boton 2 "Imprimir": con un tamaño ya recordado, imprime directo; sin tamaño recordado,
        // despliega el sub-bloque de opciones en vez de abrir nada (mismo criterio que "Cambiar",
        // que siempre despliega/oculta el sub-bloque aunque ya haya un tamaño recordado).
        $('#btnPostMovimientoImprimir').on('click', function () {
            var mm = getUltimoTicketMm();
            if (mm) {
                imprimirTicket(mm);
                return;
            }

            var collapse = getBloqueTicketCollapse();
            if (collapse) collapse.show();
        });

        $('#btnCambiarTicketMovimiento').on('click', function () {
            var collapse = getBloqueTicketCollapse();
            if (collapse) collapse.toggle();
        });

        $('.btnTicketMovimientoOpt').on('click', function () {
            var mm = parseInt($(this).data('mm'), 10);
            if (mm !== 58 && mm !== 80) return;

            setUltimoTicketMm(mm);
            actualizarTextoTicket();
            var collapse = getBloqueTicketCollapse();
            if (collapse) collapse.hide();

            imprimirTicket(mm);
        });

        $('#btnPostMovimientoPdf').on('click', function () {
            if (!tomarAccion()) return;
            abrirNuevaVentana(state.pdfUrl);
            cerrarYRedirigir();
        });

        $('#btnPostMovimientoWhatsapp').on('click', function () {
            if (!tomarAccion()) return;
            abrirWhatsapp();
            cerrarYRedirigir();
        });

        $(document).on('keydown.postMovimientoModal', function (e) {
            var $modal = $('#modalPostMovimiento');
            if (!$modal.hasClass('show')) return;

            if (e.key === '1') { e.preventDefault(); $('#btnPostMovimientoNoImprimir').click(); return; }
            if (e.key === '2') { e.preventDefault(); $('#btnPostMovimientoImprimir').click(); return; }
            if (e.key === '3') { e.preventDefault(); $('#btnPostMovimientoPdf').click(); return; }
            // Atajo "4" (WhatsApp) desactivado: el boton esta oculto (d-none, pedido explicito del
            // usuario) -- no tiene sentido mostrar un atajo de teclado para una accion invisible.
            // El boton/id/handler de WhatsApp en si NO se tocan.
        });
    });
})();
