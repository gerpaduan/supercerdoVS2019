(function (window, $) {
    'use strict';

    function createPOSKeyboard(options) {
        // Este modulo concentra dos responsabilidades que antes estaban
        // mezcladas dentro de la vista:
        // 1. Saber cual es el input "activo" del POS.
        // 2. Traducir clics del teclado virtual en acciones reales.
        let inputActivo = null;

        // Lleva el foco al input de codigo y selecciona su contenido para que
        // el cajero pueda seguir tipeando sin pasos extra.
        function focusCodigo() {
            const input = document.getElementById('inputCodigo');
            if (!input) return;

            inputActivo = input;
            input.focus();
            try { input.select(); } catch { }
        }

        // Lleva el foco a cantidad para acelerar el flujo cuando ya hay un
        // producto cargado y solo falta confirmar cuanta cantidad agregar.
        function focusCantidad() {
            const input = document.getElementById('inputCantidad');
            if (!input) return;

            inputActivo = input;
            input.focus();
            try { input.select(); } catch { }
        }

        function getInputActivo() {
            return inputActivo;
        }

        function setInputActivo(input) {
            inputActivo = input || null;
            return inputActivo;
        }

        function clearInputActivo() {
            inputActivo = null;
        }

        // Escribe un caracter en el input activo. Si estamos en cantidad,
        // recalcula subtotal. Si estamos en codigo, dispara una nueva busqueda.
        function writeCharacter(value) {
            if (!inputActivo) return;

            if (inputActivo.id === 'inputCantidad') {
                if (!/^[0-9.]$/.test(value)) return;
                if (value === '.' && inputActivo.value.includes('.')) return;
                if (value === '.' && inputActivo.value === '') inputActivo.value = '0';
            }

            inputActivo.value += value;

            if (inputActivo.id === 'inputCantidad') {
                options.calculateSubtotal();
            } else {
                options.finishTyping(inputActivo.value);
            }
        }

        // Inserta el separador X solo cuando estamos escribiendo codigo.
        function writeX() {
            if (!inputActivo || inputActivo.id !== 'inputCodigo') return;
            if (inputActivo.value.includes('X')) return;
            inputActivo.value += 'X';
        }

        // Inserta la letra G para soportar los codigos especiales heredados.
        function writeG() {
            if (!inputActivo || inputActivo.id !== 'inputCodigo') return;
            if (inputActivo.value.includes('G')) return;
            inputActivo.value += 'G';
        }

        // Borra un caracter y actualiza el estado derivado del producto.
        function backspace() {
            if (!inputActivo) return;
            inputActivo.value = inputActivo.value.slice(0, -1);

            if (inputActivo.id === 'inputCantidad') {
                options.calculateSubtotal();
            } else {
                options.finishTyping(inputActivo.value);
            }
        }

        // Cuando cambia el foco en el DOM, solo aceptamos los inputs centrales
        // del POS. Asi evitamos que el teclado virtual escriba en modales.
        function bindFocusTracking() {
            document.addEventListener('focusin', function (e) {
                const target = e.target;
                if (!target || target.tagName !== 'INPUT') return;

                if (target.id === 'inputCodigo' || target.id === 'inputCantidad') {
                    inputActivo = target;
                }
            });
        }

        // Conecta cada boton del teclado virtual con su accion correspondiente.
        function bindVirtualKeyboard() {
            document.querySelectorAll('.btn-key').forEach(function (button) {
                button.addEventListener('mousedown', function (e) {
                    e.preventDefault();
                });

                button.addEventListener('click', function () {
                    const key = (button.getAttribute('data-key') || button.innerText || '').trim();
                    if (!inputActivo) return;

                    if (key === 'BACKSPACE') {
                        backspace();
                        return;
                    }

                    if (key === 'MULTIPLY' || key === 'X') {
                        writeX();
                        return;
                    }

                    switch (key) {
                        case '←':
                            backspace();
                            break;
                        case '*':
                            if (typeof options.onManualModeRequested === 'function') {
                                options.onManualModeRequested(inputActivo ? inputActivo.id : '');
                            }
                            break;
                        case 'Enter':
                            options.setEnterDesdeTecladoVirtual(true);
                            options.handleEnter();
                            break;
                        case '×':
                            writeX();
                            break;
                        case 'g':
                        case 'G':
                            writeG();
                            break;
                        default:
                            writeCharacter(key);
                            break;
                    }
                });
            });
        }

        // Al cerrar el modal de cliente o cuando otro modulo lo pida,
        // devolvemos el foco al codigo para continuar vendiendo.
        function bindPOSFocusEvents() {
            $(document).on('pos:foco-codigo', function () {
                focusCodigo();
            });
        }

        const api = {
            init: function () {
                bindFocusTracking();
                bindVirtualKeyboard();
                bindPOSFocusEvents();
                focusCodigo();
            },
            focusCodigo: focusCodigo,
            focusCantidad: focusCantidad,
            getInputActivo: getInputActivo,
            setInputActivo: setInputActivo,
            clearInputActivo: clearInputActivo,
            writeCharacter: writeCharacter,
            writeX: writeX,
            writeG: writeG,
            backspace: backspace
        };

        // Compatibilidad con codigo legado que todavia vive dentro de la vista.
        window.escribirCaracter = writeCharacter;
        window.escribirX = writeX;
        window.escribirG = writeG;
        window.borrarCaracter = backspace;

        return api;
    }

    window.POSKeyboard = {
        create: createPOSKeyboard
    };
})(window, window.jQuery);
