// Tests del canal entre la pestaña de la pantalla dividida y su ventana independiente (WebCore/wwwroot/Scripts/app/pantalla-dividida-canal.js, 2026-11-24).
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js/*.test.js
// El script se carga en un sandbox (vm) con un BroadcastChannel falso: todos los canales del mismo nombre se reparten los mensajes, menos al que envia
// (igual que el del navegador).
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'app', 'pantalla-dividida-canal.js'), 'utf8');

// BroadcastChannel falso: un "bus" por nombre; postMessage entrega (asincronico, como el real) a los demas canales con ese nombre.
function crearBroadcastChannelFalso() {
    const buses = {};
    return class CanalFalso {
        constructor(nombre) {
            this.nombre = nombre;
            this.onmessage = null;
            (buses[nombre] = buses[nombre] || []).push(this);
        }
        postMessage(data) {
            const copia = JSON.parse(JSON.stringify(data));   // structured clone aproximado
            buses[this.nombre].filter((c) => c !== this).forEach((c) => setImmediate(() => c.onmessage && c.onmessage({ data: copia })));
        }
    };
}

function cargar() {
    const sandbox = { setTimeout, clearTimeout, console, Promise, Object, Math, Date, JSON, String };
    sandbox.window = sandbox;
    vm.createContext(sandbox);
    vm.runInContext(SCRIPT, sandbox);
    return sandbox.PantallaDivididaCanal;
}

test('sin BroadcastChannel el canal no esta disponible y pedir responde null', async () => {
    const Canal = cargar();
    const canal = Canal.crear('7', { BroadcastChannel: undefined });
    assert.equal(canal.disponible, false);
    assert.equal(canal.enviar({ tipo: Canal.TIPOS.PING }), null);
    assert.equal(await canal.pedir({ tipo: Canal.TIPOS.PING }, 20), null);
});

test('pedir recibe la respuesta del otro lado (ping -> aca)', async () => {
    const Canal = cargar();
    const BC = crearBroadcastChannelFalso();
    const pestana = Canal.crear('7', { BroadcastChannel: BC });
    const ventana = Canal.crear('7', { BroadcastChannel: BC });
    ventana.escuchar((m) => { if (m.tipo === Canal.TIPOS.PING) ventana.responder(m, { tipo: Canal.TIPOS.ACA }); });
    const respuesta = await pestana.pedir({ tipo: Canal.TIPOS.PING }, 500);
    assert.equal(respuesta.tipo, Canal.TIPOS.ACA);
});

test('sin nadie del otro lado, pedir vence y responde null', async () => {
    const Canal = cargar();
    const BC = crearBroadcastChannelFalso();
    const pestana = Canal.crear('7', { BroadcastChannel: BC });
    assert.equal(await pestana.pedir({ tipo: Canal.TIPOS.PING }, 30), null);
});

test('la respuesta va solo a quien pregunto: no se reparte a los oyentes', async () => {
    const Canal = cargar();
    const BC = crearBroadcastChannelFalso();
    const pestana = Canal.crear('7', { BroadcastChannel: BC });
    const ventana = Canal.crear('7', { BroadcastChannel: BC });
    const vistosPorLaPestana = [];
    pestana.escuchar((m) => vistosPorLaPestana.push(m.tipo));
    ventana.escuchar((m) => { if (m.tipo === Canal.TIPOS.DEVOLVER) ventana.responder(m, { tipo: Canal.TIPOS.DEVOLVIENDO, pideConfirmar: true }); });
    const respuesta = await pestana.pedir({ tipo: Canal.TIPOS.DEVOLVER }, 500);
    assert.equal(respuesta.pideConfirmar, true);
    assert.deepEqual(vistosPorLaPestana, []);
});

test('otro usuario usa otro canal: no contesta', async () => {
    const Canal = cargar();
    const BC = crearBroadcastChannelFalso();
    const pestana = Canal.crear('7', { BroadcastChannel: BC });
    const ventanaDeOtro = Canal.crear('8', { BroadcastChannel: BC });
    ventanaDeOtro.escuchar((m) => ventanaDeOtro.responder(m, { tipo: Canal.TIPOS.ACA }));
    assert.equal(await pestana.pedir({ tipo: Canal.TIPOS.PING }, 50), null);
});

test('un mensaje sin tipo se ignora y un oyente que falla no corta a los demas', async () => {
    const Canal = cargar();
    const BC = crearBroadcastChannelFalso();
    const pestana = Canal.crear('7', { BroadcastChannel: BC });
    const ventana = Canal.crear('7', { BroadcastChannel: BC });
    const recibidos = [];
    const consolaOriginal = console.warn;
    console.warn = () => {};
    try {
        ventana.escuchar(() => { throw new Error('falla a proposito'); });
        ventana.escuchar((m) => recibidos.push(m.tipo));
        new BC('carnisys-pantalla-dividida-7').postMessage({ sinTipo: true });
        pestana.enviar({ tipo: Canal.TIPOS.CERRADA });
        await new Promise((r) => setTimeout(r, 20));
    } finally {
        console.warn = consolaOriginal;
    }
    assert.deepEqual(recibidos, [Canal.TIPOS.CERRADA]);
});
