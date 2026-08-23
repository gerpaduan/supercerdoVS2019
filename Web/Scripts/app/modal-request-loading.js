(function (window, $) {
    'use strict';

    if (!window || !$) return;

    var active = false;
    var requestSeq = 0;
    var trackedRequests = {};
    var fetchWrapped = false;
    var AUTO_DELAY_MS = 1000;
    var NAV_DELAY_MS = 2000; // navegacion completa de pagina (click en un link) -- mas laxo que AJAX
                              // porque incluye el armado de la vista Razor en el servidor, no solo la query
    var stylesInjected = false;
    var navigationTimer = 0;
    var navigationWatchdogTimer = 0;
    var NAV_WATCHDOG_MS = 12000; // ver comentario en trackNavigation
    var csrfTokenCache = null;

    function hasOwn(obj, key) {
        return Object.prototype.hasOwnProperty.call(obj, key);
    }

    function isUnsafeMethod(method) {
        var normalized = String(method || 'GET').toUpperCase();
        return normalized === 'POST' || normalized === 'PUT' || normalized === 'PATCH' || normalized === 'DELETE';
    }

    function readRequestVerificationToken() {
        if (csrfTokenCache) return csrfTokenCache;

        var selector = 'input[name="__RequestVerificationToken"]';
        var globalToken = document.getElementById('globalAntiForgeryToken');
        var input = globalToken ? globalToken.querySelector(selector) : null;
        if (!input) {
            input = document.querySelector(selector);
        }

        csrfTokenCache = input && input.value ? input.value : '';
        return csrfTokenCache;
    }

    function ensureRequestVerificationHeader(jqXHR, settings) {
        if (!jqXHR || !settings || !isUnsafeMethod(settings.type || settings.method)) return;
        if (!isSameOrigin(settings.url || '')) return;
        if (settings.disableRequestVerification === true) return;
        if (settings.headers && settings.headers.RequestVerificationToken) return;

        var token = readRequestVerificationToken();
        if (!token) return;

        jqXHR.setRequestHeader('RequestVerificationToken', token);
    }

    function ensureStyles() {
        if (stylesInjected || !document || !document.head) return;

        var style = document.createElement('style');
        style.type = 'text/css';
        style.setAttribute('data-modal-request-loading-style', 'true');
        style.textContent =
            '.modal-request-loading-popup{font-size:.92rem;}' +
            '.modal-request-loading-title{font-size:1.05rem !important;}' +
            '.modal-request-loading-html{font-size:.82rem !important; line-height:1.35;}' +
            '.modal-request-loading-cancel{font-size:.82rem !important; padding:.42rem .8rem !important;}';
        document.head.appendChild(style);
        stylesInjected = true;
    }

    function isLayoutBasePage() {
        return !!(document && document.body && $(document.body).hasClass('app-shell'));
    }

    function isCancelableRequest(meta) {
        return !!(meta && typeof meta.abort === 'function');
    }

    function countTrackedRequests() {
        var count = 0;
        for (var key in trackedRequests) {
            if (hasOwn(trackedRequests, key)) {
                count++;
            }
        }
        return count;
    }

    function hasVisibleCancelableRequest() {
        for (var key in trackedRequests) {
            if (hasOwn(trackedRequests, key)) {
                var meta = trackedRequests[key];
                if (meta && meta.visible && isCancelableRequest(meta)) {
                    return true;
                }
            }
        }
        return false;
    }

    function getCurrentMessage() {
        for (var key in trackedRequests) {
            if (hasOwn(trackedRequests, key)) {
                var meta = trackedRequests[key];
                if (meta && meta.visible && meta.message) {
                    return meta.message;
                }
            }
        }
        return 'Cargando solicitud...';
    }

    function bindCancelButton() {
        if (!window.Swal || typeof window.Swal.getCancelButton !== 'function') return;

        var btn = window.Swal.getCancelButton();
        if (!btn) return;

        btn.onclick = function () {
            cancelActiveRequests();
        };
    }

    function render() {
        if (countTrackedRequests() === 0) {
            hide();
            return;
        }

        var message = getCurrentMessage();
        var hasCancelable = hasVisibleCancelableRequest();

        if (window.POSModalLoading && typeof window.POSModalLoading.show === 'function') {
            active = true;
            window.POSModalLoading.show(message);
            return;
        }

        if (!window.Swal || typeof window.Swal.fire !== 'function') return;

        ensureStyles();
        active = true;
        window.Swal.fire({
            title: message,
            html: hasCancelable
                ? 'Espere un momento.<br><small>Si la demora es excesiva, puede intentar cancelar la espera.</small>'
                : 'Espere un momento.',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
            showConfirmButton: false,
            showCancelButton: hasCancelable,
            cancelButtonText: 'Cancelar espera',
            customClass: {
                popup: 'modal-request-loading-popup',
                title: 'modal-request-loading-title',
                htmlContainer: 'modal-request-loading-html',
                cancelButton: 'modal-request-loading-cancel'
            },
            didOpen: function () {
                if (window.Swal && typeof window.Swal.showLoading === 'function') {
                    window.Swal.showLoading();
                }
                bindCancelButton();
            }
        });
    }

    function show(message) {
        var text = message || 'Cargando solicitud...';

        if (window.POSModalLoading && typeof window.POSModalLoading.show === 'function') {
            active = true;
            window.POSModalLoading.show(text);
            return;
        }

        if (!window.Swal || typeof window.Swal.fire !== 'function') return;

        ensureStyles();
        active = true;
        window.Swal.fire({
            title: text,
            text: 'Espere un momento.',
            allowOutsideClick: false,
            allowEscapeKey: false,
            allowEnterKey: false,
            showConfirmButton: false,
            customClass: {
                popup: 'modal-request-loading-popup',
                title: 'modal-request-loading-title',
                htmlContainer: 'modal-request-loading-html',
                cancelButton: 'modal-request-loading-cancel'
            },
            didOpen: function () {
                if (window.Swal && typeof window.Swal.showLoading === 'function') {
                    window.Swal.showLoading();
                }
            }
        });
    }

    function hide() {
        if (navigationTimer) {
            window.clearTimeout(navigationTimer);
            navigationTimer = 0;
        }
        if (navigationWatchdogTimer) {
            window.clearTimeout(navigationWatchdogTimer);
            navigationWatchdogTimer = 0;
        }

        if (!active) return;
        active = false;

        if (window.POSModalLoading && typeof window.POSModalLoading.hide === 'function') {
            window.POSModalLoading.hide();
            return;
        }

        if (window.Swal && typeof window.Swal.close === 'function') {
            window.Swal.close();
        }
    }

    function beginTrackedRequest(meta) {
        var id = 'req_' + (++requestSeq);
        trackedRequests[id] = $.extend({
            message: 'Procesando solicitud...',
            visible: false,
            aborted: false,
            timerId: 0,
            abort: null
        }, meta || {});

        trackedRequests[id].timerId = window.setTimeout(function () {
            if (!hasOwn(trackedRequests, id)) return;
            trackedRequests[id].visible = true;
            render();
        }, AUTO_DELAY_MS);

        return id;
    }

    function trackNavigation(message) {
        if (navigationTimer) {
            window.clearTimeout(navigationTimer);
        }
        if (navigationWatchdogTimer) {
            window.clearTimeout(navigationWatchdogTimer);
            navigationWatchdogTimer = 0;
        }

        navigationTimer = window.setTimeout(function () {
            navigationTimer = 0;

            // Chequeo en el momento del DISPARO, no solo al armar el timer (wireGlobalNavigationHandler
            // ya chequea __protegerSalida al hacer click, pero eso no alcanza): el flag puede cambiar
            // durante los 2s de espera -- si el usuario cancela el dialogo nativo "¿Desea abandonar el
            // sitio?" o el "Salir sin guardar" custom, sigue en true y esta funcion NO se entera de que
            // la navegacion nunca paso. Sin este chequeo, el spinner aparecia igual y quedaba trabado
            // hasta el watchdog de mas abajo. Ademas protege por igual a cualquier caller de
            // trackNavigation() que no chequee el flag por su cuenta (ej. _Tabs.cshtml en Elaborados),
            // sin tener que auditar cada uno de los ~20 call sites existentes en el resto de la app.
            if (window.__protegerSalida) {
                return;
            }

            show(message || 'Cargando solicitud...');

            // Nadie llama a endTrackedRequest para una navegacion (no es una request
            // trackeada como el AJAX/fetch): el modal se supone que desaparece solo porque
            // la pagina vieja se destruye al llegar la nueva. Pero si la navegacion en
            // realidad se cancela -- tipico caso: el usuario le dice "no" al dialogo NATIVO
            // "¿Desea abandonar el sitio?" que dispara el propio navegador cuando hay un
            // beforeunload activo -- seguimos parados en el mismo documento y este modal
            // queda trabado para siempre (allowOutsideClick/allowEscapeKey estan en false).
            // Nos autorecuperamos: si a los 12s segimos aca (nunca se disparo un unload real),
            // lo cerramos solos en vez de obligar a recargar la pagina.
            navigationWatchdogTimer = window.setTimeout(function () {
                navigationWatchdogTimer = 0;
                hide();
            }, NAV_WATCHDOG_MS);
        }, NAV_DELAY_MS);
    }

    // Dispara el "Cargando..." (via trackNavigation) para cualquier click en un link que vaya a
    // navegar la pagina entera -- antes solo lo llamaban a mano un puñado de vistas para acciones
    // puntuales (atajos de teclado, etc.), nunca el menu lateral. Se excluyen los casos donde en
    // realidad NO hay navegacion de la pagina actual (target=_blank, ctrl/cmd/shift/click del medio,
    // anclas, links no-http) y el caso donde el guard de "salir sin guardar" ya se encarga (si el
    // usuario cancela ahi, no debe quedar un spinner sin cerrar).
    function wireGlobalNavigationHandler() {
        $(document).on('click.modalRequestLoadingNav', 'a[href]', function (e) {
            if (window.__protegerSalida) return;
            if (e.which !== 1 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;

            var $link = $(this);
            if ($link.attr('target') === '_blank') return;
            if ($link.is('[data-no-loading]')) return;

            var href = $link.attr('href') || '';
            if (!href || href.charAt(0) === '#') return;
            if (/^(javascript:|mailto:|tel:)/i.test(href)) return;

            trackNavigation();
        });
    }

    function endTrackedRequest(id) {
        if (!id || !hasOwn(trackedRequests, id)) return;

        var meta = trackedRequests[id];
        if (meta && meta.timerId) {
            window.clearTimeout(meta.timerId);
        }

        delete trackedRequests[id];
        render();
    }

    function cancelActiveRequests() {
        var abortedAny = false;

        for (var key in trackedRequests) {
            if (!hasOwn(trackedRequests, key)) continue;

            var meta = trackedRequests[key];
            if (!meta || meta.aborted || typeof meta.abort !== 'function') continue;

            meta.aborted = true;
            abortedAny = true;

            try {
                meta.abort();
            } catch (e) { }
        }

        if (!abortedAny) {
            hide();
        }
    }

    function shouldAutoTrackAjax(settings) {
        if (!isLayoutBasePage()) return false;
        if (!settings) return false;
        if (settings.global === false) return false;
        if (settings.silentLoading === true) return false;
        if (settings.disableRequestLoading === true) return false;
        return true;
    }

    function normalizeFetchInput(input) {
        if (typeof input === 'string') return input;
        if (input && typeof input.url === 'string') return input.url;
        return '';
    }

    function isSameOrigin(url) {
        if (!url) return true;

        try {
            var parsed = new URL(url, window.location.href);
            return parsed.origin === window.location.origin;
        } catch (e) {
            return true;
        }
    }

    function shouldAutoTrackFetch(input, init) {
        if (!isLayoutBasePage()) return false;
        if (init && (init.silentLoading === true || init.disableRequestLoading === true)) return false;
        return isSameOrigin(normalizeFetchInput(input));
    }

    function wireGlobalAjaxHandlers() {
        $(document)
            .off('.modalRequestLoadingAuto')
            .on('ajaxSend.modalRequestLoadingAuto', function (event, jqXHR, settings) {
                ensureRequestVerificationHeader(jqXHR, settings);

                if (!shouldAutoTrackAjax(settings)) return;

                var requestId = beginTrackedRequest({
                    message: (settings && settings.loadingMessage) || 'Procesando solicitud...',
                    abort: jqXHR && typeof jqXHR.abort === 'function'
                        ? function () { jqXHR.abort('modal-request-loading-cancelled'); }
                        : null
                });

                try {
                    jqXHR.__modalRequestLoadingId = requestId;
                } catch (e) { }
            })
            .on('ajaxComplete.modalRequestLoadingAuto ajaxError.modalRequestLoadingAuto', function (event, jqXHR) {
                if (!jqXHR || !jqXHR.__modalRequestLoadingId) return;
                endTrackedRequest(jqXHR.__modalRequestLoadingId);
                try {
                    delete jqXHR.__modalRequestLoadingId;
                } catch (e) {
                    jqXHR.__modalRequestLoadingId = null;
                }
            });
    }

    function wireFetchWrapper() {
        if (fetchWrapped || typeof window.fetch !== 'function') return;

        var originalFetch = window.fetch;
        fetchWrapped = true;

        window.fetch = function (input, init) {
            var shouldTrack = shouldAutoTrackFetch(input, init);
            var requestId = null;
            var fetchInit = init || {};
            var controller = null;
            var abortFn = null;
            var method = (fetchInit.method || 'GET').toUpperCase();

            if (isUnsafeMethod(method) && isSameOrigin(normalizeFetchInput(input)) && fetchInit.disableRequestVerification !== true) {
                var token = readRequestVerificationToken();
                if (token) {
                    var headers = new window.Headers(fetchInit.headers || {});
                    if (!headers.has('RequestVerificationToken')) {
                        headers.set('RequestVerificationToken', token);
                    }
                    fetchInit = $.extend({}, fetchInit, { headers: headers });
                }
            }

            if (shouldTrack) {
                if (!fetchInit.signal && typeof window.AbortController === 'function') {
                    controller = new window.AbortController();
                    fetchInit = $.extend({}, fetchInit, { signal: controller.signal });
                    abortFn = function () { controller.abort(); };
                }

                requestId = beginTrackedRequest({
                    message: (fetchInit && fetchInit.loadingMessage) || 'Procesando solicitud...',
                    abort: abortFn
                });
            }

            var promise;

            try {
                promise = originalFetch.call(this, input, fetchInit);
            } catch (error) {
                endTrackedRequest(requestId);
                throw error;
            }

            return promise.then(function (response) {
                endTrackedRequest(requestId);
                return response;
            }, function (error) {
                endTrackedRequest(requestId);
                throw error;
            });
        };
    }

    $(document)
        .off('.modalRequestLoading')
        .on('shown.bs.modal.modalRequestLoading hidden.bs.modal.modalRequestLoading', '.modal', function () {
            if (countTrackedRequests() > 0) {
                render();
                return;
            }

            hide();
        });

    wireGlobalAjaxHandlers();
    wireFetchWrapper();
    if (isLayoutBasePage()) {
        wireGlobalNavigationHandler();
    }

    window.ModalRequestLoading = window.ModalRequestLoading || {
        show: show,
        hide: hide,
        cancel: cancelActiveRequests,
        trackNavigation: trackNavigation
    };
})(window, window.jQuery);
