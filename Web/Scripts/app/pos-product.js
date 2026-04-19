(function (window, $) {
    'use strict';

    function createPOSProduct(options) {
        // Este modulo concentra el estado del producto que el cajero esta
        // trabajando en este momento: codigo, resultado de la busqueda,
        // precio actual y reglas especiales como "2X123".
        let typingTimer = null;
        let productoSeleccionado = null;
        let precioActual = 0;
        let buscandoProducto = false;
        let ultimoCodigoPedido = null;
        let reqProducto = null;
        let enterDesdeTecladoVirtual = false;

        // Normaliza el codigo para que todas las comparaciones hablen el mismo
        // idioma: mayusculas, sin espacios sobrantes y con la X consistente.
        function normalizeInput(raw) {
            return String(raw ?? '')
                .trim()
                .toUpperCase()
                .replace(/\u00D7/g, 'X');
        }

        // Convierte un numero escrito con formato local a float.
        // Acepta "1,25" o "1.25".
        function parseFloatAR(value) {
            const number = parseFloat(String(value ?? '').trim().replace(',', '.'));
            return Number.isFinite(number) ? number : NaN;
        }

        // Detecta el formato "cantidad X codigo".
        // Si la entrada no aplica, devolvemos null para seguir con la busqueda normal.
        function parseCantidadXCodigo(input) {
            const normalized = normalizeInput(input);
            const match = normalized.match(/^([0-9]+(?:[.,][0-9]+)?)X(.+)$/);
            if (!match) return null;

            const cantidad = parseFloatAR(match[1]);
            const codigo = String(match[2] ?? '').trim();

            if (!codigo) return null;
            if (!Number.isFinite(cantidad) || cantidad <= 0) return null;

            return {
                cant: cantidad,
                codigo: codigo
            };
        }

        // Devuelve el foco al input principal del POS.
        function focusCodigo(selectText) {
            const $input = $('#inputCodigo');
            $input.focus();

            if (selectText !== false) {
                $input.select();
            }
        }

        // Lleva el foco a cantidad cuando ya confirmamos un producto valido.
        function focusCantidad() {
            $('#inputCantidad').focus().select();
        }

        // Restablece el panel de producto al estado neutro.
        function showWaiting() {
            productoSeleccionado = null;
            precioActual = 0;

            $('#prodNombre').text('Info producto...').removeClass('fw-bold').addClass('text-muted');
            $('#prodPrecio').text('');
            $('#prodSubtotal').text('$ 0,00');
            $('#inputCantidad').prop('disabled', true).val('');
            $('#btnAgregarProducto').prop('disabled', true);
        }

        // Muestra un mensaje de espera mientras el backend responde.
        function showSearching(message, keepQuantity) {
            const cantidadActual = keepQuantity ? $('#inputCantidad').val() : '';

            productoSeleccionado = null;
            precioActual = 0;

            $('#prodNombre').text(message || 'Buscando...').removeClass('fw-bold').addClass('text-muted');
            $('#prodPrecio').text('').addClass('text-muted');
            $('#prodSubtotal').text('$ 0,00');
            $('#inputCantidad').prop('disabled', true).val(cantidadActual);
            $('#btnAgregarProducto').prop('disabled', true);
        }

        // Deja la UI lista para un nuevo intento cuando no encontramos coincidencia.
        function showNoMatch(message) {
            productoSeleccionado = null;
            precioActual = 0;

            $('#prodNombre').text(message || 'Sin coincidencia').removeClass('fw-bold').addClass('text-muted');
            $('#prodPrecio').text('').addClass('text-muted');
            $('#prodSubtotal').text('$ 0,00');
            $('#inputCantidad').prop('disabled', true).val('');
            $('#btnAgregarProducto').prop('disabled', true);

            focusCodigo(false);
        }

        // Carga en pantalla el producto elegido y delega el subtotal al modulo
        // del carrito, que es quien conoce la logica de cantidades/subtotales.
        function showProduct(producto) {
            $('#prodNombre').text(producto.nombre).removeClass('text-muted').addClass('fw-bold');
            $('#prodPrecio').text('$ ' + producto.precioKg.toLocaleString('es-AR')).removeClass('text-muted').addClass('fw-bold');

            precioActual = producto.precioKg;

            $('#inputCantidad').prop('disabled', false);
            $('#prodSubtotal').text('$ 0,00');
            options.calculateSubtotal();
            $('#btnAgregarProducto').prop('disabled', false);
        }

        // Hace la consulta real al backend. Mantiene varias defensas del flujo
        // original: abortar requests viejos, ignorar respuestas atrasadas y
        // permitir callbacks para los casos de auto-agregado.
        function finishTyping(codigo, callback, ingresoCantidadXParam) {
            const codigoTrim = normalizeInput(codigo);

            if (!codigoTrim) {
                showWaiting();
                return;
            }

            showSearching(null, Boolean(ingresoCantidadXParam));

            if (reqProducto && reqProducto.readyState !== 4) {
                reqProducto.abort();
            }

            buscandoProducto = true;
            ultimoCodigoPedido = codigoTrim;

            reqProducto = $.ajax({
                url: window.buscarProductoUrl,
                type: 'GET',
                dataType: 'json',
                data: {
                    codigo: codigoTrim,
                    ingresoCantidadX: Boolean(ingresoCantidadXParam)
                },
                timeout: 3000,
                success: function (data) {
                    buscandoProducto = false;

                    const codigoActual = normalizeInput($('#inputCodigo').val());
                    if (codigoActual !== ultimoCodigoPedido) return;

                    if (data && data.success === false) {
                        productoSeleccionado = null;
                        showNoMatch(data.message);

                        if (typeof callback === 'function') {
                            callback(false, data);
                        }
                        return;
                    }

                    productoSeleccionado = data;
                    showProduct(data);

                    // Si el codigo es EAN valido, conservamos el comportamiento
                    // heredado: cantidad 1 y continuacion automatica del flujo.
                    if (typeof window.esEANValido === 'function' && window.esEANValido(codigoTrim)) {
                        $('#inputCantidad').val('1');
                        document.querySelector('#inputCantidad')?.focus();
                        handleEnter();
                    }

                    if (typeof callback === 'function') {
                        callback(true, data);
                    }
                },
                error: function (xhr, status) {
                    if (status === 'abort') return;

                    buscandoProducto = false;

                    if (status === 'timeout') {
                        options.showConnectionError('La conexion es lenta. Reintente.');
                        return;
                    }

                    if (!navigator.onLine) {
                        options.showConnectionError('Sin conexion a Internet');
                        return;
                    }

                    options.showConnectionError('No se pudo contactar al servidor');
                }
            });
        }

        // Interpreta formatos rapidos como "2X123" o un EAN directo.
        // Si consigue resolverlos y dispara auto-agregado, devuelve true.
        function processCodeWithQuantity() {
            const input = document.getElementById('inputCodigo');
            if (!input) return false;

            const entrada = normalizeInput(input.value);
            const parsed = parseCantidadXCodigo(entrada);

            if (parsed) {
                $('#inputCantidad').val(String(parsed.cant));
                $('#inputCodigo').val(parsed.codigo);

                finishTyping(parsed.codigo, function (ok) {
                    if (ok === false) return;
                    options.addProduct();
                    showWaiting();
                }, true);

                return true;
            }

            if (typeof window.esEANValido === 'function' && window.esEANValido(entrada)) {
                $('#inputCantidad').val('1');
                $('#inputCodigo').val(entrada);

                finishTyping(entrada, function (ok) {
                    if (ok === false) return;
                    options.addProduct();
                    showWaiting();
                }, true);

                return true;
            }

            return false;
        }

        // Centraliza la accion Enter del bloque de producto.
        // Si el foco esta en cantidad, agrega.
        // Si el foco esta en codigo, busca o intenta auto-agregado.
        function handleEnter() {
            const inputActivo = options.getInputActivo();
            if (!inputActivo) return;

            if (inputActivo.id === 'inputCantidad') {
                options.addProduct();
                enterDesdeTecladoVirtual = false;
                return;
            }

            if (enterDesdeTecladoVirtual || inputActivo.id === 'inputCodigo') {
                const autoAgregado = processCodeWithQuantity();
                enterDesdeTecladoVirtual = false;

                if (autoAgregado) return;

                const codigoInput = normalizeInput($('#inputCodigo').val());

                if (codigoInput.includes('X')) {
                    showNoMatch('Formato invalido (use 2X123)');
                    return;
                }

                if (buscandoProducto) return;

                finishTyping(codigoInput, function () {
                    const codigoActual = normalizeInput($('#inputCodigo').val());
                    const codigoProducto = normalizeInput(productoSeleccionado?.codigo);

                    if (!codigoProducto || codigoProducto !== codigoActual) {
                        showNoMatch();
                        productoSeleccionado = null;
                        return;
                    }

                    focusCantidad();
                });
            }
        }

        // Abre el modal global de productos y, cuando el usuario elige uno,
        // reusa el mismo flujo de busqueda que usa el input de codigo.
        function openSearchModal() {
            if (typeof window.abrirBuscarProductoModal !== 'function') {
                alert('No se pudo abrir el buscador de productos. Verifica que el script global del modal este cargado.');
                return;
            }

            options.clearInputActivo();

            window.abrirBuscarProductoModal({
                modalSelector: '#modalBuscarProducto',
                mostrarPrecio: true,
                onSelect: function (producto) {
                    const codigo = String((producto && producto.codigo) || '').trim();
                    if (!codigo) {
                        focusCodigo();
                        return;
                    }

                    $('#inputCodigo').val(codigo);
                    clearTimeout(typingTimer);

                    finishTyping(codigo, function (ok) {
                        if (ok === false) {
                            focusCodigo();
                            return;
                        }

                        const codigoActual = normalizeInput($('#inputCodigo').val());
                        const codigoProducto = normalizeInput(productoSeleccionado?.codigo);

                        if (!codigoProducto || codigoProducto !== codigoActual) {
                            showNoMatch();
                            productoSeleccionado = null;
                            focusCodigo();
                            return;
                        }

                        if (typeof window.esEANValido === 'function' && window.esEANValido(codigoActual)) {
                            return;
                        }

                        focusCantidad();
                    });
                }
            });
        }

        // Escucha lo que se escribe en codigo y dispara una busqueda con debounce.
        // El Enter no se procesa aca para no duplicar logica.
        function bindLiveSearch() {
            $('#inputCodigo').on('keyup', function (e) {
                const codigo = String(this.value ?? '').trim().toUpperCase();

                if (e.key === 'Enter') {
                    clearTimeout(typingTimer);
                    return;
                }

                if (codigo.includes('X')) {
                    // Mientras el usuario arma un patron tipo 4X5 o 4X5G
                    // no consultamos al backend ni forzamos el foco.
                    // El procesamiento real se hace al confirmar con Enter.
                    clearTimeout(typingTimer);
                    return;
                }

                clearTimeout(typingTimer);
                typingTimer = setTimeout(function () {
                    finishTyping(codigo);
                }, 250);
            });
        }

        // Mientras el modal de busqueda esta abierto, el teclado del POS no debe
        // quedar apuntando a inputs que no pertenecen a la pantalla principal.
        function bindSearchModalFocus() {
            $('#modalBuscarProducto')
                .off('shown.bs.modal.posBuscar hidden.bs.modal.posBuscar')
                .on('shown.bs.modal.posBuscar', function () {
                    options.clearInputActivo();
                })
                .on('hidden.bs.modal.posBuscar', function () {
                    setTimeout(function () {
                        focusCodigo();
                    }, 0);
                });
        }

        function bindDomEvents() {
            $('#btnAgregarManual').off('click').on('click', function (e) {
                e.preventDefault();
                openSearchModal();
            });

            bindLiveSearch();
            bindSearchModalFocus();
        }

        const api = {
            init: function () {
                bindDomEvents();
            },
            getProductoSeleccionado: function () {
                return productoSeleccionado;
            },
            setProductoSeleccionado: function (value) {
                productoSeleccionado = value || null;
                return productoSeleccionado;
            },
            getPrecioActual: function () {
                return precioActual;
            },
            getTypingTimer: function () {
                return typingTimer;
            },
            setEnterDesdeTecladoVirtual: function (value) {
                enterDesdeTecladoVirtual = Boolean(value);
            },
            focusCodigo: focusCodigo,
            focusCantidad: focusCantidad,
            showWaiting: showWaiting,
            showSearching: showSearching,
            showNoMatch: showNoMatch,
            showProduct: showProduct,
            handleEnter: handleEnter,
            finishTyping: finishTyping,
            openSearchModal: openSearchModal,
            normalizeInput: normalizeInput,
            parseFloatAR: parseFloatAR,
            parseCantidadXCodigo: parseCantidadXCodigo
        };

        // Wrappers globales para mantener compatibilidad con el codigo que todavia
        // sigue dentro de la vista o en scripts heredados.
        window.abrirBuscadorProductosPOS = openSearchModal;
        window.manejarEnter = handleEnter;
        window.terminarEscritura = finishTyping;
        window.normalizarEntrada = normalizeInput;
        window.parseFloatAR = parseFloatAR;
        window.parseCantidadXCodigo = parseCantidadXCodigo;
        window.mostrarProducto = showProduct;
        window.mostrarSinCoincidencia = showNoMatch;
        window.mostrarEsperando = showWaiting;
        window.mostrarBuscando = showSearching;

        return api;
    }

    window.POSProduct = {
        create: createPOSProduct
    };
})(window, window.jQuery);
