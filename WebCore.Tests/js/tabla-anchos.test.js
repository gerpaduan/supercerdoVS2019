// Tests de la logica pura de columnas redimensionables (WebCore/wwwroot/Scripts/app/tabla-anchos.js), usada por Reportes.
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// El script se carga en un sandbox (vm) sin document: solo se prueban crear() y columnasLogicas() (activar necesita el navegador).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'tabla-anchos.js'), 'utf8');
const sandbox = {};
sandbox.window = sandbox;
vm.createContext(sandbox);
vm.runInContext(SCRIPT, sandbox);
const T = sandbox.TablaAnchos;
// Los objetos vienen de otro "realm" (vm): se pasan por JSON para que deepEqual los compare bien.
const plano = (valor) => JSON.parse(JSON.stringify(valor));

const COLS = { producto: { min: 80, max: 600 }, stock: { min: 50, max: 400 } };

test('crear descarta tipos con topes invalidos', () => {
    const m = T.crear({ ok: { min: 10, max: 20 }, sinMax: { min: 10 }, alReves: { min: 30, max: 20 }, cero: { min: 0, max: 5 } });
    assert.deepEqual(Object.keys(m.columnas), ['ok']);
});

test('limitar recorta a los topes del tipo y redondea', () => {
    const m = T.crear(COLS);
    assert.equal(m.limitar('stock', 10), 50);
    assert.equal(m.limitar('stock', 9999), 400);
    assert.equal(m.limitar('producto', 180.6), 181);
    assert.equal(m.limitar('stock', '90'), 90);
});

test('limitar rechaza tipos desconocidos, heredados y valores que no son numeros', () => {
    const m = T.crear(COLS);
    for (const v of ['abc', null, undefined, '', true, Infinity, NaN]) assert.equal(m.limitar('stock', v), null, String(v));
    assert.equal(m.limitar('precio', 100), null);
    assert.equal(m.limitar('toString', 100), null);
    assert.equal(m.esColumna('__proto__'), false);
});

test('el minimo medido sube el piso para que un numero no quede cortado, sin pasar el maximo', () => {
    const m = T.crear(COLS);
    assert.equal(m.limitar('stock', 60, 112.4), 112);
    assert.equal(m.limitar('stock', 200, 112), 200);
    assert.equal(m.limitar('stock', 60, 9999), 400);
    assert.equal(m.limitar('stock', 60, 'x'), 60);
});

test('leer devuelve los anchos validos y descarta lo demas', () => {
    const m = T.crear(COLS);
    assert.deepEqual(plano(m.leer('{"producto":200,"stock":"x","otra":5}')), { producto: 200 });
    assert.deepEqual(plano(m.leer('{"stock":1}')), { stock: 50 });
});

test('leer tolera lo guardado roto o vacio', () => {
    const m = T.crear(COLS);
    for (const texto of [null, undefined, '', '{roto', '[]', '123', 'null', '"texto"']) {
        assert.deepEqual(plano(m.leer(texto)), {}, 'entrada: ' + String(texto));
    }
});

test('serializar devuelve null sin anchos cambiados y leer(serializar(x)) conserva los anchos', () => {
    const m = T.crear(COLS);
    assert.equal(m.serializar({}), null);
    assert.equal(m.serializar(null), null);
    const anchos = { producto: 220, stock: 110 };
    assert.deepEqual(plano(m.leer(m.serializar(anchos))), anchos);
});

test('anchoArrastrado suma el desplazamiento respetando topes y minimo medido', () => {
    const m = T.crear(COLS);
    assert.equal(m.anchoArrastrado('stock', 100, 25), 125);
    assert.equal(m.anchoArrastrado('stock', 100, -90), 50);
    assert.equal(m.anchoArrastrado('stock', 100, -90, 95), 95);
});

test('columnasLogicas: encabezado de una fila es el indice de cada celda', () => {
    const fila = [{ colSpan: 1, rowSpan: 1 }, { colSpan: 1, rowSpan: 1 }, { colSpan: 1, rowSpan: 1 }];
    assert.deepEqual(plano(T.columnasLogicas([fila])), [[0, 1, 2]]);
});

test('columnasLogicas: encabezado de dos filas con rowspan y colspan (Proyeccion con varias sucursales)', () => {
    // Fila 1: Producto (rowspan 2) | Suc A (colspan 3) | Suc B (colspan 3) | Total (rowspan 2)
    // Fila 2: Stock | Ventas | Dif | Stock | Ventas | Dif
    const f1 = [{ colSpan: 1, rowSpan: 2 }, { colSpan: 3, rowSpan: 1 }, { colSpan: 3, rowSpan: 1 }, { colSpan: 1, rowSpan: 2 }];
    const f2 = Array.from({ length: 6 }, () => ({ colSpan: 1, rowSpan: 1 }));
    assert.deepEqual(plano(T.columnasLogicas([f1, f2])), [[0, 1, 4, 7], [1, 2, 3, 4, 5, 6]]);
});
