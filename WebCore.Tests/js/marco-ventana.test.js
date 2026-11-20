// Tests de la geometria del marco de las ventanas independientes (WebCore/wwwroot/Scripts/app/marco-ventana.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// calcularGeometria decide la nueva posicion y tamano de la ventana al arrastrar un borde o una esquina; el script se carga en un sandbox (vm)
// sin document, que es el caso en el que solo expone esa funcion pura.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'marco-ventana.js'), 'utf8');
const sandbox = {};
sandbox.window = sandbox;
vm.createContext(sandbox);
vm.runInContext(SCRIPT, sandbox);
// Los objetos vienen de otro "realm" (vm): se copian a objetos de este contexto para que deepEqual los compare bien.
const calcularGeometria = (...args) => ({ ...sandbox.MarcoVentana.calcularGeometria(...args) });

// Pantalla de 1920x1040 disponibles; ventana de 800x600 en (400, 100); minimo 480x360.
const PANTALLA = { x: 0, y: 0, w: 1920, h: 1040 };
const MINIMO = { w: 480, h: 360 };
const INICIO = { x: 400, y: 100, w: 800, h: 600 };
const calcular = (borde, dx, dy, inicio = INICIO) => calcularGeometria(borde, inicio, dx, dy, PANTALLA, MINIMO);

test('borde derecho: crece el ancho y el borde izquierdo queda fijo', () => {
    assert.deepEqual(calcular('e', 100, 50), { x: 400, y: 100, w: 900, h: 600 });
});

test('borde izquierdo: el borde derecho queda fijo (crece hacia la izquierda)', () => {
    const r = calcular('w', -100, 0);
    assert.equal(r.w, 900);
    assert.equal(r.x + r.w, INICIO.x + INICIO.w);
});

test('borde izquierdo hacia adentro: achica y el borde derecho sigue fijo', () => {
    const r = calcular('w', 150, 0);
    assert.equal(r.w, 650);
    assert.equal(r.x, 550);
});

test('borde superior: el borde inferior queda fijo; solo cambia el alto', () => {
    const r = calcular('n', 0, -60);
    assert.equal(r.h, 660);
    assert.equal(r.y + r.h, INICIO.y + INICIO.h);
    assert.equal(r.w, INICIO.w);
});

test('borde inferior: crece el alto hacia abajo', () => {
    assert.deepEqual(calcular('s', 20, 90), { x: 400, y: 100, w: 800, h: 690 });
});

test('esquina inferior derecha cambia ancho y alto', () => {
    assert.deepEqual(calcular('se', 100, 100), { x: 400, y: 100, w: 900, h: 700 });
});

test('esquina superior izquierda deja fija la esquina opuesta (inferior derecha)', () => {
    const r = calcular('nw', -50, -40);
    assert.equal(r.x + r.w, INICIO.x + INICIO.w);
    assert.equal(r.y + r.h, INICIO.y + INICIO.h);
    assert.equal(r.w, 850);
    assert.equal(r.h, 640);
});

test('nunca baja del tamano minimo', () => {
    const r = calcular('se', -5000, -5000);
    assert.equal(r.w, MINIMO.w);
    assert.equal(r.h, MINIMO.h);
});

test('minimo por la izquierda: el borde derecho sigue fijo', () => {
    const r = calcular('w', 5000, 0);
    assert.equal(r.w, MINIMO.w);
    assert.equal(r.x + r.w, INICIO.x + INICIO.w);
});

test('no se sale de la pantalla por la derecha ni por abajo', () => {
    const r = calcular('se', 9000, 9000);
    assert.equal(r.x + r.w, PANTALLA.x + PANTALLA.w);
    assert.equal(r.y + r.h, PANTALLA.y + PANTALLA.h);
});

test('no se sale de la pantalla por la izquierda ni por arriba', () => {
    const r = calcular('nw', -9000, -9000);
    assert.equal(r.x, PANTALLA.x);
    assert.equal(r.y, PANTALLA.y);
    assert.equal(r.x + r.w, INICIO.x + INICIO.w);
    assert.equal(r.y + r.h, INICIO.y + INICIO.h);
});

test('pantalla secundaria a la izquierda (x negativo): el limite es el borde de esa pantalla', () => {
    const pantalla = { x: -1920, y: 0, w: 1920, h: 1040 };
    const inicio = { x: -1500, y: 100, w: 800, h: 600 };
    const r = calcularGeometria('w', inicio, -9000, 0, pantalla, MINIMO);
    assert.equal(r.x, -1920);
    assert.equal(r.x + r.w, inicio.x + inicio.w);
});

test('un movimiento vertical no toca el ancho ni la posicion horizontal', () => {
    const r = calcular('s', 300, 40);
    assert.equal(r.x, INICIO.x);
    assert.equal(r.w, INICIO.w);
});

// ---- calcularPosicion: mover la ventana arrastrando la franja de titulo ----
const calcularPosicion = (...args) => ({ ...sandbox.MarcoVentana.calcularPosicion(...args) });
const VISIBLE = 120;

test('mover: sigue al cursor (dx, dy)', () => {
    assert.deepEqual(calcularPosicion(INICIO, 50, -30, PANTALLA, VISIBLE), { x: 450, y: 70 });
});

test('mover: la ventana no sube por encima de la pantalla', () => {
    assert.equal(calcularPosicion(INICIO, 0, -9000, PANTALLA, VISIBLE).y, PANTALLA.y);
});

test('mover: abajo siempre queda a la vista al menos el minimo visible', () => {
    assert.equal(calcularPosicion(INICIO, 0, 9000, PANTALLA, VISIBLE).y, PANTALLA.y + PANTALLA.h - VISIBLE);
});

test('mover: a la derecha y a la izquierda queda a la vista al menos el minimo visible', () => {
    assert.equal(calcularPosicion(INICIO, 9000, 0, PANTALLA, VISIBLE).x, PANTALLA.x + PANTALLA.w - VISIBLE);
    assert.equal(calcularPosicion(INICIO, -9000, 0, PANTALLA, VISIBLE).x, PANTALLA.x - INICIO.w + VISIBLE);
});

test('mover: respeta una pantalla secundaria con x negativo', () => {
    const pantalla = { x: -1920, y: 0, w: 1920, h: 1040 };
    const r = calcularPosicion({ x: -1500, y: 100, w: 800, h: 600 }, -9000, 0, pantalla, VISIBLE);
    assert.equal(r.x, -1920 - 800 + VISIBLE);
});
