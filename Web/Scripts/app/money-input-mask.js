// Mascara de importe en dinero: mientras el usuario escribe se muestra siempre en formato
// es-AR ("." de miles, "," de decimales), sin importar que tecla haya usado para el decimal.
// Uso: window.MoneyInputMask.attach("#importe") al iniciar la pantalla, y
// window.MoneyInputMask.getRawValue("#importe") antes de mandar el form (sin puntos de miles,
// coma como separador decimal -- el mismo formato que ya interpreta ParseFloat en el server).
// No se aplica de forma automatica a toda la clase .solo-decimal: esa clase tambien se usa en
// campos de cantidad en kg (3 decimales), donde este mismo mask no aplicaria tal cual.
//
// Diseño: el campo se edita 100% por teclado interceptado (preventDefault en cada tecla
// relevante), nunca dejando que el navegador inserte texto nativamente. El estado real
// (enteros/decimales tipeados) vive en un objeto JS aparte, y lo que se ve en el input es
// siempre un RENDER de ese estado -- nunca se reinterpreta el string ya formateado para
// decidir donde esta el separador decimal. Es la unica forma de no confundir un punto de
// miles auto-insertado en un render anterior con un punto recien tecleado por el usuario.
(function (window, $) {
    'use strict';
    if (!window || !$) return;

    var MAX_DECIMALES = 2;
    var TECLAS_NAVEGACION = ['Tab', 'Shift', 'Control', 'Alt', 'Meta', 'ArrowLeft', 'ArrowRight',
        'ArrowUp', 'ArrowDown', 'Home', 'End', 'Enter', 'Escape', 'CapsLock'];

    // Heuristica para texto PEGADO (o seteado externamente, ej. "Pegar precio" de la
    // calculadora de billetes) con un solo punto y sin comas: si despues del punto hay
    // exactamente 3 digitos y nada mas, es separador de miles (se descarta), no decimal.
    // Con mas de un punto y sin comas, todos son de miles. Un punto con 1 o 2 digitos
    // despues (y nada mas) es un decimal "a la inglesa" y se normaliza a coma.
    function resolverAmbiguedad(texto) {
        if (texto.indexOf(',') >= 0) return texto;

        var partes = texto.split('.');
        if (partes.length === 2) {
            if (/^\d{3}$/.test(partes[1])) return partes[0] + partes[1];
            if (/^\d{1,2}$/.test(partes[1])) return partes[0] + ',' + partes[1];
        }
        if (partes.length > 2) return texto.replace(/\./g, '');
        return texto;
    }

    // Parsea un texto YA SIN AMBIGUEDAD (a lo sumo una coma decimal, nada de puntos) en
    // { enteros, decimales }. decimales queda null si no hay coma (todavia no se tipeo
    // el separador decimal).
    function extraerPartes(texto) {
        var idx = texto.indexOf(',');
        var enteros, decimales;
        if (idx >= 0) {
            enteros = texto.slice(0, idx).replace(/\D/g, '');
            decimales = texto.slice(idx + 1).replace(/\D/g, '').slice(0, MAX_DECIMALES);
        } else {
            enteros = texto.replace(/\D/g, '');
            decimales = null;
        }
        enteros = enteros.replace(/^0+(?=\d)/, '');
        return { enteros: enteros, decimales: decimales };
    }

    function render(input, estado) {
        var enterosFormateados = (estado.enteros || '0').replace(/\B(?=(\d{3})+(?!\d))/g, '.');
        input.value = estado.decimales === null ? enterosFormateados : enterosFormateados + ',' + estado.decimales;
        input.setSelectionRange(input.value.length, input.value.length);
    }

    // Re-sincroniza el estado interno a partir de lo que haya en el campo. Se usa al iniciar
    // (el server ya renderiza en cultura es-AR, ver Web.config) y cuando algo externo escribe
    // el valor por JS y dispara 'input' (ej. "Pegar precio" de la calculadora de billetes) --
    // nuestro propio manejo de teclado nunca dispara ese evento, asi que si llega, es externo.
    function resincronizar(input, estado) {
        var partes = extraerPartes(resolverAmbiguedad((input.value || '').trim()));
        estado.enteros = partes.enteros;
        estado.decimales = partes.decimales;
        render(input, estado);
    }

    function attach(selector) {
        var input = $(selector).get(0);
        if (!input) return;

        var estado = { enteros: '', decimales: null };
        resincronizar(input, estado);

        $(input).on('keydown', function (e) {
            var tecla = e.key;
            var seleccionTotal = input.value.length > 0 &&
                input.selectionStart === 0 && input.selectionEnd === input.value.length;

            if (/^[0-9]$/.test(tecla)) {
                e.preventDefault();
                if (seleccionTotal) { estado.enteros = ''; estado.decimales = null; }
                if (estado.decimales !== null) {
                    if (estado.decimales.length < MAX_DECIMALES) estado.decimales += tecla;
                } else {
                    estado.enteros = (estado.enteros === '0' ? '' : estado.enteros) + tecla;
                }
                render(input, estado);
                return;
            }

            if (tecla === '.' || tecla === ',') {
                e.preventDefault();
                if (seleccionTotal) { estado.enteros = ''; estado.decimales = null; }
                if (estado.decimales === null) estado.decimales = '';
                render(input, estado);
                return;
            }

            if (tecla === 'Backspace' || tecla === 'Delete') {
                e.preventDefault();
                if (seleccionTotal) {
                    estado.enteros = '';
                    estado.decimales = null;
                } else if (estado.decimales !== null) {
                    estado.decimales = estado.decimales.length > 0 ? estado.decimales.slice(0, -1) : null;
                } else {
                    estado.enteros = estado.enteros.slice(0, -1);
                }
                render(input, estado);
                return;
            }

            if (TECLAS_NAVEGACION.indexOf(tecla) >= 0 || e.ctrlKey || e.metaKey) return;

            // Cualquier otra tecla (letras, simbolos) se bloquea: el campo es solo numerico.
            e.preventDefault();
        });

        $(input).on('paste', function (e) {
            var evt = e.originalEvent || e;
            evt.preventDefault();
            var texto = ((evt.clipboardData || window.clipboardData).getData('text') || '').trim();
            var partes = extraerPartes(resolverAmbiguedad(texto));
            estado.enteros = partes.enteros;
            estado.decimales = partes.decimales;
            render(input, estado);
        });

        $(input).on('input', function () {
            resincronizar(input, estado);
        });
    }

    // Valor "de negocio": sin puntos de miles, coma como separador decimal (o sin coma si el
    // usuario no tipeo decimales) -- listo para mandar al server tal cual espera ParseFloat.
    // El valor mostrado en el campo siempre esta bien formado por construccion (nunca queda
    // un punto de miles mal ubicado), asi que alcanza con sacarle los puntos.
    function getRawValue(selector) {
        var input = $(selector).get(0);
        if (!input) return '';
        return (input.value || '').replace(/\./g, '');
    }

    window.MoneyInputMask = { attach: attach, getRawValue: getRawValue };
})(window, window.jQuery);
