// Tests de pos-multi-instance.js en la app instalada (una sola ventana): pasar entre POS original y duplicado navega la misma
// ventana y debe marcar window.PosConservarOperadorAlSalir, para que el pagehide de POS.cshtml no mande CerrarOperadorPOS y borre el
// operario de la pestana que se deja (bug real en SM, 2026-11-20: con la cuenta de produccion pedia identificarse en cada cambio).
// El duplicado que se descarta despues de una venta, en cambio, SI suelta su operario (no se marca).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// Lo que NO cubre: el pagehide real del navegador ni el servidor (eso se probo a mano contra un WebCore local, ver la bitacora).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const MULTI = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'pos-multi-instance.js'), 'utf8');
const ORIGEN = 'https://sm.ejemplo.local';

// Storage en memoria (localStorage y sessionStorage de una ventana).
function storage(inicial = {}) {
    const m = new Map(Object.entries(inicial));
    return { getItem: (k) => (m.has(k) ? m.get(k) : null), setItem: (k, v) => m.set(k, String(v)), removeItem: (k) => m.delete(k), _m: m };
}

// Carga el modulo real con la ventana en modo standalone (app instalada). rol: 'original' | 'duplicado'.
function crearPos({ rol, instanceId, sesionPwa, standalone = true, url }) {
    const session = storage();
    const pwaKey = 'carnisys-pos-pwa-session-v1-7-2';
    if (sesionPwa) session.setItem(pwaKey, JSON.stringify(sesionPwa));
    const href = url || `${ORIGEN}/Ventas/POS?modoPos=${rol}&posInstanceId=${instanceId}`;
    const u = new URL(href);
    const sandbox = {
        console, Date, JSON, Math, URL,
        localStorage: storage(),
        sessionStorage: session,
        location: { href, origin: u.origin, pathname: u.pathname, search: u.search, hash: u.hash, replace(x) { this.href = x; } },
        history: { replaceState() {} },
        document: { title: '', getElementById: () => null },
        matchMedia: (q) => ({ matches: standalone && String(q).includes('display-mode: standalone') }),
        navigator: {},
        addEventListener() {},
        setTimeout() {}, clearInterval() {}, setInterval() {},
        POSMultiInstanceConfig: { role: rol, instanceId, userId: 7, sucursalId: 2, duplicateBaseUrl: '/Ventas/POS', homeUrl: '/' }
    };
    sandbox.window = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(MULTI, sandbox);
    return { w: sandbox, session, pwaKey };
}

test('app instalada: ir del duplicado al original conserva el operario de la pestana que se deja', () => {
    const { w } = crearPos({ rol: 'duplicado', instanceId: 'dup-1', sesionPwa: { originalId: 'orig-1', duplicateId: 'dup-1' } });
    assert.equal(w.POSMultiInstance.focusOriginal(), true);
    assert.equal(w.PosConservarOperadorAlSalir, true);
    assert.match(w.location.href, /modoPos=original&posInstanceId=orig-1/);
});

test('app instalada: ir del original al duplicado conserva el operario de la pestana que se deja', () => {
    const { w } = crearPos({ rol: 'original', instanceId: 'orig-1', sesionPwa: { originalId: 'orig-1', duplicateId: 'dup-1' } });
    assert.equal(w.POSMultiInstance.focusDuplicado(), true);
    assert.equal(w.PosConservarOperadorAlSalir, true);
    assert.match(w.location.href, /modoPos=duplicado&posInstanceId=dup-1/);
});

test('app instalada: el duplicado que se descarta tras la venta NO conserva su operario', () => {
    const { w, session, pwaKey } = crearPos({ rol: 'duplicado', instanceId: 'dup-1', sesionPwa: { originalId: 'orig-1', duplicateId: 'dup-1' } });
    session.setItem('carnisys-pos-postventa-redirect-v1-dup-1', '1');
    w.POSMultiInstance.init();
    assert.equal(w.PosConservarOperadorAlSalir, undefined);
    assert.match(w.location.href, /modoPos=original&posInstanceId=orig-1/);
    assert.equal(JSON.parse(session.getItem(pwaKey)).duplicateId, '');
});

test('navegador comun: pasar de POS no navega ni marca nada (son dos ventanas)', () => {
    const { w } = crearPos({ rol: 'duplicado', instanceId: 'dup-1', standalone: false });
    w.POSMultiInstance.focusOriginal();
    assert.equal(w.PosConservarOperadorAlSalir, undefined);
    assert.match(w.location.href, /modoPos=duplicado/);
});
