// "Kill switch" para el service worker viejo de la clasica (Web/sw.js, PWA "carnisys-pos-v4",
// retirada de carnisys.com en el cutover 2026-09-14). WebCore nunca registra un service worker
// propio -- este archivo existe SOLO para que los navegadores que todavia tienen el SW viejo
// registrado (de antes del cutover) lo detecten como cambiado, instalen este, borren toda cache
// vieja, se autodesregistren y recarguen las pestañas abiertas -- asi quedan sin ningun SW
// controlandolas, sirviendo login/paginas siempre frescas de red. Ver docs/DECISIONS.md
// (2026-09-16, "Login desde celular fallaba, incognito funcionaba -- SW viejo cacheando /Login").
self.addEventListener("install", () => {
    self.skipWaiting();
});

self.addEventListener("activate", (event) => {
    event.waitUntil((async () => {
        const cacheKeys = await caches.keys();
        await Promise.all(cacheKeys.map((key) => caches.delete(key)));

        await self.registration.unregister();

        const clientsList = await self.clients.matchAll({ type: "window" });
        clientsList.forEach((client) => client.navigate(client.url));
    })());
});
