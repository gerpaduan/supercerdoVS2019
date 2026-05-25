(function (window, $) {
    'use strict';

    if (!window || !$) return;

    var DETALLE_START = '[INICIO_DETALLE_BILLETES]';
    var DETALLE_END = '[FIN_DETALLE_BILLETES]';
    var DETALLE_REGEX = /\[INICIO_DETALLE_BILLETES\][\s\S]*?\[FIN_DETALLE_BILLETES\]/g;

    function formatInputValue(total) {
        if (window.CalculadoraBilletes && typeof window.CalculadoraBilletes.formatInputValue === 'function') {
            return window.CalculadoraBilletes.formatInputValue(total);
        }

        return Number(total || 0).toFixed(2);
    }

    function buildDetalleBlock(resultado) {
        if (window.CalculadoraBilletes && typeof window.CalculadoraBilletes.buildDetalleBlock === 'function') {
            return window.CalculadoraBilletes.buildDetalleBlock(resultado);
        }

        return DETALLE_START + '\nDetalle de efectivo:\nTotal = $ ' + Number(resultado && resultado.total ? resultado.total : 0).toFixed(2) + '\n' + DETALLE_END;
    }

    function limpiarDetalleExistente(texto) {
        return String(texto || '')
            .replace(DETALLE_REGEX, '')
            .replace(/\n{3,}/g, '\n\n')
            .trim();
    }

    function mergeDetalle(textoActual, resultado) {
        if (window.CalculadoraBilletes && typeof window.CalculadoraBilletes.mergeDetalleBlock === 'function') {
            return window.CalculadoraBilletes.mergeDetalleBlock(textoActual, resultado);
        }

        var base = limpiarDetalleExistente(textoActual);
        var bloque = buildDetalleBlock(resultado);
        return base ? (base + '\n\n' + bloque) : bloque;
    }

    function setFieldValue($field, value) {
        if (!$field || !$field.length) return;

        $field.val(value);
        $field.trigger('input');
        $field.trigger('change');
        $field.trigger('keyup');
    }

    function focusField($field) {
        if (!$field || !$field.length) return;

        window.setTimeout(function () {
            $field.trigger('focus');
            if ($field.is('input[type="text"], textarea')) {
                $field.trigger('select');
            }
        }, 60);
    }

    function resolveModePago() {
        var valor = ($('#formaPago').val() || '').toString().trim().toLowerCase();
        var texto = ($('#formaPago option:selected').text() || '').toString().trim().toLowerCase();

        var esCheque = valor === 'cheque' || valor === 'eftvocheque' || valor === 'efvtocheque'
            || texto === 'cheque' || texto === 'eftvocheque' || texto === 'efvtocheque';
        var esMixto = valor === 'eftvocheque' || valor === 'efvtocheque'
            || texto === 'eftvocheque' || texto === 'efvtocheque';
        var esEfectivo = !esCheque && (valor === 'efectivo' || texto === 'efectivo');

        if (esMixto) return 'mixto';
        if (esEfectivo) return 'efectivo';
        return 'otro';
    }

    function applyPago($btn, resultado) {
        var selector = (($btn.data('cbInput') || '').toString().split(',')[0] || '').trim();
        var $target = $(selector || '');
        focusField($target);
    }

    function openForButton(btn) {
        if (!window.CalculadoraBilletes || typeof window.CalculadoraBilletes.open !== 'function') {
            return;
        }

        var $btn = $(btn);
        if ($btn.prop('disabled')) return;

        var contexto = ($btn.data('cbContext') || '').toString().toLowerCase();

        window.CalculadoraBilletes.open({
            tituloPantalla: 'Calculadora Billetes',
            titulo: 'Detalle de billetes',
            selectorInputTotal: $btn.data('cbInput') || '',
            selectorInputDetalle: $btn.data('cbDetail') || '',
            callbackOnAceptar: function (resultado) {
                if (contexto === 'pago') {
                    applyPago($btn, resultado);
                }
            }
        });
    }

    function syncPago() {
        var modo = resolveModePago();
        var mostrarImporte = modo === 'efectivo';
        var mostrarEfectivo = modo === 'mixto';

        $('.js-calculadora-billetes-pago-importe').each(function () {
            $(this).prop('disabled', !mostrarImporte).toggleClass('d-none', !mostrarImporte);
        });

        $('.js-calculadora-billetes-pago-efectivo').each(function () {
            $(this).prop('disabled', !mostrarEfectivo).toggleClass('d-none', !mostrarEfectivo);
        });
    }

    function init() {
        $(document)
            .off('click.calculadoraBilletesTargets', '.js-calculadora-billetes-launch')
            .on('click.calculadoraBilletesTargets', '.js-calculadora-billetes-launch', function (e) {
                e.preventDefault();
                openForButton(this);
            })
            .off('change.calculadoraBilletesTargets', '#formaPago')
            .on('change.calculadoraBilletesTargets', '#formaPago', function () {
                syncPago();
            });

        syncPago();
    }

    window.CalculadoraBilletesTargets = window.CalculadoraBilletesTargets || {
        init: init,
        syncPago: syncPago
    };

    $(function () {
        init();
    });
})(window, window.jQuery);
