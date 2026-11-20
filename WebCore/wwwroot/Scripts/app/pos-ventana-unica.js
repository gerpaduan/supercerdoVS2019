/* POS en ventana unica (2026-11-20). Dos piezas, un solo archivo:
   1) Desde el POS: el boton «Inicio» (atributo data-inicio-aparte) NO navega la ventana del POS: abre Home en otra pestana (nombre fijo
      CarniSysPrincipal) o trae al frente la que ya exista. Asi el POS queda intacto (carrito, cargas, modales) y no salta el aviso
      «Hay una venta en curso», que solo protege contra descargar la pagina.
   2) Desde Home / menu lateral: el enlace «Punto de Venta» (atributo data-pos-unico + data-pos-usuario / data-pos-sucursal) trae al frente
      el POS original ya abierto de ese usuario+sucursal en vez de abrir otro. Nunca abre un segundo original: si lo sabe abierto pero no puede
      alcanzarlo (otra familia de pestanas del navegador) lo avisa y deja elegir.
   Como se encuentra el POS: pos-multi-instance.js registra el POS original en localStorage con su nombre de ventana (entry.windowName); aca se lee
   con POSMultiInstance.openOriginalEntry y se llega a la ventana con window.open('', nombre), que devuelve la ya existente SIN recargarla.
   Limite del navegador: solo se alcanzan ventanas de la misma familia (abiertas una desde la otra o la misma pestana); focus() solo funciona
   dentro de un clic del usuario. Tolerancia de «presencia» mayor que la del latido (STALE_MS=15 s de pos-multi-instance.js) porque un POS en
   segundo plano por mas de 5 min el navegador le espacia los timers a 1/min. Si la entrada resulta vieja, el fallback es abrir normal.
   No actua en la app instalada (Home queda fuera de la app; alli «Inicio» sigue siendo «Cerrar», ver app-instalada.js).
   Dependencias: pos-multi-instance.js (solo para la parte 2). Decision y alternativas: docs/DECISIONS.md (2026-11-20). */
(function (window, document) {
    'use strict';

    // Nombre de la ventana de Home. Lo fija el propio Home al cargar (abajo) y lo usa el POS para encontrarla.
    var HOME_WINDOW_NAME = 'CarniSysPrincipal';
    // Cuanto tiempo se considera «abierto» un POS por su ultimo latido (ver nota de arriba sobre timers en segundo plano).
    var POS_PRESENCE_TOLERANCE_MS = 120000;
    var TOAST_MS = 4000;

    function notify(text, icon) {
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({ toast: true, position: 'top', icon: icon || 'info', title: text, showConfirmButton: false, timer: TOAST_MS });
        } else {
            window.alert(text);
        }
    }

    function isInstalledApp() {
        if (window.AppInstalada && window.AppInstalada.activa) return true;
        try { return !!(window.matchMedia && window.matchMedia('(display-mode: standalone)').matches); } catch (e) { return false; }
    }

    // Clic "normal" (izquierdo, sin teclas): con Ctrl/Shift/Cmd/Alt el usuario pide explicitamente su propio comportamiento del navegador.
    function isPlainClick(event) {
        return event.button === 0 && !event.ctrlKey && !event.metaKey && !event.shiftKey && !event.altKey;
    }

    // Abre (o ubica) la ventana con ese nombre. Devuelve null si el navegador bloqueo la ventana emergente; si no, {win, existed}.
    // existed=true: ya tenia pagina (no se recarga). existed=false: es una ventana en blanco recien creada que el llamador debe navegar o cerrar.
    function openNamed(name) {
        var win = window.open('', name);
        if (!win) return null;
        var existed = false;
        try {
            existed = win.location.href !== 'about:blank';
        } catch (e) {
            existed = true; // otro origen: ya tenia pagina
        }
        return { win: win, existed: existed };
    }

    function focusQuietly(win) {
        try { win.focus(); } catch (e) { /* el navegador puede negar el foco: la ventana ya quedo activada por window.open */ }
    }

    // ---------- 1) Desde el POS: «Inicio» sin salir del POS ----------
    function goHome(homeUrl) {
        var found = openNamed(HOME_WINDOW_NAME);
        if (!found) {
            notify('El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.', 'warning');
            return;
        }
        // Home ya abierta: solo se trae al frente (puede estar en otra pantalla de trabajo: no se pisa lo que el usuario tenga).
        if (!found.existed) found.win.location.href = homeUrl;
        focusQuietly(found.win);
    }

    window.addEventListener('click', function (event) {
        var button = event.target && event.target.closest ? event.target.closest('[data-inicio-aparte]') : null;
        if (!button || !isPlainClick(event) || isInstalledApp()) return;
        // Captura en window y antes que pos-trabajos.js (avisa «Tenés sin guardar» al salir): el POS no se descarga, no hay nada que perder.
        event.preventDefault();
        event.stopImmediatePropagation();
        goHome(button.href);
    }, true);

    // ---------- 2) Desde Home: «Punto de Venta» trae al frente el POS abierto ----------
    // Lee los atributos data-pos-usuario / data-pos-sucursal del enlace (los pone _Layout.cshtml con el usuario de la sesion).
    function configFromLink(link) {
        return {
            userId: parseInt(link.getAttribute('data-pos-usuario'), 10) || 0,
            sucursalId: parseInt(link.getAttribute('data-pos-sucursal'), 10) || 0,
            productKey: ''
        };
    }

    // Pregunta que hacer cuando el POS figura abierto pero no se lo puede alcanzar. onOpenAnyway se llama si el usuario insiste.
    function askOpenAnyway(onOpenAnyway) {
        var text = 'El punto de venta ya está abierto en otra pestaña o ventana del navegador y no puedo traerlo al frente desde acá. Buscalo en la barra de pestañas.';
        if (window.Swal && typeof window.Swal.fire === 'function') {
            window.Swal.fire({
                icon: 'info',
                title: 'El punto de venta ya está abierto',
                text: text,
                showCancelButton: true,
                confirmButtonText: 'Entendido',
                cancelButtonText: 'No lo encuentro, abrir de nuevo',
                reverseButtons: true
            }).then(function (result) {
                if (result.dismiss === window.Swal.DismissReason.cancel) onOpenAnyway();
            });
        } else if (window.confirm(text + '\n\n¿Abrir uno nuevo de todos modos?')) {
            onOpenAnyway();
        }
    }

    // Trae al frente el POS original del usuario+sucursal. Devuelve false si no hay ninguno registrado (el llamador abre normalmente); true si se
    // hizo cargo (lo trajo al frente, avisó o avisó-y-pregunta). onOpenAnyway: que hacer si el usuario insiste en abrir uno nuevo.
    // Debe llamarse DENTRO del clic (window.open / focus exigen gesto del usuario).
    function bringPosToFront(config, onOpenAnyway) {
        var multi = window.POSMultiInstance;
        if (!config || !multi || typeof multi.openOriginalEntry !== 'function') return false;
        var entry = multi.openOriginalEntry(config.userId, config.sucursalId, config.productKey, POS_PRESENCE_TOLERANCE_MS);
        if (!entry) return false;

        var found = openNamed(entry.windowName);
        if (!found) {
            notify('El navegador bloqueó la ventana emergente. Permitila para este sitio e intentá de nuevo.', 'warning');
            return true;
        }
        if (found.existed) {
            focusQuietly(found.win);
            return true;
        }
        // Figuraba abierto pero la ventana no es alcanzable desde esta (otra familia de pestanas, o el POS ya se cerro sin avisar): se descarta la
        // ventana en blanco que acabamos de crear y se deja elegir; nunca se abre un segundo POS sin que el usuario lo pida.
        try { found.win.close(); } catch (e) { /* si no se deja cerrar queda una pestana en blanco: inofensiva */ }
        askOpenAnyway(onOpenAnyway);
        return true;
    }

    document.addEventListener('click', function (event) {
        var link = event.target && event.target.closest ? event.target.closest('a[data-pos-unico]') : null;
        if (!link || event.defaultPrevented || !isPlainClick(event)) return;
        var handled = bringPosToFront(configFromLink(link), function () { window.location.href = link.href; });
        if (handled) event.preventDefault();
    }, true);

    // Home se nombra a si misma (solo la ventana de mas arriba, con menu lateral y sin nombre previo: las ventanas de trabajo ya tienen el suyo).
    try {
        if (window.top === window && !window.name && document.getElementById('accordionSidebar')) {
            window.name = HOME_WINDOW_NAME;
        }
    } catch (e) { /* sin acceso a window.name: Inicio abrira una pestana nueva en vez de reusar la existente */ }

    window.POSVentanaUnica = {
        traerPosAlFrente: bringPosToFront,
        configDeEnlace: configFromLink
    };
})(window, document);
