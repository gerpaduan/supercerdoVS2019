// Tests de "POS en ventana unica" (WebCore/wwwroot/Scripts/app/pos-ventana-unica.js + POSMultiInstance.openOriginalEntry).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// Cada test carga los scripts reales en un sandbox (vm) con window/document/localStorage simulados: window.open devuelve ventanas falsas, asi se
// verifica que (a) «Inicio» abre/reusa Home sin navegar el POS, (b) «Punto de Venta» trae al frente el POS ya abierto y nunca abre otro sin que
// el usuario lo pida. Lo que NO cubre: el comportamiento real del navegador al buscar ventanas por nombre (eso se prueba a mano en Chrome).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const APP = path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app');
const MULTI = fs.readFileSync(path.join(APP, 'pos-multi-instance.js'), 'utf8');
const UNICA = fs.readFileSync(path.join(APP, 'pos-ventana-unica.js'), 'utf8');

const STATE_KEY = (user, sucursal) => `carnisys-pos-multi-v1-${user}-${sucursal}`;

// Ventana falsa: href 'about:blank' = recien creada por window.open('', nombre); cualquier otra = ya tenia pagina.
function fakeWindow(href) {
    return { location: { href }, focused: 0, closed: false, focus() { this.focused++; }, close() { this.closed = true; } };
}

// Crea el entorno. opciones: ventanas = { nombre: ventanaFalsa | null }, nombreVentana, tieneMenuLateral, appInstalada.
function crearEntorno(opciones = {}) {
    const abiertas = []; // nombres pedidos a window.open, en orden
    const ventanas = opciones.ventanas || {};
    const toasts = [];
    const dialogos = [];
    const store = new Map();
    const listenersWindow = [];
    const listenersDocument = [];
    let respuestaDialogo = { dismiss: 'backdrop' };

    const sandbox = {
        console,
        Date,
        JSON,
        parseInt,
        localStorage: {
            getItem: (k) => (store.has(k) ? store.get(k) : null),
            setItem: (k, v) => store.set(k, String(v)),
            removeItem: (k) => store.delete(k)
        },
        sessionStorage: { getItem: () => null, setItem() {}, removeItem() {} },
        document: {
            getElementById: (id) => (id === 'accordionSidebar' && opciones.tieneMenuLateral ? {} : null),
            addEventListener: (tipo, fn) => listenersDocument.push({ tipo, fn })
        },
        Swal: {
            DismissReason: { cancel: 'cancel' },
            fire: (cfg) => {
                if (cfg && cfg.toast) { toasts.push(cfg.title); return Promise.resolve({}); }
                dialogos.push(cfg);
                return Promise.resolve(respuestaDialogo);
            }
        }
    };
    sandbox.window = sandbox;
    sandbox.window.top = sandbox;
    sandbox.name = opciones.nombreVentana || '';
    sandbox.matchMedia = () => ({ matches: false });
    sandbox.addEventListener = (tipo, fn) => listenersWindow.push({ tipo, fn });
    sandbox.open = (url, nombre) => {
        abiertas.push(nombre);
        if (!(nombre in ventanas)) return null; // ventana emergente bloqueada
        return ventanas[nombre];
    };
    sandbox.alert = () => { throw new Error('no deberia usar alert: hay Swal'); };
    if (opciones.appInstalada) sandbox.AppInstalada = { activa: true };

    vm.createContext(sandbox);
    vm.runInContext(MULTI, sandbox);
    vm.runInContext(UNICA, sandbox);

    return {
        sandbox, abiertas, toasts, dialogos, store,
        responderDialogo: (r) => { respuestaDialogo = r; },
        // Dispara un clic ante los listeners registrados (capturan en window / document), como el navegador.
        clic(selectorCoincide, extra = {}) {
            const elemento = selectorCoincide;
            const evento = Object.assign({
                button: 0, ctrlKey: false, metaKey: false, shiftKey: false, altKey: false, defaultPrevented: false,
                target: { closest: (sel) => (elemento && elemento.sel === sel ? elemento.el : null) },
                preventDefault() { this.defaultPrevented = true; },
                stopImmediatePropagation() { this.detenido = true; }
            }, extra);
            for (const l of listenersWindow.filter((x) => x.tipo === 'click')) { if (!evento.detenido) l.fn(evento); }
            for (const l of listenersDocument.filter((x) => x.tipo === 'click')) { if (!evento.detenido) l.fn(evento); }
            return evento;
        },
        // Registra un POS original abierto, como lo hace el latido de pos-multi-instance.js.
        registrarPos(user, sucursal, { haceMs = 0, windowName = `CarniSysPOS-${user}-${sucursal}-original` } = {}) {
            store.set(STATE_KEY(user, sucursal), JSON.stringify({
                original: { id: 'abc', ts: Date.now() - haceMs, url: '/Ventas/POS', windowName },
                duplicado: null
            }));
        }
    };
}

const botonInicio = { sel: '[data-inicio-aparte]', el: { href: 'http://localhost/Home/Index' } };
const atributos = { 'data-pos-usuario': '7', 'data-pos-sucursal': '3' };
const enlacePos = {
    sel: 'a[data-pos-unico]',
    el: { href: 'http://localhost/Ventas/POS', getAttribute: (n) => atributos[n] || null }
};

// ---------- «Inicio» desde el POS ----------
test('Inicio: sin Home abierta, abre una pestaña nueva con nombre fijo y la navega a Home (el POS no se toca)', () => {
    const home = fakeWindow('about:blank');
    const e = crearEntorno({ ventanas: { CarniSysPrincipal: home } });
    const ev = e.clic(botonInicio);
    assert.equal(ev.defaultPrevented, true, 'no debe navegar la ventana del POS');
    assert.equal(ev.detenido, true, 'debe cortar antes que pos-trabajos.js (aviso «Tenés sin guardar»)');
    assert.deepEqual(e.abiertas, ['CarniSysPrincipal']);
    assert.equal(home.location.href, 'http://localhost/Home/Index');
    assert.equal(home.focused, 1);
});

test('Inicio: con Home ya abierta solo la trae al frente, sin recargarla', () => {
    const home = fakeWindow('http://localhost/Stock/Index');
    const e = crearEntorno({ ventanas: { CarniSysPrincipal: home } });
    e.clic(botonInicio);
    assert.equal(home.location.href, 'http://localhost/Stock/Index');
    assert.equal(home.focused, 1);
});

test('Inicio: ventana emergente bloqueada -> avisa y no navega nada', () => {
    const e = crearEntorno({ ventanas: {} });
    const ev = e.clic(botonInicio);
    assert.equal(ev.defaultPrevented, true);
    assert.equal(e.toasts.length, 1);
    assert.match(e.toasts[0], /bloque/i);
});

test('Inicio: Ctrl+clic y la app instalada conservan el comportamiento normal', () => {
    const e1 = crearEntorno({ ventanas: { CarniSysPrincipal: fakeWindow('about:blank') } });
    const ev1 = e1.clic(botonInicio, { ctrlKey: true });
    assert.equal(ev1.defaultPrevented, false);
    assert.deepEqual(e1.abiertas, []);

    const e2 = crearEntorno({ appInstalada: true, ventanas: { CarniSysPrincipal: fakeWindow('about:blank') } });
    const ev2 = e2.clic(botonInicio);
    assert.equal(ev2.defaultPrevented, false);
    assert.deepEqual(e2.abiertas, []);
});

// ---------- «Punto de Venta» desde Home ----------
test('Punto de Venta: sin POS registrado no hace nada (el enlace navega normal)', () => {
    const e = crearEntorno({});
    const ev = e.clic(enlacePos);
    assert.equal(ev.defaultPrevented, false);
    assert.deepEqual(e.abiertas, []);
});

test('Punto de Venta: con el POS abierto y alcanzable lo trae al frente, sin abrir otro ni recargarlo', () => {
    const pos = fakeWindow('http://localhost/Ventas/POS?modoPos=original');
    const e = crearEntorno({ ventanas: { 'CarniSysPOS-7-3-original': pos } });
    e.registrarPos(7, 3);
    const ev = e.clic(enlacePos);
    assert.equal(ev.defaultPrevented, true);
    assert.deepEqual(e.abiertas, ['CarniSysPOS-7-3-original']);
    assert.equal(pos.focused, 1);
    assert.equal(pos.closed, false);
    assert.equal(pos.location.href, 'http://localhost/Ventas/POS?modoPos=original');
});

test('Punto de Venta: un POS en segundo plano con latido de hasta 2 min se sigue considerando abierto', () => {
    const pos = fakeWindow('http://localhost/Ventas/POS');
    const e = crearEntorno({ ventanas: { 'CarniSysPOS-7-3-original': pos } });
    e.registrarPos(7, 3, { haceMs: 90000 });
    assert.equal(e.clic(enlacePos).defaultPrevented, true);
    assert.equal(pos.focused, 1);
});

test('Punto de Venta: latido de mas de 2 min (POS cerrado sin avisar) o de otro usuario/sucursal -> abre normal', () => {
    const e1 = crearEntorno({ ventanas: { 'CarniSysPOS-7-3-original': fakeWindow('http://localhost/x') } });
    e1.registrarPos(7, 3, { haceMs: 121000 });
    assert.equal(e1.clic(enlacePos).defaultPrevented, false);
    assert.deepEqual(e1.abiertas, []);

    const e2 = crearEntorno({ ventanas: { 'CarniSysPOS-8-3-original': fakeWindow('http://localhost/x') } });
    e2.registrarPos(8, 3); // otro usuario: puede tener su propio POS original
    assert.equal(e2.clic(enlacePos).defaultPrevented, false);
    assert.deepEqual(e2.abiertas, []);

    const e3 = crearEntorno({ ventanas: { 'CarniSysPOS-7-4-original': fakeWindow('http://localhost/x') } });
    e3.registrarPos(7, 4); // misma persona, otra sucursal
    assert.equal(e3.clic(enlacePos).defaultPrevented, false);
});

test('Punto de Venta: POS abierto pero inalcanzable -> cierra la pestana en blanco, avisa y NO abre otro', async () => {
    const enBlanco = fakeWindow('about:blank');
    const e = crearEntorno({ ventanas: { 'CarniSysPOS-7-3-original': enBlanco } });
    e.registrarPos(7, 3);
    // El enlace real navegaria con location.href: aca se verifica que el clic lo frene (no navega solo; solo si el usuario insiste, ver el test de onOpenAnyway).
    const ev = e.clic(enlacePos);
    assert.equal(ev.defaultPrevented, true);
    assert.equal(enBlanco.closed, true, 'no debe dejar una pestana en blanco');
    assert.equal(e.dialogos.length, 1);
    assert.match(e.dialogos[0].title, /ya está abierto/i);
});

test('Punto de Venta: bloqueada la ventana emergente -> avisa y no abre otro POS', () => {
    const e = crearEntorno({ ventanas: {} });
    e.registrarPos(7, 3);
    const ev = e.clic(enlacePos);
    assert.equal(ev.defaultPrevented, true);
    assert.equal(e.toasts.length, 1);
    assert.equal(e.dialogos.length, 0);
});

test('traerPosAlFrente: si el usuario insiste («No lo encuentro») se llama a onOpenAnyway; si no, nunca', async () => {
    const e = crearEntorno({ ventanas: { 'CarniSysPOS-7-3-original': fakeWindow('about:blank') } });
    e.registrarPos(7, 3);
    const cfg = e.sandbox.POSVentanaUnica.configDeEnlace(enlacePos.el);
    assert.deepEqual({ ...cfg }, { userId: 7, sucursalId: 3, productKey: '' });

    let abiertoDeTodosModos = 0;
    e.responderDialogo({ dismiss: 'backdrop' });
    assert.equal(e.sandbox.POSVentanaUnica.traerPosAlFrente(cfg, () => abiertoDeTodosModos++), true);
    await new Promise((r) => setImmediate(r));
    assert.equal(abiertoDeTodosModos, 0);

    e.responderDialogo({ dismiss: 'cancel' });
    assert.equal(e.sandbox.POSVentanaUnica.traerPosAlFrente(cfg, () => abiertoDeTodosModos++), true);
    await new Promise((r) => setImmediate(r));
    assert.equal(abiertoDeTodosModos, 1);
});

test('Punto de Venta: Ctrl+clic (el usuario pide su propia pestana) no se intercepta', () => {
    const e = crearEntorno({ ventanas: { 'CarniSysPOS-7-3-original': fakeWindow('http://localhost/x') } });
    e.registrarPos(7, 3);
    const ev = e.clic(enlacePos, { ctrlKey: true });
    assert.equal(ev.defaultPrevented, false);
    assert.deepEqual(e.abiertas, []);
});

// ---------- Home se nombra ----------
test('Home se nombra CarniSysPrincipal solo si es la ventana de arriba, tiene menu lateral y no tenia nombre', () => {
    assert.equal(crearEntorno({ tieneMenuLateral: true }).sandbox.name, 'CarniSysPrincipal');
    assert.equal(crearEntorno({ tieneMenuLateral: false }).sandbox.name, '', 'el POS no tiene menu lateral: no se nombra');
    assert.equal(crearEntorno({ tieneMenuLateral: true, nombreVentana: 'carnisys-trabajo-menu-stock' }).sandbox.name, 'carnisys-trabajo-menu-stock');
});

// ---------- openOriginalEntry ----------
test('openOriginalEntry: usa la misma clave que el latido, respeta la tolerancia y exige windowName', () => {
    const e = crearEntorno({});
    e.registrarPos(7, 3, { haceMs: 20000 });
    const multi = e.sandbox.POSMultiInstance;
    assert.equal(multi.openOriginalEntry(7, 3, '', 15000), null, 'con la tolerancia del latido (15 s) ya esta vencido');
    assert.equal(multi.openOriginalEntry(7, 3, '', 120000).windowName, 'CarniSysPOS-7-3-original');
    assert.equal(multi.openOriginalEntry(7, 3, 'expendio', 120000), null, 'Venta y Expendio no se mezclan');
    e.store.set(STATE_KEY(7, 3), JSON.stringify({ original: { id: 'x', ts: Date.now() }, duplicado: null }));
    assert.equal(multi.openOriginalEntry(7, 3, '', 120000), null, 'sin windowName no hay a que ventana ir');
    e.store.set(STATE_KEY(7, 3), '{no es json');
    assert.equal(multi.openOriginalEntry(7, 3, '', 120000), null, 'estado corrupto: se trata como cerrado');
});
