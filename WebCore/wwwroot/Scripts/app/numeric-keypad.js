// Teclado numerico en pantalla, generico y reusable (Elaborados hoy; pensado para
// Movimientos/Stock a futuro sin cambios). No confundir con pos-keyboard.js, que esta
// hardcodeado a los ids del POS de Ventas/Expendio.
//
// Escribe sobre el input que tenga el foco (rastreado por focusin) respetando la
// posicion del cursor/seleccion, igual que tipear a mano, y dispara los mismos eventos
// DOM (input/keydown) que ya escuchan los handlers existentes de cada pantalla -- asi
// "click en el teclado" reusa esa logica en vez de duplicarla.
(function (window, $) {
    'use strict';
    if (!window || !$) return;

    function esInputNumero(el) {
        return !!el && (el.type || '').toLowerCase() === 'number';
    }

    function obtenerSeleccion(el) {
        var valor = el.value || '';
        var inicio = typeof el.selectionStart === 'number' ? el.selectionStart : valor.length;
        var fin = typeof el.selectionEnd === 'number' ? el.selectionEnd : valor.length;
        return { valor: valor, inicio: inicio, fin: fin };
    }

    function moverCursor(el, posicion) {
        // input[type=number] no soporta setSelectionRange (tira excepcion en la mayoria
        // de los navegadores) -- ahi el cursor queda donde el navegador lo deje, igual
        // que pasaria tipeando fisico en un campo numerico (tampoco se puede controlar).
        if (el.type === 'number' || typeof el.setSelectionRange !== 'function') return;
        try {
            el.setSelectionRange(posicion, posicion);
        } catch (e) { /* input sin soporte de seleccion (ej. algunos type= especiales) */ }
    }

    function dispararInput(el) {
        el.dispatchEvent(new Event('input', { bubbles: true }));
    }

    function insertarCaracter($input, caracter) {
        var el = $input && $input.get(0);
        if (!el) return;

        // Un <input type=number> real ignora una tecla invalida al tipearla (el
        // navegador no la deja entrar); si en cambio le asignamos por JS un .value que
        // no parsea como numero valido, el navegador vacia el campo ENTERO en vez de
        // rechazar solo el caracter -- por eso se valida antes de tocar el value.
        if (esInputNumero(el) && !/[0-9.]/.test(caracter)) return;

        var sel = obtenerSeleccion(el);
        el.value = sel.valor.slice(0, sel.inicio) + caracter + sel.valor.slice(sel.fin);
        moverCursor(el, sel.inicio + caracter.length);
        dispararInput(el);
    }

    function borrarCaracter($input) {
        var el = $input && $input.get(0);
        if (!el) return;

        var sel = obtenerSeleccion(el);
        var inicio = sel.inicio;
        var fin = sel.fin;

        if (inicio === fin) {
            if (inicio === 0) return;
            inicio -= 1;
        }

        el.value = sel.valor.slice(0, inicio) + sel.valor.slice(fin);
        moverCursor(el, inicio);
        dispararInput(el);
    }

    function simularEnter($input) {
        var el = $input && $input.get(0);
        if (!el) return;

        // El Enter fisico en las pantallas que usan este teclado se maneja con handlers
        // .on('keydown', ...) propios de cada una (buscar por codigo, agregar linea,
        // mover foco), nunca con submit nativo del form. Un KeyboardEvent sintetico con
        // key:'Enter' dispara esos mismos handlers (jQuery escucha eventos nativos reales
        // para keydown), logrando el mismo comportamiento que la tecla fisica sin
        // reimplementar esa logica aca.
        el.dispatchEvent(new KeyboardEvent('keydown', {
            key: 'Enter',
            code: 'Enter',
            keyCode: 13,
            which: 13,
            bubbles: true,
            cancelable: true
        }));
    }

    function create(options) {
        var opts = options || {};
        var selectorInputsValidos = opts.selectorInputsValidos;
        var $teclado = $(opts.selectorTeclado);
        var $inputActivo = null;

        if (!selectorInputsValidos || !$teclado.length) return null;

        function esInputValido($el) {
            return !!$el && $el.length > 0 && $el.is(selectorInputsValidos);
        }

        $(document).on('focusin.numericKeypad', selectorInputsValidos, function () {
            $inputActivo = $(this);
        });

        // Evita que el click en un boton le robe el foco al input activo ANTES de poder
        // escribir en el -- mismo truco que ya usa pos-keyboard.js en el POS.
        $teclado.on('mousedown.numericKeypad', '.btn-key, .btn-enter', function (e) {
            e.preventDefault();
        });

        $teclado.on('click.numericKeypad', '.btn-key, .btn-enter', function () {
            if (!esInputValido($inputActivo)) return;

            var key = $(this).data('key');
            if (key === 'BACKSPACE') {
                borrarCaracter($inputActivo);
            } else if (key === 'ENTER') {
                simularEnter($inputActivo);
            } else {
                insertarCaracter($inputActivo, String(key));
            }
        });

        return {
            destroy: function () {
                $(document).off('.numericKeypad');
                $teclado.off('.numericKeypad');
            }
        };
    }

    window.NumericKeypad = { create: create };
})(window, window.jQuery);
