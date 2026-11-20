// Cliente HTTP del agente local de balanza (Carnisys.Balanza.Agent, http://127.0.0.1:5100), compartido por todas las pantallas
// que pesan (POS, Puntos de expendio, Elaborados, Movimientos, Stock, Home). Origen: port de Web/Scripts/carnisys.balanza.js.
// 2026-11-20: frescura de la lectura (un peso viejo NUNCA se muestra ni se guarda) -- ver docs/DECISIONS.md "Balanza: lectura vigente".
(function (window) {
    'use strict';

    // Vigencia de una lectura valida: pasado este tiempo sin una lectura nueva, el peso deja de existir para las pantallas.
    // El agente lee cada ~300 ms y la web pide cada 250 ms: una lectura sana nunca pasa de ~700 ms.
    var LECTURA_VIGENCIA_MS = 1500;
    // Margen extra para el agente viejo (sin edadMs): su fechaHora viene truncada al segundo, asi que la edad calculada puede
    // exagerar hasta 1 s. Sin este margen el campo parpadearia vacio con una balanza sana.
    var MARGEN_FECHA_SIN_MS = 1000;

    var defaults = {
        baseUrl: 'http://127.0.0.1:5100',
        statusIntervalMs: 3000,
        pesoIntervalMs: 250,
        timeoutMs: 1200,
        vigenciaMs: LECTURA_VIGENCIA_MS,
        onPeso: null,
        onStatus: null,
        onError: null
    };

    var mensajes = {
        sinLectura: 'Esperando lectura de la balanza. Verificá que el peso se actualice y volvé a intentar.',
        inestable: 'Peso inestable: esperá a que la balanza se estabilice y volvé a intentar.',
        pesoCero: 'La balanza marca cero: poné la mercadería y esperá a que se actualice el peso.'
    };

    var state = {
        options: null,
        active: false,
        started: false,
        statusTimer: null,
        pesoTimer: null,
        lastStatus: null,
        lastPeso: null,
        available: false,
        // Ultima lectura VALIDA (conectada, con peso, sin error, no congelada) + cuando llego, para calcular su vigencia.
        lastValid: null,
        // Ya se aviso a la pantalla que la ultima lectura valida vencio (se rearma con la siguiente lectura valida).
        vencimientoAvisado: false,
        // Un solo /peso del timer en vuelo: si el agente tarda, no se apilan pedidos.
        pesoEnVuelo: false,
        // Orden de las respuestas: una respuesta mas vieja que la ultima aplicada se descarta (no pisa un peso nuevo con uno viejo).
        pesoSeq: 0,
        pesoSeqAplicada: 0
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
        next.vigenciaMs = Math.max(500, parseInt(next.vigenciaMs, 10) || defaults.vigenciaMs);
        return next;
    }

    function vigenciaMs() {
        return state.options ? state.options.vigenciaMs : LECTURA_VIGENCIA_MS;
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

    // fechaHora del agente ("yyyy-MM-ddTHH:mm:ss", hora local de la misma PC) -> milisegundos. null si no se puede leer.
    function parseFechaAgente(texto) {
        var match = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2}):(\d{2})/.exec(String(texto || ''));
        if (!match) return null;
        var fecha = new Date(+match[1], +match[2] - 1, +match[3], +match[4], +match[5], +match[6]);
        var ms = fecha.getTime();
        return isNaN(ms) ? null : ms;
    }

    // Edad de la lectura en el agente (ms) al momento de recibirla, y el limite que le corresponde segun lo que mando el agente.
    // Agente nuevo: edadMs exacto. Agente viejo: se estima con fechaHora (truncada al segundo, de ahi el margen).
    // Sin ninguno de los dos no se puede saber: se toma como fresca (edad 0) para no romper un agente aun mas viejo.
    function edadEnAgente(payload, ahora) {
        if (typeof payload.edadMs === 'number' && payload.edadMs >= 0) {
            return { edadMs: payload.edadMs, limiteMs: vigenciaMs() };
        }

        var fechaMs = parseFechaAgente(payload.fechaHora);
        if (fechaMs !== null) {
            return { edadMs: Math.max(0, ahora - fechaMs), limiteMs: vigenciaMs() + MARGEN_FECHA_SIN_MS };
        }

        return { edadMs: 0, limiteMs: vigenciaMs() };
    }

    // Normaliza cualquier respuesta del agente (/peso o /status). Una lectura "vencida" (el agente devuelve siempre la misma,
    // tipicamente porque su hilo de lectura se trabo) se trata como NO conectada: nunca se muestra su peso.
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

        // Frescura: solo si el agente mando como fechar la lectura (las respuestas armadas por este cliente no traen fecha).
        var ahora = Date.now();
        var edad = (payload.edadMs !== undefined || payload.fechaHora !== undefined) ? edadEnAgente(payload, ahora) : null;
        var vencida = !!(edad && conectada && edad.edadMs > edad.limiteMs);
        if (vencida) {
            conectada = false;
            tienePesoValido = false;
        }

        if (tienePesoValido) {
            pesoTexto = payload.pesoTexto || formatPeso(peso);
            pesoDisplay = payload.pesoDisplay || pesoTexto;
        } else if (vencida) {
            pesoTexto = 'Sin lectura';
            pesoDisplay = 'Sin lectura';
        } else if (conectada) {
            pesoTexto = 'Error lectura';
            pesoDisplay = 'Error lectura';
        }

        if (inestable && pesoDisplay && pesoDisplay.indexOf(' i') < 0) {
            pesoDisplay += ' i';
        }

        var error = payload.error || null;
        if (vencida && !error) error = 'Lectura congelada: la balanza no actualiza el peso.';

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
            vencida: vencida,
            // Edad de la lectura en el agente al recibirla (null si el payload no la trae) y el limite aplicable.
            edadMs: edad ? edad.edadMs : null,
            limiteMs: edad ? edad.limiteMs : vigenciaMs(),
            // Lectura utilizable como peso: conectada, con numero, sin error del agente y no congelada.
            valida: conectada && tienePesoValido && !error && payload.ok !== false,
            puerto: payload.puerto || payload.port || '',
            error: error,
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
        } else if (normalized.vencida) {
            text = 'Lectura congelada (sin peso nuevo)';
            percent = 20;
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

    // Cache-buster: aunque el agente nuevo manda Cache-Control: no-store, el viejo no manda ningun header de cache.
    function sinCache(url) {
        return url + (url.indexOf('?') >= 0 ? '&' : '?') + '_=' + Date.now();
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

        return requestJson('GET', sinCache(state.options.baseUrl + '/status'), null, state.options.timeoutMs)
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

    // Guarda la lectura si es valida (para lecturaVigente) y devuelve su normalizacion.
    function registrarLectura(data) {
        var normalized = normalizePayload(data);
        if (normalized.valida) {
            state.lastValid = {
                lectura: normalized,
                recibidaEn: Date.now(),
                edadAlRecibirMs: normalized.edadMs || 0
            };
            state.vencimientoAvisado = false;
        }
        return normalized;
    }

    // Con un /peso colgado (hasta timeoutMs) no llega ningun evento y la pantalla seguiria mostrando el ultimo peso aunque ya
    // vencio. Al vencer se avisa UNA vez como lectura sin respuesta, asi la pantalla vacia el campo en el momento.
    function avisarSiVencio() {
        if (!state.lastValid || state.vencimientoAvisado || lecturaVigente()) {
            return;
        }
        state.vencimientoAvisado = true;
        state.lastPeso = { ok: false, conectada: false, sinRespuesta: true, error: 'Sin lectura nueva de la balanza.' };
        safeCallback('onPeso', state.lastPeso);
    }

    function pollPeso(force) {
        if (!state.started || !state.options) {
            return Promise.resolve(null);
        }

        if (!force && !state.active) {
            return Promise.resolve(null);
        }

        var seq = ++state.pesoSeq;
        if (!force) state.pesoEnVuelo = true;

        return requestJson('GET', sinCache(state.options.baseUrl + '/peso'), null, state.options.timeoutMs)
            .then(function (data) {
                if (!force) state.pesoEnVuelo = false;
                // Respuesta vieja (llego despues de una mas nueva): no se aplica, pero se devuelve a quien la pidio.
                if (seq < state.pesoSeqAplicada) {
                    return data;
                }
                state.pesoSeqAplicada = seq;
                state.lastPeso = data || null;
                state.available = !!(state.lastStatus && state.lastStatus.ok);
                registrarLectura(data);
                safeCallback('onPeso', data);
                return data;
            })
            .catch(function (err) {
                if (!force) state.pesoEnVuelo = false;
                // Sin respuesta del agente: las pantallas lo reciben como lectura no conectada (asi vacian el peso cuando vence y
                // cuentan el chequeo para el corte automatico). Antes solo llegaba onError y el campo conservaba el ultimo peso.
                if (seq >= state.pesoSeqAplicada) {
                    state.pesoSeqAplicada = seq;
                    state.lastPeso = { ok: false, conectada: false, sinRespuesta: true, error: err && err.message ? err.message : 'sin respuesta' };
                    safeCallback('onPeso', state.lastPeso);
                }
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
            if (state.pesoEnVuelo) {
                avisarSiVencio();
                return;
            }
            pollPeso().catch(function () { });
        }, state.options.pesoIntervalMs);
    }

    // Ultima lectura valida si sigue vigente (edad en el agente + tiempo desde que llego <= limite); si no, null.
    // Es la UNICA fuente de peso que deben usar las pantallas para mostrar o guardar.
    function lecturaVigente(maxEdadMs) {
        var ultima = state.lastValid;
        if (!ultima) return null;
        var limite = maxEdadMs != null ? maxEdadMs : ultima.lectura.limiteMs;
        var edad = ultima.edadAlRecibirMs + (Date.now() - ultima.recibidaEn);
        return edad <= limite ? ultima.lectura : null;
    }

    // Chequeo comun antes de agregar una linea con la balanza activa: lectura vigente, estable y mayor a cero.
    // Devuelve { ok, motivo ('sinLectura' | 'inestable' | 'pesoCero'), mensaje, lectura }.
    function validarParaAgregar() {
        var lectura = lecturaVigente();
        if (!lectura) {
            return { ok: false, motivo: 'sinLectura', mensaje: mensajes.sinLectura, lectura: null };
        }
        if (lectura.inestable) {
            return { ok: false, motivo: 'inestable', mensaje: mensajes.inestable, lectura: lectura };
        }
        if (!(lectura.peso > 0)) {
            return { ok: false, motivo: 'pesoCero', mensaje: mensajes.pesoCero, lectura: lectura };
        }
        return { ok: true, motivo: '', mensaje: '', lectura: lectura };
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
        lecturaVigente: lecturaVigente,
        validarParaAgregar: validarParaAgregar,
        activar: function () {
            state.active = true;
            if (state.started && !state.pesoEnVuelo) {
                pollPeso().catch(function () { });
            }
        },
        desactivar: function () {
            state.active = false;
        },
        estaActiva: function () {
            return !!state.active;
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
        formatPeso: formatPeso,
        mensajes: mensajes
    };
})(window);
