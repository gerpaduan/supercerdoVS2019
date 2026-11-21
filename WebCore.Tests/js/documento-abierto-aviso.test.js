// Tests del aviso «Ya tenés un ... nuevo abierto» (WebCore/wwwroot/Scripts/app/documento-abierto.js, separarAbiertos; 2026-11-21).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// separarAbiertos decide cuales de los documentos abiertos en otras pestañas/paneles/ventanas se cierran solos (Nuevo vacios) y cuales se
// muestran en el aviso. Es una funcion pura: el script se carga en un sandbox (vm) con un navegador falso minimo.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'documento-abierto.js'), 'utf8');

function cargar() {
    const sandbox = { URL, Date, Math, JSON, console, document: {}, crypto: undefined };
    sandbox.window = sandbox;
    sandbox.window.location = { href: 'http://localhost/x', origin: 'http://localhost' };
    sandbox.window.sessionStorage = { getItem: () => null, setItem() {} };
    sandbox.window.localStorage = { length: 0, key: () => null, getItem: () => null, setItem() {}, removeItem() {} };
    vm.createContext(sandbox);
    vm.runInContext(SCRIPT, sandbox);
    return sandbox.window.DocumentoAbierto;
}

const nuevo = (extra) => Object.assign({ de: 'a', tipo: 'nuevo', cantLineas: 0, lugar: 'pestaña del navegador', enPos: false }, extra || {});

test('un Nuevo vacio, con acciones y fuera del POS se cierra solo', () => {
    const { cerrables, resto } = cargar()._separarAbiertos([nuevo()]);
    assert.equal(cerrables.length, 1);
    assert.equal(resto.length, 0);
});

test('un Nuevo con lineas va al aviso, no se cierra', () => {
    const { cerrables, resto } = cargar()._separarAbiertos([nuevo({ cantLineas: 3 })]);
    assert.equal(cerrables.length, 0);
    assert.equal(resto.length, 1);
});

test('una carga dentro del POS nunca se cierra sola, aunque este vacia', () => {
    const { cerrables, resto } = cargar()._separarAbiertos([nuevo({ enPos: true })]);
    assert.equal(cerrables.length, 0);
    assert.equal(resto.length, 1);
});

test('una pestaña con la version vieja del script (sin lugar) no entiende "cerrar": va al aviso', () => {
    const { cerrables, resto } = cargar()._separarAbiertos([nuevo({ lugar: undefined })]);
    assert.equal(cerrables.length, 0);
    assert.equal(resto.length, 1);
});

test('una edicion abierta (modoLista de Elaborados) nunca se cierra sola', () => {
    const { cerrables, resto } = cargar()._separarAbiertos([nuevo({ tipo: 'edicion', idRegistro: 5 })]);
    assert.equal(cerrables.length, 0);
    assert.equal(resto.length, 1);
});

test('mezcla: se separan los vacios y se conserva el orden del resto', () => {
    const lista = [nuevo({ de: '1' }), nuevo({ de: '2', cantLineas: 2 }), nuevo({ de: '3' }), nuevo({ de: '4', tipo: 'edicion' })];
    const { cerrables, resto } = cargar()._separarAbiertos(lista);
    assert.deepEqual(Array.from(cerrables, (m) => m.de), ['1', '3']);
    assert.deepEqual(Array.from(resto, (m) => m.de), ['2', '4']);
});

test('sin documentos abiertos devuelve listas vacias', () => {
    const { cerrables, resto } = cargar()._separarAbiertos(undefined);
    assert.equal(cerrables.length, 0);
    assert.equal(resto.length, 0);
});
