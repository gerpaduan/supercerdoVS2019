/* Ventana independiente (Movimientos / Stock / Elaborados): se cierra sola al guardar (2026-11-13, pedido del usuario).
   Antes, al guardar en la ventana independiente la pantalla volvia al listado dentro de la misma ventana y habia que cerrarla a mano. Ahora, despues de
   cerrar el aviso de guardado (modal de "Movimiento guardado", aviso de "Elaborado guardado" o el aviso del listado de Stock) la ventana se cierra.
   Solo actua en modo ventana (html.modo-ventana, script del <head> de _Layout.cshtml). Detalle y decision: docs/DECISIONS.md (2026-11-13 c).
   Excepcion (2026-11-24, docs/DECISIONS.md (n)): si la ventana ARRANCO en el listado (Index) de Elaborados / Movimientos / Stock, al guardar vuelve al
   listado en la misma ventana y no se cierra. Solo se cierra la que se abrio directo en un formulario.

   API (window.VentanaTrabajo):
   - cerrarVentana(alternativa): cierra la ventana. Devuelve false si NO es la ventana independiente (el que llama sigue con su flujo de siempre). Si es
     la ventana pero el navegador no deja cerrarla (window.close() solo funciona en ventanas que abrio el sistema o la mini-app instalada; una pestana normal
     abierta con ?ventana=1 no se cierra), a los ESPERA_ALTERNATIVA_MS se ejecuta la "alternativa" (lo que se hacia antes: ir al listado).
   - marcarGuardado(): Stock guarda con un POST de formulario y despues muestra el aviso en el listado (otra pagina): se deja una marca en sessionStorage
     para cerrar la ventana cuando ese aviso se cierre. La marca vence a los 60 s y se borra al cargar cualquier otra pantalla. */
(function () {
    'use strict';

    var VIGENCIA_MARCA_MS = 60000;
    var ESPERA_ALTERNATIVA_MS = 500;

    var enModoVentana = document.documentElement.classList.contains('modo-ventana');

    // Carga abierta DENTRO del POS (2026-11-18, docs/DECISIONS.md "Cargas desde el POS"): la pantalla corre en un iframe de
    // #modalTrabajoPOS cuyo nombre es carnisys-trabajo-pos-<tipo>. Ahi no hay ventana que cerrar: al terminar de guardar se le avisa al
    // POS (postMessage, mismo origen) y es el POS quien cierra esa carga.
    var nombreVentana = String(window.name || '');
    var enPos = window.parent !== window && nombreVentana.indexOf('carnisys-trabajo-pos-') === 0;

    // La marca de Stock lleva el nombre del iframe: con dos cargas de Stock abiertas (ingreso y egreso) cada una cierra solo la suya.
    var CLAVE_MARCA = 'carnisys-ventana-cerrar-tras-guardar' + (enPos ? ':' + nombreVentana : '');

    // ---------- donde arranco la ventana (2026-11-24, pedido del usuario) ----------
    // «Si abri la ventana independiente desde el listado (Index), al guardar que vuelva al listado» en vez de cerrarse. La primera pantalla que carga la
    // ventana se anota en sessionStorage: 'listado' (Elaborados, Movimientos o Stock sin accion / Index: menu lateral, clic derecho, rueda o la app
    // instalada) o 'carga' (se abrio directo en un formulario, p. ej. «Abrir en ventana» desde el editor: esa se sigue cerrando al guardar).
    // La clave lleva el nombre de la ventana: sessionStorage lo comparten los paneles (iframes) de la pantalla dividida y lo copia window.open a la
    // ventana nueva, asi que sin el nombre una ventana heredaria el origen de otra.
    var CLAVE_ORIGEN = 'carnisys-ventana-origen:' + nombreVentana;
    var RUTA_LISTADO = /\/(elaborados|movimientos|stock)(\/index)?\/?$/i;   // termina en el listado (con o sin PathBase delante)

    function esListado(ruta) { return RUTA_LISTADO.test(String(ruta || '')); }

    function registrarOrigen() {
        if (!enModoVentana || enPos) return;
        try {
            if (window.sessionStorage.getItem(CLAVE_ORIGEN) === null) {
                window.sessionStorage.setItem(CLAVE_ORIGEN, esListado(window.location.pathname) ? 'listado' : 'carga');
            }
        } catch (e) { /* sin sessionStorage: no se sabe el origen y la ventana se cierra al guardar, como antes */ }
    }

    function abrioDesdeListado() {
        try { return window.sessionStorage.getItem(CLAVE_ORIGEN) === 'listado'; } catch (e) { return false; }
    }

    registrarOrigen();

    function avisarAlPos() {
        try {
            window.parent.postMessage({ tipo: 'carnisys-trabajo-pos', evento: 'guardado' }, window.location.origin);
        } catch (e) { /* el POS no esta: queda la carga a la vista, se puede descartar a mano */ }
    }

    function leerMarca() {
        try { return parseInt(window.sessionStorage.getItem(CLAVE_MARCA), 10) || 0; } catch (e) { return 0; }
    }

    function escribirMarca() {
        try { window.sessionStorage.setItem(CLAVE_MARCA, String(Date.now())); } catch (e) { /* sin sessionStorage: la ventana simplemente no se cierra sola */ }
    }

    function borrarMarca() {
        try { window.sessionStorage.removeItem(CLAVE_MARCA); } catch (e) { /* idem */ }
    }

    function cerrarVentana(alternativa) {
        if (!enModoVentana) return false;
        borrarMarca();
        // Ya se guardo: sin esto el aviso nativo "¿Deseas abandonar el sitio?" frenaria el cierre si quedara algun cambio marcado.
        if (typeof window.desactivarProteccionSalida === 'function') window.desactivarProteccionSalida();
        else window.__protegerSalida = false;
        // Dentro del POS no se cierra ninguna ventana ni se va al listado: el POS cierra la carga y muestra el aviso.
        if (enPos) { avisarAlPos(); return true; }
        // La ventana arranco en el listado: vuelve al listado en la misma ventana (lo que hace "alternativa") en vez de cerrarse.
        if (abrioDesdeListado() && typeof alternativa === 'function') { alternativa(); return true; }
        try { window.close(); } catch (e) { /* el navegador puede negar el cierre */ }
        if (typeof alternativa === 'function') window.setTimeout(alternativa, ESPERA_ALTERNATIVA_MS);
        return true;
    }

    window.VentanaTrabajo = {
        activa: enModoVentana,
        esListado: esListado,                 // expuesto para los tests
        abrioDesdeListado: abrioDesdeListado,
        cerrarVentana: cerrarVentana,
        marcarGuardado: function () { if (enModoVentana) escribirMarca(); }
    };

    if (!enModoVentana) return;

    // Stock: el aviso "se registro correctamente" lo muestra el listado (_Layout.cshtml lo avisa con layoutAlertClosed al cerrarse).
    document.addEventListener('layoutAlertClosed', function () {
        var marca = leerMarca();
        if (!marca || Date.now() - marca >= VIGENCIA_MARCA_MS) return;
        // Arranco en el listado: ya esta en el listado de Stock con el aviso; la ventana queda abierta ahi.
        if (abrioDesdeListado()) { borrarMarca(); return; }
        cerrarVentana();
    });

    // La marca solo sirve para el listado de Stock que viene justo despues de guardar: en cualquier otra pantalla (por ejemplo, la misma pantalla
    // de carga si el guardado fallo) se descarta.
    document.addEventListener('DOMContentLoaded', function () {
        if (!/\/Stock\/?(Index)?\/?$/i.test(window.location.pathname)) borrarMarca();
    });
})();
