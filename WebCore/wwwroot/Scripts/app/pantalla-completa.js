// Boton "Pantalla completa" (#btnPantallaCompleta): entra y sale de pantalla completa igual que F11.
// Hoy lo usa Elaborados/Carga. No depende de jQuery ni de otros scripts de la pagina.
// - Click en el boton o tecla F11 (sin modificadores): alterna. F11 se intercepta para que el icono quede sincronizado
//   (el F11 nativo del navegador no dispara 'fullscreenchange'); donde el navegador no entregue F11 a la pagina, sigue andando el nativo.
// - El icono (expandir / contraer) y el tooltip (title) cambian segun el estado.
// - Si el navegador no soporta la API de pantalla completa, el boton se oculta.
(function () {
    'use strict';

    var TITULO_ENTRAR = 'F11 · Pantalla completa';
    var TITULO_SALIR = 'F11 · Salir de pantalla completa';

    var boton = document.getElementById('btnPantallaCompleta');
    if (!boton) return;

    var raiz = document.documentElement;
    var soportado = typeof raiz.requestFullscreen === 'function' && typeof document.exitFullscreen === 'function';
    if (!soportado) {
        boton.style.display = 'none';
        return;
    }

    var icono = boton.querySelector('i');

    // En pantalla completa por la API (nuestro boton / F11 interceptado) o por el F11 nativo del navegador (display-mode: fullscreen).
    function enPantallaCompleta() {
        return !!document.fullscreenElement || window.matchMedia('(display-mode: fullscreen)').matches;
    }

    // Refleja el estado actual en el icono, el tooltip, el aria-label y aria-pressed.
    function actualizar() {
        var activo = enPantallaCompleta();
        boton.title = activo ? TITULO_SALIR : TITULO_ENTRAR;
        boton.setAttribute('aria-label', activo ? 'Salir de pantalla completa (F11)' : 'Pantalla completa (F11)');
        boton.setAttribute('aria-pressed', activo ? 'true' : 'false');
        if (icono) {
            icono.classList.toggle('fa-expand', !activo);
            icono.classList.toggle('fa-compress', activo);
        }
    }

    // Alterna entre entrar y salir. Las promesas rechazadas (el navegador exige un gesto del usuario, o la pantalla completa esta bloqueada)
    // se registran en consola con contexto: no hay nada mas que mostrarle al operador.
    function alternar() {
        var promesa;
        if (document.fullscreenElement) {
            promesa = document.exitFullscreen();
        } else if (enPantallaCompleta()) {
            // F11 nativo: la API no puede salir de ese modo; se deja al navegador (volver a tocar F11).
            return;
        } else {
            promesa = raiz.requestFullscreen();
        }
        if (promesa && typeof promesa.catch === 'function') {
            promesa.catch(function (error) {
                console.warn('Pantalla completa: el navegador no permitio cambiar el modo.', error);
            });
        }
    }

    boton.addEventListener('click', alternar);

    document.addEventListener('keydown', function (evento) {
        if (evento.key !== 'F11' || evento.ctrlKey || evento.altKey || evento.shiftKey || evento.metaKey) return;
        evento.preventDefault();
        alternar();
    });

    document.addEventListener('fullscreenchange', actualizar);
    var modoPantalla = window.matchMedia('(display-mode: fullscreen)');
    if (typeof modoPantalla.addEventListener === 'function') {
        modoPantalla.addEventListener('change', actualizar);
    }

    actualizar();
})();
