const CACHE_NAME = "carnisys-pos-v3";

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
    if (url.pathname.includes("/Ventas/") ||
        url.pathname.includes("/Productos/") ||
        url.pathname.includes("/Personas/")) {
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
