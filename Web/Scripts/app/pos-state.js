(function (window) {
    'use strict';

    const state = {
        lineasVenta: [],
        fechaVenta: new Date(),
        lineaSeleccionada: null,
        nextIndex: 1,
        observaciones: ''
    };

    function normalizarLineas(lineas) {
        return Array.isArray(lineas) ? lineas : [];
    }

    function toFloatAR(value) {
        if (value == null) return 0;

        let s = String(value).trim();
        s = s.replace(/[^0-9,.\-]/g, "");

        const hasComma = s.includes(",");
        const hasDot = s.includes(".");

        if (hasComma && hasDot) {
            s = s.replace(/\./g, "").replace(",", ".");
        } else if (hasComma) {
            s = s.replace(",", ".");
        }

        const n = parseFloat(s);
        return isNaN(n) ? 0 : n;
    }

    const api = {
        getLineas: function () {
            return state.lineasVenta;
        },

        setLineas: function (lineas) {
            state.lineasVenta = normalizarLineas(lineas);
            state.nextIndex = state.lineasVenta.reduce(function (max, linea) {
                const index = parseInt(linea && linea.index, 10) || 0;
                return Math.max(max, index + 1);
            }, 1);
            return state.lineasVenta;
        },

        addLinea: function (linea) {
            state.lineasVenta.push(linea);
            return linea;
        },

        clear: function () {
            state.lineasVenta = [];
            state.lineaSeleccionada = null;
            state.nextIndex = 1;
            state.observaciones = '';
        },

        nextIndex: function () {
            return state.nextIndex++;
        },

        findLineaByIndex: function (index) {
            return state.lineasVenta.find(function (linea) {
                return linea && linea.index === index;
            }) || null;
        },

        anularLinea: function (index) {
            const linea = api.findLineaByIndex(index);
            if (!linea) return false;

            linea.anulado = true;
            return true;
        },

        getTotal: function () {
            return state.lineasVenta.reduce(function (total, linea) {
                if (!linea || linea.anulado) return total;

                let subtotal = toFloatAR(linea.subtotal);
                if (!subtotal) {
                    subtotal = toFloatAR(linea.cant) * toFloatAR(linea.precio);
                }

                return total + subtotal;
            }, 0);
        },

        hasVentaEnCurso: function () {
            return state.lineasVenta.some(function (linea) {
                return linea && !linea.anulado;
            });
        },

        setFechaVenta: function (fecha) {
            state.fechaVenta = fecha || new Date();
            return state.fechaVenta;
        },

        getFechaVenta: function () {
            return state.fechaVenta;
        },

        setLineaSeleccionada: function (linea) {
            state.lineaSeleccionada = linea || null;
            return state.lineaSeleccionada;
        },

        getLineaSeleccionada: function () {
            return state.lineaSeleccionada;
        },

        setObservaciones: function (value) {
            state.observaciones = String(value ?? '');
            return state.observaciones;
        },

        getObservaciones: function () {
            return state.observaciones;
        }
    };

    Object.defineProperty(window, 'lineasVenta', {
        configurable: true,
        enumerable: true,
        get: function () {
            return state.lineasVenta;
        },
        set: function (value) {
            state.lineasVenta = normalizarLineas(value);
        }
    });

    Object.defineProperty(window, 'fechaVenta', {
        configurable: true,
        enumerable: true,
        get: function () {
            return state.fechaVenta;
        },
        set: function (value) {
            state.fechaVenta = value || new Date();
        }
    });

    Object.defineProperty(window, 'lineaSeleccionada', {
        configurable: true,
        enumerable: true,
        get: function () {
            return state.lineaSeleccionada;
        },
        set: function (value) {
            state.lineaSeleccionada = value || null;
        }
    });

    Object.defineProperty(window, 'observacionesVenta', {
        configurable: true,
        enumerable: true,
        get: function () {
            return state.observaciones;
        },
        set: function (value) {
            state.observaciones = String(value ?? '');
        }
    });

    window.POSState = api;
})(window);
