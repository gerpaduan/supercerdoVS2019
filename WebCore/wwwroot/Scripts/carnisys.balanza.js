(function (window) {
    'use strict';

    var defaults = {
        baseUrl: 'http://127.0.0.1:5100',
        statusIntervalMs: 3000,
        pesoIntervalMs: 250,
        timeoutMs: 1200,
        onPeso: null,
        onStatus: null,
        onError: null
    };

    var state = {
        options: null,
        active: false,
        started: false,
        statusTimer: null,
        pesoTimer: null,
        lastStatus: null,
        lastPeso: null,
        available: false
    };

    function normalizeOptions(options) {
        var next = {};
        var source = options || {};
        Object.keys(defaults).forEach(function (key) {
            next[key] = source[key] != null ? source[key] : defaults[key];
        });

        next.baseUrl = String(next.baseUrl || defaults.baseUrl).replace(/\/+$/, '');
        next.statusIntervalMs = Math.max(1000, parseInt(next.statusIntervalMs, 10) || defaults.statusIntervalMs);
        next.pesoIntervalMs = Math.max(120, parseInt(next.pesoIntervalMs, 10) || defaults.pesoIntervalMs);
        next.timeoutMs = Math.max(300, parseInt(next.timeoutMs, 10) || defaults.timeoutMs);
        return next;
    }

    function clearTimer(id) {
        if (id) {
            window.clearInterval(id);
        }
        return null;
    }

    function toNumber(value) {
        if (value === null || value === undefined || value === '') {
            return 0;
        }

        if (typeof value === 'number') {
            return isNaN(value) ? 0 : value;
        }

        var text = String(value).trim();
        if (!text) {
            return 0;
        }

        text = text.replace(/[^0-9,.\-]/g, '');
        if (!text) {
            return 0;
        }

        var firstSep = text.search(/[.,]/);
        if (firstSep >= 0) {
            var intPart = text.slice(0, firstSep).replace(/[.,]/g, '');
            var decPart = text.slice(firstSep + 1).replace(/[.,]/g, '');
            text = intPart + '.' + decPart;
        } else {
            text = text.replace(/[.,]/g, '');
        }

        var number = parseFloat(text);
        return isNaN(number) ? 0 : number;
    }

    function formatPeso(value) {
        var number = toNumber(value);
        return number.toFixed(3);
    }

    function normalizePayload(data) {
        var payload = data || {};
        var pesoRaw = payload.peso;
        if (pesoRaw === undefined || pesoRaw === null) pesoRaw = payload.valor;
        if (pesoRaw === undefined || pesoRaw === null) pesoRaw = payload.weight;
        if (pesoRaw === undefined || pesoRaw === null) pesoRaw = payload.pesoTexto;
        if (pesoRaw === undefined || pesoRaw === null) pesoRaw = payload.pesoDisplay;

        var peso = toNumber(pesoRaw);
        var conectada = payload.conectada === true || payload.connected === true;
        var disponible = payload.disponible === true || payload.available === true;
        var ok = payload.ok === true || payload.estado === 'ok' || payload.status === 'ok';
        var inestable = payload.inestable === true || payload.unstable === true;
        var estable = payload.estable === true;
        var hayPesoEnPayload =
            payload.peso !== undefined && payload.peso !== null ||
            payload.valor !== undefined && payload.valor !== null ||
            payload.weight !== undefined && payload.weight !== null ||
            payload.pesoTexto !== undefined && payload.pesoTexto !== null && String(payload.pesoTexto).trim() !== '' ||
            payload.pesoDisplay !== undefined && payload.pesoDisplay !== null && String(payload.pesoDisplay).trim() !== '';
        var tienePesoValido = !!(hayPesoEnPayload && /[0-9]/.test(String(pesoRaw == null ? '' : pesoRaw)));
        var pesoTexto = '';
        var pesoDisplay = '';

        if (tienePesoValido) {
            pesoTexto = payload.pesoTexto || formatPeso(peso);
            pesoDisplay = payload.pesoDisplay || pesoTexto;
        } else if (conectada) {
            pesoTexto = 'Error lectura';
            pesoDisplay = 'Error lectura';
        }

        if (inestable && pesoDisplay.indexOf(' i') < 0) {
            pesoDisplay += ' i';
        }

        return {
            ok: ok,
            disponible: disponible || ok,
            conectada: conectada,
            peso: peso,
            pesoTexto: pesoTexto,
            pesoDisplay: pesoDisplay,
            tienePesoValido: tienePesoValido,
            estable: estable,
            inestable: inestable,
            puerto: payload.puerto || payload.port || '',
            error: payload.error || null,
            raw: payload
        };
    }

    function renderStatus(statusSelector, barSelector, data) {
        if (!statusSelector || !barSelector || !window.jQuery) {
            return;
        }

        var normalized = normalizePayload(data);
        var text = 'Desconocida';
        var percent = 0;
        var css = 'bg-secondary';

        if (!data || normalized.ok !== true) {
            text = 'Agente no detectado';
            percent = 10;
            css = 'bg-danger';
        } else if (normalized.conectada) {
            text = 'Conectada' + (normalized.puerto ? ' (' + normalized.puerto + ')' : '');
            percent = normalized.inestable ? 70 : 100;
            css = normalized.inestable ? 'bg-warning' : 'bg-success';
        } else {
            text = 'Balanza desconectada';
            percent = 35;
            css = 'bg-warning';
        }

        window.jQuery(statusSelector).text(text);
        window.jQuery(barSelector)
            .removeClass('bg-success bg-warning bg-danger bg-secondary')
            .addClass(css)
            .css('width', percent + '%');
    }

    function requestJson(method, url, payload, timeoutMs) {
        return new Promise(function (resolve, reject) {
            var xhr = new XMLHttpRequest();
            xhr.open(method, url, true);
            xhr.timeout = timeoutMs;
            xhr.setRequestHeader('Accept', 'application/json');
            xhr.setRequestHeader('Content-Type', 'application/json; charset=utf-8');

            xhr.onreadystatechange = function () {
                if (xhr.readyState !== 4) return;

                if (xhr.status >= 200 && xhr.status < 300) {
                    try {
                        resolve(xhr.responseText ? JSON.parse(xhr.responseText) : null);
                    } catch (err) {
                        reject(err);
                    }
                    return;
                }

                reject(new Error('HTTP ' + xhr.status));
            };

            xhr.ontimeout = function () {
                reject(new Error('timeout'));
            };

            xhr.onerror = function () {
                reject(new Error('network'));
            };

            xhr.send(payload ? JSON.stringify(payload) : null);
        });
    }

    function safeCallback(name, payload) {
        var fn = state.options && state.options[name];
        if (typeof fn === 'function') {
            fn(payload);
        }
    }

    function setUnavailable(error) {
        state.available = false;
        state.lastStatus = {
            ok: false,
            conectada: false,
            disponible: false,
            error: error ? error.message || String(error) : 'unavailable'
        };
        safeCallback('onStatus', state.lastStatus);
        safeCallback('onError', error || new Error('unavailable'));
    }

    function pollStatus() {
        if (!state.started || !state.options) {
            return Promise.resolve(null);
        }

        return requestJson('GET', state.options.baseUrl + '/status', null, state.options.timeoutMs)
            .then(function (data) {
                state.available = !!(data && data.ok);
                state.lastStatus = data || null;
                safeCallback('onStatus', data);
                return data;
            })
            .catch(function (err) {
                setUnavailable(err);
                throw err;
            });
    }

    function pollPeso(force) {
        if (!state.started || !state.options) {
            return Promise.resolve(null);
        }

        if (!force && !state.active) {
            return Promise.resolve(null);
        }

        return requestJson('GET', state.options.baseUrl + '/peso', null, state.options.timeoutMs)
            .then(function (data) {
                state.lastPeso = data || null;
                state.available = !!(state.lastStatus && state.lastStatus.ok);
                safeCallback('onPeso', data);
                return data;
            })
            .catch(function (err) {
                safeCallback('onError', err);
                throw err;
            });
    }

    function startTimers() {
        state.statusTimer = clearTimer(state.statusTimer);
        state.pesoTimer = clearTimer(state.pesoTimer);

        state.statusTimer = window.setInterval(function () {
            pollStatus().catch(function () { });
        }, state.options.statusIntervalMs);

        state.pesoTimer = window.setInterval(function () {
            if (!state.active) return;
            pollPeso().catch(function () { });
        }, state.options.pesoIntervalMs);
    }

    var api = {
        start: function (options) {
            state.options = normalizeOptions(options);
            state.started = true;
            startTimers();
            pollStatus().catch(function () { });
            if (state.active) {
                pollPeso().catch(function () { });
            }
            return api;
        },
        stop: function () {
            state.started = false;
            state.active = false;
            state.statusTimer = clearTimer(state.statusTimer);
            state.pesoTimer = clearTimer(state.pesoTimer);
        },
        leerAhora: function () {
            return pollPeso(true).catch(function () {
                return pollStatus();
            });
        },
        ultimo: function () {
            return state.lastPeso || state.lastStatus;
        },
        activar: function () {
            state.active = true;
            if (state.started) {
                pollPeso().catch(function () { });
            }
        },
        desactivar: function () {
            state.active = false;
        },
        estaDisponible: function () {
            return !!state.available;
        },
        ultimoStatus: function () {
            return state.lastStatus;
        }
    };

    window.CarnisysBalanza = api;
    window.CarnisysBalanzaUtils = {
        normalize: normalizePayload,
        renderStatus: renderStatus,
        toNumber: toNumber,
        formatPeso: formatPeso
    };
})(window);
