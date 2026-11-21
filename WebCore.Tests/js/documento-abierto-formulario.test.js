// Tests del traspaso de formularios simples y del registro generico (WebCore/wwwroot/Scripts/app/documento-abierto.js; Personas y Compras).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// No hay jsdom: se arma un DOM falso minimo (form.elements con campos que anotan los eventos que reciben) dentro de un sandbox (vm).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'documento-abierto.js'), 'utf8');

function campo(nombre, tipo, valor, extra) {
    return Object.assign({
        name: nombre, type: tipo, value: valor, checked: false, disabled: false, readOnly: false, tagName: 'INPUT',
        eventos: [], dispatchEvent(e) { this.eventos.push(e.type); return true; }
    }, extra || {});
}

// Carga el script con un "navegador" falso. url: la URL actual de la pagina (puede traer ?traspasoDoc=).
function cargar(elementos, url) {
    const almacen = new Map();
    const form = { elements: elementos };
    const sandbox = {
        URL, Date, Math, JSON, console, Event: class { constructor(tipo) { this.type = tipo; } },
        document: { querySelector: () => form },
        crypto: undefined
    };
    sandbox.window = sandbox;
    sandbox.window.location = { href: url, origin: 'http://localhost' };
    sandbox.window.history = { state: null, replaceState() {} };
    sandbox.window.sessionStorage = { getItem: () => null, setItem() {} };
    sandbox.window.localStorage = {
        get length() { return almacen.size; },
        key: (i) => Array.from(almacen.keys())[i],
        getItem: (k) => (almacen.has(k) ? almacen.get(k) : null),
        setItem: (k, v) => { almacen.set(k, v); },
        removeItem: (k) => { almacen.delete(k); }
    };
    vm.createContext(sandbox);
    vm.runInContext(SCRIPT, sandbox);
    return { win: sandbox, almacen };
}

const CFG = { modulo: 'personas-editar', idUsuario: 7, formSelector: '#f', esAlta: () => true };

test('una alta viaja: los valores quedan en localStorage y la URL lleva la marca', () => {
    const { win, almacen } = cargar([campo('Nombre', 'text', 'Ana'), campo('Activo', 'checkbox', 'true', { checked: true })], 'http://localhost/Personas/Editar');
    win.DocumentoAbierto.registrarTraspasoFormulario(CFG);
    const t = win.DocumentoAbierto.prepararTraspaso();
    assert.ok(t, 'debe devolver un traspaso');
    assert.equal(almacen.size, 1);
    const guardado = JSON.parse(Array.from(almacen.values())[0]);
    assert.equal(guardado.modulo, 'personas-editar');
    assert.equal(guardado.paquete.length, 2);
    assert.match(t.urlCon('/Personas/Editar'), /traspasoDoc=carnisys-traspaso-doc-/);
});

test('no viajan hidden, password, token antiforgery ni campos excluidos', () => {
    const { win, almacen } = cargar([
        campo('Nombre', 'text', 'Ana'),
        campo('IdPersona', 'hidden', '0'),
        campo('Clave', 'password', 'secreta'),
        campo('__RequestVerificationToken', 'text', 'tok'),
        campo('Otro', 'text', 'x')
    ], 'http://localhost/x');
    win.DocumentoAbierto.registrarTraspasoFormulario(Object.assign({}, CFG, { excluir: ['Otro'] }));
    win.DocumentoAbierto.prepararTraspaso();
    const guardado = JSON.parse(Array.from(almacen.values())[0]);
    assert.deepEqual(guardado.paquete.map((c) => c.nombre), ['Nombre']);
    assert.ok(!JSON.stringify(Array.from(almacen.values())).includes('secreta'), 'la contraseña no puede quedar en localStorage');
});

test('una edicion de un registro guardado no viaja (se vuelve a cargar del servidor)', () => {
    const { win, almacen } = cargar([campo('Nombre', 'text', 'Ana')], 'http://localhost/x');
    win.DocumentoAbierto.registrarTraspasoFormulario(Object.assign({}, CFG, { esAlta: () => false }));
    assert.equal(win.DocumentoAbierto.prepararTraspaso(), null);
    assert.equal(almacen.size, 0);
});

test('al llegar con la marca se aplican los valores, se dispara input/change y la clave se consume', () => {
    const origen = cargar([campo('Nombre', 'text', 'Ana'), campo('Activo', 'checkbox', 'true', { checked: true })], 'http://localhost/x');
    origen.win.DocumentoAbierto.registrarTraspasoFormulario(CFG);
    const t = origen.win.DocumentoAbierto.prepararTraspaso();

    const nombre = campo('Nombre', 'text', '');
    const activo = campo('Activo', 'checkbox', 'true');
    const destino = cargar([nombre, activo], 'http://localhost' + t.urlCon('/Personas/Editar'));
    destino.almacen.set(t.clave, origen.almacen.get(t.clave));   // mismo localStorage del navegador
    destino.win.DocumentoAbierto.registrarTraspasoFormulario(CFG);

    assert.equal(nombre.value, 'Ana');
    assert.equal(activo.checked, true);
    assert.deepEqual(nombre.eventos, ['input', 'change']);
    assert.equal(destino.almacen.has(t.clave), false, 'la clave se usa una sola vez');
});

test('un traspaso de otro modulo o de otro usuario se ignora', () => {
    const origen = cargar([campo('Nombre', 'text', 'Ana')], 'http://localhost/x');
    origen.win.DocumentoAbierto.registrarTraspasoFormulario(CFG);
    const t = origen.win.DocumentoAbierto.prepararTraspaso();

    for (const otra of [{ modulo: 'usuarios-editar' }, { idUsuario: 99 }]) {
        const nombre = campo('Nombre', 'text', '');
        const destino = cargar([nombre], 'http://localhost' + t.urlCon('/x'));
        destino.almacen.set(t.clave, origen.almacen.get(t.clave));
        destino.win.DocumentoAbierto.registrarTraspasoFormulario(Object.assign({}, CFG, otra));
        assert.equal(nombre.value, '');
    }
});

test('un campo bloqueado en la pantalla nueva no se pisa', () => {
    const origen = cargar([campo('Nombre', 'text', 'Ana')], 'http://localhost/x');
    origen.win.DocumentoAbierto.registrarTraspasoFormulario(CFG);
    const t = origen.win.DocumentoAbierto.prepararTraspaso();

    const nombre = campo('Nombre', 'text', 'fijo', { readOnly: true });
    const destino = cargar([nombre], 'http://localhost' + t.urlCon('/x'));
    destino.almacen.set(t.clave, origen.almacen.get(t.clave));
    destino.win.DocumentoAbierto.registrarTraspasoFormulario(CFG);
    assert.equal(nombre.value, 'fijo');
});

// ---- registrarTraspaso generico (Compras: paquete propio {draft, clientId}, con alCeder) ----

test('registrarTraspaso: el paquete propio viaja, alCeder se llama al ceder y al llegar se aplica', () => {
    const origen = cargar([], 'http://localhost/Compras/NuevaCompra');
    let cedido = 0;
    origen.win.DocumentoAbierto.registrarTraspaso({
        modulo: 'compras-editar', idUsuario: 7, esAlta: () => true,
        obtenerPaquete: () => ({ draft: { lineas: [{ codigo: 'A1' }] }, clientId: 'abc' }),
        aplicarPaquete: () => {}, alCeder: () => { cedido++; }
    });
    const t = origen.win.DocumentoAbierto.prepararTraspaso();
    assert.ok(t);
    assert.equal(cedido, 0, 'alCeder no se llama al preparar, solo al ceder');
    t.ceder();
    assert.equal(cedido, 1);

    let recibido = null;
    const destino = cargar([], 'http://localhost' + t.urlCon('/Compras/NuevaCompra'));
    destino.almacen.set(t.clave, origen.almacen.get(t.clave));
    destino.win.DocumentoAbierto.registrarTraspaso({
        modulo: 'compras-editar', idUsuario: 7, esAlta: () => true,
        obtenerPaquete: () => ({}), aplicarPaquete: (p) => { recibido = p; }
    });
    assert.deepEqual(recibido, { draft: { lineas: [{ codigo: 'A1' }] }, clientId: 'abc' });
    assert.equal(destino.almacen.has(t.clave), false);
});

test('registrarTraspaso: una compra existente (esAlta false) no viaja', () => {
    const { win, almacen } = cargar([], 'http://localhost/x');
    win.DocumentoAbierto.registrarTraspaso({
        modulo: 'compras-editar', idUsuario: 7, esAlta: () => false,
        obtenerPaquete: () => ({ draft: {} }), aplicarPaquete: () => {}
    });
    assert.equal(win.DocumentoAbierto.prepararTraspaso(), null);
    assert.equal(almacen.size, 0);
});

test('registrarTraspaso: si aplicarPaquete falla no rompe la pantalla', () => {
    const origen = cargar([], 'http://localhost/x');
    origen.win.DocumentoAbierto.registrarTraspaso({ modulo: 'm', idUsuario: 7, obtenerPaquete: () => ({ a: 1 }), aplicarPaquete: () => {} });
    const t = origen.win.DocumentoAbierto.prepararTraspaso();
    const destino = cargar([], 'http://localhost' + t.urlCon('/x'));
    destino.almacen.set(t.clave, origen.almacen.get(t.clave));
    assert.doesNotThrow(() => destino.win.DocumentoAbierto.registrarTraspaso({
        modulo: 'm', idUsuario: 7, obtenerPaquete: () => ({}), aplicarPaquete: () => { throw new Error('boom'); }
    }));
});
