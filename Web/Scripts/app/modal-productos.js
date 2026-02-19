(function () {

    let debounceTimer = null;

    const $modal = $('#modalBuscarProducto');
    const $input = $('#filtroProducto');
    const $tbody = $('#tablaProductos');

    /* =========================
       CARGA DE PRODUCTOS
       ========================= */

    function cargarProductos(q) {
        const apiUrl = $modal.data('api-url');

        $.get(apiUrl, { q: q || '' })
            .done(function (items) {
                renderTabla(items || []);
            })
            .fail(function () {
                $tbody.html(
                    '<tr><td colspan="3" class="text-center text-danger">Error al cargar productos</td></tr>'
                );
            });
    }

    function renderTabla(items) {

        if (!items.length) {
            $tbody.html(
                '<tr><td colspan="3" class="text-center text-muted">No se encontraron productos</td></tr>'
            );
            return;
        }

        let html = '';

        items.forEach(p => {
            html += `
                <tr data-codigo="${p.codigo}">
                    <td>${p.codigo}</td>
                    <td>${p.nombre}</td>
                    <td class="text-right">$ ${parseFloat(p.precio || 0).toFixed(2)}</td>
                </tr>`;
        });

        $tbody.html(html);

        // marcar primero
        $tbody.find('tr:first').addClass('table-active');
    }

    /* =========================
       SELECCIÓN
       ========================= */

    function seleccionarProducto(codigo) {

        const $inputPOS = $('#inputCodigo');
        if (!$inputPOS.length) return;

        // 1) Setear código y cerrar modal YA
        $inputPOS.val(codigo);
        $modal.modal('hide');

        // 2) Cuando el modal terminó de cerrar, recién ahí hacés el resto
        $modal.one('hidden.bs.modal', function () {
            if (typeof window.terminarEscritura === 'function') {
                window.terminarEscritura(codigo, function () {
                    const $inputCantidad = $('#inputCantidad');
                    if ($inputCantidad.length) $inputCantidad.focus().select();
                });
            } else {
                $inputPOS.trigger('input');
                const $inputCantidad = $('#inputCantidad');
                if ($inputCantidad.length) $inputCantidad.focus().select();
            }
        });
    }


    /* =========================
       EVENTOS
       ========================= */

    // Buscar en vivo
    $input.on('input', function () {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => {
            cargarProductos($input.val().trim());
        }, 250);
    });

    // Enter → seleccionar activa (o primera)
    $input.on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();

            const $sel = $tbody.find('tr.table-active:first');
            const $row = $sel.length ? $sel : $tbody.find('tr:first');

            if ($row.length) {
                seleccionarProducto($row.attr('data-codigo'));
            }
        }
    });

    // Click → marcar
    $tbody.on('click', 'tr', function () {
        $tbody.find('tr').removeClass('table-active');
        $(this).addClass('table-active');
    });

    // Doble click → seleccionar el clickeado
    $tbody.on('dblclick', 'tr', function () {
        seleccionarProducto($(this).attr('data-codigo'));
    });

    // Al abrir el modal
    $modal.on('shown.bs.modal', function () {
        $input.val('');
        $tbody.empty();
        $input.focus();
        cargarProductos();
    });

})();
