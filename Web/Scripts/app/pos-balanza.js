(function (window, $) {
    'use strict';

    function createPOSBalanza(options) {
        var state = {
            modoCantidad: 'Manual',
            productoActual: null,
            lecturaSuspendida: false,
            ultimaLectura: null,
            agenteDisponible: false,
            agenteDetectado: false,
            balanzaConectada: false,
            avisoAgenteMostrado: false
        };

        var mensajes = {
            sinAgente: 'No se detectó el agente local de balanza. Puede continuar con carga manual.',
            desconectada: 'Balanza desconectada',
            inestable: 'El peso está inestable. Espere a que la balanza estabilice antes de registrar el producto.',
            pesoCero: 'No se puede registrar un peso menor o igual a cero.'
        };

        function esPesable(producto) {
            return !!(producto && producto.pesable === true);
        }

        function parseCantidad(value) {
            var text = String(value == null ? '' : value).trim();
            if (!text) return NaN;

            text = text.replace(/[^0-9,.\-]/g, '');
            if (!text) return NaN;

            var firstSep = text.search(/[.,]/);
            if (firstSep >= 0) {
                var intPart = text.slice(0, firstSep).replace(/[.,]/g, '');
                var decPart = text.slice(firstSep + 1).replace(/[.,]/g, '');
                text = intPart + '.' + decPart;
            } else {
                text = text.replace(/[.,]/g, '');
            }

            var number = parseFloat(text);
            return isNaN(number) ? NaN : number;
        }

        function setEstado(text, percent, css) {
            $('#estadoBalanza').text(text || 'Desconocida');
            $('#barraBalanza')
                .removeClass('bg-success bg-warning bg-danger bg-secondary')
                .addClass(css || 'bg-secondary')
                .css('width', (percent || 0) + '%');
        }

        function setInputManual(enabled) {
            $('#inputCantidad')
                .prop('disabled', !enabled)
                .prop('readonly', false);
        }

        function setInputBalanza() {
            $('#inputCantidad')
                .prop('disabled', false)
                .prop('readonly', true);
        }

        function mostrarUltimaLectura() {
            if (state.lecturaSuspendida) {
                return;
            }

            if (!state.ultimaLectura || state.ultimaLectura.conectada !== true) {
                return;
            }

            setInputBalanza();
            $('#inputCantidad').val(state.ultimaLectura.pesoDisplay || state.ultimaLectura.pesoTexto || '');
        }

        function refreshSubtotalAndButton() {
            options.calculateSubtotal();

            if (!esPesable(state.productoActual)) {
                return;
            }

            if (state.modoCantidad !== 'Balanza') {
                return;
            }

            if (!state.ultimaLectura || !state.ultimaLectura.conectada) {
                $('#btnAgregarProducto').prop('disabled', true);
                return;
            }

            var invalid = state.ultimaLectura.inestable === true || Number(state.ultimaLectura.peso || 0) <= 0;
            if (invalid) {
                $('#btnAgregarProducto').prop('disabled', true);
            }
        }

        function aplicarLectura(data) {
            if (!data || state.lecturaSuspendida) {
                return;
            }

            if (data.conectada !== true) {
                return;
            }

            state.ultimaLectura = data;
            setInputBalanza();
            $('#inputCantidad').val(data.pesoDisplay || data.pesoTexto || '');
            refreshSubtotalAndButton();
        }

        function activarModoManual(focusCantidad, suspenderLectura) {
            state.modoCantidad = 'Manual';
            if (suspenderLectura === true) {
                state.lecturaSuspendida = true;
            }
            if (window.CarnisysBalanza) {
                window.CarnisysBalanza.desactivar();
            }

            setInputManual(true);
            if (focusCantidad !== false) {
                options.focusCantidad();
            }
        }

        function activarModoBalanza(focusCodigo) {
            state.lecturaSuspendida = false;
            state.modoCantidad = 'Balanza';

            if (window.CarnisysBalanza) {
                window.CarnisysBalanza.activar();
                window.CarnisysBalanza.leerAhora().catch(function () { });
            }

            mostrarUltimaLectura();

            if (focusCodigo === true) {
                options.focusCodigo();
            }
        }

        function intentarModoBalanza() {
            if (!esPesable(state.productoActual)) {
                activarModoManual(false, false);
                return;
            }

            if (!window.CarnisysBalanza || !state.agenteDetectado) {
                activarModoManual(false, false);
                if (!state.avisoAgenteMostrado) {
                    options.showNotice(mensajes.sinAgente);
                    state.avisoAgenteMostrado = true;
                }
                return;
            }

            if (!state.balanzaConectada) {
                activarModoManual(false, false);
                options.showNotice(mensajes.desconectada);
                return;
            }

            state.lecturaSuspendida = false;
            state.modoCantidad = 'Balanza';
            setInputBalanza();
            window.CarnisysBalanza.activar();
            if (state.ultimaLectura) {
                aplicarLectura(state.ultimaLectura);
            } else {
                $('#inputCantidad').val('');
            }
        }

        function onProductoChanged(producto) {
            state.productoActual = producto || null;
            state.ultimaLectura = window.CarnisysBalanza ? window.CarnisysBalanza.ultimo() : null;

            if (!producto) {
                state.lecturaSuspendida = false;
                options.showNotice('');
                state.modoCantidad = 'Manual';

                if (state.balanzaConectada) {
                    if (window.CarnisysBalanza) {
                        window.CarnisysBalanza.activar();
                    }
                    mostrarUltimaLectura();
                    return;
                }

                $('#inputCantidad').val('').prop('disabled', true).prop('readonly', false);
                return;
            }

            if (!esPesable(producto)) {
                options.showNotice('');
                $('#inputCantidad').val('');
                setInputManual(true);
                return;
            }

            if (!state.lecturaSuspendida && state.balanzaConectada) {
                mostrarUltimaLectura();
            }
        }

        function onProductoConfirmed(producto) {
            state.productoActual = producto || null;

            if (!producto) {
                return;
            }

            if (!esPesable(producto)) {
                $('#inputCantidad').val('');
                state.modoCantidad = 'Manual';
                state.lecturaSuspendida = true;
                setInputManual(true);
                return;
            }

            state.lecturaSuspendida = false;
            intentarModoBalanza();
        }

        function onStatus(data) {
            state.agenteDetectado = !!(data && data.ok);
            state.agenteDisponible = !!(data && data.ok);
            state.balanzaConectada = !!(data && data.conectada);

            if (!data || data.ok !== true) {
                setEstado('Agente no detectado', 10, 'bg-danger');
                if (esPesable(state.productoActual) && state.modoCantidad === 'Balanza') {
                    activarModoManual(false, false);
                }
                return;
            }

            if (data.conectada) {
                options.showNotice('');
                setEstado('Conectada (' + (data.puerto || data.port || '') + ')', 100, 'bg-success');
                if (!state.lecturaSuspendida && !state.productoActual) {
                    activarModoBalanza(false);
                }
                if (!state.lecturaSuspendida) {
                    mostrarUltimaLectura();
                }
                return;
            }

            setEstado('Balanza desconectada', 35, 'bg-warning');
            if (esPesable(state.productoActual) && state.modoCantidad === 'Balanza') {
                activarModoManual(false, false);
            }
        }

        function onPeso(data) {
            if (!data) {
                return;
            }

            state.ultimaLectura = data;
            state.balanzaConectada = !!data.conectada;

            if (data.conectada !== true) {
                setEstado('Balanza desconectada', 35, 'bg-warning');
                return;
            }

            if (data.inestable) {
                setEstado('Peso inestable', 70, 'bg-warning');
            } else {
                setEstado('Conectada', 100, 'bg-success');
            }

            aplicarLectura(data);
        }

        function validarAntesDeAgregar(productoSeleccionado, cantidad) {
            if (!esPesable(productoSeleccionado)) {
                return { ok: true };
            }

            if (state.modoCantidad === 'Manual') {
                if (!(cantidad > 0)) {
                    return { ok: false, message: mensajes.pesoCero };
                }

                return { ok: true };
            }

            if (!state.agenteDisponible || !state.balanzaConectada || !state.ultimaLectura) {
                return cantidad > 0
                    ? { ok: true }
                    : { ok: false, message: mensajes.pesoCero };
            }

            if (Number(state.ultimaLectura.peso || 0) <= 0) {
                return { ok: false, message: mensajes.pesoCero };
            }

            if (state.ultimaLectura.inestable === true) {
                return { ok: false, message: mensajes.inestable };
            }

            return { ok: true };
        }

        function bindKeyboardShortcut() {
            $(document).on('keydown.pos-balanza', function (e) {
                if (e.key !== '*' && e.code !== 'NumpadMultiply') {
                    return;
                }

                if ($('.modal.show').length) {
                    return;
                }

                e.preventDefault();
                alternarDesdeAsterisco(e.target && e.target.id ? e.target.id : '');
            });
        }

        function alternarDesdeAsterisco(inputId) {
            var origen = String(inputId || '');

            if (state.modoCantidad === 'Balanza') {
                $('#inputCantidad').val('');
                activarModoManual(origen !== 'inputCodigo', true);
                return;
            }

            if (!state.balanzaConectada) {
                activarModoManual(origen !== 'inputCodigo', true);
                return;
            }

            activarModoBalanza(origen === 'inputCantidad');
        }

        return {
            init: function () {
                setEstado('Desconocida', 0, 'bg-secondary');
                bindKeyboardShortcut();

                if (window.CarnisysBalanza) {
                    window.CarnisysBalanza.start({
                        baseUrl: options.baseUrl,
                        statusIntervalMs: options.statusIntervalMs,
                        pesoIntervalMs: options.pesoIntervalMs,
                        onStatus: onStatus,
                        onPeso: onPeso,
                        onError: function () {
                            if (!state.agenteDetectado) {
                                setEstado('Agente no detectado', 10, 'bg-danger');
                            }
                        }
                    });
                    window.CarnisysBalanza.leerAhora().catch(function () { });
                }
            },
            onProductoChanged: onProductoChanged,
            onProductoConfirmed: onProductoConfirmed,
            beforeAddProduct: validarAntesDeAgregar,
            activarModoManual: activarModoManual,
            alternarDesdeAsterisco: alternarDesdeAsterisco,
            getModoCantidad: function () { return state.modoCantidad; },
            getUltimaLectura: function () { return state.ultimaLectura; }
        };
    }

    window.POSBalanza = {
        create: createPOSBalanza
    };
})(window, window.jQuery);
