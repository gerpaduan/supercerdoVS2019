(function () {
    var KEY_TICKET_MM = 'postpago_ticket_mm';

    var state = {
        redirectUrl: '',
        imprimirUrl: '',
        pdfUrl: '',
        whatsappTexto: '',
        stayOnPage: false,
        ticketMmActual: null
    };

    function getUltimoTicketMm() {
        var raw = localStorage.getItem(KEY_TICKET_MM);
        var mm = parseInt(raw, 10);
        return (mm === 58 || mm === 80) ? mm : null;
    }

    function setUltimoTicketMm(mm) {
        localStorage.setItem(KEY_TICKET_MM, String(mm));
    }

    function actualizarTextoTicket() {
        var mm = getUltimoTicketMm();
        $('#lblPagoTicketAccion').text(mm ? ('Imprimir (' + mm + ' mm)') : 'Imprimir');
    }

    function marcarTicketSeleccionado(mm) {
        $('.btnTicketPagoOpt').removeClass('active btn-primary').addClass('btn-outline-primary');
        var $btn = $('.btnTicketPagoOpt[data-mm="' + mm + '"]');
        $btn.addClass('active btn-primary').removeClass('btn-outline-primary');
        state.ticketMmActual = mm;
    }

    function abrirOpcionesTicket(preseleccionarMm) {
        $('#bloqueTicketOpcionesPago').collapse('show');
        marcarTicketSeleccionado(preseleccionarMm || getUltimoTicketMm() || 58);
    }

    function cerrarModal() {
        $('#modalPostPago').modal('hide');
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

    function cerrarLuegoDeImprimir(delayMs) {
        window.setTimeout(function () {
            cerrarYRedirigir();
        }, delayMs || 1200);
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

    function buildTicketUrl(mm) {
        if (!state.imprimirUrl) return '';
        var sep = state.imprimirUrl.indexOf('?') >= 0 ? '&' : '?';
        return state.imprimirUrl + sep + 'mm=' + mm;
    }

    function imprimirTicket(mm) {
        var url = buildTicketUrl(mm);
        if (!url) return;

        $('#iframePrintPago').remove();

        var $iframe = $('<iframe>', {
            id: 'iframePrintPago',
            style: 'position:fixed; right:0; bottom:0; width:1px; height:1px; border:0; opacity:0; pointer-events:none;',
            src: url
        });

        $iframe.on('load', function () {
            try {
                var frameWindow = this.contentWindow;
                if (frameWindow) {
                    frameWindow.focus();
                    if (typeof frameWindow.print === 'function') {
                        frameWindow.print();
                    }
                }
            } catch (e) { }

            cerrarLuegoDeImprimir(1200);
        });

        $('body').append($iframe);
    }

    window.PostPagoModal = {
        open: function (resp) {
            state.redirectUrl = resp.redirectUrl || '';
            state.imprimirUrl = resp.imprimirUrl || '';
            state.pdfUrl = resp.pdfUrl || '';
            state.whatsappTexto = resp.whatsappTexto || '';
            state.stayOnPage = !!resp.stayOnPage;
            state.ticketMmActual = null;

            $('#modalPostPago').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    };

    $(function () {
        $('#modalPostPago').on('shown.bs.modal', function () {
            $('#bloqueTicketOpcionesPago').collapse('hide');
            state.ticketMmActual = null;
            actualizarTextoTicket();
            setTimeout(function () {
                $('#btnPostPagoImprimir').trigger('focus');
            }, 50);
        });

        $('#btnPostPagoNoImprimir').on('click', function () {
            cerrarYRedirigir();
        });

        $('#btnPostPagoImprimir').on('click', function () {
            var mm = getUltimoTicketMm();
            if (mm) {
                imprimirTicket(mm);
            } else {
                abrirOpcionesTicket(58);
            }
        });

        $('#btnCambiarTicketPago').on('click', function () {
            abrirOpcionesTicket();
        });

        $(document).on('click', '.btnTicketPagoOpt', function () {
            var mm = parseInt($(this).data('mm'), 10);
            if (mm !== 58 && mm !== 80) return;
            setUltimoTicketMm(mm);
            actualizarTextoTicket();
            marcarTicketSeleccionado(mm);
            imprimirTicket(mm);
        });

        $('#btnPostPagoPdf').on('click', function () {
            abrirNuevaVentana(state.pdfUrl);
            cerrarYRedirigir();
        });

        $('#btnPostPagoWhatsapp').on('click', function () {
            abrirWhatsapp();
            cerrarYRedirigir();
        });
    });
})();
