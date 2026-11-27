/*
 * Service worker del POS sin conexion (2026-11-26, ver docs/DECISIONS.md "POS sin conexion"). Lo registra cualquier
 * pantalla con sesion (Views/Shared/_PosOffline.cshtml), con scope <base>/Ventas/POS: no controla /Login ni ninguna
 * otra pantalla. Es otro archivo que sw.js (ese es el "kill switch" del service worker viejo del Web clasico y no se
 * reutiliza, DECISIONS 2026-10-13).
 *
 * Que hace:
 *  - Al instalarse guarda en una cache versionada (?v= del registro) la pagina estatica pos-offline.html y sus archivos.
 *  - Entrada al POS (navegacion a <base>/Ventas/POS...): RED PRIMERO. Si la red falla, se pasa del tiempo o el proxy
 *    responde 502/503/504, devuelve pos-offline.html desde la cache. Con ?modo=offline la devuelve directo.
 *  - Archivos de la pagina offline: red primero, cache si falla.
 *  - Todo lo demas (pedidos del POS, imagenes, otras pantallas): no lo toca, va a la red como siempre.
 *  - Freno de emergencia (PosOffline:Habilitado=false): las pantallas lo desregistran (ver _PosOffline.cshtml).
 */
'use strict';

var VERSION = new URL(self.location.href).searchParams.get('v') || 'dev';
var PREFIJO_CACHE = 'pos-offline-';
var CACHE = PREFIJO_CACHE + VERSION;
var BASE = new URL('./', self.location.href).pathname;
var TIMEOUT_NAVEGACION_MS = 8000;
var ESTADOS_PROXY_CAIDO = [502, 503, 504];

var PAGINA_OFFLINE = BASE + 'pos-offline.html';
var ARCHIVOS = [
    'pos-offline.html',
    'Scripts/app/pos-offline-db.js',
    'Scripts/app/pos-offline-red.js',
    'Scripts/app/pos-offline-buscar.js',
    'Scripts/app/pos-offline-pin.js',
    'Scripts/app/pos-offline-vigencia.js',
    'Scripts/app/pos-offline-sync.js',
    'Scripts/app/pos-offline-ticket.js',
    'Scripts/app/pos-offline-app.js',
    'Scripts/carnisys.balanza.js',
    'lib/bootstrap/dist/css/bootstrap.min.css'
].map(function (a) { return BASE + a; });

self.addEventListener('install', function (event) {
    // cache: 'reload' para no guardar una copia vieja del cache HTTP del navegador.
    event.waitUntil(
        caches.open(CACHE)
            .then(function (cache) {
                return cache.addAll(ARCHIVOS.map(function (u) { return new Request(u, { cache: 'reload' }); }));
            })
            .then(function () { return self.skipWaiting(); })
    );
});

self.addEventListener('activate', function (event) {
    event.waitUntil(
        caches.keys()
            .then(function (nombres) {
                return Promise.all(nombres
                    .filter(function (n) { return n.indexOf(PREFIJO_CACHE) === 0 && n !== CACHE; })
                    .map(function (n) { return caches.delete(n); }));
            })
            .then(function () { return self.clients.claim(); })
    );
});

function paginaOffline() {
    return caches.match(PAGINA_OFFLINE, { cacheName: CACHE }).then(function (r) {
        return r || caches.match(PAGINA_OFFLINE);
    }).then(function (r) {
        return r || new Response('<h3>Esta PC no está preparada para vender sin conexión.</h3>', { headers: { 'Content-Type': 'text/html; charset=utf-8' } });
    });
}

function conTimeout(promesa, ms) {
    return new Promise(function (resolve, reject) {
        var t = setTimeout(function () { reject(new Error('timeout')); }, ms);
        promesa.then(function (v) { clearTimeout(t); resolve(v); }, function (e) { clearTimeout(t); reject(e); });
    });
}

self.addEventListener('fetch', function (event) {
    var req = event.request;
    if (req.method !== 'GET') return;
    var url = new URL(req.url);
    if (url.origin !== self.location.origin) return;

    var esEntradaPos = req.mode === 'navigate' && url.pathname.toLowerCase().indexOf((BASE + 'ventas/pos').toLowerCase()) === 0;
    if (esEntradaPos) {
        if (url.searchParams.get('modo') === 'offline') {
            event.respondWith(paginaOffline());
            return;
        }
        event.respondWith(
            conTimeout(fetch(req), TIMEOUT_NAVEGACION_MS)
                .then(function (resp) { return ESTADOS_PROXY_CAIDO.indexOf(resp.status) >= 0 ? paginaOffline() : resp; })
                .catch(function () { return paginaOffline(); })
        );
        return;
    }

    if (ARCHIVOS.indexOf(url.pathname) >= 0) {
        // cache: 'no-cache' = revalidar siempre con el servidor: sin esto la cache HTTP del navegador puede devolver una
        // copia vieja de un archivo junto con la nueva de otro (versiones mezcladas, visto en la prueba del 2026-11-26).
        event.respondWith(
            fetch(req.url, { cache: 'no-cache', credentials: 'same-origin' }).then(function (resp) {
                if (!resp.ok) throw new Error('HTTP ' + resp.status);
                return resp;
            }).catch(function () {
                return caches.match(url.pathname, { cacheName: CACHE }).then(function (r) { return r || caches.match(url.pathname); })
                    .then(function (r) { return r || Response.error(); });
            })
        );
    }
    // Cualquier otro pedido: sin respondWith, el navegador lo resuelve como siempre.
});
