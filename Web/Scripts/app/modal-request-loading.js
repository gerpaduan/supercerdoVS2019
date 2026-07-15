(function (window, $) {
    'use strict';

    if (!window || !$) return;

    var active = false;
    var requestSeq = 0;
    var trackedRequests = {};
    var fetchWrapped = false;
    var AUTO_DELAY_MS = 1000;
    var stylesInjected = false;
    var navigationTimer = 0;

    function hasOwn(obj, key) {
        return Object.prototype.hasOwnProperty.call(obj, key);
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

        navigationTimer = window.setTimeout(function () {
            navigationTimer = 0;
            show(message || 'Cargando solicitud...');
        }, AUTO_DELAY_MS);
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

    window.ModalRequestLoading = window.ModalRequestLoading || {
        show: show,
        hide: hide,
        cancel: cancelActiveRequests,
        trackNavigation: trackNavigation
    };
})(window, window.jQuery);
