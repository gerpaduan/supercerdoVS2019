/*
 * Evita que la ruedita del mouse cambie el valor de un <input type="number"> enfocado.
 * Problema: al hacer scroll con el cursor sobre un campo numerico con foco, el navegador
 * incrementa/decrementa el valor en silencio (montos, cantidades) y se cargan datos erroneos.
 * Solucion: ante un "wheel" sobre un campo numerico enfocado se le quita el foco; el navegador
 * entonces hace scroll normal de la pagina y el valor no se toca.
 * Los spinners visuales se ocultan por CSS (wwwroot/css/site.css). Sin dependencias.
 */
(function () {
    'use strict';

    // captura en document: cubre tambien inputs creados dinamicamente
    document.addEventListener('wheel', function (event) {
        var target = event.target;
        if (target instanceof HTMLInputElement &&
            target.type === 'number' &&
            document.activeElement === target) {
            target.blur();
        }
    }, { capture: true, passive: true });
})();
