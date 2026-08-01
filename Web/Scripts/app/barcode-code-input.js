// Lectura de codigo de barra por camara para un input de "codigo de producto".
// Es un puerto generico de la logica ya probada en Views/Productos/AddOrEdit.cshtml
// (doble lectura consecutiva + checksum EAN-8/EAN-13) para poder reusarla en otras
// vistas sin duplicar el algoritmo. No conoce nada del dominio (producto, cantidad,
// busqueda): solo detecta un codigo valido y llama a onCodigoAceptado(codigo).
(function (window, $) {
    'use strict';

    if (!$ || window.BarcodeCodeInput) {
        return;
    }

    var TIEMPO_RELECTURA = 2000;

    function onlyDigits(s) { return (s || '').toString().replace(/\D/g, ''); }

    function isValidEAN13(code13) {
        if (!/^\d{13}$/.test(code13)) return false;
        var sum = 0;
        for (var i = 0; i < 12; i++) {
            var d = parseInt(code13[i], 10);
            sum += (i % 2 === 0) ? d : d * 3;
        }
        var check = (10 - (sum % 10)) % 10;
        return check === parseInt(code13[12], 10);
    }

    function isValidEAN8(code8) {
        if (!/^\d{8}$/.test(code8)) return false;
        var sum = 0;
        for (var i = 0; i < 7; i++) {
            var d = parseInt(code8[i], 10);
            sum += (i % 2 === 0) ? d * 3 : d;
        }
        var check = (10 - (sum % 10)) % 10;
        return check === parseInt(code8[7], 10);
    }

    function isValidEAN(code) {
        code = onlyDigits(code);
        if (code.length === 13) return isValidEAN13(code);
        if (code.length === 8) return isValidEAN8(code);
        return false;
    }

    // options: { videoSelector, containerSelector, buttonSelector, closeButtonSelector,
    //            clearButtonSelector, flashButtonSelector, msgSelector, codigoInputSelector,
    //            beepSelector, onCodigoAceptado(codigo) }
    function attach(options) {
        options = options || {};

        if (typeof BarcodeScanner === 'undefined') {
            // Vista cargada sin scanner.js (ej. Stock/Editar en modo AJAX sin layout).
            // No hay camara disponible: el boton de escanear, si existe, queda inerte.
            return null;
        }

        var beepSelector = options.beepSelector || '#beep';
        var onCodigoAceptado = typeof options.onCodigoAceptado === 'function' ? options.onCodigoAceptado : function () { };

        var ultimoCodigoLeido = '';
        var pausaLecturaActiva = false;
        var pausaLecturaTimer = null;

        function beep() {
            var audio = document.querySelector(beepSelector);
            if (!audio) return;
            try {
                audio.currentTime = 0;
                audio.play();
            } catch (e) { }
        }

        function setMsg(text) {
            var el = document.querySelector(options.msgSelector);
            if (!el) return;
            if (!text) {
                el.classList.add('d-none');
                el.textContent = '';
            } else {
                el.textContent = text;
                el.classList.remove('d-none');
            }
        }

        function limpiarEstadoLectura() {
            ultimoCodigoLeido = '';
            pausaLecturaActiva = false;
            if (pausaLecturaTimer) {
                clearTimeout(pausaLecturaTimer);
                pausaLecturaTimer = null;
            }
        }

        function syncFlashButton(estado) {
            var btn = document.querySelector(options.flashButtonSelector);
            if (!btn) return;
            if (estado) {
                btn.classList.remove('btn-secondary');
                btn.classList.add('btn-warning');
                btn.textContent = 'Flash ON';
            } else {
                btn.classList.remove('btn-warning');
                btn.classList.add('btn-secondary');
                btn.textContent = 'Flash';
            }
        }

        var scanner = new BarcodeScanner({
            videoSelector: options.videoSelector,
            containerSelector: options.containerSelector,
            onCodeDetected: function (codigoLeido) {
                if (pausaLecturaActiva) return;

                var clean = onlyDigits(codigoLeido);
                if (!clean) return;

                // Exige dos lecturas consecutivas iguales antes de aceptar: una sola
                // lectura de camara puede confundir digitos, y el checksum EAN no
                // detecta el 100% de esos casos.
                if (!ultimoCodigoLeido) {
                    ultimoCodigoLeido = clean;
                    setMsg('Código detectado. Acerque nuevamente para confirmar.');
                    return;
                }

                if (clean !== ultimoCodigoLeido) {
                    ultimoCodigoLeido = clean;
                    setMsg('La segunda lectura no coincidió. Se reinició la validación.');
                    return;
                }

                if (!isValidEAN(clean)) {
                    limpiarEstadoLectura();
                    setMsg('Código inválido (solo EAN-8 o EAN-13). Volvé a enfocar.');
                    return;
                }

                var codigoEl = document.querySelector(options.codigoInputSelector);
                if (codigoEl) {
                    codigoEl.value = clean;
                    $(codigoEl).trigger('input');
                }

                pausaLecturaActiva = true;
                setMsg('Código leído correctamente.');
                beep();
                // Por default se cierra la camara tras leer (flujo de un
                // solo codigo, ej. AddOrEdit). Si el consumidor pasa
                // cerrarAlLeer:false (flujo de cargar varios productos
                // seguidos, ej. Movimientos/Stock), la camara queda abierta
                // y lista para la proxima lectura; el cooldown de mas abajo
                // sigue evitando que la misma lectura se dispare de nuevo.
                if (options.cerrarAlLeer !== false) {
                    scanner.cerrar();
                }
                pausaLecturaTimer = setTimeout(function () {
                    limpiarEstadoLectura();
                    setMsg('');
                }, TIEMPO_RELECTURA);

                onCodigoAceptado(clean);
            }
        });

        if (options.buttonSelector) {
            var btnAbrir = document.querySelector(options.buttonSelector);
            if (btnAbrir) {
                btnAbrir.addEventListener('click', function () {
                    setMsg('');
                    limpiarEstadoLectura();
                    syncFlashButton(false);
                    scanner.iniciar().catch(function () {
                        setMsg('No se pudo iniciar la cámara.');
                    });
                });
            }
        }

        if (options.closeButtonSelector) {
            var btnCerrar = document.querySelector(options.closeButtonSelector);
            if (btnCerrar) {
                btnCerrar.addEventListener('click', function () {
                    setMsg('');
                    limpiarEstadoLectura();
                    scanner.cerrar();
                    syncFlashButton(false);
                });
            }
        }

        if (options.clearButtonSelector) {
            var btnLimpiar = document.querySelector(options.clearButtonSelector);
            if (btnLimpiar) {
                btnLimpiar.addEventListener('click', function () {
                    var codigoEl = document.querySelector(options.codigoInputSelector);
                    if (codigoEl) {
                        codigoEl.value = '';
                        $(codigoEl).trigger('input');
                    }
                    setMsg('');
                    limpiarEstadoLectura();
                });
            }
        }

        if (options.flashButtonSelector) {
            var btnFlash = document.querySelector(options.flashButtonSelector);
            if (btnFlash) {
                btnFlash.addEventListener('click', function () {
                    scanner.toggleFlash().then(function (estado) {
                        syncFlashButton(estado);
                    });
                });
            }
        }

        return scanner;
    }

    window.BarcodeCodeInput = { attach: attach };
})(window, window.jQuery);
