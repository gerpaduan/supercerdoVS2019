// Evita que se ingrese mas de un separador decimal (punto o coma) en inputs de
// cantidad/kgs tipeables (pedido 2026-09-14, ver docs/DECISIONS.md: Movimientos/nuevo,
// Stock/nuevo, edicion de Elaborados). No se engancha por la clase ".solo-decimal" ya
// existente en el proyecto a proposito: esa clase tambien la llevan Importe (Finanzas)
// y egreso-monto (Cajas), que ya tienen su propio enmascarado completo via
// money-input-mask.js -- agregar este guard ahi tambien duplicaria el manejo del mismo
// input. Se adjunta explicitamente por selector desde cada vista que lo necesita.
(function (window, $) {
    'use strict';
    if (!window || !$) return;

    function tieneSeparador(valor) {
        return /[.,]/.test(valor || '');
    }

    // Conserva el PRIMER separador (punto o coma) que aparece en el valor y quita
    // cualquier otro punto/coma posterior. Cubre los casos que un keydown no puede
    // interceptar: pegar texto, autocompletado, o un click del teclado en pantalla
    // (numeric-keypad.js dispara un evento "input" nativo tras insertar el caracter).
    function normalizarSeparadores(valor) {
        var visto = false;
        var resultado = '';
        for (var i = 0; i < valor.length; i++) {
            var c = valor[i];
            if (c === '.' || c === ',') {
                if (visto) continue;
                visto = true;
            }
            resultado += c;
        }
        return resultado;
    }

    // attach(selector): engancha ambos resguardos a los inputs que matcheen el selector.
    // Usa delegacion sobre document -- no requiere que el input ya exista en el DOM al
    // llamar attach(), ni reengancharse si la vista los recrea dinamicamente.
    function attach(selector) {
        if (!selector) return;

        $(document).on('keydown.soloDecimalGuard', selector, function (e) {
            if (e.key !== '.' && e.key !== ',') return;
            if (tieneSeparador(this.value)) {
                e.preventDefault();
            }
        });

        $(document).on('input.soloDecimalGuard', selector, function () {
            var el = this;
            var normalizado = normalizarSeparadores(el.value);
            if (normalizado === el.value) return;

            var pos = el.selectionStart;
            var eliminados = el.value.length - normalizado.length;
            el.value = normalizado;

            if (typeof pos === 'number' && typeof el.setSelectionRange === 'function') {
                try { el.setSelectionRange(pos - eliminados, pos - eliminados); } catch (ex) { /* input sin soporte de seleccion */ }
            }
        });
    }

    window.SoloDecimalGuard = { attach: attach };
})(window, window.jQuery);
