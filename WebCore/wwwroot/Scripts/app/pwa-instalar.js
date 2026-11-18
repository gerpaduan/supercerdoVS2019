/* Instalador de mini-apps (Stock / Movimientos / Elaborados / Punto de venta / Punto de expendio): dispara el aviso nativo "Instalar app".
   Por que existe: el navegador solo ofrece instalar una PWA desde una pagina que enlaza SU manifest (_PwaHead.cshtml elige el manifest por controlador),
   asi que Utilidades (manifest general) no puede instalar las mini-apps directamente. Dos modos, ambos con el boton que da el gesto del usuario
   que exige el navegador (beforeinstallprompt -> prompt()):
   - PAGINA ANFITRIONA (2026-11-17): Views/InstalarApp/Index.cshtml (<html data-pwa-anfitriona data-pwa-nombre="...">), la abre Utilidades en una ventana
     chica. Usa los elementos de esa pagina (#pwaAnfitrionaTexto / #pwaAnfitrionaBoton) y, al instalarse, cierra la ventana sola.
   - BARRA (2026-11-13, compatibilidad): cualquier pantalla con ?instalar=1 en la URL muestra una barra arriba con el boton.
   En cualquier otra pagina no hace nada. Sin dependencias. Detalle: docs/DECISIONS.md (2026-11-13 y 2026-11-17 c). */
(function () {
    'use strict';

    var anfitriona = document.documentElement.hasAttribute('data-pwa-anfitriona');
    if (!anfitriona && !/[?&]instalar=1(&|$)/.test(window.location.search)) return;

    // Si pasa este tiempo sin que el navegador ofrezca instalar, se muestran las instrucciones manuales (ya instalada, HTTPS no valido, etc.).
    var ESPERA_OFERTA_MS = 4000;
    // En la pagina anfitriona, cuanto se deja ver el "listo" antes de cerrar la ventana.
    var ESPERA_CIERRE_MS = 1500;

    // Cola de instalacion (2026-11-17, «Instalar seleccionadas / todas» de Utilidades): ?cola=Movimientos,Elaborados lista las apps que faltan DESPUES de esta.
    // Cada app necesita su propio clic en «Instalar» (el navegador exige un gesto del usuario por instalacion y no lo arrastra entre paginas), asi que la ventana
    // pasa sola a la siguiente cuando esta se instala, se rechaza o el navegador no la ofrece (ya instalada). Solo claves alfabeticas: son carpetas de InstalarAppController.
    var ESPERA_SIGUIENTE_MS = 1500;       // tras instalarse o rechazarse, cuanto se muestra el resultado antes de pasar a la siguiente
    var ESPERA_SIGUIENTE_SIN_OFERTA_MS = 3000; // idem cuando el navegador no la ofrecio (ya instalada)
    var cola = [];
    if (anfitriona) {
        var parametroCola = new URLSearchParams(window.location.search).get('cola') || '';
        cola = parametroCola.split(',').filter(function (clave) { return /^[A-Za-z]+$/.test(clave); });
    }
    var baseApp = document.documentElement.getAttribute('data-pwa-base') || '/';

    var eventoInstalacion = null; // evento beforeinstallprompt guardado para dispararlo con el clic
    var estado = 'esperando';     // esperando | listo | instalada | manual
    var nombreApp = document.documentElement.getAttribute('data-pwa-nombre') || 'esta app';
    var barra = null;

    function estaEnAppInstalada() {
        return (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) || window.navigator.standalone === true;
    }

    // El evento puede dispararse antes de que exista el DOM: se captura ya (el script va en el <head>) y se dibuja despues.
    window.addEventListener('beforeinstallprompt', function (evento) {
        evento.preventDefault(); // frena el mini-aviso automatico: el aviso se pide con el boton
        eventoInstalacion = evento;
        estado = 'listo';
        dibujar();
    });

    window.addEventListener('appinstalled', function () {
        eventoInstalacion = null;
        marcarInstalada();
    });

    function textoManual() {
        if (!window.isSecureContext) {
            return 'Este sitio no usa una conexión segura válida (HTTPS), así que el navegador no permite instalar la app. Solo se puede instalar desde https://carnisys.com.';
        }
        return 'El navegador no ofreció instalar ' + nombreApp + '. Si ya la tenés instalada, abrila desde el ícono. Si no, en Chrome o Edge: menú ⋮ → «Guardar y compartir» (Edge: «Aplicaciones») → «Instalar página como aplicación».';
    }

    // Elementos donde se dibuja el estado: los de la pagina anfitriona o los de la barra.
    function elementos() {
        if (anfitriona) return { texto: document.getElementById('pwaAnfitrionaTexto'), boton: document.getElementById('pwaAnfitrionaBoton') };
        if (!barra) return null;
        return { texto: barra.querySelector('.pwa-instalar-texto'), boton: barra.querySelector('.pwa-instalar-boton') };
    }

    function dibujar() {
        var el = elementos();
        if (!el || !el.texto || !el.boton) return;
        el.boton.style.display = 'none';
        if (estado === 'listo') {
            el.texto.textContent = anfitriona
                ? 'Tocá «Instalar»: el navegador te pide confirmar y la app queda con su ventana y su ícono.'
                : 'Instalá ' + nombreApp + ' como aplicación: tiene su ventana y su ícono en la barra de tareas.';
            el.boton.style.display = '';
        } else if (estado === 'instalada') {
            el.texto.textContent = anfitriona
                ? nombreApp + ' quedó instalada. Esta ventana se cierra sola; si no, ya podés cerrarla.'
                : nombreApp + ' quedó instalada. Ya podés cerrar esta pestaña y abrirla desde su ícono.';
        } else if (estado === 'manual') {
            el.texto.textContent = textoManual() + (anfitriona && cola.length ? ' Sigo con la siguiente app…' : '');
        } else {
            el.texto.textContent = 'Preparando la instalación de ' + nombreApp + '…';
        }
    }

    // Pasa a la siguiente app de la cola; si no queda ninguna, cierra la ventana chica (window.close() funciona porque la abrio Utilidades con window.open).
    function avanzar() {
        if (cola.length) {
            var resto = cola.slice(1);
            window.location.href = baseApp + cola[0] + '/InstalarApp' + (resto.length ? '?cola=' + resto.join(',') : '');
            return;
        }
        try { window.close(); } catch (e) { /* el navegador puede negar el cierre: queda el aviso */ }
    }

    // Instalada: en la pagina anfitriona sigue con la cola o, si no hay mas, cierra la ventana.
    function marcarInstalada() {
        estado = 'instalada';
        dibujar();
        if (anfitriona) window.setTimeout(avanzar, cola.length ? ESPERA_SIGUIENTE_MS : ESPERA_CIERRE_MS);
    }

    // Rechazada o no ofrecida: instrucciones manuales y, si hay cola, sigue con la siguiente sin trabar el resto.
    function marcarManual(sinOferta) {
        estado = 'manual';
        dibujar();
        if (anfitriona && cola.length) window.setTimeout(avanzar, sinOferta ? ESPERA_SIGUIENTE_SIN_OFERTA_MS : ESPERA_SIGUIENTE_MS);
    }

    function pedirInstalacion() {
        if (!eventoInstalacion) return;
        var pendiente = eventoInstalacion;
        eventoInstalacion = null; // el evento solo se puede usar una vez
        pendiente.prompt();
        pendiente.userChoice.then(function (eleccion) {
            if (eleccion && eleccion.outcome === 'accepted') {
                marcarInstalada();
            } else {
                marcarManual(false); // rechazó el aviso: el navegador no lo vuelve a ofrecer enseguida, quedan las instrucciones manuales
            }
        }).catch(function (error) {
            console.warn('[pwa-instalar] no se pudo completar la instalación', error);
            marcarManual(false);
        });
    }

    // Pagina anfitriona con cola: avisa cuantas apps faltan despues de esta.
    function pintarCola() {
        var elemento = document.getElementById('pwaAnfitrionaCola');
        if (!elemento || !cola.length) return;
        elemento.textContent = 'Instalando varias apps: después de esta ' + (cola.length === 1 ? 'falta 1 más.' : 'faltan ' + cola.length + ' más.');
    }

    // Modo barra: toma el nombre de la mini-app desde el manifest que enlaza esta pagina; si falla, queda el texto generico.
    function cargarNombreApp() {
        var enlace = document.querySelector('link[rel="manifest"]');
        if (!enlace) return Promise.resolve();
        return window.fetch(enlace.href, { credentials: 'same-origin' })
            .then(function (respuesta) { return respuesta.ok ? respuesta.json() : null; })
            .then(function (manifest) { if (manifest && manifest.name) nombreApp = manifest.name; })
            .catch(function (error) { console.warn('[pwa-instalar] no se pudo leer el manifest', error); });
    }

    function crearBarra() {
        var estilo = document.createElement('style');
        estilo.textContent =
            '#pwaInstalarBarra{position:fixed;top:0;left:0;right:0;z-index:2000;display:flex;flex-wrap:wrap;align-items:center;justify-content:center;gap:.6rem;' +
            'padding:.55rem 1rem;background:#062d63;color:#fff;font-size:.9rem;box-shadow:0 2px 8px rgba(0,0,0,.35)}' +
            '#pwaInstalarBarra .pwa-instalar-boton{border:0;border-radius:.5rem;padding:.3rem .9rem;font-weight:600;background:#fff;color:#062d63;cursor:pointer}' +
            '#pwaInstalarBarra .pwa-instalar-cerrar{border:0;background:transparent;color:#fff;font-size:1.2rem;line-height:1;cursor:pointer}';
        document.head.appendChild(estilo);

        barra = document.createElement('div');
        barra.id = 'pwaInstalarBarra';
        barra.setAttribute('role', 'status');
        barra.innerHTML = '<span class="pwa-instalar-texto"></span>' +
            '<button type="button" class="pwa-instalar-boton">Instalar</button>' +
            '<button type="button" class="pwa-instalar-cerrar" aria-label="Cerrar">&times;</button>';
        barra.querySelector('.pwa-instalar-boton').addEventListener('click', pedirInstalacion);
        barra.querySelector('.pwa-instalar-cerrar').addEventListener('click', function () { barra.remove(); });
        document.body.appendChild(barra);
    }

    document.addEventListener('DOMContentLoaded', function () {
        if (estaEnAppInstalada()) return; // ya se está usando la app instalada: no hay nada que instalar
        if (anfitriona) {
            document.getElementById('pwaAnfitrionaBoton').addEventListener('click', pedirInstalacion);
        } else {
            crearBarra();
            cargarNombreApp().then(dibujar);
        }
        dibujar();
        pintarCola();
        window.setTimeout(function () {
            if (estado === 'esperando') marcarManual(true); // el navegador no la ofrecio (ya instalada, etc.)
        }, ESPERA_OFERTA_MS);
    });
})();
