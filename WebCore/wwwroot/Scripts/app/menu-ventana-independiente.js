/* Menu lateral: clic derecho -> menu propio para abrir la pantalla en una ventana independiente SIN salir de la vista actual (2026-11-20).
   - Pantallas que tienen app independiente (enlace con data-app: Stock, Movimientos, Elaborados, Punto de venta, Punto de expendio, Existencia por
     sucursales): «Abrir ventana de la app».
   - Resto de las pantallas: «Abrir en ventana independiente».
   - Clic del medio (rueda) sobre el enlace (2026-11-24): hace directamente esa primera opcion, sin pestaña nueva (con Ctrl/Mayus/Alt, lo nativo). La ventana
     se abre en una tarea aparte (setTimeout 0): llamada dentro del evento de la rueda, Chrome la abriria como pestaña.
   La ventana es la misma de «Abrir en ventana»: su nombre empieza con carnisys-trabajo-, y el script del <head> de _Layout.cshtml la muestra sin menu
   lateral, barra superior ni pie (html.modo-ventana). Una ventana por destino: repetir el clic derecho sobre el mismo destino la trae al frente SIN
   recargarla (no se pierde lo que tenga cargado). Una pagina web no puede lanzar una app ya instalada (Chrome no lo permite): «Abrir ventana de la app»
   abre la URL de la app en una ventana del navegador; no se intenta detectar si esta instalada. Instalar las apps se hace solo desde Utilidades
   (el 2026-11-20 se quito «Instalar la app…» de este menu a pedido del usuario).
   Atributos que lee del enlace (los pone _Layout.cshtml, ya con el PathBase): data-app (clave), data-app-url (URL de la app si difiere del href) y
   data-ventana-siempre (2026-11-25: Punto de venta y Punto de expendio; el clic izquierdo comun tambien abre la ventana independiente).
   Solo en equipos con mouse (any-pointer: fine), con la ventana de cualquier ancho (antes >= 992 px, 2026-11-24). Dependencias: ninguna (JS plano; usa Swal solo para avisar si esta cargado).
   Decision y alternativas: docs/DECISIONS.md (2026-11-20). */
(function () {
    'use strict';

    var menuLateral = document.getElementById('accordionSidebar');
    // Ventana independiente / mini-app: no hay menu lateral que escuchar.
    if (!menuLateral || document.documentElement.classList.contains('modo-ventana')) return;

    // Equipos con mouse (2026-11-24): el menu propio y la rueda actuan si hay un puntero preciso, SIN importar el ancho de la ventana. Antes se pedia
    // un ancho de 992 px (CSS) y una ventana de escritorio a media pantalla (con Windows al 125 %, menos de ~1240 px reales) quedaba con el menu nativo de
    // Chrome y la rueda abria una ventana nueva de la app CON menu (aviso del usuario). En celular / tablet sin mouse se sigue dejando lo nativo.
    var CONSULTA_MOUSE = '(any-pointer: fine)';
    // Tamano de la ventana independiente (igual que ventana-independiente.js); el usuario puede moverla, redimensionarla y maximizarla.
    var VENTANA_ANCHO_PX = 1100;
    var VENTANA_ALTO_PX = 800;
    // Prefijos de nombre: "carnisys-trabajo-" activa el modo ventana (sin menu); el resto del nombre identifica el destino.
    var PREFIJO_VENTANA_APP = 'carnisys-trabajo-app-';
    var PREFIJO_VENTANA_PANTALLA = 'carnisys-trabajo-menu-';
    var MARGEN_PANTALLA_PX = 8;
    var NOMBRE_VENTANA_DIVIDIDA = 'carnisys-trabajo-pantalla-dividida';   // el mismo de pantalla-dividida.js

    // true si el enlace va a la pantalla dividida (su URL la da data-pantalla-dividida-url del menu lateral).
    function esPantallaDividida(enlace) {
        var urlDividida = menuLateral.getAttribute('data-pantalla-dividida-url');
        if (!urlDividida) return false;
        try { return enlace.pathname.toLowerCase() === new URL(urlDividida, window.location.origin).pathname.toLowerCase(); } catch (e) { return false; }
    }

    var menu = null;          // <div role="menu"> abierto, o null
    var origenFoco = null;    // enlace al que se devuelve el foco al cerrar con el teclado

    // ---------- estilos (se inyectan una sola vez) ----------
    function inyectarEstilos() {
        if (document.getElementById('mvi-estilos')) return;
        var estilo = document.createElement('style');
        estilo.id = 'mvi-estilos';
        estilo.textContent =
            '.mvi-menu{position:fixed;z-index:2000;min-width:15rem;padding:4px;background:#fff;color:#212529;border:1px solid rgba(0,0,0,.18);' +
            'border-radius:.5rem;box-shadow:0 .25rem .9rem rgba(0,0,0,.18);font-size:.85rem}' +
            '.mvi-menu button{display:flex;align-items:center;gap:.5rem;width:100%;margin:0;padding:.4rem .65rem;border:0;border-radius:.35rem;' +
            'background:transparent;color:inherit;text-align:left;white-space:nowrap;cursor:pointer;font:inherit}' +
            '.mvi-menu button:hover,.mvi-menu button:focus{background:#e7edf9;color:#1f3a8a;outline:0}' +
            'html.dark-mode .mvi-menu{background:#2b2f36;color:#e6e8eb;border-color:rgba(255,255,255,.18)}' +
            'html.dark-mode .mvi-menu button:hover,html.dark-mode .mvi-menu button:focus{background:#3b4a78;color:#fff}';
        document.head.appendChild(estilo);
    }

    // ---------- utilidades ----------
    function hayMouse() {
        return window.matchMedia ? window.matchMedia(CONSULTA_MOUSE).matches : true;
    }

    function avisar(texto, icono) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: icono || 'info', title: texto, showConfirmButton: false, timer: 4000 });
        } else {
            window.alert(texto);
        }
    }

    // Nombre de ventana a partir de la ruta ("/Stock/Index" -> "stock-index"): estable, asi el mismo destino reutiliza su ventana.
    function normalizarRuta(url) {
        var ruta = '';
        try { ruta = new URL(url, window.location.href).pathname; } catch (e) { ruta = String(url || ''); }
        return ruta.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-+|-+$/g, '') || 'inicio';
    }

    // ---------- apertura de ventanas ----------
    // Abre (o trae al frente) la ventana "nombre" con "url". Si ya existia, NO se recarga: se preserva lo que el usuario tenga cargado.
    function abrirVentana(url, nombre) {
        var disponibleAncho = (window.screen && (window.screen.availWidth || window.screen.width)) || VENTANA_ANCHO_PX;
        var disponibleAlto = (window.screen && (window.screen.availHeight || window.screen.height)) || VENTANA_ALTO_PX;
        var ancho = Math.min(VENTANA_ANCHO_PX, disponibleAncho);
        var alto = Math.min(VENTANA_ALTO_PX, disponibleAlto);
        var izquierda = Math.max(0, Math.round((disponibleAncho - ancho) / 2));
        var arriba = Math.max(0, Math.round((disponibleAlto - alto) / 2));
        var caracteristicas = 'popup=yes,left=' + izquierda + ',top=' + arriba + ',width=' + ancho + ',height=' + alto + ',resizable=yes,scrollbars=yes';

        // window.open con nombre existente devuelve esa ventana; si no existia, abre una en blanco que se navega enseguida (el nombre ya queda puesto,
        // y es lo que _Layout.cshtml usa para activar el modo ventana).
        var ventana = window.open('', nombre, caracteristicas);
        if (!ventana) {
            avisar('El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.', 'warning');
            return;
        }
        var yaTeniaPagina = false;
        try { yaTeniaPagina = ventana.location.href !== 'about:blank'; } catch (e) { yaTeniaPagina = true; /* otra origen: ya tenia pagina */ }
        if (yaTeniaPagina) {
            ventana.focus();
            avisar('Ya hay una ventana abierta de esta pantalla. La traje al frente.');
            return;
        }
        ventana.location.href = url;
        ventana.focus();
    }

    // Accion principal sobre un enlace del menu (primera opcion del clic derecho y clic del medio, 2026-11-24):
    // - con data-app: la ventana de la app (data-app-url si difiere del href); en el Punto de venta, si ya hay un POS abierto se lo trae al frente
    //   en vez de abrir otro (pos-ventana-unica.js);
    // - sin data-app: la pantalla en una ventana independiente, una por destino.
    function abrirEnVentanaIndependiente(enlace) {
        var clave = enlace.getAttribute('data-app');
        if (!clave) {
            // La pantalla dividida usa el nombre de SU ventana (pantalla-dividida.js, NOMBRE_VENTANA_DIVIDIDA): asi guarda su propia distribucion y no
            // pisa la de la pestaña.
            abrirVentana(enlace.href, esPantallaDividida(enlace) ? NOMBRE_VENTANA_DIVIDIDA : PREFIJO_VENTANA_PANTALLA + normalizarRuta(enlace.href));
            return;
        }
        var urlApp = enlace.getAttribute('data-app-url') || enlace.href;
        var abrirApp = function () { abrirVentana(urlApp, PREFIJO_VENTANA_APP + clave.toLowerCase()); };
        var unica = window.POSVentanaUnica;
        if (enlace.hasAttribute('data-pos-unico') && unica && unica.traerPosAlFrente(unica.configDeEnlace(enlace), abrirApp)) return;
        abrirApp();
    }

    // Enlace del menu lateral sobre el que cae el evento, si admite ventana independiente; null si no (celular/tablet sin mouse, encabezados desplegables
    // —Stock, Empleados, Configuracion…—, "#", javascript: u otro sitio). Lo usan el clic derecho y el clic del medio.
    function enlaceConVentana(evento) {
        if (!hayMouse()) return null;
        var enlace = evento.target && evento.target.closest ? evento.target.closest('a[href]') : null;
        if (!enlace || !menuLateral.contains(enlace)) return null;
        var href = enlace.getAttribute('href') || '';
        if (href === '' || href.charAt(0) === '#' || /^javascript:/i.test(href) || enlace.getAttribute('data-bs-toggle')) return null;
        if (enlace.origin !== window.location.origin) return null;
        return enlace;
    }

    // ---------- menu contextual ----------
    function cerrarMenu(devolverFoco) {
        if (!menu) return;
        menu.parentNode.removeChild(menu);
        menu = null;
        if (devolverFoco && origenFoco && typeof origenFoco.focus === 'function') origenFoco.focus();
        origenFoco = null;
    }

    function crearOpcion(texto, icono, alElegir) {
        var boton = document.createElement('button');
        boton.type = 'button';
        boton.setAttribute('role', 'menuitem');
        boton.innerHTML = '<i class="fas ' + icono + '" aria-hidden="true"></i>';
        boton.appendChild(document.createTextNode(texto));
        boton.addEventListener('click', function () {
            cerrarMenu(false);
            alElegir();
        });
        return boton;
    }

    function posicionar(elemento, x, y) {
        var caja = elemento.getBoundingClientRect();
        var maxX = window.innerWidth - caja.width - MARGEN_PANTALLA_PX;
        var maxY = window.innerHeight - caja.height - MARGEN_PANTALLA_PX;
        elemento.style.left = Math.max(MARGEN_PANTALLA_PX, Math.min(x, maxX)) + 'px';
        elemento.style.top = Math.max(MARGEN_PANTALLA_PX, Math.min(y, maxY)) + 'px';
    }

    // Pantalla dividida (2026-11-20, docs/DECISIONS.md "Pantalla dividida"): ver varios modulos lado a lado.
    // - Estando en la pantalla dividida (window.PantallaDividida, pantalla-dividida.js): «Abrir en un panel nuevo» y «Abrir en el panel activo».
    // - Desde cualquier otra pantalla: «Abrir en pantalla dividida» va a la pantalla dividida con este modulo en un panel (?abrir=<ruta>).
    // El POS no se abre en un panel (solo ventana independiente); la URL de la pantalla dividida viene de data-pantalla-dividida-url del menu lateral.
    // Va a la pantalla dividida con "ruta" como panel (?abrir=<ruta>). Si esta pantalla tiene cambios sin guardar pide confirmar con el aviso de la app
    // («Salir sin guardar») y apaga el cartel del navegador para no preguntar dos veces (mismo criterio que los enlaces de _Layout.cshtml).
    // llevarCarga (solo para la pantalla ACTUAL, boton «Dividir pantalla»): si es una carga que sabe pasar lo cargado (DocumentoAbierto:
    // Elaborados, Elaborados rapido, Movimientos, Stock) sigue en el panel con todo lo que tenia, sin el aviso; si no, el aviso de siempre.
    function irAPantallaDividida(ruta, llevarCarga) {
        var urlDividida = menuLateral.getAttribute('data-pantalla-dividida-url');
        if (!urlDividida) return;
        var traspaso = null;
        if (llevarCarga && window.DocumentoAbierto && typeof window.DocumentoAbierto.prepararTraspaso === 'function') {
            traspaso = window.DocumentoAbierto.prepararTraspaso();
        }
        var ir = function () {
            if (traspaso) {
                ruta = traspaso.urlCon(ruta);
                traspaso.ceder();   // esta pagina deja de guardar borrador y de proteger la salida: lo cargado sigue en el panel
            }
            if (typeof window.desactivarProteccionSalida === 'function') window.desactivarProteccionSalida();
            window.location.href = urlDividida + '?abrir=' + encodeURIComponent(ruta);
        };
        if (!traspaso && window.__protegerSalida && typeof window.confirmarSalidaSinGuardar === 'function') window.confirmarSalidaSinGuardar(ir);
        else ir();
    }

    // Boton «Dividir pantalla» de la barra superior: la pantalla actual pasa a ser un panel de la pantalla dividida, con lo que tenga cargado.
    var botonDividir = document.getElementById('btnDividirPantalla');
    if (botonDividir) {
        botonDividir.addEventListener('click', function () { irAPantallaDividida(window.location.pathname + window.location.search, true); });
    }

    // El item «Pantalla dividida» del menu lateral (2026-11-21): estando en una pantalla normal hace lo mismo que el boton de arriba —la pantalla actual
    // pasa como primer panel, con lo cargado y sin el aviso «Salir sin guardar», y queda un panel vacio al lado para elegir—. Antes navegaba normal
    // (sin llevar la pantalla) y los enlaces de _Layout.cshtml pedian confirmar la salida. No se toca si: es una pestaña/ventana nueva (Ctrl, Mayus,
    // Alt; el clic del medio lo atiende el auxclick de abajo), ya estamos en la pantalla dividida, o la pantalla actual no se abre en un panel (Inicio, POS, login): ahi navega normal.
    var RUTA_NO_DIVIDIBLE = /(^|\/)(ventas\/pos|login|logout)(\/|$)/i;
    var RUTA_INICIO = /(^\/$)|(\/home(\/index)?\/?$)/i;
    window.addEventListener('click', function (evento) {
        if (evento.defaultPrevented || evento.button !== 0 || evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) return;
        if (window.PantallaDividida || !document.getElementById('btnDividirPantalla')) return;   // ya en la pantalla dividida, o sin el boton de dividir
        var enlace = evento.target && evento.target.closest ? evento.target.closest('a[href]') : null;
        if (!enlace || !menuLateral.contains(enlace)) return;
        var urlDividida = menuLateral.getAttribute('data-pantalla-dividida-url');
        if (!urlDividida) return;
        var destino;
        try { destino = new URL(urlDividida, window.location.origin).pathname.toLowerCase(); } catch (err) { return; }
        if (enlace.pathname.toLowerCase() !== destino) return;
        var rutaActual = window.location.pathname;
        if (RUTA_NO_DIVIDIBLE.test(rutaActual) || RUTA_INICIO.test(rutaActual)) return;
        evento.preventDefault();
        evento.stopImmediatePropagation();   // antes que los enlaces de _Layout.cshtml, que pedirian confirmar la salida
        irAPantallaDividida(rutaActual + window.location.search, true);
    }, true);

    function agregarOpcionesPantallaDividida(enlace) {
        var urlDividida = menuLateral.getAttribute('data-pantalla-dividida-url');
        if (!urlDividida || enlace.hasAttribute('data-pos-unico')) return;
        var ruta = enlace.pathname + enlace.search;
        if (ruta.toLowerCase().indexOf('/pantalladividida') !== -1) return;   // el propio enlace de la pantalla dividida
        var api = window.PantallaDividida;
        if (api) {
            if (!api.esUrlPermitida(ruta)) return;
            menu.appendChild(crearOpcion('Abrir en un panel nuevo', 'fa-columns', function () { api.abrirEnPanelNuevo(ruta); }));
            menu.appendChild(crearOpcion('Abrir en el panel activo', 'fa-window-maximize', function () { api.abrirEnPanelActivo(ruta); }));
        } else {
            menu.appendChild(crearOpcion('Abrir en pantalla dividida', 'fa-columns', function () { irAPantallaDividida(ruta); }));
        }
    }

    function abrirMenu(enlace, x, y) {
        cerrarMenu(false);
        inyectarEstilos();
        origenFoco = enlace;

        var clave = enlace.getAttribute('data-app');
        menu = document.createElement('div');
        menu.className = 'mvi-menu';
        menu.setAttribute('role', 'menu');

        // Primera opcion: la misma accion que el clic del medio (abrirEnVentanaIndependiente).
        menu.appendChild(crearOpcion(clave ? 'Abrir ventana de la app' : 'Abrir en ventana independiente', 'fa-external-link-alt', function () {
            abrirEnVentanaIndependiente(enlace);
        }));

        agregarOpcionesPantallaDividida(enlace);

        document.body.appendChild(menu);
        posicionar(menu, x, y);
        var primera = menu.querySelector('button');
        if (primera) primera.focus();
    }

    // ---------- eventos ----------
    menuLateral.addEventListener('contextmenu', function (evento) {
        // Un menu abierto se cierra siempre; si el clic derecho cae en un enlace sin menu propio (encabezado desplegable) no debe quedar el anterior.
        cerrarMenu(false);
        // Los encabezados desplegables, "#" y otros sitios conservan el menu nativo.
        var enlace = enlaceConVentana(evento);
        if (!enlace) return;

        evento.preventDefault();
        var x = evento.clientX, y = evento.clientY;
        // Abierto con el teclado (tecla Menu / Shift+F10): el evento no trae posicion, se usa la del enlace.
        if (!x && !y) {
            var r = enlace.getBoundingClientRect();
            x = r.left + 24;
            y = r.bottom;
        }
        abrirMenu(enlace, x, y);
    });

    // Clic del medio (rueda) sobre un enlace del menu (2026-11-24, pedido del usuario): hace lo mismo que la primera opcion del clic derecho, en vez de
    // la pestaña nueva del navegador (que trae el menu lateral). Con Ctrl, Mayus o Alt se deja el comportamiento nativo.
    function esClicDelMedioPropio(evento) {
        return evento.button === 1 && !evento.ctrlKey && !evento.metaKey && !evento.shiftKey && !evento.altKey;
    }
    // mousedown: evita el icono de autodesplazamiento de Windows que aparece al apretar la rueda sobre la pagina.
    menuLateral.addEventListener('mousedown', function (evento) {
        if (esClicDelMedioPropio(evento) && enlaceConVentana(evento)) evento.preventDefault();
    });
    menuLateral.addEventListener('auxclick', function (evento) {
        if (!esClicDelMedioPropio(evento)) return;
        var enlace = enlaceConVentana(evento);
        if (!enlace) return;
        evento.preventDefault();   // sin esto el navegador abre ademas la pestaña nueva
        cerrarMenu(false);
        // La ventana se abre FUERA del evento de la rueda (2026-11-24): Chrome deriva si window.open abre ventana o pestaña del evento que la dispara, y
        // con un clic del medio la abre como PESTAÑA en segundo plano aunque se pida popup=yes (quedaba dentro del navegador, con el menu lateral).
        // El permiso de abrir ventanas (activacion del usuario) dura unos segundos, asi que una tarea inmediata ya no es un clic del medio y sigue permitida.
        window.setTimeout(function () { abrirEnVentanaIndependiente(enlace); }, 0);
    });

    // Punto de venta y Punto de expendio SIEMPRE en ventana independiente (2026-11-25, pedido del usuario): el clic izquierdo comun sobre un enlace
    // marcado con data-ventana-siempre (lo pone _Layout.cshtml) hace lo mismo que «Abrir ventana de la app», en vez de navegar la pestaña actual
    // (que dejaba el POS dentro del navegador, con el menu lateral a la vista). Solo en equipos con mouse (enlaceConVentana); con Ctrl / Mayus / Alt
    // se deja lo nativo, igual que la rueda. Va en window y en captura: tiene que ganarle a pos-ventana-unica.js (document, captura) y a los enlaces
    // de _Layout.cshtml que piden confirmar la salida; la unicidad del POS la resuelve igual abrirEnVentanaIndependiente. El permiso de abrir ventanas
    // lo da este mismo clic (activacion del usuario), asi que window.open se llama dentro del evento.
    window.addEventListener('click', function (evento) {
        if (evento.defaultPrevented || evento.button !== 0 || evento.ctrlKey || evento.metaKey || evento.shiftKey || evento.altKey) return;
        var enlace = enlaceConVentana(evento);
        if (!enlace || !enlace.hasAttribute('data-ventana-siempre') || !enlace.hasAttribute('data-app')) return;
        evento.preventDefault();
        evento.stopImmediatePropagation();
        cerrarMenu(false);
        abrirEnVentanaIndependiente(enlace);
    }, true);

    document.addEventListener('mousedown', function (evento) {
        if (menu && !menu.contains(evento.target)) cerrarMenu(false);
    }, true);
    window.addEventListener('blur', function () { cerrarMenu(false); });
    window.addEventListener('resize', function () { cerrarMenu(false); });
    document.addEventListener('scroll', function () { cerrarMenu(false); }, true);

    document.addEventListener('keydown', function (evento) {
        if (!menu) return;
        if (evento.key === 'Escape') {
            evento.preventDefault();
            cerrarMenu(true);
        } else if (evento.key === 'ArrowDown' || evento.key === 'ArrowUp') {
            evento.preventDefault();
            var botones = Array.prototype.slice.call(menu.querySelectorAll('button'));
            var i = botones.indexOf(document.activeElement);
            var siguiente = evento.key === 'ArrowDown' ? i + 1 : i - 1;
            botones[(siguiente + botones.length) % botones.length].focus();
        } else if (evento.key === 'Tab') {
            cerrarMenu(true);
        }
    }, true);
})();
