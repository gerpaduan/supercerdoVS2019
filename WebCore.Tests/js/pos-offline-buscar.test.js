// Tests de la busqueda offline del POS (WebCore/wwwroot/Scripts/app/pos-offline-buscar.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// El fixture fixtures/pos-offline-barcode-paridad.json lo genera el C# real (WebCore.Tests/PosOfflineBarcodeParidadTests.cs):
// si el JS da otra cosa que el servidor para algun codigo, falla aca.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'pos-offline-buscar.js'), 'utf8');
const FIXTURE = JSON.parse(fs.readFileSync(path.join(__dirname, 'fixtures', 'pos-offline-barcode-paridad.json'), 'utf8'));

// Carga el script en un sandbox con window (como en el navegador) y devuelve window.PosOfflineBuscar.
function cargar() {
    const window = {};
    vm.runInNewContext(SCRIPT, { window, Map, Math, Number, String, Object, parseInt, isNaN });
    return window.PosOfflineBuscar;
}

// Resultado del JS con la misma forma resumida que el fixture.
function resumir(r) {
    if (r.error) return { tipo: 'error' };
    if (r.success === false) {
        return (r.message === 'Código inexistente' || r.message === 'No existe  el código genérico')
            ? { tipo: 'noEncontrado' }
            : { tipo: 'error' };
    }
    return { tipo: 'producto', id: r.id, codigo: r.codigo, precioKg: r.precioKg, cantidadSugerida: r.cantidadSugerida };
}

test('paridad con el servidor: cada codigo del fixture da lo mismo que BuscarProducto', () => {
    const api = cargar();
    for (const caso of FIXTURE.casos) {
        const catalogo = JSON.parse(JSON.stringify(FIXTURE.catalogo));
        const obtenido = JSON.parse(JSON.stringify(resumir(api.buscarProducto(caso.codigo, caso.ingresoCantidadX, catalogo))));
        assert.deepEqual(obtenido, caso.esperado, `codigo ${JSON.stringify(caso.codigo)} (ingresoCantidadX=${caso.ingresoCantidadX})`);
    }
});

test('origen del precio: etiqueta de precio, generico y catalogo', () => {
    const api = cargar();
    const cat = FIXTURE.catalogo;
    const etiquetaPrecio = FIXTURE.casos.find(c => c.codigo.startsWith('21') && c.esperado.tipo === 'producto');
    assert.equal(api.buscarProducto(etiquetaPrecio.codigo, false, cat).origenPrecio, 'etiqueta');
    assert.equal(api.buscarProducto('1500.50G2', true, cat).origenPrecio, 'generico');
    assert.equal(api.buscarProducto('998', false, cat).origenPrecio, 'catalogo');
    // Etiqueta de peso: el precio es el de lista, solo sugiere la cantidad.
    const etiquetaPeso = FIXTURE.casos.find(c => c.esperado.cantidadSugerida);
    assert.equal(api.buscarProducto(etiquetaPeso.codigo, false, cat).origenPrecio, 'catalogo');
});

test('sin catalogo guardado avisa en vez de romper', () => {
    const api = cargar();
    const r = api.buscarProducto('998', false, null);
    assert.equal(r.success, false);
    assert.match(r.message, /catálogo/);
});

test('busqueda por texto (F10): por nombre sin mayusculas, por codigo y sin ceros a la izquierda', () => {
    const api = cargar();
    const cat = FIXTURE.catalogo;
    assert.deepEqual(api.buscarPorTexto('vacío', cat).map(p => p.id), [11]);
    assert.deepEqual(api.buscarPorTexto('ASADO', cat).map(p => p.id), [10]);
    assert.deepEqual(api.buscarPorTexto('0998', cat).map(p => p.id), [10]);
    assert.equal(api.buscarPorTexto('', cat).length, cat.productos.length);
    assert.equal(api.buscarPorTexto('', cat, 2).length, 2);
    // JSON ida y vuelta: el objeto viene de otro realm (vm) y deepStrictEqual compara prototipos.
    const asado = JSON.parse(JSON.stringify(api.buscarPorTexto('asado', cat)[0]));
    assert.deepEqual(asado, { id: 10, codigo: '998', nombre: 'Asado', precio: 2.42 });
});

test('redondeo igual a Math.Round de .NET (al par en el punto medio exacto)', () => {
    const api = cargar();
    assert.equal(api.redondear2(0.125), 0.12);
    assert.equal(api.redondear2(0.375), 0.38);
    assert.equal(api.redondear2(1234.56494140625), 1234.56);
});
