// Guarda compartida de "producto sin agregar": impide guardar un registro (Stock, Movimientos,
// Elaborados > Carga y Fórmulas) mientras quede un código escrito/escaneado en el buscador que
// no se sumó a la tabla. Sin esto el registro se guarda sin ese producto y el usuario no se entera
// (se olvidó de presionar Enter o "+"). Mismo criterio que Compras (compras.js, submitForm).
//
// Uso: al inicio del handler de submit de cada pantalla,
//     if (window.ProductoPendienteGuard.bloqueaGuardado({ codigoSelector: '#txtCodigoProducto', destino: 'el movimiento' })) return;
// Solo mira el campo Código (visible y con texto); la Cantidad NO cuenta porque la balanza la
// completa sola aunque no haya producto. Depende de jQuery; usa SweetAlert2 y BusquedaFeedback si están.
(function (window, $) {
    'use strict';

    if (!window || !$) return;

    // Devuelve el campo Código si tiene un valor sin agregar; null si está vacío, oculto o no existe.
    function obtenerCodigoPendiente(codigoSelector) {
        var $codigo = $(codigoSelector).first();
        if (!$codigo.length || !$codigo.is(':visible')) return null;
        return $.trim($codigo.val() || '') !== '' ? $codigo : null;
    }

    function enfocar($campo) {
        // setTimeout: si el foco se pide mientras se cierra el Swal, el navegador lo pisa.
        window.setTimeout(function () {
            $campo.trigger('focus');
            if (typeof $campo.select === 'function') $campo.select();
        }, 0);
    }

    // Si hay un código sin agregar: avisa (sonido + mensaje), devuelve el foco al campo y
    // retorna true (el llamador debe cortar el guardado). Si no hay pendiente retorna false sin
    // efectos secundarios.
    function bloqueaGuardado(opciones) {
        var config = opciones || {};
        var $pendiente = obtenerCodigoPendiente(config.codigoSelector);
        if (!$pendiente) return false;

        var destino = config.destino || 'el registro';
        var mensaje = 'El campo Código tiene un valor sin agregar. Agregá el producto a ' + destino +
            ' o borrá el código; tiene que estar vacío para guardar.';

        window.BusquedaFeedback && window.BusquedaFeedback.beepError();

        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ icon: 'warning', title: 'Validacion', text: mensaje }).then(function () {
                enfocar($pendiente);
            });
        } else {
            window.alert(mensaje);
            enfocar($pendiente);
        }

        return true;
    }

    window.ProductoPendienteGuard = { bloqueaGuardado: bloqueaGuardado };
})(window, window.jQuery);
