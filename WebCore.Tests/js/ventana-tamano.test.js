// Tests de la eleccion del ancla de los botones − / + de tamano de ventana (WebCore/wwwroot/Scripts/app/ventana-tamano.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// buscarAncla decide ANTES de que boton se inserta el par; el script se carga en un sandbox (vm) con un document minimo.
// Regresion 2026-11-24: con SQL Server los borradores genericos estan apagados (no hay boton «sin guardar») y el par −/+ no aparecia en SM / San Lorenzo.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'ventana-tamano.js'), 'utf8');
const sandbox = { document: { addEventListener() {} } };
sandbox.window = sandbox;
sandbox.top = sandbox; // el script no corre dentro de un iframe (window !== window.top)
vm.createContext(sandbox);
vm.runInContext(SCRIPT, sandbox);
const { buscarAncla } = sandbox.VentanaTamano;

// Arma un "buscar" que solo conoce los selectores indicados; closest() devuelve null salvo que se pida un envoltorio .ex-ctl.
function pantallaCon(selectores, envoltorios = {}) {
    return (selector) => {
        if (!selectores.includes(selector)) return null;
        return { id: selector, closest: (css) => (css === '.ex-ctl' ? envoltorios[selector] || null : null) };
    };
}

test('con el boton «Movimientos sin guardar» (Postgres) el par va antes de ese boton', () => {
    const ancla = buscarAncla(pantallaCon(['#btnVerBorradoresMovimientos', '#btnPantallaCompleta']));
    assert.equal(ancla.referencia.id, '#btnVerBorradoresMovimientos');
});

test('sin borradores (SQL Server) el par va antes de «Pantalla completa»: Movimientos, Stock y Elaborados', () => {
    const ancla = buscarAncla(pantallaCon(['#btnPantallaCompleta']));
    assert.equal(ancla.referencia.id, '#btnPantallaCompleta');
    assert.equal(ancla.enExistencia, false);
});

test('Existencia sin borradores sigue anclando en el bloque Excel / PDF, no en pantalla completa', () => {
    const envoltorio = { id: 'ex-ctl-excel' };
    const ancla = buscarAncla(pantallaCon(['#btnExcelExistencia'], { '#btnExcelExistencia': envoltorio }));
    assert.equal(ancla.referencia, envoltorio);
    assert.equal(ancla.enExistencia, true);
});

test('el panel de stock del destino (Existencia en modo movimiento) no lleva el par', () => {
    assert.equal(buscarAncla(pantallaCon(['.ex-modo-mov', '#btnPantallaCompleta'])), null);
});

test('pantallas sin ninguno de los botones (listados, POS, Expendio) no muestran nada', () => {
    assert.equal(buscarAncla(pantallaCon([])), null);
});
