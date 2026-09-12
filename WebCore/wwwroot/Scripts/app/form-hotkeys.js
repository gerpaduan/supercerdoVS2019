// Modulo compartido de atajos Alt+Enter=Guardar / Alt+C=Cancelar (Batch 5, 2026-09-11, ver
// docs/DECISIONS.md "atajos rotos/inconsistentes"). Mapeo confirmado por el usuario, unico y
// obligatorio para toda la app -- ver Views/Productos/AddOrEdit.cshtml (que ya lo aplica con su
// propio handler local, corregido en el mismo batch) y Scripts/app/compras.js (keydown.
// comprasHotkeys, ya lo hacia bien desde antes). Este modulo es el primer punto de entrada
// REUTILIZABLE para el resto de las vistas de alta/edicion -- se aplica ahora solo a
// Finanzas/AddOrEditPago.cshtml (unico consumidor real en este batch); migrar las demas ~18
// vistas que ya tienen su propia implementacion local queda como pasada aparte, documentada como
// pendiente (no se tocan en este batch).
//
// Reusa el mismo criterio de "modal abierto" que Scripts/app/edit-page-guard.js
// (getTopVisibleModal/resolveGuardFromTarget): si hay un modal visible, los candidatos de
// guardar/cancelar solo se buscan si el foco esta dentro de ese modal (o es el modal mismo) --
// evita que Alt+Enter en un formulario de fondo dispare el guardado de una vista que quedo
// abierta detras. Helpers duplicados a proposito (no hay modulo de utils compartido en este
// proyecto, mismo criterio que ResolverOperadorPOS repetido por controller).
(function (window, $) {
    'use strict';

    if (!window || !$) return;

    var registros = [];
    var hookAttached = false;

    function isVisibleAction($el) {
        return !!($el && $el.length && $el.is(':visible') && !$el.prop('disabled') && !$el.hasClass('d-none'));
    }

    function getTopVisibleModal() {
        return $('.modal.show:visible').last();
    }

    function isElementInside($element, $container) {
        return !!($element && $element.length && $container && $container.length && $.contains($container[0], $element[0]));
    }

    function resolveRegistroFromTarget(target) {
        var $target = $(target || document.activeElement);
        var $topModal = getTopVisibleModal();
        var $form = $target.closest('form');

        if ($topModal.length) {
            if ($target.length && !isElementInside($target, $topModal) && !$target.is($topModal)) {
                $form = $topModal.find('form:visible').first();
            } else if (!$form.length) {
                $form = $topModal.find('form:visible').first();
            }
        }

        if ($form.length) {
            for (var i = 0; i < registros.length; i++) {
                if (registros[i].$form.length && registros[i].$form[0] === $form[0]) {
                    return registros[i];
                }
            }
        }

        for (var j = 0; j < registros.length; j++) {
            if (registros[j].$form.is(':visible')) {
                return registros[j];
            }
        }

        return registros.length ? registros[0] : null;
    }

    function handleKeydown(evt) {
        if (!evt || !evt.altKey || evt.ctrlKey || evt.metaKey || evt.shiftKey || evt.repeat) return;

        var key = String(evt.key || '').toLowerCase();
        var esGuardar = key === 'enter';
        var esCancelar = key === 'c';
        if (!esGuardar && !esCancelar) return;

        if (window.Swal && typeof window.Swal.isVisible === 'function' && window.Swal.isVisible()) return;

        var $topModal = getTopVisibleModal();
        var $target = $(evt.target || document.activeElement);
        if ($topModal.length && $target.length && !isElementInside($target, $topModal) && !$target.is($topModal)) return;

        var registro = resolveRegistroFromTarget(evt.target);
        if (!registro) return;

        var $destino = esGuardar ? registro.$save : registro.$cancel;
        if (!isVisibleAction($destino)) return;

        evt.preventDefault();
        evt.stopPropagation();
        if (typeof evt.stopImmediatePropagation === 'function') evt.stopImmediatePropagation();

        if (esGuardar && registro.$form[0] && typeof registro.$form[0].requestSubmit === 'function' && $destino.is('button[type="submit"], input[type="submit"]')) {
            registro.$form[0].requestSubmit($destino[0]);
            return;
        }

        $destino.trigger('click');
    }

    function attachHook() {
        if (hookAttached) return;
        hookAttached = true;
        document.addEventListener('keydown', handleKeydown, true);
    }

    function init(options) {
        var settings = $.extend({
            formSelector: '',
            saveSelector: '',
            cancelSelector: ''
        }, options || {});

        if (!settings.formSelector) return;
        var $form = $(settings.formSelector).first();
        if (!$form.length) return;

        if ($form.data('formHotkeysInit')) return;
        $form.data('formHotkeysInit', true);

        registros.push({
            $form: $form,
            $save: settings.saveSelector ? $(settings.saveSelector).first() : $(),
            $cancel: settings.cancelSelector ? $(settings.cancelSelector).first() : $()
        });

        attachHook();
    }

    window.FormHotkeys = window.FormHotkeys || { init: init };
})(window, window.jQuery);
