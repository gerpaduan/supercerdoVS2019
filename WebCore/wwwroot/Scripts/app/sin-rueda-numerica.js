/*
 * Evita que la ruedita del mouse cambie el valor de un <input type="number"> enfocado.
 * Problema: al hacer scroll con el cursor sobre un campo numerico con foco, el navegador
 * incrementa/decrementa el valor en silencio (montos, cantidades) y se cargan datos erroneos.
 * Solucion: ante un "wheel" sobre un campo numerico enfocado se le quita el foco; el navegador
 * entonces hace scroll normal de la pagina y el valor no se toca.
 * Los spinners visuales se ocultan por CSS (wwwroot/css/site.css). Sin dependencias.
 *
 * Pedido 2026-10-21: tampoco las flechas del teclado (arriba/abajo) deben subir o bajar el valor;
 * en la carga de productos/cantidades (Movimientos, Compras, Stock, Elaborados, etc.) el numero
 * solo se escribe. Se cancela el keydown de esas dos teclas (el navegador es quien hace el paso);
 * NO se corta la propagacion, para que las listas de autocompletar que escuchan las flechas sigan
 * funcionando. Detalle: docs/DECISIONS.md (2026-10-21).
 */
(function () {
    'use strict';

    function esNumericoDeEntrada(target) {
        return target instanceof HTMLInputElement && target.type === 'number';
    }

    // captura en document: cubre tambien inputs creados dinamicamente
    document.addEventListener('wheel', function (event) {
        var target = event.target;
        if (esNumericoDeEntrada(target) && document.activeElement === target) {
            target.blur();
        }
    }, { capture: true, passive: true });

    // flechas arriba/abajo: el navegador las usa para incrementar/decrementar (step) el numerico
    document.addEventListener('keydown', function (event) {
        if ((event.key === 'ArrowUp' || event.key === 'ArrowDown') && esNumericoDeEntrada(event.target)) {
            event.preventDefault();
        }
    }, { capture: true });
})();
