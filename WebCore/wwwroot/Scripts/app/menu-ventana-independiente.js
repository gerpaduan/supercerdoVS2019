/* Menu lateral: clic derecho -> menu propio para abrir la pantalla en una ventana independiente SIN salir de la vista actual (2026-11-20).
   - Pantallas que tienen app independiente (enlace con data-app: Stock, Movimientos, Elaborados, Punto de venta, Punto de expendio, Existencia por
     sucursales): «Abrir ventana de la app».
   - Resto de las pantallas: «Abrir en ventana independiente».
   La ventana es la misma de «Abrir en ventana»: su nombre empieza con carnisys-trabajo-, y el script del <head> de _Layout.cshtml la muestra sin menu
   lateral, barra superior ni pie (html.modo-ventana). Una ventana por destino: repetir el clic derecho sobre el mismo destino la trae al frente SIN
   recargarla (no se pierde lo que tenga cargado). Una pagina web no puede lanzar una app ya instalada (Chrome no lo permite): «Abrir ventana de la app»
   abre la URL de la app en una ventana del navegador; no se intenta detectar si esta instalada. Instalar las apps se hace solo desde Utilidades
   (el 2026-11-20 se quito «Instalar la app…» de este menu a pedido del usuario).
   Atributos que lee del enlace (los pone _Layout.cshtml, ya con el PathBase): data-app (clave) y data-app-url (URL de la app si difiere del href).
   Solo escritorio (>= 992 px). Dependencias: ninguna (JS plano; usa Swal solo para avisar si esta cargado).
   Decision y alternativas: docs/DECISIONS.md (2026-11-20). */
(function () {
    'use strict';

    var menuLateral = document.getElementById('accordionSidebar');
    // Ventana independiente / mini-app: no hay menu lateral que escuchar.
    if (!menuLateral || document.documentElement.classList.contains('modo-ventana')) return;

    // Por debajo de este ancho el menu lateral es la franja de iconos del celular/tablet: se deja el menu nativo (mismo piso que ventana-independiente.js).
    var ESCRITORIO_ANCHO_MIN_PX = 992;
    // Tamano de la ventana independiente (igual que ventana-independiente.js); el usuario puede moverla, redimensionarla y maximizarla.
    var VENTANA_ANCHO_PX = 1100;
    var VENTANA_ALTO_PX = 800;
    // Prefijos de nombre: "carnisys-trabajo-" activa el modo ventana (sin menu); el resto del nombre identifica el destino.
    var PREFIJO_VENTANA_APP = 'carnisys-trabajo-app-';
    var PREFIJO_VENTANA_PANTALLA = 'carnisys-trabajo-menu-';
    var MARGEN_PANTALLA_PX = 8;

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
    function esEscritorio() {
        return window.matchMedia ? window.matchMedia('(min-width: ' + ESCRITORIO_ANCHO_MIN_PX + 'px)').matches : true;
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
    // Alt, clic del medio), ya estamos en la pantalla dividida, o la pantalla actual no se abre en un panel (Inicio, POS, login): ahi navega normal.
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

        if (clave) {
            // Pantalla con app independiente: se abre la URL de la app (data-app-url si difiere del href del menu).
            var urlApp = enlace.getAttribute('data-app-url') || enlace.href;
            menu.appendChild(crearOpcion('Abrir ventana de la app', 'fa-external-link-alt', function () {
                var abrirApp = function () { abrirVentana(urlApp, PREFIJO_VENTANA_APP + clave.toLowerCase()); };
                // Punto de venta: si ya hay un POS abierto se trae al frente en vez de abrir otro (pos-ventana-unica.js).
                var unica = window.POSVentanaUnica;
                if (enlace.hasAttribute('data-pos-unico') && unica && unica.traerPosAlFrente(unica.configDeEnlace(enlace), abrirApp)) return;
                abrirApp();
            }));
        } else {
            menu.appendChild(crearOpcion('Abrir en ventana independiente', 'fa-external-link-alt', function () {
                abrirVentana(enlace.href, PREFIJO_VENTANA_PANTALLA + normalizarRuta(enlace.href));
            }));
        }

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
        if (!esEscritorio()) return;
        var enlace = evento.target.closest ? evento.target.closest('a[href]') : null;
        if (!enlace || !menuLateral.contains(enlace)) return;
        // Los encabezados desplegables (Stock, Empleados, Configuracion...) y cualquier "#" conservan el menu nativo.
        var href = enlace.getAttribute('href') || '';
        if (href === '' || href.charAt(0) === '#' || /^javascript:/i.test(href) || enlace.getAttribute('data-bs-toggle')) return;
        // Solo enlaces del mismo sitio.
        if (enlace.origin !== window.location.origin) return;

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
