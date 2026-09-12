// Version reducida de Web/Scripts/app/modal-postmovimiento.js -- sin ticket ESC/POS via agente
// local (print-agent.js no esta portado, ver _ModalPostMovimiento.cshtml). Mantiene "Generar PDF"
// y "Enviar a WhatsApp" (oculto, ver abajo), que no dependen del agente. Ticket termico HTML
// (opcion 2, cuarta ronda de pedidos, 2026-09-10 -- ver docs/DECISIONS.md "Batch 7") agregado:
// recuerda el tamaño en localStorage (clave propia de este modulo, no compartida con
// postventa_ticket_mm de Ventas), mismo patron de "abrir en pestaña nueva, la vista del ticket
// dispara window.print() sola" ya usado en Ventas/POS.cshtml -- mejora deliberada frente al
// clasico, que usa un <iframe> oculto (ver entrada de DECISIONS.md de este batch).
(function () {
    var KEY_TICKET_MM = 'postmovimiento_ticket_mm';

    var state = {
        redirectUrl: '',
        pdfUrl: '',
        imprimirUrl: '',
        whatsappTexto: '',
        stayOnPage: false
    };

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

    function cerrarModal() {
        $('#modalPostMovimiento').modal('hide');
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
            state.whatsappTexto = resp.whatsappTexto || '';
            state.stayOnPage = !!resp.stayOnPage;
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
        }
    };

    $(function () {
        $('#modalPostMovimiento').on('shown.bs.modal', function () {
            setTimeout(function () {
                $('#btnPostMovimientoNoImprimir').trigger('focus');
            }, 50);
        });

        $('#btnPostMovimientoNoImprimir').on('click', function () {
            cerrarYRedirigir();
        });

        // Boton 2 "Imprimir": con un tamaño ya recordado, imprime directo; sin tamaño recordado,
        // despliega el sub-bloque de opciones en vez de abrir nada (mismo criterio que "Cambiar",
        // que siempre despliega/oculta el sub-bloque aunque ya haya un tamaño recordado).
        $('#btnPostMovimientoImprimir').on('click', function () {
            var mm = getUltimoTicketMm();
            if (mm) {
                abrirNuevaVentana(state.imprimirUrl + (state.imprimirUrl.indexOf('?') >= 0 ? '&' : '?') + 'mm=' + mm);
                cerrarYRedirigir();
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

            abrirNuevaVentana(state.imprimirUrl + (state.imprimirUrl.indexOf('?') >= 0 ? '&' : '?') + 'mm=' + mm);
            cerrarYRedirigir();
        });

        $('#btnPostMovimientoPdf').on('click', function () {
            abrirNuevaVentana(state.pdfUrl);
            cerrarYRedirigir();
        });

        $('#btnPostMovimientoWhatsapp').on('click', function () {
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
