(function () {
    var KEY_TICKET_MM = 'postexpendio_ticket_mm';

    var state = {
        redirectUrl: '',
        imprimirUrl: '',
        pdfUrl: '',
        whatsappTexto: '',
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
        $('#lblPuntoExpendioTicketAccion').text(mm ? ('Imprimir (' + mm + ' mm)') : 'Imprimir');
    }

    function marcarTicketSeleccionado(mm) {
        $('.btnTicketPuntoExpendioOpt').removeClass('active btn-primary').addClass('btn-outline-primary');
        var $btn = $('.btnTicketPuntoExpendioOpt[data-mm="' + mm + '"]');
        $btn.addClass('active btn-primary').removeClass('btn-outline-primary');
        state.ticketMmActual = mm;
    }

    function abrirOpcionesTicket(preseleccionarMm) {
        $('#bloqueTicketOpcionesPuntoExpendio').collapse('show');
        marcarTicketSeleccionado(preseleccionarMm || getUltimoTicketMm() || 58);
    }

    function cerrarModal() {
        $('#modalPostPuntoExpendio').modal('hide');
    }

    function cerrarYRedirigir() {
        cerrarModal();
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

        $('#iframePrintPuntoExpendio').remove();

        var $iframe = $('<iframe>', {
            id: 'iframePrintPuntoExpendio',
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

    window.PostPuntoExpendioModal = {
        open: function (resp) {
            state.redirectUrl = resp.redirectUrl || '';
            state.imprimirUrl = resp.imprimirUrl || '';
            state.pdfUrl = resp.pdfUrl || '';
            state.whatsappTexto = resp.whatsappTexto || '';
            state.ticketMmActual = null;

            $('#modalPostPuntoExpendio').modal({
                show: true
            });
        }
    };

    $(function () {
        $('#modalPostPuntoExpendio').on('shown.bs.modal', function () {
            $('#bloqueTicketOpcionesPuntoExpendio').collapse('hide');
            state.ticketMmActual = null;
            actualizarTextoTicket();
            setTimeout(function () {
                $('#btnPostPuntoExpendioImprimir').trigger('focus');
            }, 50);
        });

        $('#btnPostPuntoExpendioNoImprimir').on('click', function () {
            cerrarYRedirigir();
        });

        $('#btnPostPuntoExpendioImprimir').on('click', function () {
            var mm = getUltimoTicketMm();
            if (mm) {
                imprimirTicket(mm);
            } else {
                abrirOpcionesTicket(58);
            }
        });

        $('#btnCambiarTicketPuntoExpendio').on('click', function () {
            abrirOpcionesTicket();
        });

        $(document).on('click', '.btnTicketPuntoExpendioOpt', function () {
            var mm = parseInt($(this).data('mm'), 10);
            if (mm !== 58 && mm !== 80) return;
            setUltimoTicketMm(mm);
            actualizarTextoTicket();
            marcarTicketSeleccionado(mm);
            imprimirTicket(mm);
        });

        $('#btnPostPuntoExpendioPdf').on('click', function () {
            abrirNuevaVentana(state.pdfUrl);
            cerrarYRedirigir();
        });

        $('#btnPostPuntoExpendioWhatsapp').on('click', function () {
            abrirWhatsapp();
            cerrarYRedirigir();
        });

        $(document).on('keydown.postPuntoExpendioModal', function (e) {
            var $modal = $('#modalPostPuntoExpendio');
            if (!$modal.hasClass('show')) return;

            if (e.key === '1') { e.preventDefault(); $('#btnPostPuntoExpendioNoImprimir').click(); return; }
            if (e.key === '2') { e.preventDefault(); $('#btnPostPuntoExpendioImprimir').click(); return; }
            if (e.key === '3') { e.preventDefault(); $('#btnPostPuntoExpendioPdf').click(); return; }
            if (e.key === '4') { e.preventDefault(); $('#btnPostPuntoExpendioWhatsapp').click(); return; }

            var ticketVisible = $('#bloqueTicketOpcionesPuntoExpendio').hasClass('show');
            if (!ticketVisible) return;

            if (e.key === 'ArrowDown' || e.key === 'ArrowRight' || e.key === 'ArrowUp' || e.key === 'ArrowLeft') {
                e.preventDefault();
                var actual = state.ticketMmActual || getUltimoTicketMm() || 58;
                var nuevo = actual === 58 ? 80 : 58;
                marcarTicketSeleccionado(nuevo);
                return;
            }

            if (e.key === 'Enter') {
                e.preventDefault();
                var mm = state.ticketMmActual || getUltimoTicketMm() || 58;
                setUltimoTicketMm(mm);
                actualizarTextoTicket();
                imprimirTicket(mm);
            }
        });
    });
})();
