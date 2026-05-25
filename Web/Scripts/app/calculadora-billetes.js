(function (window, $) {
    'use strict';

    if (!$ || window.CalculadoraBilletes) {
        return;
    }

    var KEY_TICKET_MM = 'calculadora_billetes_ticket_mm';
    var DENOMINACIONES = [20000, 10000, 2000, 1000, 500, 200, 100, 50, 20, 10];
    var agenteDisponible = false;
    var agenteVerificado = false;
    var agenteNombre = '';
    var permitiendoCerrar = false;
    var mostrandoPost = false;
    var opcionesActuales = {};

    function getConfig() {
        return window.CalculadoraBilletesConfig || {};
    }

    function formatMoney(value) {
        return '$ ' + Number(value || 0).toLocaleString('es-AR', {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function sanitizeIntegerInput($input) {
        var limpio = ($input.val() || '').replace(/\D/g, '');
        if (($input.val() || '') !== limpio) {
            $input.val(limpio);
        }
    }

    function sanitizeDecimalInput($input) {
        var value = ($input.val() || '').replace(/[^0-9,.\-]/g, '');
        value = value.replace(/-/g, '');

        var parts = value.split(/[,.]/);
        if (parts.length > 1) {
            value = parts[0] + ',' + parts.slice(1).join('');
        }

        if (($input.val() || '') !== value) {
            $input.val(value);
        }
    }

    function toInt(value) {
        var limpio = String(value || '').replace(/\D/g, '');
        return limpio ? parseInt(limpio, 10) : 0;
    }

    function toDecimal(value) {
        var normalizado = String(value || '')
            .replace(/[^0-9,.]/g, '')
            .replace(/\./g, '')
            .replace(',', '.');
        var numero = parseFloat(normalizado);
        return isNaN(numero) || numero < 0 ? 0 : numero;
    }

    function showMessage(icon, title, text) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({
                icon: icon,
                title: title,
                text: text
            });
            return;
        }

        window.alert((title ? title + '\n' : '') + (text || ''));
    }

    function getUltimoTicketMm() {
        var value = parseInt(window.localStorage.getItem(KEY_TICKET_MM), 10);
        return value === 58 || value === 80 ? value : null;
    }

    function setUltimoTicketMm(mm) {
        window.localStorage.setItem(KEY_TICKET_MM, String(mm));
    }

    function actualizarTextoTicket() {
        var mm = getUltimoTicketMm();
        $('#lblCbTicketAccion').text(mm ? ('Ticket (' + mm + ' mm)') : 'Ticket');
    }

    function marcarTicketSeleccionado(mm) {
        $('.btnCbTicketOpt').removeClass('active btn-primary').addClass('btn-outline-primary');
        $('.btnCbTicketOpt[data-mm="' + mm + '"]').addClass('active btn-primary').removeClass('btn-outline-primary');
        $('#modalPostCalculadoraBilletes').data('ticket-mm-actual', mm);
    }

    function getTicketSeleccionActual() {
        var mm = $('#modalPostCalculadoraBilletes').data('ticket-mm-actual');
        return mm === 58 || mm === 80 ? mm : null;
    }

    function abrirOpcionesTicket(preseleccionarMm) {
        $('#bloqueCbTicketOpciones').collapse('show');
        marcarTicketSeleccionado(preseleccionarMm || getUltimoTicketMm() || 58);
    }

    function refrescarResumenPantalla(total, monedas) {
        $('#lblCalculadoraBilletesTotal').text(formatMoney(total));
        $('#lblCalculadoraBilletesMonedasSubtotal').text(formatMoney(monedas));
    }

    function recalcular() {
        var total = 0;

        $('.js-calculadora-billetes-cantidad').each(function () {
            var $input = $(this);
            var denominacion = parseInt($input.data('denominacion'), 10) || 0;
            var cantidad = toInt($input.val());
            var subtotal = cantidad * denominacion;
            total += subtotal;

            $('.js-calculadora-billetes-subtotal[data-denominacion="' + denominacion + '"]').text(formatMoney(subtotal));
        });

        var monedas = toDecimal($('#txtCalculadoraBilletesMonedas').val());
        total += monedas;
        refrescarResumenPantalla(total, monedas);
    }

    function focusInputCodigo() {
        var input = document.getElementById('inputCodigo');
        if (!input) return;
        input.focus();
        try { input.select(); } catch (err) { }
    }

    function focusPrimerCampo() {
        var input = $('.js-calculadora-billetes-cantidad[data-denominacion="20000"]').get(0);
        if (!input) return;
        input.focus();
        try { input.select(); } catch (err) { }
    }

    function focusNextByOrder(order) {
        var $next = $('[data-order="' + (order + 1) + '"]');
        if ($next.length) {
            $next.focus();
            try { $next.select(); } catch (err) { }
            return;
        }

        $('#btnAceptarCalculadoraBilletes').trigger('focus');
    }

    function resetModal() {
        $('.js-calculadora-billetes-cantidad').val('');
        $('#txtCalculadoraBilletesMonedas').val('');
        $('.js-calculadora-billetes-subtotal').text(formatMoney(0));
        refrescarResumenPantalla(0, 0);
        permitiendoCerrar = false;
        mostrandoPost = false;
        opcionesActuales = {};
        $('#calculadoraBilletesTitulo').text('Calculadora Billetes');
        $('#cbPostSubTitle').hide().text('');
        $('#bloqueCbTicketOpciones').collapse('hide');
    }

    function buildDetalleTexto(denominaciones, monedas) {
        var partes = (denominaciones || []).map(function (item) {
            return (item.cantidad || 0) + ' x ' + Number(item.denominacion || 0).toLocaleString('es-AR');
        });

        if ((monedas || 0) > 0) {
            partes.push('Monedas ' + Number(monedas).toLocaleString('es-AR', {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            }));
        }

        return partes.join(' + ');
    }

    function collectDataFromInputs() {
        var denominaciones = [];
        var total = 0;

        DENOMINACIONES.forEach(function (denominacion) {
            var cantidad = toInt($('.js-calculadora-billetes-cantidad[data-denominacion="' + denominacion + '"]').val());
            total += denominacion * cantidad;
            denominaciones.push({
                denominacion: denominacion,
                cantidad: cantidad
            });
        });

        var monedas = toDecimal($('#txtCalculadoraBilletesMonedas').val());
        total += monedas;

        return {
            titulo: opcionesActuales.titulo || 'Detalle de billetes',
            total: total,
            monedas: monedas,
            denominaciones: denominaciones,
            detalleTexto: buildDetalleTexto(denominaciones, monedas),
            whatsapp: opcionesActuales.whatsapp || ''
        };
    }

    function getPrintData() {
        if (opcionesActuales && opcionesActuales.usarDetalleExterno) {
            var denominaciones = opcionesActuales.denominaciones || [];
            var monedas = Number(opcionesActuales.monedas || 0);
            return {
                titulo: opcionesActuales.titulo || 'Detalle de billetes',
                total: Number(opcionesActuales.total || 0),
                monedas: monedas,
                denominaciones: denominaciones,
                detalleTexto: opcionesActuales.detalleTexto || buildDetalleTexto(denominaciones, monedas),
                whatsapp: opcionesActuales.whatsapp || ''
            };
        }

        return collectDataFromInputs();
    }

    function buildTicketPayload(data, mm, lineas) {
        return {
            printerName: null,
            ticketMm: mm === 58 ? 58 : 80,
            ticketLines: lineas || [],
            cortarPapel: true
        };
    }

    function buildBrowserTicketHtml(lineas) {
        var htmlLineas = (lineas || []).map(function (linea) {
            return '<div>' + $('<div/>').text(linea == null ? '' : String(linea)).html() + '</div>';
        }).join('');

        return '<!DOCTYPE html><html><head><meta charset="utf-8" />'
            + '<title>Ticket</title>'
            + '<style>body{font-family:Consolas,monospace;padding:12px;font-size:14px;}'
            + '.ticket-total{font-weight:700;}div{white-space:pre-wrap;}</style>'
            + '</head><body>' + htmlLineas + '</body></html>';
    }

    function imprimirEnNavegador(lineas) {
        var popup = window.open('', '_blank', 'width=420,height=700');
        if (!popup) {
            showMessage('warning', 'Impresion', 'El navegador bloqueo la ventana de impresion.');
            return $.Deferred().reject().promise();
        }

        popup.document.open();
        popup.document.write(buildBrowserTicketHtml(lineas));
        popup.document.close();
        popup.focus();
        popup.print();
        return $.Deferred().resolve().promise();
    }

    function actualizarEstadoAgente(texto, disponible) {
        $('#estadoAgenteCalculadoraBilletes')
            .text(texto)
            .toggleClass('text-success', !!disponible)
            .toggleClass('text-muted', !disponible);
    }

    function refrescarEstadoAgente() {
        if (!agenteVerificado) {
            actualizarEstadoAgente('Impresion local: verificando agente...', false);
            return;
        }

        if (agenteDisponible) {
            var texto = 'Impresion local activa';
            if (agenteNombre) texto += ': ' + agenteNombre;
            actualizarEstadoAgente(texto + '.', true);
            return;
        }

        actualizarEstadoAgente('Impresion local: usando navegador.', false);
    }

    function verificarAgente() {
        var dfd = $.Deferred();

        if (!window.CarniSysPrintAgent) {
            agenteVerificado = true;
            agenteDisponible = false;
            agenteNombre = '';
            refrescarEstadoAgente();
            return dfd.resolve(false).promise();
        }

        window.CarniSysPrintAgent.health()
            .done(function (resp) {
                agenteVerificado = true;
                agenteDisponible = !!(resp && resp.ok);
                agenteNombre = resp && resp.printerName ? resp.printerName : '';
                refrescarEstadoAgente();
                dfd.resolve(agenteDisponible);
            })
            .fail(function () {
                agenteVerificado = true;
                agenteDisponible = false;
                agenteNombre = '';
                refrescarEstadoAgente();
                dfd.resolve(false);
            });

        return dfd.promise();
    }

    function abrirModalConfigAgente() {
        if (!window.CarniSysPrintAgent) {
            showMessage('info', 'Impresora', 'Instala el agente local para configurar la impresora.');
            return;
        }

        $('#msgConfigAgenteCalculadoraBilletes').addClass('d-none').text('');
        $('#cmbImpresoraAgenteCalculadoraBilletes').empty();

        $.when(
            window.CarniSysPrintAgent.getPrinters(),
            window.CarniSysPrintAgent.getConfig()
        ).done(function (printersResp, configResp) {
            var printersData = printersResp && printersResp[0] ? printersResp[0] : printersResp;
            var configData = configResp && configResp[0] ? configResp[0] : configResp;
            var printers = printersData && printersData.printers ? printersData.printers : [];

            if (!printers.length) {
                $('#msgConfigAgenteCalculadoraBilletes').removeClass('d-none').text('No se encontraron impresoras disponibles.');
            }

            printers.forEach(function (printer) {
                $('#cmbImpresoraAgenteCalculadoraBilletes').append(
                    $('<option/>').val(printer).text(printer)
                );
            });

            if (configData && configData.printerName) {
                $('#cmbImpresoraAgenteCalculadoraBilletes').val(configData.printerName);
            }

            $('#cmbMmAgenteCalculadoraBilletes').val(
                configData && (configData.ticketMm === 58 || configData.ticketMm === 80)
                    ? String(configData.ticketMm)
                    : String(getUltimoTicketMm() || 58)
            );

            $('#modalConfigAgenteCalculadoraBilletes').modal('show');
        }).fail(function () {
            $('#msgConfigAgenteCalculadoraBilletes').removeClass('d-none').text('No se pudo consultar el agente de impresion.');
            $('#modalConfigAgenteCalculadoraBilletes').modal('show');
        });
    }

    function guardarConfigAgente() {
        if (!window.CarniSysPrintAgent) {
            return;
        }

        var config = {
            printerName: $('#cmbImpresoraAgenteCalculadoraBilletes').val() || null,
            ticketMm: parseInt($('#cmbMmAgenteCalculadoraBilletes').val(), 10) === 58 ? 58 : 80
        };

        window.CarniSysPrintAgent.saveConfig(config)
            .done(function () {
                setUltimoTicketMm(config.ticketMm);
                actualizarTextoTicket();
                $('#modalConfigAgenteCalculadoraBilletes').modal('hide');
                verificarAgente();
            })
            .fail(function () {
                $('#msgConfigAgenteCalculadoraBilletes').removeClass('d-none').text('No se pudo guardar la configuracion de impresion.');
            });
    }

    function cerrarPostYCalculadora() {
        $('#modalPostCalculadoraBilletes').data('permitir-cierre', true).modal('hide');
        permitiendoCerrar = true;
        $('#modalCalculadoraBilletes').modal('hide');
    }

    function abrirPostModal() {
        mostrandoPost = true;
        actualizarTextoTicket();
        refrescarEstadoAgente();
        $('#cbPostSubTitle').text($('#calculadoraBilletesTitulo').text()).show();
        $('#modalPostCalculadoraBilletes').data('permitir-cierre', false).modal('show');

        setTimeout(function () {
            $('#btnCbNoImprimir').trigger('focus');
        }, 150);
    }

    function imprimirTicket(mm) {
        var config = getConfig();
        var data = getPrintData();
        data.ticketMm = mm;

        return $.ajax({
            url: config.payloadUrl,
            method: 'POST',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            timeout: 12000
        }).then(function (resp) {
            if (!resp || !resp.ok) {
                return $.Deferred().reject(resp && resp.mensaje ? resp.mensaje : 'No se pudo preparar la impresion.').promise();
            }

            var ticketMm = resp.ticketMm === 58 ? 58 : 80;
            setUltimoTicketMm(ticketMm);
            actualizarTextoTicket();

            if (window.CarniSysPrintAgent && agenteDisponible) {
                return window.CarniSysPrintAgent.printExpendio(buildTicketPayload(data, ticketMm, resp.ticketLines))
                    .then(function () {
                        return true;
                    }, function () {
                        return imprimirEnNavegador(resp.ticketLines);
                    });
            }

            return imprimirEnNavegador(resp.ticketLines);
        });
    }

    function descargarPdf() {
        var config = getConfig();
        var data = getPrintData();

        return $.ajax({
            url: config.pdfUrl,
            method: 'POST',
            data: JSON.stringify(data),
            dataType: 'json',
            contentType: 'application/json; charset=utf-8',
            timeout: 12000
        }).done(function (resp) {
            if (!resp || !resp.ok || !resp.base64) {
                showMessage('error', 'PDF', resp && resp.mensaje ? resp.mensaje : 'No se pudo generar el PDF.');
                return;
            }

            var byteChars = window.atob(resp.base64);
            var byteNumbers = new Array(byteChars.length);
            for (var i = 0; i < byteChars.length; i++) {
                byteNumbers[i] = byteChars.charCodeAt(i);
            }

            var blob = new Blob([new Uint8Array(byteNumbers)], { type: 'application/pdf' });
            var url = window.URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = url;
            a.download = resp.fileName || 'DetalleBilletes.pdf';
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            window.URL.revokeObjectURL(url);
            cerrarPostYCalculadora();
        }).fail(function () {
            showMessage('error', 'PDF', 'No se pudo generar el PDF.');
        });
    }

    function abrirWhatsapp() {
        var data = getPrintData();
        var numero = String(data.whatsapp || '').replace(/\D/g, '');
        if (!numero) {
            showMessage('warning', 'WhatsApp', 'No hay un numero valido cargado.');
            return;
        }

        var texto = (data.titulo || 'Detalle de billetes')
            + '\nTotal: ' + formatMoney(data.total)
            + '\nDetalles: ' + (data.detalleTexto || '');
        window.open('https://wa.me/' + numero + '?text=' + encodeURIComponent(texto), '_blank');
    }

    function bindEvents() {
        $(document).on('input', '.js-calculadora-billetes-cantidad', function () {
            sanitizeIntegerInput($(this));
            recalcular();
        });

        $(document).on('input', '#txtCalculadoraBilletesMonedas', function () {
            sanitizeDecimalInput($(this));
            recalcular();
        });

        $(document).on('keydown', '.js-calculadora-billetes-cantidad, #txtCalculadoraBilletesMonedas', function (e) {
            if (e.key === 'Enter') {
                e.preventDefault();
                focusNextByOrder(parseInt($(this).data('order'), 10) || 0);
            }
        });

        $(document).on('keydown', '#modalPostCalculadoraBilletes', function (e) {
            if (e.key === '1') {
                e.preventDefault();
                $('#btnCbNoImprimir').trigger('click');
                return;
            }

            if (e.key === '2') {
                e.preventDefault();
                $('#btnCbTicket').trigger('click');
                return;
            }

            if (e.key === '3') {
                e.preventDefault();
                $('#btnCbPdf').trigger('click');
                return;
            }

            if (e.key === '4') {
                e.preventDefault();
                $('#btnCbWhatsapp').trigger('click');
                return;
            }

            if (e.key === 'Enter') {
                e.preventDefault();
                $('#btnCbNoImprimir').trigger('click');
            }
        });

        $(document).on('click', '#btnAceptarCalculadoraBilletes', function () {
            abrirPostModal();
        });

        $(document).on('click', '#btnCancelarCalculadoraBilletes, #btnCerrarCalculadoraBilletes', function () {
            abrirPostModal();
        });

        $(document).on('click', '#btnCbNoImprimir', function () {
            cerrarPostYCalculadora();
        });

        $(document).on('click', '#btnCbCambiarTicket', function () {
            abrirOpcionesTicket();
        });

        $(document).on('click', '.btnCbTicketOpt', function () {
            var mm = parseInt($(this).data('mm'), 10) === 58 ? 58 : 80;
            marcarTicketSeleccionado(mm);
            setUltimoTicketMm(mm);
            actualizarTextoTicket();
            imprimirTicket(mm)
                .done(function () {
                    cerrarPostYCalculadora();
                })
                .fail(function (mensaje) {
                    showMessage('error', 'Impresion', mensaje || 'No se pudo imprimir el ticket.');
                });
        });

        $(document).on('click', '#btnCbTicket', function () {
            var mm = getTicketSeleccionActual() || getUltimoTicketMm() || 58;
            if (!getTicketSeleccionActual()) {
                marcarTicketSeleccionado(mm);
            }

            imprimirTicket(mm)
                .done(function () {
                    cerrarPostYCalculadora();
                })
                .fail(function (mensaje) {
                    showMessage('error', 'Impresion', mensaje || 'No se pudo imprimir el ticket.');
                });
        });

        $(document).on('click', '#btnCbPdf', function () {
            descargarPdf();
        });

        $(document).on('click', '#btnCbWhatsapp', function () {
            abrirWhatsapp();
        });

        $(document).on('click', '#btnConfigurarAgenteCalculadoraBilletes', function () {
            abrirModalConfigAgente();
        });

        $(document).on('click', '#btnGuardarConfigAgenteCalculadoraBilletes', function () {
            guardarConfigAgente();
        });

        $('#modalCalculadoraBilletes').on('show.bs.modal', function () {
            permitiendoCerrar = false;
            mostrandoPost = false;
            $('#calculadoraBilletesTitulo').text(opcionesActuales.tituloPantalla || 'Calculadora Billetes');
            verificarAgente();
        });

        $('#modalCalculadoraBilletes').on('shown.bs.modal', function () {
            if (opcionesActuales.usarDetalleExterno) {
                refrescarResumenPantalla(Number(opcionesActuales.total || 0), Number(opcionesActuales.monedas || 0));
                $('#btnAceptarCalculadoraBilletes').trigger('focus');
                return;
            }

            focusPrimerCampo();
        });

        $('#modalCalculadoraBilletes').on('hide.bs.modal', function (e) {
            if (permitiendoCerrar) {
                return;
            }

            if (!mostrandoPost) {
                e.preventDefault();
                abrirPostModal();
            }
        });

        $('#modalCalculadoraBilletes').on('hidden.bs.modal', function () {
            resetModal();
            focusInputCodigo();
        });

        $('#modalPostCalculadoraBilletes').on('hide.bs.modal', function (e) {
            if (!$(this).data('permitir-cierre')) {
                e.preventDefault();
            }
        });
    }

    window.CalculadoraBilletes = {
        open: function (options) {
            opcionesActuales = $.extend({}, options || {});
            resetModal();

            if (opcionesActuales.usarDetalleExterno) {
                $('#calculadoraBilletesTitulo').text(opcionesActuales.tituloPantalla || 'Calculadora Billetes');
            }

            $('#modalCalculadoraBilletes').modal({
                backdrop: 'static',
                keyboard: false
            });
        }
    };

    $(function () {
        bindEvents();
        actualizarTextoTicket();
        refrescarEstadoAgente();
    });
})(window, window.jQuery);
