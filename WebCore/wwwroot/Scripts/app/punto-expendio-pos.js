// Port recortado de Web/Scripts/app/punto-expendio-pos.js (974 lineas) -- UI de Expendios, ver
// docs/10-migracion-aspnet-core/PLAN-POS-EXPENDIOS-UI.md. Recortes deliberados respecto al
// original (mismo criterio ya usado en Ventas/POS.cshtml):
//   - Sin "Mis expendios" (F6) ni su modal -- misExpendiosCache y todas las funciones
//     relacionadas (loadMisExpendios/renderMisExpendios/etc.) se sacaron enteras.
//   - Sin PosOperadorConfig/login de operador de produccion -- "esperandoOperadorPOS" queda
//     siempre false (window.PosOperadorConfig nunca se define en WebCore).
//   - Sin buscar cliente real (#btnBuscarPersona esta disabled en la vista) -- se saco el
//     handler que abria el modal de busqueda de personas.
//   - window.PostPuntoExpendioModal.open(resp) (modal-postexpendio.js, no portado) reemplazado
//     por el wiring directo del modal basico (_ModalPostPuntoExpendioBasico.cshtml, solo PDF +
//     email, mismos endpoints ya portados que ExpendiosGenerados.cshtml).
(function (window, $, document) {
    'use strict';

    function parseMoney(value) {
        return parseDecimal(String(value == null ? '' : value).replace(/\$/g, '').trim());
    }

    function parseDecimal(value) {
        var text = String(value == null ? '' : value).trim();
        if (!text) return 0;

        text = text.replace(/\s/g, '');

        var lastComma = text.lastIndexOf(',');
        var lastDot = text.lastIndexOf('.');
        var decimalSep = '';

        if (lastComma >= 0 && lastDot >= 0) decimalSep = lastComma > lastDot ? ',' : '.';
        else if (lastComma >= 0) decimalSep = ',';
        else if (lastDot >= 0) decimalSep = '.';

        var normalized = '';
        var decimalIndex = decimalSep ? text.lastIndexOf(decimalSep) : -1;

        for (var i = 0; i < text.length; i++) {
            var ch = text.charAt(i);
            if (ch >= '0' && ch <= '9') normalized += ch;
            else if (ch === '-' && normalized.length === 0) normalized += ch;
            else if ((ch === ',' || ch === '.') && i === decimalIndex) normalized += '.';
        }

        var n = parseFloat(normalized);
        return isNaN(n) ? 0 : n;
    }

    $(function () {
        var config = window.puntoExpendioPosConfig || {};
        if (!document.getElementById('pos-app')) return;

        window.buscarProductoUrl = config.urlBuscarProductoPos;

        var POSState = window.POSState;
        POSState.clear();
        POSState.setFechaVenta(new Date());

        var fechaInput = document.getElementById('fechaExpendio');
        var fechaLabel = document.getElementById('fechaHoraPOSExpendio');

        function pad(value) {
            return String(value).padStart(2, '0');
        }

        function formatFechaSql(fecha) {
            return fecha.getFullYear() + '-' + pad(fecha.getMonth() + 1) + '-' + pad(fecha.getDate()) +
                'T' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes()) + ':' + pad(fecha.getSeconds());
        }

        function formatFechaVisible(fecha) {
            var dias = ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'];
            var meses = ['enero', 'febrero', 'marzo', 'abril', 'mayo', 'junio', 'julio', 'agosto', 'septiembre', 'octubre', 'noviembre', 'diciembre'];

            return dias[fecha.getDay()] + ' ' + fecha.getDate() + ' de ' + meses[fecha.getMonth()] + ', ' +
                fecha.getFullYear() + '. ' + pad(fecha.getHours()) + ':' + pad(fecha.getMinutes());
        }

        function actualizarFechaHora() {
            var lineas = POSState.getLineas().filter(function (linea) { return !!linea; });
            if (!(lineas.length === 0 || lineas.length === 1)) return;

            var ahora = new Date();
            POSState.setFechaVenta(ahora);

            if (fechaInput) fechaInput.value = formatFechaSql(ahora);
            if (fechaLabel) fechaLabel.textContent = formatFechaVisible(ahora);
        }

        function beep() {
            var audio = document.getElementById('beep');
            if (!audio) return;
            audio.currentTime = 0;
            audio.play().catch(function () { });
        }

        document.addEventListener('click', function habilitarAudio() {
            var audio = document.getElementById('beep');
            if (!audio) return;
            audio.volume = 0;
            audio.play().finally(function () {
                audio.pause();
                audio.currentTime = 0;
                audio.volume = 1;
            });
        }, { once: true });

        var scanner = new BarcodeScanner({
            videoSelector: '#videoScanner',
            containerSelector: '#scannerContainer',
            onCodeDetected: function (codigo) {
                document.querySelector('#inputCodigo').value = codigo;
                document.querySelector('#inputCodigo').focus();
                window.manejarEnter();
            }
        });

        document.getElementById('btnScanner')?.addEventListener('click', function () { scanner.iniciar(); });
        document.getElementById('btnCerrarScanner')?.addEventListener('click', function () { scanner.cerrar(); });
        document.getElementById('btnFlash')?.addEventListener('click', async function () {
            var estado = await scanner.toggleFlash();
            var btn = document.getElementById('btnFlash');
            if (!btn) return;
            btn.textContent = estado ? 'Flash ON' : 'Flash';
            btn.classList.toggle('btn-warning', !!estado);
            btn.classList.toggle('btn-secondary', !estado);
        });

        function getPrecioParaAgregar(productoSeleccionado) {
            if (!config.permiteEditarPrecio) {
                return productoSeleccionado ? productoSeleccionado.precioKg : 0;
            }

            var precioManual = parseDecimal($('#inputPrecioManualExpendio').val());
            return precioManual > 0 ? precioManual : (productoSeleccionado ? productoSeleccionado.precioKg : 0);
        }

        function showConnectionError(msg) {
            Swal.fire({ icon: 'error', title: 'Sin conexión', text: msg });
        }

        function mostrarAvisoBalanzaDiscreto(msg) {
            var $msg = $('#msgBalanzaPOS');
            if (!$msg.length) return;
            $msg.text(msg || '');
        }

        function normalizarProductoParaBalanza(producto) {
            if (!producto) return null;

            var normalized = $.extend({}, producto);
            normalized.pesable = producto.pesable === true || producto.balanza === true;
            normalized.balanza = producto.balanza === true || normalized.pesable === true;
            return normalized;
        }

        function clickIfEnabled(selector) {
            var button = document.querySelector(selector);
            if (button && !button.disabled) button.click();
        }

        function firePosAlert(options) {
            return Swal.fire($.extend(true, {
                focusConfirm: true,
                didOpen: function () {
                    var confirmButton = Swal.getConfirmButton();
                    if (confirmButton) confirmButton.focus();
                }
            }, options || {}));
        }

        function scrollPantallaMobile() {
            if (!window.matchMedia('(max-width: 576px)').matches) return;
            var anchor = document.getElementById('pos-bottom-anchor');
            if (anchor) anchor.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
        }

        var posProduct = null;
        var posCart = null;
        var posBalanza = null;
        var posKeyboard = window.POSKeyboard.create({
            handleEnter: function () { return posProduct ? posProduct.handleEnter() : null; },
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            finishTyping: function (value) { return posProduct ? posProduct.finishTyping(value) : null; },
            setEnterDesdeTecladoVirtual: function (value) { return posProduct ? posProduct.setEnterDesdeTecladoVirtual(value) : null; },
            onManualModeRequested: function (inputId) { return posBalanza ? posBalanza.alternarDesdeAsterisco(inputId) : null; }
        });
        var posHelp = window.POSHelp.create({
            focusCodigo: function () { return posKeyboard.focusCodigo(); },
            clickIfEnabled: clickIfEnabled
        });
        var posComment = window.POSComment.create({
            POSState: POSState,
            focusCodigo: function () { return posKeyboard.focusCodigo(); }
        });

        posProduct = window.POSProduct.create({
            soloFormaPago: false,
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            addProduct: function () { return posCart ? posCart.addProduct() : null; },
            getInputActivo: function () { return posKeyboard.getInputActivo(); },
            clearInputActivo: function () { return posKeyboard.clearInputActivo(); },
            showConnectionError: showConnectionError,
            onProductoChanged: function (producto) { return posBalanza ? posBalanza.onProductoChanged(normalizarProductoParaBalanza(producto)) : null; },
            onProductoConfirmed: function (producto) { return posBalanza ? posBalanza.onProductoConfirmed(normalizarProductoParaBalanza(producto)) : null; }
        });

        posCart = window.POSCart.create({
            POSState: POSState,
            soloFormaPago: false,
            eliminarFisicoLineasPendientes: true,
            deshabilitarAvisoSalida: true,
            puedeBonificar: config.puedeBonificar === true,
            mensajeSinPermisoBonificar: config.mensajeSinPermisoBonificar || 'No tiene permisos para bonificar.',
            getProductoSeleccionado: function () { return posProduct.getProductoSeleccionado(); },
            setProductoSeleccionado: function (value) { return posProduct.setProductoSeleccionado(value); },
            getPrecioActual: function () { return getPrecioParaAgregar(posProduct.getProductoSeleccionado()); },
            getPrecioParaAgregar: getPrecioParaAgregar,
            beep: beep,
            focusCodigo: function () { return posKeyboard.focusCodigo(); },
            showWaiting: function () { return posProduct.showWaiting(); },
            scrollPantallaMobile: scrollPantallaMobile,
            handleEnter: function () { return posProduct.handleEnter(); },
            getTypingTimer: function () { return posProduct.getTypingTimer(); },
            setEnterDesdeTecladoVirtual: function (value) { return posProduct.setEnterDesdeTecladoVirtual(value); },
            beforeAddProduct: function (producto, cantidad) {
                return posBalanza
                    ? posBalanza.beforeAddProduct(normalizarProductoParaBalanza(producto), cantidad)
                    : { ok: true };
            }
        });

        posBalanza = window.POSBalanza.create({
            baseUrl: 'http://127.0.0.1:5100',
            statusIntervalMs: 2500,
            pesoIntervalMs: 250,
            calculateSubtotal: function () { return posCart ? posCart.calculateSubtotal() : null; },
            focusCantidad: function () { return posKeyboard.focusCantidad(); },
            focusCodigo: function () { return posKeyboard.focusCodigo(); },
            showNotice: mostrarAvisoBalanzaDiscreto
        });

        posProduct.init();
        posCart.init();
        posBalanza.init();
        posKeyboard.init();
        posHelp.init();
        posComment.init();
        window.POSMultiInstance?.init?.();
        actualizarFechaHora();
        window.setInterval(actualizarFechaHora, 60000);

        $('#inputPrecioManualExpendio').on('input', function () {
            posCart.calculateSubtotal();
        });

        $('#btnConsumidorFinal').on('click', function () {
            $('#idPersona').val(config.idConsumidorFinal || 0);
            $('#razonSocial').val('Consumidor Final');
            setClienteIdentificacionVisual('', 'Consumidor Final');
        });

        $('#razonSocial').on('input', function () {
            $('#idPersona').val('0');
            setClienteIdentificacionVisual('', '');
        });

        $('#razonSocial').on('keydown', function (e) {
            if (e.key !== 'Enter') return;
            e.preventDefault();
            clickIfEnabled('#btnFinalizar');
        });

        function setClienteIdentificacionVisual(identificacion, razonSocial) {
            var razon = (razonSocial || '').toString().trim();
            var identificacionNormalizada = (identificacion || '').toString().trim();
            var $wrap = $('#clienteRazonSocialWrap');
            var $texto = $('#clienteRazonSocialTexto');
            var $valor = $('#clienteRazonSocialValor');

            if (!$wrap.length || !$texto.length) return;

            if (identificacionNormalizada) $('#razonSocial').val(identificacionNormalizada);

            var mostrar = !!identificacionNormalizada &&
                !!razon &&
                identificacionNormalizada.localeCompare(razon, undefined, { sensitivity: 'accent' }) !== 0;

            if ($valor.length) $valor.val(razon);

            $texto.text(razon);
            $wrap.attr('title', razon);
            $wrap.toggleClass('d-none', !mostrar);
        }
        window.setClienteIdentificacionVisual = setClienteIdentificacionVisual;

        function actualizarSectorUI(sector) {
            var texto = sector || 'Sin seleccionar';
            $('#sectorPuntoExpendio').val(sector || '');
            $('#estadoSectorPuntoExpendioFooter').text(texto);
            $('#btnFinalizar').prop('disabled', !sector);
        }

        function openSectorModal() {
            $('#modalSectoresPuntoExpendio').modal('show');
            setTimeout(function () {
                $('#txtBuscarSectorExpendio').focus().select();
            }, 50);
        }

        $('#txtBuscarSectorExpendio').on('input', function () {
            var texto = ($(this).val() || '').toLowerCase();
            $('#listaSectoresExpendio .js-sector-item').each(function () {
                var $item = $(this);
                $item.toggle(($item.text() || '').toLowerCase().indexOf(texto) >= 0);
            });
        });

        // Siempre incluye posInstanceId al navegar de vuelta a POS -- mismo motivo que en Ventas/
        // POS.cshtml: si se arma la URL sin el, pos-multi-instance.js genera uno nuevo y pierde
        // el registro de "instancia ya abierta" contra localStorage.
        function urlPosConSector(sector) {
            var params = [];
            if (sector) params.push('sector=' + encodeURIComponent(sector));
            if (config.posInstanceId) params.push('posInstanceId=' + encodeURIComponent(config.posInstanceId));
            return config.urlPos + (params.length ? ('?' + params.join('&')) : '');
        }

        $(document).on('click', '.js-sector-item', function () {
            var sector = $(this).data('sector');
            if (!sector) return;
            window.location.href = urlPosConSector(sector);
        });

        if (!config.sectorSeleccionado) {
            openSectorModal();
        } else {
            actualizarSectorUI(config.sectorSeleccionado);
        }

        function construirPayload() {
            var lineas = POSState.getLineas()
                .filter(function (linea) { return linea && !linea.anulado; })
                .map(function (linea) {
                    return {
                        IdCorte: parseInt(linea.idCorte || 0, 10) || 0,
                        Codigo: parseInt(linea.codigo || 0, 10) || 0,
                        Descripcion: linea.descripcion || linea.producto || '',
                        CantKg: parseDecimal(linea.cant),
                        PrecioKg: parseMoney(linea.precio),
                        Importe: parseMoney(linea.subtotal),
                        Estado: 0,
                        Balanza: linea.balanza === true
                    };
                });

            return {
                FechaExpendio: $('#fechaExpendio').val(),
                Sector: $('#sectorPuntoExpendio').val(),
                IdentificacionCliente: ($('#razonSocial').val() || '').trim(),
                Observaciones: POSState.getObservaciones(),
                LineasVenta: lineas,
                PosInstanceId: config.posInstanceId || ''
            };
        }

        // ===== Post-guardado basico (PDF + email, ver _ModalPostPuntoExpendioBasico.cshtml) =====
        function mostrarModalPostExpendio(resp) {
            beep();
            $('#ppebResumen').text('Expendio #' + resp.idExpendio + ' registrado correctamente.');
            $('#btnPpebPdf').attr('href', resp.pdfUrl);
            $('#btnPpebPdf, #btnPpebEmail').data('id-expendio', resp.idExpendio);
            $('#modalPostPuntoExpendioBasico').modal('show');
        }

        $('#btnPpebContinuar').on('click', function () {
            $('#modalPostPuntoExpendioBasico').modal('hide');
            window.location.href = urlPosConSector($('#sectorPuntoExpendio').val());
        });

        $('#btnPpebEmail').on('click', function () {
            var idExpendio = $(this).data('id-expendio');
            $('#ppebEmailError').addClass('d-none').text('');

            $.getJSON(config.urlEmailConfig, { idExpendio: idExpendio }).done(function (resp) {
                if (!resp || !resp.ok) {
                    Swal.fire({ icon: 'error', title: 'No se pudo preparar el email', text: (resp && resp.msg) || 'Error desconocido.' });
                    return;
                }

                $('#ppebEmailDestino').val(resp.email || '');
                $('#ppebEmailAsunto').val(resp.asunto || '');
                $('#ppebEmailMensaje').val(resp.mensaje || '');
                $('#modalEmailPostPuntoExpendio').modal('show');
            }).fail(function () {
                Swal.fire({ icon: 'error', title: 'No se pudo preparar el email', text: 'Error de conexión.' });
            });
        });

        $('#btnPpebConfirmarEmail').on('click', function () {
            var idExpendio = $('#btnPpebEmail').data('id-expendio');
            var email = ($('#ppebEmailDestino').val() || '').trim();
            var asunto = ($('#ppebEmailAsunto').val() || '').trim();
            var mensaje = ($('#ppebEmailMensaje').val() || '').trim();

            if (!email) {
                $('#ppebEmailError').removeClass('d-none').text('Ingrese un email destino.');
                return;
            }

            var $btn = $(this);
            $btn.prop('disabled', true);

            $.post(config.urlEmailEnviar, {
                idExpendio: idExpendio,
                emailDestino: email,
                asunto: asunto,
                mensaje: mensaje
            }).done(function (resp) {
                $btn.prop('disabled', false);

                if (!resp || !resp.ok) {
                    $('#ppebEmailError').removeClass('d-none').text((resp && resp.msg) || 'No se pudo enviar el email.');
                    return;
                }

                $('#modalEmailPostPuntoExpendio').modal('hide');
                Swal.fire({ icon: 'success', title: 'Email enviado', text: 'El comprobante se envió correctamente.' });
            }).fail(function (xhr) {
                $btn.prop('disabled', false);
                $('#ppebEmailError').removeClass('d-none').text((xhr.responseJSON && xhr.responseJSON.msg) || 'No se pudo enviar el email.');
            });
        });

        var guardando = false;

        $('#btnFinalizar').on('click', function () {
            if (guardando) return;

            var payload = construirPayload();
            if (!payload.Sector) {
                firePosAlert({ icon: 'warning', title: 'Sector', text: 'Debe seleccionar un sector.' });
                openSectorModal();
                return;
            }

            if (!payload.LineasVenta.length) {
                firePosAlert({ icon: 'warning', title: 'Punto de expendio', text: 'Debe agregar al menos un producto.' });
                posKeyboard.focusCodigo();
                return;
            }

            guardando = true;
            $('#btnFinalizar').prop('disabled', true);

            $.ajax({
                url: config.urlFinalizar,
                type: 'POST',
                dataType: 'json',
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(payload)
            })
                .done(function (resp) {
                    if (!resp || !resp.ok) {
                        firePosAlert({
                            icon: 'error',
                            title: 'Punto de Expendio',
                            text: resp && resp.mensaje ? resp.mensaje : 'No se pudo guardar el punto de expendio.'
                        });
                        return;
                    }

                    mostrarModalPostExpendio(resp);
                })
                .fail(function () {
                    firePosAlert({
                        icon: 'error',
                        title: 'Punto de Expendio',
                        text: 'No se pudo guardar el punto de expendio.'
                    });
                })
                .always(function () {
                    guardando = false;
                    $('#btnFinalizar').prop('disabled', false);
                });
        });

        $('#btnCancelarItem').on('click', function () {
            var lineas = POSState.getLineas().filter(function (linea) { return !!linea; });
            if (!lineas.length) {
                window.location.href = urlPosConSector($('#sectorPuntoExpendio').val());
                return;
            }

            Swal.fire({
                icon: 'warning',
                title: 'Cancelar registro',
                text: 'Se quitarán las líneas cargadas en este punto de expendio.',
                showCancelButton: true,
                confirmButtonText: 'Sí, cancelar',
                cancelButtonText: 'No'
            }).then(function (result) {
                if (!result.isConfirmed) return;
                POSState.clear();
                posCart.renderTable(POSState.getLineas());
                posCart.recalculateTotal();
                posCart.updateSaleState();
                posComment.updateButtonState();
                posProduct.showWaiting();
                posKeyboard.focusCodigo();
            });
        });

        document.addEventListener('keydown', function (e) {
            if ($('.modal.show').length && !$('#modalAyudaPOS').hasClass('show')) return;

            if (e.key === 'End') {
                e.preventDefault();

                var $cliente = $('#razonSocial');
                var clienteVacio = !($cliente.val() || '').trim();
                var clienteTieneFoco = document.activeElement === $cliente.get(0);

                if (clienteVacio && !clienteTieneFoco) {
                    $cliente.trigger('focus');
                    return;
                }

                clickIfEnabled('#btnFinalizar');
                return;
            }

            if (e.key === 'Home') {
                e.preventDefault();
                posKeyboard.focusCodigo();
                return;
            }

            if (e.key === 'PageDown') {
                e.preventDefault();
                $('#razonSocial').trigger('focus');
                return;
            }

            if (e.key === 'F9') {
                e.preventDefault();
                clickIfEnabled('#btnBuscarPersona');
                return;
            }

            if (e.key === 'F10') {
                e.preventDefault();
                if (typeof window.abrirBuscadorProductosPOS === 'function') {
                    window.abrirBuscadorProductosPOS();
                }
            }
        });

        window.posHotkeysHooks = window.posHotkeysHooks || {};
        window.posHotkeysHooks.AvPag = function () {
            $('#razonSocial').trigger('focus');
        };
        // F6 (Mis expendios) NO se registra a proposito -- no portado en este MVP, ver header.
        window.posHotkeysHooks.F4 = function () {
            $('#tablaItems tr.fila-item').last().trigger('click');
        };

        setTimeout(function () {
            posKeyboard.focusCodigo();
        }, 50);
    });
})(window, window.jQuery, document);
