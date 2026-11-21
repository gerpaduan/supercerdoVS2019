/* Marca «esta ventana es una app independiente» (2026-11-17): una de las apps instaladas desde Utilidades (Stock, Movimientos, Elaborados, Punto de venta,
   Punto de expendio, Existencia), NO la app general «CarniSys».
   Por que hace falta: display-mode: standalone es verdadero en CUALQUIER app instalada, incluida la general (CarniSys), y no dice cual es. Antes se usaba
   «standalone + <meta carnisys-app-scope>» (que _PwaHead emite en toda pagina de esos controladores) y la app general tambien perdia el menu lateral y los enlaces
   de Elaborados / Stock / Movimientos / POS. Ahora la marca es el ?ventana=1 que traen los start_url de manifest-{stock,movimientos,elaborados,pos,expendio,existencia}.json
   (la app general arranca en Home/Index?source=pwa, sin ?ventana=1).
   Al abrirse la app con ?ventana=1 en modo standalone se guarda la marca en sessionStorage (por ventana: sobrevive a la navegacion dentro de la app y no pasa a la
   app general ni a una pestana). Se usa sessionStorage y no window.name porque pos-multi-instance.js reescribe window.name en el POS.
   Expone window.__appIndependiente (true/false). Script SINCRONO en el <head> (lo cargan _PwaHead.cshtml y _LayoutEmbed.cshtml): lo leen el script de modo ventana de
   _Layout.cshtml, app-instalada.js y ventana-tamano.js. Decision: docs/DECISIONS.md (2026-11-17 i).
   Ademas guarda la direccion con la que arranco la app (su start_url, ej. /Ventas/POS?ventana=1) y la expone en window.__appInicio (o null): el login la usa
   para volver a la app despues de cerrar sesion dentro de esta ventana, en vez de ir al dashboard (login-volver-app.js, docs/DECISIONS.md 2026-11-21 «Login en una app instalada vuelve a la app»). */
(function () {
    'use strict';

    var CLAVE = 'carnisys-app-independiente';
    var CLAVE_INICIO = 'carnisys-app-inicio';
    var standalone = false;
    try {
        standalone = (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) || window.navigator.standalone === true;
    } catch (e) { /* sin matchMedia: no es una app instalada */ }

    var esApp = false;
    var inicio = null;
    try {
        if (standalone && /[?&]ventana=1(&|$)/.test(window.location.search)) {
            window.sessionStorage.setItem(CLAVE, '1');
            // Direccion de arranque de la app (incluye el PathBase del sitio, ej. /CarniSysWeb/Ventas/POS?ventana=1).
            window.sessionStorage.setItem(CLAVE_INICIO, window.location.pathname + window.location.search);
        }
        esApp = standalone && window.sessionStorage.getItem(CLAVE) === '1';
        // Se lee aunque esta pagina no sea standalone: el login queda fuera del scope de la app y no se sabe si Chrome lo informa como standalone.
        // Solo la escribe una ventana de app independiente, asi que una pestana normal o la app general no la tienen.
        inicio = window.sessionStorage.getItem(CLAVE_INICIO);
    } catch (e) {
        // sin sessionStorage (modo restringido): solo cuenta la carga que trae ?ventana=1 y no se puede recordar el inicio
        esApp = standalone && /[?&]ventana=1(&|$)/.test(window.location.search);
    }

    window.__appIndependiente = esApp;
    window.__appInicio = inicio || null;
})();
