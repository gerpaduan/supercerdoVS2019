/* Canal entre la pestaña de la pantalla dividida y SU ventana independiente (2026-11-24, docs/DECISIONS.md «Pantalla dividida: la ventana original
   queda abierta»). Cuando la distribucion se pasa a la ventana, la pestaña queda con un aviso; las dos se hablan por un BroadcastChannel del navegador
   (mismo sitio, mismo equipo) para: saber si la ventana sigue abierta (ping), mandarle una pantalla para abrir en un panel nuevo, y devolver la
   distribucion a la pestaña. Se usa un canal y no la referencia window.open porque esa referencia se pierde al recargar la pestaña.
   Sin dependencias. Lo usa pantalla-dividida.js; tests: WebCore.Tests/js/pantalla-dividida-canal.test.js. */
(function (global) {
    'use strict';

    // Tipos de mensaje (pestaña -> ventana y ventana -> pestaña).
    var TIPOS = {
        PING: 'ping',                 // pestaña: ¿estas abierta?
        ACA: 'aca',                   // ventana: si, estoy (respuesta al ping y aviso al arrancar)
        ABRIR: 'abrir',               // pestaña: abri esta ruta en un panel nuevo
        ABIERTO: 'abierto',           // ventana: la abri
        DEVOLVER: 'devolver',         // pestaña: devolveme la distribucion
        DEVOLVIENDO: 'devolviendo',   // ventana: la estoy devolviendo (pideConfirmar: true si espera el «Salir sin guardar»)
        DISTRIBUCION: 'distribucion', // ventana: la distribucion esta en la clave temporal "clave"
        RECIBIDA: 'recibida',         // pestaña: la recibi (la ventana ya puede cerrarse)
        CERRADA: 'cerrada'            // ventana: me estoy cerrando
    };

    var contador = 0;
    function nuevoId() {
        contador += 1;
        return Date.now().toString(36) + '-' + contador + '-' + Math.random().toString(36).slice(2, 8);
    }

    // Crea el canal del usuario. opciones.BroadcastChannel permite inyectar uno falso (tests); sin BroadcastChannel el canal queda "no disponible":
    // enviar no hace nada y pedir responde null (quien lo usa lo trata como «la ventana no contesto»).
    function crear(idUsuario, opciones) {
        var Constructor = (opciones && opciones.BroadcastChannel) || global.BroadcastChannel;
        var bc = null;
        try { bc = Constructor ? new Constructor('carnisys-pantalla-dividida-' + (idUsuario || '0')) : null; } catch (e) {
            console.warn('Pantalla dividida: no se pudo abrir el canal entre ventanas.', e);
            bc = null;
        }
        var oyentes = [];
        var esperas = {};   // id del pedido -> function(respuesta)

        if (bc) {
            bc.onmessage = function (evento) {
                var mensaje = evento && evento.data;
                if (!mensaje || typeof mensaje.tipo !== 'string') return;
                // Respuesta a un pedido nuestro: la toma quien espera y no se reparte a los oyentes.
                if (mensaje.respondeA && esperas[mensaje.respondeA]) {
                    esperas[mensaje.respondeA](mensaje);
                    return;
                }
                oyentes.slice().forEach(function (fn) {
                    try { fn(mensaje); } catch (e) { console.warn('Pantalla dividida: fallo al atender un mensaje del canal.', e); }
                });
            };
        }

        function enviar(mensaje) {
            if (!bc) return null;
            var conId = Object.assign({}, mensaje, { id: mensaje.id || nuevoId() });
            try { bc.postMessage(conId); } catch (e) { console.warn('Pantalla dividida: no se pudo mandar un mensaje a la otra ventana.', e); return null; }
            return conId;
        }

        // Manda "mensaje" y espera la primera respuesta (otro lado llama a responder). Promesa con la respuesta, o null si nadie contesta en esperaMs.
        function pedir(mensaje, esperaMs) {
            return new Promise(function (resolver) {
                var enviado = enviar(mensaje);
                if (!enviado) { resolver(null); return; }
                var hecho = false;
                var terminar = function (respuesta) {
                    if (hecho) return;
                    hecho = true;
                    delete esperas[enviado.id];
                    resolver(respuesta);
                };
                esperas[enviado.id] = terminar;
                global.setTimeout(function () { terminar(null); }, esperaMs);
            });
        }

        function responder(pedido, respuesta) {
            return enviar(Object.assign({}, respuesta, { respondeA: pedido.id }));
        }

        function escuchar(fn) { oyentes.push(fn); }

        return { disponible: !!bc, enviar: enviar, pedir: pedir, responder: responder, escuchar: escuchar };
    }

    global.PantallaDivididaCanal = { TIPOS: TIPOS, crear: crear };
})(window);
