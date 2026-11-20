// Tests del cliente de balanza (WebCore/wwwroot/Scripts/carnisys.balanza.js): un peso viejo nunca debe mostrarse ni guardarse.
// Corren con el runner de Node, sin dependencias:  node --test WebCore.Tests/js
// El script se ejecuta en un sandbox (vm) con un window y un XMLHttpRequest falsos que simulan al agente local.
'use strict';

const test = require('node:test');
const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');

const SCRIPT = fs.readFileSync(path.join(__dirname, '..', '..', 'WebCore', 'wwwroot', 'Scripts', 'carnisys.balanza.js'), 'utf8');

// Arma un entorno nuevo: reloj controlable, timers manuales y un agente falso que responde lo que diga `agente.responder`.
function crearEntorno() {
    const reloj = { ahora: new Date(2026, 10, 20, 10, 0, 0).getTime() };
    const pendientes = [];
    const agente = {
        pedidos: [],
        // Por defecto: lectura valida recien hecha (formato del agente nuevo).
        responder: (url) => ({ ok: true, conectada: true, peso: 2, pesoTexto: '2.000', pesoDisplay: '2.000', inestable: false, estable: true, edadMs: 50, secuencia: 1, fechaHora: fechaAgente(reloj.ahora), error: null })
    };

    class FakeXhr {
        open(method, url) { this.url = url; }
        setRequestHeader() { }
        send() {
            agente.pedidos.push(this.url);
            pendientes.push(this);
        }
        // Completa este pedido con la respuesta del agente (o con un error de red si responder devuelve 'colgar'/'error').
        completar() {
            const respuesta = agente.responder(this.url);
            if (respuesta === 'colgar') { this.ontimeout && this.ontimeout(); return; }
            this.readyState = 4;
            this.status = 200;
            this.responseText = JSON.stringify(respuesta);
            this.onreadystatechange();
        }
    }

    class FakeDate extends Date {
        constructor(...args) { if (args.length) super(...args); else super(reloj.ahora); }
        static now() { return reloj.ahora; }
    }

    const intervalos = [];
    const window = {
        XMLHttpRequest: FakeXhr,
        setInterval: (fn) => { intervalos.push(fn); return intervalos.length; },
        clearInterval: () => { }
    };
    const contexto = vm.createContext({ window, XMLHttpRequest: FakeXhr, Date: FakeDate, Promise, JSON, Math, String, Object, parseInt, parseFloat, isNaN, Error });
    vm.runInContext(SCRIPT, contexto);

    // Resuelve todos los pedidos en vuelo y deja correr las promesas.
    async function responderTodo() {
        while (pendientes.length) pendientes.shift().completar();
        await new Promise((r) => setImmediate(r));
    }

    return { window, reloj, agente, pendientes, intervalos, responderTodo, api: window.CarnisysBalanza, utils: window.CarnisysBalanzaUtils };
}

function fechaAgente(ms) {
    const d = new Date(ms);
    const p = (n) => String(n).padStart(2, '0');
    return d.getFullYear() + '-' + p(d.getMonth() + 1) + '-' + p(d.getDate()) + 'T' + p(d.getHours()) + ':' + p(d.getMinutes()) + ':' + p(d.getSeconds());
}

async function arrancarConLectura(env, opciones) {
    const recibidos = [];
    env.api.start(Object.assign({ onPeso: (d) => recibidos.push(d), onStatus: () => { }, onError: () => { } }, opciones || {}));
    env.api.activar();
    await env.responderTodo();
    return recibidos;
}

test('lectura valida fresca queda vigente y se puede agregar', async () => {
    const env = crearEntorno();
    await arrancarConLectura(env);
    const vigente = env.api.lecturaVigente();
    assert.ok(vigente);
    assert.equal(vigente.peso, 2);
    assert.equal(env.api.validarParaAgregar().ok, true);
});

test('el peso viejo deja de estar vigente si no llegan lecturas nuevas (agente colgado)', async () => {
    const env = crearEntorno();
    await arrancarConLectura(env);
    env.reloj.ahora += 1600;
    assert.equal(env.api.lecturaVigente(), null);
    const chequeo = env.api.validarParaAgregar();
    assert.equal(chequeo.ok, false);
    assert.equal(chequeo.motivo, 'sinLectura');
});

test('agente nuevo con edadMs alto: la lectura congelada se marca vencida y no conectada', () => {
    const env = crearEntorno();
    const n = env.utils.normalize({ ok: true, conectada: true, peso: 2, pesoTexto: '2.000', edadMs: 5000, fechaHora: fechaAgente(env.reloj.ahora) });
    assert.equal(n.vencida, true);
    assert.equal(n.conectada, false);
    assert.equal(n.valida, false);
    assert.equal(n.pesoDisplay, 'Sin lectura');
});

test('agente viejo (sin edadMs): se estima la edad con fechaHora, con margen por el truncado al segundo', () => {
    const env = crearEntorno();
    const hace = (ms) => env.utils.normalize({ ok: true, conectada: true, peso: 2, pesoTexto: '2.000', fechaHora: fechaAgente(env.reloj.ahora - ms) });
    assert.equal(hace(0).vencida, false);
    assert.equal(hace(2000).vencida, false, 'hasta 2.5 s con agente viejo no se corta (evita parpadeo con balanza sana)');
    assert.equal(hace(4000).vencida, true);
});

test('la lectura siempre igual del agente viejo trabado vence al pasar el tiempo', async () => {
    const env = crearEntorno();
    const fija = fechaAgente(env.reloj.ahora);
    env.agente.responder = () => ({ ok: true, conectada: true, peso: 2, pesoTexto: '2.000', pesoDisplay: '2.000', fechaHora: fija });
    await arrancarConLectura(env);
    assert.ok(env.api.lecturaVigente());
    // Pasan 5 s; el agente sigue respondiendo la misma lectura (misma fechaHora): ya no es valida.
    env.reloj.ahora += 5000;
    env.intervalos[1]();
    await env.responderTodo();
    assert.equal(env.api.lecturaVigente(), null);
});

test('peso inestable no se puede agregar', async () => {
    const env = crearEntorno();
    env.agente.responder = () => ({ ok: true, conectada: true, peso: 4, pesoTexto: '4.000', pesoDisplay: '4.000 i', inestable: true, edadMs: 10 });
    await arrancarConLectura(env);
    const chequeo = env.api.validarParaAgregar();
    assert.equal(chequeo.ok, false);
    assert.equal(chequeo.motivo, 'inestable');
});

test('peso cero no se puede agregar', async () => {
    const env = crearEntorno();
    env.agente.responder = () => ({ ok: true, conectada: true, peso: 0, pesoTexto: '0.000', pesoDisplay: '0.000', edadMs: 10 });
    await arrancarConLectura(env);
    assert.equal(env.api.validarParaAgregar().motivo, 'pesoCero');
});

test('una lectura con error del agente no reemplaza a la ultima valida ni cuenta como peso', async () => {
    const env = crearEntorno();
    await arrancarConLectura(env);
    env.agente.responder = () => ({ ok: false, conectada: true, peso: 0, pesoTexto: '0.000', pesoDisplay: '0.000', edadMs: 10, error: 'Lectura Systel incompleta.' });
    env.reloj.ahora += 300;
    env.intervalos[1]();
    await env.responderTodo();
    assert.equal(env.api.lecturaVigente().peso, 2, 'dentro de la vigencia sigue la ultima valida (sin parpadeo)');
    env.reloj.ahora += 1500;
    assert.equal(env.api.lecturaVigente(), null, 'pasada la vigencia no queda ningun peso');
});

test('sin respuesta del agente: la pantalla recibe una lectura no conectada (antes solo onError)', async () => {
    const env = crearEntorno();
    const recibidos = await arrancarConLectura(env);
    env.agente.responder = () => 'colgar';
    env.intervalos[1]();
    await env.responderTodo();
    const ultimo = recibidos[recibidos.length - 1];
    assert.equal(ultimo.conectada, false);
    assert.equal(ultimo.sinRespuesta, true);
});

test('no se apilan pedidos: con uno en vuelo el timer no manda otro', async () => {
    const env = crearEntorno();
    await arrancarConLectura(env);
    const antes = env.agente.pedidos.length;
    env.intervalos[1]();
    env.intervalos[1]();
    env.intervalos[1]();
    assert.equal(env.agente.pedidos.length, antes + 1);
    await env.responderTodo();
});

test('una respuesta vieja que llega despues de una nueva no pisa el peso', async () => {
    const env = crearEntorno();
    const recibidos = await arrancarConLectura(env);
    let peso = 3;
    env.agente.responder = () => ({ ok: true, conectada: true, peso: peso, pesoTexto: peso.toFixed(3), pesoDisplay: peso.toFixed(3), edadMs: 10 });
    // Pedido 1 (timer) y pedido 2 (leerAhora) en vuelo; el 2 responde primero con 4 kg y el 1 despues con 3 kg.
    env.intervalos[1]();
    env.api.leerAhora();
    const [p1, p2] = env.pendientes.splice(0, 2);
    peso = 4; p2.completar();
    peso = 3; p1.completar();
    await new Promise((r) => setImmediate(r));
    assert.equal(env.api.lecturaVigente().peso, 4);
    assert.equal(recibidos[recibidos.length - 1].peso, 4);
});

test('con un pedido colgado, al vencer la lectura la pantalla se entera una sola vez (sin esperar el timeout)', async () => {
    const env = crearEntorno();
    const recibidos = await arrancarConLectura(env);
    env.intervalos[1](); // pedido en vuelo que no responde todavia
    const antes = recibidos.length;
    env.reloj.ahora += 1000;
    env.intervalos[1]();
    assert.equal(recibidos.length, antes, 'todavia vigente: no se avisa nada');
    env.reloj.ahora += 700;
    env.intervalos[1]();
    env.intervalos[1]();
    assert.equal(recibidos.length, antes + 1, 'vencida: un solo aviso');
    assert.equal(recibidos[recibidos.length - 1].sinRespuesta, true);
    env.pendientes.length = 0;
});

test('los pedidos llevan cache-buster', async () => {
    const env = crearEntorno();
    await arrancarConLectura(env);
    assert.ok(env.agente.pedidos.every((u) => /[?&]_=\d+/.test(u)));
});

test('status de un agente con la lectura congelada se muestra como no conectada', () => {
    const env = crearEntorno();
    const n = env.utils.normalize({ ok: true, conectada: true, puerto: 'COM3', error: null, edadMs: 9000, fechaHora: fechaAgente(env.reloj.ahora - 9000) });
    assert.equal(n.ok, true);
    assert.equal(n.vencida, true);
    assert.equal(n.conectada, false);
});
