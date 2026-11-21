// Tests del modal de seleccion de usuario en modo obligatorio (WebCore/wwwroot/Scripts/app/seleccion-usuario.js, 2026-11-21):
// en el POS / Expendio sin operador resuelto el modal no tiene X ni «Cancelar» (antes, cerrarlo dejaba la pantalla en blanco en la app instalada).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// El script se carga en un sandbox (vm) con un jQuery falso que registra las llamadas: abrir() se prueba por lo que le hace al DOM del modal.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'seleccion-usuario.js'), 'utf8');

// jQuery falso: $(selector) devuelve un objeto que guarda toggleClass/attr por selector y acepta (sin hacer nada) el resto de los metodos encadenados.
function crearSandbox(appInstalada) {
    const estado = {}; // selector -> { ocultoD: bool|undefined, href }
    function envolver(selector) {
        const reg = (estado[selector] = estado[selector] || {});
        const obj = new Proxy(function () {}, {
            get(_, nombre) {
                if (nombre === 'toggleClass') return (clase, forzar) => { if (clase === 'd-none') reg.oculto = !!forzar; return obj; };
                if (nombre === 'attr') return (k, v) => { if (v !== undefined) reg[k] = v; return obj; };
                if (nombre === 'modal') return () => obj;
                if (nombre === 'data') return () => obj;
                return () => obj; // addClass, removeClass, empty, val, text, prop, off, on, trigger, etc.
            },
        });
        return obj;
    }
    const jq = (selector) => envolver(typeof selector === 'string' ? selector : '(documento)');
    jq.extend = Object.assign;
    jq.ajax = () => {};
    const sandbox = {
        jQuery: jq,
        document: { addEventListener: () => {} },
        setTimeout, clearTimeout, setInterval, clearInterval,
        AppInstalada: appInstalada,
    };
    sandbox.window = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(SCRIPT, sandbox);
    return { sandbox, estado };
}

test('salidasDelModal: no obligatorio muestra X y Cancelar y nunca el enlace', () => {
    const { sandbox } = crearSandbox(undefined);
    const s = { ...sandbox.SeleccionUsuario.salidasDelModal(false, '/Home/Index', false) };
    assert.deepEqual(s, { mostrarCerrar: true, mostrarCancelar: true, mostrarIrInicio: false });
});

test('salidasDelModal: obligatorio en pestana normal oculta X y Cancelar y deja «Ir a Inicio»', () => {
    const { sandbox } = crearSandbox(undefined);
    const s = { ...sandbox.SeleccionUsuario.salidasDelModal(true, '/Home/Index', false) };
    assert.deepEqual(s, { mostrarCerrar: false, mostrarCancelar: false, mostrarIrInicio: true });
});

test('salidasDelModal: obligatorio en la app instalada no deja ninguna salida dentro del modal', () => {
    const { sandbox } = crearSandbox(undefined);
    const s = { ...sandbox.SeleccionUsuario.salidasDelModal(true, '/Home/Index', true) };
    assert.deepEqual(s, { mostrarCerrar: false, mostrarCancelar: false, mostrarIrInicio: false });
});

test('salidasDelModal: obligatorio sin urlInicio no muestra el enlace', () => {
    const { sandbox } = crearSandbox(undefined);
    assert.equal(sandbox.SeleccionUsuario.salidasDelModal(true, '', false).mostrarIrInicio, false);
    assert.equal(sandbox.SeleccionUsuario.salidasDelModal(true, undefined, false).mostrarIrInicio, false);
});

test('abrir obligatorio en la app instalada: oculta X y Cancelar y el enlace', () => {
    const { sandbox, estado } = crearSandbox({ activa: true });
    sandbox.SeleccionUsuario.abrir({ obligatorio: true, urlInicio: '/Home/Index', usuarios: [] });
    assert.equal(estado['#btnCerrarSeleccionUsuario'].oculto, true);
    assert.equal(estado['#btnCancelarSeleccionUsuario'].oculto, true);
    assert.equal(estado['#lnkIrInicioSeleccionUsuario'].oculto, true);
});

test('abrir obligatorio en pestana normal: oculta X y Cancelar y muestra «Ir a Inicio» con su direccion', () => {
    const { sandbox, estado } = crearSandbox({ activa: false });
    sandbox.SeleccionUsuario.abrir({ obligatorio: true, urlInicio: '/CarniSysWeb/Home/Index', usuarios: [] });
    assert.equal(estado['#btnCerrarSeleccionUsuario'].oculto, true);
    assert.equal(estado['#btnCancelarSeleccionUsuario'].oculto, true);
    assert.equal(estado['#lnkIrInicioSeleccionUsuario'].oculto, false);
    assert.equal(estado['#lnkIrInicioSeleccionUsuario'].href, '/CarniSysWeb/Home/Index');
});

test('abrir no obligatorio despues de uno obligatorio (Cambiar operario): vuelven la X y Cancelar', () => {
    const { sandbox, estado } = crearSandbox({ activa: true });
    sandbox.SeleccionUsuario.abrir({ obligatorio: true, urlInicio: '/Home/Index', usuarios: [] });
    sandbox.SeleccionUsuario.abrir({ usuarios: [] });
    assert.equal(estado['#btnCerrarSeleccionUsuario'].oculto, false);
    assert.equal(estado['#btnCancelarSeleccionUsuario'].oculto, false);
    assert.equal(estado['#lnkIrInicioSeleccionUsuario'].oculto, true);
});
