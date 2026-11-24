// Tests de que hace la ventana independiente al guardar (WebCore/wwwroot/Scripts/app/ventana-trabajo.js).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// 2026-11-24: si la ventana ARRANCO en el listado (Index) de Elaborados / Movimientos / Stock, al guardar vuelve al listado y no se cierra; si se abrio
// directo en un formulario, se cierra como antes (2026-11-13 c). El script se carga en un sandbox (vm) por cada "pagina" que carga la ventana, con un
// sessionStorage compartido entre cargas (igual que en el navegador: sobrevive a la navegacion dentro de la ventana).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'ventana-trabajo.js'), 'utf8');

function crearSessionStorage() {
    const datos = new Map();
    return {
        getItem: (k) => (datos.has(k) ? datos.get(k) : null),
        setItem: (k, v) => datos.set(k, String(v)),
        removeItem: (k) => datos.delete(k),
    };
}

// Carga una "pagina" de la ventana: devuelve el sandbox, lo que paso con close() y los listeners de documento.
function cargarPagina({ ruta, nombre = 'carnisys-trabajo-menu-elaborados', modoVentana = true, sesion }) {
    const registro = { cerrada: false, timeouts: [] };
    const oyentes = {};
    const sandbox = {
        name: nombre,
        location: { pathname: ruta, origin: 'http://localhost' },
        sessionStorage: sesion,
        document: {
            documentElement: { classList: { contains: (c) => c === 'modo-ventana' && modoVentana } },
            addEventListener: (tipo, fn) => { oyentes[tipo] = fn; },
        },
        close: () => { registro.cerrada = true; },
        setTimeout: (fn) => { registro.timeouts.push(fn); },
        desactivarProteccionSalida: () => {},
    };
    sandbox.window = sandbox;
    sandbox.parent = sandbox;   // ventana de primer nivel (no iframe del POS)
    vm.createContext(sandbox);
    vm.runInContext(SCRIPT, sandbox);
    return { api: sandbox.VentanaTrabajo, registro, oyentes };
}

test('esListado: Index de Elaborados, Movimientos y Stock (con o sin PathBase); los formularios no', () => {
    const { api } = cargarPagina({ ruta: '/Elaborados', sesion: crearSessionStorage() });
    for (const r of ['/Elaborados', '/Elaborados/', '/Elaborados/Index', '/Movimientos', '/Stock/Index', '/CarniSysWeb/Movimientos']) {
        assert.equal(api.esListado(r), true, r);
    }
    for (const r of ['/Elaborados/Carga', '/Movimientos/Editar/5', '/Stock/Editar', '/Elaborados/Formulas', '/Home/Index']) {
        assert.equal(api.esListado(r), false, r);
    }
});

test('ventana que arranco en el listado: Nuevo -> guardar vuelve al listado y NO se cierra', () => {
    const sesion = crearSessionStorage();
    cargarPagina({ ruta: '/Elaborados', sesion });                         // 1ra pagina: el listado
    const { api, registro } = cargarPagina({ ruta: '/Elaborados/Carga', sesion });   // «Nuevo elaborado»
    let fueAlListado = false;
    assert.equal(api.cerrarVentana(() => { fueAlListado = true; }), true);
    assert.equal(fueAlListado, true);
    assert.equal(registro.cerrada, false);
});

test('ventana que se abrio directo en el formulario: al guardar se cierra (como antes)', () => {
    const sesion = crearSessionStorage();
    const { api, registro } = cargarPagina({ ruta: '/Movimientos/Editar', sesion });
    let fueAlListado = false;
    assert.equal(api.cerrarVentana(() => { fueAlListado = true; }), true);
    assert.equal(registro.cerrada, true);
    assert.equal(fueAlListado, false);          // la alternativa queda en espera por si el navegador no la deja cerrar
    assert.equal(registro.timeouts.length, 1);
});

test('el origen lo fija la PRIMERA pagina: volver al listado despues no lo cambia', () => {
    const sesion = crearSessionStorage();
    cargarPagina({ ruta: '/Movimientos/Editar', sesion });
    cargarPagina({ ruta: '/Movimientos', sesion });
    const { api } = cargarPagina({ ruta: '/Movimientos/Editar', sesion });
    assert.equal(api.abrioDesdeListado(), false);
});

test('cada ventana / panel tiene su origen aunque compartan sessionStorage (paneles de la pantalla dividida)', () => {
    const sesion = crearSessionStorage();
    cargarPagina({ ruta: '/Elaborados', nombre: 'carnisys-trabajo-panel-1', sesion });
    const panel2 = cargarPagina({ ruta: '/Elaborados/Carga', nombre: 'carnisys-trabajo-panel-2', sesion });
    const panel1 = cargarPagina({ ruta: '/Elaborados/Carga', nombre: 'carnisys-trabajo-panel-1', sesion });
    assert.equal(panel1.api.abrioDesdeListado(), true);
    assert.equal(panel2.api.abrioDesdeListado(), false);
});

test('Stock arrancada en el listado: al cerrarse el aviso de guardado la ventana queda abierta en el listado', () => {
    const sesion = crearSessionStorage();
    cargarPagina({ ruta: '/Stock', sesion });
    const editar = cargarPagina({ ruta: '/Stock/Editar', sesion });
    editar.api.marcarGuardado();
    const listado = cargarPagina({ ruta: '/Stock', sesion });
    listado.oyentes.layoutAlertClosed();
    assert.equal(listado.registro.cerrada, false);
});

test('Stock abierta directo en el formulario: al cerrarse el aviso se cierra (como antes)', () => {
    const sesion = crearSessionStorage();
    const editar = cargarPagina({ ruta: '/Stock/Editar', sesion });
    editar.api.marcarGuardado();
    const listado = cargarPagina({ ruta: '/Stock', sesion });
    listado.oyentes.layoutAlertClosed();
    assert.equal(listado.registro.cerrada, true);
});

test('fuera del modo ventana (pestaña normal) no hace nada: cerrarVentana devuelve false', () => {
    const { api, registro } = cargarPagina({ ruta: '/Elaborados/Carga', nombre: '', modoVentana: false, sesion: crearSessionStorage() });
    assert.equal(api.cerrarVentana(() => {}), false);
    assert.equal(registro.cerrada, false);
});
