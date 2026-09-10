// Port literal de Web/Scripts/app/pos-forma-pago-precios.js (2026-09-09, Batch C, ver
// docs/DECISIONS.md "Batch C: POS forma de pago") -- nunca se habia portado a WebCore, y sin el,
// forma-pago.js normalizaba el tipo de pago sin este helper (normalizarTipoFormaPago cae a
// "return tipo || ''" sin normalizar), asi que el atajo "5" (mapeado a "Qr") nunca matcheaba
// contra el boton real (data-tipo="QR", distinto en mayusculas) -- getBotonFormaPago() no
// encontraba nada y el atajo no hacia nada. Ademas provee el recalculo de precio por forma de
// pago que usan pos-cart.js/pos-product.js (precio distinto segun forma de pago electronica).
(function (window) {
    'use strict';

    function toNumber(value) {
        if (value == null) return 0;

        let text = String(value).trim();
        text = text.replace(/[^0-9,.\-]/g, '');

        const hasComma = text.indexOf(',') >= 0;
        const hasDot = text.indexOf('.') >= 0;

        if (hasComma && hasDot) {
            text = text.replace(/\./g, '').replace(',', '.');
        } else if (hasComma) {
            text = text.replace(',', '.');
        }

        const parsed = parseFloat(text);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    function roundPrice(value) {
        return Math.round((toNumber(value) + Number.EPSILON) * 100) / 100;
    }

    function normalizeFormaPago(tipo) {
        const raw = String(tipo || '').trim().toLowerCase();

        switch (raw) {
            case 'efectivo':
                return 'Efectivo';
            case 'debito':
                return 'Debito';
            case 'credito':
                return 'Credito';
            case 'ctacte':
            case 'cta.cte':
            case 'cta cte':
                return 'CtaCte';
            case 'qr':
            case 'q.r.':
                return 'Qr';
            case 'transferencia':
            case 'tranf':
                return 'Transferencia';
            default:
                return tipo || '';
        }
    }

    function getConfig(config) {
        return config || window.POSFormaPagoConfig || {};
    }

    function obtenerFactorFormaPago(tipo, config) {
        const normalized = normalizeFormaPago(tipo);
        const values = getConfig(config);
        const raw = values[normalized];
        const factor = Number(raw);
        return Number.isFinite(factor) && factor > 0 ? factor : 1;
    }

    function calcularPrecioFormaPago(precioBase, tipo, config) {
        const factor = obtenerFactorFormaPago(tipo, config);
        return roundPrice(toNumber(precioBase) * factor);
    }

    function tieneBonificacion(linea) {
        return Math.abs(toNumber(linea && linea.bonificacion)) > 0;
    }

    function puedeRecalcularLinea(linea) {
        if (!linea) return false;
        if (linea.anulado === true || linea.anulado === 1 || linea.anulado === '1' || linea.anulado === 'true') return false;
        if (linea.esHistorica === true || linea.esHistorica === 1 || linea.esHistorica === '1' || linea.esHistorica === 'true') return false;
        if (tieneBonificacion(linea)) return false;
        return true;
    }

    function recalcularLineaSegunFormaPago(linea, tipo, config) {
        if (!puedeRecalcularLinea(linea)) return false;

        const precioBase = toNumber(linea.precioOriginal || linea.precioLista || linea.precio);
        const cantidad = toNumber(linea.cant);
        if (precioBase <= 0 || cantidad <= 0) return false;

        const precioCalculado = calcularPrecioFormaPago(precioBase, tipo, config);
        linea.precio = '$ ' + precioCalculado.toFixed(2);
        linea.subtotal = '$ ' + (precioCalculado * cantidad).toFixed(2);
        linea.formaPagoAplicada = normalizeFormaPago(tipo);
        return true;
    }

    function recalcularCarritoSegunFormaPago(lineas, tipo, config) {
        if (!Array.isArray(lineas)) return 0;

        let actualizadas = 0;
        lineas.forEach(function (linea) {
            if (recalcularLineaSegunFormaPago(linea, tipo, config)) {
                actualizadas++;
            }
        });

        return actualizadas;
    }

    window.POSFormaPagoPrecios = {
        normalizeFormaPago: normalizeFormaPago,
        obtenerFactorFormaPago: obtenerFactorFormaPago,
        calcularPrecioFormaPago: calcularPrecioFormaPago,
        puedeRecalcularLinea: puedeRecalcularLinea,
        recalcularLineaSegunFormaPago: recalcularLineaSegunFormaPago,
        recalcularCarritoSegunFormaPago: recalcularCarritoSegunFormaPago,
        toNumber: toNumber
    };
})(window);
