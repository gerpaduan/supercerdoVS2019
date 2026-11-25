// Tests del resumen de los filtros plegados en celular (WebCore/wwwroot/Scripts/app/filtros-celular.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');

const { armarResumen, fechaCorta } = require(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'filtros-celular.js'));

const select = (nombre, texto) => ({ tipo: 'select', nombre, valor: '1', texto });
const fecha = (nombre, valor, tipo = 'date') => ({ tipo, nombre, valor, texto: '' });

test('fechaCorta pasa yyyy-MM-dd (con o sin hora) a dd/mm', () => {
    assert.equal(fechaCorta('2026-11-25'), '25/11');
    assert.equal(fechaCorta('2026-11-01T08:30:00'), '01/11');
    assert.equal(fechaCorta(''), '');
});

test('junta desde y hasta en un tramo, en el lugar de la primera fecha', () => {
    const resumen = armarResumen([select('idSucursal', 'San Lorenzo'), fecha('fechaDesde', '2026-11-01'), fecha('fechaHasta', '2026-11-25'), select('tipoCompra', 'Ingreso')]);
    assert.equal(resumen, 'San Lorenzo · 01/11 al 25/11 · Ingreso');
});

test('mismo dia se muestra una vez; una sola fecha dice desde o hasta', () => {
    assert.equal(armarResumen([fecha('fechaDesde', '2026-11-25T00:00:00', 'datetime-local'), fecha('fechaHasta', '2026-11-25T23:59:59', 'datetime-local')]), '25/11');
    assert.equal(armarResumen([fecha('fechaDesde', '2026-11-25')]), 'desde 25/11');
    assert.equal(armarResumen([fecha('fechaHasta', '2026-11-25')]), 'hasta 25/11');
});

test('omite los valores que no filtran y los textos vacios', () => {
    const resumen = armarResumen([select('idSucursal', 'Todas'), select('tipo', 'Ver Todos'), { tipo: 'text', nombre: 'texto', valor: '   ' }, { tipo: 'text', nombre: 'proveedor', valor: 'Frigorífico Sur' }]);
    assert.equal(resumen, 'Frigorífico Sur');
});

test('sin nada elegido avisa que no hay filtros', () => {
    assert.equal(armarResumen([select('idSucursal', 'Todas')]), 'Sin filtros');
    assert.equal(armarResumen([]), 'Sin filtros');
});
