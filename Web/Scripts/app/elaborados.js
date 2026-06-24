(function () {
    function normalizarTexto(value) {
        return (value || '').toString().toLowerCase().trim();
    }

    function toggleAvisoFiltrosPendientes(show) {
        var $alerta = $('#alertaFiltrosPendientes');
        if (!$alerta.length) return;
        $alerta.toggleClass('d-none', !show);
    }

    function bindFiltrosPendientes() {
        $('.js-filtro-recarga').on('change', function () {
            toggleAvisoFiltrosPendientes(true);
        });

        $('form[id^="formFiltros"]').on('submit', function () {
            toggleAvisoFiltrosPendientes(false);
        });
    }

    function renderEmptyRow($tbody, cssClass, colspan, text) {
        var $existente = $tbody.find('.' + cssClass);
        if ($existente.length) {
            $existente.removeClass('d-none');
            return;
        }

        $tbody.append('<tr class="' + cssClass + '"><td colspan="' + colspan + '" class="text-center text-muted py-4">' + text + '</td></tr>');
    }

    function syncIcon(button, expanded) {
        if (!button) return;
        button.setAttribute('aria-expanded', expanded ? 'true' : 'false');
        var icon = button.querySelector('i');
        if (icon) {
            icon.classList.toggle('fa-chevron-down', !expanded);
            icon.classList.toggle('fa-chevron-up', expanded);
        }
    }

    function ensureDetalleElaboradoLoaded(detalle) {
        var container = detalle ? detalle.querySelector('.js-elaborado-detalle-container') : null;
        if (!container || container.getAttribute('data-loaded') === 'true') return;

        var id = container.getAttribute('data-elaborado-id');
        var root = document.querySelector('[data-elaborados-page="index"]');
        var baseUrl = root ? root.getAttribute('data-detalle-url') : '';
        if (!id || !baseUrl) {
            container.textContent = 'No se pudieron cargar los detalles del elaborado.';
            container.setAttribute('data-loaded', 'error');
            return;
        }

        container.textContent = 'Cargando detalles...';

        $.ajax({
            url: baseUrl,
            type: 'GET',
            cache: false,
            data: { id: id }
        }).done(function (html) {
            container.innerHTML = html;
            container.setAttribute('data-loaded', 'true');
        }).fail(function () {
            container.textContent = 'No se pudieron cargar los detalles del elaborado.';
            container.setAttribute('data-loaded', 'error');
        });
    }

    function filtrarElaborados() {
        var $page = $('[data-elaborados-page="index"]');
        if (!$page.length) return;

        var filtro = normalizarTexto($('.js-filtro-vivo-elaborados').val());
        var visibles = 0;
        var $rows = $page.find('.js-elaborado-row');
        var $tbody = $('#tbodyElaborados');

        $tbody.find('.js-elaborado-empty-client').remove();
        $tbody.find('.js-elaborado-empty-server').toggleClass('d-none', $rows.length > 0);

        $rows.each(function () {
            var $row = $(this);
            var detailId = $row.data('detail-id');
            var $detailRow = detailId ? $tbody.find('.js-elaborado-detail-row[data-parent-detail-id="' + detailId + '"]') : $();
            var match = !filtro
                || normalizarTexto($row.data('elaborado')).indexOf(filtro) >= 0
                || normalizarTexto($row.data('codigo')).indexOf(filtro) >= 0
                || normalizarTexto($row.data('observaciones')).indexOf(filtro) >= 0
                || normalizarTexto($row.data('estado')).indexOf(filtro) >= 0;

            $row.toggleClass('d-none', !match);
            $detailRow.toggleClass('d-none', !match);

            if (!match && detailId) {
                var $collapse = $('#' + detailId);
                if ($collapse.length) {
                    $collapse.collapse('hide');
                    var btn = document.querySelector('.btn-detalles-elaborado[data-target="#' + detailId + '"]');
                    syncIcon(btn, false);
                }
            }

            if (match) visibles++;
        });

        if ($rows.length > 0 && visibles === 0) {
            renderEmptyRow($tbody, 'js-elaborado-empty-client', 8, 'No hay resultados con el filtro en vivo.');
        }
    }

    function filtrarLineas() {
        var $page = $('[data-elaborados-page="lineas"]');
        if (!$page.length) return;

        var filtro = normalizarTexto($('.js-filtro-vivo-lineas').val());
        var soloIngredientes = $('#soloIngredientes').is(':checked');
        var visibles = 0;
        var $rows = $page.find('.js-linea-row');
        var $tbody = $('#tbodyLineasElaborados');

        $tbody.find('.js-linea-empty-client').remove();
        $tbody.find('.js-linea-empty-server').toggleClass('d-none', $rows.length > 0);

        $rows.each(function () {
            var $row = $(this);
            var ingrediente = normalizarTexto($row.data('ingrediente'));
            var elaborado = normalizarTexto($row.data('elaborado'));
            var codIngrediente = normalizarTexto($row.data('cod-ingrediente'));
            var codElaborado = normalizarTexto($row.data('cod-elaborado'));
            var estado = normalizarTexto($row.data('estado'));

            var source = soloIngredientes
                ? [ingrediente, codIngrediente]
                : [ingrediente, codIngrediente, elaborado, codElaborado, estado];

            var match = !filtro || source.some(function (item) {
                return item.indexOf(filtro) >= 0;
            });

            $row.toggleClass('d-none', !match);
            if (match) visibles++;
        });

        if ($rows.length > 0 && visibles === 0) {
            renderEmptyRow($tbody, 'js-linea-empty-client', 8, 'No hay resultados con el filtro en vivo.');
        }
    }

    function bindDetallesElaborados() {
        var switchVista = document.getElementById('switchVistaCompletaElaborados');
        if (!switchVista) return;

        document.addEventListener('click', function (e) {
            var btn = e.target.closest('.btn-detalles-elaborado');
            if (!btn) return;

            e.preventDefault();
            var selector = btn.getAttribute('data-target');
            var detalle = selector ? document.querySelector(selector) : null;
            if (!detalle) return;

            var expanded = detalle.classList.contains('show');
            if (!expanded) {
                ensureDetalleElaboradoLoaded(detalle);
            }
            $(detalle).collapse(expanded ? 'hide' : 'show');
            syncIcon(btn, !expanded);
        });

        switchVista.addEventListener('change', function () {
            var expandir = switchVista.checked;
            document.querySelectorAll('.elaborado-detalle-collapse').forEach(function (detalle) {
                if ($(detalle).closest('.js-elaborado-detail-row').hasClass('d-none')) return;
                if (expandir) {
                    ensureDetalleElaboradoLoaded(detalle);
                }
                $(detalle).collapse(expandir ? 'show' : 'hide');
                var btn = document.querySelector('.btn-detalles-elaborado[data-target="#' + detalle.id + '"]');
                syncIcon(btn, expandir);
            });
        });
    }

    $(function () {
        bindFiltrosPendientes();
        bindDetallesElaborados();

        $('.js-filtro-vivo-elaborados').on('input', filtrarElaborados);
        $('.js-filtro-vivo-lineas').on('input', filtrarLineas);
        $('#soloIngredientes').on('change', filtrarLineas);

        filtrarElaborados();
        filtrarLineas();
    });
})();
