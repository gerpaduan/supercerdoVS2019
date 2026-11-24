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

// ---------- decidirAplica: cuando una ventana es independiente (marco, franja y −/+) ----------
// Regresion 2026-11-24: una ventana abierta por el menu / «Abrir en ventana» desde la app instalada (Chrome la trata como app: standalone) no mostraba el
// marco porque se la descartaba por no ser una app de Utilidades; la misma ventana abierta desde una pestana normal si lo mostraba.
const { decidirAplica } = sandbox.VentanaTamano;
const base = { enPos: false, enApp: false, appIndependiente: false, modoVentana: false, embebida: false, conOpener: false };
const con = (cambios) => decidirAplica({ ...base, ...cambios });

test('ventana abierta por nosotros (modo ventana) desde una pestana normal: es independiente', () => {
    assert.equal(con({ modoVentana: true }), true);
});

test('ventana abierta por nosotros (modo ventana) que Chrome trata como app instalada: tambien (el caso de «Movimientos»)', () => {
    assert.equal(con({ modoVentana: true, enApp: true, appIndependiente: false }), true);
});

test('emergente embebida con opener: es independiente, tambien si Chrome la trata como app instalada', () => {
    assert.equal(con({ embebida: true, conOpener: true }), true);
    assert.equal(con({ embebida: true, conOpener: true, enApp: true }), true);
});

test('embebida sin opener y sin app: no (por ejemplo un panel o una pagina embebida suelta)', () => {
    assert.equal(con({ embebida: true }), false);
});

test('app instalada de Utilidades (marca __appIndependiente): es independiente', () => {
    assert.equal(con({ enApp: true, appIndependiente: true }), true);
});

test('app general CarniSys abierta normalmente (standalone, sin marca ni nombre de ventana): no lleva marco', () => {
    assert.equal(con({ enApp: true, appIndependiente: false }), false);
});

test('pestana normal: no es independiente', () => {
    assert.equal(con({}), false);
});

test('carga dentro del POS (iframe): nunca, aunque este en modo ventana', () => {
    assert.equal(con({ enPos: true, modoVentana: true }), false);
    assert.equal(con({ enPos: true, enApp: true, appIndependiente: true }), false);
});
