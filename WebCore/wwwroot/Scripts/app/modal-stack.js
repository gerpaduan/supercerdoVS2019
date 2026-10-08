// Apilado de modales de Bootstrap 5 (un modal que se abre SOBRE otro ya abierto), para toda la app.
//
// Problema de fondo: custom.css fija TODOS los .modal en z-index 1050 !important y TODOS los .modal-backdrop en
// 1040 !important, asi que un modal abierto sobre otro empata en z-index y gana el que esta mas abajo en el DOM
// (queda detras, invisible, y su FocusTrap le saca el foco al de arriba: "pantalla bloqueada"). Cada pantalla lo
// resolvia por su cuenta (posPagoStack, cajasStack, ctacteCompra, compras.js, calculadora, traerModalFacturaAlFrente...)
// con el mismo patron fragil: "z-index = 1040 + 10 * cantidad de modales" y "marcar el ultimo backdrop sin la clase
// modal-stack". Ese marcado a veces le caia al backdrop del modal de ATRAS (z-index mayor que el de su propio modal:
// lo cubria y bloqueaba todo) y tres de las copias usaban .css() sin !important, que no gana contra custom.css.
// Ver docs/DECISIONS.md (2026-10-06).
//
// Regla unica (aca y solo aca):
//  1. Al abrirse un modal (show.bs.modal), si hay otros abiertos: z-index = el mayor de los abiertos + 10, con
//     !important. Su backdrop (siempre el ULTIMO que Bootstrap agrega al body) queda 1 punto abajo. Se marca UNA vez
//     por apertura (bandera) y solo si el modal realmente se esta mostrando.
//  2. Al abrir/cerrar cualquier modal se sanea: ningun backdrop puede tener un z-index igual o mayor al del modal abierto
//     mas alto (si lo tiene, pierde el valor forzado). Y si al cerrar uno quedan otros abiertos, se restaura modal-open
//     del <body> (Bootstrap lo saca) y, si no queda ningun backdrop, se agrega uno.
// Depende de: jQuery, Bootstrap 5 (bootstrap.bundle). No cambia nada si no hay modales apilados.
(function (window, $) {
    'use strict';

    if (!$ || window.ModalStack) return;

    function zDe(el) {
        return parseInt(window.getComputedStyle(el).zIndex, 10) || 0;
    }

    // Mayor z-index entre los modales abiertos, sin contar `excluir`.
    function maxZAbiertos(excluir) {
        var maxZ = 0;
        Array.prototype.forEach.call(document.querySelectorAll('.modal.show'), function (el) {
            if (el !== excluir) maxZ = Math.max(maxZ, zDe(el));
        });
        return maxZ;
    }

    // true si Bootstrap ya acepto mostrar este modal (show.bs.modal no fue cancelado). _isShown es interno de
    // Bootstrap (igual que _focustrap, que ya usan calculadora-billetes.js y swal-single-confirm.js).
    function seEstaMostrando(modalEl) {
        var instancia = window.bootstrap && window.bootstrap.Modal ? window.bootstrap.Modal.getInstance(modalEl) : null;
        return !!(instancia && instancia._isShown);
    }

    // Marca el backdrop de ESTE modal: el ultimo del body. Idempotente por apertura.
    function marcarBackdrop(modalEl, zIndex) {
        if (modalEl.__modalStackMarcado) return;
        if (!seEstaMostrando(modalEl)) return;
        modalEl.__modalStackMarcado = true;

        var backdrops = document.querySelectorAll('.modal-backdrop');
        if (!backdrops.length) return;

        var propio = backdrops[backdrops.length - 1];
        propio.style.setProperty('z-index', zIndex - 1, 'important');
        propio.classList.add('modal-stack');
    }

    function elevar(modalEl) {
        if (!modalEl) return;

        modalEl.style.removeProperty('z-index');
        modalEl.__modalStackMarcado = false;

        var maxZ = maxZAbiertos(modalEl);
        if (maxZ < 1) return; // nada abierto debajo: sirve el z-index normal de custom.css

        var zIndex = maxZ + 10;
        modalEl.style.setProperty('z-index', zIndex, 'important');

        // El backdrop se agrega durante el show: se marca justo despues y, si todavia no se estaba mostrando (el modal
        // se abrio un poco despues de llamar a elevar), al terminar de mostrarse.
        window.setTimeout(function () { marcarBackdrop(modalEl, zIndex); }, 0);
        $(modalEl).off('shown.bs.modal.modalStack').one('shown.bs.modal.modalStack', function () {
            marcarBackdrop(modalEl, zIndex);
        });
    }

    // Ningun backdrop al mismo nivel o por encima del modal abierto mas alto: lo cubriria y dejaria la pantalla oscura y
    // bloqueada. Un backdrop "de mas" pierde el z-index forzado y vuelve al normal.
    function sanear() {
        var topZ = maxZAbiertos(null);
        if (topZ < 1) return;

        Array.prototype.forEach.call(document.querySelectorAll('.modal-backdrop'), function (el) {
            if (zDe(el) >= topZ) {
                el.style.removeProperty('z-index');
                el.classList.remove('modal-stack');
            }
        });
    }

    // Bootstrap, al cerrar un modal, saca modal-open del <body> y puede dejar sin backdrop a los que siguen abiertos.
    function restaurar() {
        if (!document.querySelector('.modal.show')) return;

        document.body.classList.add('modal-open');
        if (!document.querySelector('.modal-backdrop')) {
            var fondo = document.createElement('div');
            fondo.className = 'modal-backdrop fade show';
            document.body.appendChild(fondo);
        }
    }

    window.ModalStack = { elevar: elevar, sanear: sanear, restaurar: restaurar };

    // Global: cualquier modal que se abra sobre otro se apila solo; y se sanea al abrir/cerrar cualquiera.
    $(document)
        .off('.modalStack')
        .on('show.bs.modal.modalStack', '.modal', function (e) {
            if (e.target !== this) return; // eventos burbujeados de modales anidados dentro de este
            elevar(this);
        })
        .on('shown.bs.modal.modalStack', '.modal', function (e) {
            if (e.target === this) sanear();
        })
        .on('hidden.bs.modal.modalStack', '.modal', function (e) {
            if (e.target !== this) return;
            sanear();
            restaurar();
        });
})(window, window.jQuery);
