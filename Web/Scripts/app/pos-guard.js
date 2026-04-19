(function (window, $) {
    if (!window || !$) return;

    // Guard central del POS:
    // - evita dobles clicks en acciones criticas
    // - evita reabrir modales mientras estan cargando, abriendose o cerrandose
    // Lo dejamos chico y reutilizable para que cualquier modulo del POS pueda usarlo.
    window.POSGuard = window.POSGuard || (function () {
        const busyActions = Object.create(null);
        const modalStates = Object.create(null);

        function actionKey(key) {
            return String(key || "").trim().toLowerCase();
        }

        function markActionBusy(key, busy) {
            const normalized = actionKey(key);
            if (!normalized) return;

            if (busy) busyActions[normalized] = true;
            else delete busyActions[normalized];
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

        function modalKey(key) {
            return actionKey(key);
        }

        function isModalBusy(key) {
            const normalized = modalKey(key);
            const state = modalStates[normalized];
            return state === "opening" || state === "closing" || isActionBusy("modal-load:" + normalized);
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
                })
                .on("shown.bs.modal" + ns, selector, function () {
                    modalStates[normalized] = "open";
                })
                .on("hide.bs.modal" + ns, selector, function () {
                    modalStates[normalized] = "closing";
                })
                .on("hidden.bs.modal" + ns, selector, function () {
                    delete modalStates[normalized];
                    endAction("modal-load:" + normalized);
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

        return {
            bindModal: bindModal,
            startAction: startAction,
            endAction: endAction,
            isActionBusy: isActionBusy,
            startModalLoad: startModalLoad,
            endModalLoad: endModalLoad,
            isModalBusy: isModalBusy,
            requestModalOpen: requestModalOpen
        };
    })();
})(window, window.jQuery);
