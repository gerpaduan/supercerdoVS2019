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
        if (options.showCancelButton === true) return false;
        if (options.showDenyButton === true) return false;
        if (options.showCloseButton === true) return false;
        if (options.input) return false;
        if (options.html && options.focusConfirm === false) return false;
        if (options.showConfirmButton === false) return false;
        return true;
    }

    window.Swal.fire = function () {
        var args = Array.prototype.slice.call(arguments);
        var options = normalizeArgs(args);

        if (!isSingleConfirmAlert(options)) {
            return originalFire.apply(null, args);
        }

        var keydownHandler = null;
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

                if (typeof originalWillClose === 'function') {
                    originalWillClose.call(this, popup);
                }
            }
        });

        return originalFire(patchedOptions);
    };

    window.Swal.__singleConfirmPatched = true;
})(window);
