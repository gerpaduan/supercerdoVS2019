// Tests del ingreso sin conexion con contrasena o PIN (WebCore/wwwroot/Scripts/app/pos-offline-pin.js).
// Corren con el runner de Node:  node --test WebCore.Tests/js/*.test.js
// fixtures/pos-offline-pin.json lo genera el C# real (WebCore.Tests/PosOfflinePinParidadTests.cs) con
// Utilidades.PasswordSecurity: si el JS calcula otra huella, nadie podria entrar sin conexion.
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

const usuario = () => ({
    id: 7, nombre: 'Cajero',
    claveHash: FIXTURE.claveHash, claveSalt: FIXTURE.claveSalt, claveIteraciones: FIXTURE.claveIteraciones,
    pinHash: FIXTURE.pinHash, pinSalt: FIXTURE.pinSalt, pinIteraciones: FIXTURE.pinIteraciones
});

test('las huellas calculadas en el navegador son iguales a las del servidor (PIN y contraseña con ñ)', async () => {
    assert.equal(await Pin.hashClave(FIXTURE.pin, FIXTURE.pinSalt, FIXTURE.pinIteraciones), FIXTURE.pinHash);
    assert.equal(await Pin.hashClave(FIXTURE.clave, FIXTURE.claveSalt, FIXTURE.claveIteraciones), FIXTURE.claveHash);
});

test('entra con la contraseña o con el PIN; con otra cosa no', async () => {
    const db = dbEnMemoria();
    assert.equal((await Pin.verificar(usuario(), FIXTURE.clave, db)).ok, true);
    assert.equal((await Pin.verificar(usuario(), FIXTURE.pin, db)).ok, true);
    const mal = await Pin.verificar(usuario(), '9999', db);
    assert.equal(mal.ok, false);
    assert.match(mal.mensaje, /Te quedan 4 intentos/);
    assert.equal((await Pin.verificar(usuario(), FIXTURE.clave.toLowerCase(), db)).ok, false);
});

test('sin PIN configurado entra igual con la contraseña', async () => {
    const sinPin = Object.assign(usuario(), { pinHash: null, pinSalt: null });
    const db = dbEnMemoria();
    assert.equal((await Pin.verificar(sinPin, FIXTURE.clave, db)).ok, true);
    assert.equal((await Pin.verificar(sinPin, FIXTURE.pin, db)).ok, false);
});

test('tras 5 fallos queda bloqueado 5 minutos en esta PC, aun con la clave correcta', async () => {
    const db = dbEnMemoria();
    const t0 = Date.UTC(2026, 10, 26, 12, 0, 0);
    for (let i = 0; i < Pin.MAX_INTENTOS; i++) await Pin.verificar(usuario(), '9999', db, t0);
    const bloqueado = await Pin.verificar(usuario(), FIXTURE.clave, db, t0 + 60000);
    assert.equal(bloqueado.ok, false);
    assert.match(bloqueado.mensaje, /Demasiados intentos/);
    const despues = await Pin.verificar(usuario(), FIXTURE.clave, db, t0 + Pin.MINUTOS_BLOQUEO * 60000 + 1);
    assert.equal(despues.ok, true);
});

test('un usuario sin ninguna clave guardada no entra, y vacío pide la clave', async () => {
    const db = dbEnMemoria();
    assert.match((await Pin.verificar({ id: 1, nombre: 'X' }, '1234', db)).mensaje, /iniciar sesión/);
    assert.match((await Pin.verificar(usuario(), '', db)).mensaje, /contraseña o tu PIN/);
});
