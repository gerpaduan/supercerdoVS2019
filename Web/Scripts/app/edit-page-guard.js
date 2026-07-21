(function (window, $) {
    'use strict';

    if (!window || !$) return;

    var guards = [];
    var keyboardHookAttached = false;
    var syncTimerAttached = false;

    function isVisibleAction($el) {
        return !!($el && $el.length && $el.is(':visible') && !$el.prop('disabled') && !$el.hasClass('d-none'));
    }

    function buildSnapshot($form) {
        if (!$form || !$form.length) return '';
        return $form.serialize();
    }

    function getTopVisibleModal() {
        return $('.modal.show:visible').last();
    }

    function isElementInside($element, $container) {
        return !!($element && $element.length && $container && $container.length && $.contains($container[0], $element[0]));
    }

    function resolveGuardFromTarget(target) {
        var $target = $(target || document.activeElement);
        var $topModal = getTopVisibleModal();
        var $form = $target.closest('form');

        if ($topModal.length) {
            if ($target.length && !isElementInside($target, $topModal) && !$target.is($topModal)) {
                $target = $topModal;
                $form = $topModal.find('form:visible').first();
            } else if (!$form.length) {
                $form = $topModal.find('form:visible').first();
            }
        }

        if ($form.length) {
            for (var i = 0; i < guards.length; i++) {
                if (guards[i].$form.length && guards[i].$form[0] === $form[0]) {
                    return guards[i];
                }
            }
        }

        for (var j = 0; j < guards.length; j++) {
            if (guards[j].$form.is(':visible')) {
                return guards[j];
            }
        }

        return guards.length ? guards[0] : null;
    }

    function syncGlobalProtection() {
        var dirty = false;

        for (var i = 0; i < guards.length; i++) {
            if (guards[i].isDirty()) {
                dirty = true;
                break;
            }
        }

        window.__protegerSalida = dirty;
    }

    function handleShortcut(evt) {
        if (!(evt.altKey && !evt.ctrlKey && !evt.metaKey)) return;
        if (evt.shiftKey || evt.repeat) return;

        var key = String(evt.key || '').toLowerCase();
        var isSaveShortcut = key === 'g' || key === 'enter';
        if (!isSaveShortcut) return;

        if (window.Swal && typeof window.Swal.isVisible === 'function' && window.Swal.isVisible()) {
            return;
        }

        var $topModal = getTopVisibleModal();
        var $target = $(evt.target || document.activeElement);
        if ($topModal.length && $target.length && !isElementInside($target, $topModal) && !$target.is($topModal)) {
            return;
        }

        var guard = resolveGuardFromTarget(evt.target);
        if (!guard || !guard.$form.length) return;

        evt.preventDefault();
        evt.stopPropagation();
        if (typeof evt.stopImmediatePropagation === 'function') {
            evt.stopImmediatePropagation();
        }

        if (isVisibleAction(guard.$modify)) {
            guard.$modify.trigger('click');
            return;
        }

        if (isVisibleAction(guard.$save)) {
            if (guard.$form[0] && typeof guard.$form[0].requestSubmit === 'function') {
                guard.$form[0].requestSubmit(guard.$save[0]);
                return;
            }

            guard.$save.trigger('click');
            return;
        }

        guard.allowExit = true;
        syncGlobalProtection();
        guard.$form.trigger('submit');
    }

    function attachKeyboardHook() {
        if (keyboardHookAttached) return;
        keyboardHookAttached = true;

        document.addEventListener('keydown', handleShortcut, true);
    }

    function attachSyncTimer() {
        if (syncTimerAttached) return;
        syncTimerAttached = true;

        window.setInterval(function () {
            syncGlobalProtection();
        }, 700);
    }

    function init(options) {
        var settings = $.extend({
            formSelector: '',
            saveButtonSelector: '',
            modifyButtonSelector: ''
        }, options || {});

        if (!settings.formSelector) return null;

        var $form = $(settings.formSelector).first();
        if (!$form.length) return null;

        var existing = $form.data('editPageGuardApi');
        if (existing) return existing;

        var guard = {
            $form: $form,
            $save: settings.saveButtonSelector ? $(settings.saveButtonSelector).first() : $(),
            $modify: settings.modifyButtonSelector ? $(settings.modifyButtonSelector).first() : $(),
            allowExit: false,
            initialSnapshot: buildSnapshot($form)
        };

        guard.isDirty = function () {
            if (guard.allowExit) return false;
            return buildSnapshot(guard.$form) !== guard.initialSnapshot;
        };

        guard.markClean = function () {
            guard.allowExit = false;
            guard.initialSnapshot = buildSnapshot(guard.$form);
            syncGlobalProtection();
        };

        guard.allowNavigation = function () {
            guard.allowExit = true;
            syncGlobalProtection();
        };

        $form.on('input.editPageGuard change.editPageGuard', ':input', function () {
            syncGlobalProtection();
        });

        $form.on('submit.editPageGuard', function () {
            guard.allowExit = true;
            syncGlobalProtection();

            window.setTimeout(function () {
                if (!document.body.contains(guard.$form[0])) return;
                guard.allowExit = false;
                syncGlobalProtection();
            }, 1500);
        });

        guards.push(guard);
        attachKeyboardHook();
        attachSyncTimer();
        syncGlobalProtection();

        var api = {
            isDirty: guard.isDirty,
            markClean: guard.markClean,
            allowNavigation: guard.allowNavigation
        };

        $form.data('editPageGuardApi', api);
        return api;
    }

    window.EditPageGuard = window.EditPageGuard || {
        init: init,
        sync: syncGlobalProtection
    };
})(window, window.jQuery);
