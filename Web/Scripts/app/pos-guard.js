(function (window, $) {
    if (!window || !$) return;

    // Guard central del POS:
    // - evita dobles clicks en acciones criticas
    // - evita reabrir modales mientras estan cargando, abriendose o cerrandose
    // Lo dejamos chico y reutilizable para que cualquier modulo del POS pueda usarlo.
    window.POSGuard = window.POSGuard || (function () {
        const busyActions = Object.create(null);
        const busyActionSince = Object.create(null);
        const modalStates = Object.create(null);
        const modalStateSince = Object.create(null);
        const MODAL_TRANSITION_STALE_MS = 3000;
        const MODAL_LOAD_STALE_MS = 15000;

        function actionKey(key) {
            return String(key || "").trim().toLowerCase();
        }

        function markActionBusy(key, busy) {
            const normalized = actionKey(key);
            if (!normalized) return;

            if (busy) {
                busyActions[normalized] = true;
                busyActionSince[normalized] = Date.now();
                return;
            }

            delete busyActions[normalized];
            delete busyActionSince[normalized];
        }

        function startAction(key) {
            const normalized = actionKey(key);
            if (!normalized) return true;
            if (busyActions[normalized]) return false;

            busyActions[normalized] = true;
            return true;
        }

        function endAction(key) {
            markActionBusy(key, false);
        }

        function isActionBusy(key) {
            return !!busyActions[actionKey(key)];
        }

        function isActionStale(key, maxAgeMs) {
            const normalized = actionKey(key);
            if (!normalized || !busyActions[normalized]) return false;

            const startedAt = busyActionSince[normalized] || 0;
            return !!startedAt && (Date.now() - startedAt) > (maxAgeMs || 0);
        }

        function modalKey(key) {
            return actionKey(key);
        }

        function clearModalBusyState(key) {
            const normalized = modalKey(key);
            if (!normalized) return;

            delete modalStates[normalized];
            delete modalStateSince[normalized];
            endAction("modal-load:" + normalized);
        }

        function isModalBusy(key) {
            const normalized = modalKey(key);
            const state = modalStates[normalized];
            const startedAt = modalStateSince[normalized] || 0;

            if ((state === "opening" || state === "closing")
                && startedAt
                && (Date.now() - startedAt) > MODAL_TRANSITION_STALE_MS) {
                clearModalBusyState(normalized);
            }

            if (isActionStale("modal-load:" + normalized, MODAL_LOAD_STALE_MS)) {
                endAction("modal-load:" + normalized);
            }

            return modalStates[normalized] === "opening"
                || modalStates[normalized] === "closing"
                || isActionBusy("modal-load:" + normalized);
        }

        function bindModal(selector, key) {
            const normalized = modalKey(key);
            if (!normalized || !selector) return;

            const ns = ".posGuard." + normalized;

            $(document)
                .off("show.bs.modal" + ns, selector)
                .off("shown.bs.modal" + ns, selector)
                .off("hide.bs.modal" + ns, selector)
                .off("hidden.bs.modal" + ns, selector)
                .on("show.bs.modal" + ns, selector, function () {
                    modalStates[normalized] = "opening";
                    modalStateSince[normalized] = Date.now();
                })
                .on("shown.bs.modal" + ns, selector, function () {
                    modalStates[normalized] = "open";
                    modalStateSince[normalized] = Date.now();
                })
                .on("hide.bs.modal" + ns, selector, function () {
                    modalStates[normalized] = "closing";
                    modalStateSince[normalized] = Date.now();
                })
                .on("hidden.bs.modal" + ns, selector, function () {
                    clearModalBusyState(normalized);
                });
        }

        function startModalLoad(key) {
            return startAction("modal-load:" + modalKey(key));
        }

        function endModalLoad(key) {
            endAction("modal-load:" + modalKey(key));
        }

        function requestModalOpen(key, openFn) {
            const normalized = modalKey(key);
            if (!normalized) return false;
            if (isModalBusy(normalized)) return false;

            openFn();
            return true;
        }

        function getTopVisibleModal() {
            return $(".modal.show:visible").last();
        }

        function isModalOnTop(selector) {
            if (!selector) return false;

            const $topModal = getTopVisibleModal();
            return $topModal.length > 0 && $topModal.is(selector);
        }

        return {
            bindModal: bindModal,
            startAction: startAction,
            endAction: endAction,
            isActionBusy: isActionBusy,
            startModalLoad: startModalLoad,
            endModalLoad: endModalLoad,
            isModalBusy: isModalBusy,
            requestModalOpen: requestModalOpen,
            getTopVisibleModal: getTopVisibleModal,
            isModalOnTop: isModalOnTop
        };
    })();
})(window, window.jQuery);
