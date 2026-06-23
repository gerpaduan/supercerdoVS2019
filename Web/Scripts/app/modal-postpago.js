(function () {
    var KEY_TICKET_MM = 'postpago_ticket_mm';

    var state = {
        pagoId: 0,
        redirectUrl: '',
        imprimirUrl: '',
        imprimirPayloadUrl: '',
        pdfUrl: '',
        emailConfigUrl: '',
        emailSendUrl: '',
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
        $('#estadoAgentePago')
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
                var $cmb = $('#cmbImpresoraAgentePago');
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

                $('#cmbMmAgentePago').val((configData.ticketMm === 80 ? 80 : 58).toString());
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

    function esEmailValido(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email || '').trim());
    }

    function abrirModalEmail() {
        if (!state.pagoId || !state.emailConfigUrl) {
            Swal.fire({ icon: 'error', title: 'Email', text: 'No se pudo preparar el envio por email.' });
            return;
        }

        $('#ppEmailError').addClass('d-none').text('');
        $('#ppEmailDestino').val('');
        $('#ppEmailAsunto').val('');
        $('#ppEmailMensaje').val('');
        $('#btnConfirmarEmailPago').prop('disabled', true);

        $.getJSON(state.emailConfigUrl, { id: state.pagoId })
            .done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'Email', text: (resp && resp.msg) || 'No se pudieron recuperar los datos del email.' });
                    return;
                }

                $('#ppEmailDestino').val(resp.email || '');
                $('#ppEmailAsunto').val(resp.asunto || '');
                $('#ppEmailMensaje').val(resp.mensaje || '');
                $('#btnConfirmarEmailPago').prop('disabled', false);
                $('#modalEmailPago').modal('show');
            })
            .fail(function () {
                Swal.fire({ icon: 'error', title: 'Email', text: 'No se pudieron recuperar los datos del email.' });
            });
    }

    function confirmarEmail() {
        var payload = {
            idPago: state.pagoId,
            emailDestino: ($('#ppEmailDestino').val() || '').trim(),
            asunto: ($('#ppEmailAsunto').val() || '').trim(),
            mensaje: ($('#ppEmailMensaje').val() || '').trim()
        };

        if (!state.emailSendUrl || !payload.idPago) {
            $('#ppEmailError').removeClass('d-none').text('No se pudo preparar el envio por email.');
            return;
        }

        if (!payload.emailDestino) {
            $('#ppEmailError').removeClass('d-none').text('Ingresa el email destino.');
            return;
        }

        if (!esEmailValido(payload.emailDestino)) {
            $('#ppEmailError').removeClass('d-none').text('Ingresa un email valido.');
            return;
        }

        if (!payload.asunto) {
            $('#ppEmailError').removeClass('d-none').text('Ingresa el asunto del email.');
            return;
        }

        $('#ppEmailError').addClass('d-none').text('');
        $('#btnConfirmarEmailPago').prop('disabled', true);

        $.ajax({
            url: state.emailSendUrl,
            type: 'POST',
            dataType: 'json',
            data: payload
        }).done(function (resp) {
            if (!resp || !resp.ok) {
                $('#btnConfirmarEmailPago').prop('disabled', false);
                $('#ppEmailError').removeClass('d-none').text((resp && resp.msg) || 'No se pudo enviar el email.');
                return;
            }

            $('#modalEmailPago').modal('hide');
            Swal.fire({ icon: 'success', title: 'Email', text: resp.msg || 'El recibo se envio correctamente.' });
            cerrarYRedirigir();
        }).fail(function (xhr) {
            var msg = (xhr.responseJSON && xhr.responseJSON.msg) || 'No se pudo enviar el email.';
            $('#btnConfirmarEmailPago').prop('disabled', false);
            $('#ppEmailError').removeClass('d-none').text(msg);
        });
    }

    window.PostPagoModal = {
        open: function (resp) {
            state.pagoId = resp.pagoId || 0;
            state.redirectUrl = resp.redirectUrl || '';
            state.imprimirUrl = resp.imprimirUrl || '';
            state.imprimirPayloadUrl = resp.imprimirPayloadUrl || '';
            state.pdfUrl = resp.pdfUrl || '';
            state.emailConfigUrl = resp.emailConfigUrl || '';
            state.emailSendUrl = resp.emailSendUrl || '';
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
            refrescarEstadoAgente();
            verificarAgente();
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
                if (state.agentAvailable) {
                    imprimirConAgente(mm);
                } else {
                    imprimirTicket(mm);
                }
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
            if (state.agentAvailable) {
                imprimirConAgente(mm);
            } else {
                imprimirTicket(mm);
            }
        });

        $('#btnConfigurarAgentePago').on('click', function () {
            $('#msgConfigAgentePago').addClass('d-none').text('');

            verificarAgente().done(function (available) {
                if (!available) {
                    $('#msgConfigAgentePago')
                        .removeClass('d-none')
                        .text('No se detectó el agente local. Descargalo e instalalo en esta terminal.');
                    $('#modalConfigAgentePago').modal('show');
                    return;
                }

                cargarConfiguracionAgente()
                    .done(function () {
                        $('#modalConfigAgentePago').modal('show');
                    })
                    .fail(function () {
                        $('#msgConfigAgentePago')
                            .removeClass('d-none')
                            .text('No se pudieron leer las impresoras instaladas.');
                        $('#modalConfigAgentePago').modal('show');
                    });
            });
        });

        $('#btnGuardarConfigAgentePago').on('click', function () {
            if (!window.CarniSysPrintAgent) return;

            var printerName = ($('#cmbImpresoraAgentePago').val() || '').toString();
            var ticketMm = parseInt($('#cmbMmAgentePago').val(), 10);
            window.CarniSysPrintAgent.saveConfig({
                printerName: printerName,
                ticketMm: ticketMm === 80 ? 80 : 58
            }).done(function () {
                if (ticketMm === 58 || ticketMm === 80) {
                    setUltimoTicketMm(ticketMm);
                    actualizarTextoTicket();
                }
                $('#modalConfigAgentePago').modal('hide');
                verificarAgente();
            }).fail(function () {
                $('#msgConfigAgentePago')
                    .removeClass('d-none')
                    .text('No se pudo guardar la configuración de impresión.');
            });
        });

        $('#btnPostPagoPdf').on('click', function () {
            abrirNuevaVentana(state.pdfUrl);
            cerrarYRedirigir();
        });

        $('#btnPostPagoEmail').on('click', function () {
            abrirModalEmail();
        });

        $('#btnConfirmarEmailPago').on('click', function () {
            confirmarEmail();
        });

        $('#ppEmailDestino, #ppEmailAsunto, #ppEmailMensaje').on('input', function () {
            $('#ppEmailError').addClass('d-none').text('');
        });

        $('#modalEmailPago').on('shown.bs.modal', function () {
            $('#ppEmailDestino').trigger('focus');
        });

        $(document).on('keydown', function (e) {
            var $modal = $('#modalPostPago');
            if (!$modal.hasClass('show')) return;

            var $modalEmail = $('#modalEmailPago');
            if ($modalEmail.hasClass('show')) return;

            var tag = (e.target && e.target.tagName) ? e.target.tagName.toLowerCase() : '';
            if (tag === 'input' || tag === 'textarea') return;

            if (e.key === '1') { e.preventDefault(); $('#btnPostPagoNoImprimir').click(); return; }
            if (e.key === '2') { e.preventDefault(); $('#btnPostPagoImprimir').click(); return; }
            if (e.key === '3') { e.preventDefault(); $('#btnPostPagoPdf').click(); return; }
            if (e.key === '4') { e.preventDefault(); $('#btnPostPagoEmail').click(); return; }

            var ticketVisible = $('#bloqueTicketOpcionesPago').hasClass('show');
            if (!ticketVisible) return;

            if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight'].indexOf(e.key) >= 0) {
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
