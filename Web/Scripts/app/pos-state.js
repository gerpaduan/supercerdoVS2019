(function (window) {
    'use strict';

    const state = {
        lineasVenta: [],
        fechaVenta: new Date(),
        lineaSeleccionada: null,
        nextIndex: 1,
        observaciones: '',
        listaExpendios: []
    };

    function normalizarLinea(linea) {
        if (!linea) return linea;

        const index = parseInt(linea.index, 10);
        if (Number.isFinite(index)) linea.index = index;

        linea.anulado = linea.anulado === true || linea.anulado === 1 || linea.anulado === "1" || linea.anulado === "true";

        const indexAnulado = parseInt(linea.indexAnulado, 10);
        linea.indexAnulado = Number.isFinite(indexAnulado) ? indexAnulado : -1;

        return linea;
    }

    function normalizarLineas(lineas) {
        return Array.isArray(lineas) ? lineas.map(normalizarLinea) : [];
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
            const lineaNormalizada = normalizarLinea(linea);
            state.lineasVenta.push(lineaNormalizada);
            return lineaNormalizada;
        },

        removeLinea: function (index) {
            const indexBuscado = parseInt(index, 10);
            const posicion = state.lineasVenta.findIndex(function (linea) {
                return linea && parseInt(linea.index, 10) === indexBuscado;
            });

            if (posicion < 0)
                return false;

            state.lineasVenta.splice(posicion, 1);

            if (state.lineaSeleccionada && parseInt(state.lineaSeleccionada.index, 10) === indexBuscado) {
                state.lineaSeleccionada = null;
            }

            return true;
        },

        clear: function () {
            state.lineasVenta = [];
            state.lineaSeleccionada = null;
            state.nextIndex = 1;
            state.observaciones = '';
            state.listaExpendios = [];
        },

        nextIndex: function () {
            return state.nextIndex++;
        },

        findLineaByIndex: function (index) {
            const indexBuscado = parseInt(index, 10);
            return state.lineasVenta.find(function (linea) {
                return linea && parseInt(linea.index, 10) === indexBuscado;
            }) || null;
        },

        anularLinea: function (index) {
            const linea = api.findLineaByIndex(index);
            if (!linea) return false;

            linea.anulado = true;
            linea.indexAnulado = Number.isFinite(parseInt(linea.indexAnulado, 10)) ? parseInt(linea.indexAnulado, 10) : -1;
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
        },

        setListaExpendios: function (lista) {
            state.listaExpendios = Array.isArray(lista)
                ? lista.map(function (id) { return parseInt(id, 10) || 0; }).filter(function (id) { return id > 0; })
                : [];
            state.listaExpendios = Array.from(new Set(state.listaExpendios));
            return state.listaExpendios;
        },

        getListaExpendios: function () {
            return state.listaExpendios;
        },

        addExpendio: function (idExpendio) {
            const id = parseInt(idExpendio, 10) || 0;
            if (id <= 0) return false;
            if (state.listaExpendios.indexOf(id) >= 0) return false;
            state.listaExpendios.push(id);
            return true;
        },

        removeExpendio: function (idExpendio) {
            const id = parseInt(idExpendio, 10) || 0;
            const index = state.listaExpendios.indexOf(id);
            if (index < 0) return false;
            state.listaExpendios.splice(index, 1);
            return true;
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

    Object.defineProperty(window, 'listaExpendiosVenta', {
        configurable: true,
        enumerable: true,
        get: function () {
            return state.listaExpendios;
        },
        set: function (value) {
            api.setListaExpendios(value);
        }
    });

    window.POSState = api;
})(window);
