(function () {
    var KEY_TICKET_MM = 'postmovimiento_ticket_mm';

    var state = {
        redirectUrl: '',
        imprimirUrl: '',
        imprimirPayloadUrl: '',
        pdfUrl: '',
        whatsappTexto: '',
        stayOnPage: false,
        ticketMmActual: null,
        agentAvailable: false,
        agentChecked: false,
        printerName: ''
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
        $('#lblMovimientoTicketAccion').text(mm ? ('Imprimir (' + mm + ' mm)') : 'Imprimir');
    }

    function marcarTicketSeleccionado(mm) {
        $('.btnTicketMovimientoOpt').removeClass('active btn-primary').addClass('btn-outline-primary');
        var $btn = $('.btnTicketMovimientoOpt[data-mm="' + mm + '"]');
        $btn.addClass('active btn-primary').removeClass('btn-outline-primary');
        state.ticketMmActual = mm;
    }

    function abrirOpcionesTicket(preseleccionarMm) {
        $('#bloqueTicketOpcionesMovimiento').collapse('show');
        marcarTicketSeleccionado(preseleccionarMm || getUltimoTicketMm() || 58);
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

    function buildPayloadUrl(mm) {
        if (!state.imprimirPayloadUrl) return '';
        var sep = state.imprimirPayloadUrl.indexOf('?') >= 0 ? '&' : '?';
        return state.imprimirPayloadUrl + sep + 'mm=' + mm;
    }

    function actualizarEstadoAgente(texto, disponible) {
        $('#estadoAgenteMovimiento')
            .text(texto)
            .toggleClass('text-success', !!disponible)
            .toggleClass('text-muted', !disponible);
    }

    function refrescarEstadoAgente() {
        if (!state.agentChecked) {
            actualizarEstadoAgente('Impresión local: verificando agente...', false);
            return;
        }

        if (state.agentAvailable) {
            var texto = 'Impresión local activa';
            if (state.printerName) {
                texto += ': ' + state.printerName;
            }
            actualizarEstadoAgente(texto + '.', true);
            return;
        }

        actualizarEstadoAgente('Impresión local: usando navegador.', false);
    }

    function verificarAgente() {
        var dfd = $.Deferred();
        if (!window.CarniSysPrintAgent) {
            state.agentChecked = true;
            state.agentAvailable = false;
            state.printerName = '';
            refrescarEstadoAgente();
            return dfd.resolve(false).promise();
        }

        window.CarniSysPrintAgent.health()
            .done(function (resp) {
                state.agentChecked = true;
                state.agentAvailable = !!(resp && resp.ok);
                state.printerName = resp && resp.printerName ? resp.printerName : '';
                refrescarEstadoAgente();
                dfd.resolve(state.agentAvailable);
            })
            .fail(function () {
                state.agentChecked = true;
                state.agentAvailable = false;
                state.printerName = '';
                refrescarEstadoAgente();
                dfd.resolve(false);
            });

        return dfd.promise();
    }

    function cargarConfiguracionAgente() {
        var dfd = $.Deferred();
        if (!window.CarniSysPrintAgent) return dfd.reject().promise();

        $.when(window.CarniSysPrintAgent.getPrinters(), window.CarniSysPrintAgent.getConfig())
            .done(function (printersResp, configResp) {
                var printersData = printersResp[0] || {};
                var configData = configResp[0] || {};
                var items = printersData.items || [];
                var $cmb = $('#cmbImpresoraAgenteMovimiento');
                $cmb.empty();

                $.each(items, function (_, item) {
                    var printerName = item.name || item.Name || '';
                    var isDefault = !!(item.isDefault || item.IsDefault);
                    $('<option>')
                        .val(printerName)
                        .text(printerName + (isDefault ? ' (predeterminada)' : ''))
                        .appendTo($cmb);
                });

                if (configData.printerName) {
                    $cmb.val(configData.printerName);
                }

                $('#cmbMmAgenteMovimiento').val((configData.ticketMm === 80 ? 80 : 58).toString());
                dfd.resolve();
            })
            .fail(function () {
                dfd.reject();
            });

        return dfd.promise();
    }

    function imprimirTicket(mm) {
        var url = buildTicketUrl(mm);
        if (!url) return;

        $('#iframePrintMovimiento').remove();

        var $iframe = $('<iframe>', {
            id: 'iframePrintMovimiento',
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

    function imprimirConAgente(mm) {
        var payloadUrl = buildPayloadUrl(mm);
        if (!payloadUrl || !window.CarniSysPrintAgent) {
            imprimirTicket(mm);
            return;
        }

        $.getJSON(payloadUrl)
            .done(function (payload) {
                if (!payload || payload.ok === false) {
                    imprimirTicket(mm);
                    return;
                }

                window.CarniSysPrintAgent.printExpendio(payload)
                    .done(function () {
                        verificarAgente();
                        cerrarLuegoDeImprimir(400);
                    })
                    .fail(function () {
                        imprimirTicket(mm);
                    });
            })
            .fail(function () {
                imprimirTicket(mm);
            });
    }

    window.PostMovimientoModal = {
        open: function (resp) {
            state.redirectUrl = resp.redirectUrl || '';
            state.imprimirUrl = resp.imprimirUrl || '';
            state.imprimirPayloadUrl = resp.imprimirPayloadUrl || '';
            state.pdfUrl = resp.pdfUrl || '';
            state.whatsappTexto = resp.whatsappTexto || '';
            state.stayOnPage = !!resp.stayOnPage;
            state.ticketMmActual = null;

            $('#modalPostMovimiento').modal({
                backdrop: 'static',
                keyboard: false,
                show: true
            });
        }
    };

    $(function () {
        $('#modalPostMovimiento').on('shown.bs.modal', function () {
            $('#bloqueTicketOpcionesMovimiento').collapse('hide');
            state.ticketMmActual = null;
            actualizarTextoTicket();
            refrescarEstadoAgente();
            verificarAgente();
            setTimeout(function () {
                $('#btnPostMovimientoImprimir').trigger('focus');
            }, 50);
        });

        $('#btnPostMovimientoNoImprimir').on('click', function () {
            cerrarYRedirigir();
        });

        $('#btnPostMovimientoImprimir').on('click', function () {
            var mm = getUltimoTicketMm();
            if (mm) {
                if (state.agentAvailable) {
                    imprimirConAgente(mm);
                } else {
                    imprimirTicket(mm);
                }
            } else {
                abrirOpcionesTicket(58);
            }
        });

        $('#btnCambiarTicketMovimiento').on('click', function () {
            abrirOpcionesTicket();
        });

        $(document).on('click', '.btnTicketMovimientoOpt', function () {
            var mm = parseInt($(this).data('mm'), 10);
            if (mm !== 58 && mm !== 80) return;
            setUltimoTicketMm(mm);
            actualizarTextoTicket();
            marcarTicketSeleccionado(mm);
            if (state.agentAvailable) {
                imprimirConAgente(mm);
            } else {
                imprimirTicket(mm);
            }
        });

        $('#btnConfigurarAgenteMovimiento').on('click', function () {
            $('#msgConfigAgenteMovimiento').addClass('d-none').text('');

            verificarAgente().done(function (available) {
                if (!available) {
                    $('#msgConfigAgenteMovimiento')
                        .removeClass('d-none')
                        .text('No se detectó el agente local. Descargalo e instalalo en esta terminal.');
                    $('#modalConfigAgenteMovimiento').modal('show');
                    return;
                }

                cargarConfiguracionAgente()
                    .done(function () {
                        $('#modalConfigAgenteMovimiento').modal('show');
                    })
                    .fail(function () {
                        $('#msgConfigAgenteMovimiento')
                            .removeClass('d-none')
                            .text('No se pudieron leer las impresoras instaladas.');
                        $('#modalConfigAgenteMovimiento').modal('show');
                    });
            });
        });

        $('#btnGuardarConfigAgenteMovimiento').on('click', function () {
            if (!window.CarniSysPrintAgent) return;

            var printerName = ($('#cmbImpresoraAgenteMovimiento').val() || '').toString();
            var ticketMm = parseInt($('#cmbMmAgenteMovimiento').val(), 10);
            window.CarniSysPrintAgent.saveConfig({
                printerName: printerName,
                ticketMm: ticketMm === 80 ? 80 : 58
            }).done(function () {
                if (ticketMm === 58 || ticketMm === 80) {
                    setUltimoTicketMm(ticketMm);
                    actualizarTextoTicket();
                }
                $('#modalConfigAgenteMovimiento').modal('hide');
                verificarAgente();
            }).fail(function () {
                $('#msgConfigAgenteMovimiento')
                    .removeClass('d-none')
                    .text('No se pudo guardar la configuración de impresión.');
            });
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
            if (e.key === '4') { e.preventDefault(); $('#btnPostMovimientoWhatsapp').click(); return; }

            var ticketVisible = $('#bloqueTicketOpcionesMovimiento').hasClass('show');
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
                if (state.agentAvailable) {
                    imprimirConAgente(mm);
                } else {
                    imprimirTicket(mm);
                }
            }
        });
    });
})();
