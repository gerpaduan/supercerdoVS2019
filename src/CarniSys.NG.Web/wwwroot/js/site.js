(function () {
    function debounce(fn, wait) {
        let timer = null;
        return function () {
            const args = arguments;
            const context = this;
            window.clearTimeout(timer);
            timer = window.setTimeout(function () {
                fn.apply(context, args);
            }, wait);
        };
    }

    function escapeHtml(value) {
        return String(value ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function setStatus(statusBox, type, message) {
        if (!statusBox) {
            return;
        }

        statusBox.className = "alert js-lookup-status";
        if (!message) {
            statusBox.classList.add("d-none");
            statusBox.textContent = "";
            return;
        }

        statusBox.classList.remove("d-none");
        statusBox.classList.add(type === "success" ? "alert-success" : type === "warning" ? "alert-warning" : type === "danger" ? "alert-danger" : "alert-info");
        statusBox.textContent = message;
    }

    function renderPersonResults(modal, items, append) {
        const tbody = modal.querySelector(".js-lookup-results");
        const showTaxId = modal.dataset.showTaxId === "true";
        const showIdentification = modal.dataset.showIdentification === "true";
        const visibleColumnCount = 1 + (showIdentification ? 1 : 0) + (showTaxId ? 1 : 0);
        if (!tbody) {
            return;
        }

        if ((!Array.isArray(items) || items.length === 0) && !append) {
            tbody.innerHTML = `<tr><td colspan="${visibleColumnCount}" class="text-center text-muted py-4">No se encontraron personas.</td></tr>`;
            return;
        }

        const html = items.map(function (item) {
            const identificationCell = showIdentification ? `<td>${escapeHtml(item.identification || "")}</td>` : "";
            const taxIdCell = showTaxId ? `<td>${escapeHtml(item.taxId || "")}</td>` : "";
            return `
                <tr class="lookup-result-row" data-lookup-item='${escapeHtml(JSON.stringify(item))}'>
                    ${identificationCell}
                    <td>${escapeHtml(item.businessName || "")}</td>
                    ${taxIdCell}
                </tr>`;
        }).join("");

        if (append) {
            if (tbody.querySelector("[data-empty='1']")) {
                tbody.innerHTML = "";
            }
            tbody.insertAdjacentHTML("beforeend", html);
            return;
        }

        tbody.innerHTML = html;
    }

    function renderProductResults(modal, items, append) {
        const tbody = modal.querySelector(".js-lookup-results");
        const showType = modal.dataset.showType === "true";
        const showPrice = modal.dataset.showPrice === "true";
        if (!tbody) {
            return;
        }

        if ((!Array.isArray(items) || items.length === 0) && !append) {
            tbody.innerHTML = '<tr><td colspan="4" class="text-center text-muted py-4">No se encontraron productos.</td></tr>';
            return;
        }

        const html = items.map(function (item) {
            const typeCell = showType ? `<td>${escapeHtml(item.type || "")}</td>` : '<td class="d-none"></td>';
            const priceCell = showPrice ? `<td class="text-end">${escapeHtml(item.pricePerKilogramText || "0.00")}</td>` : '<td class="d-none"></td>';
            return `
                <tr class="lookup-result-row" data-lookup-item='${escapeHtml(JSON.stringify(item))}'>
                    <td>${escapeHtml(item.code || "")}</td>
                    <td>${escapeHtml(item.description || "")}</td>
                    ${typeCell}
                    ${priceCell}
                </tr>`;
        }).join("");

        if (append) {
            if (tbody.querySelector("[data-empty='1']")) {
                tbody.innerHTML = "";
            }
            tbody.insertAdjacentHTML("beforeend", html);
            return;
        }

        tbody.innerHTML = html;
    }

    async function executeLookup(modal, searchText, state, append) {
        const searchUrl = modal.dataset.searchUrl;
        const kind = modal.dataset.lookupKind;
        const statusBox = modal.querySelector(".js-lookup-status");
        const normalizedSearchText = String(searchText || "").trim();
        const skip = append ? state.loadedCount : 0;
        const take = state.pageSize;

        if (!searchUrl) {
            setStatus(statusBox, "danger", "No hay URL configurada para esta busqueda.");
            return;
        }

        if (state.abortController) {
            state.abortController.abort();
        }

        state.abortController = new AbortController();
        state.requestSequence += 1;
        const requestId = state.requestSequence;
        state.searchText = normalizedSearchText;
        state.isLoading = true;

        if (!append) {
            state.loadedCount = 0;
            state.hasMore = false;
            setStatus(statusBox, "info", "Buscando...");
        } else {
            setStatus(statusBox, "info", "Cargando mas resultados...");
        }

        try {
            const response = await fetch(`${searchUrl}?searchText=${encodeURIComponent(normalizedSearchText)}&skip=${encodeURIComponent(skip)}&take=${encodeURIComponent(take)}`, {
                headers: { "X-Requested-With": "XMLHttpRequest" },
                signal: state.abortController.signal
            });

            if (!response.ok) {
                throw new Error("No se pudo completar la busqueda.");
            }

            const payload = await response.json();
            if (requestId !== state.requestSequence) {
                return;
            }

            const items = Array.isArray(payload.items) ? payload.items : [];
            state.hasMore = !!payload.hasMore;
            state.loadedCount = skip + items.length;

            if (kind === "person") {
                renderPersonResults(modal, items, append);
            } else if (kind === "product") {
                renderProductResults(modal, items, append);
            }

            if (!append) {
                setStatus(statusBox, items.length > 0 ? "success" : "warning", items.length > 0 ? `${items.length} resultado(s) cargados. ${state.hasMore ? "Desplazate para ver mas." : ""}` : "No se encontraron resultados.");
            } else {
                setStatus(statusBox, state.hasMore ? "info" : "success", items.length > 0 ? (state.hasMore ? `${state.loadedCount} resultado(s) cargados.` : `Se cargaron ${state.loadedCount} resultado(s).`) : "No hay mas resultados.");
            }
        } catch (error) {
            if (error instanceof DOMException && error.name === "AbortError") {
                return;
            }

            if (requestId !== state.requestSequence) {
                return;
            }

            setStatus(statusBox, "danger", error instanceof Error ? error.message : "No se pudo completar la busqueda.");
        } finally {
            if (requestId === state.requestSequence) {
                state.isLoading = false;
            }
        }
    }

    function initLookupModal(modal) {
        const searchInput = modal.querySelector(".js-lookup-search");
        const resultsContainer = modal.querySelector(".js-lookup-results");
        const dialogBody = modal.querySelector(".modal-body");
        const state = {
            abortController: null,
            requestSequence: 0,
            pageSize: 50,
            loadedCount: 0,
            hasMore: false,
            isLoading: false,
            searchText: ""
        };
        const runLookup = debounce(function () {
            executeLookup(modal, searchInput ? searchInput.value : "", state, false);
        }, 1000);

        if (searchInput) {
            searchInput.addEventListener("input", runLookup);
            searchInput.addEventListener("keydown", function (event) {
                if (event.key !== "Enter") {
                    return;
                }

                event.preventDefault();

                const firstRow = resultsContainer ? resultsContainer.querySelector(".lookup-result-row") : null;
                if (firstRow instanceof HTMLElement) {
                    firstRow.click();
                    return;
                }

                executeLookup(modal, searchInput.value, state, false);
            });
        }

        modal.addEventListener("shown.bs.modal", function () {
            if (searchInput) {
                searchInput.focus();
                searchInput.select();
                if ((searchInput.value || "").trim()) {
                    executeLookup(modal, searchInput.value, state, false);
                }
            }
        });

        if (dialogBody) {
            dialogBody.addEventListener("scroll", debounce(function () {
                if (state.isLoading || !state.hasMore) {
                    return;
                }

                const remaining = dialogBody.scrollHeight - dialogBody.scrollTop - dialogBody.clientHeight;
                if (remaining > 120) {
                    return;
                }

                executeLookup(modal, state.searchText, state, true);
            }, 150));
        }

        modal.addEventListener("hidden.bs.modal", function () {
            if (state.abortController) {
                state.abortController.abort();
                state.abortController = null;
            }
            state.isLoading = false;
            state.hasMore = false;
            state.loadedCount = 0;
        });

        modal.addEventListener("click", function (event) {
            const target = event.target;
            if (!(target instanceof HTMLElement)) {
                return;
            }

            const row = target.closest(".lookup-result-row");
            if (!row) {
                return;
            }

            const payloadText = row.getAttribute("data-lookup-item") || "";
            if (!payloadText) {
                return;
            }

            let payload = null;
            try {
                payload = JSON.parse(payloadText);
            } catch {
                payload = null;
            }

            if (!payload) {
                return;
            }

            modal.dispatchEvent(new CustomEvent("lookup:selected", {
                detail: payload
            }));

            const modalInstance = bootstrap.Modal.getInstance(modal);
            if (modalInstance) {
                modalInstance.hide();
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".app-lookup-modal").forEach(initLookupModal);
    });
})();
