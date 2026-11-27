// Tests de las funciones puras del selector de columnas (WebCore/wwwroot/Scripts/app/columnas-visibles.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');

const { leerOcultas, ocultasControl, mismasClaves, textoBoton, esCompacta, reglas } =
    require(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'columnas-visibles.js'));

const col = (clave, control = false, fija = false) => ({ clave, control, fija });
const columnas = [col('fecha', true), col('sucursal'), col('vendedor'), col('codigo', true), col('producto', true, true), col('kg', true), col('precio')];

test('leerOcultas ignora basura, claves desconocidas y columnas fijas', () => {
    assert.deepEqual(leerOcultas('["vendedor","producto","noexiste","sucursal"]', columnas), ['sucursal', 'vendedor']);
    assert.deepEqual(leerOcultas(null, columnas), []);
    assert.deepEqual(leerOcultas('{roto', columnas), []);
    assert.deepEqual(leerOcultas('"sucursal"', columnas), []);
});

test('Control deja fecha, codigo, producto y cantidad', () => {
    assert.deepEqual(ocultasControl(columnas), ['sucursal', 'vendedor', 'precio']);
    assert.equal(mismasClaves(['precio', 'sucursal', 'vendedor'], ocultasControl(columnas)), true);
    assert.equal(mismasClaves(['sucursal'], ocultasControl(columnas)), false);
});

test('el boton dice cuantas columnas hay ocultas', () => {
    assert.equal(textoBoton(0), 'Columnas');
    assert.equal(textoBoton(1), 'Columnas (1 oculta)');
    assert.equal(textoBoton(8), 'Columnas (8 ocultas)');
});

test('la tabla se compacta con 7 columnas visibles o menos', () => {
    assert.equal(esCompacta(12, 5), true);
    assert.equal(esCompacta(12, 4), false);
});

test('reglas oculta por clase y no deja escapar del selector', () => {
    assert.equal(reglas('.tabla', ['nro-comprobante']), '.tabla .lc-nro-comprobante { display: none !important; }');
    assert.equal(reglas('.tabla', ['x{}body']), '.tabla .lc-xbody { display: none !important; }');
    assert.equal(reglas('.tabla', []), '');
});
