// Tests de los anchos de columna de Existencia por sucursales (WebCore/wwwroot/Scripts/app/existencia-anchos.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// El script se carga en un sandbox (vm) sin document: solo expone funciones puras (topes, leer/guardar lo de localStorage).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'existencia-anchos.js'), 'utf8');
const sandbox = {};
sandbox.window = sandbox;
vm.createContext(sandbox);
vm.runInContext(SCRIPT, sandbox);
const A = sandbox.ExistenciaAnchos;
// Los objetos vienen de otro "realm" (vm): se pasan por JSON para que deepEqual los compare bien.
const plano = (valor) => JSON.parse(JSON.stringify(valor));

test('las columnas redimensionables son Cod., Producto, Stock y Estado', () => {
    assert.deepEqual(Object.keys(A.COLUMNAS).sort(), ['cod', 'estado', 'prod', 'stock']);
});

test('limitar recorta a los topes de la columna y redondea a px enteros', () => {
    assert.equal(A.limitar('cod', 5), A.COLUMNAS.cod.min);
    assert.equal(A.limitar('cod', 99999), A.COLUMNAS.cod.max);
    assert.equal(A.limitar('prod', 180.6), 181);
    assert.equal(A.limitar('stock', '90'), 90);
});

test('limitar rechaza columnas desconocidas y valores que no son numeros', () => {
    assert.equal(A.limitar('precio', 100), null);
    assert.equal(A.limitar('cod', 'abc'), null);
    assert.equal(A.limitar('cod', null), null);
    assert.equal(A.limitar('cod', ''), null);
    assert.equal(A.limitar('cod', true), null);
    assert.equal(A.limitar('cod', Infinity), null);
    assert.equal(A.limitar('cod', NaN), null);
});

test('limitar no acepta propiedades heredadas como columnas', () => {
    assert.equal(A.limitar('toString', 100), null);
    assert.equal(A.esColumna('__proto__'), false);
});

test('leer devuelve los anchos validos de lo guardado', () => {
    assert.deepEqual(plano(A.leer('{"cod":70,"stock":120}')), { cod: 70, stock: 120 });
});

test('leer descarta columnas desconocidas, valores invalidos y recorta los fuera de rango', () => {
    const r = plano(A.leer('{"cod":"x","prod":5,"estado":99999,"precio":80}'));
    assert.deepEqual(r, { prod: A.COLUMNAS.prod.min, estado: A.COLUMNAS.estado.max });
});

test('leer tolera lo guardado roto o vacio y devuelve un objeto vacio', () => {
    for (const texto of [null, undefined, '', '{roto', '[]', '123', 'null', '"texto"']) {
        assert.deepEqual(plano(A.leer(texto)), {}, 'entrada: ' + String(texto));
    }
});

test('serializar guarda solo lo valido y devuelve null si no hay anchos cambiados', () => {
    assert.equal(A.serializar({}), null);
    assert.equal(A.serializar(null), null);
    assert.equal(A.serializar({ precio: 100 }), null);
    assert.deepEqual(JSON.parse(A.serializar({ cod: 70.4, otra: 1 })), { cod: 70 });
});

test('leer(serializar(x)) conserva los anchos', () => {
    const anchos = { cod: 64, prod: 220, stock: 110, estado: 95 };
    assert.deepEqual(plano(A.leer(A.serializar(anchos))), anchos);
});

test('anchoArrastrado suma el desplazamiento al ancho inicial dentro de los topes', () => {
    assert.equal(A.anchoArrastrado('stock', 100, 25), 125);
    assert.equal(A.anchoArrastrado('stock', 100, -90), A.COLUMNAS.stock.min);
    assert.equal(A.anchoArrastrado('prod', 200, 5000), A.COLUMNAS.prod.max);
});

test('cada modo recuerda sus anchos con su propia clave; un modo desconocido usa la de la pantalla normal', () => {
    const normal = A.claveAlmacen('normal');
    const popup = A.claveAlmacen('popup');
    const mov = A.claveAlmacen('movimiento');
    assert.equal(new Set([normal, popup, mov]).size, 3);
    assert.equal(A.claveAlmacen('otro'), normal);
    assert.equal(A.claveAlmacen(undefined), normal);
});
