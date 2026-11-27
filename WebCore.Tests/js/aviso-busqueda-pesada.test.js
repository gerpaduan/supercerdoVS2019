// Tests de las funciones puras del aviso de busqueda pesada (WebCore/wwwroot/Scripts/app/aviso-busqueda-pesada.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const path = require('node:path');

const { armarUrl, fechaCorta, textos } = require(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'aviso-busqueda-pesada.js'));

test('armarUrl codifica los filtros y no manda los vacios', () => {
    assert.equal(armarUrl('/Ventas/ContarBusqueda', 'ventas', { fechaDesde: '2026-11-01T00:00', fechaHasta: '2026-11-26T23:59', idSucursal: '' }),
        '/Ventas/ContarBusqueda?pantalla=ventas&fechaDesde=2026-11-01T00%3A00&fechaHasta=2026-11-26T23%3A59');
    assert.equal(armarUrl('/app/Ventas/ContarBusqueda', 'lineas', { idSucursal: '-1' }), '/app/Ventas/ContarBusqueda?pantalla=lineas&idSucursal=-1');
});

test('fechaCorta pasa yyyy-MM-dd (con o sin hora) a dd/mm/yyyy', () => {
    assert.equal(fechaCorta('2026-11-26T00:00:00'), '26/11/2026');
    assert.equal(fechaCorta(''), '');
});

test('textos arma el periodo, la cifra y el cartel de carga', () => {
    const t = textos({ cantidad: 18150, unidad: 'ventas' }, { fechaDesde: '2026-01-01T00:00', fechaHasta: '2026-11-26T23:59' });
    assert.equal(t.periodo, 'Del 01/01/2026 al 26/11/2026 hay');
    assert.equal(t.cifra, '18.150 ventas');
    assert.match(t.carga, /^Cargando 18\.150 ventas\./);
});

test('textos sin fechas no inventa un periodo y limpia la unidad', () => {
    const t = textos({ cantidad: '5000', unidad: '<b>líneas</b>' }, {});
    assert.equal(t.periodo, 'La búsqueda trae');
    assert.equal(t.cifra, '5.000 blíneas/b');
});
