/* App instalada (mini-app Stock / Movimientos / Elaborados / Punto de venta / Punto de expendio): deja usar SOLO esa pantalla (2026-11-17, pedido del usuario).
   Solo actua cuando la pagina corre como app instalada (display-mode: standalone) Y es una pantalla de mini-app (_PwaHead.cshtml publica su carpeta en
   <meta name="carnisys-app-scope">). En el navegador normal y en la app general no hace nada.
   - Enlaces <a> a otra seccion del sistema (misma web, fuera de la carpeta de la app): no navegan y avisan «No disponible en esta ventana». Se dejan pasar
     /Login (sesion vencida), enlaces a otros sitios (WhatsApp, etc.), descargas, #hash y javascript:. Si el enlace trae data-app-salir se cierra la ventana.
   - window.AppInstalada.salir(urlAlternativa): reemplaza el "volver a Inicio" del POS/Expendio: en la app instalada cierra la ventana (los avisos de
     beforeunload del POS siguen protegiendo una venta en curso); fuera de ella navega a urlAlternativa como siempre.
   - Dentro de la app el circuito es Index -> editar / ver / agregar -> de vuelta al Index (Volver y Cancelar son enlaces al Index de la propia app).
   Limite conocido: solo se interceptan clics en enlaces; una navegacion hecha por JS (location.href) no pasa por aqui. Decision: docs/DECISIONS.md (2026-11-17 c). */
(function () {
    'use strict';

    var metaScope = document.querySelector('meta[name="carnisys-app-scope"]');
    var scope = metaScope ? (metaScope.getAttribute('content') || '').toLowerCase() : '';
    var enAppInstalada = (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) || window.navigator.standalone === true;
    // Solo en las apps independientes de Utilidades: standalone tambien es verdadero en la app general CarniSys, que debe navegar libre (window.__appIndependiente,
    // app-independiente.js: la marca es el ?ventana=1 del start_url de cada mini-app).
    var activa = !!scope && enAppInstalada && window.__appIndependiente === true;

    // Tiempo que se espera para saber si el navegador acepto cerrar la ventana antes de mostrar el aviso de respaldo.
    var ESPERA_CIERRE_MS = 800;

    function avisar(texto) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: 'info', title: texto, showConfirmButton: false, timer: 2800 });
        } else {
            window.alert(texto);
        }
    }

    // Cierra la ventana de la app. Si el navegador no la cierra (no hay beforeunload en ese tiempo) avisa al usuario; si hubo beforeunload
    // (venta en curso: el navegador pregunta) no se avisa nada, porque la ventana se cerro o el usuario eligio quedarse.
    function cerrarVentana() {
        var hubo = false;
        function marcar() { hubo = true; }
        window.addEventListener('beforeunload', marcar);
        try { window.close(); } catch (error) { console.warn('[app-instalada] window.close fallo', error); }
        window.setTimeout(function () {
            window.removeEventListener('beforeunload', marcar);
            if (!hubo) avisar('Cerrá esta ventana con la X de arriba a la derecha.');
        }, ESPERA_CIERRE_MS);
    }

    function salir(urlAlternativa) {
        if (!activa) {
            if (urlAlternativa) window.location.href = urlAlternativa;
            return;
        }
        cerrarVentana();
    }

    window.AppInstalada = { activa: activa, salir: salir };
    if (!activa) return;

    // El boton de salida del POS/Expendio dice "Inicio" (va a Home); en la app instalada NO va a Home, cierra la ventana: pasa a decir
    // "Cerrar" (2026-11-19). Los botones lo piden con data-salir-cerrar (+ data-title-cerrar). El script va al final del body: el
    // boton ya existe; si no, se reintenta al terminar de cargar.
    function rotularBotonesDeSalida() {
        var botones = document.querySelectorAll('[data-salir-cerrar]');
        Array.prototype.forEach.call(botones, function (boton) {
            var texto = boton.querySelector('span');
            var icono = boton.querySelector('i');
            if (texto) texto.textContent = 'Cerrar';
            if (icono) icono.className = 'fas fa-times';
            boton.setAttribute('title', boton.getAttribute('data-title-cerrar') || 'Cerrar esta ventana');
        });
    }
    rotularBotonesDeSalida();
    document.addEventListener('DOMContentLoaded', rotularBotonesDeSalida);

    // true si el enlace es una navegacion dentro de este sitio pero fuera de la carpeta de la app (y no es /Login).
    function saleDeLaApp(enlace) {
        if (enlace.hasAttribute('download')) return false;
        var url;
        try { url = new URL(enlace.href, window.location.href); } catch (error) { return false; }
        if (url.protocol !== 'http:' && url.protocol !== 'https:') return false; // javascript:, mailto:, tel:
        if (url.origin !== window.location.origin) return false;                  // otros sitios (WhatsApp, etc.): los maneja el navegador
        if (url.pathname === window.location.pathname && url.search === window.location.search) return false; // mismo lugar / #hash
        var ruta = url.pathname.toLowerCase();
        // Dentro de la app = la carpeta misma (/Movimientos) o cualquier cosa debajo (/Movimientos/Editar). Se compara SIN la barra final del scope:
        // Url.Action("Index", "Movimientos") genera "/Movimientos" (la ruta por defecto omite "/Index"), y contra "/movimientos/" no coincidia, asi que
        // Volver / Cancelar mostraban «No disponible en esta ventana» (2026-11-24).
        var carpetaApp = scope.replace(/\/+$/, '');
        if (ruta === carpetaApp || ruta.indexOf(carpetaApp + '/') === 0) return false;
        if (/\/login(\/|$)/.test(ruta)) return false; // sesion vencida: la app tiene que poder volver a pedir clave
        return true;
    }

    // Fase de captura: corre antes que los manejadores de cada pantalla.
    document.addEventListener('click', function (evento) {
        var enlace = evento.target.closest ? evento.target.closest('a[href]') : null;
        if (!enlace || !saleDeLaApp(enlace)) return;
        evento.preventDefault();
        evento.stopImmediatePropagation();
        if (enlace.hasAttribute('data-app-salir')) cerrarVentana();
        else avisar('No disponible en esta ventana.');
    }, true);
})();
