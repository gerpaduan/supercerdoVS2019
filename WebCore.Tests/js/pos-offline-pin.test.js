// Tests del login sin conexion con PIN (WebCore/wwwroot/Scripts/app/pos-offline-pin.js).
// Corren con el runner de Node:  node --test WebCore.Tests/js
// fixtures/pos-offline-pin.json lo genera el C# real (WebCore.Tests/PosOfflinePinParidadTests.cs) con
// Utilidades.PasswordSecurity: si el JS calcula otro hash, nadie podria entrar sin conexion.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');

const Pin = require(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'pos-offline-pin.js'));
const FIXTURE = JSON.parse(fs.readFileSync(path.join(__dirname, 'fixtures', 'pos-offline-pin.json'), 'utf8'));

// Doble en memoria de PosOfflineDb (solo leer/guardar).
function dbEnMemoria() {
    const datos = {};
    return { leer: async k => datos[k], guardar: async (k, v) => { datos[k] = v; } };
}

const usuario = () => ({ id: 7, nombre: 'Cajero', pinHash: FIXTURE.pinHash, pinSalt: FIXTURE.pinSalt, pinIteraciones: FIXTURE.pinIteraciones });

test('el hash del PIN calculado en el navegador es igual al del servidor', async () => {
    const hash = await Pin.hashPin(FIXTURE.pin, FIXTURE.pinSalt, FIXTURE.pinIteraciones);
    assert.equal(hash, FIXTURE.pinHash);
});

test('PIN correcto entra; incorrecto no', async () => {
    const db = dbEnMemoria();
    assert.equal((await Pin.verificar(usuario(), FIXTURE.pin, db)).ok, true);
    const mal = await Pin.verificar(usuario(), '9999', db);
    assert.equal(mal.ok, false);
    assert.match(mal.mensaje, /Te quedan 4 intentos/);
});

test('tras 5 fallos queda bloqueado 5 minutos en esta PC, aun con el PIN correcto', async () => {
    const db = dbEnMemoria();
    const t0 = Date.UTC(2026, 10, 26, 12, 0, 0);
    for (let i = 0; i < Pin.MAX_INTENTOS; i++) await Pin.verificar(usuario(), '9999', db, t0);
    const bloqueado = await Pin.verificar(usuario(), FIXTURE.pin, db, t0 + 60000);
    assert.equal(bloqueado.ok, false);
    assert.match(bloqueado.mensaje, /Demasiados intentos/);
    const despues = await Pin.verificar(usuario(), FIXTURE.pin, db, t0 + Pin.MINUTOS_BLOQUEO * 60000 + 1);
    assert.equal(despues.ok, true);
});

test('usuario sin PIN o PIN con forma invalida no entra', async () => {
    const db = dbEnMemoria();
    assert.equal((await Pin.verificar({ id: 1, nombre: 'X' }, '1234', db)).ok, false);
    assert.match((await Pin.verificar(usuario(), '12a4', db)).mensaje, /4 a 6/);
});
