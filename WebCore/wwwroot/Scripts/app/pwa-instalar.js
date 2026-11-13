/* Instalador de mini-apps (Stock / Movimientos / Elaborados), 2026-11-13, pedido del usuario: tarjetas en Utilidades.
   Por que existe: el navegador solo ofrece instalar una PWA desde una pagina que enlaza SU manifest (_PwaHead.cshtml elige el manifest por
   controlador), asi que Utilidades (manifest general) no puede instalar las mini-apps directamente. Utilidades abre la pantalla con ?instalar=1
   y este script muestra una barra con el boton "Instalar", que dispara el aviso nativo del navegador (beforeinstallprompt) con un clic del usuario.
   Solo actua con ?instalar=1 en la URL: en cualquier otra pagina no hace nada. Sin dependencias. Detalle: docs/DECISIONS.md (2026-11-13). */
(function () {
    'use strict';

    if (!/[?&]instalar=1(&|$)/.test(window.location.search)) return;

    // Si pasa este tiempo sin que el navegador ofrezca instalar, se muestran las instrucciones manuales (ya instalada, HTTPS no valido, etc.).
    var ESPERA_OFERTA_MS = 4000;

    var eventoInstalacion = null; // evento beforeinstallprompt guardado para dispararlo con el clic
    var estado = 'esperando';     // esperando | listo | instalada | manual
    var nombreApp = 'esta app';
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
        estado = 'instalada';
        dibujar();
    });

    function textoManual() {
        if (!window.isSecureContext) {
            return 'Este sitio no usa una conexión segura válida (HTTPS), así que el navegador no permite instalar la app. Solo se puede instalar desde https://carnisys.com.';
        }
        return 'El navegador no ofreció instalar ' + nombreApp + '. Si ya la tenés instalada, abrila desde el ícono. Si no, en Chrome o Edge: menú ⋮ → «Guardar y compartir» (Edge: «Aplicaciones») → «Instalar página como aplicación».';
    }

    function dibujar() {
        if (!barra) return;
        var texto = barra.querySelector('.pwa-instalar-texto');
        var boton = barra.querySelector('.pwa-instalar-boton');
        boton.style.display = 'none';
        if (estado === 'listo') {
            texto.textContent = 'Instalá ' + nombreApp + ' como aplicación: tiene su ventana y su ícono en la barra de tareas.';
            boton.style.display = '';
        } else if (estado === 'instalada') {
            texto.textContent = nombreApp + ' quedó instalada. Ya podés cerrar esta pestaña y abrirla desde su ícono.';
        } else if (estado === 'manual') {
            texto.textContent = textoManual();
        } else {
            texto.textContent = 'Preparando la instalación de ' + nombreApp + '…';
        }
    }

    function pedirInstalacion() {
        if (!eventoInstalacion) return;
        var pendiente = eventoInstalacion;
        eventoInstalacion = null; // el evento solo se puede usar una vez
        pendiente.prompt();
        pendiente.userChoice.then(function (eleccion) {
            if (eleccion && eleccion.outcome === 'accepted') {
                estado = 'instalada';
            } else {
                estado = 'manual'; // rechazó el aviso: el navegador no lo vuelve a ofrecer enseguida, quedan las instrucciones manuales
            }
            dibujar();
        }).catch(function (error) {
            console.warn('[pwa-instalar] no se pudo completar la instalación', error);
            estado = 'manual';
            dibujar();
        });
    }

    // Toma el nombre de la mini-app desde el manifest que enlaza esta pagina (ej. "CarniSys Stock"); si falla, queda el texto genérico.
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
        crearBarra();
        dibujar();
        cargarNombreApp().then(dibujar);
        window.setTimeout(function () {
            if (estado === 'esperando') { estado = 'manual'; dibujar(); }
        }, ESPERA_OFERTA_MS);
    });
})();
