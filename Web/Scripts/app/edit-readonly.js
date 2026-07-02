(function (window, $) {
    'use strict';

    if (!window || !$) return;

    function isManagedControl($el, settings) {
        if (!$el || !$el.length) return false;
        if ($el.is('[type="hidden"]')) return false;
        if ($el.is(settings.ignoreSelector)) return false;
        return true;
    }

    function isTextLike($el) {
        return $el.is('textarea') ||
            $el.is('input[type="text"]') ||
            $el.is('input[type="number"]') ||
            $el.is('input[type="datetime-local"]') ||
            $el.is('input[type="date"]') ||
            $el.is('input[type="time"]') ||
            $el.is('input[type="email"]') ||
            $el.is('input[type="tel"]') ||
            $el.is('input[type="search"]') ||
            $el.is('input[type="url"]') ||
            $el.is('input:not([type])');
    }

    function rememberOriginalState($controls) {
        $controls.each(function () {
            var $el = $(this);
            if ($el.data('editReadonlyTracked')) return;

            $el.data('editReadonlyTracked', true);
            $el.data('editReadonlyOriginalDisabled', $el.prop('disabled'));
            $el.data('editReadonlyOriginalReadonly', $el.prop('readonly'));
        });
    }

    function restoreOriginalState($controls) {
        $controls.each(function () {
            var $el = $(this);
            if (!$el.data('editReadonlyTracked')) return;

            $el.prop('disabled', !!$el.data('editReadonlyOriginalDisabled'));
            $el.prop('readonly', !!$el.data('editReadonlyOriginalReadonly'));
            $el.removeClass('edit-readonly-field');
        });
    }

    function applyReadOnly($controls, settings) {
        $controls.each(function () {
            var $el = $(this);
            if (!isManagedControl($el, settings)) return;

            if (isTextLike($el)) {
                $el.prop('readonly', true);
            } else {
                $el.prop('disabled', true);
            }

            $el.addClass('edit-readonly-field');
        });
    }

    function focusFirstEditable($form, settings) {
        var $target = $form.find(settings.focusSelector).filter(':visible').filter(function () {
            var $el = $(this);
            return !$el.prop('disabled') && !$el.prop('readonly');
        }).first();

        if (!$target.length) return;

        window.setTimeout(function () {
            $target.trigger('focus');
            if ($target.is('input[type="text"], input[type="number"], textarea')) {
                $target.trigger('select');
            }
        }, 40);
    }

    function init(options) {
        var settings = $.extend({
            formSelector: '',
            soloLecturaInicial: false,
            modifyButtonSelector: '',
            saveButtonSelector: '',
            ignoreSelector: '[data-edit-readonly-ignore="true"]',
            focusSelector: 'input, select, textarea',
            onStateChanged: null
        }, options || {});

        var $form = $(settings.formSelector);
        if (!$form.length) return null;

        var $controls = $form.find('input, select, textarea, button');
        var $modify = settings.modifyButtonSelector ? $(settings.modifyButtonSelector) : $();
        var $save = settings.saveButtonSelector ? $(settings.saveButtonSelector) : $();
        var state = {
            readOnly: !!settings.soloLecturaInicial
        };

        rememberOriginalState($controls);

        function syncButtons() {
            if ($save.length) {
                $save.toggleClass('d-none', state.readOnly);
                $save.prop('disabled', state.readOnly);
            }

            if ($modify.length) {
                $modify.toggleClass('d-none', !state.readOnly);
                $modify.prop('disabled', false);
            }
        }

        function setReadOnly(readOnly) {
            state.readOnly = !!readOnly;

            restoreOriginalState($controls);

            if (state.readOnly) {
                applyReadOnly($controls, settings);
                $form.addClass('edit-readonly-active');
            } else {
                $form.removeClass('edit-readonly-active');
            }

            syncButtons();

            if (!state.readOnly) {
                focusFirstEditable($form, settings);
            }

            if (typeof settings.onStateChanged === 'function') {
                settings.onStateChanged(state.readOnly, $form);
            }
        }

        $form.off('submit.editReadonly').on('submit.editReadonly', function (evt) {
            if (!state.readOnly) return;
            evt.preventDefault();
            return false;
        });

        if ($modify.length) {
            $modify.off('click.editReadonly').on('click.editReadonly', function () {
                setReadOnly(false);
            });
        }

        setReadOnly(state.readOnly);

        if (window.EditPageGuard && typeof window.EditPageGuard.init === 'function') {
            window.EditPageGuard.init({
                formSelector: settings.formSelector,
                modifyButtonSelector: settings.modifyButtonSelector,
                saveButtonSelector: settings.saveButtonSelector
            });
        }

        return {
            setReadOnly: setReadOnly,
            isReadOnly: function () { return state.readOnly; }
        };
    }

    window.EditReadOnly = window.EditReadOnly || {
        init: init
    };
})(window, window.jQuery);
