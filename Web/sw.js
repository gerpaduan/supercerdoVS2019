const CACHE_NAME = "carnisys-pos-v4";

const CORE = [
    "./",
    "./manifest.json",
    "./Content/img/pwa-192.png",
    "./Content/img/pwa-512.png",
    "./Content/img/pwa-512-maskable.png"
];

// Install: cache básico
self.addEventListener("install", (event) => {
    event.waitUntil(caches.open(CACHE_NAME).then((cache) => cache.addAll(CORE)));
    self.skipWaiting();
});

// Activate: limpiar caches viejos
self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.map(k => (k !== CACHE_NAME ? caches.delete(k) : null)))
        )
    );
    self.clients.claim();
});

// Fetch: cache-first para estáticos; network para lo demás
self.addEventListener("fetch", (event) => {
    const req = event.request;

    if (req.method !== "GET") return;

    const url = new URL(req.url);

    // No cachear endpoints típicos de tu app (ajustá si querés)
    // Ej: acciones que devuelven JSON, listar productos, finalizar venta, etc.
    // BUG real encontrado: /Stock/ no estaba en esta lista -- el service worker servia una
    // copia cacheada de /Stock/Index (cache-first) en vez de pedirla de nuevo al servidor, asi
    // que el cartel de "guardado con exito" (que viaja via TempData en la respuesta de ESE
    // request puntual, nunca dos veces igual) nunca se llegaba a ver -- se mostraba la version
    // vieja, cacheada de una visita anterior sin ese aviso. Mismo riesgo (no solo el cartel --
    // datos de negocio desactualizados) se confirmo en Movimientos/Elaborados/Compras/Finanzas,
    // se agregaron los 4 por el mismo motivo. Ver docs/DECISIONS.md.
    if (url.pathname.includes("/Ventas/") ||
        url.pathname.includes("/Productos/") ||
        url.pathname.includes("/Personas/") ||
        url.pathname.includes("/Stock/") ||
        url.pathname.includes("/Movimientos/") ||
        url.pathname.includes("/Elaborados/") ||
        url.pathname.includes("/Compras/") ||
        url.pathname.includes("/Finanzas/") ||
        url.pathname.includes("/Scripts/app/pos-") ||
        url.pathname.includes("/Scripts/app/ventas-expendios-pos.js") ||
        url.pathname.includes("/Scripts/app/punto-expendio-pos.js")) {
        return; // deja pasar a red
    }

    event.respondWith(
        caches.match(req).then((cached) => {
            if (cached) return cached;

            return fetch(req).then((resp) => {
                const copy = resp.clone();
                caches.open(CACHE_NAME).then((cache) => cache.put(req, copy));
                return resp;
            });
        })
    );
});

self.addEventListener('message', (event) => {
    if (event.data.type === 'GET_LAST_URL') {
        // Leer la última URL desde localStorage (guardada por la app)
        event.waitUntil((async () => {
            const allClients = await clients.matchAll({ type: 'window' });
            for (const client of allClients) {
                client.postMessage({
                    type: 'LAST_URL',
                    url: localStorage.getItem('lastVisitedURL') || '/'
                });
            }
        })());
    }
});
