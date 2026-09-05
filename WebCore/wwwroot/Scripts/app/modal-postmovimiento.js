// Version reducida de Web/Scripts/app/modal-postmovimiento.js -- sin ticket ESC/POS via agente
// local (print-agent.js no esta portado, ver _ModalPostMovimiento.cshtml). Mantiene "Generar PDF"
// y "Enviar a WhatsApp", que no dependen del agente.
(function () {
    var state = {
        redirectUrl: '',
        pdfUrl: '',
        whatsappTexto: '',
        stayOnPage: false
    };

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
            state.whatsappTexto = resp.whatsappTexto || '';
            state.stayOnPage = !!resp.stayOnPage;

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
            if (e.key === '2') { e.preventDefault(); $('#btnPostMovimientoPdf').click(); return; }
            if (e.key === '3') { e.preventDefault(); $('#btnPostMovimientoWhatsapp').click(); return; }
        });
    });
})();
