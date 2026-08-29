(function () {
    var KEY_TICKET_MM = 'postexpendio_ticket_mm';

    var state = {
        idExpendio: 0,
        redirectUrl: '',
        imprimirUrl: '',
        imprimirPayloadUrl: '',
        pdfUrl: '',
        ticketMmActual: null,
        returnModalSelector: '',
        titulo: '',
        mensaje: '',
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

    function restaurarModalOrigenOContinuar() {
        if (state.returnModalSelector) {
            $(state.returnModalSelector).modal('show');
            return;
        }

        if (state.redirectUrl) {
            // Vuelve a la misma instancia de POS Expendio (mismo posInstanceId, ver
            // PuntosExpendioController.FinalizarPOS) -- no debe disparar la limpieza del
            // operador de produccion via pagehide (ver punto-expendio-pos.js / POS.cshtml).
            window.PosNavegandoInternoPOS = true;
            window.location.href = state.redirectUrl;
        }
    }

    function cerrarYContinuar() {
        $('#modalPostPuntoExpendio').one('hidden.bs.modal.postPuntoExpendioFlow', function () {
            restaurarModalOrigenOContinuar();
        });
        cerrarModal();
    }

    function cerrarLuegoDeImprimir(delayMs) {
        window.setTimeout(function () {
            cerrarYContinuar();
        }, delayMs || 1200);
    }

    function abrirNuevaVentana(url) {
        if (!url) return;
        window.open(url, '_blank', 'noopener');
    }

    function esEmailValido(email) {
        return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(email || '').trim());
    }

    function getEmailConfigUrl() {
        var cfg = window.puntoExpendioPosConfig || {};
        return cfg.urlEmailConfig || '';
    }

    function getEmailEnviarUrl() {
        var cfg = window.puntoExpendioPosConfig || {};
        return cfg.urlEmailEnviar || '';
    }

    function abrirModalEmail() {
        var configUrl = getEmailConfigUrl();
        if (!state.idExpendio || !configUrl) {
            Swal.fire({ icon: 'error', title: 'Email', text: 'No se pudo preparar el envío por email.' });
            return;
        }

        $('#peEmailError').addClass('d-none').text('');
        $('#peEmailDestino').val('');
        $('#peEmailAsunto').val('');
        $('#peEmailMensaje').val('');
        $('#btnConfirmarEmailPuntoExpendio').prop('disabled', true);

        $.getJSON(configUrl, { idExpendio: state.idExpendio })
            .done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'Email', text: (resp && resp.msg) || 'No se pudieron recuperar los datos del email.' });
                    return;
                }

                $('#peEmailDestino').val(resp.email || '');
                $('#peEmailAsunto').val(resp.asunto || '');
                $('#peEmailMensaje').val(resp.mensaje || '');
                $('#btnConfirmarEmailPuntoExpendio').prop('disabled', false);
                $('#modalEmailPuntoExpendio').modal('show');
            })
            .fail(function () {
                Swal.fire({ icon: 'error', title: 'Email', text: 'No se pudieron recuperar los datos del email.' });
            });
    }

    function confirmarEmail() {
        var sendUrl = getEmailEnviarUrl();
        var payload = {
            idExpendio: state.idExpendio,
            emailDestino: ($('#peEmailDestino').val() || '').trim(),
            asunto: ($('#peEmailAsunto').val() || '').trim(),
            mensaje: ($('#peEmailMensaje').val() || '').trim()
        };

        if (!payload.emailDestino) {
            $('#peEmailError').removeClass('d-none').text('Ingresá el email destino.');
            return;
        }

        if (!esEmailValido(payload.emailDestino)) {
            $('#peEmailError').removeClass('d-none').text('Ingresá un email válido.');
            return;
        }

        if (!payload.asunto) {
            $('#peEmailError').removeClass('d-none').text('Ingresá el asunto del email.');
            return;
        }

        $('#peEmailError').addClass('d-none').text('');
        $('#btnConfirmarEmailPuntoExpendio').prop('disabled', true);

        $.ajax({
            url: sendUrl,
            type: 'POST',
            dataType: 'json',
            data: payload
        }).done(function (resp) {
            if (!resp || !resp.ok) {
                $('#btnConfirmarEmailPuntoExpendio').prop('disabled', false);
                $('#peEmailError').removeClass('d-none').text((resp && resp.msg) || 'No se pudo enviar el email.');
                return;
            }

            $('#modalEmailPuntoExpendio').modal('hide');
            Swal.fire({ icon: 'success', title: 'Email', text: resp.msg || 'El comprobante se envió correctamente.' });
            cerrarYContinuar();
        }).fail(function (xhr) {
            var msg = (xhr.responseJSON && xhr.responseJSON.msg) || 'No se pudo enviar el email.';
            $('#btnConfirmarEmailPuntoExpendio').prop('disabled', false);
            $('#peEmailError').removeClass('d-none').text(msg);
        });
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
        $('#estadoAgentePuntoExpendio')
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
                var $cmb = $('#cmbImpresoraAgentePuntoExpendio');
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

                $('#cmbMmAgentePuntoExpendio').val((configData.ticketMm === 80 ? 80 : 58).toString());
                dfd.resolve();
            })
            .fail(function () {
                dfd.reject();
            });

        return dfd.promise();
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
        open: function (resp, options) {
            resp = resp || {};
            options = options || {};

            state.idExpendio = parseInt(resp.idExpendio, 10) || 0;
            state.redirectUrl = resp.redirectUrl || '';
            state.imprimirUrl = resp.imprimirUrl || '';
            state.imprimirPayloadUrl = resp.imprimirPayloadUrl || '';
            state.pdfUrl = resp.pdfUrl || '';
            state.ticketMmActual = null;
            state.returnModalSelector = options.returnModalSelector || '';
            state.titulo = options.titulo || 'Punto de expendio guardado';
            state.mensaje = options.mensaje || 'El expendio se guardó correctamente. ¿Qué desea hacer?';

            $('#lblTituloPostPuntoExpendio').text(state.titulo);
            $('#lblMensajePostPuntoExpendio').text(state.mensaje);

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
            refrescarEstadoAgente();
            verificarAgente();
            setTimeout(function () {
                $('#btnPostPuntoExpendioImprimir').trigger('focus');
            }, 50);
        });

        $('#btnPostPuntoExpendioNoImprimir').on('click', function () {
            cerrarYContinuar();
        });

        $('#btnPostPuntoExpendioImprimir').on('click', function () {
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

        $('#btnCambiarTicketPuntoExpendio').on('click', function () {
            abrirOpcionesTicket();
        });

        $(document).on('click', '.btnTicketPuntoExpendioOpt', function () {
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

        $('#btnConfigurarAgentePuntoExpendio').on('click', function () {
            $('#msgConfigAgentePuntoExpendio').addClass('d-none').text('');

            verificarAgente().done(function (available) {
                if (!available) {
                    $('#msgConfigAgentePuntoExpendio')
                        .removeClass('d-none')
                        .text('No se detectó el agente local. Descargalo e instalalo en esta terminal.');
                    $('#modalConfigAgentePuntoExpendio').modal('show');
                    return;
                }

                cargarConfiguracionAgente()
                    .done(function () {
                        $('#modalConfigAgentePuntoExpendio').modal('show');
                    })
                    .fail(function () {
                        $('#msgConfigAgentePuntoExpendio')
                            .removeClass('d-none')
                            .text('No se pudieron leer las impresoras instaladas.');
                        $('#modalConfigAgentePuntoExpendio').modal('show');
                    });
            });
        });

        $('#btnGuardarConfigAgentePuntoExpendio').on('click', function () {
            if (!window.CarniSysPrintAgent) return;

            var printerName = ($('#cmbImpresoraAgentePuntoExpendio').val() || '').toString();
            var ticketMm = parseInt($('#cmbMmAgentePuntoExpendio').val(), 10);
            window.CarniSysPrintAgent.saveConfig({
                printerName: printerName,
                ticketMm: ticketMm === 80 ? 80 : 58
            }).done(function () {
                if (ticketMm === 58 || ticketMm === 80) {
                    setUltimoTicketMm(ticketMm);
                    actualizarTextoTicket();
                }
                $('#modalConfigAgentePuntoExpendio').modal('hide');
                verificarAgente();
            }).fail(function () {
                $('#msgConfigAgentePuntoExpendio')
                    .removeClass('d-none')
                    .text('No se pudo guardar la configuración de impresión.');
            });
        });

        $('#btnPostPuntoExpendioPdf').on('click', function () {
            abrirNuevaVentana(state.pdfUrl);
            cerrarYContinuar();
        });

        $('#btnPostPuntoExpendioEmail').on('click', function () {
            abrirModalEmail();
        });

        $('#btnConfirmarEmailPuntoExpendio').on('click', function () {
            confirmarEmail();
        });

        $('#peEmailDestino, #peEmailAsunto, #peEmailMensaje').on('input', function () {
            $('#peEmailError').addClass('d-none').text('');
        });

        $('#modalEmailPuntoExpendio').on('shown.bs.modal', function () {
            $('#peEmailDestino').trigger('focus');
        });

        $(document).on('keydown.postPuntoExpendioModal', function (e) {
            var $modal = $('#modalPostPuntoExpendio');
            if (!$modal.hasClass('show')) return;

            // El modal de email (#modalEmailPuntoExpendio) se apila arriba de este sin
            // cerrarlo, asi que sin este guard escribir "1234" en destino/asunto/mensaje
            // dispara los atajos de abajo en vez de escribir el caracter.
            var tag = (e.target && e.target.tagName) ? e.target.tagName.toLowerCase() : '';
            if (tag === 'input' || tag === 'textarea') return;

            if (e.key === '1') { e.preventDefault(); $('#btnPostPuntoExpendioNoImprimir').click(); return; }
            if (e.key === '2') { e.preventDefault(); $('#btnPostPuntoExpendioImprimir').click(); return; }
            if (e.key === '3') { e.preventDefault(); $('#btnPostPuntoExpendioPdf').click(); return; }
            if (e.key === '4') { e.preventDefault(); $('#btnPostPuntoExpendioEmail').click(); return; }

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
                if (state.agentAvailable) {
                    imprimirConAgente(mm);
                } else {
                    imprimirTicket(mm);
                }
            }
        });
    });
})();
