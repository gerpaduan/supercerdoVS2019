// Tests de las reglas de vigencia del POS sin conexion (WebCore/wwwroot/Scripts/app/pos-offline-vigencia.js).
// Corren con el runner de Node:  node --test WebCore.Tests/js/*.test.js
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');

const Vig = require(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'pos-offline-vigencia.js'));

const DIA = 86400000;
const HORA = 3600000;
const T0 = Date.UTC(2026, 10, 26, 12, 0, 0);
const catalogo = () => ({ productos: [], revisado: new Date(T0).toISOString(), diasVigencia: 15, horasAvisoPrecios: 24 });

test('sin datos guardados no deja vender', () => {
    assert.equal(Vig.evaluar(null, 0, T0).motivo, 'sinDatos');
    assert.equal(Vig.evaluar({ productos: [] }, 0, T0).motivo, 'sinDatos');
});

test('recien actualizada: vigente 15 dias y sin aviso de precios', () => {
    const r = Vig.evaluar(catalogo(), T0, T0 + HORA);
    assert.equal(r.vigente, true);
    assert.equal(r.diasRestantes, 15);
    assert.equal(r.preciosViejos, false);
});

test('precios de mas de 24 horas avisan pero no bloquean', () => {
    const r = Vig.evaluar(catalogo(), T0, T0 + 25 * HORA);
    assert.equal(r.vigente, true);
    assert.equal(r.preciosViejos, true);
});

test('vence exactamente a los 15 dias', () => {
    assert.equal(Vig.evaluar(catalogo(), T0, T0 + 15 * DIA - 1).vigente, true);
    assert.equal(Vig.evaluar(catalogo(), T0, T0 + 15 * DIA).motivo, 'vencida');
});

test('atrasar el reloj no estira la vigencia', () => {
    // La PC ya vio el dia 10; ahora dice ser el dia 1.
    assert.equal(Vig.evaluar(catalogo(), T0 + 10 * DIA, T0 + DIA).motivo, 'reloj');
    // Antes de la ultima actualizacion.
    assert.equal(Vig.evaluar(catalogo(), 0, T0 - 2 * HORA).motivo, 'reloj');
    // Un ajuste chico del reloj no bloquea.
    assert.equal(Vig.evaluar(catalogo(), T0 + 2 * HORA, T0 + 90 * 60000).vigente, true);
});

test('conserva el admin guardado mientras siga vigente con la misma clave', () => {
    const anterior = { idEmpresa: 1, usuarios: [{ id: 1, admin: true, version: 'A' }, { id: 2, admin: false, version: 'B' }] };
    const nuevo = { idEmpresa: 1, usuarios: [{ id: 3, admin: false, version: 'C' }], adminsVigentes: [{ id: 1, version: 'A' }] };
    assert.deepEqual(Vig.combinarUsuarios(anterior, nuevo).map(u => u.id), [3, 1]);
});

test('descarta el admin que cambio su clave, se dio de baja o es de otra empresa', () => {
    const anterior = { idEmpresa: 1, usuarios: [{ id: 1, admin: true, version: 'A' }] };
    assert.deepEqual(Vig.combinarUsuarios(anterior, { idEmpresa: 1, usuarios: [], adminsVigentes: [{ id: 1, version: 'NUEVA' }] }), []);
    assert.deepEqual(Vig.combinarUsuarios(anterior, { idEmpresa: 1, usuarios: [], adminsVigentes: [] }), []);
    assert.deepEqual(Vig.combinarUsuarios(anterior, { idEmpresa: 2, usuarios: [], adminsVigentes: [{ id: 1, version: 'A' }] }), []);
});

test('los no administradores que ya no vienen se borran', () => {
    const anterior = { idEmpresa: 1, usuarios: [{ id: 2, admin: false, version: 'B' }] };
    assert.deepEqual(Vig.combinarUsuarios(anterior, { idEmpresa: 1, usuarios: [], adminsVigentes: [] }), []);
});

test('al vencer se borran las claves y se fuerza una descarga completa', () => {
    const c = Object.assign(catalogo(), { version: 'V1', usuarios: [{ id: 1 }], adminsVigentes: [{ id: 1 }] });
    const limpio = Vig.sinClaves(c);
    assert.deepEqual(limpio.usuarios, []);
    assert.equal(limpio.version, '');
    assert.equal(limpio.revisado, c.revisado);
    assert.equal(c.usuarios.length, 1);
});
