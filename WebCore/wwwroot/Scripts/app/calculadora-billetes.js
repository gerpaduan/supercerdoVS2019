(function (window, $) {
    'use strict';

    if (!$ || window.CalculadoraBilletes) {
        return;
    }

    var KEY_TICKET_MM = 'calculadora_billetes_ticket_mm';
    var DENOMINACIONES = [20000, 10000, 2000, 1000, 500, 200, 100, 50, 20, 10];
    // Delimitadores del bloque que se agrega al texto (Observaciones/Detalle): sirven para reemplazar el bloque
    // si se vuelve a usar la calculadora. El regex tambien reconoce los delimitadores viejos
    // ([INICIO_DETALLE_BILLETES]) para no duplicar el bloque en registros ya guardados con ese formato.
    var DETALLE_START = '--- Conteo de efectivo ---';
    var DETALLE_END = '--- Fin del conteo ---';
    var DETALLE_REGEX = /\[INICIO_DETALLE_BILLETES\][\s\S]*?\[FIN_DETALLE_BILLETES\]|--- Conteo de efectivo ---[\s\S]*?--- Fin del conteo ---/g;
    var agenteDisponible = false;
    var agenteVerificado = false;
    var agenteNombre = '';
    var permitiendoCerrar = false;
    var mostrandoPost = false;
    var opcionesActuales = {};
    var resultadoAplicado = false;

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
        resultadoAplicado = false;
        $('#calculadoraBilletesTitulo').text('Calculadora Billetes');
        $('#cbPostSubTitle').hide().text('');
        $('#bloqueCbTicketOpciones').collapse('hide');
    }

    function resolveFieldTarget(target) {
        if (!target) {
            return $();
        }

        if (target.jquery) {
            return target.first();
        }

        if (typeof target === 'string') {
            var selectors = target.split(',');
            for (var i = 0; i < selectors.length; i++) {
                var selector = $.trim(selectors[i]);
                if (!selector) continue;

                var $field = $(selector).first();
                if ($field.length) {
                    return $field;
                }
            }

            return $();
        }

        return $(target).first();
    }

    function normalizeEventList(events, defaults) {
        if ($.isArray(events) && events.length) {
            return events;
        }

        if (typeof events === 'string' && $.trim(events)) {
            return events.split(/[,\s]+/).filter(function (item) { return !!item; });
        }

        return defaults || [];
    }

    function setFieldValue($field, value, events) {
        if (!$field || !$field.length) return;

        $field.val(value);
        normalizeEventList(events, ['input', 'change', 'keyup']).forEach(function (eventName) {
            $field.trigger(eventName);
        });
    }

    function focusField(target) {
        var $field = resolveFieldTarget(target);
        if (!$field.length) return;

        window.setTimeout(function () {
            $field.trigger('focus');
            if ($field.is('input[type="text"], input:not([type]), textarea')) {
                $field.trigger('select');
            }
        }, 60);
    }

    function buildDetalleTexto(denominaciones, monedas) {
        // Solo las denominaciones con cantidad (mismo criterio que buildDetalleLineas).
        var partes = (denominaciones || []).filter(function (item) {
            return Number(item && item.cantidad ? item.cantidad : 0) > 0;
        }).map(function (item) {
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

    function buildDetalleLineas(denominaciones, monedas, total) {
        var lineas = [];

        (denominaciones || []).forEach(function (item) {
            var cantidad = Number(item && item.cantidad ? item.cantidad : 0);
            var denominacion = Number(item && item.denominacion ? item.denominacion : 0);
            // Solo las denominaciones con cantidad: las de cero no aportan y gastan el limite del campo.
            if (cantidad <= 0) return;

            lineas.push(
                cantidad + ' x $' + denominacion.toLocaleString('es-AR')
                + ' = ' + formatMoney(cantidad * denominacion)
            );
        });

        if ((monedas || 0) > 0) {
            lineas.push('Monedas: ' + formatMoney(monedas));
        }

        lineas.push('TOTAL EFECTIVO: ' + formatMoney(total));
        return lineas;
    }

    function buildResultado(data) {
        var denominaciones = data && data.denominaciones ? data.denominaciones : [];
        var monedas = Number(data && data.monedas ? data.monedas : 0);
        var total = Number(data && data.total ? data.total : 0);
        var detalleLineas = buildDetalleLineas(denominaciones, monedas, total);

        return $.extend({}, data || {}, {
            detalleLineas: detalleLineas,
            detalleMultilinea: detalleLineas.join('\n'),
            detalle: detalleLineas.join('\n')
        });
    }

    function limpiarDetalleExistente(texto) {
        return String(texto || '')
            .replace(DETALLE_REGEX, '')
            .replace(/\n{3,}/g, '\n\n')
            .trim();
    }

    function mergeDetalle(textoActual, resultado) {
        var base = limpiarDetalleExistente(textoActual);
        var bloque = window.CalculadoraBilletes.buildDetalleBlock(resultado);
        return base ? (base + '\n\n' + bloque) : bloque;
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

    function devolverResultado() {
        if (resultadoAplicado) {
            return null;
        }

        var resultado = buildResultado(getPrintData());
        var selectorInputTotal = opcionesActuales.selectorInputTotal || opcionesActuales.targetTotalSelector;
        var selectorInputDetalle = opcionesActuales.selectorInputDetalle || opcionesActuales.targetDetailSelector;
        var selectorFocus = opcionesActuales.selectorFocus || opcionesActuales.targetFocusSelector || selectorInputTotal;
        var callback = opcionesActuales.callbackOnAceptar || opcionesActuales.onAccept;

        if (selectorInputTotal) {
            setFieldValue(
                resolveFieldTarget(selectorInputTotal),
                window.CalculadoraBilletes.formatInputValue(resultado.total),
                opcionesActuales.eventosInputTotal
            );
        }

        if (selectorInputDetalle) {
            var $detalle = resolveFieldTarget(selectorInputDetalle);
            if ($detalle.length) {
                setFieldValue(
                    $detalle,
                    opcionesActuales.reemplazarDetalle
                        ? window.CalculadoraBilletes.buildDetalleBlock(resultado)
                        : mergeDetalle($detalle.val(), resultado),
                    opcionesActuales.eventosInputDetalle || ['input', 'change']
                );
            }
        }

        if (typeof callback === 'function') {
            try {
                callback(resultado);
            } catch (err) { }
        }

        if (selectorFocus) {
            focusField(selectorFocus);
        }

        resultadoAplicado = true;
        return resultado;
    }

    function aplicarCapturaYCerrar() {
        devolverResultado();
        permitiendoCerrar = true;
        $('#modalCalculadoraBilletes').modal('hide');
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
        // Las lineas pueden traer comandos ESC/POS de formato (EscPosFormato.cs: ESC E n, ESC M n,
        // GS ! n) pensados para el agente; el navegador no los interpreta y mostraria la letra suelta.
        lineas = (lineas || []).map(function (linea) {
            return String(linea).replace(/\x1B[EM][\x00\x01]|\x1D![\x00-\xFF]/g, '');
        });

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

    // Si no se cargo ningun billete/moneda (total 0), no tiene sentido
    // ofrecer imprimir/enviar un comprobante vacio.
    function hayMontoParaImprimir() {
        var data = getPrintData();
        return Number(data && data.total) > 0;
    }

    function cerrarCalculadoraSinPost() {
        permitiendoCerrar = true;
        $('#modalCalculadoraBilletes').modal('hide');
    }

    function cerrarPostYCalculadora() {
        devolverResultado();
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

    // ===== Apilado sobre otros modales + atajos de teclado (2026-09-26, ver docs/DECISIONS.md) =====
    var MODALES_PROPIOS = '#modalCalculadoraBilletes, #modalPostCalculadoraBilletes, #modalConfigAgenteCalculadoraBilletes';

    // La calculadora se abre tambien desde adentro de otros modales (ej. Pago en el POS): sin subir el z-index
    // quedaba DETRAS. Se pone 10 por encima del modal abierto con mayor z-index (no una cuenta fija: el z-index
    // base de Bootstrap ya es 1050 y otros modales del POS ya vienen elevados a mano).
    function elevarSobreModalesAbiertos(modalEl) {
        modalEl.style.removeProperty('z-index');

        var abiertos = document.querySelectorAll('.modal.show');
        if (abiertos.length < 1) return;

        var maxZ = 0;
        Array.prototype.forEach.call(abiertos, function (el) {
            maxZ = Math.max(maxZ, parseInt(window.getComputedStyle(el).zIndex, 10) || 0);
        });

        var zIndex = maxZ + 10;
        modalEl.style.setProperty('z-index', zIndex, 'important');

        $(modalEl).one('shown.bs.modal.cbZ', function () {
            var $backdrop = $('.modal-backdrop').not('.modal-stack').last();
            if ($backdrop.length) {
                $backdrop[0].style.setProperty('z-index', zIndex - 1, 'important');
                $backdrop.addClass('modal-stack');
            }
        });
    }

    // Bootstrap 5 deja un FocusTrap activo por modal: el del modal de abajo le robaria el foco al de arriba.
    // Se deja activo solo el del modal con mayor z-index.
    function reajustarFocusTraps() {
        if (!window.bootstrap || !window.bootstrap.Modal) return;

        var modales = Array.prototype.slice.call(document.querySelectorAll('.modal.show'));
        var maxZ = -1;
        var superior = null;
        modales.forEach(function (el) {
            var z = parseInt(window.getComputedStyle(el).zIndex, 10) || 0;
            if (z >= maxZ) { maxZ = z; superior = el; }
        });

        modales.forEach(function (el) {
            var inst = window.bootstrap.Modal.getInstance(el);
            var trap = inst && inst._focustrap;
            if (!trap) return;
            if (el === superior && typeof trap.activate === 'function') trap.activate();
            else if (el !== superior && typeof trap.deactivate === 'function') trap.deactivate();
        });
    }

    function calculadoraAbierta() {
        return $('#modalCalculadoraBilletes').hasClass('show');
    }

    function hayCampoEscrito() {
        if (opcionesActuales && opcionesActuales.usarDetalleExterno) return false;

        var escrito = false;
        $('.js-calculadora-billetes-cantidad, #txtCalculadoraBilletesMonedas').each(function () {
            if (($(this).val() || '').toString().trim() !== '') escrito = true;
        });
        return escrito;
    }

    // Esc / Alt+C: sin nada escrito cierra directo; con datos pregunta (Enter = cerrar igual, Esc = seguir).
    function intentarCerrarCalculadora() {
        if (!hayCampoEscrito()) {
            cerrarCalculadoraSinPost();
            return;
        }

        if (!window.Swal || typeof window.Swal.fire !== 'function') {
            if (window.confirm('Hay importes cargados. ¿Cerrar la calculadora igual?')) cerrarCalculadoraSinPost();
            return;
        }

        // Campo de la calculadora con el foco al abrir el aviso: es adonde se vuelve si se elige "seguir".
        var activo = document.activeElement;
        var campoConFoco = activo && $(activo).closest('#modalCalculadoraBilletes').length && $(activo).is('input, textarea')
            ? activo
            : null;
        var cerrando = false;

        // El foco SIEMPRE vuelve a la calculadora, nunca al modal de atras (Pago, etc.).
        function devolverFocoALaCalculadora() {
            if (cerrando) return;
            reajustarFocusTraps();

            if (campoConFoco && document.body.contains(campoConFoco)) {
                campoConFoco.focus();
                try { campoConFoco.select(); } catch (err) { }
            } else {
                focusPrimerCampo();
            }
        }

        window.Swal.fire({
            icon: 'warning',
            title: '¿Cerrar la calculadora?',
            text: 'Hay importes cargados que se van a perder.',
            showCancelButton: true,
            confirmButtonText: 'Cerrar igual (Enter)',
            cancelButtonText: 'Seguir en la calculadora (Esc)',
            confirmButtonColor: '#d33',
            reverseButtons: true,
            // El parche de swal-single-confirm.js reactiva los focus-trap de TODOS los modales al cerrar, y el
            // trap del modal de atras (Bootstrap 5, autofocus) se lleva el foco. Se desactiva de nuevo en
            // willClose (justo despues del parche) y se devuelve el foco al campo; didClose repite por si el
            // foco se movio durante la animacion de cierre.
            willClose: devolverFocoALaCalculadora,
            didClose: devolverFocoALaCalculadora
        }).then(function (r) {
            if (r.isConfirmed) {
                cerrando = true;
                cerrarCalculadoraSinPost();
            }
        });
    }

    var ATAJOS_MODAL_POST = {
        '1': '#btnCbNoImprimir',
        '2': '#btnCbTicket',
        '3': '#btnCbPdf',
        '4': '#btnCbWhatsapp',
        'enter': '#btnCbNoImprimir'
    };

    function swalVisible() {
        return !!(window.Swal && typeof window.Swal.isVisible === 'function' && window.Swal.isVisible());
    }

    // Alt+B: abre la calculadora desde la pantalla actual. Prioridad: boton de calculadora visible del modal de
    // arriba (o de la pagina); si no hay ninguno y el POS no tiene modales abiertos, el mismo F3 del POS.
    function abrirCalculadoraPorAtajo() {
        var $tope = $('.modal.show:visible').last();
        var $ambito = $tope.length ? $tope : $(document);
        var $boton = $ambito.find('.js-calculadora-billetes-launch:visible:not(:disabled)').first();

        if ($boton.length) {
            $boton.trigger('click');
            return true;
        }

        if (!$tope.length && window.posHotkeysHooks && typeof window.posHotkeysHooks.F3 === 'function') {
            window.posHotkeysHooks.F3();
            return true;
        }

        return false;
    }

    function onKeydownAtajos(e) {
        if (e.ctrlKey || e.metaKey || e.repeat || swalVisible()) return;
        var key = String(e.key || '').toLowerCase();

        // Atajos del modal "que deseas hacer" (1 No imprimir, 2 Ticket, 3 PDF, 4 WhatsApp, Enter = No imprimir).
        // Van en captura de window (antes que los atajos del POS/pantalla de origen, que se comian los digitos
        // y no llegaban al modal). Con el modal de configurar impresora arriba no se interceptan.
        if ($('#modalPostCalculadoraBilletes').hasClass('show')
            && !$('#modalConfigAgenteCalculadoraBilletes').hasClass('show')
            && !e.altKey && !e.shiftKey) {
            var botonAtajo = ATAJOS_MODAL_POST[key];
            if (botonAtajo) {
                e.preventDefault();
                e.stopPropagation();
                e.stopImmediatePropagation();
                $(botonAtajo).trigger('click');
            }
            return;
        }

        if (calculadoraAbierta()) {
            if (mostrandoPost || $('#modalPostCalculadoraBilletes').hasClass('show')
                || $('#modalConfigAgenteCalculadoraBilletes').hasClass('show')) return;

            var esEscape = key === 'escape' && !e.altKey && !e.shiftKey;
            var esAltC = key === 'c' && e.altKey && !e.shiftKey;
            var esAltEnter = key === 'enter' && e.altKey && !e.shiftKey;
            if (!esEscape && !esAltC && !esAltEnter) return;

            e.preventDefault();
            e.stopPropagation();
            e.stopImmediatePropagation();

            if (esAltEnter) $('#btnAceptarCalculadoraBilletes').trigger('click');
            else intentarCerrarCalculadora();
            return;
        }

        if (key === 'b' && e.altKey && !e.shiftKey) {
            if (abrirCalculadoraPorAtajo()) {
                e.preventDefault();
                e.stopPropagation();
                e.stopImmediatePropagation();
            }
        }
    }

    function bindEvents() {
        // En window (captura) y no en document: corre ANTES que los atajos de la pantalla que abrio la calculadora
        // (Pago, POS, Egresos), que escuchan en document/captura y se creen "el modal de arriba".
        window.addEventListener('keydown', onKeydownAtajos, true);

        $(MODALES_PROPIOS).on('show.bs.modal', function () {
            elevarSobreModalesAbiertos(this);
        }).on('shown.bs.modal hidden.bs.modal', function () {
            reajustarFocusTraps();
        });

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

        $(document).on('click', '#btnAceptarCalculadoraBilletes', function () {
            devolverResultado();

            if (!hayMontoParaImprimir()) {
                cerrarCalculadoraSinPost();
                return;
            }

            abrirPostModal();
        });

        $(document).on('click', '#btnCancelarCalculadoraBilletes, #btnCerrarCalculadoraBilletes', function () {
            if (!hayMontoParaImprimir()) {
                cerrarCalculadoraSinPost();
                return;
            }

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
            // resetModal() vacia opcionesActuales: tiene que ir ANTES de asignarlas. Al reves (como estaba) se
            // perdian selectorInputTotal/selectorInputDetalle/callbackOnAceptar y Aceptar no cargaba el importe
            // en la pantalla que llamo a la calculadora.
            resetModal();
            opcionesActuales = $.extend({}, options || {});

            if (opcionesActuales.usarDetalleExterno) {
                $('#calculadoraBilletesTitulo').text(opcionesActuales.tituloPantalla || 'Calculadora Billetes');
            }

            // Bootstrap 5: .modal(objetoOpciones) solo crea/configura la instancia (getOrCreateInstance),
            // a diferencia de BS4 NO la muestra sola -- hace falta el .modal('show') explicito de abajo.
            // Bug real: sin esta segunda linea el modal nunca aparecia (ni error en consola), afectaba
            // tanto F3 en el POS como "Calcular efectivo" en Cajas/_AddOrEditEgresoCaja.cshtml.
            $('#modalCalculadoraBilletes').modal({
                backdrop: 'static',
                keyboard: false
            });
            $('#modalCalculadoraBilletes').modal('show');
        },
        formatInputValue: function (value) {
            return Number(value || 0).toFixed(2);
        },
        buildDetalleBlock: function (resultado) {
            var data = buildResultado(resultado || {});
            return DETALLE_START + '\n' + data.detalleMultilinea + '\n' + DETALLE_END;
        },
        mergeDetalleBlock: function (textoActual, resultado) {
            return mergeDetalle(textoActual, resultado);
        }
    };

    $(function () {
        bindEvents();
        actualizarTextoTicket();
        refrescarEstadoAgente();
    });
})(window, window.jQuery);
