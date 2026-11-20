// Port literal de Web/Scripts/app/pos-balanza.js (batch 4, ver
// docs/10-migracion-aspnet-core/PLAN-POS-UI.md). Sin cambios de logica -- depende de
// Scripts/carnisys.balanza.js (tambien portado sin cambios), degrada solo a modo manual si el
// agente local (http://127.0.0.1:5100) no responde.
// 2026-11-10: con peso inestable ya no hay modal: el aviso va debajo de Cantidad (#avisoCantidadPOS,
// presente en Ventas/POS y PuntosExpendio/POS) y el cursor queda en Cantidad para reintentar con Enter.
// 2026-11-20: Cantidad en modo balanza muestra y valida SOLO la lectura vigente (CarnisysBalanza.lecturaVigente): una lectura
// congelada, con error o sin respuesta del agente deja de verse al vencer (docs/DECISIONS.md "Balanza: lectura vigente").
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
            perdida: 'Se perdió la conexión con la balanza. Cargue la cantidad a mano o vuelva a activarla con *.',
            inestable: 'Peso inestable, espere y presione Enter',
            pesoCero: 'No se puede registrar un peso menor o igual a cero.'
        };

        // Payload del agente normalizado (marca 'vencida' y la trata como no conectada). Sin el cliente, se usa tal cual.
        function normalizar(data) {
            if (!data) return null;
            return window.CarnisysBalanzaUtils && typeof window.CarnisysBalanzaUtils.normalize === 'function'
                ? window.CarnisysBalanzaUtils.normalize(data)
                : data;
        }

        function lecturaVigente() {
            return window.CarnisysBalanza && typeof window.CarnisysBalanza.lecturaVigente === 'function'
                ? window.CarnisysBalanza.lecturaVigente()
                : null;
        }

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

        // Muestra (msg) o borra ('') el aviso de peso inestable debajo de Cantidad. Devuelve false si la
        // vista no tiene el contenedor, para que el llamador caiga al modal de siempre.
        function setAvisoCantidad(msg) {
            var $aviso = $('#avisoCantidadPOS');
            if (!$aviso.length) {
                return false;
            }

            var texto = String(msg || '');
            $aviso.empty();
            if (texto) {
                $aviso.append($('<i class="fas fa-exclamation-triangle" aria-hidden="true"></i>'), document.createTextNode(' ' + texto));
            }
            $aviso.closest('.pos-product-metric-cantidad').toggleClass('pos-cantidad-inestable', !!texto);
            return true;
        }

        // Marca el bloque de Cantidad cuando el valor viene de la balanza: el CSS muestra ahi el icono de balanza
        // (.pos-cantidad-icono-balanza). No se usa [readonly] como indicador porque pos-keyboard.js tambien lo
        // pone en pantallas tactiles.
        function marcarCantidadDeBalanza(activa) {
            $('.pos-product-metric-cantidad').toggleClass('pos-cantidad-balanza', !!activa);
        }

        function setInputManual(enabled) {
            marcarCantidadDeBalanza(false);
            $('#inputCantidad')
                .prop('disabled', !enabled)
                .prop('readonly', false);
        }

        function setInputBalanza() {
            marcarCantidadDeBalanza(true);
            $('#inputCantidad')
                .prop('disabled', false)
                .prop('readonly', true);
        }

        function mostrarUltimaLectura() {
            if (state.lecturaSuspendida) {
                return;
            }

            var vigente = lecturaVigente();
            if (!vigente) {
                // En modo balanza sin lectura vigente Cantidad queda vacia: nunca un peso viejo listo para Enter.
                if (state.modoCantidad === 'Balanza') $('#inputCantidad').val('');
                return;
            }

            setInputBalanza();
            $('#inputCantidad').val(vigente.pesoDisplay || vigente.pesoTexto || '');
        }

        function refreshSubtotalAndButton() {
            options.calculateSubtotal();

            if (!esPesable(state.productoActual)) {
                return;
            }

            if (state.modoCantidad !== 'Balanza') {
                return;
            }

            var vigente = lecturaVigente();
            if (!vigente) {
                $('#btnAgregarProducto').prop('disabled', true);
                return;
            }

            var invalid = vigente.inestable === true || Number(vigente.peso || 0) <= 0;
            if (invalid) {
                $('#btnAgregarProducto').prop('disabled', true);
            }
        }

        // Muestra en Cantidad la lectura vigente (el parametro solo indica que hubo una lectura; el peso sale de lecturaVigente).
        function aplicarLectura(data) {
            if (!data || state.lecturaSuspendida) {
                return;
            }

            var vigente = lecturaVigente();
            if (!vigente) {
                if (state.modoCantidad === 'Balanza') {
                    $('#inputCantidad').val('');
                    refreshSubtotalAndButton();
                }
                return;
            }

            state.ultimaLectura = vigente;
            setInputBalanza();
            $('#inputCantidad').val(vigente.pesoDisplay || vigente.pesoTexto || '');
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
            setAvisoCantidad('');
            state.ultimaLectura = window.CarnisysBalanza ? normalizar(window.CarnisysBalanza.ultimo()) : null;

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

                marcarCantidadDeBalanza(false);
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

        // Corte automatico: la balanza se cae con la lectura activa (sin que el operador haga nada). Se pasa a manual,
        // se vacia Cantidad (que no quede un peso viejo que un Enter pueda guardar) y se avisa. Exige 2 chequeos
        // consecutivos sin conexion para no cortar por un solo poll fallido del agente local.
        var chequeosSinConexion = 0;
        var huboConexion = false; // el aviso de corte solo sale si desde que se abrio la pantalla hubo lectura de la balanza
        function registrarChequeoDesconectado() {
            // Solo cuenta con la lectura activa: en manual el contador no acumula (si no, al activarla cortaria de inmediato).
            if (state.modoCantidad !== 'Balanza') {
                chequeosSinConexion = 0;
                return;
            }
            if (!huboConexion) return;
            chequeosSinConexion += 1;
            if (chequeosSinConexion < 2) {
                return;
            }

            var cantidadTieneFoco = document.activeElement && document.activeElement.id === 'inputCantidad';
            $('#inputCantidad').val('');
            activarModoManual(cantidadTieneFoco, true);
            options.calculateSubtotal();
            options.showNotice(mensajes.perdida);
        }

        function onStatus(data) {
            var normalizado = normalizar(data);
            state.agenteDetectado = !!(data && data.ok);
            state.agenteDisponible = !!(data && data.ok);
            state.balanzaConectada = !!(normalizado && normalizado.conectada);

            if (!data || data.ok !== true) {
                setEstado('Agente no detectado', 10, 'bg-danger');
                registrarChequeoDesconectado();
                return;
            }

            // El agente responde pero su ultima lectura no se renueva (hilo de lectura trabado): no hay peso real.
            if (normalizado && normalizado.vencida) {
                setEstado('Lectura congelada (sin peso nuevo)', 20, 'bg-danger');
                mostrarUltimaLectura();
                registrarChequeoDesconectado();
                return;
            }

            if (state.balanzaConectada) {
                chequeosSinConexion = 0;
                huboConexion = true;
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
            registrarChequeoDesconectado();
        }

        function onPeso(payload) {
            if (!payload) {
                return;
            }

            var data = normalizar(payload);
            state.ultimaLectura = data;
            state.balanzaConectada = !!data.conectada;

            if (data.conectada !== true) {
                if (data.vencida) {
                    setEstado('Lectura congelada (sin peso nuevo)', 20, 'bg-danger');
                } else {
                    setEstado('Balanza desconectada', 35, 'bg-warning');
                }
                setAvisoCantidad('');
                // Al vencer la ultima lectura valida, Cantidad (modo balanza) queda vacia en vez de conservar el peso viejo.
                mostrarUltimaLectura();
                refreshSubtotalAndButton();
                registrarChequeoDesconectado();
                return;
            }

            chequeosSinConexion = 0;
            huboConexion = true;
            if (data.inestable) {
                setEstado('Peso inestable', 70, 'bg-warning');
            } else {
                setEstado('Conectada', 100, 'bg-success');
                // El aviso solo se prende al apretar Enter (validarAntesDeAgregar); se apaga solo al estabilizar.
                setAvisoCantidad('');
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

            // Modo balanza: hace falta una lectura vigente, estable y > 0. Antes, sin agente o sin lectura se aceptaba lo que hubiera
            // en Cantidad (podia ser el peso viejo). Sin el cliente de balanza (script ausente) queda la regla manual.
            if (!window.CarnisysBalanza || typeof window.CarnisysBalanza.validarParaAgregar !== 'function') {
                return cantidad > 0 ? { ok: true } : { ok: false, message: mensajes.pesoCero };
            }

            var chequeo = window.CarnisysBalanza.validarParaAgregar();
            if (chequeo.motivo === 'sinLectura') {
                mostrarUltimaLectura();
                return { ok: false, message: chequeo.mensaje };
            }

            if (chequeo.motivo === 'pesoCero') {
                return { ok: false, message: mensajes.pesoCero };
            }

            if (chequeo.motivo === 'inestable') {
                // inline: pos-cart no abre modal y deja el foco en Cantidad para reintentar con Enter.
                // Sin #avisoCantidadPOS en la vista, se mantiene el modal de antes.
                return setAvisoCantidad(mensajes.inestable)
                    ? { ok: false, inline: true, message: mensajes.inestable }
                    : { ok: false, message: mensajes.inestable };
            }

            setAvisoCantidad('');
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
                // Sin balanza el * deja Cantidad vacia: si habia un valor tipeado a mano, un Enter distraido lo guardaria.
                $('#inputCantidad').val('');
                activarModoManual(origen !== 'inputCodigo', true);
                options.calculateSubtotal();
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
