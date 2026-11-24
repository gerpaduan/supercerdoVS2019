// Tests de app-instalada.js (docs/DECISIONS.md 2026-11-17 c y 2026-11-24): en una app independiente instalada, los enlaces fuera de la carpeta de la app no navegan
// y avisan «No disponible en esta ventana»; Volver / Cancelar al Index de la propia app (que Url.Action genera SIN barra final: "/Movimientos") SI navegan.
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'app-instalada.js'), 'utf8');

// Carga el script en una "pagina" de la app Movimientos (scope con PathBase /CarniSysWeb) y devuelve una funcion que simula el clic en un enlace:
// devuelve { bloqueado, avisos }.
function cargarPagina({ scope, independiente = true, standalone = true, paginaActual = '/CarniSysWeb/Movimientos/Editar' }) {
    const avisos = [];
    let manejadorClic = null;
    const sandbox = {
        URL,
        matchMedia: () => ({ matches: standalone }),
        navigator: {},
        location: { href: 'http://localhost' + paginaActual, origin: 'http://localhost', pathname: paginaActual, search: '' },
        __appIndependiente: independiente,
        alert: (texto) => avisos.push(texto),
        document: {
            querySelector: (selector) => (selector === 'meta[name="carnisys-app-scope"]' ? { getAttribute: () => scope } : null),
            querySelectorAll: () => [],
            addEventListener: (tipo, manejador) => { if (tipo === 'click') manejadorClic = manejador; },
        },
    };
    sandbox.window = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(SCRIPT, sandbox);

    return function clicEnEnlace(href) {
        let bloqueado = false;
        const enlace = { href: 'http://localhost' + href, hasAttribute: () => false };
        const evento = {
            target: { closest: () => enlace },
            preventDefault: () => { bloqueado = true; },
            stopImmediatePropagation: () => {},
        };
        if (manejadorClic) manejadorClic(evento);
        return { bloqueado, avisos };
    };
}

const SCOPE = '/CarniSysWeb/Movimientos/';

test('Volver / Cancelar al Index de la app (/Movimientos sin barra final) NO se bloquea', () => {
    const clic = cargarPagina({ scope: SCOPE });
    assert.equal(clic('/CarniSysWeb/Movimientos').bloqueado, false);
    assert.equal(clic('/CarniSysWeb/Movimientos?idSucursalOrigen=1').bloqueado, false);
});

test('enlaces debajo de la carpeta de la app no se bloquean', () => {
    const clic = cargarPagina({ scope: SCOPE });
    assert.equal(clic('/CarniSysWeb/Movimientos/Index').bloqueado, false);
    assert.equal(clic('/CarniSysWeb/Movimientos/Editar/5').bloqueado, false);
});

test('enlaces a otra seccion del sistema se bloquean y avisan', () => {
    const clic = cargarPagina({ scope: SCOPE });
    const resultado = clic('/CarniSysWeb/Stock');
    assert.equal(resultado.bloqueado, true);
    assert.deepEqual(resultado.avisos, ['No disponible en esta ventana.']);
    assert.equal(clic('/CarniSysWeb/Home/Index').bloqueado, true);
});

test('una carpeta que solo empieza igual (/MovimientosOtro) cuenta como otra seccion', () => {
    const clic = cargarPagina({ scope: SCOPE });
    assert.equal(clic('/CarniSysWeb/MovimientosOtro').bloqueado, true);
});

test('el login (sesion vencida) pasa siempre', () => {
    const clic = cargarPagina({ scope: SCOPE });
    assert.equal(clic('/CarniSysWeb/Account/Login').bloqueado, false);
});

test('fuera de una app independiente (pestana normal o app general) no bloquea nada', () => {
    const clic = cargarPagina({ scope: SCOPE, independiente: false });
    assert.equal(clic('/CarniSysWeb/Stock').bloqueado, false);
});
