(function (window) {
    'use strict';

    if (!window.Swal || typeof window.Swal.fire !== 'function' || window.Swal.__singleConfirmPatched === true) {
        return;
    }

    var originalFire = window.Swal.fire.bind(window.Swal);

    function normalizeArgs(args) {
        if (!args || !args.length) return {};

        if (typeof args[0] === 'object' && args[0] !== null) {
            return args[0];
        }

        return {
            title: args[0],
            text: args.length > 1 ? args[1] : undefined,
            icon: args.length > 2 ? args[2] : undefined
        };
    }

    function isSingleConfirmAlert(options) {
        if (!options || typeof options !== 'object') return false;
        if (options.toast === true) return false;
        // showCancelButton ya NO excluye (2026-09-10, pedido explicito del usuario -- ver
        // docs/DECISIONS.md): "todos los sweetalert que tengan boton OK, agregar el atajo Enter".
        // El listener de keydown de este archivo no depende de donde este el foco real (dispara
        // Swal.clickConfirm() directo en fase de captura sobre document) -- pero el foco SI importa
        // para que el navegador no procese Enter como "click nativo sobre el boton actualmente
        // enfocado" antes de que este listener llegue a hacer preventDefault (ver willOpen mas
        // abajo, fix real 2026-09-11 para el caso con un modal Bootstrap de fondo).
        if (options.showDenyButton === true) return false;      // 3 botones: ambiguo cual confirma con Enter
        if (options.showCloseButton === true) return false;
        if (options.input) return false;                        // prompts: Enter tiene otro significado
        if (options.html && options.focusConfirm === false) return false;
        if (options.showConfirmButton === false) return false;
        return true;
    }

    // Desactiva el FocusTrap de cada modal de Bootstrap 5 abierto (API real del bundle:
    // bootstrap.Modal.getInstance(el)._focustrap.deactivate()) y devuelve las instancias afectadas
    // para reactivarlas despues. Sin esto, el trap le devuelve el foco al modal de fondo cada vez
    // que el usuario hace clic en un input del Swal, que queda "dibujado" pero sin poder escribirse.
    function suspenderFocusTrap() {
        var afectadas = [];
        var modales = document.querySelectorAll('.modal.show');
        for (var i = 0; i < modales.length; i++) {
            var instancia = window.bootstrap && window.bootstrap.Modal ? window.bootstrap.Modal.getInstance(modales[i]) : null;
            if (instancia && instancia._focustrap && typeof instancia._focustrap.deactivate === 'function') {
                instancia._focustrap.deactivate();
                afectadas.push(instancia);
            }
        }
        return afectadas;
    }

    function reactivarFocusTrap(afectadas) {
        for (var i = 0; i < afectadas.length; i++) {
            var instancia = afectadas[i];
            if (instancia && instancia._focustrap && typeof instancia._focustrap.activate === 'function') {
                instancia._focustrap.activate();
            }
        }
    }

    window.Swal.fire = function () {
        var args = Array.prototype.slice.call(arguments);
        var options = normalizeArgs(args);

        if (!isSingleConfirmAlert(options)) {
            // Fix real (2026-10-05, ver docs/DECISIONS.md): los Swals que NO son de confirmacion simple
            // (prompts con input, html con focusConfirm:false como el de supervisor, 3 botones) tambien
            // se abren sobre modales Bootstrap con su focus trap activo -- el input del motivo de
            // "Descartar borrador" no recibia foco. Solo se suspende el trap: se conserva el resto del
            // comportamiento (sin atajo Enter, sin forzar foco al boton confirmar). Toasts no toman foco.
            if (options.toast === true) {
                return originalFire.apply(null, args);
            }

            var trapsSuspendidos = [];
            var willOpenOriginal = options.willOpen;
            var willCloseOriginal = options.willClose;

            return originalFire(Object.assign({}, options, {
                willOpen: function (popup) {
                    trapsSuspendidos = suspenderFocusTrap();
                    if (typeof willOpenOriginal === 'function') willOpenOriginal.call(this, popup);
                },
                willClose: function (popup) {
                    reactivarFocusTrap(trapsSuspendidos);
                    trapsSuspendidos = [];
                    if (typeof willCloseOriginal === 'function') willCloseOriginal.call(this, popup);
                }
            }));
        }

        var keydownHandler = null;
        var modalesConFocusTrapDesactivado = [];
        var originalWillOpen = options.willOpen;
        var originalDidOpen = options.didOpen;
        var originalWillClose = options.willClose;

        var patchedOptions = Object.assign({}, options, {
            focusConfirm: true,
            allowEnterKey: true,
            allowEscapeKey: true,
            returnFocus: false,
            keydownListenerCapture: true,
            willOpen: function (popup) {
                // Bug real (2026-09-11, ver docs/DECISIONS.md): la app corre Bootstrap 5 real (no
                // 4). Cada modal de Bootstrap 5 instancia su propio FocusTrap (bootstrap.bundle.js,
                // clase FocusTrap), que reafirma el foco DENTRO de su elemento via un listener
                // propio de Bootstrap sobre "focusin" en document (su propio EventHandler interno,
                // NO jQuery -- $(document).off('focusin.bs.modal'), el workaround puntual que tenia
                // factura-electronica.js y que se saco en una ronda anterior, es un no-op total
                // contra el mecanismo real: namespace equivocado ("bs.focustrap", no "bs.modal") y
                // sistema de eventos equivocado (nativo, no jQuery) -- confirmado leyendo el bundle
                // real y con un test que mostro ~14 idas y vueltas de foco por segundo entre el
                // boton de SweetAlert2 y el modal de fondo). Se desactiva el FocusTrap de cada
                // modal de Bootstrap actualmente abierto usando su API PUBLICA real
                // (bootstrap.Modal.getInstance(el)._focustrap.deactivate()) mientras este Swal este
                // abierto, y se reactiva en willClose -- a diferencia del viejo workaround (que
                // "no hacia falta restaurar a mano" porque el modal de fondo se cerraba del todo),
                // aca el modal de fondo NO se cierra, sigue abierto detras del Swal, asi que su
                // focus trap si hay que reactivarlo a mano al cerrar.
                var modales = document.querySelectorAll('.modal.show');
                for (var i = 0; i < modales.length; i++) {
                    var instancia = window.bootstrap && window.bootstrap.Modal ? window.bootstrap.Modal.getInstance(modales[i]) : null;
                    if (instancia && instancia._focustrap && typeof instancia._focustrap.deactivate === 'function') {
                        instancia._focustrap.deactivate();
                        modalesConFocusTrapDesactivado.push(instancia);
                    }
                }

                var active = document.activeElement;
                if (active && typeof active.blur === 'function') {
                    active.blur();
                }

                if (typeof originalWillOpen === 'function') {
                    originalWillOpen.call(this, popup);
                }
            },
            didOpen: function (popup) {
                keydownHandler = function (evt) {
                    if (!evt) return;
                    if (evt.key !== 'Enter' && evt.key !== 'Escape') return;
                    // Bug real reportado 2026-09-11 (ver docs/DECISIONS.md): con un Swal de
                    // confirmacion abierto, Alt+Enter (atajo de "guardar" en las vistas de
                    // alta/edicion) se tratraba como Enter simple -- confirmaba este Swal y
                    // cortaba la propagacion ANTES de que el Alt+Enter real llegara al handler de
                    // la vista de atras. Con un modificador presente, este listener no debe
                    // interceptar nada: solo maneja Enter/Escape "limpios".
                    if (evt.altKey || evt.ctrlKey || evt.metaKey) return;

                    if (typeof evt.preventDefault === 'function') evt.preventDefault();
                    if (typeof evt.stopPropagation === 'function') evt.stopPropagation();
                    if (typeof evt.stopImmediatePropagation === 'function') evt.stopImmediatePropagation();

                    if (evt.key === 'Escape') {
                        window.Swal.close();
                        return;
                    }

                    if (typeof window.Swal.clickConfirm === 'function') {
                        window.Swal.clickConfirm();
                    }
                };

                document.addEventListener('keydown', keydownHandler, true);

                setTimeout(function () {
                    var confirmButton = window.Swal.getConfirmButton ? window.Swal.getConfirmButton() : null;
                    if (confirmButton && typeof confirmButton.focus === 'function') {
                        confirmButton.focus();
                    }
                }, 0);

                if (typeof originalDidOpen === 'function') {
                    originalDidOpen.call(this, popup);
                }
            },
            willClose: function (popup) {
                if (keydownHandler) {
                    document.removeEventListener('keydown', keydownHandler, true);
                    keydownHandler = null;
                }

                for (var i = 0; i < modalesConFocusTrapDesactivado.length; i++) {
                    var instanciaCerrar = modalesConFocusTrapDesactivado[i];
                    if (instanciaCerrar && instanciaCerrar._focustrap && typeof instanciaCerrar._focustrap.activate === 'function') {
                        instanciaCerrar._focustrap.activate();
                    }
                }
                modalesConFocusTrapDesactivado = [];

                if (typeof originalWillClose === 'function') {
                    originalWillClose.call(this, popup);
                }
            }
        });

        return originalFire(patchedOptions);
    };

    window.Swal.__singleConfirmPatched = true;
})(window);
