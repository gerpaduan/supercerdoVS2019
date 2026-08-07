(function (window, document, $) {
    'use strict';

    var STORAGE_PREFIX = 'carnisys-pos-multi-v1';
    var COMMAND_KEY = 'carnisys-pos-multi-command-v1';
    var POSTVENTA_REDIRECT_PREFIX = 'carnisys-pos-postventa-redirect-v1';
    var PWA_SESSION_PREFIX = 'carnisys-pos-pwa-session-v1';
    var HEARTBEAT_MS = 4000;
    var STALE_MS = 15000;
    var heartbeatTimer = null;
    var blocked = false;
    var suppressReleaseOnUnload = false;

    function config() {
        return window.POSMultiInstanceConfig || {};
    }

    function now() {
        return Date.now();
    }

    // Namespace por producto (Venta vs Expendio) dentro de la misma clave de
    // usuario+sucursal. Sin esto, abrir POS Expendio en una pestaña y POS
    // Venta en otra (mismo cajero) se detectaban como el MISMO POS "ya
    // abierto" -- eran dos productos distintos compartiendo este modulo, no
    // dos instancias del mismo POS. Default '' para Venta: no cambia su
    // clave de storage existente (compatibilidad con sesiones ya abiertas).
    function productKey() {
        return config().productKey ? '-' + config().productKey : '';
    }

    function stateKey() {
        return STORAGE_PREFIX + productKey() + '-' + (config().userId || 0) + '-' + (config().sucursalId || 0);
    }

    function postVentaRedirectKey() {
        return POSTVENTA_REDIRECT_PREFIX + '-' + (config().instanceId || '');
    }

    function pwaSessionKey() {
        return PWA_SESSION_PREFIX + productKey() + '-' + (config().userId || 0) + '-' + (config().sucursalId || 0);
    }

    function isDuplicate() {
        return config().role === 'duplicado';
    }

    function isStandalonePwa() {
        try {
            if (window.matchMedia && window.matchMedia('(display-mode: standalone)').matches) {
                return true;
            }
        } catch (e) { }

        try {
            if (window.navigator && window.navigator.standalone === true) {
                return true;
            }
        } catch (e) { }

        return false;
    }

    function isFresh(entry) {
        return !!(entry && entry.id && entry.ts && (now() - entry.ts) <= STALE_MS);
    }

    function readJsonStorage(storage, key, fallbackValue) {
        try {
            return JSON.parse(storage.getItem(key) || 'null') || fallbackValue;
        } catch (e) {
            storage.removeItem(key);
            return fallbackValue;
        }
    }

    function readState() {
        var state = readJsonStorage(localStorage, stateKey(), { original: null, duplicado: null });

        if (!isFresh(state.original)) state.original = null;
        if (!isFresh(state.duplicado)) state.duplicado = null;
        return state;
    }

    function writeState(state) {
        localStorage.setItem(stateKey(), JSON.stringify(state || { original: null, duplicado: null }));
    }

    function readPwaSession() {
        return readJsonStorage(sessionStorage, pwaSessionKey(), { originalId: '', duplicateId: '' });
    }

    function writePwaSession(sessionValue) {
        sessionStorage.setItem(pwaSessionKey(), JSON.stringify(sessionValue || { originalId: '', duplicateId: '' }));
    }

    function ownEntry() {
        return {
            id: config().instanceId,
            ts: now(),
            url: window.location.href,
            windowName: config().windowName || ''
        };
    }

    function isSameWindowEntry(entry) {
        return !!(entry &&
            entry.windowName &&
            config().windowName &&
            entry.windowName === config().windowName);
    }

    function buildRoleUrl(role, instanceId) {
        var url = new URL(config().duplicateBaseUrl || window.location.pathname, window.location.origin);
        url.searchParams.set('modoPos', role);
        url.searchParams.set('posInstanceId', instanceId);
        return url.toString();
    }

    function ensureWindowName() {
        if (!config().windowName) return;

        try {
            if (window.name !== config().windowName) {
                window.name = config().windowName;
            }
        } catch (e) { }
    }

    function ensureStandaloneSession() {
        if (!isStandalonePwa()) return readPwaSession();

        var sessionValue = readPwaSession();
        var changed = false;

        if (!sessionValue.originalId) {
            if (isDuplicate()) {
                var originalFromQuery = '';
                try {
                    originalFromQuery = new URL(window.location.href).searchParams.get('originalInstanceId') || '';
                } catch (e) { }

                sessionValue.originalId = originalFromQuery || config().instanceId;
            } else {
                sessionValue.originalId = config().instanceId;
            }
            changed = true;
        }

        if (isDuplicate() && !sessionValue.duplicateId) {
            sessionValue.duplicateId = config().instanceId;
            changed = true;
        }

        if (!isDuplicate() && sessionValue.originalId !== config().instanceId) {
            config().instanceId = sessionValue.originalId;
            changed = true;
        }

        if (isDuplicate() && sessionValue.duplicateId && sessionValue.duplicateId !== config().instanceId) {
            config().instanceId = sessionValue.duplicateId;
            changed = true;
        }

        if (changed) {
            writePwaSession(sessionValue);
        }

        return sessionValue;
    }

    function register() {
        var state;

        if (isStandalonePwa()) {
            var sessionValue = ensureStandaloneSession();
            state = {
                original: sessionValue.originalId ? {
                    id: sessionValue.originalId,
                    ts: now(),
                    url: buildRoleUrl('original', sessionValue.originalId),
                    windowName: config().originalWindowName || ''
                } : null,
                duplicado: sessionValue.duplicateId ? {
                    id: sessionValue.duplicateId,
                    ts: now(),
                    url: buildRoleUrl('duplicado', sessionValue.duplicateId),
                    windowName: config().duplicateWindowName || ''
                } : null
            };
        } else {
            state = readState();
            if (isDuplicate()) state.duplicado = ownEntry();
            else state.original = ownEntry();
        }

        writeState(state);
        updateUi();
    }

    function release() {
        if (isStandalonePwa()) return;

        var state = readState();
        if (isDuplicate()) {
            if (state.duplicado && state.duplicado.id === config().instanceId) {
                state.duplicado = null;
            }
        } else {
            if (state.original && state.original.id === config().instanceId) {
                state.original = null;
            }
        }
        writeState(state);
    }

    function hasDuplicateOpen(state) {
        if (isStandalonePwa()) {
            var sessionValue = ensureStandaloneSession();
            return !!sessionValue.duplicateId;
        }

        return isFresh(state.duplicado);
    }

    function updateUi() {
        var state = readState();
        var hasDuplicado = hasDuplicateOpen(state);
        var $badge = $('#posInstanceBadge');
        var $btnDuplicar = $('#btnDuplicarPOS');
        var $btnDuplicarText = $btnDuplicar.find('span');

        if ($badge.length) {
            if (isDuplicate()) {
                $badge
                    .removeClass('d-none badge-info')
                    .addClass('badge-warning')
                    .show()
                    .css('display', '')
                    .text('POS Duplicado');
            } else if (hasDuplicado) {
                $badge
                    .removeClass('d-none badge-warning')
                    .addClass('badge-info')
                    .show()
                    .css('display', '')
                    .text('POS Original');
            } else {
                $badge
                    .addClass('d-none')
                    .hide();
            }
        }

        if ($btnDuplicar.length) {
            if (isDuplicate()) {
                $btnDuplicar.prop('disabled', false).removeClass('disabled').attr('title', 'Ir al POS original');
                if ($btnDuplicarText.length) {
                    $btnDuplicarText.text('Ir al original');
                }
            } else if (hasDuplicado) {
                $btnDuplicar.prop('disabled', false).removeClass('disabled').attr('title', 'Ir al POS duplicado');
                if ($btnDuplicarText.length) {
                    $btnDuplicarText.text('Ir al duplicado');
                }
            } else {
                $btnDuplicar.prop('disabled', false).removeClass('disabled').attr('title', 'Abrir POS duplicado');
                if ($btnDuplicarText.length) {
                    $btnDuplicarText.text('Duplicar POS');
                }
            }
        }

        if (isDuplicate()) {
            document.title = 'CarniSys | POS Duplicado';
        } else if (hasDuplicado) {
            document.title = 'CarniSys | POS Original';
        } else {
            document.title = 'CarniSys | Punto de Venta';
        }
    }

    function ensureUrlParams() {
        try {
            var url = new URL(window.location.href);
            if (url.searchParams.get('modoPos') !== config().role) {
                url.searchParams.set('modoPos', config().role);
            }
            if (url.searchParams.get('posInstanceId') !== config().instanceId) {
                url.searchParams.set('posInstanceId', config().instanceId);
            }
            var finalUrl = url.pathname + url.search + url.hash;
            if (finalUrl !== (window.location.pathname + window.location.search + window.location.hash)) {
                window.history.replaceState({}, document.title, finalUrl);
            }
        } catch (e) { }
    }

    function notifyCommand(command) {
        try {
            localStorage.setItem(COMMAND_KEY, JSON.stringify(command));
            localStorage.removeItem(COMMAND_KEY);
        } catch (e) { }
    }

    function requestFocus(target, commandType) {
        if (!isFresh(target)) return false;

        notifyCommand({
            type: commandType,
            targetId: target.id,
            sourceId: config().instanceId,
            ts: now()
        });

        if (!target.windowName) return false;

        try {
            var targetWindow = window.open('', target.windowName);
            if (targetWindow) {
                targetWindow.focus();
                return true;
            }
        } catch (e) { }

        return false;
    }

    function persistDraftBeforeSwitch() {
        try {
            if (typeof window.desactivarAvisoSalidaPOS === 'function') {
                window.desactivarAvisoSalidaPOS();
            }

            if (window.POSDraft && typeof window.POSDraft.save === 'function') {
                window.POSDraft.save();
            }
        } catch (e) { }
    }

    function navigateSamePwaWindow(role, keepDuplicateSession) {
        var sessionValue = ensureStandaloneSession();
        var targetId = role === 'duplicado' ? sessionValue.duplicateId : sessionValue.originalId;

        if (!targetId) return false;

        if (role === 'original' && keepDuplicateSession === false) {
            sessionValue.duplicateId = '';
            writePwaSession(sessionValue);
        }

        persistDraftBeforeSwitch();
        suppressReleaseOnUnload = true;
        window.location.href = buildRoleUrl(role, targetId);
        return true;
    }

    function focusOriginal() {
        if (isStandalonePwa()) {
            return navigateSamePwaWindow('original', true);
        }

        var state = readState();
        return requestFocus(state.original, 'focus-original');
    }

    function focusDuplicado() {
        if (isStandalonePwa()) {
            return navigateSamePwaWindow('duplicado', true);
        }

        var state = readState();
        return requestFocus(state.duplicado, 'focus-duplicado');
    }

    function focusCodigoInput() {
        var input = document.getElementById('inputCodigo');
        if (!input || input.disabled || input.readOnly) return false;

        try {
            input.focus();
            if (typeof input.select === 'function') {
                input.select();
            }
            return document.activeElement === input;
        } catch (e) {
            return false;
        }
    }

    function scheduleFocusCodigo() {
        var attempts = 0;

        function tryFocus() {
            attempts += 1;
            if (focusCodigoInput()) return;
            if (attempts >= 8) return;
            window.setTimeout(tryFocus, 120);
        }

        window.setTimeout(tryFocus, 30);
    }

    function markPostVentaRedirect() {
        if (!isDuplicate()) return;

        try {
            sessionStorage.setItem(postVentaRedirectKey(), '1');
        } catch (e) { }
    }

    function clearPostVentaRedirect() {
        try {
            sessionStorage.removeItem(postVentaRedirectKey());
        } catch (e) { }
    }

    function shouldRedirectAfterPostVenta() {
        if (!isDuplicate()) return false;

        try {
            return sessionStorage.getItem(postVentaRedirectKey()) === '1';
        } catch (e) {
            return false;
        }
    }

    function redirectDuplicateAfterPostVenta() {
        var state = readState();
        var original = state.original;

        clearPostVentaRedirect();

        if (isStandalonePwa()) {
            var sessionValue = ensureStandaloneSession();
            if (sessionValue.originalId) {
                sessionValue.duplicateId = '';
                writePwaSession(sessionValue);
                return navigateSamePwaWindow('original', false);
            }

            window.location.replace(config().homeUrl || '/');
            return true;
        }

        if (isFresh(original)) {
            focusOriginal();
            release();

            try {
                window.close();
                return true;
            } catch (e) { }

            if (original.url) {
                try {
                    window.location.replace(original.url);
                    return true;
                } catch (e) { }
            }

            if (config().originalUrl) {
                window.location.replace(config().originalUrl);
                return true;
            }
        }

        release();
        window.location.replace(config().homeUrl || '/');
        return true;
    }

    function buildDuplicateUrl(duplicateId, originalId) {
        var url = new URL(config().duplicateBaseUrl || window.location.pathname, window.location.origin);
        url.searchParams.set('modoPos', 'duplicado');
        url.searchParams.set('posInstanceId', duplicateId);
        if (originalId) {
            url.searchParams.set('originalInstanceId', originalId);
        }
        return url.toString();
    }

    function abrirDuplicado() {
        var state = readState();
        var duplicateId = 'dup-' + Math.random().toString(36).slice(2, 10);
        var originalId = config().instanceId;
        var targetName = config().duplicateWindowName || '_blank';
        var duplicateUrl;

        if (isStandalonePwa()) {
            var sessionValue = ensureStandaloneSession();
            originalId = sessionValue.originalId || config().instanceId;
            duplicateId = sessionValue.duplicateId || duplicateId;
            sessionValue.originalId = originalId;
            sessionValue.duplicateId = duplicateId;
            writePwaSession(sessionValue);
        }

        duplicateUrl = buildDuplicateUrl(duplicateId, originalId);

        state.duplicado = {
            id: duplicateId,
            ts: now(),
            url: duplicateUrl,
            windowName: config().duplicateWindowName || ''
        };

        if (!state.original) {
            state.original = {
                id: originalId,
                ts: now(),
                url: buildRoleUrl('original', originalId),
                windowName: config().originalWindowName || ''
            };
        }

        writeState(state);
        updateUi();

        if (isStandalonePwa()) {
            persistDraftBeforeSwitch();
            suppressReleaseOnUnload = true;
            window.location.href = duplicateUrl;
            return;
        }

        var win = window.open(duplicateUrl, targetName);
        if (win) {
            try { win.focus(); } catch (e) { }
            return;
        }

        Swal.fire({
            icon: 'warning',
            title: 'POS Duplicado',
            text: 'El navegador bloqueo la apertura del POS duplicado. Permita ventanas emergentes para este sitio.'
        });
    }

    function confirmarYAbrirDuplicado() {
        Swal.fire({
            icon: 'question',
            title: 'Abrir POS duplicado',
            text: 'Se va a abrir un POS duplicado para atender otro cliente.',
            showCancelButton: true,
            confirmButtonText: 'OK',
            cancelButtonText: 'Cancelar',
            focusConfirm: true
        }).then(function (result) {
            if (!result.isConfirmed) return;
            abrirDuplicado();
        });
    }

    function alternarPorAtajo() {
        var state = readState();
        if (!hasDuplicateOpen(state)) {
            if (isDuplicate()) {
                focusOriginal();
                return;
            }

            confirmarYAbrirDuplicado();
            return;
        }

        if (isDuplicate()) {
            focusOriginal();
            return;
        }

        focusDuplicado();
    }

    function handleCommand(event) {
        if (event.key !== COMMAND_KEY || !event.newValue) return;

        try {
            var command = JSON.parse(event.newValue);
            if (!command || command.targetId !== config().instanceId) return;

            if (command.type === 'focus-original' || command.type === 'focus-duplicado') {
                try { window.focus(); } catch (e) { }
                if (command.type === 'focus-original') {
                    scheduleFocusCodigo();
                }
            }
        } catch (e) { }
    }

    function checkConflicts() {
        if (isStandalonePwa()) return true;

        var state = readState();
        var active = isDuplicate() ? state.duplicado : state.original;
        if (isFresh(active) && active.id !== config().instanceId && isSameWindowEntry(active)) {
            config().instanceId = active.id;
            ensureUrlParams();
            return true;
        }

        if (isFresh(active) && active.id !== config().instanceId) {
            blocked = true;
            updateUi();

            var text = isDuplicate()
                ? 'Ya existe un POS duplicado abierto.'
                : 'Ya hay un Punto de Venta abierto. Para atender otro cliente, use el boton Duplicar POS.';

            Swal.fire({
                icon: 'warning',
                title: 'POS ya abierto',
                text: text,
                allowOutsideClick: false
            }).then(function () {
                window.location.href = config().homeUrl || '/';
            });

            return false;
        }

        return true;
    }

    function startHeartbeat() {
        window.clearInterval(heartbeatTimer);
        heartbeatTimer = window.setInterval(function () {
            register();
        }, HEARTBEAT_MS);
    }

    function showFocusFallback(title, text) {
        Swal.fire({
            icon: 'info',
            title: title,
            text: text
        });
    }

    function initUiBindings() {
        $('#btnDuplicarPOS').off('.posMulti').on('click.posMulti', function (e) {
            e.preventDefault();
            if (blocked) return;

            var state = readState();

            if (isDuplicate()) {
                if (!focusOriginal()) {
                    showFocusFallback('POS original', 'No se pudo volver al POS original automaticamente.');
                }
                return;
            }

            if (hasDuplicateOpen(state)) {
                if (!focusDuplicado()) {
                    showFocusFallback('POS duplicado', 'No se pudo volver al POS duplicado automaticamente.');
                }
                return;
            }

            confirmarYAbrirDuplicado();
        });
    }

    function init() {
        ensureStandaloneSession();
        ensureWindowName();
        ensureUrlParams();
        if (shouldRedirectAfterPostVenta()) return redirectDuplicateAfterPostVenta();
        if (!checkConflicts()) return false;

        register();
        startHeartbeat();
        updateUi();
        initUiBindings();

        window.addEventListener('storage', function (event) {
            if (event.key === stateKey()) {
                updateUi();
                return;
            }

            handleCommand(event);
        });

        window.addEventListener('beforeunload', function () {
            if (suppressReleaseOnUnload) return;
            release();
        });

        if (!isDuplicate()) {
            scheduleFocusCodigo();
        }

        return true;
    }

    function closeAfterPostVenta() {
        if (!isDuplicate()) return true;

        markPostVentaRedirect();
        return true;
    }

    window.POSMultiInstance = {
        init: init,
        isBlocked: function () { return blocked; },
        isDuplicate: isDuplicate,
        updateUi: updateUi,
        closeAfterPostVenta: closeAfterPostVenta,
        focusOriginal: focusOriginal,
        focusDuplicado: focusDuplicado
    };
})(window, document, window.jQuery);
