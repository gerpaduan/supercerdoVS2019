// Tests de "volver a la app instalada despues de loguearse" (docs/DECISIONS.md 2026-11-21 «Login en una app instalada vuelve a la app»):
//   - WebCore/wwwroot/Scripts/app/app-independiente.js guarda en sessionStorage la direccion de arranque de la app (?ventana=1) y la publica en window.__appInicio.
//   - WebCore/wwwroot/Scripts/app/login-volver-app.js completa el ReturnUrl vacio del login con esa direccion.
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// Cada script se carga en un sandbox (vm) con un window/document falso: la misma "ventana" (sessionStorage) se reusa entre paginas para simular la navegacion.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const CARPETA = path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app');
const APP_INDEPENDIENTE = fs.readFileSync(path.join(CARPETA, 'app-independiente.js'), 'utf8');
const LOGIN_VOLVER_APP = fs.readFileSync(path.join(CARPETA, 'login-volver-app.js'), 'utf8');

// sessionStorage minimo (memoria de UNA ventana del navegador).
function crearSessionStorage() {
    const datos = new Map();
    return {
        getItem: (clave) => (datos.has(clave) ? datos.get(clave) : null),
        setItem: (clave, valor) => { datos.set(clave, String(valor)); },
    };
}

// Carga una "pagina" en la ventana: corre app-independiente.js (como en el head de _PwaHead) y, si hay campo ReturnUrl, login-volver-app.js.
function cargarPagina({ storage, pathname, search = '', standalone, returnUrl }) {
    const campo = returnUrl === undefined ? null : { value: returnUrl };
    const sandbox = {
        matchMedia: () => ({ matches: standalone }),
        navigator: {},
        location: { pathname, search },
        sessionStorage: storage,
        document: { getElementById: (id) => (id === 'ReturnUrl' ? campo : null) },
    };
    sandbox.window = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(APP_INDEPENDIENTE, sandbox);
    if (campo) vm.runInContext(LOGIN_VOLVER_APP, sandbox);
    return { sandbox, campo };
}

test('app POS instalada: cerrar sesion y loguearse vuelve al POS (no al dashboard)', () => {
    const storage = crearSessionStorage();
    cargarPagina({ storage, pathname: '/Ventas/POS', search: '?ventana=1', standalone: true });
    // El POS sigue navegando dentro de la ventana (otra URL, sin ?ventana=1): el inicio guardado no cambia.
    cargarPagina({ storage, pathname: '/Ventas/POS', search: '?posInstanceId=abc', standalone: true });
    // El cierre de sesion redirige a /Login sin returnUrl; el login queda fuera del scope (puede no ser standalone).
    const { campo } = cargarPagina({ storage, pathname: '/Login', standalone: false, returnUrl: '' });
    assert.equal(campo.value, '/Ventas/POS?ventana=1');
});

test('sitio bajo subcarpeta de IIS: se conserva el PathBase en el destino', () => {
    const storage = crearSessionStorage();
    cargarPagina({ storage, pathname: '/CarniSysWeb/PuntosExpendio/POS', search: '?ventana=1', standalone: true });
    const { campo } = cargarPagina({ storage, pathname: '/CarniSysWeb/Login', standalone: true, returnUrl: '' });
    assert.equal(campo.value, '/CarniSysWeb/PuntosExpendio/POS?ventana=1');
});

test('sesion vencida: el destino que ya trae el login no se pisa', () => {
    const storage = crearSessionStorage();
    cargarPagina({ storage, pathname: '/Stock/Index', search: '?ventana=1', standalone: true });
    const { campo } = cargarPagina({ storage, pathname: '/Login', standalone: true, returnUrl: '/Stock/Editar/5' });
    assert.equal(campo.value, '/Stock/Editar/5');
});

test('pestana normal del navegador con ?ventana=1: no guarda inicio y el login va al dashboard', () => {
    const storage = crearSessionStorage();
    cargarPagina({ storage, pathname: '/Ventas/POS', search: '?ventana=1', standalone: false });
    const { campo } = cargarPagina({ storage, pathname: '/Login', standalone: false, returnUrl: '' });
    assert.equal(campo.value, '');
});

test('app general CarniSys (standalone sin ?ventana=1): el login va al dashboard', () => {
    const storage = crearSessionStorage();
    cargarPagina({ storage, pathname: '/Home/Index', search: '?source=pwa', standalone: true });
    const { campo, sandbox } = cargarPagina({ storage, pathname: '/Login', standalone: true, returnUrl: '' });
    assert.equal(campo.value, '');
    assert.equal(sandbox.__appInicio, null);
});

test('destinoLogin: solo acepta rutas del mismo sitio', () => {
    const { sandbox } = cargarPagina({ storage: crearSessionStorage(), pathname: '/Login', standalone: false, returnUrl: '' });
    const destino = sandbox.LoginVolverApp.destinoLogin;
    assert.equal(destino('', '/Ventas/POS?ventana=1'), '/Ventas/POS?ventana=1');
    assert.equal(destino('', '//otro-sitio.com/x'), null);
    assert.equal(destino('', '/\\otro-sitio.com'), null);
    assert.equal(destino('', 'https://otro-sitio.com'), null);
    assert.equal(destino('', '/'), null);
    assert.equal(destino('', null), null);
    assert.equal(destino('   ', '/Stock/Index?ventana=1'), '/Stock/Index?ventana=1');
});

test('sin sessionStorage (modo restringido): no falla y no cambia el destino', () => {
    const roto = { getItem: () => { throw new Error('bloqueado'); }, setItem: () => { throw new Error('bloqueado'); } };
    const { campo, sandbox } = cargarPagina({ storage: roto, pathname: '/Login', standalone: true, returnUrl: '' });
    assert.equal(campo.value, '');
    assert.equal(sandbox.__appInicio, null);
});
