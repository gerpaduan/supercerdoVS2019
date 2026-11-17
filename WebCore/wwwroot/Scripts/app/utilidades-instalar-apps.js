/* Utilidades > «Aplicaciones instalables»: abre la pagina anfitriona de instalacion de cada mini-app en una VENTANA CHICA (2026-11-17).
   Antes (2026-11-13/17) se probo instalar directo con navigator.install(): en Chrome instalo la app general «CarniSys» en lugar de la mini-app (la firma de esa
   API cambia entre versiones), asi que se descarto. Ahora cada boton abre {Carpeta}/InstalarApp (InstalarAppController), que enlaza el manifest de ESA app y
   muestra el boton «Instalar» (pwa-instalar.js); al confirmar, esa ventana se cierra sola.
   Si el navegador bloquea el popup, no se hace nada y rige el enlace normal (se abre en una pestana). Decision: docs/DECISIONS.md (2026-11-17 c). */
(function () {
    'use strict';

    var CARACTERISTICAS_VENTANA = 'popup=yes,width=520,height=640';

    document.addEventListener('click', function (evento) {
        var boton = evento.target.closest('.js-instalar-app');
        if (!boton) return;

        // Nombre fijo por app: un segundo clic reutiliza la ventana ya abierta en lugar de apilar otra.
        var nombreVentana = 'carnisys-instalar-' + boton.id;
        var ventana = window.open(boton.href, nombreVentana, CARACTERISTICAS_VENTANA);
        if (!ventana) return; // popup bloqueado: sigue el enlace normal (pestana nueva)

        evento.preventDefault();
        ventana.focus();
    });
})();
