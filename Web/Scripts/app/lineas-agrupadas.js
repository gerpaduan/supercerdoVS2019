(function () {
    function normalizarTexto(value) {
        return (value || '').toString().toLowerCase().trim();
    }

    function toggleAvisoFiltrosPendientes(show) {
        var alerta = document.getElementById('alertaFiltrosPendientes');
        if (!alerta) return;
        alerta.classList.toggle('d-none', !show);
    }

    function bindFiltrosPendientes(page) {
        var recarga = page.querySelectorAll('.js-filtro-recarga');
        Array.prototype.forEach.call(recarga, function (input) {
            input.addEventListener('change', function () {
                toggleAvisoFiltrosPendientes(true);
            });
        });

        var form = page.querySelector('.js-form-filtros-lineas');
        if (form) {
            form.addEventListener('submit', function () {
                toggleAvisoFiltrosPendientes(false);
            });
        }
    }

    function syncDetalles(page) {
        var expandir = false;
        var toggle = page.querySelector('.js-toggle-detalles-general');
        if (toggle) expandir = !!toggle.checked;

        Array.prototype.forEach.call(page.querySelectorAll('.linea-registro-extra'), function (item) {
            item.classList.toggle('d-none', !expandir);
        });
    }

    function getLiveFilterValue(page, key) {
        var input = page.querySelector('[data-live-filter="' + key + '"]');
        if (!input) return '';

        if (input.type === 'checkbox') {
            return input.checked ? '1' : '';
        }

        return normalizarTexto(input.value);
    }

    function getCheckedValues(page, key) {
        var items = page.querySelectorAll('[data-live-filter="' + key + '"]:checked');
        return Array.prototype.map.call(items, function (item) { return normalizarTexto(item.value); });
    }

    function cardMatchesGroupFilters(card, page) {
        var groupFilters = ['cliente', 'vendedor', 'proveedor', 'tipoCompra', 'tipoOperacion', 'estado', 'formaPago'];
        for (var i = 0; i < groupFilters.length; i++) {
            var key = groupFilters[i];
            if (key === 'formaPago') {
                var pagos = getCheckedValues(page, key);
                if (pagos.length > 0) {
                    var valuePago = normalizarTexto(card.getAttribute('data-' + key));
                    if (pagos.indexOf(valuePago) < 0) return false;
                }
                continue;
            }

            var value = getLiveFilterValue(page, key);
            if (!value) continue;

            var cardValue = normalizarTexto(card.getAttribute('data-' + key));
            if (cardValue.indexOf(value) < 0) return false;
        }

        return true;
    }

    function lineMatchesFilters(line, page) {
        var producto = getLiveFilterValue(page, 'producto');
        if (!producto) return true;

        var codigo = normalizarTexto(line.getAttribute('data-codigo'));
        var descripcion = normalizarTexto(line.getAttribute('data-producto'));
        return codigo.indexOf(producto) >= 0 || descripcion.indexOf(producto) >= 0;
    }

    function formatNumber(value, decimals) {
        return Number(value || 0).toLocaleString('es-AR', {
            minimumFractionDigits: decimals,
            maximumFractionDigits: decimals
        });
    }

    function formatCurrency(value) {
        return Number(value || 0).toLocaleString('es-AR', {
            style: 'currency',
            currency: 'ARS'
        });
    }

    function parseDecimal(value) {
        if (value === null || value === undefined) return 0;

        var text = value.toString().trim();
        if (!text) return 0;

        text = text.replace(/\s+/g, '');

        var commaIndex = text.lastIndexOf(',');
        var dotIndex = text.lastIndexOf('.');

        if (commaIndex >= 0 && dotIndex >= 0) {
            if (commaIndex > dotIndex) {
                text = text.replace(/\./g, '').replace(',', '.');
            } else {
                text = text.replace(/,/g, '');
            }
        } else if (commaIndex >= 0) {
            text = text.replace(',', '.');
        }

        var parsed = parseFloat(text);
        return isNaN(parsed) ? 0 : parsed;
    }

    function renderClientEmpty(page, visibleCards) {
        var empty = page.querySelector('.js-lineas-empty-client');
        var server = page.querySelector('.js-lineas-empty-server');

        if (server) server.classList.toggle('d-none', visibleCards > 0);
        if (empty) empty.classList.toggle('d-none', visibleCards > 0);
    }

    function recalcTotals(page) {
        var cards = page.querySelectorAll('.js-registro-card:not(.d-none)');
        var totalRegistros = 0;
        var totalLineas = 0;
        var totalKg = 0;
        var totalImporte = 0;
        var totalUnidades = 0;

        Array.prototype.forEach.call(cards, function (card) {
            totalRegistros++;
            var rows = card.querySelectorAll('.js-linea-row:not(.d-none)');
            Array.prototype.forEach.call(rows, function (row) {
                totalLineas++;
                totalKg += parseDecimal(row.getAttribute('data-kg'));
                totalImporte += parseDecimal(row.getAttribute('data-total'));
                totalUnidades += parseDecimal(row.getAttribute('data-unidades'));
            });
        });

        var map = {
            totalRegistros: totalRegistros,
            totalLineas: totalLineas,
            totalKg: formatNumber(totalKg, 3),
            totalImporte: formatCurrency(totalImporte),
            totalUnidades: formatNumber(totalUnidades, 0)
        };

        Object.keys(map).forEach(function (key) {
            var el = document.getElementById(key);
            if (el) el.textContent = map[key];
        });
    }

    function filtrar(page) {
        var cards = page.querySelectorAll('.js-registro-card');
        var visibles = 0;

        Array.prototype.forEach.call(cards, function (card) {
            var groupOk = cardMatchesGroupFilters(card, page);
            var lineasVisibles = 0;
            var rows = card.querySelectorAll('.js-linea-row');

            Array.prototype.forEach.call(rows, function (row) {
                var match = groupOk && lineMatchesFilters(row, page);
                row.classList.toggle('d-none', !match);
                if (match) lineasVisibles++;
            });

            var showCard = groupOk && lineasVisibles > 0;
            card.classList.toggle('d-none', !showCard);
            if (showCard) visibles++;
        });

        renderClientEmpty(page, visibles);
        recalcTotals(page);
    }

    document.addEventListener('DOMContentLoaded', function () {
        var page = document.querySelector('[data-lineas-agrupadas-page]');
        if (!page) return;

        bindFiltrosPendientes(page);
        syncDetalles(page);

        Array.prototype.forEach.call(page.querySelectorAll('.js-filtro-vivo'), function (input) {
            input.addEventListener(input.type === 'checkbox' ? 'change' : 'input', function () {
                filtrar(page);
            });
        });

        var toggle = page.querySelector('.js-toggle-detalles-general');
        if (toggle) {
            toggle.addEventListener('change', function () {
                syncDetalles(page);
            });
        }

        filtrar(page);
    });
})();
