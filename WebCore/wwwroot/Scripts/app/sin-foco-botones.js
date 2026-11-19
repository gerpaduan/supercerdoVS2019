/* Botones de accion sin foco (2026-11-19): Guardar, Cancelar, Modificar y Anular de Movimientos, Stock, Elaborados (nuevo e ingreso rapido) y Compras
   solo responden a un CLIC sobre el boton o al atajo de la pantalla (Alt+Enter, Alt+G, Alt+C). Nunca toman el foco del teclado:
   - El HTML marca cada boton con data-sin-foco + tabindex="-1" (Tab no se detiene ahi).
   - mousedown/pointerdown con preventDefault: el clic sigue disparando "click", pero el navegador no le pasa el foco (sin recuadro de foco
     y un Enter/Espacio posterior no lo vuelve a activar). El foco queda donde estaba el cursor de trabajo.
   - Red de seguridad: si algo (otro script, lector de pantalla) le diera el foco igual, se lo saca.
   - Enter dentro de un campo del formulario NO lo envia (el navegador hace un "clic" implicito sobre el primer boton de envio = Guardar). Los
     campos que usan Enter para algo (Codigo, Cantidad, editor de linea...) lo manejan ellos mismos antes de llegar aca.
   Idempotente: Compras se carga dentro de modales (POS, cuenta corriente) y este script puede incluirse mas de una vez.
   Dependencias: ninguna (JS plano, delegado en document, sirve para botones que otro script mueve de lugar, ej. pantalla-trabajo.js). */
(function () {
    'use strict';

    if (window.__sinFocoBotones) return;
    window.__sinFocoBotones = true;

    var SELECTOR = '[data-sin-foco]';

    function botonSinFoco(target) {
        return target && target.closest ? target.closest(SELECTOR) : null;
    }

    // Clic con el mouse o el dedo: se evita el foco, el "click" sigue su curso normal.
    ['mousedown', 'pointerdown'].forEach(function (nombre) {
        document.addEventListener(nombre, function (e) {
            if (botonSinFoco(e.target)) e.preventDefault();
        }, true);
    });

    // Red de seguridad: un foco por cualquier otra via se devuelve al instante.
    document.addEventListener('focusin', function (e) {
        var boton = botonSinFoco(e.target);
        if (boton && typeof boton.blur === 'function') boton.blur();
    }, true);

    // Enter dentro de un campo no guarda: se corta el envio implicito del formulario.
    // Se excluyen: textarea (Enter = salto de linea), botones (Enter = clic explicito) y archivos.
    // Solo actua en formularios que tienen un boton marcado, y no toca eventos de los modales (estan fuera del <form>).
    var TIPOS_QUE_NO_ENVIAN = /^(button|submit|reset|image|file)$/i;
    document.addEventListener('keydown', function (e) {
        if (e.key !== 'Enter' || e.altKey || e.ctrlKey || e.metaKey || e.shiftKey) return;
        var campo = e.target;
        if (!campo || campo.tagName !== 'INPUT' || TIPOS_QUE_NO_ENVIAN.test(campo.type || '')) return;
        var form = campo.form;
        if (!form || !form.querySelector(SELECTOR)) return;
        e.preventDefault();
    }, true); // captura: corta el envio aunque otro manejador haga stopPropagation
})();
