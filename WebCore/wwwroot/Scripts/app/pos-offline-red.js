/*
 * POS sin conexion: chequeo de conexion y causa probable del corte (2026-11-26).
 *  - 'sinred':      navigator.onLine === false (cable o wifi de ESTA PC).
 *  - 'servidor':    el servidor no responde (o responde 502/503/504 del proxy) pero la URL externa de chequeo si.
 *  - 'sininternet': no responde ni el servidor ni la URL externa.
 *  - null:          no responde el servidor y no hay URL externa configurada (PosOffline:UrlChequeoInternet).
 * Es una causa PROBABLE: un firewall que bloquee la URL externa confunde 'sininternet' con 'servidor'.
 * El ping es un archivo estatico (pos-offline-ping.txt), servido sin login, asi una sesion vencida no se toma como corte.
 */
(function (root) {
    'use strict';

    var TIMEOUT_DEFAULT_MS = 4000;
    // Respuestas del proxy (Caddy/IIS) cuando la app no esta: cuentan como corte del servidor.
    var ESTADOS_PROXY_CAIDO = [502, 503, 504];

    // Ruta base de la app (con / final), sacada de la URL del POS: '/', o '/CarniSysWeb/' detras de un PathBase.
    function base() {
        var ruta = root.location ? root.location.pathname : '/';
        var i = ruta.toLowerCase().indexOf('/ventas/pos');
        return i >= 0 ? ruta.substring(0, i + 1) : '/';
    }

    function conTimeout(promesa, ms) {
        return new Promise(function (resolve, reject) {
            var t = setTimeout(function () { reject(new Error('timeout')); }, ms);
            promesa.then(function (v) { clearTimeout(t); resolve(v); }, function (e) { clearTimeout(t); reject(e); });
        });
    }

    function servidorResponde(ms) {
        return conTimeout(root.fetch(base() + 'pos-offline-ping.txt?t=' + Date.now(), { cache: 'no-store' }), ms)
            .then(function (r) { return r.ok && ESTADOS_PROXY_CAIDO.indexOf(r.status) < 0; })
            .catch(function () { return false; });
    }

    function internetResponde(url, ms) {
        if (!url) return Promise.resolve(null);
        // no-cors: la respuesta es opaca, pero que resuelva ya prueba que hay salida a internet.
        return conTimeout(root.fetch(url + (url.indexOf('?') >= 0 ? '&' : '?') + 't=' + Date.now(), { mode: 'no-cors', cache: 'no-store' }), ms)
            .then(function () { return true; })
            .catch(function () { return false; });
    }

    // Resuelve { online: bool, causa: null | 'sinred' | 'sininternet' | 'servidor' }.
    function chequear(opciones) {
        opciones = opciones || {};
        var ms = (opciones.segundosTimeout || 0) * 1000 || TIMEOUT_DEFAULT_MS;
        if (root.navigator && root.navigator.onLine === false) return Promise.resolve({ online: false, causa: 'sinred' });

        return servidorResponde(ms).then(function (ok) {
            if (ok) return { online: true, causa: null };
            return internetResponde(opciones.urlChequeoInternet, ms).then(function (hayInternet) {
                if (hayInternet === null) return { online: false, causa: null };
                return { online: false, causa: hayInternet ? 'servidor' : 'sininternet' };
            });
        });
    }

    var TEXTOS_CAUSA = {
        sinred: 'Esta PC no tiene red (revisá el cable o el wifi).',
        sininternet: 'Sin internet en el local (llamá al proveedor de internet).',
        servidor: 'El servidor de CarniSys no responde (avisá al administrador).'
    };

    function textoCausa(causa) {
        return TEXTOS_CAUSA[causa] || 'Sin conexión con el servidor.';
    }

    root.PosOfflineRed = { base: base, chequear: chequear, textoCausa: textoCausa, conTimeout: conTimeout };
})(typeof window !== 'undefined' ? window : globalThis);
