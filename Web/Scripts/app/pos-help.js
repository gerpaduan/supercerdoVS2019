(function (window, $) {
    'use strict';

    function createPOSHelp(options) {
        // Este modulo encapsula la ayuda del POS y los atajos globales que
        // comparten comportamiento con ese modal.

        function cleanupBootstrapModalState() {
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open');
            $('body').css('padding-right', '');
        }

        function hasOtherOpenModal() {
            return $('.modal.show').not('#modalAyudaPOS').length > 0;
        }

        function isHelpOpen() {
            return $('#modalAyudaPOS').hasClass('show');
        }

        function openHelp() {
            if (hasOtherOpenModal()) return;
            $('#modalAyudaPOS').modal('show');
        }

        function closeHelp(callback) {
            const $modal = $('#modalAyudaPOS');

            if (!$modal.length) {
                if (typeof callback === 'function') callback();
                return;
            }

            if (!$modal.hasClass('show')) {
                cleanupBootstrapModalState();
                if (typeof callback === 'function') {
                    setTimeout(function () {
                        callback();
                    }, 0);
                }
                return;
            }

            $modal
                .off('hidden.bs.modal.posAyudaAccion')
                .one('hidden.bs.modal.posAyudaAccion', function () {
                    cleanupBootstrapModalState();

                    if (typeof callback === 'function') {
                        setTimeout(function () {
                            callback();
                        }, 80);
                    }
                });

            $modal.modal('hide');
        }

        function runHook(key) {
            if (window.posHotkeysHooks && typeof window.posHotkeysHooks[key] === 'function') {
                window.posHotkeysHooks[key]();
                return true;
            }

            $(document).trigger('pos:atajo:' + key, [{ clave: key }]);
            return false;
        }

        function runAction(key, closeFirst) {
            const run = function () {
                switch (key) {
                    case 'Home':
                        options.focusCodigo();
                        break;
                    case 'End':
                        options.clickIfEnabled('#btnFinalizar');
                        break;
                    case 'F9':
                        options.clickIfEnabled('#btnBuscarPersona');
                        break;
                    case 'F10':
                        if (typeof window.abrirBuscadorProductosPOS === 'function') {
                            window.abrirBuscadorProductosPOS();
                        }
                        break;
                    case 'F2':
                    case 'F3':
                    case 'F4':
                    case 'F5':
                    case 'F6':
                    case 'F7':
                    case 'F8':
                    case 'AvPag':
                        runHook(key);
                        break;
                }
            };

            if (closeFirst && isHelpOpen()) {
                closeHelp(run);
            } else {
                run();
            }
        }

        function bindHelpButtons() {
            $(document)
                .off('click.posAyudaBtn', '#btnAyuda')
                .on('click.posAyudaBtn', '#btnAyuda', function (e) {
                    e.preventDefault();
                    openHelp();
                });

            $(document)
                .off('click.posAyudaAction', '.pos-help-action')
                .on('click.posAyudaAction', '.pos-help-action', function () {
                    const action = $(this).data('accion');
                    runAction(action, true);
                });
        }

        function bindModalLifecycle() {
            $('#modalAyudaPOS')
                .off('hidden.bs.modal.posAyuda')
                .on('hidden.bs.modal.posAyuda', function () {
                    setTimeout(function () {
                        cleanupBootstrapModalState();

                        if (!$('.modal.show').length) {
                            options.focusCodigo();
                        }
                    }, 0);
                });
        }

        function bindKeyboardShortcuts() {
            document.addEventListener('keydown', function (e) {
                const key = e.key;
                const helpOpen = isHelpOpen();
                const otherModalOpen = hasOtherOpenModal();

                if (key === 'F1') {
                    e.preventDefault();

                    if (helpOpen) {
                        closeHelp();
                    } else if (!otherModalOpen) {
                        openHelp();
                    }
                    return;
                }

                if (otherModalOpen && !helpOpen) {
                    return;
                }

                if (key === 'F2' || key === 'F3' || key === 'F4' || key === 'F5' || key === 'F6' || key === 'F7' || key === 'F8' || key === 'PageDown') {
                    e.preventDefault();
                    runAction(key === 'PageDown' ? 'AvPag' : key, false);
                    return;
                }

                if (helpOpen) {
                    if (key === 'Escape') {
                        e.preventDefault();
                        closeHelp();
                        return;
                    }

                    if (key === 'Home' || key === 'End' || key === 'F9' || key === 'F10' || key === 'PageDown') {
                        e.preventDefault();
                        runAction(key === 'PageDown' ? 'AvPag' : key, true);
                    }
                }
            }, true);
        }

        const api = {
            init: function () {
                window.posHotkeysHooks = window.posHotkeysHooks || {};

                bindHelpButtons();
                bindModalLifecycle();
                bindKeyboardShortcuts();
            },
            openHelp: openHelp,
            closeHelp: closeHelp
        };

        return api;
    }

    window.POSHelp = {
        create: createPOSHelp
    };
})(window, window.jQuery);
